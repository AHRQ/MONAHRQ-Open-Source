using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Sdk.Generators;
using System.IO;
using System.Text;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Sdk.Extensions;

namespace Monahrq.Wing.Dynamic
{
    /// <summary>
    /// Dynamic Report Generator class for generating reports
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
    [Export("DynamicReportGenerator", typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(new[] { "7AF51434-5745-4538-B972-193F58E737D7" },
                     new[] { "Medicare Provider Charge Data" },
					 new[] { typeof(object) },
					 1)]
    public class DynamicReportGenerator : BaseReportGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicReportGenerator"/> class.
        /// </summary>
        public DynamicReportGenerator()
        {}

        /// <summary>
        /// Loads the report data needed for the data output.
        /// </summary>
        /// <returns></returns>
        protected override bool LoadReportData()
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                Logger.Write(ex.InnerException ?? ex);
                return false;
            }
        }

        /// <summary>
        /// Outputs the report data files.
        /// </summary>
        /// <returns></returns>
        protected override bool OutputDataFiles()
        {
            try{
                if (ActiveReport != null)
                {
                    var dataset = CurrentWebsite.Datasets.FirstOrDefault(wd => wd.Dataset.ContentType.IsCustom && 
                                                                         wd.Dataset.ContentType.Name.In(ActiveReport.Datasets));
                    //var report = CurrentWebsite.Reports.FirstOrDefault(wr => wr.Report.ReportType.EqualsIgnoreCase(ActiveReport.ReportType));

                    if (dataset == null) return true;

                    var reportQuery = ParseTokens(ActiveReport.ReportOutputSql, dataset.Dataset); // parse tokens in ReportOutputSql statement

                    var results = RunSqlReturnDataTable(reportQuery, null); // Execute query and return results

                    // Format Json/JavaScript namespace for report output
                    var jsonNamespace = !string.IsNullOrEmpty(ActiveReport.OutputJsNamespace)
                                                    ? ActiveReport.OutputJsNamespace.ToLower().Trim()
                                                    : string.Format("$.monahrq.Flutters.{0}.Report.Data=", dataset.Dataset.ContentType.Name.Replace(" ", null)).ToLower();

                    if (!jsonNamespace.StartsWith("$."))
                        jsonNamespace = string.Format("$.{0}", jsonNamespace);

                    if (!jsonNamespace.EndsWith("="))
                        jsonNamespace = string.Format("{0}=", jsonNamespace);

                    var jsonNamespaceMapping = CreateNameSpaceMapping(jsonNamespace);

                    // Format Json/JavaScript file name for report output
                    var reportFileName = !string.IsNullOrEmpty(ActiveReport.OutputFileName)
                                                    ? ActiveReport.OutputFileName.ToLower().Trim()
                                                    : "report-data.js";

                    if (!reportFileName.EndsWith(".js"))
                        reportFileName = string.Format("{0}.js", reportFileName);

                    // Create Wing Output Directory
                    var wingsDirectory = Path.Combine(BaseDataDirectoryPath, "Wings");
                    var fileDirectoryPath = Path.Combine(wingsDirectory, dataset.Dataset.ContentType.Name.ToLower().Replace(" ", null)); 

                    if (!Directory.Exists(fileDirectoryPath)) // If doesn't exist then create it.
                        Directory.CreateDirectory(fileDirectoryPath);

                    // Create/Generate File
                    GenerateJsonFile(results, Path.Combine(fileDirectoryPath, reportFileName), jsonNamespace, true, jsonNamespaceMapping);


                    // Generating Measures
                    var measuresToUse = CurrentWebsite.Measures.Where(wm => wm.ReportMeasure.Owner != null && 
                                                                            wm.ReportMeasure.Owner.Name.EqualsIgnoreCase(dataset.Dataset.ContentType.Name))
                                                               .Select(wm => wm.ReportMeasure)
                                                               .ToList();

                    if(measuresToUse.Any())
                    {
                        var measuresDirectory = Path.Combine(wingsDirectory, "Measures");

                        if (!Directory.Exists(measuresDirectory)) // If doesn't exist then create it.
                            Directory.CreateDirectory(measuresDirectory);

                        foreach(var measure in measuresToUse)
                        {
                            var measureJsonNamespace = string.Format("$.monahrq.wing.measuredescription_{0}=", measure.Id);
                            var measureFileName = string.Format("Measure_{0}.js", measure.Id);
                            var measureDto = DynamicMeasureDto.Create(measure);
                            GenerateJsonFile(measureDto, Path.Combine(measuresDirectory, measureFileName), measureJsonNamespace, true);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Write(ex.InnerException ?? ex);
                return false;
            }
        }

        /// <summary>
        /// Creates the name space mapping.
        /// </summary>
        /// <param name="jsonNamespace">The json namespace.</param>
        /// <returns></returns>
        private static string CreateNameSpaceMapping(string jsonNamespace)
        {
            if (string.IsNullOrEmpty(jsonNamespace)) return null;

            var temp = jsonNamespace;

            var mappingBuilder = new StringBuilder();

            if (temp.TrimEnd().EndsWith("="))
                temp = temp.SubStrBeforeLast("=");

            var namespaceItems = temp.Split('.').ToList();

            for (var i = 1; i < namespaceItems.Count; i++)
            {
                var curNamespace = String.Join(".", namespaceItems.StringSubList(i + 1));
                mappingBuilder.AppendLine(curNamespace + " = " + curNamespace + " || {};");
            }

            return mappingBuilder.ToString();
        }

        /// <summary>
        /// Parses any of the possible tokens in the wing report output sql statement.
        /// </summary>
        /// <param name="sqlToParse">The SQL to parse.</param>
        /// <param name="dataset">The dataset.</param>
        /// <returns>The processed the wing report output sql statement.</returns>
        private string ParseTokens(string sqlToParse, Dataset dataset)
        {
            // WEBSITE ID
            if (sqlToParse.ContainsIgnoreCase("[@@WEBSITE_ID@@]"))
                sqlToParse = sqlToParse.Replace("[@@WEBSITE_ID@@]", CurrentWebsite.Id.ToString());
            // REGIONAL CONTEXT
            if (sqlToParse.ContainsIgnoreCase("[@@WEBSITE_REGIONCONTEXT@@]"))
                sqlToParse = sqlToParse.Replace("[@@WEBSITE_REGIONCONTEXT@@]", CurrentWebsite.RegionTypeContext);
            // REPORTING YEAR
            if (sqlToParse.ContainsIgnoreCase("[@@REPORTING_YRS@@]"))
                sqlToParse = sqlToParse.Replace("[@@REPORTING_YRS@@]", CurrentWebsite.ReportedYear);
            // DATASET ID
            if (sqlToParse.ContainsIgnoreCase("[@@DATASET_ID@@]"))
                sqlToParse = sqlToParse.Replace("[@@DATASET_ID@@]", dataset.Id.ToString());
            // STATES
            if (sqlToParse.ContainsIgnoreCase("[@@REPORTING_STATES@@]"))
            {
                //var states = string.Join("','", CurrentWebsite.SelectedReportingStates);
                var states = CurrentWebsite.SelectedReportingStates.ToList().Aggregate(string.Empty, (current, state) => current + ("'" + state + "',"));

                if (states.EndsWith(","))
                    states = states.SubStrBeforeLast(",");

                sqlToParse = sqlToParse.Replace("[@@REPORTING_STATES@@]", states);
            }
            // HOSPITALS
            if (sqlToParse.ContainsIgnoreCase("[@@REPORTING_HOSPITAL_IDS@@]"))
            {
                //var ids = string.Join(",", CurrentWebsite.Hospitals.ToList().Select(wh => wh.Hospital.Id));
                var ids = CurrentWebsite.Hospitals.ToList().Aggregate(string.Empty, (current, wh) => current + ("" + wh.Hospital.Id + ","));

                if (ids.EndsWith(","))
                    ids = ids.SubStrBeforeLast(",");

                sqlToParse = sqlToParse.Replace("[@@REPORTING_HOSPITAL_IDS@@]", ids);
            }
            if (sqlToParse.ContainsIgnoreCase("[@@REPORTING_HOSPITAL_PROVIDERIDS@@]"))
            {
                //var providerIds = string.Join("','", CurrentWebsite.Hospitals.ToList().Select(wh => wh.Hospital.CmsProviderID));
                var providerIds = CurrentWebsite.Hospitals.ToList().Aggregate(string.Empty, (current, wh) => current + ("'" + wh.Hospital.CmsProviderID + "',"));

                if (providerIds.EndsWith(","))
                    providerIds = providerIds.SubStrBeforeLast(","); 

                sqlToParse = sqlToParse.Replace("[@@REPORTING_HOSPITAL_PROVIDERIDS@@]", providerIds);
            }
            // NURSING HOMES
            if (sqlToParse.ContainsIgnoreCase("[@@REPORTING_NURSINGHOME_IDS@@]"))
            {
                //var ids = string.Join(",", CurrentWebsite.NursingHomes.ToList().Select(nh => nh.NursingHome.Id));
                var ids = CurrentWebsite.NursingHomes.ToList().Aggregate(string.Empty, (current, nh) => current + ("" + nh.NursingHome.Id + ","));

                if (ids.EndsWith(","))
                    ids = ids.SubStrBeforeLast(",");

                sqlToParse = sqlToParse.Replace("[@@REPORTING_NURSINGHOME_IDS@@]", ids);
            }
            if (sqlToParse.ContainsIgnoreCase("[@@REPORTING_NURSINGHOME_PROVIDERIDS@@]"))
            {
                //var providerIds = string.Join("','", CurrentWebsite.NursingHomes.ToList().Select(nh => nh.NursingHome.ProviderId));
                var providerIds = CurrentWebsite.NursingHomes.ToList().Aggregate(string.Empty, (current, nh) => current + ("'" + nh.NursingHome.ProviderId + "',"));

                if (providerIds.EndsWith(","))
                    providerIds = providerIds.SubStrBeforeLast(",");

                sqlToParse = sqlToParse.Replace("[@@REPORTING_NURSINGHOME_PROVIDERIDS@@]", providerIds);
            } 

            // Measures
            var measuresToUse = CurrentWebsite.Measures.Where(wm => wm.ReportMeasure.Owner != null && wm.ReportMeasure.Owner.Name.EqualsIgnoreCase(dataset.ContentType.Name));
            if (sqlToParse.ContainsIgnoreCase("[@@REPORTING_MEASURES_CODES@@]"))
            {
                var measures = measuresToUse.ToList().Aggregate(string.Empty, (current, wm) => current + ("'" + wm.ReportMeasure.Name + "',"));

                if (measures.EndsWith(","))
                    measures = measures.SubStrBeforeLast(",");

                sqlToParse = sqlToParse.Replace("[@@REPORTING_MEASURES_CODES@@]", measures);
            }
            if (sqlToParse.ContainsIgnoreCase("[@@REPORTING_MEASURES_IDS@@]"))
            {
                var ids = measuresToUse.ToList().Aggregate(string.Empty, (current, wm) => current + ("" + wm.ReportMeasure.Id + ","));

                if (ids.EndsWith(","))
                    ids = ids.SubStrBeforeLast(",");

                sqlToParse = sqlToParse.Replace("[@@REPORTING_MEASURES_IDS@@]", ids);
            }
            return sqlToParse;
        }

        /// <summary>
        /// Initializes any data objects needed by the report generator while executing. This method is call during the application initialization/bootstrap
        /// </summary>
        public override void InitGenerator()
        {
            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for open source wing reports" });
        }

        /// <summary>
        /// Validates any dependencies that this report generator may need to execute.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="validationResults">The validation results.</param>
        /// <returns></returns>
        public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
        {
            var result = false;
            var dataSetName = string.Empty;
            if (base.ValidateDependencies(website, validationResults))
            {
                foreach (var dataset in CurrentWebsite.Datasets.Where(wd => wd.Dataset.ContentType.IsCustom).Select(wd => wd.Dataset.ContentType.Name).ToList())
                {
                    dataSetName = dataset;
                    if(ActiveReport.Datasets.Contains(dataset))
                    {
                        result = true;
                        break;
                    }
                }

                if(!result)
                {
                    var validationMessage = string.Format("Report \"{0}\" could not be generated because the dataset \"{1}\" was not included with website \"{2}\".",
                                                          ActiveReport.Name, dataSetName, CurrentWebsite.Name);
                    validationResults.Add(new ValidationResult(validationMessage));
                }
            }

            return validationResults == null || validationResults.Count == 0;
        }
    }

    #region DTO Objects

    #endregion
}
