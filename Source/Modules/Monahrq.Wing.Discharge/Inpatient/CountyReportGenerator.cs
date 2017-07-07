using System.Text;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Generators;

using Monahrq.Sdk.Services.Generators;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Monahrq.Infrastructure.Configuration;
using Microsoft.Practices.ServiceLocation;

namespace Monahrq.Wing.Discharge.Inpatient
{
	/// <summary>
	/// Generates the report data/.json files for County Discharge reports.
	/// </summary>
	/// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
	/// <seealso cref="Monahrq.Sdk.Generators.IReportGenerator" />
	[Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(	new string[] { "5AAF7FBA-7102-4C66-8598-A70597E2F825" },
						new[] { "County Report" },
						new[] { typeof(InpatientTarget) },
						12)]
    public class CountyReportGenerator : BaseReportGenerator, IReportGenerator
    {
		/// <summary>
		/// The json domain
		/// </summary>
		private const string jsonDomain = "$.monahrq.County=";
		/// <summary>
		/// Gets or sets the report identifier.
		/// </summary>
		/// <value>
		/// The report identifier.
		/// </value>
		private string ReportID { get; set; }

		/// <summary>
		/// Gets or sets the ip dataset i ds.
		/// </summary>
		/// <value>
		/// The ip dataset i ds.
		/// </value>
		private DataTable IPDatasetIDs { get; set; }

		/// <summary>
		/// Gets or sets the base data dir.
		/// </summary>
		/// <value>
		/// The base data dir.
		/// </value>
		private string BaseDataDir { get; set; }
		/// <summary>
		/// Gets or sets the utilization data dir.
		/// </summary>
		/// <value>
		/// The utilization data dir.
		/// </value>
		private string UtilizationDataDir { get; set; }
		/// <summary>
		/// Gets or sets the DRG data dir.
		/// </summary>
		/// <value>
		/// The DRG data dir.
		/// </value>
		private string DRGDataDir { get; set; }
		/// <summary>
		/// Gets or sets the DRG hospital counties data dir.
		/// </summary>
		/// <value>
		/// The DRG hospital counties data dir.
		/// </value>
		private string DRGHospitalCountiesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the MDC data dir.
		/// </summary>
		/// <value>
		/// The MDC data dir.
		/// </value>
		private string MDCDataDir { get; set; }
		/// <summary>
		/// Gets or sets the MDC hospital counties data dir.
		/// </summary>
		/// <value>
		/// The MDC hospital counties data dir.
		/// </value>
		private string MDCHospitalCountiesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS data dir.
		/// </summary>
		/// <value>
		/// The CCS data dir.
		/// </value>
		private string CCSDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS hospital counties data dir.
		/// </summary>
		/// <value>
		/// The CCS hospital counties data dir.
		/// </value>
		private string CCSHospitalCountiesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the PRCCS data dir.
		/// </summary>
		/// <value>
		/// The PRCCS data dir.
		/// </value>
		private string PRCCSDataDir { get; set; }
		/// <summary>
		/// Gets or sets the PRCCS hospital counties data dir.
		/// </summary>
		/// <value>
		/// The PRCCS hospital counties data dir.
		/// </value>
		private string PRCCSHospitalCountiesDataDir { get; set; }

		/// <summary>
		/// Gets or sets the DRG list.
		/// </summary>
		/// <value>
		/// The DRG list.
		/// </value>
		private DataTable DRGList { get; set; }
		/// <summary>
		/// Gets or sets the MDC list.
		/// </summary>
		/// <value>
		/// The MDC list.
		/// </value>
		private DataTable MDCList { get; set; }
		/// <summary>
		/// Gets or sets the CCS list.
		/// </summary>
		/// <value>
		/// The CCS list.
		/// </value>
		private DataTable CCSList { get; set; }
		/// <summary>
		/// Gets or sets the PRCCS list.
		/// </summary>
		/// <value>
		/// The PRCCS list.
		/// </value>
		private DataTable PRCCSList { get; set; }
		/// <summary>
		/// Gets or sets the hospital list.
		/// </summary>
		/// <value>
		/// The hospital list.
		/// </value>
		private DataTable HospitalList { get; set; }
		/// <summary>
		/// Gets or sets the county list.
		/// </summary>
		/// <value>
		/// The county list.
		/// </value>
		private DataTable CountyList { get; set; }
		/// <summary>
		/// Gets or sets the region list.
		/// </summary>
		/// <value>
		/// The region list.
		/// </value>
		private DataTable RegionList { get; set; }
		/// <summary>
		/// Gets or sets the hospital type list.
		/// </summary>
		/// <value>
		/// The hospital type list.
		/// </value>
		private DataTable HospitalTypeList { get; set; }


		#region SQLCode

		#region MeanSQL
		/// <summary>
		/// The mean SQL
		/// </summary>
		private const string MeanSql = @"
	        INSERT INTO Temp_UtilCounty_County
            SELECT      
		        '%%ReportID%%',
		        %%MeanSelect%%,
	            Temp_UtilCounty_Prep.UtilTypeID,
		        %%UtilID%% AS UtilID,
		        %%CatID%% AS CatID,
		        %%CatVal%% AS CatVal,

	            COUNT(*) AS Discharges,
                -1 AS RateDischarges,
	            AVG(Temp_UtilCounty_Prep.TotalCost) AS MeanCosts,
	            -1 AS MedianCosts
	        FROM Temp_UtilCounty_Prep
	        WHERE ID = '%%ReportID%%'
	        GROUP BY Temp_UtilCounty_Prep.UtilTypeID%%GroupBy%%;
        ";

		#endregion MeanSQL

		#region MedianSql
		/// <summary>
		/// The median SQL
		/// </summary>
		private const string MedianSql = @"
            WITH TotalRows AS
            (
	            SELECT UtilTypeID%%MedianRowsSelect%%, COUNT(*) AS TotalRows
	            FROM Temp_UtilCounty_Prep
	            WHERE Temp_UtilCounty_Prep.ID = '%%ReportID%%' AND TotalCost IS NOT NULL
	            GROUP BY UtilTypeID%%GroupBy%%
            ),
            Medians (UtilTypeID%%MedianGroupBy%%, MedianVal)
            AS
            (
	            SELECT TotalRows.UtilTypeID%%MedianMediansSelect%%, AVG(0.+TotalCost) AS MedianVal
	            FROM TotalRows CROSS APPLY
	            (
		            SELECT TOP(((TotalRows.TotalRows - 1) / 2) + (1 + (1 - TotalRows.TotalRows % 2)))
			            TotalCost,
			            ROW_NUMBER() OVER (ORDER BY TotalCost) AS RowNumber
			            FROM Temp_UtilCounty_Prep
			            WHERE Temp_UtilCounty_Prep.ID = '%%ReportID%%'
				            AND TotalRows.UtilTypeID = Temp_UtilCounty_Prep.UtilTypeID
				            %%MedianMediansWhere%%
				            AND TotalCost IS NOT NULL
			            ORDER BY TotalCost
	            ) Medians
	            WHERE RowNumber BETWEEN (((TotalRows.TotalRows - 1) / 2) + 1) AND
		            (((TotalRows.TotalRows - 1) / 2) + ( 1 + (1 - TotalRows.TotalRows % 2)))
	            GROUP BY UtilTypeID%%MedianGroupBy%%
            )
            UPDATE Temp_UtilCounty_County
            SET Temp_UtilCounty_County.MedianCosts = Medians.MedianVal
            FROM Temp_UtilCounty_County 
	            JOIN Medians ON Temp_UtilCounty_County.UtilTypeID = Medians.UtilTypeID %%MedianUpdateJoin%%;
        ";
		#endregion MedianSql

		#endregion SQLCode

		/// <summary>
		/// Initializes a new instance of the <see cref="CountyReportGenerator"/> class.
		/// </summary>
		public CountyReportGenerator()
            : base()
        {

        }


		/// <summary>
		/// Called when [installed].
		/// </summary>
		public void OnInstalled()
        {
            // The wing was just added to Monahrq, so start importing the needed base data.
            //base.OnWingAdded();

            // Load the .sql scripts and run them.
            string[] scriptFiles ={
                                        "Table-TempUtilCountyCounty.sql",
                                        "Table-TempUtilCountyPrep.sql",
                                        "Sproc-spUtilCountyAddRateDischargesToAllCombined.sql",
                                        "Sproc-spUtilCountyAddRateDischargesToCounty.sql",
                                        "Sproc-spUtilCountyGetRecordsInIPTarget.sql",
                                        "Sproc-spUtilCountyGetDetailData.sql",
                                        "Sproc-spUtilCountyGetSummaryDataByClinical.sql",
                                        "Sproc-spUtilCountyGetSummaryDataByGeo.sql",
                                        "Sproc-spUtilCountyInitializeDRG.sql",
                                        "Sproc-spUtilCountyInitializeDXCCS.sql",
                                        "Sproc-spUtilCountyInitializeMDC.sql",
                                        "Sproc-spUtilCountyInitializePRCCS.sql"
                                  };

            RunSqlScripts(Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\UtilizationCounty"), scriptFiles);
        }

		/// <summary>
		/// Initializes the generator.
		/// </summary>
		public override void InitGenerator()
        {
            // Following should only run once, but this procedure is running every time on application startup.
            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for County reports" });

            //OnInstalled();
        }

		/// <summary>
		/// Validates the dependencies.
		/// </summary>
		/// <param name="website">The website.</param>
		/// <param name="validationResults">The validation results.</param>
		/// <returns></returns>
		public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
        {
            return true;
        }

		/// <summary>
		/// Generates the report.
		/// </summary>
		/// <param name="website">The website.</param>
		/// <param name="publishTask">The publish task.</param>
		public override void GenerateReport(Website website, PublishTask publishTask)
        {


            CurrentWebsite = website;

            //foreach (WebsiteDataset dataSet in website.Datasets)
            //{
            //    switch (dataSet.Dataset.ContentType.Name)
            //    {
            //        case "Inpatient Discharge":
            //            ContentItemRecord = dataSet.Dataset.Id;
            //            break;
            //        default:
            //            break;
            //    }
            //}
			var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
			decimal suppression = GetSuppression("IP-08");
            var countyRateMeasure = CurrentWebsite.Measures.FirstOrDefault(m => m.ReportMeasure.Name.Equals("IP-11"));
            var scale = countyRateMeasure == null ? 1000 : countyRateMeasure.ReportMeasure.ScaleBy ?? 1000;

            string regionType = CurrentWebsite.RegionTypeContext;
            switch (CurrentWebsite.RegionTypeContext)
            {
                case "HealthReferralRegion":
                    regionType = "HealthReferralRegion_Id";
                    break;
                case "HospitalServiceArea":
                    regionType = "HospitalServiceArea_Id";
                    break;
                case "CustomRegion":
                    regionType = "CustomRegion_Id";
                    break;
            }

            var ipDatasets = website.Datasets.Where(d => d.Dataset.ContentType.Name == "Inpatient Discharge").ToList();
            foreach (var dataset in ipDatasets)
            {
                LogMessage(String.Format("Generating {0} Report Data for year {1}", "County", dataset.Dataset.ReportingYear));
                var contentItemRecord = dataset.Dataset.Id;
                try
                {
                    var process = new Process();
                    var psi = new ProcessStartInfo();
                    var fileName = @"Modules\Generators\CountyGenerator\CountyGenerator.exe";
                    if (!File.Exists(fileName)) return;
                    psi.FileName = fileName;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    psi.RedirectStandardOutput = true;
                    psi.Arguments = string.Format(
                        "-d \"{0}\" -c \"{1}\" -wid \"{2}\" -i {3} -s {4} -r {5} -l 1 -scale {6} -o {7}",
                        Path.Combine(website.OutPutDirectory, "Data", "County", dataset.Dataset.ReportingYear),
                        MonahrqConfiguration.SettingsGroup.MonahrqSettings().EntityConnectionSettings.ConnectionString,
                        CurrentWebsite.Id,
                        contentItemRecord,
                        suppression,
                        regionType,
                        scale,
                        (website.UtilizationReportCompression.HasValue && website.UtilizationReportCompression.Value));
                    process.StartInfo = psi;
                    process.Start();
                    do
                    {
                        var logMessage = process.StandardOutput.ReadLineAsync().Result;
                        if (!string.IsNullOrEmpty(logMessage))
                            LogMessage(logMessage);

                    } while (!process.HasExited);
                    process.Close();
                    process.Dispose();

                    var tempPath = Path.GetTempPath() + "Monahrq\\Generators\\CountyGenerator\\";
                    if (Directory.Exists(tempPath))
                        Directory.Delete(tempPath, true);
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return;

            // This is the one that should be called first.
            try
            {
                if (publishTask == PublishTask.PreviewOnly)
                {
                    // Do nothing for previews
                    return;
                }

                // Start the timer.
                DateTime groupStart = DateTime.Now;

                base.GenerateReport(website);

                // Initialize the data for this report.
                InitializeReportData();

                // Make sure the base directories are created.
                CreateBaseDirectories();

                // Generate paths
                GenerateDimensionPath("DRG");
                GenerateDimensionPath("MDC");
                GenerateDimensionPath("DXCCS");
                GenerateDimensionPath("PRCCS");

                // Write out the complete time for generation.
                Logger.Write(string.Format("Utilization County - Generation completed in {0:c}", DateTime.Now - groupStart));
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

		/// <summary>
		/// Initializes the report data.
		/// </summary>
		private void InitializeReportData()
        {
            try
            {
				var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
				#region Get base information about the website - hospitals, measures, datasets, etc.

                // Get the needed DataSets
                IPDatasetIDs = new DataTable();
                IPDatasetIDs.Columns.Add("ID", typeof(int));
                foreach (WebsiteDataset dataSet in CurrentWebsite.Datasets)
                {
                    switch (dataSet.Dataset.ContentType.Name)
                    {
                        case "Inpatient Discharge":
                            // Add a new IP dataset
                            IPDatasetIDs.Rows.Add((dataSet.Dataset.Id));
                            break;
                        default:
                            break;
                    }
                }

                #endregion Get base information about the website - hospitals, measures, datasets, etc.

                #region Generate the specific data for this report.

                // Save a report ID for this particular report run.
                ReportID = Guid.NewGuid().ToString();

                // Get the lists needed to generate the json files.
                DRGList = new DataTable();
                DRGList = RunSprocReturnDataTable("spGetDRG");

                MDCList = new DataTable();
                MDCList = RunSprocReturnDataTable("spGetMDC");

                CCSList = new DataTable();
                CCSList = RunSprocReturnDataTable("spGetDXCCS");

                PRCCSList = new DataTable();
                PRCCSList = RunSprocReturnDataTable("spGetPRCCS");

                HospitalList = new DataTable();
                HospitalList = RunSprocReturnDataTable("spGetHospitals",
                        new KeyValuePair<string, object>("@Hospitals", HospitalIds),
						new KeyValuePair<string, object>("@RegionType", configService.HospitalRegion.SelectedRegionType.Name));

                CountyList = new DataTable();
                CountyList = RunSprocReturnDataTable("spGetStateCounties", new KeyValuePair<string, object>("@States", StateIds));

                RegionList = new DataTable();
                RegionList = RunSprocReturnDataTable("spGetHospitalRegions",
                        new KeyValuePair<string, object>("@Hospitals", HospitalIds),
						new KeyValuePair<string, object>("@RegionType", configService.HospitalRegion.SelectedRegionType.Name));

                HospitalTypeList = new DataTable();
                HospitalTypeList = RunSprocReturnDataTable("spGetHospitalTypes", new KeyValuePair<string, object>("@Hospitals", HospitalIds));

                // Get the number of rows in the target tables
                DataTable IPRows = RunSprocReturnDataTable("spUtilCountyGetRecordsInIPTarget",
                        new KeyValuePair<string, object>("@IPDataset", IPDatasetIDs));
                if (IPRows.Rows.Count == 1)
                {
                    Logger.Write(string.Format("Utilization County - Rows in IP Dataset: {0:n0}", IPRows.Rows[0]["IPRows"]));
                }

                #endregion Generate the specific data for this report.

            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

		/// <summary>
		/// Generates the dimension path.
		/// </summary>
		/// <param name="section">The section.</param>
		private void GenerateDimensionPath(string section)
        {
            DateTime start = DateTime.Now;
            PrepData(section);
            AggregateData(section);
            SuppressData(section);
            GenerateJsonFiles(section);
            TimeSpan timeDiff = DateTime.Now - start;
            Logger.Write(string.Format("Utilization County - {0} dimension completed in {1:c}", section, timeDiff));
        }

		/// <summary>
		/// Preps the data.
		/// </summary>
		/// <param name="section">The section.</param>
		private void PrepData(string section)
        {
            // Add minimally needed data to prep table

            var initParams = new KeyValuePair<string, object>[] {
                            new KeyValuePair<string, object>("@ReportID", ReportID),
                            new KeyValuePair<string, object>("@ReportYear", CurrentWebsite.ReportedYear),
                            new KeyValuePair<string, object>("@IPDataset", IPDatasetIDs)
                        };

            EnableDisableTableIndexes(true, "Temp_UtilCounty_Prep");
            LogMessage(string.Format("Utilization County - Prepping {0} Data - Prepping data", section));
            RunSproc(string.Format("spUtilCountyInitialize{0}", section), string.Format("Utilization County - Prepping {0} Data - Prepping data", section), initParams);
            EnableDisableTableIndexes(false, "Temp_UtilCounty_Prep");
        }

		/// <summary>
		/// Aggregates the data.
		/// </summary>
		/// <param name="section">The section.</param>
		private void AggregateData(string section)
        {
            // Aggregate the data into the temp tables.

            DateTime start = DateTime.Now;
            string[] ipTables = { "Combined", "County" };
            string[] ipStrats = { "Combined", "Age", "Sex", "Race" };
            bool calcMedianCost = CurrentWebsite.Measures.Any(wm => wm.IsSelected && wm.ReportMeasure.Name.StartsWith("IP-10"));

            // Disable the temp table indexes.
            EnableDisableTableIndexes(true, "Temp_UtilCounty_County");

            foreach (var ipTable in ipTables)
            {
                foreach (var ipStrat in ipStrats)
                {
                    string finalMeanSql = "";
                    string finalMedianSql = "";
                    string meanSelect = "";
                    string medianRowsSelect = "";
                    string medianMediansSelect = "";
                    string medianMediansWhere = "";
                    string medianUpdateJoin = "";
                    string catId = "";
                    string catVal = "";
                    string groupBy = "";
                    string medianGroupBy = "";

                    switch (ipTable)
                    {
                        case "Combined":
                            meanSelect = "0";
                            medianUpdateJoin = "AND Temp_UtilCounty_County.CountyID = 0 ";
                            break;
                        case "County":
                            meanSelect = "Temp_UtilCounty_Prep.CountyID";
                            medianRowsSelect = ", CountyID";
                            medianMediansSelect = ", TotalRows.CountyID";
                            medianMediansWhere = "AND TotalRows.CountyID = Temp_UtilCounty_Prep.CountyID ";
                            medianUpdateJoin = "AND Temp_UtilCounty_County.CountyID = Medians.CountyID ";
                            groupBy = groupBy + ", Temp_UtilCounty_Prep.CountyID";
                            medianGroupBy = medianGroupBy + ", CountyID";
                            break;
                    }

                    switch (ipStrat)
                    {
                        case "Combined":
                            catId = "0";
                            break;
                        case "Age":
                            catId = "1";
                            break;
                        case "Sex":
                            catId = "2";
                            break;
                        case "Race":
                            catId = "4";
                            break;
                    }
                    catVal = (ipStrat == "Combined") ? "0" : "Temp_UtilCounty_Prep." + ipStrat;
                    if (ipStrat == "Combined")
                    {
                        medianUpdateJoin = medianUpdateJoin + "AND Temp_UtilCounty_County.CatID = 0 " +
                            "AND Temp_UtilCounty_County.CatVal = 0 ";
                    }
                    else
                    {
                        groupBy = groupBy + ", Temp_UtilCounty_Prep." + ipStrat;
                        medianRowsSelect = medianRowsSelect + ", " + ipStrat;
                        medianGroupBy = medianGroupBy + ", " + ipStrat;
                        medianMediansSelect = medianMediansSelect + ", TotalRows." + ipStrat;
                        medianMediansWhere = medianMediansWhere + "AND TotalRows." + ipStrat + " = Temp_UtilCounty_Prep." + ipStrat + " ";
                        medianUpdateJoin = medianUpdateJoin + "AND Temp_UtilCounty_County.CatID = " + catId + " " +
                            "AND Temp_UtilCounty_County.CatVal = Medians." + ipStrat + " ";
                    }

                    // Setup the message for this section
                    string message = string.Format("Utilization County - Aggregating {0} Data - ", section);
                    message = message + ((ipStrat == "Combined") ? "Summary data" : "Stratified data for " + ((ipStrat == "PrimaryPayer") ? "primary payer" : ipStrat.ToLower()));
                    message = message + ((ipTable == "Combined") ? " for all data combined" : " by " + ((ipTable == "HospitalType") ? "hospital type" : ipTable.ToLower()) + ".");
                    LogMessage(message);

                    // Setup the base sql for generating the means.
                    string baseMeanSql = MeanSql
                        .Replace("%%MeanSelect%%", meanSelect)
                        .Replace("%%CatID%%", catId)
                        .Replace("%%CatVal%%", catVal)
                        .Replace("%%ReportID%%", ReportID);

                    // Customize the base means sql for all util types combined.
                    finalMeanSql = baseMeanSql
                        .Replace("%%UtilID%%", "0")
                        .Replace("%%GroupBy%%", groupBy);

                    ExecuteNonQuery(finalMeanSql, string.Format("Utilization County - Aggregating {0} Data - UtilType {1} by {2}", section, ipTable, ipStrat));

                    if (calcMedianCost)
                    {
                        finalMedianSql = MedianSql
                            .Replace("%%ReportID%%", ReportID)
                            .Replace("%%MedianRowsSelect%%", medianRowsSelect)
                            .Replace("%%GroupBy%%", groupBy)
                            .Replace("%%MedianGroupBy%%", medianGroupBy)
                            .Replace("%%MedianMediansSelect%%", medianMediansSelect)
                            .Replace("%%MedianMediansWhere%%", medianMediansWhere)
                            .Replace("%%MedianUpdateJoin%%", medianUpdateJoin);
                        ExecuteNonQuery(finalMedianSql, string.Format("Utilization County - Aggregating {0} Data - Add median cost", section));
                    }

                    // Customize the base means sql for individual util types.
                    finalMeanSql = baseMeanSql
                        .Replace("%%UtilID%%", "Temp_UtilCounty_Prep.UtilID")
                        .Replace("%%GroupBy%%", ", Temp_UtilCounty_Prep.UtilID" + groupBy);
                    ExecuteNonQuery(finalMeanSql, string.Format("Utilization County - Aggregating {0} Data - UtilTypeUtil {1} by {2}", section, ipTable, ipStrat));

                    if (calcMedianCost)
                    {
                        finalMedianSql = MedianSql
                            .Replace("%%ReportID%%", ReportID)
                            .Replace("%%MedianRowsSelect%%", ", UtilID" + medianRowsSelect)
                            .Replace("%%GroupBy%%", ", UtilID" + groupBy)
                            .Replace("%%MedianGroupBy%%", ", UtilID" + medianGroupBy)
                            .Replace("%%MedianMediansSelect%%", ", TotalRows.UtilID" + medianMediansSelect)
                            .Replace("%%MedianMediansWhere%%", "AND TotalRows.UtilID = Temp_UtilCounty_Prep.UtilID " + medianMediansWhere)
                            .Replace("%%MedianUpdateJoin%%", "AND Temp_UtilCounty_County.UtilID = Medians.UtilID " + medianUpdateJoin);
                        ExecuteNonQuery(finalMedianSql, string.Format("Utilization County - Aggregating {0} Data - Add median cost", section));
                    }
                }
            }

            // Add the rate of discharge.
            LogMessage(string.Format("Utilization County - Aggregating {0} Data - Adding rate of discharge.", section));
            var countyRateMeasure = CurrentWebsite.Measures.Where(m => m.OriginalMeasure.Name.Equals("IP-11")).FirstOrNull();
            var scale =
                countyRateMeasure == null ? 1000 :
                    (((WebsiteMeasure)countyRateMeasure).OriginalMeasure.ScaleBy.HasValue ? ((WebsiteMeasure)countyRateMeasure).OriginalMeasure.ScaleBy.Value : 1000);
            var initParams = new KeyValuePair<string, object>[] {
                            new KeyValuePair<string, object>("@ReportID", ReportID),
                            // TODO: This is the right way I think
                            //new KeyValuePair<string, object>("@ReportYear", CurrentWebsite.ReportedYear),
                            new KeyValuePair<string, object>("@ReportYear", 2013),
                            new KeyValuePair<string, object>("@Scale", scale)
                        };
            RunSproc("spUtilCountyAddRateDischargesToCounty", string.Format("Utilization County - Aggregating {0} Data - Add rate of discharges to counties.", section), initParams);
            RunSproc("spUtilCountyAddRateDischargesToAllCombined", string.Format("Utilization County - Aggregating {0} Data - Add rate of discharges to all combined.", section), initParams);

            // Reenable the indexes.
            EnableDisableTableIndexes(false, "Temp_UtilCounty_County");

            // Clean up prep table.
            string sql = string.Format("DELETE FROM Temp_UtilCounty_Prep WHERE ID = '{0}'", ReportID);
            ExecuteNonQuery(sql, string.Format("Utilization County - Aggregating {0} Data - Removing any previous prep data.", section));

            TimeSpan timeDiff = DateTime.Now - start;
            Logger.Write(string.Format("Utilization County - Aggregating {0} Data - Aggregation completed in {1:c}", section, timeDiff));
        }

		/// <summary>
		/// Suppresses the data.
		/// </summary>
		/// <param name="section">The section.</param>
		private void SuppressData(string section)
        {
            DateTime start = DateTime.Now;
            LogMessage(string.Format("Utilization County - Suppressing {0} Data", section));

            // Suppress the main field.
            StringBuilder sql = new StringBuilder();
            sql.Append("UPDATE Temp_UtilCounty_County ");
            sql.Append("SET Discharges = -2, RateDischarges = -2, MeanCosts = -2, MedianCosts = -2 ");
            sql.Append("WHERE Discharges > 0 AND Discharges < " + GetSuppression("IP-08"));
            ExecuteNonQuery(sql.ToString(), string.Format("Utilization County - Suppressing {0} Data - Suppressing discharges", section));
            TimeSpan timeDiff = DateTime.Now - start;
            Logger.Write(string.Format("Utilization County - Suppressing {0} Data - Suppression completed in {1:c}", section, timeDiff));
        }

		/// <summary>
		/// Creates the base directories.
		/// </summary>
		private void CreateBaseDirectories()
        {
            try
            {
                BaseDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "Base");
                if (!Directory.Exists(BaseDataDir)) Directory.CreateDirectory(BaseDataDir);

                UtilizationDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "County");
                if (!Directory.Exists(UtilizationDataDir)) Directory.CreateDirectory(UtilizationDataDir);


                // DRG Directories
                DRGDataDir = Path.Combine(UtilizationDataDir, "DRG");
                if (!Directory.Exists(DRGDataDir)) Directory.CreateDirectory(BaseDataDir);


                DRGHospitalCountiesDataDir = Path.Combine(DRGDataDir, "County");
                if (!Directory.Exists(DRGHospitalCountiesDataDir)) Directory.CreateDirectory(DRGHospitalCountiesDataDir);


                // MDC Directories
                MDCDataDir = Path.Combine(UtilizationDataDir, "MDC");
                if (!Directory.Exists(MDCDataDir)) Directory.CreateDirectory(MDCDataDir);


                MDCHospitalCountiesDataDir = Path.Combine(MDCDataDir, "County");
                if (!Directory.Exists(MDCHospitalCountiesDataDir)) Directory.CreateDirectory(MDCHospitalCountiesDataDir);

                // CCS Directories
                CCSDataDir = Path.Combine(UtilizationDataDir, "CCS");
                if (!Directory.Exists(CCSDataDir)) Directory.CreateDirectory(CCSDataDir);


                CCSHospitalCountiesDataDir = Path.Combine(CCSDataDir, "County");
                if (!Directory.Exists(CCSHospitalCountiesDataDir)) Directory.CreateDirectory(CCSHospitalCountiesDataDir);

                // PRCCS Directories
                PRCCSDataDir = Path.Combine(UtilizationDataDir, "PRCCS");
                if (!Directory.Exists(PRCCSDataDir)) Directory.CreateDirectory(PRCCSDataDir);


                PRCCSHospitalCountiesDataDir = Path.Combine(PRCCSDataDir, "County");
                if (!Directory.Exists(PRCCSHospitalCountiesDataDir)) Directory.CreateDirectory(PRCCSHospitalCountiesDataDir);

            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

		/// <summary>
		/// Generates the json files.
		/// </summary>
		/// <param name="section">The section.</param>
		private void GenerateJsonFiles(string section)
        {
            try
            {
                // Setup the base parameters being passed in.
                KeyValuePair<string, object>[] baseOptions = new KeyValuePair<string, object>[] 
                {
                    new KeyValuePair<string, object>("@ReportID", ReportID),
                };

                string sectionStart = "Utilization County - Output {0} Data - Starting generation of output files by {1}";
                string sectionEnd = "Utilization County - Output {0} Data - Generated output files by {1} in {2:c}";

                // Reset the timer in the base file that tracks the json conversion and file write time.
                FileIOTime = TimeSpan.Zero;

                TimeSpan elapsedTime;
                DateTime groupStartAll = DateTime.Now;

                // Generate the main json files

                #region DRG Output
                // Output the two DRG sections
                if (section == "DRG")
                {
                    LogMessage(string.Format(sectionStart, "DRG", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(DRGDataDir, "DRG/DRG_", jsonDomain, "spUtilCountyGetSummaryDataByClinical", "spCountyGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 1)), DRGList, "@UtilID", "Id", CountyList, "@CountyID", "CountyID");
                    Logger.Write(string.Format(sectionEnd, "DRG", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "DRG", "county"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(DRGHospitalCountiesDataDir, "County_", jsonDomain, "spUtilCountyGetSummaryDataByGeo", "spCountyGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 1)), CountyList, "@CountyID", "CountyID", DRGList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "DRG", "clinical dimension", elapsedTime));
                }
                #endregion DRG Output

                #region MDC Output
                // Output the two MDC sections
                if (section == "MDC")
                {
                    LogMessage(string.Format(sectionStart, "MDC", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(MDCDataDir, "MDC/MDC_", jsonDomain, "spUtilCountyGetSummaryDataByClinical", "spCountyGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 2)), MDCList, "@UtilID", "Id", CountyList, "@CountyID", "CountyID");
                    Logger.Write(string.Format(sectionEnd, "MDC", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "MDC", "county"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(MDCHospitalCountiesDataDir, "County_", jsonDomain, "spUtilCountyGetSummaryDataByGeo", "spCountyGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 2)), CountyList, "@CountyID", "CountyID", MDCList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "MDC", "clinical dimension", elapsedTime));
                }
                #endregion MDC Output

                #region CCS Output
                // Output the two CCS sections
                if (section == "DXCCS")
                {
                    LogMessage(string.Format(sectionStart, "DXCCS", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(CCSDataDir, "CCS/CCS_", jsonDomain, "spUtilCountyGetSummaryDataByClinical", "spCountyGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 3)), CCSList, "@UtilID", "Id", CountyList, "@CountyID", "CountyID");
                    Logger.Write(string.Format(sectionEnd, "DXCCS", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "DXCCS", "county"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(CCSHospitalCountiesDataDir, "County_", jsonDomain, "spUtilCountyGetSummaryDataByGeo", "spCountyGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 3)), CountyList, "@CountyID", "CountyID", CCSList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "DXCCS", "clinical dimension", elapsedTime));
                }
                #endregion CCS Output

                #region PRCCS Output
                // Output the two PRCCS sections
                if (section == "PRCCS")
                {
                    LogMessage(string.Format(sectionStart, "PRCCS", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(PRCCSDataDir, "PRCCS/PRCCS_", jsonDomain, "spUtilCountyGetSummaryDataByClinical", "spCountyGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 4)), PRCCSList, "@UtilID", "Id", CountyList, "@CountyID", "CountyID");
                    Logger.Write(string.Format(sectionEnd, "PRCCS", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "PRCCS", "county"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(PRCCSHospitalCountiesDataDir, "County_", jsonDomain, "spUtilCountyGetSummaryDataByGeo", "spCountyGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 4)), CountyList, "@CountyID", "CountyID", PRCCSList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "PRCCS", "clinical dimension", elapsedTime));
                }
                #endregion PRCCS Output

                // Log the total time.
                TimeSpan groupTimeDiffAll = DateTime.Now - groupStartAll;
                Logger.Write(string.Format("Utilization County - Output {0} Data - Generated output files in {1:c}", section, groupTimeDiffAll));
                Logger.Write(string.Format("Utilization County - Output {0} Data - Total file IO time was {1:c}", section, FileIOTime));
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

		/// <summary>
		/// Loads the report data.
		/// </summary>
		/// <returns></returns>
		protected override bool LoadReportData()
        {
            return true;
        }

		/// <summary>
		/// Outputs the data files.
		/// </summary>
		/// <returns></returns>
		protected override bool OutputDataFiles()
        {
            return true;
        }
    }
}
