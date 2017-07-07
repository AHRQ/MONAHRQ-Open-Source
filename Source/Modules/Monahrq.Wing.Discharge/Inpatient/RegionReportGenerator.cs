using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Generators;
using NHibernate.Util;
using Monahrq.Sdk.Services.Generators;

using Monahrq.Infrastructure.Configuration;
using System.Diagnostics;
using Microsoft.Practices.ServiceLocation;

namespace Monahrq.Wing.Discharge.Inpatient
{
	/// <summary>
	/// Generates the report data/.json files for Region Discharge reports
	/// </summary>
	/// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
	[Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(
		new[] { "3A40CF6B-37AD-4861-B272-930DDF2B8802" },
		new[] { "Region Report" },
		new[] { typeof(InpatientTarget) },
		13)]
    public class RegionReportGenerator : BaseReportGenerator
    {
		//ConnectionStringSettings ConnectionSettings { get; set; }

		/// <summary>
		/// The json domain
		/// </summary>
		private const string jsonDomain = "$.monahrq.Region=";
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
		/// Gets or sets the DRG hospital regions data dir.
		/// </summary>
		/// <value>
		/// The DRG hospital regions data dir.
		/// </value>
		private string DRGHospitalRegionsDataDir { get; set; }
		/// <summary>
		/// Gets or sets the MDC data dir.
		/// </summary>
		/// <value>
		/// The MDC data dir.
		/// </value>
		private string MDCDataDir { get; set; }
		/// <summary>
		/// Gets or sets the MDC hospital regions data dir.
		/// </summary>
		/// <value>
		/// The MDC hospital regions data dir.
		/// </value>
		private string MDCHospitalRegionsDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS data dir.
		/// </summary>
		/// <value>
		/// The CCS data dir.
		/// </value>
		private string CCSDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS hospital regions data dir.
		/// </summary>
		/// <value>
		/// The CCS hospital regions data dir.
		/// </value>
		private string CCSHospitalRegionsDataDir { get; set; }
		/// <summary>
		/// Gets or sets the PRCCS data dir.
		/// </summary>
		/// <value>
		/// The PRCCS data dir.
		/// </value>
		private string PRCCSDataDir { get; set; }
		/// <summary>
		/// Gets or sets the PRCCS hospital regions data dir.
		/// </summary>
		/// <value>
		/// The PRCCS hospital regions data dir.
		/// </value>
		private string PRCCSHospitalRegionsDataDir { get; set; }

		/// <summary>
		/// The DRG list
		/// </summary>
		private DataTable DRGList;
		/// <summary>
		/// The MDC list
		/// </summary>
		private DataTable MDCList;
		/// <summary>
		/// The CCS list
		/// </summary>
		private DataTable CCSList;
		/// <summary>
		/// The PRCCS list
		/// </summary>
		private DataTable PRCCSList;
		/// <summary>
		/// The region list
		/// </summary>
		private DataTable RegionList;

		/// <summary>
		/// Initializes a new instance of the <see cref="RegionReportGenerator"/> class.
		/// </summary>
		public RegionReportGenerator()
            : base()
        {

        }

		#region SQLCode

		#region MeanSQL
		/// <summary>
		/// The mean SQL
		/// </summary>
		private const string MeanSql = @"
	        INSERT INTO Temp_UtilRegion_Region
            SELECT      
		        '%%ReportID%%',
		        %%MeanSelect%%,
	            Temp_UtilRegion_Prep.UtilTypeID,
		        %%UtilID%% AS UtilID,
		        %%CatID%% AS CatID,
		        %%CatVal%% AS CatVal,

	            COUNT(*) AS Discharges,
                -1 AS RateDischarges,
	            AVG(Temp_UtilRegion_Prep.TotalCharge) AS MeanCharges,
	            AVG(Temp_UtilRegion_Prep.TotalCost) AS MeanCosts,
	            AVG(Temp_UtilRegion_Prep.LengthOfStay) AS MeanLOS,
	            -1 AS MedianCharges,
	            -1 AS MedianCosts,
	            -1 AS MedianLOS
	        FROM Temp_UtilRegion_Prep
	        WHERE ID = '%%ReportID%%'
	        GROUP BY Temp_UtilRegion_Prep.UtilTypeID%%GroupBy%%;
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
	            FROM Temp_UtilRegion_Prep
	            WHERE Temp_UtilRegion_Prep.ID = '%%ReportID%%' AND %%SourceMedianField%% IS NOT NULL
	            GROUP BY UtilTypeID%%GroupBy%%
            ),
            Medians (UtilTypeID%%MedianGroupBy%%, MedianVal)
            AS
            (
	            SELECT TotalRows.UtilTypeID%%MedianMediansSelect%%, AVG(0.+%%SourceMedianField%%) AS MedianVal
	            FROM TotalRows CROSS APPLY
	            (
		            SELECT TOP(((TotalRows.TotalRows - 1) / 2) + (1 + (1 - TotalRows.TotalRows % 2)))
			            %%SourceMedianField%%,
			            ROW_NUMBER() OVER (ORDER BY %%SourceMedianField%%) AS RowNumber
			            FROM Temp_UtilRegion_Prep
			            WHERE Temp_UtilRegion_Prep.ID = '%%ReportID%%'
				            AND TotalRows.UtilTypeID = Temp_UtilRegion_Prep.UtilTypeID
				            %%MedianMediansWhere%%
				            AND %%SourceMedianField%% IS NOT NULL
			            ORDER BY %%SourceMedianField%%
	            ) Medians
	            WHERE RowNumber BETWEEN (((TotalRows.TotalRows - 1) / 2) + 1) AND
		            (((TotalRows.TotalRows - 1) / 2) + ( 1 + (1 - TotalRows.TotalRows % 2)))
	            GROUP BY UtilTypeID%%MedianGroupBy%%
            )
            UPDATE Temp_UtilRegion_Region
            SET %%DestMedianField%% = Medians.MedianVal
            FROM Temp_UtilRegion_Region 
	            JOIN Medians ON Temp_UtilRegion_Region.UtilTypeID = Medians.UtilTypeID %%MedianUpdateJoin%%;
        ";
		#endregion MedianSql

		#endregion SQLCode

		/// <summary>
		/// Called when [installed].
		/// </summary>
		public void OnInstalled()
        {
            // The wing was just added to Monahrq, so start importing the needed base data.
            //base.OnWingAdded();

            // Load the .sql scripts and run them.
            string[] scriptFiles ={
                                        "Table-TempUtilRegionRegion.sql",
                                        "Table-TempUtilRegionPrep.sql",

                                        "Sproc-spUtilRegionGetTargetRegions.sql",
                                        "Sproc-spUtilRegionGetRecordsInIPTarget.sql",
                                        
                                        "Sproc-spUtilRegionAddRateDischargesToAllCombined.sql",
                                        "Sproc-spUtilRegionAddRateDischargesToRegion.sql",
                                        
                                        "Sproc-spUtilRegionGetDetailData.sql",
                                        "Sproc-spUtilRegionGetSummaryDataByClinical.sql",
                                        "Sproc-spUtilRegionGetSummaryDataByGeo.sql",
                                        
                                        "Sproc-spUtilRegionInitializeDRG.sql",
                                        "Sproc-spUtilRegionInitializeDXCCS.sql",
                                        "Sproc-spUtilRegionInitializeMDC.sql",
                                        "Sproc-spUtilRegionInitializePRCCS.sql"
                                  };

            RunSqlScripts(Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\UtilizationRegion"), scriptFiles);
        }

		/// <summary>
		/// Initializes the generator.
		/// </summary>
		public override void InitGenerator()
        {
            // Following should only run once, but this procedure is running every time on application startup.
            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for Region reports" });

            OnInstalled();
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
            //base.GenerateReport(website);
            if (publishTask == PublishTask.PreviewOnly)
            {
                // Do nothing for previews
                return;
            }

			//var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
			CurrentWebsite = website;

            //foreach (WebsiteDataset dataSet in website.Datasets)
            //{
            //    switch (dataSet.Dataset.ContentType.Name)
            //    {
            //        case "Inpatient Discharge":
            //            datasetRecord = dataSet.Dataset.Id;
            //            break;
            //        default:
            //            break;
            //    }
            //}
            decimal suppression = GetSuppression("IP-12");
            var regionRateMeasure = CurrentWebsite.Measures.Where(m => m.ReportMeasure.Name.Equals("IP-15")).FirstOrNull();
            var scale = regionRateMeasure == null ? 1000 : (((WebsiteMeasure) regionRateMeasure).ReportMeasure.ScaleBy ?? 1000);

			//string regionType = configService.HospitalRegion.SelectedRegionType.Name;
            string regionSource = "";
			switch (CurrentWebsite.RegionTypeContext)
            {
                case "HealthReferralRegion":
                    //regionType = "HealthReferralRegion_Id";
                    regionSource = "HRRRegionID";
                    break;
                case "HospitalServiceArea":
                    //regionType = "HospitalServiceArea_Id";
                    regionSource = "HSARegionID";
                    break;
                case "CustomRegion":
                    //regionType = "CustomRegion_Id";
                    regionSource = "CustomRegionID";
                    break;
            }

            var ipDatasets = website.Datasets.Where(d => d.Dataset.ContentType.Name == "Inpatient Discharge");
            foreach (var dataset in ipDatasets)
            {
                LogMessage(String.Format("Generating {0} Report Data for year {1}", "Region", dataset.Dataset.ReportingYear));
                var datasetRecord = dataset.Dataset.Id;

                try
                {
                    var process = new Process();
                    var psi = new ProcessStartInfo();
                    const string fileName = @"Modules\Generators\RegionGenerator\RegionGenerator.exe";
                    if (!File.Exists(fileName)) return;
                    psi.FileName = fileName;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    psi.RedirectStandardOutput = true;
                    psi.Arguments = string.Format(
                        "-d \"{0}\" -c \"{1}\" -wid \"{2}\" -i {3} -s {4} -r {5} -l 1 -scale {6} -y {7} -dy {8} -o {9}",
                        Path.Combine(website.OutPutDirectory, "Data", "Region", dataset.Dataset.ReportingYear),
                        MonahrqConfiguration.SettingsGroup.MonahrqSettings().EntityConnectionSettings.ConnectionString,
                        CurrentWebsite.Id,
                        datasetRecord,
                        suppression,
                        regionSource,
                        scale,
                        CurrentWebsite.ReportedYear,
                        dataset.Dataset.ReportingYear,
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

                    var tempPath = Path.GetTempPath() + "Monahrq\\Generators\\RegionGenerator\\";
                    if (Directory.Exists(tempPath))
                        Directory.Delete(tempPath, true);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            //return;
            // This is the one that should be called first.
            //try
            //{
            //    if (publishTask == PublishTask.PreviewOnly)
            //    {
            //        // Do nothing for previews
            //        return;
            //    }

            //    // Start the timer.
            //    DateTime groupStart = DateTime.Now;

            //    base.GenerateReport(website);

            //    // Initialize the data for this report.
            //    InitializeReportData();

            //    // Make sure the base directories are created.
            //    CreateBaseDirectories();

            //    // Generate the json files for the report.

            //    // Generate paths
            //    GenerateDimensionPath("DRG");
            //    GenerateDimensionPath("MDC");
            //    GenerateDimensionPath("DXCCS");
            //    GenerateDimensionPath("PRCCS");

            //    // Write out the complete time for generation.
            //    Logger.Write(string.Format("Utilization Region - Generation completed in {0:c}", DateTime.Now - groupStart));
            //}
            //catch (Exception ex)
            //{
            //    Logger.Write(ex);
            //}
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

                RegionList = new DataTable();
                RegionList = RunSprocReturnDataTable("spUtilRegionGetTargetRegions",
                        new KeyValuePair<string, object>("@IPDataset", IPDatasetIDs),
						new KeyValuePair<string, object>("@RegionType", configService.HospitalRegion.SelectedRegionType.Name)
                    );

                // Get the number of rows in the target tables
                DataTable IPRows = RunSprocReturnDataTable("spUtilRegionGetRecordsInIPTarget",
                        new KeyValuePair<string, object>("@IPDataset", IPDatasetIDs));
                if (IPRows.Rows.Count == 1)
                {
                    Logger.Write(string.Format("Utilization Region - Rows in IP Dataset: {0:n0}", IPRows.Rows[0]["IPRows"]));
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
            Logger.Write(string.Format("Utilization Region - {0} dimension completed in {1:c}", section, timeDiff));
        }

		/// <summary>
		/// Preps the data.
		/// </summary>
		/// <param name="section">The section.</param>
		private void PrepData(string section)
        {
            // Add minimally needed data to prep table

            var countyRateMeasure = CurrentWebsite.Measures.Where(m => m.ReportMeasure.Name.Equals("IP-11")).FirstOrNull();
            var scale =
                countyRateMeasure == null ? 1 :
                    (((WebsiteMeasure)countyRateMeasure).ReportMeasure.ScaleBy.HasValue ? ((WebsiteMeasure)countyRateMeasure).ReportMeasure.ScaleBy.Value : 1);

            var initParams = new KeyValuePair<string, object>[] {
                            new KeyValuePair<string, object>("@ReportID", ReportID),
                            new KeyValuePair<string, object>("@RegionType", CurrentWebsite.RegionTypeContext),
                            new KeyValuePair<string, object>("@ReportYear", CurrentWebsite.ReportedYear),
                            new KeyValuePair<string, object>("@IPDataset", IPDatasetIDs)
                        };

            EnableDisableTableIndexes(true, "Temp_UtilRegion_Prep");
            LogMessage(string.Format("Utilization Region - Prepping {0} Data - Prepping data", section));
            RunSproc(string.Format("spUtilRegionInitialize{0}", section), string.Format("Utilization Region - Prepping {0} Data - Prepping data", section), initParams);
            EnableDisableTableIndexes(false, "Temp_UtilRegion_Prep");
        }

		/// <summary>
		/// Aggregates the data.
		/// </summary>
		/// <param name="section">The section.</param>
		private void AggregateData(string section)
        {
            // Aggregate the data into the temp tables.

            DateTime start = DateTime.Now;
            string[] ipTables = { "Combined", "Region" };
            string[] ipStrats = { "Combined", "Age", "Sex", "Race" };
            bool calcMedianCost = CurrentWebsite.Measures.Any(wm => wm.IsSelected && wm.ReportMeasure.Name.StartsWith("IP-14"));
            bool calcMedianCharge = CurrentWebsite.Measures.Any(wm => wm.IsSelected && wm.ReportMeasure.Name.StartsWith("IP-14"));
            bool calcMedianLos = CurrentWebsite.Measures.Any(wm => wm.IsSelected && wm.ReportMeasure.Name.StartsWith("IP-14"));

            // Disable the temp table indexes.
            EnableDisableTableIndexes(true, "Temp_UtilRegion_Region");

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
                    string chargeMedianSql = "";
                    string costMedianSql = "";
                    string losMedianSql = "";


                    switch (ipTable)
                    {
                        case "Combined":
                            meanSelect = "0";
                            medianUpdateJoin = "AND Temp_UtilRegion_Region.RegionID = 0 ";
                            break;
                        case "Region":
                            meanSelect = "Temp_UtilRegion_Prep.RegionID";
                            medianRowsSelect = ", RegionID";
                            medianMediansSelect = ", TotalRows.RegionID";
                            medianMediansWhere = "AND TotalRows.RegionID = Temp_UtilRegion_Prep.RegionID ";
                            medianUpdateJoin = "AND Temp_UtilRegion_Region.RegionID = Medians.RegionID ";
                            groupBy = groupBy + ", Temp_UtilRegion_Prep.RegionID";
                            medianGroupBy = medianGroupBy + ", RegionID";
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
                    catVal = (ipStrat == "Combined") ? "0" : "Temp_UtilRegion_Prep." + ipStrat;
                    if (ipStrat == "Combined")
                    {
                        medianUpdateJoin = medianUpdateJoin + "AND Temp_UtilRegion_Region.CatID = 0 " +
                            "AND Temp_UtilRegion_Region.CatVal = 0 ";
                    }
                    else
                    {
                        groupBy = groupBy + ", Temp_UtilRegion_Prep." + ipStrat;
                        medianRowsSelect = medianRowsSelect + ", " + ipStrat;
                        medianGroupBy = medianGroupBy + ", " + ipStrat;
                        medianMediansSelect = medianMediansSelect + ", TotalRows." + ipStrat;
                        medianMediansWhere = medianMediansWhere + "AND TotalRows." + ipStrat + " = Temp_UtilRegion_Prep." + ipStrat + " ";
                        medianUpdateJoin = medianUpdateJoin + "AND Temp_UtilRegion_Region.CatID = " + catId + " " +
                            "AND Temp_UtilRegion_Region.CatVal = Medians." + ipStrat + " ";
                    }

                    // Setup the message for this section
                    string message = string.Format("Utilization Region - Aggregating {0} Data - ", section);
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

                    ExecuteNonQuery(finalMeanSql, string.Format("Utilization Region - Aggregating {0} Data - UtilType {1} by {2}", section, ipTable, ipStrat));

                    finalMedianSql = MedianSql
                        .Replace("%%ReportID%%", ReportID)
                        .Replace("%%MedianRowsSelect%%", medianRowsSelect)
                        .Replace("%%GroupBy%%", groupBy)
                        .Replace("%%MedianGroupBy%%", medianGroupBy)
                        .Replace("%%MedianMediansSelect%%", medianMediansSelect)
                        .Replace("%%MedianMediansWhere%%", medianMediansWhere)
                        .Replace("%%MedianUpdateJoin%%", medianUpdateJoin);

                    if (calcMedianCharge)
                    {
                        chargeMedianSql = finalMedianSql
                            .Replace("%%SourceMedianField%%", "TotalCharge")
                            .Replace("%%DestMedianField%%", "Temp_UtilRegion_Region.MedianCharges");
                        ExecuteNonQuery(chargeMedianSql, string.Format("Utilization Region - Aggregating {0} Data - Add median charge", section));
                    }

                    if (calcMedianCost)
                    {
                        costMedianSql = finalMedianSql
                            .Replace("%%SourceMedianField%%", "TotalCost")
                            .Replace("%%DestMedianField%%", "Temp_UtilRegion_Region.MedianCosts");
                        ExecuteNonQuery(costMedianSql, string.Format("Utilization Region - Aggregating {0} Data - Add median cost", section));
                    }

                    if (calcMedianLos)
                    {
                        losMedianSql = finalMedianSql
                            .Replace("%%SourceMedianField%%", "LengthOfStay")
                            .Replace("%%DestMedianField%%", "Temp_UtilRegion_Region.MedianLOS");
                        ExecuteNonQuery(losMedianSql, string.Format("Utilization Region - Aggregating {0} Data - Add median length of stay", section));
                    }


                    // Customize the base means sql for individual util types.
                    finalMeanSql = baseMeanSql
                        .Replace("%%UtilID%%", "Temp_UtilRegion_Prep.UtilID")
                        .Replace("%%GroupBy%%", ", Temp_UtilRegion_Prep.UtilID" + groupBy);
                    ExecuteNonQuery(finalMeanSql, string.Format("Utilization Region - Aggregating {0} Data - UtilTypeUtil {1} by {2}", section, ipTable, ipStrat));

                    finalMedianSql = MedianSql
                        .Replace("%%ReportID%%", ReportID)
                        .Replace("%%MedianRowsSelect%%", ", UtilID" + medianRowsSelect)
                        .Replace("%%GroupBy%%", ", UtilID" + groupBy)
                        .Replace("%%MedianGroupBy%%", ", UtilID" + medianGroupBy)
                        .Replace("%%MedianMediansSelect%%", ", TotalRows.UtilID" + medianMediansSelect)
                        .Replace("%%MedianMediansWhere%%", "AND TotalRows.UtilID = Temp_UtilRegion_Prep.UtilID " + medianMediansWhere)
                        .Replace("%%MedianUpdateJoin%%", "AND Temp_UtilRegion_Region.UtilID = Medians.UtilID " + medianUpdateJoin);

                    if (calcMedianCharge)
                    {
                        chargeMedianSql = finalMedianSql
                            .Replace("%%SourceMedianField%%", "TotalCharge")
                            .Replace("%%DestMedianField%%", "Temp_UtilRegion_Region.MedianCharges");
                        ExecuteNonQuery(chargeMedianSql, string.Format("Utilization Region - Aggregating {0} Data - Add median charge", section));
                    }

                    if (calcMedianCost)
                    {
                        costMedianSql = finalMedianSql
                            .Replace("%%SourceMedianField%%", "TotalCost")
                            .Replace("%%DestMedianField%%", "Temp_UtilRegion_Region.MedianCosts");
                        ExecuteNonQuery(costMedianSql, string.Format("Utilization Region - Aggregating {0} Data - Add median cost", section));
                    }

                    if (calcMedianLos)
                    {
                        losMedianSql = finalMedianSql
                            .Replace("%%SourceMedianField%%", "LengthOfStay")
                            .Replace("%%DestMedianField%%", "Temp_UtilRegion_Region.MedianLOS");
                        ExecuteNonQuery(losMedianSql, string.Format("Utilization Region - Aggregating {0} Data - Add median length of stay", section));
                    }
                }
            }

            // Add the rate of discharge.
            LogMessage(string.Format("Utilization Region - Aggregating {0} Data - Adding rate of discharge.", section));
            var RegionRateMeasure = CurrentWebsite.Measures.Where(m => m.ReportMeasure.Name.Equals("IP-15")).FirstOrNull();
            var scale =
                RegionRateMeasure == null ? 1000 :
                    (((WebsiteMeasure)RegionRateMeasure).ReportMeasure.ScaleBy.HasValue ? ((WebsiteMeasure)RegionRateMeasure).ReportMeasure.ScaleBy.Value : 1000);
            var initParams = new KeyValuePair<string, object>[] {
                            new KeyValuePair<string, object>("@ReportID", ReportID),
                            // TODO: This is the right way I think
                            //new KeyValuePair<string, object>("@ReportYear", CurrentWebsite.ReportedYear),
                            new KeyValuePair<string, object>("@ReportYear", 2012),
                            new KeyValuePair<string, object>("@Scale", scale)
                        };
            RunSproc("spUtilRegionAddRateDischargesToRegion", string.Format("Utilization Region - Aggregating {0} Data - Add rate of discharges to counties.", section), initParams);
            RunSproc("spUtilRegionAddRateDischargesToAllCombined", string.Format("Utilization Region - Aggregating {0} Data - Add rate of discharges to all combined.", section), initParams);

            // Reenable the indexes.
            EnableDisableTableIndexes(false, "Temp_UtilRegion_Region");

            // Clean up prep table.
            string sql = string.Format("DELETE FROM Temp_UtilRegion_Prep WHERE ID = '{0}'", ReportID);
            ExecuteNonQuery(sql, string.Format("Utilization Region - Aggregating {0} Data - Removing any previous prep data.", section));

            TimeSpan timeDiff = DateTime.Now - start;
            Logger.Write(string.Format("Utilization Region - Aggregating {0} Data - Aggregation completed in {1:c}", section, timeDiff));
        }

		/// <summary>
		/// Suppresses the data.
		/// </summary>
		/// <param name="section">The section.</param>
		private void SuppressData(string section)
        {
            DateTime start = DateTime.Now;
            LogMessage(string.Format("Utilization Region - Suppressing {0} Data", section));

            // Suppress the main field.
            StringBuilder sql = new StringBuilder();
            sql.Append("UPDATE Temp_UtilRegion_Region ");
            sql.Append("SET Discharges = -2, RateDischarges = -2, MeanCharges = -2, MeanCosts = -2, MeanLOS = -2, ");
            sql.Append("    MedianCharges = -2, MedianCosts = -2, MedianLOS = -2 ");
            sql.Append("WHERE Discharges > 0 AND Discharges < " + GetSuppression("IP-12"));
            ExecuteNonQuery(sql.ToString(), string.Format("Utilization Region - Suppressing {0} Data - Suppressing discharges", section));
            TimeSpan timeDiff = DateTime.Now - start;
            Logger.Write(string.Format("Utilization Region - Suppressing {0} Data - Suppression completed in {1:c}", section, timeDiff));
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

                UtilizationDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "Region");
                if (!Directory.Exists(UtilizationDataDir)) Directory.CreateDirectory(UtilizationDataDir);


                // DRG Directories
                DRGDataDir = Path.Combine(UtilizationDataDir, "DRG");
                if (!Directory.Exists(DRGDataDir)) Directory.CreateDirectory(BaseDataDir);


                DRGHospitalRegionsDataDir = Path.Combine(DRGDataDir, "Region");
                if (!Directory.Exists(DRGHospitalRegionsDataDir)) Directory.CreateDirectory(DRGHospitalRegionsDataDir);


                // MDC Directories
                MDCDataDir = Path.Combine(UtilizationDataDir, "MDC");
                if (!Directory.Exists(MDCDataDir)) Directory.CreateDirectory(MDCDataDir);


                MDCHospitalRegionsDataDir = Path.Combine(MDCDataDir, "Region");
                if (!Directory.Exists(MDCHospitalRegionsDataDir)) Directory.CreateDirectory(MDCHospitalRegionsDataDir);

                // CCS Directories
                CCSDataDir = Path.Combine(UtilizationDataDir, "CCS");
                if (!Directory.Exists(CCSDataDir)) Directory.CreateDirectory(CCSDataDir);


                CCSHospitalRegionsDataDir = Path.Combine(CCSDataDir, "Region");
                if (!Directory.Exists(CCSHospitalRegionsDataDir)) Directory.CreateDirectory(CCSHospitalRegionsDataDir);

                // PRCCS Directories
                PRCCSDataDir = Path.Combine(UtilizationDataDir, "PRCCS");
                if (!Directory.Exists(PRCCSDataDir)) Directory.CreateDirectory(PRCCSDataDir);


                PRCCSHospitalRegionsDataDir = Path.Combine(PRCCSDataDir, "Region");
                if (!Directory.Exists(PRCCSHospitalRegionsDataDir)) Directory.CreateDirectory(PRCCSHospitalRegionsDataDir);

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

                string sectionStart = "Utilization Region - Output {0} Data - Starting generation of output files by {1}";
                string sectionEnd = "Utilization Region - Output {0} Data - Generated output files by {1} in {2:c}";

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
                    elapsedTime = GenerateUtilizationJsonFileCombinations(DRGDataDir, "DRG/DRG_", jsonDomain, "spUtilRegionGetSummaryDataByClinical", "spUtilRegionGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 1)), DRGList, "@UtilID", "Id", RegionList, "@RegionID", "RegionID");
                    Logger.Write(string.Format(sectionEnd, "DRG", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "DRG", "region"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(DRGHospitalRegionsDataDir, "Region_", jsonDomain, "spUtilRegionGetSummaryDataByGeo", "spUtilRegionGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 1)), RegionList, "@RegionID", "RegionID", DRGList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "DRG", "clinical dimension", elapsedTime));
                }
                #endregion DRG Output

                #region MDC Output
                // Output the two MDC sections
                if (section == "MDC")
                {
                    LogMessage(string.Format(sectionStart, "MDC", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(MDCDataDir, "MDC/MDC_", jsonDomain, "spUtilRegionGetSummaryDataByClinical", "spUtilRegionGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 2)), MDCList, "@UtilID", "Id", RegionList, "@RegionID", "RegionID");
                    Logger.Write(string.Format(sectionEnd, "MDC", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "MDC", "region"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(MDCHospitalRegionsDataDir, "Region_", jsonDomain, "spUtilRegionGetSummaryDataByGeo", "spUtilRegionGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 2)), RegionList, "@RegionID", "RegionID", MDCList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "MDC", "clinical dimension", elapsedTime));
                }
                #endregion MDC Output

                #region CCS Output
                // Output the two CCS sections
                if (section == "DXCCS")
                {
                    LogMessage(string.Format(sectionStart, "DXCCS", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(CCSDataDir, "CCS/CCS_", jsonDomain, "spUtilRegionGetSummaryDataByClinical", "spUtilRegionGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 3)), CCSList, "@UtilID", "Id", RegionList, "@RegionID", "RegionID");
                    Logger.Write(string.Format(sectionEnd, "DXCCS", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "DXCCS", "region"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(CCSHospitalRegionsDataDir, "Region_", jsonDomain, "spUtilRegionGetSummaryDataByGeo", "spUtilRegionGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 3)), RegionList, "@RegionID", "RegionID", CCSList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "DXCCS", "clinical dimension", elapsedTime));
                }
                #endregion CCS Output

                #region PRCCS Output
                // Output the two PRCCS sections
                if (section == "PRCCS")
                {
                    LogMessage(string.Format(sectionStart, "PRCCS", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(PRCCSDataDir, "PRCCS/PRCCS_", jsonDomain, "spUtilRegionGetSummaryDataByClinical", "spUtilRegionGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 4)), PRCCSList, "@UtilID", "Id", RegionList, "@RegionID", "RegionID");
                    Logger.Write(string.Format(sectionEnd, "PRCCS", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "PRCCS", "region"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(PRCCSHospitalRegionsDataDir, "Region_", jsonDomain, "spUtilRegionGetSummaryDataByGeo", "spUtilRegionGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 4)), RegionList, "@RegionID", "RegionID", PRCCSList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "PRCCS", "clinical dimension", elapsedTime));
                }
                #endregion PRCCS Output

                // Log the total time.
                TimeSpan groupTimeDiffAll = DateTime.Now - groupStartAll;
                Logger.Write(string.Format("Utilization Region - Output {0} Data - Generated output files in {1:c}", section, groupTimeDiffAll));
                Logger.Write(string.Format("Utilization Region - Output {0} Data - Total file IO time was {1:c}", section, FileIOTime));
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
