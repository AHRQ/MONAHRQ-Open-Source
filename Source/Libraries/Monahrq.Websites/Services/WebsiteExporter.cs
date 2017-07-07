using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Win32;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Extensions;
using NHibernate.Criterion;
using NHibernate.Linq;
using System.ComponentModel.Composition;
using Microsoft.Practices.ServiceLocation;

namespace Monahrq.Websites.Services
{
    public class WebsiteExporter
    {

        public static void Export(Website website)
        {
            if (website == null) return;

            var exportFileName = string.Format("{0}_Settings_{1:yyyyMMdd}.txt", website.Name.Replace(" ", "_"), DateTime.Now);
            var exportDirectory = MonahrqContext.FileExportsDirPath;
            if (!Directory.Exists(exportDirectory)) Directory.CreateDirectory(exportDirectory);

            var websitDto = new WebsiteExport(website);

            var websiteXml = XmlHelper.Serialize(websitDto, true);

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                FileName = exportFileName,
                AddExtension = true,
                DefaultExt = ".txt",
                InitialDirectory = exportDirectory
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                Stream xmlStream;
                using (xmlStream = saveFileDialog.OpenFile())
                {
                    // Code to write the stream goes here.
                    var xr = XmlWriter.Create(xmlStream);
                    websiteXml.Save(xr);

                    xmlStream.Flush(); //Adjust this if you want read your data 
                    xmlStream.Position = 0;
                    xmlStream.Close();
                }
            }
            saveFileDialog = null;
        }

        public static void Import(ImportRequest request, CancellationToken ctx, Action<ImportResponse> responseCallback, Action<ImportResponse> errorCallback)
        {
            Task.Run(() =>
            {
                Website newWebsite = new Website();
                var importCompleted = false;

                try
                {
                    WebsiteExport websiteExport = null;
                    Website existingWebsite = null;

                    ctx.ThrowIfCancellationRequested();

                    while (!importCompleted)
                    {
                        if (request.ImportType == WebsiteImportTypeEnum.File)
                        {
                            var websiteXmlDoc = new XmlDocument();
                            websiteXmlDoc.LoadXml(request.WebsiteXmlFromFile);
                            websiteExport = XmlHelper.Deserialize<WebsiteExport>(websiteXmlDoc);
                        }
                        else
                        {
                            using (var session = request.Provider.SessionFactory.OpenSession())
                            {
                                existingWebsite = session.CreateCriteria<Website>()
                                                             .Add(Restrictions.Eq("Id", request.ExistingWebsiteId))
                                                             .SetMaxResults(1)
                                                             .Future<Website>()
                                                             .FirstOrDefault();

                                if (existingWebsite != null)
                                {
                                    websiteExport = new WebsiteExport(existingWebsite);
                                }
                            }
                        }

                        if (websiteExport == null && existingWebsite == null)
                            throw new Exception("Website could not be imported due to either the website not existing and/or the website Import file is invalid. Please contact MONAHRQ Technical if issue persists.");

                        newWebsite.Id = 0;
                        newWebsite.CurrentStatus = null;
                        if (newWebsite.ActivityLog == null)
                            newWebsite.ActivityLog = new List<ActivityLogEntry>();

                        responseCallback(new ImportResponse { IsComplete = false, Message = "Processing website details." });

                        if (!ProcessDetails(request.ImportType, websiteExport, existingWebsite, newWebsite, request.Logger))
                        {
                            responseCallback(new ImportResponse { IsComplete = false, Message = "An error occurred while processing details. Continuing to settings processing." });
                        }
                        //responseCallback(new ImportResponse { IsComplete = false, Website = default(Website) });

                        responseCallback(new ImportResponse { IsComplete = false, Message = "Processing website settings." });
                        if (!ProcessSettings(request.ImportType, websiteExport, existingWebsite, newWebsite, request.Provider, request.Logger))
                        {
                            responseCallback(new ImportResponse { IsComplete = false, Message = "An error occurred while processing settings. Continuing to dataset processing." });
                        }
                        //responseCallback(new ImportResponse { IsComplete = false, Website = default(Website) });

                        responseCallback(new ImportResponse { IsComplete = false, Message = "Processing website datasets." });
                        if (!ProcessDatasets(request.ImportType, websiteExport, existingWebsite, newWebsite, request.Provider, request.Logger))
                        {
                            responseCallback(new ImportResponse { IsComplete = false, Message = "An error occurred while processing datsets. Continuing to measures processing." });
                        }
                        // responseCallback(new ImportResponse { IsComplete = false, Website = default(Website) });

                        responseCallback(new ImportResponse { IsComplete = false, Message = "Processing website measures." });
                        if (!ProcessMeasures(request.ImportType, websiteExport, existingWebsite, newWebsite, request.Provider, request.Logger))
                        {
                            responseCallback(new ImportResponse { IsComplete = false, Message = "An error occurred while processing measures. Continuing to reports processing." });
                        }
                        //responseCallback(new ImportResponse { IsComplete = false, Website = default(Website) });

                        responseCallback(new ImportResponse { IsComplete = false, Message = "Processing website reports." });
                        if (!ProcessReports(request.ImportType, websiteExport, existingWebsite, newWebsite, request.Provider, request.Logger))
                        {
                            responseCallback(new ImportResponse { IsComplete = false, Message = "An error occurred while processing reports." });
                        }
                        //responseCallback(new ImportResponse { IsComplete = false, Website = default(Website) });

                        if (ctx.IsCancellationRequested)
                        {
                            importCompleted = true;
                            responseCallback(new ImportResponse { IsComplete = true, Website = default(Website), Message = "Import cancelled by user." });
                            //ctx.ThrowIfCancellationRequested();
                            // return Task<Website>.FromResult(default(Website));
                        }
                        importCompleted = true;
                    }

                    responseCallback(new ImportResponse { IsComplete = true, Website = newWebsite, Message = "Website import complete successfully." });

                    // return Task<Website>.FromResult(newWebsite);
                }
                catch (Exception exc)
                {
                    importCompleted = false;
                    var excToUse = exc.GetBaseException();
                    errorCallback(new ImportResponse { IsComplete = false, Exception = excToUse, Message = excToUse.Message });
                    //return;
                }
            }, ctx);
        }

        #region Processing Methods
        private static bool ProcessDetails(WebsiteImportTypeEnum importType, WebsiteExport websiteExport, Website existingWebsite, Website newWebsite, ILogWriter logger)
        {
            try
            {
                if (importType == WebsiteImportTypeEnum.File && websiteExport != null)
                {
                    newWebsite.Name = string.Format("Copy of {0}", websiteExport.Name);
                    newWebsite.StateContext = websiteExport.StateContext.ToList();
                    newWebsite.SelectedReportingStates = websiteExport.ReportingStates.ToList();
                    newWebsite.RegionTypeContext = websiteExport.RegionTypeContext;
                    //newWebsite.CurrentStatus = websiteExport.CurrentStatus;
                    newWebsite.Description = websiteExport.Description;
                    newWebsite.ReportedYear = websiteExport.ReportedYear;
                    newWebsite.ReportedQuarter = websiteExport.ReportedQuarterNullable;
                    newWebsite.Audiences = websiteExport.Audiences.ToList();
                    newWebsite.DefaultAudience = websiteExport.DefaultAudienceNullable;
                }
                else if (importType == WebsiteImportTypeEnum.Existing && existingWebsite != null)
                {
                    newWebsite.Id = 0;
                    newWebsite.Name = string.Format("Copy of {0}", existingWebsite.Name);
                    newWebsite.StateContext = existingWebsite.StateContext.ToList();
                    newWebsite.SelectedReportingStates = existingWebsite.SelectedReportingStates.ToList();
                    newWebsite.RegionTypeContext = existingWebsite.RegionTypeContext;
                    //newWebsite.CurrentStatus = existingWebsite.CurrentStatus;
                    newWebsite.Description = existingWebsite.Description;
                    newWebsite.ReportedYear = existingWebsite.ReportedYear;
                    newWebsite.ReportedQuarter = existingWebsite.ReportedQuarter;
                    newWebsite.Audiences = existingWebsite.Audiences.ToList();
                    newWebsite.DefaultAudience = existingWebsite.DefaultAudience;
                }

                newWebsite.ActivityLog.Add(new ActivityLogEntry("Details created and/or updated", DateTime.Now));

                return true;
            }
            catch (Exception exc)
            {
                logger.Write(exc.GetBaseException(), TraceEventType.Error);
                return false;
            }
        }

        private static bool ProcessSettings(WebsiteImportTypeEnum importType, WebsiteExport websiteExport, Website existingWebsite, Website newWebsite, IDomainSessionFactoryProvider provider, ILogWriter logger)
        {
            try
            {
                var config = ServiceLocator.Current.GetInstance<IConfigurationService>();
                var availableWebsiteThemes = config.MonahrqSettings.Themes.Cast<MonahrqThemeElement>();
                var defaultTheme = availableWebsiteThemes.FirstOrDefault(x => x.Name.Contains("Default"));

                if (importType == WebsiteImportTypeEnum.File && websiteExport != null)
                {
                    newWebsite.OutPutDirectory = websiteExport.OutPutDirectory;

                    newWebsite.SelectedZipCodeRadii = websiteExport.SelectedZipCodeRadii.ToList();
                    newWebsite.GeographicDescription = websiteExport.GeographicDescription;

                    newWebsite.AboutUsSectionSummary = websiteExport.AboutUsSectionSummary;
                    newWebsite.AboutUsSectionText = websiteExport.AboutUsSectionText;

                    newWebsite.CustomFeedbackFormUrl = websiteExport.CustomFeedbackFormUrl;
                    newWebsite.FeedBackEmail = websiteExport.FeedBackEmail;
                    newWebsite.FeedbackTopics = websiteExport.FeedbackTopics.ToList();
                    newWebsite.IncludeFeedbackFormInYourWebsite = websiteExport.IncludeFeedbackFormInYourWebsite;

                    newWebsite.IncludeGuideToolInYourWebsite = websiteExport.IncludeGuideToolInYourWebsite;

                    newWebsite.GoogleAnalyticsKey = websiteExport.GoogleAnalyticsKey;
                    newWebsite.GoogleMapsApiKey = websiteExport.GoogleMapsApiKey;

                    newWebsite.BrowserTitle = websiteExport.BrowserTitle;
                    newWebsite.HeaderTitle = websiteExport.HeaderTitle;
                    newWebsite.Keywords = websiteExport.Keywords;

                    var theme = availableWebsiteThemes.FirstOrDefault(x => x.Name.ContainsCaseInsensitive(websiteExport.SelectedTheme));
                    var professionalWebsiteTheme = GetWebsiteTheme(Audience.Professionals, websiteExport.SelectedTheme, websiteExport.AccentColor, websiteExport.SelectedFont, websiteExport.BrandColor, theme);

                    if (!string.IsNullOrEmpty(websiteExport.ConsumerBrandColor))
                    {
                        var consumerWebsiteTheme = GetWebsiteTheme(Audience.Consumers, websiteExport.ConsumerSelectedTheme, websiteExport.AccentColor, websiteExport.ConsumerSelectedFont, websiteExport.ConsumerBrandColor, theme);
                        newWebsite.Themes.Add(consumerWebsiteTheme);
                    }
                    newWebsite.Themes.Add(professionalWebsiteTheme);
                    newWebsite.Menus = !string.IsNullOrEmpty(websiteExport.Menus.Value) ?
                         JsonHelper.Deserialize<List<Menu>>(websiteExport.Menus.Value).Select(x => new WebsiteMenu { Menu = x }).ToList()
                         : new List<WebsiteMenu>(); ;

                    if (websiteExport.BannerImage != null)
                    {
                        // Encoding.UTF8.GetBytes(websiteExport.BannerImage.Image),
                        newWebsite.BannerImage = new WebsiteImage
                        {
                            Image = websiteExport.BannerImage.Image,
                            MemeType = websiteExport.BannerImage.MemeType,
                            ImagePath = websiteExport.BannerImage.Path
                        };
                    }

                    if (websiteExport.LogoImage != null)
                    {
                        //Encoding.UTF8.GetBytes(websiteExport.LogoImage.Image),
                        newWebsite.LogoImage = new WebsiteImage
                        {
                            Image = websiteExport.LogoImage.Image,
                            MemeType = websiteExport.LogoImage.MemeType,
                            ImagePath = websiteExport.LogoImage.Path
                        };
                    }

                    if (websiteExport.Hospitals != null && websiteExport.Hospitals.Any())
                    {
                        var hospitals = new List<Hospital>();
                        var hositalIdList = websiteExport.Hospitals.Select(ds => ds.Id).ToList();

                        var hospitExportList = websiteExport.Hospitals
                                                            .ToDictionary(wh => wh.ProviderId,
                                                                          wh => new Holder { Hospital = null, ProviderId = wh.ProviderId, Ccr = wh.Ccr });

                        using (var session = provider.SessionFactory.OpenSession())
                        {
                            hospitals = session.CreateCriteria<Hospital>()
                                                .Add(Restrictions.In("State", newWebsite.StateContext.Cast<object>().ToArray()))
                                                .Add(Restrictions.And(Restrictions.Eq("IsArchived", false), Restrictions.Eq("IsDeleted", false)))
                                                .Future<Hospital>()
                                                .ToList();
                        }

                        if (hospitals.Any())
                        {
                            if (newWebsite.Hospitals == null) newWebsite.Hospitals = new List<WebsiteHospital>();
                            foreach (var hospital in hospitals)
                            {
                                if (hospitExportList.Values.All(x => x.ProviderId.ToUpper() != hospital.CmsProviderID.ToUpper())) continue;

                                hospitExportList[hospital.CmsProviderID].Hospital = hospital;
                            }

                            foreach (var hospitalHolder in hospitExportList.Where(x => x.Value.Hospital != null).ToList())
                            {
                                newWebsite.Hospitals.Add(new WebsiteHospital
                                {
                                    Hospital = hospitalHolder.Value.Hospital,
                                    CCR = hospitalHolder.Value.Ccr
                                });
                            }
                        }
                        else
                        {
                            newWebsite.Hospitals = new List<WebsiteHospital>();
                        }
                    }

                    if (websiteExport.NursingHomes != null && websiteExport.NursingHomes.Any())
                    {
                        var nursingHomes = new List<NursingHome>();
                        // var nursingHomeList = websiteExport.NursingHomes.Select(ds => ds.Id).ToList();

                        var nursingHomeExportList = websiteExport.NursingHomes.ToDictionary(h => h.ProviderId,
                                                                                                 h => new Holder { NursingHome = null });

                        using (var session = provider.SessionFactory.OpenSession())
                        {
                            nursingHomes = session.CreateCriteria<NursingHome>()
                                                  .Add(Restrictions.In("State", newWebsite.StateContext.Cast<object>().ToArray()))
                                                  //.Add(Restrictions.Eq("IsDeleted",false))
                                                  .Future<NursingHome>()
                                                  .ToList();

                        }

                        if (nursingHomes.Any())
                        {
                            if (newWebsite.NursingHomes == null) newWebsite.NursingHomes = new List<WebsiteNursingHome>();
                            foreach (var nursingHome in nursingHomes)
                            {
                                if (!nursingHomeExportList.ContainsKey(nursingHome.ProviderId)) continue;

                                nursingHomeExportList[nursingHome.ProviderId].NursingHome = nursingHome;
                            }

                            foreach (var hospitalHolder in nursingHomeExportList.Where(x => x.Value != null && x.Value.NursingHome != null).ToList())
                            {
                                newWebsite.NursingHomes.Add(new WebsiteNursingHome
                                {
                                    NursingHome = hospitalHolder.Value.NursingHome
                                });
                            }
                        }
                        else
                        {
                            newWebsite.NursingHomes = new List<WebsiteNursingHome>();
                        }
                    }
                }
                else if (importType == WebsiteImportTypeEnum.Existing && existingWebsite != null)
                {
                    newWebsite.OutPutDirectory = existingWebsite.OutPutDirectory;

                    newWebsite.SelectedZipCodeRadii = existingWebsite.SelectedZipCodeRadii.ToList();
                    newWebsite.GeographicDescription = existingWebsite.GeographicDescription;

                    newWebsite.AboutUsSectionSummary = existingWebsite.AboutUsSectionSummary;
                    newWebsite.AboutUsSectionText = existingWebsite.AboutUsSectionText;

                    newWebsite.CustomFeedbackFormUrl = existingWebsite.CustomFeedbackFormUrl;
                    newWebsite.FeedBackEmail = existingWebsite.FeedBackEmail;
                    newWebsite.FeedbackTopics = existingWebsite.FeedbackTopics.ToList();
                    newWebsite.IncludeFeedbackFormInYourWebsite = existingWebsite.IncludeFeedbackFormInYourWebsite;

                    newWebsite.IncludeGuideToolInYourWebsite = existingWebsite.IncludeGuideToolInYourWebsite;

                    newWebsite.GoogleAnalyticsKey = existingWebsite.GoogleAnalyticsKey;
                    newWebsite.GoogleMapsApiKey = existingWebsite.GoogleMapsApiKey;

                    newWebsite.BrowserTitle = existingWebsite.BrowserTitle;
                    newWebsite.HeaderTitle = existingWebsite.HeaderTitle;
                    newWebsite.Keywords = existingWebsite.Keywords;

                    if (existingWebsite.Themes != null && existingWebsite.Themes.Any())
                    {
                        newWebsite.Themes = existingWebsite.Themes.Select(t => new WebsiteTheme
                        {
                            AudienceType = t.AudienceType,
                            SelectedTheme = t.SelectedTheme ?? defaultTheme.Name,
                            AccentColor = t.AccentColor ?? defaultTheme.AccentColor,
                            BrandColor = t.BrandColor ?? defaultTheme.BrandColor,
                            SelectedFont = t.SelectedFont ?? "'Droid Sans', Arial, sans-serif;",
                            BackgroundColor = t.BackgroundColor ?? defaultTheme.BackgroundColor,
                            BodyTextColor = t.BodyTextColor ?? defaultTheme.BodyTextColor,
                            LinkTextColor = t.LinkTextColor ?? defaultTheme.LinkTextColor
                        }).ToList();
                    }

                    if (existingWebsite.Hospitals != null && existingWebsite.Hospitals.Any())
                    {
                        newWebsite.Hospitals = existingWebsite.Hospitals.Select(wh => new WebsiteHospital { Hospital = wh.Hospital, CCR = wh.CCR, Index = wh.Index })
                                                                        .ToList();
                    }

                    if (existingWebsite.NursingHomes != null && existingWebsite.NursingHomes.Any())
                    {
                        newWebsite.NursingHomes = existingWebsite.NursingHomes.Select(nh => new WebsiteNursingHome { NursingHome = nh.NursingHome, Index = nh.Index })
                                                                        .ToList();
                    }

                    if (existingWebsite.BannerImage != null)
                    {
                        newWebsite.BannerImage = existingWebsite.BannerImage;
                    }

                    if (existingWebsite.LogoImage != null)
                    {
                        newWebsite.LogoImage = existingWebsite.LogoImage;
                    }
                }

                newWebsite.ActivityLog.Add(new ActivityLogEntry("Settings have be saved and/our updated.", DateTime.Now));

                return true;
            }
            catch (Exception exc)
            {
                logger.Write(exc.GetBaseException(), TraceEventType.Error);
                return false;
            }
        }

        class Holder
        {
            public Hospital Hospital { get; set; }
            public string ProviderId { get; set; }
            public NursingHome NursingHome { get; set; }
            public string Ccr { get; set; }
        }

        private static bool ProcessDatasets(WebsiteImportTypeEnum importType, WebsiteExport websiteExport, Website existingWebsite, Website newWebsite,
                                            IDomainSessionFactoryProvider provider, ILogWriter logger)
        {
            try
            {
                if (importType == WebsiteImportTypeEnum.File && websiteExport != null)
                {
                    List<Dataset> datasets;
                    List<string> datasetNameList = websiteExport.Datasets.Select(ds => ds.Name).ToList();

                    using (var session = provider.SessionFactory.OpenSession())
                    {
                        datasets = session.Query<Dataset>()
                                        .Where(d => datasetNameList.Contains(d.File))
                                        .ToFuture()
                                        .ToList();
                    }

                    if (datasets.Any())
                    {
                        if (newWebsite.Datasets == null) newWebsite.Datasets = new List<WebsiteDataset>();
                        datasets.ForEach(d =>
                        {
                            newWebsite.Datasets.Add(new WebsiteDataset { Dataset = d });
                        });
                    }
                }
                else if (importType == WebsiteImportTypeEnum.Existing && existingWebsite != null)
                {
                    newWebsite.Datasets = existingWebsite.Datasets.DistinctBy(d => d.Dataset.Id).Select(wd => new WebsiteDataset { Dataset = wd.Dataset, Index = wd.Index }).ToList();
                    //newWebsite.Datasets.ForEach(wd => wd.Id= 0);
                }

                return true;
            }
            catch (Exception exc)
            {
                logger.Write(exc.GetBaseException(), TraceEventType.Error);
                return false;
            }
        }

        private static bool ProcessMeasures(WebsiteImportTypeEnum importType, WebsiteExport websiteExport, Website existingWebsite, Website newWebsite,
                                            IDomainSessionFactoryProvider provider, ILogWriter logger)
        {
            try
            {
                if (importType == WebsiteImportTypeEnum.File && websiteExport != null)
                {
                    List<WebsiteMeasure> measures;

                    using (var session = provider.SessionFactory.OpenSession())
                    {
                        measures = (from measure in websiteExport.Measures
                                    let originalMeasure = session.Query<Measure>().Where(m => m.Name.ToUpper() == measure.OrginalCode.ToUpper() && !m.IsOverride).ToFuture().FirstOrDefault()
                                    where originalMeasure != null
                                    let overrideMeasure = session.Query<Measure>().Where(m => m.Name.ToUpper() == (!string.IsNullOrEmpty(measure.OverrideCode) ? measure.OverrideCode.ToUpper() : string.Empty) && m.IsOverride).ToFuture().FirstOrDefault()
                                    select new WebsiteMeasure
                                    {
                                        OriginalMeasure = originalMeasure,
                                        OverrideMeasure = overrideMeasure,
                                        IsSelected = measure.IsSelected
                                    }).ToList();
                    }

                    if (measures.Any())
                    {
                        if (newWebsite.Measures == null) newWebsite.Measures = new List<WebsiteMeasure>();

                        if (newWebsite.Datasets.Any())
                        {
                            var targetNames = newWebsite.Datasets.DistinctBy(d => d.Dataset.Id).Select(d => d.Dataset.ContentType.Name).ToList();
                            //newWebsite.Measures.ToList().RemoveAll(m => !m.OriginalMeasure.Owner.Name.In(targetNames));

                            foreach (var measure in measures.Where(measure => measure.OriginalMeasure.Owner.Name.In(targetNames)).ToList())
                                newWebsite.Measures.Add(measure);
                        }
                        else
                        {
                            newWebsite.Measures = new List<WebsiteMeasure>();
                        }
                    }
                }
                else if (importType == WebsiteImportTypeEnum.Existing && existingWebsite != null)
                {
                    newWebsite.Measures = existingWebsite.Measures.DistinctBy(d => d.OriginalMeasure.Id).Select(wm => new WebsiteMeasure { OriginalMeasure = wm.OriginalMeasure, OverrideMeasure = wm.OverrideMeasure, IsSelected = wm.IsSelected, Index = wm.Index })
                                                                  .ToList();
                    // newWebsite.Measures.ForEach(wd => wd.Id = 0);
                }

                return true;
            }
            catch (Exception exc)
            {
                logger.Write(exc.GetBaseException(), TraceEventType.Error);
                return false;
            }
        }

        private static bool ProcessReports(WebsiteImportTypeEnum importType, WebsiteExport websiteExport, Website existingWebsite,
                            Website newWebsite, IDomainSessionFactoryProvider provider, ILogWriter logger)
        {
            try
            {
                if (importType == WebsiteImportTypeEnum.File && websiteExport != null)
                {
                    var reports = new List<WebsiteReport>();

                    using (var session = provider.SessionFactory.OpenSession())
                    {
                        foreach (var wr in websiteExport.Reports.ToList())
                        {
                            var report = session.Query<Report>()
                                                .FirstOrDefault(r => r.Name.ToUpper() == wr.Name.ToUpper() &&
                                                                     r.ReportType.ToUpper() == wr.ReportType.ToUpper());

                            if (report != null)
                            {
                                reports.Add(new WebsiteReport
                                {
                                    Report = report,
                                    DefaultSelectedYear = wr.DefaultSelectedYear,
                                    SelectedYears = wr.SelectedYears,
                                });
                            }
                        }
                    }

                    if (reports.Any())
                    {
                        if (newWebsite.Reports == null) newWebsite.Reports = new List<WebsiteReport>();
                        //newWebsite.Reports = reports;

                        if (newWebsite.Datasets.Any())
                        {
                            var targetNames = newWebsite.Datasets.DistinctBy(d => d.Dataset.Id).Select(d => d.Dataset.ContentType.Name).ToList();
                            foreach (var report in reports.ToList())
                            {
                                if (!report.Report.Datasets.Any(ds => ds.In(targetNames))) continue;
                                newWebsite.Reports.Add(report);
                            }
                        }
                        else
                        {
                            newWebsite.Reports = new List<WebsiteReport>();
                        }
                    }
                }
                else if (importType == WebsiteImportTypeEnum.Existing && existingWebsite != null)
                {
                    newWebsite.Reports = existingWebsite.Reports.DistinctBy(d => d.Report.Id).Select(wr =>
                        new WebsiteReport
                        {
                            Report = wr.Report,
                            DefaultSelectedYear = wr.DefaultSelectedYear,
                            SelectedYears = wr.SelectedYears,
                            Index = wr.Index
                        }).ToList();

                    //newWebsite.Reports.ForEach(wd => wd.Id = 0);
                }

                return true;
            }
            catch (Exception exc)
            {
                logger.Write(exc.GetBaseException(), TraceEventType.Error);
                return false;
            }
        }
        #endregion

        #region Helper Methods

        private static WebsiteTheme GetWebsiteTheme(Audience audience, string selectedTheme, string accentColor, string selectedFont, string brandColor, MonahrqThemeElement theme)
        {
            return new WebsiteTheme
            {
                AudienceType = audience,
                SelectedTheme = selectedTheme ?? theme.Name,
                AccentColor = accentColor ?? theme.AccentColor,
                BrandColor = brandColor ?? theme.BrandColor,
                SelectedFont = selectedFont ?? "'Droid Sans', Arial, sans-serif;",
                BackgroundColor = theme.BackgroundColor,
                BodyTextColor = theme.BodyTextColor,
                LinkTextColor = theme.LinkTextColor
            };
        }



        #endregion
    }

    public enum WebsiteImportTypeEnum
    {
        Existing,
        File
    }

    public class ImportRequest
    {
        public IDomainSessionFactoryProvider Provider { get; set; }
        public ILogWriter Logger { get; set; }

        public WebsiteImportTypeEnum ImportType { get; set; }
        public int? ExistingWebsiteId { get; set; }
        public string WebsiteXmlFromFile { get; set; }
    }

    public class ImportResponse
    {
        public string Message { get; set; }
        public bool IsComplete { get; set; }
        public Exception Exception { get; set; }
        public bool HasErrors { get { return Exception != null; } }
        public Website Website { get; set; }
    }
}