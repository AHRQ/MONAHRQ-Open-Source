using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Generators;

using Monahrq.Sdk.Services.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Monahrq.Infrastructure.Configuration;

namespace Monahrq.Wing.Discharge.Inpatient
{
	/// <summary>
	/// Generates the report data/.json files for County Inpatient reports.
	/// </summary>
	/// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
	/// <seealso cref="Monahrq.Sdk.Generators.IReportGenerator" />
	[Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(new string[] { "2AAF7FBA-7102-4C66-8598-A70597E2F824" },
					 new[] { "Inpatient Utilization Report" },
					 new[] { typeof(InpatientTarget) },
					 10)]
    public class InpatientReportGenerator : BaseReportGenerator, IReportGenerator
    {
		//ConnectionStringSettings ConnectionSettings { get; set; }

		/// <summary>
		/// The json domain
		/// </summary>
		private const string jsonDomain = "$.monahrq.inpatientutilization=";
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
		/// Gets or sets the DRG hospital names data dir.
		/// </summary>
		/// <value>
		/// The DRG hospital names data dir.
		/// </value>
		private string DRGHospitalNamesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the DRG hospital counties data dir.
		/// </summary>
		/// <value>
		/// The DRG hospital counties data dir.
		/// </value>
		private string DRGHospitalCountiesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the DRG hospital regions data dir.
		/// </summary>
		/// <value>
		/// The DRG hospital regions data dir.
		/// </value>
		private string DRGHospitalRegionsDataDir { get; set; }
		/// <summary>
		/// Gets or sets the DRG hospital types data dir.
		/// </summary>
		/// <value>
		/// The DRG hospital types data dir.
		/// </value>
		private string DRGHospitalTypesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the MDC data dir.
		/// </summary>
		/// <value>
		/// The MDC data dir.
		/// </value>
		private string MDCDataDir { get; set; }
		/// <summary>
		/// Gets or sets the MDC hospital names data dir.
		/// </summary>
		/// <value>
		/// The MDC hospital names data dir.
		/// </value>
		private string MDCHospitalNamesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the MDC hospital counties data dir.
		/// </summary>
		/// <value>
		/// The MDC hospital counties data dir.
		/// </value>
		private string MDCHospitalCountiesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the MDC hospital regions data dir.
		/// </summary>
		/// <value>
		/// The MDC hospital regions data dir.
		/// </value>
		private string MDCHospitalRegionsDataDir { get; set; }
		/// <summary>
		/// Gets or sets the MDC hospital types data dir.
		/// </summary>
		/// <value>
		/// The MDC hospital types data dir.
		/// </value>
		private string MDCHospitalTypesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS data dir.
		/// </summary>
		/// <value>
		/// The CCS data dir.
		/// </value>
		private string CCSDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS hospital names data dir.
		/// </summary>
		/// <value>
		/// The CCS hospital names data dir.
		/// </value>
		private string CCSHospitalNamesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS hospital counties data dir.
		/// </summary>
		/// <value>
		/// The CCS hospital counties data dir.
		/// </value>
		private string CCSHospitalCountiesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS hospital regions data dir.
		/// </summary>
		/// <value>
		/// The CCS hospital regions data dir.
		/// </value>
		private string CCSHospitalRegionsDataDir { get; set; }
		/// <summary>
		/// Gets or sets the CCS hospital types data dir.
		/// </summary>
		/// <value>
		/// The CCS hospital types data dir.
		/// </value>
		private string CCSHospitalTypesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the PRCCS data dir.
		/// </summary>
		/// <value>
		/// The PRCCS data dir.
		/// </value>
		private string PRCCSDataDir { get; set; }
		/// <summary>
		/// Gets or sets the PRCCS hospital names data dir.
		/// </summary>
		/// <value>
		/// The PRCCS hospital names data dir.
		/// </value>
		private string PRCCSHospitalNamesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the PRCCS hospital counties data dir.
		/// </summary>
		/// <value>
		/// The PRCCS hospital counties data dir.
		/// </value>
		private string PRCCSHospitalCountiesDataDir { get; set; }
		/// <summary>
		/// Gets or sets the PRCCS hospital regions data dir.
		/// </summary>
		/// <value>
		/// The PRCCS hospital regions data dir.
		/// </value>
		private string PRCCSHospitalRegionsDataDir { get; set; }
		/// <summary>
		/// Gets or sets the PRCCS hospital types data dir.
		/// </summary>
		/// <value>
		/// The PRCCS hospital types data dir.
		/// </value>
		private string PRCCSHospitalTypesDataDir { get; set; }

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
	        INSERT INTO %%DestTableName%%
            SELECT      
		        '%%ReportID%%',
		        %%MeanSelect%%,
	            Temp_UtilIP_Prep.UtilTypeID,
		        %%UtilID%% AS UtilID,
		        %%CatID%% AS CatID,
		        %%CatVal%% AS CatVal,

	            COUNT(*) AS Discharges,
	            AVG(Temp_UtilIP_Prep.TotalCharge) AS MeanCharges,
	            AVG(Temp_UtilIP_Prep.TotalCost) AS MeanCosts,
	            AVG(Temp_UtilIP_Prep.LengthOfStay) AS MeanLOS,
	            -1 AS MedianCharges,
	            -1 AS MedianCosts,
	            -1 AS MedianLOS
	        FROM %%FromTable%%
	        WHERE ID = '%%ReportID%%'
	        GROUP BY Temp_UtilIP_Prep.UtilTypeID%%GroupBy%%;
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
	            FROM %%FromTable%%
	            WHERE Temp_UtilIP_Prep.ID = '%%ReportID%%' AND %%SourceMedianField%% IS NOT NULL
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
			            FROM %%FromTable%%
			            WHERE Temp_UtilIP_Prep.ID = '%%ReportID%%'
				            AND TotalRows.UtilTypeID = Temp_UtilIP_Prep.UtilTypeID
				            %%MedianMediansWhere%%
				            AND %%SourceMedianField%% IS NOT NULL
			            ORDER BY %%SourceMedianField%%
	            ) Medians
	            WHERE RowNumber BETWEEN (((TotalRows.TotalRows - 1) / 2) + 1) AND
		            (((TotalRows.TotalRows - 1) / 2) + ( 1 + (1 - TotalRows.TotalRows % 2)))
	            GROUP BY UtilTypeID%%MedianGroupBy%%
            )
            UPDATE %%DestTableName%%
            SET %%DestMedianField%% = Medians.MedianVal
            FROM %%DestTableName%% 
	            JOIN Medians ON %%DestTableName%%.UtilTypeID = Medians.UtilTypeID %%MedianUpdateJoin%%;
        ";
		#endregion MedianSql

		#endregion SQLCode

		/// <summary>
		/// Initializes a new instance of the <see cref="InpatientReportGenerator"/> class.
		/// </summary>
		public InpatientReportGenerator() : base()
        {}

		/// <summary>
		/// Called when [installed].
		/// </summary>
		public void OnInstalled()
        {
            // The wing was just added to Monahrq, so start importing the needed base data.
            //base.OnWingAdded();
            //string[] scriptFiles ={
            //                            "Table-TempUtilIPCounty.sql",
            //                            "Table-TempUtilIPHospital.sql",
            //                            "Table-TempUtilIPHospitalType.sql",
            //                            "Table-TempUtilIPNationalTotals.sql",
            //                            "Table-TempUtilIPPrep.sql",
            //                            "Table-TempUtilIPRegion.sql",
            //                            "Sproc-spUtilIPGetDetailData.sql",
            //                            "Sproc-spUtilIPGetRecordsInIPTarget.sql",
            //                            "Sproc-spUtilIPGetSummaryDataByClinical.sql",
            //                            "Sproc-spUtilIPGetSummaryDataByGeo.sql",
            //                            "Sproc-spUtilIPInitializeDRG.sql",
            //                            "Sproc-spUtilIPInitializeDXCCS.sql",
            //                            "Sproc-spUtilIPInitializeMDC.sql",
            //                            "Sproc-spUtilIPInitializePRCCS.sql",
            //                            "Sproc-spUtilIPUpdateHospitalType.sql",
            //                            "Sproc-spUtilIPUpdateZip.sql"
            //                      };

            //RunSqlScripts(Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\UtilizationIP"), scriptFiles);
        }

		/// <summary>
		/// Initializes the generator.
		/// </summary>
		public override void InitGenerator()
        {
            // Following should only run once, but this procedure is running every time on application startup.
            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for Inpatient reports" });

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
		/// Handles the MessageReceived event of the p control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
		private void p_MessageReceived(object sender, ProcessEventArgs e)
        {
            LogMessage(e.Message);
        }

		/// <summary>
		/// Generates the report.
		/// </summary>
		/// <param name="website">The website.</param>
		/// <param name="publishTask">The publish task.</param>
		public override void GenerateReport(Website website, PublishTask publishTask = PublishTask.Full)
        {
            if (publishTask == PublishTask.PreviewOnly)
            {
                // Do nothing for previews
                return;
            }

            CurrentWebsite = website;

            // Rebuild all Targets_InpatientTargets indexes.
            ReIndexInpatientTargetTable();

            decimal suppression = GetSuppression("IP-01");

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
                LogMessage(string.Format("Generating {0} Report Data for year {1}", "Inpatient Utilization", dataset.Dataset.ReportingYear));
                var datasetRecord=dataset.Dataset.Id;

                try
                {
                    var process = new Process();
                    var psi = new ProcessStartInfo();
                    const string fileName = @"Modules\Generators\IPGenerator\IPGenerator.exe";
                    if (!File.Exists(fileName)) return;
                    psi.FileName = fileName;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    psi.RedirectStandardOutput = true;
                    psi.Arguments = string.Format("-d \"{0}\" -c \"{1}\" -wid \"{2}\" -i {3} -s {4} -r {5} -l 1 -o {6}",
                                                  Path.Combine(website.OutPutDirectory, "Data", "InpatientUtilization",dataset.Dataset.ReportingYear),
                                                  MonahrqConfiguration.SettingsGroup.MonahrqSettings().EntityConnectionSettings.ConnectionString,
                                                  CurrentWebsite.Id,
                                                  datasetRecord,
                                                  suppression,
                                                  regionType,
                                                  (website.UtilizationReportCompression.HasValue && website.UtilizationReportCompression.Value));

                    process.StartInfo = psi;
                    process.Start();
                    
                    do
                    {
                        var logMessage = process.StandardOutput.ReadLineAsync().Result;
                        if (!string.IsNullOrEmpty(logMessage))
                            LogMessage(logMessage);

                    } while (!process.HasExited);//!process.WaitForExit(43200000)
                    process.Close();
                    process.Dispose();

                    var tempPath = Path.GetTempPath() + "Monahrq\\Generators\\IPGenerator\\";
                    //IOHelper.DeleteFolderRecursive(new DirectoryInfo(tempPath));
                    if (Directory.Exists(tempPath))
                        Directory.Delete(tempPath, true);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            
            //return;
            /*
            try
            {
                if (publishTask == PublishTask.PreviewOnly)
                {
                    // Do nothing for previews
                    return;
                }

                // Start the timer.
                DateTime groupStart = DateTime.Now;

                // This is the one that should be called first.
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
                Logger.Write(string.Format("Utilization IP - Generation completed in {0:c}", DateTime.Now - groupStart));
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
            */
        }

		/// <summary>
		/// Res the index inpatient target table.
		/// </summary>
		private void ReIndexInpatientTargetTable()
        {
            Logger.Write("Start re-indexing Inpatient Discharge target table: Targets_InpatientTargets");
            
            var query = "ALTER INDEX ALL ON [dbo].[Targets_InpatientTargets] REBUILD;";
            base.ExecuteNonQuery(query);

            Logger.Write("Finished re-indexing Inpatient Discharge target table: Targets_InpatientTargets");

        }

		//    private void InitializeReportData()
		//    {
		//        try
		//        {
		//var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
		//#region Get base information about the website - hospitals, measures, datasets, etc.

		//            // Get the needed DataSets
		//            IPDatasetIDs = new DataTable();
		//            IPDatasetIDs.Columns.Add("ID", typeof(int));
		//            foreach (WebsiteDataset dataSet in CurrentWebsite.Datasets)
		//            {
		//                switch (dataSet.Dataset.ContentType.Name)
		//                {
		//                    case "Inpatient Discharge":
		//                        // Add a new IP dataset
		//                        IPDatasetIDs.Rows.Add((dataSet.Dataset.Id));
		//                        break;
		//                    default:
		//                        break;
		//                }
		//            }

		//            #endregion Get base information about the website - hospitals, measures, datasets, etc.

		//            #region Generate the specific data for this report.

		//            // Save a report ID for this particular report run.
		//            ReportID = Guid.NewGuid().ToString();

		//            // Get the lists needed to generate the json files.
		//            DRGList = new DataTable();
		//            DRGList = RunSprocReturnDataTable("spGetDRG");

		//            MDCList = new DataTable();
		//            MDCList = RunSprocReturnDataTable("spGetMDC");

		//            CCSList = new DataTable();
		//            CCSList = RunSprocReturnDataTable("spGetDXCCS");

		//            PRCCSList = new DataTable();
		//            PRCCSList = RunSprocReturnDataTable("spGetPRCCS");

		//var selectedRegionContextType = new KeyValuePair<string, object>("@RegionType", configService.HospitalRegion.SelectedRegionType.Name);

		//            HospitalList = new DataTable();
		//            HospitalList = RunSprocReturnDataTable("spGetHospitals", new KeyValuePair<string, object>("@Hospitals", HospitalIds), selectedRegionContextType);

		//            CountyList = new DataTable();
		//            CountyList = RunSprocReturnDataTable("spGetHospitalCounties", new KeyValuePair<string, object>("@Hospitals", HospitalIds));

		//            RegionList = new DataTable();
		//            RegionList = RunSprocReturnDataTable("spGetHospitalRegions", new KeyValuePair<string, object>("@Hospitals", HospitalIds), selectedRegionContextType);

		//            HospitalTypeList = new DataTable();
		//            HospitalTypeList = RunSprocReturnDataTable("spGetHospitalTypes", new KeyValuePair<string, object>("@Hospitals", HospitalIds));

		//            // Get the number of rows in the target tables
		//            DataTable IPRows = RunSprocReturnDataTable("spUtilIPGetRecordsInIPTarget",
		//                    new KeyValuePair<string, object>("@Hospitals", HospitalIds),
		//                    new KeyValuePair<string, object>("@IPDataset", IPDatasetIDs));
		//            if (IPRows.Rows.Count == 1)
		//            {
		//                Logger.Write(string.Format("Utilization IP - Rows in IP Dataset: {0:n0}", IPRows.Rows[0]["IPRows"]));
		//            }

		//            #endregion Generate the specific data for this report.
		//        }
		//        catch (Exception ex)
		//        {
		//            Logger.Write(ex);
		//        }
		//    }

		//private void GenerateDimensionPath(string section)
		//{
		//    DateTime start = DateTime.Now;
		//    PrepData(section);
		//    AggregateData(section);
		//    SuppressData(section);
		//    GenerateJsonFiles(section);
		//    TimeSpan timeDiff = DateTime.Now - start;
		//    Logger.Write(string.Format("Utilization IP - {0} dimension completed in {1:c}", section, timeDiff));
		//}

		//     private void PrepData(string section)
		//     {
		//         // Add minimally needed data to prep table

		//var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
		//var initParams = new KeyValuePair<string, object>[] {
		//                         new KeyValuePair<string, object>("@ReportID", ReportID),
		//                         new KeyValuePair<string, object>("@ReportYear", this.CurrentWebsite.ReportedYear),
		//                         new KeyValuePair<string, object>("@IPDataset", IPDatasetIDs),
		//                         new KeyValuePair<string, object>("@Hospitals", HospitalIds), 
		//                         new KeyValuePair<string, object>("@RegionType", configService.HospitalRegion.SelectedRegionType.Name)
		//                     };

		//         EnableDisableTableIndexes(true, "Temp_UtilIP_Prep");
		//         LogMessage(string.Format("Utilization IP - Prepping {0} Data - Prepping data", section));
		//         RunSproc(string.Format("spUtilIPInitialize{0}", section), string.Format("Utilization IP - Prepping {0} Data - Prepping data", section), initParams);
		//         EnableDisableTableIndexes(false, "Temp_UtilIP_Prep");
		//     }


		/*
        private void AggregateData(string section)
        {
            // Aggregate the data into the temp tables.

            DateTime start = DateTime.Now;
            string[] ipTables = { "Combined", "Hospital", "County", "Region", "HospitalType" };
            string[] ipStrats = { "Combined", "Age", "Sex", "PrimaryPayer", "Race" };
            bool calcMedianCharge = CurrentWebsite.Measures.Any(wm => wm.IsSelected && wm.ReportMeasure.Name.StartsWith("IP-05"));
            bool calcMedianCost = CurrentWebsite.Measures.Any(wm => wm.IsSelected && wm.ReportMeasure.Name.StartsWith("IP-06"));
            bool calcMedianLos = CurrentWebsite.Measures.Any(wm => wm.IsSelected && wm.ReportMeasure.Name.StartsWith("IP-07"));

            // Disable the temp table indexes.
            EnableDisableTableIndexes(true, "Temp_UtilIP_Hospital");
            EnableDisableTableIndexes(true, "Temp_UtilIP_County");
            EnableDisableTableIndexes(true, "Temp_UtilIP_Region");
            EnableDisableTableIndexes(true, "Temp_UtilIP_HospitalType");

            // Loop through the different tables and strat combinations, modify the aggregation script and generate the report data.
            foreach (var ipTable in ipTables)
            {
                foreach (var ipStrat in ipStrats)
                {
                    string finalMeanSql = "";
                    string finalMedianSql = "";
                    string destTableName = "";
                    string meanSelect = "";
                    string medianRowsSelect = "";
                    string medianMediansSelect = "";
                    string medianMediansWhere = "";
                    string medianUpdateJoin = "";
                    string chargeMedianSql = "";
                    string costMedianSql = "";
                    string losMedianSql = "";
                    string catId = "";
                    string fromTable = "";
                    string groupBy = "";

                    switch (ipTable)
                    {
                        case "Combined":
                            destTableName = "Temp_UtilIP_Hospital";
                            meanSelect = "0, 0, 0, '', ''";
                            medianRowsSelect = "";
                            medianMediansSelect = "";
                            medianMediansWhere = "";
                            medianUpdateJoin = "AND Temp_UtilIP_Hospital.HospitalID = 0 ";
                            fromTable = "Temp_UtilIP_Prep";
                            break;
                        case "Hospital":
                            destTableName = "Temp_UtilIP_Hospital";
                            meanSelect = "Temp_UtilIP_Prep.HospitalID, Temp_UtilIP_Prep.RegionID, Temp_UtilIP_Prep.CountyID, '', ''";
                            medianRowsSelect = ", HospitalID";
                            medianMediansSelect = ", TotalRows.HospitalID";
                            medianMediansWhere = "AND TotalRows.HospitalID = Temp_UtilIP_Prep.HospitalID ";
                            medianUpdateJoin = "AND Temp_UtilIP_Hospital.HospitalID = Medians.HospitalID ";
                            fromTable = "Temp_UtilIP_Prep";
                            groupBy = groupBy + ", Temp_UtilIP_Prep.HospitalID, Temp_UtilIP_Prep.RegionID, Temp_UtilIP_Prep.CountyID";
                            break;
                        case "County":
                            destTableName = "Temp_UtilIP_County";
                            meanSelect = "Temp_UtilIP_Prep.CountyID";
                            medianRowsSelect = ", CountyID";
                            medianMediansSelect = ", TotalRows.CountyID";
                            medianMediansWhere = "AND TotalRows.CountyID = Temp_UtilIP_Prep.CountyID ";
                            medianUpdateJoin = "AND Temp_UtilIP_County.CountyID = Medians.CountyID ";
                            fromTable = "Temp_UtilIP_Prep";
                            groupBy = groupBy + ", Temp_UtilIP_Prep.CountyID";
                            break;
                        case "Region":
                            destTableName = "Temp_UtilIP_Region";
                            meanSelect = "Temp_UtilIP_Prep.RegionID";
                            medianRowsSelect = ", RegionID";
                            medianMediansSelect = ", TotalRows.RegionID";
                            medianMediansWhere = "AND TotalRows.RegionID = Temp_UtilIP_Prep.RegionID ";
                            medianUpdateJoin = "AND Temp_UtilIP_Region.RegionID = Medians.RegionID ";
                            fromTable = "Temp_UtilIP_Prep";
                            groupBy = groupBy + ", Temp_UtilIP_Prep.RegionID";
                            break;
                        case "HospitalType":
                            destTableName = "Temp_UtilIP_HospitalType";
                            meanSelect = "Hosp2HospCat.Category_Id";
                            medianRowsSelect = ", Hosp2HospCat.Category_Id AS HospitalTypeID";
                            medianMediansSelect = ", TotalRows.HospitalTypeID";
                            medianMediansWhere = "AND TotalRows.HospitalTypeID = Hosp2HospCat.Category_Id ";
                            medianUpdateJoin = "AND Temp_UtilIP_HospitalType.HospitalTypeID = Medians.HospitalTypeID ";
                            fromTable = "Hospitals_HospitalCategories Hosp2HospCat JOIN Temp_UtilIP_Prep ON Hosp2HospCat.Hospital_Id = Temp_UtilIP_Prep.HospitalID";
                            groupBy = groupBy + ", Hosp2HospCat.Category_Id";
                            break;
                    }
                    string medianGroupBy = (ipTable == "Combined") ? "" : ", " + ipTable + "ID";
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
                        case "PrimaryPayer":
                            catId = "3";
                            break;
                        case "Race":
                            catId = "4";
                            break;
                    }
                    string catVal = (ipStrat == "Combined") ? "0" : "Temp_UtilIP_Prep." + ipStrat;
                    if (ipStrat == "Combined")
                    {
                        medianUpdateJoin = medianUpdateJoin + "AND " + destTableName + ".CatID = 0 " +
                            "AND " + destTableName + ".CatVal = 0 ";
                    }
                    else
                    {
                        groupBy = groupBy + ", Temp_UtilIP_Prep." + ipStrat;
                        medianRowsSelect = medianRowsSelect + ", " + ipStrat;
                        medianGroupBy = medianGroupBy + ", " + ipStrat;
                        medianMediansSelect = medianMediansSelect + ", TotalRows." + ipStrat;
                        medianMediansWhere = medianMediansWhere + "AND TotalRows." + ipStrat + " = Temp_UtilIP_Prep." + ipStrat + " ";
                        medianUpdateJoin = medianUpdateJoin + "AND " + destTableName + ".CatID = " + catId + " " +
                            "AND " + destTableName + ".CatVal = Medians." + ipStrat + " ";
                    }

                    // Setup the message for this section
                    string message = string.Format("Utilization IP - Aggregating {0} Data - ", section);
                    message = message + ((ipStrat == "Combined") ? "Summary data" : "Stratified data for " + ((ipStrat == "PrimaryPayer") ? "primary payer" : ipStrat.ToLower()));
                    message = message + ((ipTable == "Combined") ? " for all data combined" : " by " + ((ipTable == "HospitalType") ? "hospital type" : ipTable.ToLower()) + ".");
                    LogMessage(message);

                    // Setup the base sql for generating the means.
                    string baseMeanSql = MeanSql
                        .Replace("%%DestTableName%%", destTableName)
                        .Replace("%%MeanSelect%%", meanSelect)
                        .Replace("%%CatID%%", catId)
                        .Replace("%%CatVal%%", catVal)
                        .Replace("%%FromTable%%", fromTable)
                        .Replace("%%ReportID%%", ReportID);

                    // Customize the base means sql for all util types combined.
                    finalMeanSql = baseMeanSql
                        .Replace("%%UtilID%%", "0")
                        .Replace("%%GroupBy%%", groupBy);

                    ExecuteNonQuery(finalMeanSql, string.Format("Utilization IP - Aggregating {0} Data - UtilType {1} by {2}", section, ipTable, ipStrat));

                    string baseMedianSql = MedianSql
                        .Replace("%%FromTable%%", fromTable)
                        .Replace("%%DestTableName%%", destTableName)
                        .Replace("%%ReportID%%", ReportID);

                    finalMedianSql = baseMedianSql
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
                            .Replace("%%DestMedianField%%", destTableName + ".MedianCharges");
                        ExecuteNonQuery(chargeMedianSql, string.Format("Utilization IP - Aggregating {0} Data - Add median charge", section));
                    }

                    if (calcMedianCost)
                    {
                        costMedianSql = finalMedianSql
                            .Replace("%%SourceMedianField%%", "TotalCost")
                            .Replace("%%DestMedianField%%", destTableName + ".MedianCosts");
                        ExecuteNonQuery(costMedianSql, string.Format("Utilization IP - Aggregating {0} Data - Add median cost", section));
                    }

                    if (calcMedianLos)
                    {
                        losMedianSql = finalMedianSql
                            .Replace("%%SourceMedianField%%", "LengthOfStay")
                            .Replace("%%DestMedianField%%", destTableName + ".MedianLOS");
                        ExecuteNonQuery(losMedianSql, string.Format("Utilization IP - Aggregating {0} Data - Add median length of stay", section));
                    }

                    // Customize the base means sql for individual util types.
                    finalMeanSql = baseMeanSql
                        .Replace("%%UtilID%%", "Temp_UtilIP_Prep.UtilID")
                        .Replace("%%GroupBy%%", ", Temp_UtilIP_Prep.UtilID" + groupBy);
                    ExecuteNonQuery(finalMeanSql, string.Format("Utilization IP - Aggregating {0} Data - UtilTypeUtil {1} by {2}", section, ipTable, ipStrat));

                    finalMedianSql = baseMedianSql
                        .Replace("%%MedianRowsSelect%%", ", UtilID" + medianRowsSelect)
                        .Replace("%%GroupBy%%", ", UtilID" + groupBy)
                        .Replace("%%MedianGroupBy%%", ", UtilID" + medianGroupBy)
                        .Replace("%%MedianMediansSelect%%", ", TotalRows.UtilID" + medianMediansSelect)
                        .Replace("%%MedianMediansWhere%%", "AND TotalRows.UtilID = Temp_UtilIP_Prep.UtilID " + medianMediansWhere)
                        .Replace("%%MedianUpdateJoin%%", "AND " + destTableName + ".UtilID = Medians.UtilID " + medianUpdateJoin);

                    if (calcMedianCharge)
                    {
                        chargeMedianSql = finalMedianSql
                            .Replace("%%SourceMedianField%%", "TotalCharge")
                            .Replace("%%DestMedianField%%", destTableName + ".MedianCharges");
                        ExecuteNonQuery(chargeMedianSql, string.Format("Utilization IP - Aggregating {0} Data - Add median charge", section));
                    }

                    if (calcMedianCost)
                    {
                        costMedianSql = finalMedianSql
                            .Replace("%%SourceMedianField%%", "TotalCost")
                            .Replace("%%DestMedianField%%", destTableName + ".MedianCosts");
                        ExecuteNonQuery(costMedianSql, string.Format("Utilization IP - Aggregating {0} Data - Add median cost", section));
                    }

                    if (calcMedianLos)
                    {
                        losMedianSql = finalMedianSql
                            .Replace("%%SourceMedianField%%", "LengthOfStay")
                            .Replace("%%DestMedianField%%", destTableName + ".MedianLOS");
                        ExecuteNonQuery(losMedianSql, string.Format("Utilization IP - Aggregating {0} Data - Add median length of stay", section));
                    }
                }
            }

            // Add the zip codes and hospital types to hospital table only.
            var initParams = new KeyValuePair<string, object>[] { new KeyValuePair<string, object>("@ReportID", ReportID) };
            RunSproc("spUtilIPUpdateZip", string.Format("Utilization IP - Aggregating {0} Data - Adding zip codes.", section), initParams);
            RunSproc("spUtilIPUpdateHospitalType", string.Format("Utilization IP - Aggregating {0} Data - Adding hospital types.", section), initParams);

            // Reenable the indexes.
            EnableDisableTableIndexes(false, "Temp_UtilIP_Hospital");
            EnableDisableTableIndexes(false, "Temp_UtilIP_County");
            EnableDisableTableIndexes(false, "Temp_UtilIP_Region");
            EnableDisableTableIndexes(false, "Temp_UtilIP_HospitalType");

            // Clean up prep table.
            string sql = string.Format("DELETE FROM Temp_UtilIP_Prep WHERE ID = '{0}'", ReportID);
            ExecuteNonQuery(sql, string.Format("Utilization IP - Aggregating {0} Data - Removing any previous prep data.", section));

            TimeSpan timeDiff = DateTime.Now - start;
            Logger.Write(string.Format("Utilization IP - Aggregating {0} Data - Aggregation completed in {1:c}", section, timeDiff));
        }

        private void SuppressData(string section)
        {
            DateTime start = DateTime.Now;
            LogMessage(string.Format("Utilization IP - Suppressing {0} Data", section));

            string[] ipTables = { "Hospital", "County", "Region", "HospitalType" };

            // Get suppression value
            decimal suppression = GetSuppression("IP-01");


            foreach (var ipTable in ipTables)
            {
                // Suppress the main field.
                StringBuilder sql = new StringBuilder();
                sql.Append("UPDATE Temp_UtilIP_" + ipTable + " ");
                sql.Append("SET Discharges = -2, MeanCharges = -2, MeanCosts = -2, MeanLOS = -2, ");
                sql.Append("MedianCharges = -2, MedianCosts = -2, MedianLOS = -2 ");
                sql.Append("WHERE Discharges > 0 AND Discharges < " + suppression);
                ExecuteNonQuery(sql.ToString(), string.Format("Utilization IP - Suppressing {0} Data - Suppressing discharges in {1}", section, ipTable));
            }
            TimeSpan timeDiff = DateTime.Now - start;
            Logger.Write(string.Format("Utilization IP - Suppressing {0} Data - Suppression completed in {1:c}", section, timeDiff));
        }
        */

		/// <summary>
		/// Creates the base directories.
		/// </summary>
		private void CreateBaseDirectories()
        {
            try
            {
                BaseDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "Base");
                if (!Directory.Exists(BaseDataDir)) Directory.CreateDirectory(BaseDataDir);

                UtilizationDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "InpatientUtilization");
                if (!Directory.Exists(UtilizationDataDir)) Directory.CreateDirectory(UtilizationDataDir);


                // DRG Directories
                DRGDataDir = Path.Combine(UtilizationDataDir, "DRG");
                if (!Directory.Exists(DRGDataDir)) Directory.CreateDirectory(BaseDataDir);

                DRGHospitalNamesDataDir = Path.Combine(DRGDataDir, "Hospital");
                if (!Directory.Exists(DRGHospitalNamesDataDir)) Directory.CreateDirectory(DRGHospitalNamesDataDir);

                DRGHospitalCountiesDataDir = Path.Combine(DRGDataDir, "County");
                if (!Directory.Exists(DRGHospitalCountiesDataDir)) Directory.CreateDirectory(DRGHospitalCountiesDataDir);

                DRGHospitalRegionsDataDir = Path.Combine(DRGDataDir, "Region");
                if (!Directory.Exists(DRGHospitalRegionsDataDir)) Directory.CreateDirectory(DRGHospitalRegionsDataDir);

                DRGHospitalTypesDataDir = Path.Combine(DRGDataDir, "HospitalType");
                if (!Directory.Exists(DRGHospitalTypesDataDir)) Directory.CreateDirectory(DRGHospitalTypesDataDir);

                // MDC Directories
                MDCDataDir = Path.Combine(UtilizationDataDir, "MDC");
                if (!Directory.Exists(MDCDataDir)) Directory.CreateDirectory(MDCDataDir);

                MDCHospitalNamesDataDir = Path.Combine(MDCDataDir, "Hospital");
                if (!Directory.Exists(MDCHospitalNamesDataDir)) Directory.CreateDirectory(MDCHospitalNamesDataDir);

                MDCHospitalCountiesDataDir = Path.Combine(MDCDataDir, "County");
                if (!Directory.Exists(MDCHospitalCountiesDataDir)) Directory.CreateDirectory(MDCHospitalCountiesDataDir);

                MDCHospitalRegionsDataDir = Path.Combine(MDCDataDir, "Region");
                if (!Directory.Exists(MDCHospitalRegionsDataDir)) Directory.CreateDirectory(MDCHospitalRegionsDataDir);

                MDCHospitalTypesDataDir = Path.Combine(MDCDataDir, "HospitalType");
                if (!Directory.Exists(MDCHospitalTypesDataDir)) Directory.CreateDirectory(MDCHospitalTypesDataDir);

                // CCS Directories
                CCSDataDir = Path.Combine(UtilizationDataDir, "CCS");
                if (!Directory.Exists(CCSDataDir)) Directory.CreateDirectory(CCSDataDir);

                CCSHospitalNamesDataDir = Path.Combine(CCSDataDir, "Hospital");
                if (!Directory.Exists(CCSHospitalNamesDataDir)) Directory.CreateDirectory(CCSHospitalNamesDataDir);

                CCSHospitalCountiesDataDir = Path.Combine(CCSDataDir, "County");
                if (!Directory.Exists(CCSHospitalCountiesDataDir)) Directory.CreateDirectory(CCSHospitalCountiesDataDir);

                CCSHospitalRegionsDataDir = Path.Combine(CCSDataDir, "Region");
                if (!Directory.Exists(CCSHospitalRegionsDataDir)) Directory.CreateDirectory(CCSHospitalRegionsDataDir);

                CCSHospitalTypesDataDir = Path.Combine(CCSDataDir, "HospitalType");
                if (!Directory.Exists(CCSHospitalTypesDataDir)) Directory.CreateDirectory(CCSHospitalTypesDataDir);

                // PRCCS Directories
                PRCCSDataDir = Path.Combine(UtilizationDataDir, "PRCCS");
                if (!Directory.Exists(PRCCSDataDir)) Directory.CreateDirectory(PRCCSDataDir);

                PRCCSHospitalNamesDataDir = Path.Combine(PRCCSDataDir, "Hospital");
                if (!Directory.Exists(PRCCSHospitalNamesDataDir)) Directory.CreateDirectory(PRCCSHospitalNamesDataDir);

                PRCCSHospitalCountiesDataDir = Path.Combine(PRCCSDataDir, "County");
                if (!Directory.Exists(PRCCSHospitalCountiesDataDir)) Directory.CreateDirectory(PRCCSHospitalCountiesDataDir);

                PRCCSHospitalRegionsDataDir = Path.Combine(PRCCSDataDir, "Region");
                if (!Directory.Exists(PRCCSHospitalRegionsDataDir)) Directory.CreateDirectory(PRCCSHospitalRegionsDataDir);

                PRCCSHospitalTypesDataDir = Path.Combine(PRCCSDataDir, "HospitalType");
                if (!Directory.Exists(PRCCSHospitalTypesDataDir)) Directory.CreateDirectory(PRCCSHospitalTypesDataDir);
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
                // Generate any report specific json files

                // Setup the base parameters being passed in.
                KeyValuePair<string, object>[] baseOptions = new KeyValuePair<string, object>[] 
                {
                    new KeyValuePair<string, object>("@ReportID", ReportID),
                };

                string sectionStart = "Utilization IP - Output {0} Data - Starting generation of output files by {1}";
                string sectionEnd = "Utilization IP - Output {0} Data - Generated output files by {1} in {2:c}";

                // Reset the timer in the base file that tracks the json conversion and file write time.
                FileIOTime = TimeSpan.Zero;

                TimeSpan elapsedTime;
                DateTime groupStartAll = DateTime.Now;


                #region DRG Output
                // Output the five DRG sections
                if(section == "DRG")
                {
                    LogMessage(string.Format(sectionStart, "DRG", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(DRGDataDir, "DRG/DRG_", jsonDomain, "spUtilIPGetSummaryDataByClinical", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 1)), DRGList, "@UtilID", "Id", HospitalList, "@HospitalID", "ID");
                    Logger.Write(string.Format(sectionEnd, "DRG", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "DRG", "hospital"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(DRGHospitalNamesDataDir, "Hospital_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 1)), HospitalList, "@HospitalID", "ID", DRGList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "DRG", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "DRG", "county"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(DRGHospitalCountiesDataDir, "County_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 1)), CountyList, "@CountyID", "CountyID", DRGList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "DRG", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "DRG", "region"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(DRGHospitalRegionsDataDir, "Region_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 1)), RegionList, "@RegionID", "RegionID", DRGList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "DRG", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "DRG", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(DRGHospitalTypesDataDir, "HospitalType_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 1)), HospitalTypeList, "@HospitalCategoryID", "HospitalTypeID", DRGList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "DRG", "clinical dimension", elapsedTime));
                }
                #endregion DRG Output


                #region MDC Output
                //Output the five MDC sections
                if (section == "MDC")
                {
                    LogMessage(string.Format(sectionStart, "MDC", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(MDCDataDir, "MDC/MDC_", jsonDomain, "spUtilIPGetSummaryDataByClinical", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 2)), MDCList, "@UtilID", "Id", HospitalList, "@HospitalID", "ID");
                    Logger.Write(string.Format(sectionEnd, "MDC", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "MDC", "hospital"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(MDCHospitalNamesDataDir, "Hospital_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 2)), HospitalList, "@HospitalID", "ID", MDCList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "MDC", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "MDC", "county"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(MDCHospitalCountiesDataDir, "County_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 2)), CountyList, "@CountyID", "CountyID", MDCList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "MDC", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "MDC", "region"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(MDCHospitalRegionsDataDir, "Region_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 2)), RegionList, "@RegionID", "RegionID", MDCList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "MDC", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "MDC", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(MDCHospitalTypesDataDir, "HospitalType_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 2)), HospitalTypeList, "@HospitalCategoryID", "HospitalTypeID", MDCList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "MDC", "clinical dimension", elapsedTime));
                }                
                #endregion MDC Output


                #region DXCCS Output
                //Output the five DXCCS sections
                if (section == "DXCCS")
                {
                    LogMessage(string.Format(sectionStart, "DXCCS", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(CCSDataDir, "CCS/CCS_", jsonDomain, "spUtilIPGetSummaryDataByClinical", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 3)), CCSList, "@UtilID", "Id", HospitalList, "@HospitalID", "ID");
                    Logger.Write(string.Format(sectionEnd, "DXCCS", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "DXCCS", "hospital"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(CCSHospitalNamesDataDir, "Hospital_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 3)), HospitalList, "@HospitalID", "ID", CCSList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "DXCCS", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "DXCCS", "county"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(CCSHospitalCountiesDataDir, "County_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 3)), CountyList, "@CountyID", "CountyID", CCSList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "DXCCS", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "DXCCS", "region"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(CCSHospitalRegionsDataDir, "Region_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 3)), RegionList, "@RegionID", "RegionID", CCSList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "DXCCS", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "DXCCS", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(CCSHospitalTypesDataDir, "HospitalType_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 3)), HospitalTypeList, "@HospitalCategoryID", "HospitalTypeID", CCSList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "DXCCS", "clinical dimension", elapsedTime));
                }
                #endregion DXCCS Output


                #region PRCCS Output
                //Output the five PRCCS sections
                if (section == "PRCCS")
                {
                    LogMessage(string.Format(sectionStart, "PRCCS", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(PRCCSDataDir, "PRCCS/PRCCS_", jsonDomain, "spUtilIPGetSummaryDataByClinical", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 4)), PRCCSList, "@UtilID", "Id", HospitalList, "@HospitalID", "ID");
                    Logger.Write(string.Format(sectionEnd, "PRCCS", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "PRCCS", "hospital"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(PRCCSHospitalNamesDataDir, "Hospital_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 4)), HospitalList, "@HospitalID", "ID", PRCCSList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "PRCCS", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "PRCCS", "county"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(PRCCSHospitalCountiesDataDir, "County_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 4)), CountyList, "@CountyID", "CountyID", PRCCSList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "PRCCS", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "PRCCS", "region"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(PRCCSHospitalRegionsDataDir, "Region_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 4)), RegionList, "@RegionID", "RegionID", PRCCSList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "PRCCS", "clinical dimension", elapsedTime));

                    LogMessage(string.Format(sectionStart, "PRCCS", "clinical dimension"));
                    elapsedTime = GenerateUtilizationJsonFileCombinations(PRCCSHospitalTypesDataDir, "HospitalType_", jsonDomain, "spUtilIPGetSummaryDataByGeo", "spUtilIPGetDetailData",
                        baseOptions.Add(new KeyValuePair<string, object>("@UtilTypeID", 4)), HospitalTypeList, "@HospitalCategoryID", "HospitalTypeID", PRCCSList, "@UtilID", "Id");
                    Logger.Write(string.Format(sectionEnd, "PRCCS", "clinical dimension", elapsedTime));
                }                
                #endregion PRCCS Output

                // Log the total time.
                TimeSpan groupTimeDiffAll = DateTime.Now - groupStartAll;
                Logger.Write(string.Format("Utilization IP - Output {0} Data - Generated output files in {1:c}", section, groupTimeDiffAll));
                Logger.Write(string.Format("Utilization IP - Output {0} Data - Total file IO time was {1:c}", section, FileIOTime));
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
