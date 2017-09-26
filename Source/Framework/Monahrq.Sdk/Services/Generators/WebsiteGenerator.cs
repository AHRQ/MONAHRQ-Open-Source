using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Generators;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Monahrq.Infrastructure.Domain.Flutters;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Domain.Reports.Attributes;
using Monahrq.Sdk.Services.Import;
using NHibernate.Criterion;
using NHibernate.Transform;
using Monahrq.Infrastructure.Domain.BaseData;
using System.Reflection;
using NHibernate.Linq;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Sdk.Services.Generators
{
    [Export(typeof(WebsiteGenerator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WebsiteGenerator
    {
        #region Fields and Constants

        private const string CSS_FILE_NAME = "user-settings.css";
        private const string DEFAULT_CSS_FILE_NAME = "professional.css";
        private const string CONSUMER_CSS_FILE_NAME = "consumer.css";
        private const string ABOUTUS_FILE_NAME = "pgAboutUs.js";
        private string _outputDirectoryPath;
        // private IDictionary<string, State> _selectedStates;
        private string _imagesDirectoryPath;
        private string _dataDirectoryPath;
        private string _baseDataDirectoryPath;
        private PublishTask _publishTask;
        private const string MapQuestApiKey = "Fmjtd%7Cluu82168nq%2C7w%3Do5-942llu";

        #endregion

        #region Imports

        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        IDomainSessionFactoryProvider SessionProvider { get; set; }

        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        [Import]
        IConfigurationService ConfigService { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        ILogWriter Logger { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the generation status.
        /// </summary>
        /// <value>
        /// The generation status.
        /// </value>
        public WebsiteGenerationStatus GenerationStatus { get; private set; }

        List<Flutter> _generatedfluttersForWebsite = new List<Flutter>();

        #endregion

        #region Methods

        /// <summary>
        /// Publishes the specified website.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="publishTask">The publish task.</param>
        /// <exception cref="ArgumentNullException">website;A valid website is required to generate the website.</exception>
        public void Publish(Website website, PublishTask publishTask = PublishTask.Full)
        {
            try
            {
                if (website == null)
                {
                    throw new ArgumentNullException("website", @"A valid website is required to generate the website.");
                }

                _publishTask = publishTask;


                PublishEvent(WebsitePublishEventRegion.DefaultRegion, string.Format(PublishWebsiteResources.GenerationInProgress, website.Name),
                    PubishMessageTypeEnum.Information,
                    WebsiteGenerationStatus.InProgress, DateTime.Now);

                Logger.Information("Website Reported Year: {0}", website.ReportedYear);
                Logger.Information("Website Dataset Count: {0}", website.Datasets.Count);

                GenerationStatus = WebsiteGenerationStatus.InProgress;

                InitGenerator(website, publishTask);

                while (GenerationStatus != WebsiteGenerationStatus.Complete &&
                       GenerationStatus != WebsiteGenerationStatus.Error && GenerationStatus != WebsiteGenerationStatus.PreviewComplete)
                {
                    switch (publishTask)
                    {
                        case PublishTask.ThemeOnly:
                            // Generate theme files
                            GenerateThemeFiles(website);
                            break;
                        case PublishTask.ReportsOnly:
                            // Generate Report Json files
                            GenerateReportDataJsonFiles(website);
                            break;
                        case PublishTask.PreviewOnly:
                            CopySiteTemplateToOutputDirectory(website, false);

                            // Generate Website Properties files
                            GeneratePropertiesJsonFiles(website);

                            // Generate theme files
                            GenerateThemeFiles(website);

                            // Copy saved images if any.
                            GenerateOutputImages(website);

                            // Generate Report Json files
                            GenerateReportDataJsonFiles(website, PublishTask.PreviewOnly);

                            GenerationStatus = WebsiteGenerationStatus.PreviewComplete;
                            break;
                        case PublishTask.BaseCMSZoneOnly:

                            // Generate Report Json files
                            GenerateReportDataJsonFiles(website, PublishTask.BaseCMSZoneOnly);
                            GenerationStatus = WebsiteGenerationStatus.Complete;
                            break;
                        default:

                            //Copy Site Template to output directory
                            CopySiteTemplateToOutputDirectory(website);

                            // Generate Website Properties files
                            GeneratePropertiesJsonFiles(website);
                            // Generate theme files
                            GenerateThemeFiles(website);
                            // Copy saved images if any.
                            GenerateOutputImages(website);
                            // Generate Flutter/FlutterRegistry files
                            GenetateFlutterItems(website);
                            // Generate Report Json files
                            GenerateReportDataJsonFiles(website);

                            GenerationStatus = WebsiteGenerationStatus.Complete;
                            break;
                    }
                }

                PublishEvent(new WebsitePublishEventRegion("Website Completed"), string.Format(PublishWebsiteResources.GenerationCompleted, website.Name),
                    PubishMessageTypeEnum.Information, GenerationStatus, DateTime.Now);
            }
            catch (Exception ex)
            {
                GenerationStatus = WebsiteGenerationStatus.Error;

                var error = ex.InnerException ?? ex;
                PublishEvent(WebsitePublishEventRegion.DefaultRegion, string.Format(PublishWebsiteResources.GenerationError, website.Name, error.Message),
                    PubishMessageTypeEnum.Error,
                    WebsiteGenerationStatus.Error, DateTime.Now);
                // TODO: Add exception Handling and error processing
            }
        }

        /// <summary>
        /// Copies the site template to output directory.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="isPreview">if set to <c>true</c> [is preview].</param>
        /// <exception cref="DirectoryNotFoundException">
        /// </exception>
        /// <exception cref="FileNotFoundException">No website page templates files could be found.</exception>
        private void CopySiteTemplateToOutputDirectory(Website website, bool isPreview = false)
        {
            PublishEvent(WebsitePublishEventRegion.DefaultRegion, string.Format("Starting the copying website template to output directory: ({0})", _outputDirectoryPath),
                        PubishMessageTypeEnum.Information,
                        WebsiteGenerationStatus.InProgress, DateTime.Now);

            var baseDirect = AppDomain.CurrentDomain.BaseDirectory;

            var websiteDirectoryPath = Path.Combine(baseDirect, "resources\\templates\\site\\");

            if (!Directory.Exists(websiteDirectoryPath))
                throw new DirectoryNotFoundException(websiteDirectoryPath);

            IOHelper.DirectoryCopy(websiteDirectoryPath, _outputDirectoryPath, true, Logger);
            //IOHelper.DirectoryCopy2(websiteDirectoryPath, _outputDirectoryPath);

            // Page Files
            var pageDirectoryPath = Path.Combine(baseDirect, "resources\\templates\\data\\pages\\");

            if (!Directory.Exists(pageDirectoryPath))
                throw new DirectoryNotFoundException(pageDirectoryPath);

            var pagefiles = new DirectoryInfo(pageDirectoryPath).GetFiles("*", SearchOption.AllDirectories).ToList();

            if (!pagefiles.Any())
                throw new FileNotFoundException("No website page templates files could be found.");

            Parallel.ForEach(pagefiles, page =>
            {
                var newpageFileName = Path.Combine(_baseDataDirectoryPath, page.Name);
                page.CopyTo(newpageFileName, true);
            });

            //foreach (var pagefile in pagefiles)
            //{
            //    var newpageFileName = Path.Combine(_baseDataDirectoryPath, pagefile.Name);
            //    pagefile.CopyTo(newpageFileName, true);
            //}

            if (!isPreview)
            {
                // County Geolocation
                GenerateCountyGeoLocationFiles(baseDirect, website);

                // Region Geolocation
                GenerateRegionGeoLocationFiles(baseDirect, website);
            }

            PublishEvent(WebsitePublishEventRegion.DefaultRegion, string.Format("Finishing the copying website template to output directory: ({0})", _outputDirectoryPath),
                        PubishMessageTypeEnum.Information,
                        WebsiteGenerationStatus.InProgress, DateTime.Now);
        }

        /// <summary>
        /// Generates the county geo location files for the website selected reporting states.
        /// </summary>
        /// <param name="baseDirect">The base direct.</param>
        /// <param name="website">The website.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="FileNotFoundException">No county geolocation files could be found.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="System.IO.FileNotFoundException">No county geolocation files could be found.</exception>
        private void GenerateCountyGeoLocationFiles(string baseDirect, Website website)
        {
            var countyGeolocationDirectoryPath = Path.Combine(baseDirect, "resources\\templates\\data\\countygeoboundaries\\");

            if (!Directory.Exists(countyGeolocationDirectoryPath))
                throw new DirectoryNotFoundException(countyGeolocationDirectoryPath);

            var files = new DirectoryInfo(countyGeolocationDirectoryPath).GetFiles("*", SearchOption.AllDirectories).ToList();

            if (!files.Any())
                throw new FileNotFoundException("No county geolocation files could be found.");

            foreach (var state in website.SelectedReportingStates)
            {
                var file = files.FirstOrDefault(fi => fi.Name.ToLower().EqualsIgnoreCase(string.Format("countygeo_{0}.js", state)));

                var newFileName = Path.Combine(_baseDataDirectoryPath, file.Name);
                file.CopyTo(newFileName, true);
            }
        }

        /// <summary>
        /// Generates the region geo location files for the website selected reporting states.
        /// </summary>
        /// <param name="baseDirect">The base direct.</param>
        /// <param name="website">The website.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        private void GenerateRegionGeoLocationFiles(string baseDirect, Website website)
        {
            foreach (var regionType in new string[] { "HRR", "HSA" })
            {
                var regionGeolocationDirectoryPath = Path.Combine(baseDirect, "resources\\templates\\data\\regiongeoboundaries\\", regionType.ToLower());

                if (!Directory.Exists(regionGeolocationDirectoryPath))
                    throw new DirectoryNotFoundException(regionGeolocationDirectoryPath);

                var regionfiles = new DirectoryInfo(regionGeolocationDirectoryPath).GetFiles("*", SearchOption.AllDirectories).ToList();

                if (!regionfiles.Any())
                    throw new FileNotFoundException(string.Format("No {0} region geolocation files could be found.", regionType));

                foreach (var state in website.SelectedReportingStates)
                {
                    var file = regionfiles.FirstOrDefault(fi => fi.Name.ToLower().EqualsIgnoreCase(string.Format("{0}geo_{1}.js", regionType, state.ToLower())));

                    var newFileName = Path.Combine(_baseDataDirectoryPath, file.Name);
                    file.CopyTo(newFileName, true);
                }
            }
        }

        /// <summary>
        /// Generates the output images.
        /// </summary>
        /// <param name="website">The website.</param>
        private void GenerateOutputImages(Website website)
        {
            if (!Directory.Exists(_imagesDirectoryPath))
            {
                Directory.CreateDirectory(_imagesDirectoryPath);
            }

            CopyImage(website.LogoImage, "Logo");
            CopyImage(website.BannerImage, "banner");
            CopyImage(website.HomepageContentImage, "Homepage content");

        }

        /// <summary>
        /// Copies the website image to output directory.
        /// </summary>
        /// <param name="websiteImage">The website image.</param>
        /// <param name="imageType">Type of the image.</param>
        private void CopyImage(WebsiteImage websiteImage, string imageType)
        {

            if (websiteImage == null || string.IsNullOrEmpty(websiteImage.ImagePath) || websiteImage.Image == null) return;


            var fileName = websiteImage.ImagePath.SubStrAfterLast(@"\");
            var fileFullName = Path.Combine(_imagesDirectoryPath, fileName);
            File.WriteAllBytes(fileFullName, websiteImage.Image);

            PublishEvent(WebsitePublishEventRegion.DefaultRegion, string.Format(PublishWebsiteResources.ImageGenerated, imageType, fileFullName), PubishMessageTypeEnum.Information, WebsiteGenerationStatus.InProgress, DateTime.Now);

        }

        /// <summary>
        /// Setups the output directory.
        /// </summary>
        /// <param name="website">The website.</param>
        private void SetupOutputDirectory(Website website, bool overwriteDir = true)
        {

            var outputDirectory = new DirectoryInfo(_outputDirectoryPath);

            PublishEvent(
				WebsitePublishEventRegion.DefaultRegion,
				string.Format(PublishWebsiteResources.GenerationOutputDirectoryExists, outputDirectory.FullName),
				PubishMessageTypeEnum.Information,
                WebsiteGenerationStatus.InProgress, DateTime.Now);

            IOHelper.CreateDirectory(outputDirectory.FullName, overwriteDir);

            PublishEvent(
				WebsitePublishEventRegion.DefaultRegion,
				string.Format(PublishWebsiteResources.GenerationOutputDirectoryDeleted, outputDirectory.FullName),
				PubishMessageTypeEnum.Information,
                WebsiteGenerationStatus.InProgress, DateTime.Now);

            GenerationStatus = WebsiteGenerationStatus.OutputDirectoryCreateUpdateComplete;

            PublishEvent(
				WebsitePublishEventRegion.DefaultRegion,
				string.Format(PublishWebsiteResources.GenerationOutputDirectoryCreated, outputDirectory.FullName),
				PubishMessageTypeEnum.Information,
				WebsiteGenerationStatus.OutputDirectoryCreateUpdateComplete, DateTime.Now);
        }

        /// <summary>
        /// Named ExportAttribute instances are not retrieved by default "GetAllInstances<*>()" method.
        /// </summary>
        /// <returns></returns>
        private List<IReportGenerator> GetAllIReportGeneratorInstances()
        {
            var generators = new List<IReportGenerator>();
            generators = ServiceLocator.Current.GetAllInstances<IReportGenerator>().ToList();
            generators.Add(ServiceLocator.Current.GetInstance<IReportGenerator>("BaseNursingHomeReportGenerator"));
            generators.Add(ServiceLocator.Current.GetInstance<IReportGenerator>("QualityReportGenerator"));
            return generators;
        }

        #region GenerateReportDataJsonFiles Methods.

        /// <summary>
        /// Generates the report data json files remove dynamic report generators.
        /// </summary>
        /// <param name="generators">The generators.</param>
        /// <param name="dynamicWingTargetGenerator">The dynamic wing target generator.</param>
        /// <param name="publishTask">The publish task.</param>
        private void GenerateReportDataJsonFiles_RemoveDynamicReportGenerators(ref List<IReportGenerator> generators, IReportGenerator dynamicWingTargetGenerator, PublishTask publishTask)
        {
            // **********************************************************************
            //	Report Generator: DynamicReportGenerator.
            //	- Remove any DynamicReportGenerator if present.
            if (dynamicWingTargetGenerator != null)
            {
                if (generators.Any(gen => string.Equals(gen.GetType().Name, dynamicWingTargetGenerator.GetType().Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    generators.RemoveAll(gen => string.Equals(gen.GetType().Name, dynamicWingTargetGenerator.GetType().Name, StringComparison.CurrentCultureIgnoreCase));
                }
            }
        }

        /// <summary>
        /// Generates the report data json files add base data report generator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generators">The generators.</param>
        /// <param name="website">The website.</param>
        /// <param name="publishTask">The publish task.</param>
        /// <param name="reportGenSets">The report gen sets.</param>
        /// <param name="reportName">Name of the report.</param>
        /// <returns></returns>
        private bool GenerateReportDataJsonFiles_AddBaseDataReportGenerator<T>(
            ref List<IReportGenerator> generators,
            Website website,
            PublishTask publishTask,
            ref SortedDictionary<int, List<Action>> reportGenSets,
            String reportName)
            where T : BaseReportGenerator
        {
            // **********************************************************************
            //	Report Generator: BaseDataReportGenerator.
            //	- Cancel All Json Report generation if PublishTask == PreviewOnly (&& BaseDataReportGenerator is present - why this added condition.)
            //	- Generate Report.
            var baseReportGenerator = generators.OfType<T>().SingleOrDefault();
            if (baseReportGenerator != null)
            {
                if (typeof(T) == typeof(BaseDataReportGenerator))
                    GenerateReportsTemplatesJsonFiles(website);

                if (publishTask == PublishTask.PreviewOnly)
                    return false;
                if (publishTask == PublishTask.BaseCMSZoneOnly && typeof(T) != typeof(CMSZoneReportGenerator))
                    return true;

                int executionOrder = baseReportGenerator.ExecutionOrder;
                reportGenSets.SetIfNotExist(executionOrder, () => new List<Action>());
                reportGenSets[executionOrder].Add(
                    new Action(() =>
                    {

                        PublishEvent(
							new WebsitePublishEventRegion(baseReportGenerator),
							string.Format("Generating {0} Report.", reportName),
                            PubishMessageTypeEnum.Information,
                            WebsiteGenerationStatus.ReportsGenerationInProgress,
                            DateTime.Now);

                        baseReportGenerator.GenerateReport(website, publishTask);

                        PublishEvent(
							new WebsitePublishEventRegion(baseReportGenerator),
							string.Format("DONE - Generating {0} Report.", reportName),
                            PubishMessageTypeEnum.Information,
                            WebsiteGenerationStatus.ReportsGenerationInProgress,
                            DateTime.Now);
                    }));
            }
            return true;
        }

        /// <summary>
        /// Generates the report data json files add hospital profile report generator.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="generators">The generators.</param>
        /// <param name="reportGenSets">The report gen sets.</param>
        /// <param name="publishTask">The publish task.</param>
        private void GenerateReportDataJsonFiles_AddHospitalProfileReportGenerator(Website website, ref List<IReportGenerator> generators, ref SortedDictionary<int, List<Action>> reportGenSets, PublishTask publishTask)
        {
            // **********************************************************************
            //	Report Generator: HospitalProfileReportGenerator.
            //	- Remove any HospitalProfileReportGenerator if present.
            //	- Generate Report.

            if (publishTask == PublishTask.BaseCMSZoneOnly) return;

            if (website.Hospitals.Any())
            {
                var hospitalProfileReportGenerator = ServiceLocator.Current.GetInstance<IReportGenerator>("HospitalProfileReportGenerator");
                if (hospitalProfileReportGenerator != null)
                {
                    if (generators.Any(
                            gen =>
                                String.Equals(
                                    gen.GetType().Name,
                                    hospitalProfileReportGenerator.GetType().Name,
                                    StringComparison.CurrentCultureIgnoreCase)))
                    {
                        generators.RemoveAll(
                            gen =>
                                String.Equals(
                                    gen.GetType().Name,
                                    hospitalProfileReportGenerator.GetType().Name,
                                    StringComparison.CurrentCultureIgnoreCase));
                    }

                    int executionOrder = hospitalProfileReportGenerator.ExecutionOrder;
                    reportGenSets.SetIfNotExist(executionOrder, () => new List<Action>());
                    reportGenSets[executionOrder].Add(
                        new Action(() =>
                        {
                            PublishEvent(
								new WebsitePublishEventRegion(hospitalProfileReportGenerator),
								string.Format("Generating HospitalProfile Report."),
                                PubishMessageTypeEnum.Information,
                                WebsiteGenerationStatus.ReportsGenerationInProgress,
                                DateTime.Now);

                            hospitalProfileReportGenerator.ActiveReport = website.Reports
                                                                                 .Where(r => string.Equals(r.Report.ReportType, "Hospital Profile Report", StringComparison.CurrentCultureIgnoreCase))
                                                                                 .Select(r => r.Report)
                                                                                 .FirstOrDefault();

                            hospitalProfileReportGenerator.GenerateReport(website, publishTask);

                            PublishEvent(
								new WebsitePublishEventRegion(hospitalProfileReportGenerator),
								string.Format("DONE - Generating HospitalProfile Report."),
                                PubishMessageTypeEnum.Information,
                                WebsiteGenerationStatus.ReportsGenerationInProgress,
                                DateTime.Now);

                        }));
                }
            }
        }

        /// <summary>
        /// Generates the report data json files retrieve analyzed included generators.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="generators">The generators.</param>
        /// <param name="publishTask">The publish task.</param>
        /// <returns></returns>
        private List<dynamic> GenerateReportDataJsonFiles_RetrieveAnalyzedIncludedGenerators(Website website, ref List<IReportGenerator> generators, PublishTask publishTask)
        {
            // **********************************************************************
            //	Retrieve all non-custom (Analyzed) reports.
            IList<dynamic> analyzedGenerators = new List<dynamic>();

            if (publishTask == PublishTask.BaseCMSZoneOnly) return analyzedGenerators.ToList();


            foreach (var wr in website.Reports.Where(wr => !wr.Report.IsCustom).ToList())
            {
                generators.Where(g => g.ReportIds.Any(id => id.EqualsIgnoreCase(wr.Report.SourceTemplate.RptId)))
                    .DistinctBy(x => x.GetType().FullName).ForEach(gen =>
                {
                    if (gen != null)
                    {
                        gen.CurrentWebsite = null;
                        gen.CurrentWebsite = website;

                        var item = new
                        {
                            Generator = gen,
                            WebsiteReport = wr,
                            IsIncluded = true,
                            FullName = gen.GetType().FullName,
                            Name = gen.GetType().Name,
                            ReportGeneratorAtt = gen.GetType().GetCustomAttribute<ReportGeneratorAttribute>() ?? new ReportGeneratorAttribute()
                        };

                        analyzedGenerators.Add(item);
                    }
                });
            }

            //	Retrieve included analyzed reports.
            var includedItems = analyzedGenerators.Where(o => o.IsIncluded == true).DistinctBy(o => o.FullName).ToList();

            //	Remove any BaseNursingHomereports from the 'Included' list.
            if (includedItems.Count(o => o.FullName.Equals("Monahrq.Wing.NursingHomeCompare.Reports.BaseNursingHomeReportGenerator") && o.IsIncluded) > 1)
            {
                var nhGenList = includedItems.Where(o => o.FullName.Equals("Monahrq.Wing.NursingHomeCompare.Reports.BaseNursingHomeReportGenerator") && o.IsIncluded).ToList();

                if (nhGenList.Any())
                {
                    for (var i = 0; i <= nhGenList.Count - 1; i++)
                    {
                        if (i > 0)
                        {
                            includedItems.Remove(nhGenList[i]);
                        }
                    }
                }
            }

            return includedItems;
        }

        /// <summary>
        /// Generates the report data json files add preview mode generators.
        /// </summary>
        /// <param name="includedItems">The included items.</param>
        /// <param name="publishTask">The publish task.</param>
        private void GenerateReportDataJsonFiles_AddPreviewModeGenerators(ref List<dynamic> includedItems, PublishTask publishTask)
        {
            if (publishTask == PublishTask.BaseCMSZoneOnly) return;

            // Add QualityReportGenerator
            if (!includedItems.Any(o => o.FullName.Equals("Monahrq.Wing.Ahrq.QualityReportGenerator") && o.IsIncluded))
            {
                var itemToAdd = includedItems.FirstOrDefault(o => o.FullName.Equals("Monahrq.Wing.Ahrq.QualityReportGenerator"));
                includedItems.Add(new { Generator = itemToAdd.Generator, WebsiteReport = itemToAdd.WebsiteReport, IsIncluded = true, FullName = "Monahrq.Wing.Ahrq.QualityReportGenerator", Name = "QualityReportGenerator", ReportGeneratorAtt = itemToAdd.ReportGeneratorAtt });
            }

            // Add AvoidableStaysReportGenerator
            if (!includedItems.Any(o => o.FullName.Equals("Monahrq.Wing.Ahrq.AvoidableStaysReportGenerator") && o.IsIncluded))
            {
                var itemToAdd = includedItems.FirstOrDefault(o => o.FullName.Equals("Monahrq.Wing.Ahrq.AvoidableStaysReportGenerator"));
                includedItems.Add(new { Generator = itemToAdd.Generator, WebsiteReport = itemToAdd.WebsiteReport, IsIncluded = true, FullName = "Monahrq.Wing.Ahrq.AvoidableStaysReportGenerator", Name = "AvoidableStaysReportGenerator", ReportGeneratorAtt = itemToAdd.ReportGeneratorAtt });
            }
        }

        /// <summary>
        /// Generates the report data json files add analyzed included report generators.
        /// </summary>
        /// <param name="includedItems">The included items.</param>
        /// <param name="reportGenSets">The report gen sets.</param>
        /// <param name="website">The website.</param>
        /// <param name="publishTask">The publish task.</param>
        private void GenerateReportDataJsonFiles_AddAnalyzedIncludedReportGenerators(ref List<dynamic> includedItems, ref SortedDictionary<int, List<Action>> reportGenSets, Website website, PublishTask publishTask)
        {
            if (publishTask == PublishTask.BaseCMSZoneOnly) return;

            if (includedItems.Any())
            {
                //includedItems = includedItems.OrderBy(x => x.ReportGeneratorAtt.ExecutionOrder).ToList();
                foreach (var item in includedItems)
                {
                    int executionOrder = item.Generator.ExecutionOrder;
                    reportGenSets.SetIfNotExist(executionOrder, () => new List<Action>());
                    reportGenSets[executionOrder].Add(new Action(() =>
                        {                          
                            if (!item.Generator.CheckIfCanRun()) return;

                            var  reportGenerationMessage  = string.Format("Generating data for report: {0}", !string.IsNullOrEmpty(item.ReportGeneratorAtt.MessageOverride) 
                                                                                ? item.ReportGeneratorAtt.MessageOverride 
                                                                                : item.WebsiteReport.Report.Name);

                            PublishEvent(
								new WebsitePublishEventRegion(item.Generator),
								reportGenerationMessage,
                                PubishMessageTypeEnum.Information,
                                WebsiteGenerationStatus.ReportsGenerationInProgress,
                                DateTime.Now);

                            try
                            {
                                // Logger.Write(reportGenerationMessage, Category.Info, Priority.Medium);
                                item.Generator.ActiveReport = item.WebsiteReport.Report;
                                item.Generator.GenerateReport(website, publishTask);
                            }
                            catch (Exception exc)
                            {
                                var excToUse = exc.GetBaseException();
                                var eecMessage = string.Format("{0} : {1}", item.Name, excToUse.Message);
                                //Logger.Write(eecMessage, Category.Exception, Priority.High);

                                PublishEvent(
									new WebsitePublishEventRegion(item.Generator),
									eecMessage,
									PubishMessageTypeEnum.Error,
									WebsiteGenerationStatus.Error,
									DateTime.Now);
                            }


                            PublishEvent(
								new WebsitePublishEventRegion(item.Generator),
                                string.Format("DONE - Generating data for report: {0}.", item.WebsiteReport.Report.Name),
                                PubishMessageTypeEnum.Information,
                                WebsiteGenerationStatus.ReportsGenerationInProgress,
                                DateTime.Now);
                        }));
                }
            }
        }

        /// <summary>
        /// Generates the report data json files add dynamic open source report generators.
        /// </summary>
        /// <param name="dynamicWingTargetGenerator">The dynamic wing target generator.</param>
        /// <param name="website">The website.</param>
        /// <param name="reportGenSets">The report gen sets.</param>
        /// <param name="publishTask">The publish task.</param>
        private void GenerateReportDataJsonFiles_AddDynamicOpenSourceReportGenerators(IReportGenerator dynamicWingTargetGenerator, Website website, ref SortedDictionary<int, List<Action>> reportGenSets, PublishTask publishTask)
        {
            if (publishTask == PublishTask.BaseCMSZoneOnly) return;


            // **********************************************************************
            // ****** Dynamic Open Source Reports ******
            //	- Get custom/included generators.
            //	- ForEach:
            //		- PublishEvent for starting 'current' report.
            //		- Set Active Report.
            //		- Generate Report.
            if (dynamicWingTargetGenerator != null)
            {
                IList<dynamic> analyzedOSGenerators = new List<dynamic>();
                foreach (var wr in website.Reports.Where(wr => wr.Report.IsCustom).ToList())
                {
                    var gen = dynamicWingTargetGenerator;

                    var item = new
                    {
                        Generator = gen,
                        WebsiteReport = wr,
                        IsIncluded = true,
                        FullName = gen.GetType().FullName,
                        Name = gen.GetType().Name
                    };

                    analyzedOSGenerators.Add(item);
                }

                // var includedOSItems = analyzedOSGenerators.Where(o => o.IsIncluded == true).ToList();

                //	var lockObj = new Object();
                foreach (var item in analyzedOSGenerators.Where(o => o.IsIncluded == true).ToList())
                {
                    int executionOrder = item.Generator.ExecutionOrder;
                    reportGenSets.SetIfNotExist(executionOrder, () => new List<Action>());
                    reportGenSets[executionOrder].Add(
                        new Action(() =>
                        {
                            //	lock (lockObj)
                            {
                                PublishEvent(
									new WebsitePublishEventRegion(item.Name),
									string.Format("Generating data for report: {0}", item.WebsiteReport.Report.Name),
                                    PubishMessageTypeEnum.Information,
                                    WebsiteGenerationStatus.ReportsGenerationInProgress,
                                    DateTime.Now);

                                try
                                {
                                    item.Generator.ActiveReport = item.WebsiteReport.Report;
                                    item.Generator.GenerateReport(website, publishTask);
                                }
                                catch (Exception exc)
                                {
                                    var excToUse = exc.GetBaseException();
                                    var eecMessage = string.Format("{0} : {1}", item.Name, excToUse.Message);
                                    //Logger.Write(eecMessage, Category.Exception, Priority.High);
                                    PublishEvent(
										new WebsitePublishEventRegion(item.Name),
										eecMessage,
										PubishMessageTypeEnum.Error,
										WebsiteGenerationStatus.InProgress,
										DateTime.Now);
                                }

                                PublishEvent(
									new WebsitePublishEventRegion(item.Name),
									string.Format("DONE - Generating data for report: {0}.", item.WebsiteReport.Report.Name),
                                    PubishMessageTypeEnum.Information,
                                    WebsiteGenerationStatus.ReportsGenerationInProgress,
                                    DateTime.Now);
                            }
                        }));
                }
            }
        }

        /// <summary>
        /// Generates the report data JSON files.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="publishTask">The publish task.</param>
        private void GenerateReportDataJsonFiles(Website website, PublishTask publishTask = PublishTask.Full)
        {
            //	Variables.
            //	- Var reportGeneratorSets holds sets of Action that each perform the generation of a Report.
            //	- These Actions are grouped in Sets according to their 'Execution Order'.
            //	- Each 'Set' is run in order; where each Action in the set can be run in Parallel with any other Action in the same Set.
            //	- Sets are executed completely before the next Set is begun.
            var reportGeneratorSets = new SortedDictionary<int, List<Action>>();
            GenerationStatus = WebsiteGenerationStatus.ReportsGenerationInProgress;

            //	Push event announcing the start of Json Report generation.
            PublishEvent(
				WebsitePublishEventRegion.DefaultRegion,
				string.Format(PublishWebsiteResources.ReportsGenerationStart),
                PubishMessageTypeEnum.Information,
                WebsiteGenerationStatus.ReportsGenerationInProgress,
                DateTime.Now);

            //	Get all report generators.
            var generators = GetAllIReportGeneratorInstances();

            if (generators.Count > 0)
            {
                var dynamicWingTargetGenerator = ServiceLocator.Current.GetInstance<IReportGenerator>("DynamicReportGenerator");

                // **********************************************************************
                //	Report Generator: DynamicReportGenerator.
                //	- Remove any DynamicReportGenerator if present.
                GenerateReportDataJsonFiles_RemoveDynamicReportGenerators(ref generators, dynamicWingTargetGenerator, publishTask);

                // **********************************************************************
                //	Report Generator: BaseDataReportGenerator.
                //	- Cancel All Json Report generation if PublishTask == PreviewOnly (&& BaseDataReportGenerator is present - why this added condition.)
                if (!GenerateReportDataJsonFiles_AddBaseDataReportGenerator<BaseDataReportGenerator>(
                        ref generators, website, publishTask, ref reportGeneratorSets, "Base"))
                    return;

                // **********************************************************************
                //	Report Generator: CMSZoneReportGenerator.
                //	- Cancel All Json Report generation if PublishTask == PreviewOnly (&& BaseDataReportGenerator is present - why this added condition.)
                if (!GenerateReportDataJsonFiles_AddBaseDataReportGenerator<CMSZoneReportGenerator>(
                        ref generators, website, publishTask, ref reportGeneratorSets, "CMS Custom Zones"))
                    return;

                // **********************************************************************
                //	Report Generator: HospitalProfileReportGenerator.
                //	- Remove any HospitalProfileReportGenerator if present.
                //	- Generate Report.
                GenerateReportDataJsonFiles_AddHospitalProfileReportGenerator(website, ref generators, ref reportGeneratorSets, publishTask);

                // **********************************************************************
                //	Retrieve all non-custom (Analyzed) reports.
                var includedItems = GenerateReportDataJsonFiles_RetrieveAnalyzedIncludedGenerators(website, ref generators, publishTask);

                // **********************************************************************
                //	In PreviowOnly mode, Add Quality and AvoidableStays reports to Analyzed-And-Included-Generators.
                if (publishTask == PublishTask.PreviewOnly)
                    GenerateReportDataJsonFiles_AddPreviewModeGenerators(ref includedItems, publishTask);

                // **********************************************************************
                //	Report Generator: Analyzed-And-Included-Generators.
                //	- PublishEvent of the current Analyzed-And-Included-Gen Report
                //	- Generate Report.
                GenerateReportDataJsonFiles_AddAnalyzedIncludedReportGenerators(ref includedItems, ref reportGeneratorSets, website, publishTask);

                // **********************************************************************
                // ****** Dynamic Open Source Reports ******
                //	- Get custom/included generators.
                //	- ForEach:
                //		- PublishEvent for starting 'current' report.
                //		- Set Active Report.
                //		- Generate Report.
                GenerateReportDataJsonFiles_AddDynamicOpenSourceReportGenerators(dynamicWingTargetGenerator, website, ref reportGeneratorSets, publishTask);

            }

            var po = new ParallelOptions
            {
                MaxDegreeOfParallelism = 4, //System.Environment.ProcessorCount,
                                            //CancellationToken = cancelToken
            };

            //	Run Report Actions - Execute each Set in order, complete all Actions in a Set before starting next Set.
            var asyncReportGenActionLoop = Task.Factory.StartNew(() =>
                {

                    foreach (var reportGeneratorSet in reportGeneratorSets)
                        foreach (var generator in reportGeneratorSet.Value)
                            generator();
               });

            // Wait for reports to be generated, but still allow UI thread to process other events.
            asyncReportGenActionLoop.WaitWithPumping();

            GenerationStatus = WebsiteGenerationStatus.ReportsGenerationComplete;

            PublishEvent(
				new WebsitePublishEventRegion("Website Completed"),
				PublishWebsiteResources.ReportsGenerationCompleted,
				PubishMessageTypeEnum.Information,
				WebsiteGenerationStatus.ReportsGenerationComplete,
				DateTime.Now);

            try
            {
                var tempPath = Path.GetTempPath() + "Monahrq\\Generators\\";
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
            catch (Exception exc)
            {
                Logger.Write(exc, "Error generating report data file");
            }
        }

        #endregion

        /// <summary>
        /// Genetates the flutter items.
        /// </summary>
        /// <param name="website">The website.</param>
        private void GenetateFlutterItems(Website website)
        {
            _generatedfluttersForWebsite.Clear();

            // Flutter output
            var wingReports = website.Reports.Where(wr => wr.Report.IsCustom).Select(wr => wr.Report).ToList();

            foreach (var rpt in wingReports)
                GenerateFlutterFiles(rpt, website);

            // Publish flutter Registry
            GenerateFlutteryRegistry(wingReports);
        }

        /// <summary>
        /// Generates the fluttery registry.
        /// </summary>
        /// <param name="wingReports">The wing reports.</param>
        private void GenerateFlutteryRegistry(IEnumerable<Report> wingReports)
        {
            var fluttersForRegistry = new List<Flutter>();

            if (_generatedfluttersForWebsite != null && _generatedfluttersForWebsite.Any())
            {
                fluttersForRegistry = new List<Flutter>(_generatedfluttersForWebsite);
            }
            else
            {
                // First grab all appropriate flutters via open source wing reports selected by website.
                using (var session = this.SessionProvider.SessionFactory.OpenSession())
                {
                    foreach (var report in wingReports.Where(wr => wr.IsCustom).ToList())
                    {
                        var flutters = session.CreateCriteria<Flutter>()
                                            .Add(Restrictions.InsensitiveLike("AssociatedReportsTypes", string.Format("%{0}%", report.ReportType)))
                                            .AddOrder(new Order("Name", true))
                                            .SetResultTransformer(new DistinctRootEntityResultTransformer())
                                            .Future<Flutter>()
                                            .ToList();

                        if (flutters.Any())
                            fluttersForRegistry.AddRange(flutters);
                    }
                }
            }

            if (fluttersForRegistry.Any())
            {
                PublishEvent(
					WebsitePublishEventRegion.DefaultRegion,
					"Creating Flutter Registry",
					PubishMessageTypeEnum.Information,
                    WebsiteGenerationStatus.ReportsGenerationInProgress,
					DateTime.Now);
            }

            // Create Registry...
            var flutterRegistry = new FlutterRegistryDto();
            foreach (var flutter in fluttersForRegistry.DistinctBy(f => f.Name).ToList())
            {
                var entry = new FlutterDto
                {
                    ModulePath = flutter.OutputPath.Replace("\\", "/"),
                    ConfigFilename = "flutter-config.js",
                    ConfigId = flutter.ConfigurationId
                };

                if (!flutterRegistry.Flutters.Any(f => f.ConfigId.EqualsIgnoreCase(entry.ConfigId)))
                    flutterRegistry.Flutters.Add(entry);
            }

            // write out FlutterRegistry.js file
            const string namspace = "$.monahrq.Flutter.Base.FlutterRegistry =";
            const string fileName = "FlutterRegistry.js";

            var outputPath = Path.Combine(_baseDataDirectoryPath, fileName);

            if (File.Exists(outputPath))
                File.Delete(outputPath);

            JsonHelper.GenerateJsonFile(flutterRegistry, outputPath, namspace);
        }

        /// <summary>
        /// Generates the flutter files.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="website">The website.</param>
        private void GenerateFlutterFiles(Report report, Website website)
        {
            var fluttersForRegistry = new List<Flutter>();

            // First grab all appropriate flutters via open source wing reports selected by website.
            using (var session = this.SessionProvider.SessionFactory.OpenSession())
            {
                var flutters = session.CreateCriteria<Flutter>()
                                    .Add(Restrictions.InsensitiveLike("AssociatedReportsTypes", string.Format("%{0}%", report.ReportType)))
                                    .AddOrder(new Order("Name", true))
                                    .SetResultTransformer(new DistinctRootEntityResultTransformer())
                                    .Future<Flutter>()
                                    .ToList();

                if (flutters.Any())
                    fluttersForRegistry.AddRange(flutters);
            }

            var websiteFlutterDirectory = Path.Combine(website.OutPutDirectory, "Flutters");

            if (!Directory.Exists(websiteFlutterDirectory))
                Directory.CreateDirectory(websiteFlutterDirectory);

            foreach (var flutter in fluttersForRegistry.DistinctBy(f => f.Name).ToList())
            {
                if (_generatedfluttersForWebsite.Any(f => f.Name.EqualsIgnoreCase(flutter.Name))) continue;

                PublishEvent(
					WebsitePublishEventRegion.DefaultRegion,
					string.Format("Copying flutter \"{0}\" to directory \"{1}\".", flutter.Name, flutter.OutputPath),
					PubishMessageTypeEnum.Information,
                    WebsiteGenerationStatus.ReportsGenerationInProgress,
					DateTime.Now);

                var flutterDirectory = Path.Combine(website.OutPutDirectory, flutter.OutputPath.Replace("/", "\\"));

                if (!Directory.Exists(flutterDirectory))
                    Directory.CreateDirectory(flutterDirectory);

                var root = MonahrqContext.MyDocumentsApplicationDirPath;
                var sourcePath = string.Format("{0}{1}", root, flutter.InstallPath);

                IOHelper.DirectoryCopy(sourcePath, flutterDirectory, true, Logger);

                _generatedfluttersForWebsite.Add(flutter);
            }

        }

        /// <summary>
        /// Generates the reports templates json files.
        /// </summary>
        /// <param name="website">The website.</param>
        protected void GenerateReportsTemplatesJsonFiles(Website website)
        {
            GenerateReportConfig(website, website.Reports.ToList(), "ReportConfig.js", "$.monahrq.ReportConfig=");
            GenerateReportConfig(website, website.Reports.Where(x => x.Report.Audiences.Contains(Audience.Consumers)).ToList(), "ConsumerReportConfig.js", "$.monahrq.ConsumerReportConfig=");
        }

        /// <summary>
        /// Generates the report configuration.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="reports">The reports.</param>
        /// <param name="reportFileName">Name of the report file.</param>
        /// <param name="module">The module.</param>
        private void GenerateReportConfig(Website website, IEnumerable<WebsiteReport> reports, string reportFileName, string module)
        {
            var fileName = Path.Combine(_baseDataDirectoryPath, reportFileName);
            var result = new List<object>();
            foreach (var report in reports)
            {
                if (report.Report.IsCustom) continue;

                //todo : refactor to mapping table
                string[] display = null;
                var measuresList = "";
                switch (report.Report.SourceTemplate.RptId.ToLower())
                {
                    case "5aaf7fba-7102-4c66-8598-a70597e2f826":
                        measuresList = "IP-08, IP-09, IP-10, IP-11,";
                        break;
                    case "2aaf7fba-7102-4c66-8598-a70597e2f827":
                        measuresList = "ED-01, ED-02, ED-03, ED-04,";
                        break;
                    case "2aaf7fba-7102-4c66-8598-a70597e2f824":
                        measuresList = "IP-01, IP-02, IP-03, IP-04, IP-05, IP-06, IP-07,";
                        break;
                    case "2aaf7fba-7102-4c66-8598-a70597e2f825":
                        measuresList = "IP-01, IP-02, IP-03, IP-04, IP-05, IP-06, IP-07,";
                        break;
                    case "5aaf7fba-7102-4c66-8598-a70597e2f825":
                        measuresList = "IP-08, IP-09, IP-10, IP-11,";
                        break;
                    case "3a40cf6b-37ad-4861-b272-930ddf2b8802":
                    case "8d25f78e-86ba-43fb-ba5f-352227187759":
                        measuresList = "IP-12, IP-13, IP-14, IP-15,";
                        break;
                    case "2aaf7fba-7102-4c66-8598-a70597e2f828":
                        measuresList = "ED-01, ED-02, ED-03, ED-04,";
                        break;
                    case "7af51434-5745-4538-b972-193f58e737d7":
                        ReportProfileSetCollection rpsc;
                        if (report.Report.IsDefaultReport)
                        {
                            rpsc = new ReportProfileSetCollection(
                                ReportProfileDisplayItem.Basic |
                                ReportProfileDisplayItem.CostToChargeMedicare |
                                ReportProfileDisplayItem.Map |
                                ReportProfileDisplayItem.PatientExperience |
                                ReportProfileDisplayItem.PayerCost
                            );
                        }
                        else
                        {
                            rpsc = new ReportProfileSetCollection(report.Report.ReportProfile);
                        }
                        display = rpsc.Where(a => a.IsSelected).Select(a => a.Caption).ToArray();

                        break;
                    case "b723c759-d150-415b-a646-7e5a6c7dd185":
                    case "87E04110-46B0-4CAE-9592-022C3111FAC7":
                        display = new[] { "Basic Descriptive Data", "Map", "Overall Score", "Quality Measures", "Health Inspection", "Staffing" };

                        break;
                }

                var reportMeasures = website.Measures.Where(m => m.IsSelected && measuresList.Contains(m.ReportMeasure.MeasureCode + ",")).ToList();
                var columnList = new List<object>();
                foreach (var rv in reportMeasures)
                {
                    var dataElementLink = "";
                    var dataFormat = "";
                    var scale = "";
                    var description = rv.ReportMeasure.Description;
                    switch (rv.ReportMeasure.MeasureCode)
                    {
                        case "ED-01":
                            dataElementLink = "NumEdVisits";
                            dataFormat = "number";
                            break;
                        case "ED-02":
                            dataElementLink = "NumAdmitHosp";
                            dataFormat = "number";
                            break;
                        case "ED-03":
                            dataElementLink = "DiedEd";
                            dataFormat = "number";
                            break;
                        case "ED-04":
                            dataElementLink = "DiedHosp";
                            dataFormat = "number";
                            break;
                        case "IP-01":
                            dataElementLink = "Discharges";
                            dataFormat = "number";
                            break;
                        case "IP-02":
                            dataElementLink = "MeanCharges";
                            dataFormat = "nfcurrency";
                            break;
                        case "IP-03":
                            dataElementLink = "MeanCosts";
                            dataFormat = "nfcurrency";
                            break;
                        case "IP-04":
                            dataElementLink = "MeanLOS";
                            dataFormat = "number";
                            break;
                        case "IP-05":
                            dataElementLink = "MedianCharges";
                            dataFormat = "nfcurrency";
                            break;
                        case "IP-06":
                            dataElementLink = "MedianCosts";
                            dataFormat = "nfcurrency";
                            break;
                        case "IP-07":
                            dataElementLink = "MedianLOS";
                            dataFormat = "number";
                            break;
                        case "IP-08":
                        case "IP-12":
                            dataElementLink = "Discharges";
                            dataFormat = "number";
                            break;
                        case "IP-09":
                        case "IP-13":
                            dataElementLink = "MeanCosts";
                            dataFormat = "nfcurrency";
                            break;
                        case "IP-10":
                        case "IP-14":
                            dataElementLink = "MedianCosts";
                            dataFormat = "nfcurrency";
                            break;
                        case "IP-11":
                        case "IP-15":
                            dataElementLink = "RateDischarges";
                            dataFormat = "number";
                            description = description.SubStrBefore("(");
                            if (!rv.ReportMeasure.ScaleBy.HasValue || rv.ReportMeasure.ScaleBy.Value == 1000)
                            {
                                scale = "Per 1,000 people";
                                description += " (Per 1,000 Persons)";
                            }
                            else
                            {
                                if (rv.ReportMeasure.ScaleBy.Value == 10000)
                                {
                                    scale = "Per 10,000 people";
                                    description += " (Per 10,000 Persons)";
                                }
                                else
                                {
                                    if (rv.ReportMeasure.ScaleBy.Value == 100000)
                                    {
                                        scale = "Per 100,000 people";
                                        description += " (Per 100,000 Persons)";
                                    }
                                }
                            }

                            break;
                    }

                    columnList.Add(new { Name = description, DataElementLink = dataElementLink, DataFormat = dataFormat, Scale = scale });
                }

                var geoInfoFilters = report.Report.Filters
                                                  .Where(c => c.Type == ReportFilterTypeEnum.Hospital ||
                                                              c.Type == ReportFilterTypeEnum.County)
                                                  .Select(c => c.Values).ToList();

                var clinicalDrgFilters = report.Report.Filters
                                                      .Where(c => c.Type == ReportFilterTypeEnum.DRGsDischarges ||
                                                                  c.Type == ReportFilterTypeEnum.ConditionsAndDiagnosis)
                                                      .Select(c => c.Values).ToList();

                var displayFilters = report.Report.Filters
                                                  .Where(c => c.Type == ReportFilterTypeEnum.Display)
                                                  .Select(c => c.Values).Distinct().ToList();


                var icdCodedMessage = GetMixICDCodeString(report, website);


                switch (report.Report.SourceTemplate.RptId.ToLower())
                {
                    case "5aaf7fba-7102-4c66-8598-a70597e2f826":
                    case "5aaf7fba-7102-4c66-8598-a70597e2f825":
                        result.Add(new
                        {
                            ID = report.Report.SourceTemplate.RptId.ToLower(),
                            //ID = report.Report.Id,
                            TYPE = report.Report.ReportType,
                            //  IsActive=true,
                            ReportHeader = report.Report.Description + icdCodedMessage,
                            ReportFooter = GetReportFooter(report, website),
                            ReportQuarters = ExtractReportingQuarters(report, website),
                            ReportingYears = website.Datasets.Where(d => d.Dataset.ContentType.Name == report.Report.Datasets[0]).Select(d => int.Parse(d.Dataset.ReportingYear)).ToArray(),
                            GeoInfo = new[] { "Patient County" },

                            ClinicalDRGAndDiagnosis = clinicalDrgFilters.Any()
                                       ? clinicalDrgFilters[0].Where(f => f.Value).Select(c1 => c1.Name).ToArray()
                                       : new string[] { },
                            //Display =display,
                            Display = displayFilters.Any()
                                            ? displayFilters[0].Where(f => f.Value).Select(c1 => c1.Name).ToArray()
                                            : new string[] { },

                            IncludedColumns = columnList,
                            ShowInterpretationFlag = report.Report.ShowInterpretationText,
                            InterpretationHTMLDescription = report.Report.InterpretationText
                        });
                        break;
                    case "3a40cf6b-37ad-4861-b272-930ddf2b8802":
                    case "8d25f78e-86ba-43fb-ba5f-352227187759":

                        result.Add(new
                        {
                            ID = report.Report.SourceTemplate.RptId.ToLower(),
                            TYPE = report.Report.ReportType,
                            ReportHeader = report.Report.Description + icdCodedMessage,
                            ReportFooter = GetReportFooter(report, website),
                            ReportQuarters = ExtractReportingQuarters(report, website),
                            ReportingYears = website.Datasets.Where(d => d.Dataset.ContentType.Name == report.Report.Datasets[0]).Select(d => int.Parse(d.Dataset.ReportingYear)).ToArray(),
                            GeoInfo = new[] { "Patient Region" },
                            ClinicalDRGAndDiagnosis = clinicalDrgFilters.Any()
                                                               ? clinicalDrgFilters[0].Where(f => f.Value).Select(c1 => c1.Name).ToArray()
                                                               : new string[] { },
                            //Display =display,
                            Display = displayFilters.Any()
                                            ? displayFilters[0].Where(f => f.Value).Select(c1 => c1.Name).ToArray()
                                            : new string[] { },
                            IncludedColumns = columnList,
                            ShowInterpretationFlag = report.Report.ShowInterpretationText,
                            InterpretationHTMLDescription = report.Report.InterpretationText
                        });
                        break;
                    case "4c5727b4-0e85-4f80-ade9-418b49a1373e":
                        result.Add(new
                        {
                            ID = report.Report.SourceTemplate.RptId.ToLower(),
                            //ID = report.Report.Id,
                            TYPE = report.Report.ReportType,
                            ReportHeader = report.Report.Description + icdCodedMessage,
                            ReportFooter = GetReportFooter(report, website),
                            ReportQuarters = ExtractReportingQuarters(report, website),
                            ReportingYears =
                      website.Datasets.Where(d => d.Dataset.ContentType.Name == report.Report.Datasets[0])
                          .Select(d =>
                          {
                              int returnValue = int.Parse(website.ReportedYear);

                              if (!string.IsNullOrEmpty(d.Dataset.ReportingYear))
                                  int.TryParse(d.Dataset.ReportingYear, out returnValue);

                              return returnValue;
                          }).ToArray(),
                            GeoInfo = geoInfoFilters.Any()
                      ? geoInfoFilters[0].Where(f => f.Value).Select(c1 => c1.Name).ToArray()
                      : new string[] { },
                            ClinicalDRGAndDiagnosis = clinicalDrgFilters.Any()
                      ? clinicalDrgFilters[0].Where(f => f.Value).Select(c1 => c1.Name).ToArray()
                      : new string[] { },

                            //Display =display,
                            Display = displayFilters.Any()
                      ? displayFilters[0].Where(f => f.Value).Select(c1 => c1.Name).ToArray()
                      : new string[] { },

                            IncludedColumns = columnList,
                            ShowInterpretationFlag = report.Report.ShowInterpretationText,
                            InterpretationHTMLDescription = report.Report.InterpretationText,
                            ChildConfig = new
                            {
                                MedicalPractice = new
                                {
                                    HelpOverride = false,
                                    InterpretationHTMLDescription = " InterpretationHTMLDescription text here",
                                    Header = "Header Text goes here",
                                    Footer = "Footer Text goes here"
                                },
                                HospitalAffiliation = new
                                {
                                    HelpOverride = false,
                                    InterpretationHTMLDescription = " InterpretationHTMLDescription text here",
                                    Header = "Header Text goes here",
                                    Footer = "Footer Text goes here"
                                }
                            },
                        });
                        break;
                    case "7af51434-5745-4538-b972-193f58e737d7":
                        result.Add(new
                        {
                            ID = report.Report.SourceTemplate.RptId.ToLower(),
                            //ID = report.Report.Id,
                            TYPE = report.Report.ReportType,
                            ReportHeader = report.Report.Description + icdCodedMessage,
                            ReportFooter = GetReportFooter(report, website),
                            ReportQuarters = ExtractReportingQuarters(report, website),
                            ReportingYears =
                      website.Datasets.Where(d => d.Dataset.ContentType.Name == report.Report.Datasets[0])
                          .Select(d =>
                          {
                              int returnValue = int.Parse(website.ReportedYear);

                              if (!string.IsNullOrEmpty(d.Dataset.ReportingYear))
                                  int.TryParse(d.Dataset.ReportingYear, out returnValue);

                              return returnValue;
                          }).ToArray(),
                            GeoInfo = geoInfoFilters.Any()
                      ? geoInfoFilters[0].Where(f => f.Value).Select(c1 => c1.Name).ToArray()
                      : new string[] { },
                            ClinicalDRGAndDiagnosis = clinicalDrgFilters.Any()
                      ? clinicalDrgFilters[0].Where(f => f.Value).Select(c1 => c1.Name).ToArray()
                      : new string[] { },

                            //Display =display,
                            Display = displayFilters.Any()
                      ? displayFilters[0].Where(f => f.Value).Select(c1 => c1.Name).ToArray()
                      : new string[] { },

                            IncludedColumns = columnList,
                            ShowInterpretationFlag = report.Report.ShowInterpretationText,
                            InterpretationHTMLDescription = report.Report.InterpretationText,
                            ChildConfig = new
                            {
                                Top25DRG = new
                                {
                                    HelpOverride = false,
                                    InterpretationHTMLDescription = " InterpretationHTMLDescription text here",
                                    Header = "Header Text goes here",
                                    Footer = "Footer Text goes here"
                                },
                                AdmittingDoctors = new
                                {
                                    HelpOverride = false,
                                    InterpretationHTMLDescription = " InterpretationHTMLDescription text here",
                                    Header = "Header Text goes here",
                                    Footer = "Footer Text goes here"
                                }
                            },
                        });
                        break;
                    default:
                        // If column list is null let's set with actual report columns if custom report.
                        // TODO: Add functionality to add column data type, element link scale in report xml manifest. 
                        // TODO: That way we can avoid hardcoding the report this information. Jason
                        if (report.Report.IsCustom)
                        {
                            if (!columnList.Any() && report.Report.Columns.Any())
                            {
                                report.Report.Columns = report.Report.Columns.RemoveNullValues();
                                columnList.AddRange(report.Report.Columns.Select(column => column.Name).ToList());
                            }
                        }


                        result.Add(new
                        {
                            ID = report.Report.SourceTemplate.RptId.ToLower(),
                            //ID = report.Report.Id,
                            TYPE = report.Report.ReportType,
                            ReportHeader = report.Report.Description + icdCodedMessage,
                            ReportFooter = GetReportFooter(report, website),
                            ReportQuarters = ExtractReportingQuarters(report, website),
                            ReportingYears =
                                website.Datasets.Where(d => d.Dataset.ContentType.Name == report.Report.Datasets[0])
                                    .Select(d =>
                                    {
                                        int returnValue = int.Parse(website.ReportedYear);

                                        if (!string.IsNullOrEmpty(d.Dataset.ReportingYear))
                                            int.TryParse(d.Dataset.ReportingYear, out returnValue);

                                        return returnValue;
                                    }).ToArray(),
                            GeoInfo = geoInfoFilters.Any()
                                ? geoInfoFilters[0].Where(f => f.Value).Select(c1 => c1.Name).ToArray()
                                : new string[] { },
                            ClinicalDRGAndDiagnosis = clinicalDrgFilters.Any()
                                ? clinicalDrgFilters[0].Where(f => f.Value).Select(c1 => c1.Name).ToArray()
                                : new string[] { },

                            //Display =display,
                            Display = displayFilters.Any()
                                ? displayFilters[0].Where(f => f.Value).Select(c1 => c1.Name).ToArray()
                                : new string[] { },

                            IncludedColumns = columnList,
                            ShowInterpretationFlag = report.Report.ShowInterpretationText,
                            InterpretationHTMLDescription = report.Report.InterpretationText
                        });
                        break;
                }
            }

            JsonHelper.GenerateJsonFile(result, fileName, module);

        }

        /// <summary>
        /// Extracts the reporting quarters.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="website">The website.</param>
        /// <returns></returns>
        private object ExtractReportingQuarters(WebsiteReport report, Website website)
        {
            if (report.Report.SourceTemplate.Category != ReportCategory.Utilization || !report.IsQuarterlyTrendingEnabled) return null;

            if (report.SelectedYears == null) return new { Year = int.Parse(website.ReportedYear), Quaters = new[] { 1, 2, 3, 4 } };

            return report.SelectedYears.ToList().Where(yr => yr.Quarters.Any(q => q.IsSelected))
                         .Select(yr =>
                         {
                             return new
                             {
                                 Year = int.Parse(yr.Year),
                                 Quaters = yr.Quarters.Where(q => q.IsSelected)
                                                         .OrderBy(k => k.Text)
                                                         .Select(q => int.Parse(q.Text.Replace("Quarter ", null)))
                             };
                         });
        }

        /// <summary>
        /// Gets the mix icd code string.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="website">The website.</param>
        /// <returns></returns>
        private string GetMixICDCodeString(WebsiteReport report, Website website)
        {
            if (report.Report.Category != ReportCategory.Utilization) return null;


            var ipDatasetIds = website.Datasets
                                        .Where(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge"))
                                        .Distinct()
                                        .Select(d => d.Dataset.Id)
                                        .ToList();

            IList<int> edDatasetIds = new List<int>();
            if (website.Datasets.Any(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("ED Treat And Release")))
            {
                edDatasetIds = website.Datasets
                                        .Where(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("ED Treat And Release"))
                                        .Distinct()
                                        .Select(d => d.Dataset.Id)
                                        .ToList();
            }

            if (!ipDatasetIds.Any() && !edDatasetIds.Any())
                return null;

            string query = "select count(distinct [ICDCodeType]) from Targets_InpatientTargets where [Dataset_Id] in (" +
                        string.Join(",", ipDatasetIds) + ");";

            if (ipDatasetIds.Any() && edDatasetIds.Any())
            {
                query = @";with MixICDCount(ICDCodeType) as (" +
"    select distinct [ICDCodeType] from Targets_InpatientTargets where [Dataset_Id] in (" + string.Join(",", ipDatasetIds) + ")" +
"union" +
"    select distinct [ICDCodeType] from Targets_TreatAndReleaseTargets where [Dataset_Id] in (" + string.Join(",", edDatasetIds) + ")" +
")" +
"select count(distinct ICDCodeType) from MixICDCount;";
            }

            bool result;
            using (var session = this.SessionProvider.SessionFactory.OpenStatelessSession())
            {
                result = (session.CreateSQLQuery(query)
                                   .UniqueResult<int>() > 1);
            }

            return result ? "<br /><p class=\"mixed-icd-msg\">This report contains both ICD 9 and ICD 10 coded data.<p>" : null;
        }

        /// <summary>
        /// Gets the report footer.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="website">The website.</param>
        /// <returns></returns>
        private string GetReportFooter(WebsiteReport report, Website website)
        {
            var datasets = website.Datasets.Where(d => report.Report.Datasets.Contains(d.Dataset.ContentType.Name))
                                           .Select(d => d.Dataset).ToList();

            var footer = new StringBuilder();
            footer.Append("Data Sources: ");

            foreach (var dataset in datasets)
            {
                if (!dataset.ContentType.IsCustom)
                {
                    switch (dataset.ContentType.Name.ToUpper())
                    {
                        case "INPATIENT DISCHARGE":

                            if (report.Report.ReportType.EqualsIgnoreCase("HOSPITAL PROFILE REPORT") && dataset.ReportingYear.EqualsIgnoreCase(website.ReportedYear))
                                footer.Append(string.Format("<br/>{0} ({1})", dataset.ContentType.Name, dataset.ReportingYear));
                            else if (!report.Report.ReportType.EqualsIgnoreCase("HOSPITAL PROFILE REPORT"))
                                footer.Append(string.Format("<br/>{0} ({1})", dataset.ContentType.Name, dataset.ReportingYear));

                            break;
                        case "ED TREAT AND RELEASE":
                        case "AHRQ-QI AREA DATA":
                        case "AHRQ-QI COMPOSITE DATA":
                        case "AHRQ-QI PROVIDER DATA":
                            footer.Append(string.Format("<br/>{0} ({1})", dataset.ContentType.Name, dataset.ReportingYear));
                            break;
                        case "HOSPITAL COMPARE DATA":
                        case "NURSING HOME COMPARE DATA":
                            footer.Append(string.Format("<br/>{0} ({1})", dataset.ContentType.Name, string.Format("{0} {1}", dataset.VersionMonth, dataset.VersionYear)));
                            break;
                        case "MEDICARE PROVIDER CHARGE DATA":
                            footer.Append(string.Format("<br/>{0}", dataset.ContentType.Name));
                            break;
                        default:
                            footer.Append(string.Format("<br/>{0} ({1:MM/dd/yyyy})", dataset.ContentType.Name, dataset.DateImported));
                            break;
                    }
                }
                else
                {
                    footer.AppendLine(string.Format("<br/>{0}{1}", dataset.ContentType.Name, dataset.DateImported.HasValue ? string.Format("({0})", dataset.DateImported.Value.Year) : string.Empty));
                }
            }

            var spacer = "-----<br/>";
            return string.Format("{0}<br/>{2}{1}", footer, report.Report.Footnote, !string.IsNullOrEmpty(report.Report.Footnote) ? spacer : string.Empty);
        }

        /// <summary>
        /// Generates the properties json files.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="isPreview">if set to <c>true</c> [is preview].</param>
        private void GeneratePropertiesJsonFiles(Website website, bool isPreview = false)
        {
            GenerationStatus = WebsiteGenerationStatus.WebsiteFeaturesGenerationInProgress;

            PublishEvent(
				WebsitePublishEventRegion.DefaultRegion, 
				PublishWebsiteResources.GenerationWebsiteContentStart,
				PubishMessageTypeEnum.Information,
                WebsiteGenerationStatus.WebsiteFeaturesGenerationInProgress,
				DateTime.Now);

            GenerateWebsiteConfiguration(website);

            GenerateWebsiteMenus(website);

            // ***** Start about us page JS generation *****
            var aboutUsFilePath = Path.Combine(_baseDataDirectoryPath, ABOUTUS_FILE_NAME);
            var aboutUs = new
            {
                pageContent = website.AboutUsSectionText.ConvertToHTMLParagraph(),
                footerContent = website.AboutUsSectionSummary.ConvertToHTMLParagraph()
            };

            File.WriteAllText(aboutUsFilePath, string.Format("$.monahrq.pgAboutUs = {0}", JsonConvert.SerializeObject(aboutUs, Formatting.Indented)), Encoding.UTF8);

            // ***** End about us page JS generation *****

            GenerationStatus = WebsiteGenerationStatus.WebsiteFeaturesGenerationComplete;

            PublishEvent(
				WebsitePublishEventRegion.DefaultRegion,
				PublishWebsiteResources.GenerationWebsiteContentEnd,
				PubishMessageTypeEnum.Information,
				WebsiteGenerationStatus.WebsiteFeaturesGenerationComplete,
				DateTime.Now);
        }

        /// <summary>
        /// Generates the CSS files.
        /// </summary>
        /// <param name="website">The website.</param>
        private void GenerateThemeFiles(Website website)
        {
            if (website.BannerImage == null || string.IsNullOrEmpty(website.BannerImage.ImagePath) || website.BannerImage.Image == null)
            {
                var banner1 = ConfigService.MonahrqSettings.Banners.OfType<MonahrqBannerElement>().FirstOrDefault(x => x.Name.EqualsIgnoreCase("Healthcare Providers"));
                var fileName = banner1 != null ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, banner1.Value) : string.Empty;
                website.BannerImage = new WebsiteImage()
                {
                    ImagePath = fileName,
                    Image = File.ReadAllBytes(fileName),
                    MemeType = banner1.Value.SubStrAfterLast("."),
                    Name = banner1.Name
                };
            }

            GenerateProfessionalThemesFiles(website);
            GenerateConsumerThemesFiles(website);

            GenerationStatus = WebsiteGenerationStatus.CssGenerationComplete;
            PublishEvent(
				WebsitePublishEventRegion.DefaultRegion, 
				PublishWebsiteResources.GenerationThemeEnd,
				PubishMessageTypeEnum.Information,
				WebsiteGenerationStatus.CssGenerationComplete,
				DateTime.Now);
        }

        /// <summary>
        /// Generates the professional themes files.
        /// </summary>
        /// <param name="website">The website.</param>
        private void GenerateProfessionalThemesFiles(Website website)
        {
            var cssDirectoryPath = Path.Combine(_outputDirectoryPath, @"themes\professional\css\");
            var destinationFilePath = Path.Combine(cssDirectoryPath, CSS_FILE_NAME);
            var defaultCssFilePath = Path.Combine(cssDirectoryPath, DEFAULT_CSS_FILE_NAME);
            const string defaultBannerBackground = @"../../base/assets/Healthcare_Providers.png";
            var userSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\Templates\Site\themes\professional\css");

            GenerationStatus = WebsiteGenerationStatus.CssGenerationInProgress;

            PublishEvent(
				WebsitePublishEventRegion.DefaultRegion,
				PublishWebsiteResources.GenerationThemeStart,
				PubishMessageTypeEnum.Information,
				WebsiteGenerationStatus.CssGenerationInProgress,
				DateTime.Now);

            if (!Directory.Exists(cssDirectoryPath))
            {
                Directory.CreateDirectory(cssDirectoryPath);
            }


            if (website.BannerImage != null)
            {
                var imgName = Path.GetFileName(website.BannerImage.ImagePath);
                if (!string.IsNullOrEmpty(imgName))
                {
                    var bannerBackground = string.Format(@"../../base/assets/{0}", imgName);
                    var defaultCssFileContent = ReadFileContent(Path.Combine(@"Resources\Templates\Site\themes\professional\css", DEFAULT_CSS_FILE_NAME));
                    defaultCssFileContent = defaultCssFileContent.Replace(defaultBannerBackground, bannerBackground);
                    DeleteAndRecreateFile(defaultCssFilePath, defaultCssFileContent, Encoding.UTF8);
                }
            }

            GenerateUserSettingsCSS(website, ConfigService, userSettingsPath, destinationFilePath, Audience.Professionals);
        }

        /// <summary>
        /// Generates the consumer themes files.
        /// </summary>
        /// <param name="website">The website.</param>
        private void GenerateConsumerThemesFiles(Website website)
        {
            var cssDirectoryPath = Path.Combine(_outputDirectoryPath, @"themes\consumer\css\");
            var destinationFilePath = Path.Combine(cssDirectoryPath, CSS_FILE_NAME);
            var userSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\templates\site\themes\consumer\css");

            var imageFileName = Path.GetFileName(website.BannerImage.ImagePath);
            var fileFullName = Path.Combine(_outputDirectoryPath, @"themes\consumer\assets\images", !string.IsNullOrEmpty(imageFileName) ? imageFileName : string.Empty);

            if (!File.Exists(fileFullName))
            {
                File.WriteAllBytes(fileFullName, website.BannerImage.Image);
            }

            GenerateUserSettingsCSS(website, ConfigService, userSettingsPath, destinationFilePath, Audience.Consumers);
        }

        /// <summary>
        /// Initializes the generator.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="publishTask">The publish task.</param>
        private void InitGenerator(Website website, PublishTask publishTask = PublishTask.Full)
        {
            _outputDirectoryPath = string.IsNullOrEmpty(website.OutPutDirectory)
                                                ? Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), MonahrqContext.ApplicationName, "Websites", website.Name)
                                                : website.OutPutDirectory;

            

            switch (publishTask)
            {
                case PublishTask.PreviewOnly:
                    {
                        _outputDirectoryPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
                                            , MonahrqContext.ApplicationName, "WebsitePreviews"
                                            , !string.IsNullOrEmpty(website.Name) ? website.Name : string.Empty);

                        website.OutPutDirectory = _outputDirectoryPath;
                    }
                    SetupOutputDirectory(website);
                    break;
                case PublishTask.BaseCMSZoneOnly:
                    SetupOutputDirectory(website, false);
                    break;
                default:
                    SetupOutputDirectory(website);
                    break;
            }

            _imagesDirectoryPath = Path.Combine(_outputDirectoryPath, @"themes\base\assets");
            _dataDirectoryPath = Path.Combine(_outputDirectoryPath, "Data");
            _baseDataDirectoryPath = Path.Combine(_dataDirectoryPath, "Base");

            IOHelper.CreateDirectory(_dataDirectoryPath);
            IOHelper.CreateDirectory(_baseDataDirectoryPath);
            IOHelper.CreateDirectory(_imagesDirectoryPath);
        }

        /// <summary>
        /// Publishes the event.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="status">The status.</param>
        /// <param name="eventDateType">Type of the event date.</param>
        private void PublishEvent(WebsitePublishEventRegion region, string message, PubishMessageTypeEnum messageType, WebsiteGenerationStatus status, DateTime eventDateType)
		{
			var args = new ExtendedEventArgs<WebsitePublishEventArgs>(new WebsitePublishEventArgs(region, message, messageType, status, eventDateType, _publishTask));
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                {
                    EventAggregator.GetEvent<WebsitePublishEvent>().Publish(args);
                }
            });
        }

        /// <summary>
        /// Generates the website configuration.
        /// </summary>
        /// <param name="website">The website.</param>
        private void GenerateWebsiteConfiguration(Website website)
        {
            var dataDirectoryPath = Path.Combine(_outputDirectoryPath, "Data");
            if (!Directory.Exists(dataDirectoryPath))
            {
                Directory.CreateDirectory(dataDirectoryPath);
            }
            var ssMeasureNames = new[] { /*"PSI 04","PSI 08","PSI 09","PSI 10",*/"PSI 11","PSI 12","PSI 13", /*"PSI 14","OP-6","OP-7",
                                                "SCIP-CARD-2",*/"SCIP-INF-1",/*"SCIP-INF-2A",*/"SCIP-INF-3",/*"SCIP-INF-4","SCIP-INF-9","SCIP-INF-10",*/"SCIP-VTE-2" };
            var nhCaphsCompositeMeasures = new List<string> { "NH_COMP_01", "NH_COMP_02", "NH_COMP_03", "NH_COMP_04", "NH_COMP_05" };
            var cgCahpsCompositeMeauses = new List<string> { "AV_COMP_01", "AV_COMP_02", "AV_COMP_03", "AV_COMP_04", "AV_COMP_05", "AV_COMP_06", "CD_COMP_01", "CD_COMP_02" };

            var cmsOverallStarId = GetOverallMeasureId(website, "CMS-­OVERALL-STAR", true) ?? 0;
            var useOverallStarRatings = cmsOverallStarId > 0 ? 1 : 0;

            var audiences = GetActiveAudiences(website.Audiences);
            // ***** Start config JS generation *****
            var config = new WebsiteConfiguration
            {
                WebsiteVersion = MonahrqContext.ApplicationVersion,
                ActiveAudiences = audiences,
                DefaultAudience = audiences.Count == 1 ? audiences.FirstOrDefault() : website.DefaultAudience != null && website.DefaultAudience == Audience.Consumers ? "consumer" : "professional",
                BrowserTitle = website.BrowserTitle,
                HeaderTitle = website.HeaderTitle,
                Keywords = website.Keywords,
                GoogleAnalyticsKey = website.GoogleAnalyticsKey,
                GoogleMapsApiKey = website.GoogleMapsApiKey,
                MapquestApiKey = MapQuestApiKey,
                States = website.SelectedReportingStates.ConvertAll(s => s.ToLowerInvariant()),
                RegionContext = website.RegionTypeContext,
                ZipCodeRadii = website.SelectedZipCodeRadii,
                GeographicDescription = website.GeographicDescription,
                PhysicianApiToken = PhysiciansImporter.APP_TOKEN,
                UseRealTimeApi = DeterminIfPhysicianRealtime(website),
                Deidentification = 0,
                PublishIFrameVersion = website.PublishIframeVersion.HasValue && website.PublishIframeVersion.Value ? 1 : 0,
                CompressedAndOptimized = website.UtilizationReportCompression.HasValue && website.UtilizationReportCompression.Value ? 1 : 0,

                HospitalOverAllId = website.Measures.Where(m => m.ReportMeasure.MeasureCode.Equals("H-HSP-RATING"))
                .Select(m => m.ReportMeasure.Id)
                .FirstOrDefault().ToString(),

                PatientEXxperienceIDd = website.Measures.Where(m => m.ReportMeasure.MeasureCode.Equals("H-HSP-RATING"))
                        .Select(m => m.ReportMeasure.Id)
                        .FirstOrDefault().ToString(),

                // CMS-OVER-ALL Measure Config
                CmsOverallId = cmsOverallStarId,
                UseCmsOverallId = useOverallStarRatings,

                NursingOverAllId = GetOverallMeasureId(website, "NH-OA-01", false), //Overall Rating
                NursingOverAllQualityId = GetOverallMeasureId(website, "NH-QM-01", true),
                NursingOverAllStaffingId = GetOverallMeasureId(website, "NH-SD-01", true),
                NursingOverAllHealthId = GetOverallMeasureId(website, "NH-HI-01", true),

                SurgicalSafetyMeasures = website.Measures.Where(m => m.IsSelected && m.ReportMeasure.MeasureCode.ToUpper().In(ssMeasureNames))
                .Select(m => m.ReportMeasure.Name).ToArray(),

                NuringHomeFmlyRateId = GetOverallMeasureId(website, "NH_COMP_OVERALL", false),
                NHCaphsOverallMeasures = website.Measures.Where(x => x.IsSelected && x.ReportMeasure.MeasureCode.ToUpper().In(nhCaphsCompositeMeasures)).Select(x => x.ReportMeasure.Id).ToList(),
                NHCaphsQustionsType = GetAllNHCahpsQustionTypes(website),
                MedicalPracticeOverallMeasureId = GetOverallMeasureId(website, "CG_ALL", false),
                MedicalPracticeOverallMeasures = website.Measures.Where(x => x.IsSelected && x.ReportMeasure.MeasureCode.ToUpper().In(cgCahpsCompositeMeauses)).Select(x => x.ReportMeasure.Id).ToList(),
                MedicalPracticeQuestionTypes = GetAllCGCahpsQuestionTypes(website),
            };

            config.Products.ForEach(x =>
                {
                    //x.Value.SelectedTheme = website.SelectedTheme;
                    //x.Value.SelectedFont = website.SelectedFont;

                    x.Value.LogoImagePath = GetImagePath("themes/base/assets/{0}", website.LogoImage);
                    x.Value.BannerImagePath = GetImagePath("themes/base/assets/{0}", website.BannerImage);
                    var homePageImage = GetImagePath("themes/base/assets/{0}", website.HomepageContentImage);

                    if (string.IsNullOrEmpty(homePageImage))
                    {
                        var filePath = string.Format(@"{0}Resources\Templates\Site\themes\professional\assets\photo-data.png", AppDomain.CurrentDomain.BaseDirectory);
                        var fileInfo = new FileInfo(filePath);
                        if (fileInfo.Directory != null)
                        {
                            var targetDirectory = fileInfo.Directory.FullName;
                            if (!Directory.Exists(targetDirectory))
                            {
                                Directory.CreateDirectory(targetDirectory);
                            }
                        }

                        var targetFile = new FileInfo(string.Format(@"{0}\themes\professional\assets\photo-data.png", _outputDirectoryPath));
                        if (!targetFile.Exists)
                        {
                            if (targetFile.Directory != null && !targetFile.Directory.Exists)
                            {
                                Directory.CreateDirectory(targetFile.Directory.FullName);
                            }
                            fileInfo.CopyTo(targetFile.FullName);
                        }
                        x.Value.HomepageContentImagePath = string.Format("themes/professinal/assets/photo-data.png");
                    }
                    else
                    {
                        x.Value.HomepageContentImagePath = GetImagePath("themes/base/assets/{0}", website.HomepageContentImage);
                    }
                    if (website.IncludeFeedbackFormInYourWebsite && website.IsStandardFeedbackForm)
                    {
                        var filePath = string.Format(@"{0}Resources\Feedback\endusersurvey.docx", AppDomain.CurrentDomain.BaseDirectory);
                        IOHelper.DirectoryCopy(new FileInfo(filePath).Directory.FullName, string.Format(@"{0}\Data\Feedback\", _outputDirectoryPath), true, Logger);

                        x.Value.FeebackUrl = "Data/Feedback/endusersurvey.docx";
                    }
                    else
                    {
                        x.Value.FeebackUrl = website.CustomFeedbackFormUrl;
                    }

                    x.Value.FeedbackTopics = website.FeedbackTopics;
                    x.Value.FeedBackEmail = website.FeedBackEmail;
                    x.Value.HomepageVideo = 1;
                    x.Value.HomepageVideoUrl = "https://www.youtube.com/embed/Y45huAp8i6o";

                    if (website.IncludeGuideToolInYourWebsite)
                    {
                        x.Value.guidetool = true;
                    }
                    else
                    {
                        x.Value.guidetool = false;
                    }

                    if (x.Key == "professional")
                    {
                        x.Value.Help = new WebsiteAudience.WebsiteAudienceHelp()
                        {
                            InterpretationHtmlDescription = "Test",
                        };
                    }
                });


            var fileText = string.Format("$.monahrq.configuration= {0}", JsonConvert.SerializeObject(config, Formatting.Indented));
            File.WriteAllText(Path.Combine(dataDirectoryPath, "website_config.js"), fileText, Encoding.UTF8);

        }

        /// <summary>
        /// Gets all cg cahps question types.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <returns></returns>
        private CGCAHPSQuestionType GetAllCGCahpsQuestionTypes(Website website)
        {
            var result = new CGCAHPSQuestionType();
            using (var session = SessionProvider.SessionFactory.OpenSession())
            {
				result.YesNo = session.QueryOver<YesNo>().Where(website.YesNoExcludeClause).List().Select(x => x.Name).ToList().Select(name => Inflector.Titleize(name)).ToList();
				result.Often = session.QueryOver<HowOften>().Where(website.HowOftenExcludeClause).List().Select(x => x.Name).ToList().Select(name => Inflector.Titleize(name)).ToList();
				result.Times = session.QueryOver<Definite>().Where(website.DefiniteExcludeClause).List().Select(x => x.Name).ToList().Select(name => Inflector.Titleize(name)).ToList();
				result.Rating = session.QueryOver<Ratings>().Where(website.RatingExcludeClause).List().Select(x => x.Name).ToList().Select(name => Inflector.Titleize(name)).ToList();
                result.Avg = new List<string> { };
            }
            return result;
        }

        /// <summary>
        /// Gets all nh cahps qustion types.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <returns></returns>
        private NHCAHPSQuestionType GetAllNHCahpsQustionTypes(Website website)
        {
            var result = new NHCAHPSQuestionType();

            using (var session = SessionProvider.SessionFactory.OpenSession())
            {
				result.YesNo = session.QueryOver<YesNo>().Where(website.YesNoExcludeClause).List().Select(x => x.Name).ToList().Select(name => Inflector.Titleize(name)).ToList();
				result.Often = session.QueryOver<HowOften>().Where(website.HowOftenExcludeClause).List().Select(x => x.Name).ToList().Select(name => Inflector.Titleize(name)).ToList();
				result.Times = session.QueryOver<NumberOfTimes2>().Where(website.Times2ExcludeClause).List().Select(x => x.Name).ToList().Select(name => Inflector.Titleize(name)).ToList();
				result.Rating = session.QueryOver<Ratings>().Where(website.RatingExcludeClause).List().Select(x => x.Name).ToList().Select(name => Inflector.Titleize(name)).ToList();
            }
            return result;
        }

        /// <summary>
        /// Generates the website menus.
        /// </summary>
        /// <param name="website">The website.</param>
        private void GenerateWebsiteMenus(Website website)
        {
            var dataDirectoryPath = Path.Combine(_outputDirectoryPath, "Data", "Base");
            var menuSettings = new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver(new List<string> { "SubMenus", "$id", "DataSets", "target", "IsSelected" }),
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            var menuConfigSettings = new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver(new List<string> { "SubMenus", "$id", "DataSets", "Id", "parent", "label", "priority", "route", "IsSelected", "id" }),
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
            if (!Directory.Exists(dataDirectoryPath))
            {
                Directory.CreateDirectory(dataDirectoryPath);
            }

            var items = new List<Menu>();

            website.Menus.Select(x => x.Menu).ToList().ForEach(m => m.FindAllChildren(ref items));
            items = items.Where(x => x.Owner == null || (x.Owner != null && x.DataSets.ContainsAny(website.Datasets.Select(d => d.Dataset.ContentType.Name))))
                         .DistinctBy(wm => wm.ProductLabel)
                         .ToList();

            var websiteMenuItems = JsonHelper.Serialize(items.Where(x => x.IsSelected).ToList(), menuSettings);

            var menuConfigItems = string.Empty;
            var menuConfigs = new List<Menu>();
            using (var session = SessionProvider.SessionFactory.OpenSession())
            {
                menuConfigs = session.Query<Menu>().Where(x => x.Type == "menu-config").Distinct().ToList();
                menuConfigItems = JsonHelper.Serialize(menuConfigs, menuConfigSettings);
            }

            var fileText = string.Format("$.monahrq.Menu= [{0}{2}{1}]", websiteMenuItems.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' }), menuConfigItems.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' }), items.Any() ? "," : "");
            File.WriteAllText(Path.Combine(dataDirectoryPath, "Menu.js"), fileText, Encoding.UTF8);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Deletes the and recreate file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="content">The content.</param>
        /// <param name="encoding">The encoding.</param>
        private static void DeleteAndRecreateFile(string filename, string content, Encoding encoding)
        {
            if (File.Exists(filename))
                File.Delete(filename);

            File.WriteAllText(filename, content, encoding);
        }

        /// <summary>
        /// Gets the active audiences.
        /// </summary>
        /// <param name="audiences">The audiences.</param>
        /// <returns></returns>
        private static List<string> GetActiveAudiences(IEnumerable<Audience> audiences)
        {
            var result = new List<string>();
            foreach (var audience in audiences)
            {
                switch (audience)
                {
                    case Audience.Consumers:
                        result.Add("consumer");
                        break;
                    case Audience.Professionals:
                        result.Add("professional");
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the image path.
        /// </summary>
        /// <param name="imagesRelativePath">The images relative path.</param>
        /// <param name="websiteImage">The website image.</param>
        /// <returns></returns>
        private static string GetImagePath(string imagesRelativePath, WebsiteImage websiteImage)
        {
            if (websiteImage == null || string.IsNullOrEmpty(websiteImage.ImagePath) || websiteImage.Image == null)
                return null;

            var fileName = websiteImage.ImagePath.SubStrAfterLast(@"\");
            return string.Format(imagesRelativePath, fileName);
        }

        /// <summary>
        /// Gets the overall measure identifier.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="meassureCode">The meassure code.</param>
        /// <param name="useIsSelected">if set to <c>true</c> [use is selected].</param>
        /// <returns></returns>
        private static int? GetOverallMeasureId(Website website, string meassureCode, bool useIsSelected)
        {
            if (website == null || string.IsNullOrEmpty(meassureCode)) return null;

            Int32 measureId;
            if (useIsSelected)
                measureId = website.Measures
                 .Where(m => m.IsSelected && m.ReportMeasure.MeasureCode.Equals(meassureCode))
                 .Select(m => m.ReportMeasure.Id)
                 .FirstOrDefault();
            else
                measureId = website.Measures
                 .Where(m => m.ReportMeasure.MeasureCode.Equals(meassureCode))
                 .Select(m => m.ReportMeasure.Id)
                 .FirstOrDefault();

            if (measureId == 0) return null;

            return measureId;
        }

        /// <summary>
        /// Gets the color of the ideal fore.
        /// </summary>
        /// <param name="backgroundColor">Color of the background.</param>
        /// <param name="foreColor">Color of the fore.</param>
        /// <returns></returns>
        private static string GetIdealForeColor(string backgroundColor, string foreColor)
        {
            return ColorTranslator.ToHtml(ColorHelper.GetIdealForeColor(ColorTranslator.FromHtml(backgroundColor), ColorTranslator.FromHtml(foreColor)));
        }

        /// <summary>
        /// Reads the content of the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        private static string ReadFileContent(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;

            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// Determins if physician realtime.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <returns></returns>
        private static int DeterminIfPhysicianRealtime(Website website)
        {
            return (website.Datasets.Any(wds => wds.Dataset.ContentType.Name.EqualsIgnoreCase("Physician Data") && wds.Dataset.UseRealtimeData))
                        ? 1
                        : 0;
        }

        /// <summary>
        /// Replaces the nuggets.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="brandColor">Color of the brand.</param>
        /// <param name="accentColor">Color of the accent.</param>
        /// <param name="accent2Color">Color of the accent2.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        /// <param name="bodyTextColor">Color of the body text.</param>
        /// <param name="linkTextColor">Color of the link text.</param>
        /// <param name="foreground">The foreground.</param>
        /// <param name="websiteFont">The website font.</param>
        /// <param name="brand2Color">Color of the brand2.</param>
        /// <returns></returns>
        private static string ReplaceNuggets(string content, string brandColor, string accentColor, string accent2Color, string backgroundColor, string bodyTextColor, string linkTextColor, string foreground, string websiteFont, string brand2Color)
        {
            if (string.IsNullOrEmpty(content)) return content;
            float percentage = 0.15F;

            return content.Replace("[@@brandColor@@]", brandColor.Replace("#FF", "#"))
                .Replace("[@@accentColor@@]", accentColor.Replace("#FF", "#"))
                .Replace("[@@accent2Color@@]", accent2Color.Replace("#FF", "#"))
                .Replace("[@@backgroundColor@@]", backgroundColor.Replace("#FF", "#"))
                .Replace("[@@bodyTextColor@@]", bodyTextColor.Replace("#FF", "#"))
                .Replace("[@@linkTextColor@@]", linkTextColor.Replace("#FF", "#"))
                .Replace("[@@website_Font@@]", websiteFont)
                .Replace("[@@foregroundColor@@]", foreground.Replace("#FF", "#"))
                .Replace("[@@brand2Color@@]", brand2Color.Replace("#FF", "#"))
                .Replace("[@@brandColorAlt@@]", ColorTranslator.ToHtml(ColorHelper.Darken(ColorTranslator.FromHtml(brandColor), percentage)))
                .Replace("[@@brand2ColorAlt@@]", ColorTranslator.ToHtml(ColorHelper.Darken(ColorTranslator.FromHtml(brand2Color), percentage)));
        }

        /// <summary>
        /// Generates the user settings CSS.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="destinationfile">The destinationfile.</param>
        /// <param name="audienceType">Type of the audience.</param>
        private static void GenerateUserSettingsCSS(Website website, IConfigurationService config, string filePath, string destinationfile, Audience audienceType)
        {

            var availableThemes = config.MonahrqSettings.Themes.OfType<MonahrqThemeElement>();
            var siteTheme = website.Themes.FirstOrDefault(x => x.AudienceType == audienceType);
            var defaultTheme = availableThemes.FirstOrDefault(t => siteTheme != null && t.Name.EqualsIgnoreCase(siteTheme.SelectedTheme))
                ?? availableThemes.SingleOrDefault(t => t.Name.ContainsIgnoreCase("Default"));

            var brandColor = siteTheme != null && !string.IsNullOrEmpty(siteTheme.BrandColor) ? siteTheme.BrandColor : defaultTheme.BrandColor;
            var brand2Color = siteTheme != null && !string.IsNullOrEmpty(siteTheme.Brand2Color) ? siteTheme.Brand2Color : defaultTheme.BrandColor;
            var accentColor = siteTheme != null && !string.IsNullOrEmpty(siteTheme.AccentColor) ? siteTheme.AccentColor : defaultTheme.AccentColor;
            var accent2Color = defaultTheme.Accent2Color;
            var websiteFont = siteTheme != null && !string.IsNullOrEmpty((siteTheme.SelectedFont)) ? siteTheme.SelectedFont : "'Droid Sans', Arial, sans-serif;";
            var backgroundColor = siteTheme != null && !string.IsNullOrEmpty((siteTheme.BackgroundColor)) ? siteTheme.BackgroundColor : defaultTheme.BackgroundColor;
            var bodyTextColor = siteTheme != null && string.IsNullOrEmpty((siteTheme.BodyTextColor)) ? siteTheme.BodyTextColor : defaultTheme.BodyTextColor;
            var linkTextColor = siteTheme != null && !string.IsNullOrEmpty((siteTheme.LinkTextColor)) ? siteTheme.LinkTextColor : defaultTheme.LinkTextColor;
            var foreground = GetIdealForeColor(brandColor, "white");

            var cssfilecontent = ReadFileContent(Path.Combine(filePath, CSS_FILE_NAME));

            cssfilecontent = ReplaceNuggets(cssfilecontent, brandColor, accentColor, accent2Color, backgroundColor, bodyTextColor, linkTextColor, foreground, websiteFont, brand2Color);

            if (!Directory.Exists(Path.GetDirectoryName(destinationfile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationfile));
            }


            DeleteAndRecreateFile(destinationfile, cssfilecontent, Encoding.UTF8);
        }

        #endregion
    }

    /// <summary>
    /// The pubish task enumeration. Used in determining website generation workflow.
    /// </summary>
    public enum PublishTask
    {
        /// <summary>
        /// The theme only
        /// </summary>
        ThemeOnly,
        /// <summary>
        /// The reports only
        /// </summary>
        ReportsOnly,
        /// <summary>
        /// The preview only
        /// </summary>
        PreviewOnly,
        /// <summary>
        /// The base CMS zone only
        /// </summary>
        BaseCMSZoneOnly,
        /// <summary>
        /// The full
        /// </summary>
        Full
    }

    /// <summary>
    /// The website generation status enumeration. Used in website generation workflow.
    /// </summary>
    [Serializable]
    public enum WebsiteGenerationStatus
    {
        /// <summary>
        /// The in progress
        /// </summary>
        InProgress,
        /// <summary>
        /// The output directory create update complete
        /// </summary>
        OutputDirectoryCreateUpdateComplete,
        /// <summary>
        /// The website features generation in progress
        /// </summary>
        WebsiteFeaturesGenerationInProgress,
        /// <summary>
        /// The website features generation complete
        /// </summary>
        WebsiteFeaturesGenerationComplete,
        /// <summary>
        /// The CSS generated
        /// </summary>
        CssGenerated,
        /// <summary>
        /// The CSS generation in progress
        /// </summary>
        CssGenerationInProgress,
        /// <summary>
        /// The CSS generation complete
        /// </summary>
        CssGenerationComplete,
        /// <summary>
        /// The reports generation in progress
        /// </summary>
        ReportsGenerationInProgress,
        /// <summary>
        /// The reports generation complete
        /// </summary>
        ReportsGenerationComplete,
        /// <summary>
        /// The error
        /// </summary>
        Error,
        /// <summary>
        /// The complete
        /// </summary>
        Complete,
        /// <summary>
        /// The preview complete
        /// </summary>
        PreviewComplete
    }

    /// <summary>
    /// The Website publish event.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Infrastructure.Entities.Events.ExtendedEventArgs{Monahrq.Sdk.Services.Generators.WebsitePublishEventArgs}}" />
    public class WebsitePublishEvent : CompositePresentationEvent<ExtendedEventArgs<WebsitePublishEventArgs>> { }

    /// <summary>
    /// The class to pass the website Publis event region information.
    /// </summary>
    public class WebsitePublishEventRegion
	{
        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="WebsitePublishEventRegion"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="order">The order.</param>
        public WebsitePublishEventRegion(string name=DefaultRegionName, int order=DefaultRegionOrder) { Name = name; Order = order; }
        /// <summary>
        /// Initializes a new instance of the <see cref="WebsitePublishEventRegion"/> class.
        /// </summary>
        /// <param name="reportGen">The report gen.</param>
        public WebsitePublishEventRegion(IReportGenerator reportGen)
		{

			var reportGenAtt = reportGen.GetType().GetCustomAttribute<ReportGeneratorAttribute>();
			Name =
				!string.IsNullOrEmpty(reportGenAtt.EventRegion)
					? reportGenAtt.EventRegion
                    : Inflector.Titleize(reportGen.GetType().Name.Replace("ReportGenerator", ""));
			Order = reportGen.ExecutionOrder;
		}

        /// <summary>
        /// The default region name
        /// </summary>
        private const string DefaultRegionName = "General";
        /// <summary>
        /// The default region order
        /// </summary>
        private const int DefaultRegionOrder = -10;
        /// <summary>
        /// The default region
        /// </summary>
        public static WebsitePublishEventRegion DefaultRegion = new WebsitePublishEventRegion(DefaultRegionName, DefaultRegionOrder);
	}

    /// <summary>
    /// The website publish event arguments used in website generation event publishing status.
    /// </summary>
    public class WebsitePublishEventArgs
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="WebsitePublishEventArgs"/> class.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="status">The status.</param>
        /// <param name="eventDateType">Type of the event date.</param>
        /// <param name="publishTask">The publish task.</param>
        public WebsitePublishEventArgs(WebsitePublishEventRegion region, string message, PubishMessageTypeEnum messageType, WebsiteGenerationStatus status,
                                       DateTime eventDateType, PublishTask publishTask)
        {
            Region = region != null ? region : WebsitePublishEventRegion.DefaultRegion;
			Message = message;
			MessageType = messageType;
            Status = status;
            EventTime = eventDateType;
            PublishTask = publishTask;
        }

        /// <summary>
        /// Gets the region.
        /// </summary>
        /// <value>
        /// The region.
        /// </value>
        public WebsitePublishEventRegion Region { get; private set; }
        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; private set; }
        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public PubishMessageTypeEnum MessageType { get; private set; }
        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public WebsiteGenerationStatus Status { get; private set; }
        /// <summary>
        /// Gets the event time.
        /// </summary>
        /// <value>
        /// The event time.
        /// </value>
        public DateTime EventTime { get; private set; }
        /// <summary>
        /// Gets or sets the website output directory.
        /// </summary>
        /// <value>
        /// The website output directory.
        /// </value>
        public string WebsiteOutputDirectory { get; set; }
        /// <summary>
        /// Gets or sets the publish task.
        /// </summary>
        /// <value>
        /// The publish task.
        /// </value>
        public PublishTask PublishTask { get; set; }
    }

    /// <summary>
    /// The website generation pushlishing message type enumeration.
    /// </summary>
    public enum PubishMessageTypeEnum
    {
        /// <summary>
        /// The error
        /// </summary>
        Error,
        /// <summary>
        /// The information
        /// </summary>
        Information,
        /// <summary>
        /// The warning
        /// </summary>
        Warning
    }

    #region Flutter Related Objects

    /// <summary>
    /// the open source flutter registery dto object used in serialization of the flutter registry used in published website.
    /// </summary>
    [DataContract]
    public class FlutterRegistryDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlutterRegistryDto"/> class.
        /// </summary>
        public FlutterRegistryDto()
        {
            Flutters = new List<FlutterDto>();
        }

        /// <summary>
        /// Gets or sets the install 3rd party flutters .
        /// </summary>
        /// <value>
        /// The flutters.
        /// </value>
        [DataMember(Name = "flutters")]
        public List<FlutterDto> Flutters { get; set; }
    }

    [DataContract]
    public class FlutterDto
    {
        /// <summary>
        /// Gets or sets the flutter module path.
        /// </summary>
        /// <value>
        /// The module path.
        /// </value>
        [DataMember(Name = "modulePath")]
        public string ModulePath { get; set; }
        /// <summary>
        /// Gets or sets the fullter configuration filename (file path).
        /// </summary>
        /// <value>
        /// The configuration filename.
        /// </value>
        [DataMember(Name = "configFilename")]
        public string ConfigFilename { get; set; }
        /// <summary>
        /// Gets or sets the configuration identifier.
        /// </summary>
        /// <value>
        /// The configuration identifier.
        /// </value>
        [DataMember(Name = "configId")]
        public string ConfigId { get; set; }
    }
    #endregion


}
