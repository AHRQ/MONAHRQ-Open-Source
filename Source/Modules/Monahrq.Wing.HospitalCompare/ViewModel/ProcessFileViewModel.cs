using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Utility;
using Monahrq.Theme.Helpers;
using Monahrq.Wing.HospitalCompare.Model;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Monahrq.Sdk.Extensibility.Data;
using System.Xml.Linq;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using System.IO;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using Monahrq.Infrastructure.Extensions;
using NHibernate.Linq;
using System.Dynamic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Monahrq.Sdk.Logging;
using Monahrq.Infrastructure;
using Monahrq.Wing.HospitalCompare.Helpers;

namespace Monahrq.Wing.HospitalCompare.ViewModel
{
    /// <summary>
    /// Import the HospitalCompare data.
    /// </summary>
    /// <seealso cref="HospitalCompareTarget" />
    [ImplementPropertyChanged]
    public class ProcessFileViewModel : SessionedViewModelBase<WizardContext, HospitalCompareTarget>
    {
        // Need to update Access DB tables - stand alone app?
        // Need to transfer data from Access to SQL - Stand alone app?
        // Need to setup SProcs - Part of Monahrq 5 setup

        // CrunchCMS - run at time of website creation?

        private bool Cancelled { get; set; }
        public bool Done { get; set; }
        public ObservableCollection<string> LogFile { get; private set; }
        public event EventHandler<ExtendedEventArgs<Action>> NotifyUi = delegate { };
        //private List<Tuple<string, string>> ModifySqlStatements { get; set; }
        //private ITransaction Transaction { get; set; }
        //private DataTable Hospitals { get; set; }
        //private DataTable Footnotes { get; set; }
        public ILogWriter SessionLogger { get; private set; }

        DateTime start;
        DateTime end;

        IConfigurationService _configurationService;

        //lock object for synchronization;
        private static readonly object _syncLock = new object();

        public ProcessFileViewModel(WizardContext context)
            : base(context)
        {
            _configurationService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            //LogFile = new BackgroundObservableCollection<string>();
            LogFile = new ObservableCollection<string>();
            BindingOperations.EnableCollectionSynchronization(LogFile, _syncLock);
            Done = false;
            Cancelled = false;

            SessionLogger = ServiceLocator.Current.GetInstance<ILogWriter>(LogNames.Session);
        }

        public override string DisplayName
        {
            get { return "Import Data"; }
        }


        public override bool IsValid()
        {
            return Done;
        }

        /// <summary>
        /// Starts the import for HospitalCompare data.
        /// Imports the Measure data from a MAccessDB file and dumps it into the
        /// Targets_HospitalCompareTarget table.
        /// </summary>
        public override void StartImport()
        {
            base.StartImport();
            Task.Run((Action)this.DoImport)
                .ContinueWith(t =>
                {
                    var sb = new StringBuilder();
                    sb.Append("One or more errors occurred during the import process:");
                    for (var ex = t.Exception.Flatten().InnerException; ex != null; ex = ex.InnerException)
                        sb.Append("\t" + ex.Message);
                    AppendLog("Error during import process: " + sb.ToString());
                    this.ImportCompleted(false);
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void DoImport()
        {
            var importer = new ImportProcess();
            var config = importer.Execute(
                base.DataContextObject.ImportType,
                base.DataContextObject.FileName,
                base.DataContextObject.DatasetItem.Id,
                this.AppendLog);
            base.DataContextObject.DatasetItem.VersionMonth = config.SchemaVersionMonth;
            base.DataContextObject.DatasetItem.VersionYear = config.SchemaVersionYear;
            this.ImportCompleted(true);
        }

        /* unused code
        private void VerifyCorrectAccessDB()
        {
            DataTable schema;
            DataRow[] drRows;
            OleDbCommand oleCmd;
            OleDbDataReader oleRdr;
            string sqlStatement;

            try
            {
                schema = AccessConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                drRows = schema.Select("TABLE_NAME = 'vwMeasure_Dates'");
                if (drRows.Count() == 1)
                {
                    sqlStatement = "SELECT * FROM vwMeasure_Dates";
                    oleCmd = new OleDbCommand(sqlStatement, AccessConn);
                    oleRdr = oleCmd.ExecuteReader();
                    schema = oleRdr.GetSchemaTable();
                    drRows = schema.Select("ColumnName = 'msr_cd'");
                    if (drRows.Count() == 1)
                    {
                        // Close the reader so we can reuse it.
                        oleRdr.Close();
                        oleRdr.Dispose();
                        oleCmd.Dispose();

                        sqlStatement = "SELECT * FROM vwMeasure_Dates WHERE msr_cd = 'AMI-2'";
                        oleCmd = new OleDbCommand(sqlStatement, AccessConn);
                        oleRdr = oleCmd.ExecuteReader();
                        if (oleRdr.Read())
                        {
                            if (oleRdr.FieldCount == 5 &&
                                oleRdr[1].ToString() == "3Q2012" &&
                                oleRdr[2].ToString() == "07/01/2012" &&
                                oleRdr[3].ToString() == "2Q2013" &&
                                oleRdr[4].ToString() == "06/30/2013")
                            {
                                // Got the right database.
                                oleRdr.Close();
                                oleRdr.Dispose();
                                oleCmd.Dispose();

                                // Start the database modification procedures.
                                NotifyUi(this, new ExtendedEventArgs<Action>(ImportNationalIMMTable));
                            }
                            else
                            {
                                // Database not recognized.
                                oleRdr.Close();
                                oleRdr.Dispose();
                                oleCmd.Dispose();
                                NotifyUi(this, new ExtendedEventArgs<Action>(DatabaseNotRecognized));
                            }
                        }
                        else
                        {
                            // Database not recognized.
                            oleRdr.Close();
                            oleRdr.Dispose();
                            oleCmd.Dispose();
                            NotifyUi(this, new ExtendedEventArgs<Action>(DatabaseNotRecognized));
                        }

                    }
                    else
                    {
                        // Database not recognized.
                        oleRdr.Close();
                        oleRdr.Dispose();
                        oleCmd.Dispose();
                        NotifyUi(this, new ExtendedEventArgs<Action>(DatabaseNotRecognized));
                    }
                }
                else
                {
                    // Database not recognized.
                    NotifyUi(this, new ExtendedEventArgs<Action>(DatabaseNotRecognized));
                }
            }
            catch (Exception e)
            {
                AppendLog("******************************************************************************");
                AppendLog("Error verifying the Access database:");
                AppendLog(e.Message);
                AppendLog("******************************************************************************");
                NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
            }
        }

        #region Import Access Tables

        /// <summary>
        /// Imports the Hospital Readmissions from Access database to SQL server.
        /// </summary>
        public void ImportHospitalMortalityReadmissionCrosswalkTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing Hospital Readmissions.");

                    // Get the measures from the Access table.
                    string sqlStatement = "SELECT [Provider Number] AS CMSProviderID, Condition AS ConditionCode, [Measure Name] AS MeasureCode, " +
                        "LEFT([Comparison to National Rate], 3) AS Category, Mortality_Readm_Compl_Rate AS Rate, [Number of Patients] AS Sample, " +
                        "[Lower Mortality_Readm Estimate] AS LowerCI, [Upper Mortality_Readm Estimate] AS UpperCI " +
                        "FROM [dbo_vwHQI_HOSP_MORTALITY_READM_XWLK] " +
                        "WHERE Condition IN ('Heart Attack', 'Heart Failure', 'Pneumonia', 'Hospital-Wide All-Cause')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        from rate in dtRates.AsEnumerable()
                        select new
                        {
                            CMSProviderID = rate.Field<string>("CMSProviderID").Trim().Replace("'", ""),
                            ConditionCode = GetCondition(rate.Field<string>("ConditionCode").Trim()),
                            Category = rate.Field<string>("Category").Trim(),
                            MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                            Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1),
                            Sample = GetIntFromString(rate.Field<string>("Sample"), -1),
                            LowerCI = GetDoubleFromString(rate.Field<string>("LowerCI"), -1),
                            UpperCI = GetDoubleFromString(rate.Field<string>("UpperCI"), -1)
                        };

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        DataRow sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["CMSProviderID"] = row.CMSProviderID;
                        sqlRow["ConditionCode"] = row.ConditionCode;
                        sqlRow["CategoryCode"] = (row.Category == "Num") ? 0 : (row.Category == "Bet") ? 1 : (row.Category == "No ") ? 2 : (row.Category == "Wor") ? 3 : 4;
                        sqlRow["MeasureCode"] = (row.ConditionCode == 7) ? "READM-30-HOSP-WIDE" :
                                                ((row.MeasureCode.Contains("Mortality")) ? "MORT-30-" : "READM-30-") +
                                                ((row.ConditionCode == 1) ? "AMI" : (row.ConditionCode == 2) ? "HF" : "PN");
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = row.Rate;
                        sqlRow["Sample"] = row.Sample;
                        sqlRow["Lower"] = row.LowerCI;
                        sqlRow["Upper"] = row.UpperCI;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    //   NotifyUi(this, new ExtendedEventArgs<Action>(ImportStateMortalityReadmissionCrosswalkTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing Hospital Readmissions:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Imports the state mortality readmission crosswalk table.
        /// </summary>
        public void ImportStateMortalityReadmissionCrosswalkTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing State Readmissions.");

                    // Get the measures from the Access table.
                    string sqlStatement = "SELECT State AS BenchmarkID, Condition AS ConditionCode, [Measure Name] AS MeasureCode, " +
                        "Category, [Number of Hospitals] AS Sample " +
                        "FROM [dbo_vwHQI_STATE_MORTALITY_READM_SCRE] " +
                        "WHERE Condition IN ('Heart Attack', 'Heart Failure', 'Pneumonia', 'Hospital-Wide All-Cause')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        from rate in dtRates.AsEnumerable()
                        select new
                        {
                            BenchmarkID = rate.Field<string>("BenchmarkID").Trim().Replace("'", ""),
                            ConditionCode = GetCondition(rate.Field<string>("ConditionCode").Trim()),
                            Category = rate.Field<string>("Category").Trim(),
                            MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                            Sample = GetIntFromString(rate.Field<string>("Sample"), -1),
                        };

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        DataRow sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["BenchmarkID"] = row.BenchmarkID;
                        sqlRow["ConditionCode"] = row.ConditionCode;
                        sqlRow["CategoryCode"] = (row.Category == "Num") ? 0 : (row.Category == "Bet") ? 1 : (row.Category == "No ") ? 2 : (row.Category == "Wor") ? 3 : 4;
                        sqlRow["MeasureCode"] = (row.ConditionCode == 7) ? "READM-30-HOSP-WIDE" :
                                                ((row.MeasureCode.Contains("Mortality")) ? "30DAY_MORT_" : "30DAY_READM_") +
                                                ((row.ConditionCode == 1) ? "HA" : (row.ConditionCode == 2) ? "HF" : "Pn");
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = -1;
                        sqlRow["Sample"] = row.Sample;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    //NotifyUi(this, new ExtendedEventArgs<Action>(ImportNationalMortalityReadmissionCrosswalkTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing State Readmissions:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Imports the national mortality readmission crosswalk table.
        /// </summary>
        public void ImportNationalMortalityReadmissionCrosswalkTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing National Readmissions.");

                    // Get the measures from the Access table.
                    string sqlStatement = "SELECT Condition AS ConditionCode, [Measure Name] AS MeasureCode, [National Mortality_Readm Rate] AS Rate " +
                        "FROM [dbo_vwHQI_US_NATIONAL_MORTALITY_READM_RATE] " +
                        "WHERE Condition IN ('Heart Attack', 'Heart Failure', 'Pneumonia', 'Hospital-Wide All-Cause')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        from rate in dtRates.AsEnumerable()
                        select new
                        {
                            ConditionCode = GetCondition(rate.Field<string>("ConditionCode").Trim()),
                            MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                            Rate = GetFloatFromString(rate.Field<string>("Rate"), -1),
                        };

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        DataRow sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["BenchmarkID"] = "US";
                        sqlRow["ConditionCode"] = row.ConditionCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["MeasureCode"] = (row.ConditionCode == 7) ? "READM-30-HOSP-WIDE" :
                                                ((row.MeasureCode.Contains("Mortality")) ? "30DAY_MORT_" : "30DAY_READM_") +
                                                ((row.ConditionCode == 1) ? "HA" : (row.ConditionCode == 2) ? "HF" : "Pn");
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = row.Rate;
                        sqlRow["Sample"] = -1;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    //  NotifyUi(this, new ExtendedEventArgs<Action>(ImportHospitalMeasureCrosswalkTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing National Readmissions:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Import the Hospital Measures from Access database to SQL server.
        /// </summary>
        public void ImportHospitalMeasureCrosswalkTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing Hospital Measures.");

                    // Get the measures from the Access table.
                    string sqlStatement = "SELECT [Provider Number] AS CMSProviderID, Condition AS ConditionCode, [Measure Code] AS MeasureCode, Score AS Rate, Sample " +
                        "FROM [dbo_vwHQI_HOSP_MSR_XWLK] " +
                        "WHERE [Measure Code] IN " +
                        "('AMI_2', 'AMI_7a', 'AMI_8a', 'AMI_10', 'HF_1', 'HF_2', 'HF_3', 'OP_2', 'OP_3b', 'OP_4', 'OP_5', 'OP_6', 'OP_7', 'PC_01', 'PN_3B', 'PN_6', " +
                        "'SCIP_CARD_2', 'SCIP_INF_1', 'SCIP_INF_2', 'SCIP_INF_3', 'SCIP_INF_4', 'SCIP_INF_9', 'SCIP_INF_10', 'SCIP_VTE_2', 'VTE_6')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        from rate in dtRates.AsEnumerable()
                        select new
                        {
                            CMSProviderID = rate.Field<string>("CMSProviderID").Trim().Replace("'", ""),
                            ConditionCode = GetCondition(rate.Field<string>("ConditionCode").Trim()),
                            MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-")
                                .Replace("SCIP-INF-1a0", "SCIP-INF-10"),
                            Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1),
                            Sample = GetIntFromString(rate.Field<string>("Sample"), -1)
                        };

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        DataRow sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["CMSProviderID"] = row.CMSProviderID;
                        sqlRow["ConditionCode"] = row.ConditionCode;
                        sqlRow["MeasureCode"] = row.MeasureCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = row.Rate;
                        sqlRow["Sample"] = row.Sample;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    //NotifyUi(this, new ExtendedEventArgs<Action>(ImportStateMeasureCrosswalkTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing Hospital Measures:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Imports the state measure crosswalk table.
        /// </summary>
        public void ImportStateMeasureCrosswalkTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing State Measures.");

                    // Get the measures from the Access table.
                    string sqlStatement = "SELECT State AS BenchmarkID, Condition AS ConditionCode, [Measure Code] AS MeasureCode, Score AS Rate " +
                        "FROM [dbo_vwHQI_STATE_MSR_AVG] " +
                        "WHERE [Measure Code] IN " +
                        "('AMI_2', 'AMI_7a', 'AMI_8a', 'AMI_10', 'HF_1', 'HF_2', 'HF_3', 'OP_1', 'OP_2', 'OP_3b', 'OP_4', 'OP_5', 'OP_6', 'OP_7', 'PC_01', 'PN_3B', 'PN_6', " +
                        "'SCIP_CARD_2', 'SCIP_INF_1', 'SCIP_INF_2', 'SCIP_INF_3', 'SCIP_INF_4', 'SCIP_INF_9', 'SCIP_INF_10', 'SCIP_VTE_1', 'SCIP_VTE_2', 'VTE_6')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        from rate in dtRates.AsEnumerable()
                        select new
                        {
                            BenchmarkID = rate.Field<string>("BenchmarkID").Trim().Replace("'", ""),
                            ConditionCode = GetCondition(rate.Field<string>("ConditionCode").Trim()),
                            MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                            Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1)
                        };

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        DataRow sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["BenchmarkID"] = row.BenchmarkID;
                        sqlRow["ConditionCode"] = row.ConditionCode;
                        sqlRow["MeasureCode"] = row.MeasureCode == "OP-3b" ? "OP-3" : row.MeasureCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = row.Rate;
                        sqlRow["Sample"] = -1;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    //NotifyUi(this, new ExtendedEventArgs<Action>(ImportPercentileMeasureCrosswalkTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing State Measures:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Imports the percentile measure crosswalk table.
        /// </summary>
        public void ImportPercentileMeasureCrosswalkTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing National Measures.");

                    // Get the measures from the Access table.
                    string sqlStatement = "SELECT Condition AS ConditionCode, [Measure Code] AS MeasureCode, Percentile, Score AS Rate " +
                        "FROM [dbo_vwHQI_PCTL_MSR_XWLK] " +
                        "WHERE [Measure Code] IN " +
                        "('AMI_2', 'AMI_7a', 'AMI_8a', 'AMI_10', 'HF_1', 'HF_2', 'HF_3', 'OP_1', 'OP_2', 'OP_3b', 'OP_4', 'OP_5', 'OP_6', 'OP_7', 'PC_01', 'PN_3B', 'PN_6', " +
                        "'SCIP_CARD_2', 'SCIP_INF_1', 'SCIP_INF_2', 'SCIP_INF_3', 'SCIP_INF_4', 'SCIP_INF_9', 'SCIP_INF_10', 'SCIP_VTE_1', 'SCIP_VTE_2', 'VTE_6')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        from rate in dtRates.AsEnumerable()
                        select new
                        {
                            ConditionCode = GetCondition(rate.Field<string>("ConditionCode").Trim()),
                            MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                            Percentile = rate.Field<string>("Percentile").Trim(),
                            Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1)
                        };

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        DataRow sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["BenchmarkID"] = row.Percentile.Contains("Top 10%") ? "TOP10" : "US";
                        sqlRow["ConditionCode"] = row.ConditionCode;
                        sqlRow["MeasureCode"] = row.MeasureCode == "OP-3b" ? "OP-3" : row.MeasureCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = row.Rate;
                        sqlRow["Sample"] = -1;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    // NotifyUi(this, new ExtendedEventArgs<Action>(ImportHospitalHcahpsMeasuresTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing National Measures:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(() => AbortImport()));
                }
            }
        }

        /// <summary>
        /// Import the Hospital Consumer Assessment of Healthcare Providers and Systems from Access database to SQL server.
        /// </summary>
        public void ImportHospitalHcahpsMeasuresTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing Hospital Hospital Consumer Assessment of Healthcare Providers and Systems.");

                    // Get the measures from the Access table.
                    string sqlStatement = "SELECT [Provider Number] AS CMSProviderID, [HCAHPS Measure Code] AS MeasureCode, [HCAHPS Answer Percent] AS Rate, " +
                        "[Survey Response Rate Percent] AS Sample, [Number of Completed Surveys] AS CategoryCode " +
                        "FROM [dbo_vwHQI_HOSP_HCAHPS_MSR] " +
                        "WHERE [HCAHPS Measure Code] IN " +
                        "('H_CLEAN_HSP_A_P', 'H_COMP_1_A_P', 'H_COMP_2_A_P', 'H_COMP_3_A_P', 'H_COMP_4_A_P', 'H_COMP_5_A_P', 'H_QUIET_HSP_A_P', " +
                        "'H_CLEAN_HSP_SN_P', 'H_COMP_1_SN_P', 'H_COMP_2_SN_P', 'H_COMP_3_SN_P', 'H_COMP_4_SN_P', 'H_COMP_5_SN_P', 'H_QUIET_HSP_SN_P', " +
                        "'H_CLEAN_HSP_U_P', 'H_COMP_1_U_P', 'H_COMP_2_U_P', 'H_COMP_3_U_P', 'H_COMP_4_U_P', 'H_COMP_5_U_P', 'H_QUIET_HSP_U_P', " +
                        "'H_COMP_6_Y_P', 'H_HSP_RATING_0_6', 'H_HSP_RATING_7_8', 'H_HSP_RATING_9_10', 'H_RECMND_DY', 'H_RECMND_PY', 'H_RECMND_DN')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        from rate in dtRates.AsEnumerable()
                        select new
                        {
                            CMSProviderID = rate.Field<string>("CMSProviderID").Trim().Replace("'", ""),
                            MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-")
                                .Replace("-A-P", "").Replace("-9-10", "").Replace("-DY", "").Replace("-Y-P", ""),
                            Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1),
                            Sample = GetIntFromString(rate.Field<string>("Sample"), -1),
                            CategoryCode = (rate.Field<string>("CategoryCode").StartsWith("300")) ? 3 :
                                           (rate.Field<string>("CategoryCode").StartsWith("Between")) ? 2 :
                                           (rate.Field<string>("CategoryCode").StartsWith("Fewer")) ? 1 : 0
                        };

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        DataRow sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["CMSProviderID"] = row.CMSProviderID;
                        sqlRow["ConditionCode"] = 0;
                        sqlRow["MeasureCode"] = row.MeasureCode;
                        sqlRow["CategoryCode"] = row.CategoryCode;
                        sqlRow["Rate"] = row.Rate;
                        sqlRow["Sample"] = row.Sample;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    // NotifyUi(this, new ExtendedEventArgs<Action>(ImportStateHcahpsMeasuresTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing Hospital Hospital Consumer Assessment of Healthcare Providers and Systems:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Imports the state hcahps measures table.
        /// </summary>
        public void ImportStateHcahpsMeasuresTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing State Hospital Consumer Assessment of Healthcare Providers and Systems.");

                    // Load the records from the Access database.
                    string sqlStatement = "SELECT State AS BenchmarkID, [HCAHPS Measure Code] AS MeasureCode, [HCAHPS Answer Percent] AS Rate " +
                        "FROM [dbo_vwHQI_STATE_HCAHPS_MSR] " +
                        "WHERE [HCAHPS Measure Code] IN " +
                        "('H_CLEAN_HSP_A_P', 'H_COMP_1_A_P', 'H_COMP_2_A_P', 'H_COMP_3_A_P', 'H_COMP_4_A_P', 'H_COMP_5_A_P', 'H_QUIET_HSP_A_P', " +
                        "'H_CLEAN_HSP_SN_P', 'H_COMP_1_SN_P', 'H_COMP_2_SN_P', 'H_COMP_3_SN_P', 'H_COMP_4_SN_P', 'H_COMP_5_SN_P', 'H_QUIET_HSP_SN_P', " +
                        "'H_CLEAN_HSP_U_P', 'H_COMP_1_U_P', 'H_COMP_2_U_P', 'H_COMP_3_U_P', 'H_COMP_4_U_P', 'H_COMP_5_U_P', 'H_QUIET_HSP_U_P', " +
                        "'H_COMP_6_Y_P', 'H_HSP_RATING_0_6', 'H_HSP_RATING_7_8', 'H_HSP_RATING_9_10', 'H_RECMND_DY', 'H_RECMND_PY', 'H_RECMND_DN')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        from rate in dtRates.AsEnumerable()
                        select new
                        {
                            BenchmarkID = rate.Field<string>("BenchmarkID").Trim().Replace("'", ""),
                            MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-")
                                .Replace("-A-P", "").Replace("-9-10", "").Replace("-DY", "").Replace("-Y-P", ""),
                            Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1)
                        };

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        DataRow sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["BenchmarkID"] = row.BenchmarkID;
                        sqlRow["ConditionCode"] = 0;
                        sqlRow["MeasureCode"] = row.MeasureCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = row.Rate;
                        sqlRow["Sample"] = -1;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    //NotifyUi(this, new ExtendedEventArgs<Action>(ImportNationalHcahpsMeasuresTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing State Hospital Consumer Assessment of Healthcare Providers and Systems:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Imports the national hcahps measures table.
        /// </summary>
        public void ImportNationalHcahpsMeasuresTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing National Hospital Consumer Assessment of Healthcare Providers and Systems.");

                    // Load the records from the Access database.
                    string sqlStatement = "SELECT [HCAHPS Measure Code] AS MeasureCode, [HCAHPS Answer Percent] AS Rate " +
                        "FROM [dbo_vwHQI_US_NATIONAL_HCAHPS_MSR] " +
                        "WHERE [HCAHPS Measure Code] IN " +
                        "('H_CLEAN_HSP_A_P', 'H_COMP_1_A_P', 'H_COMP_2_A_P', 'H_COMP_3_A_P', 'H_COMP_4_A_P', 'H_COMP_5_A_P', 'H_QUIET_HSP_A_P', " +
                        "'H_CLEAN_HSP_SN_P', 'H_COMP_1_SN_P', 'H_COMP_2_SN_P', 'H_COMP_3_SN_P', 'H_COMP_4_SN_P', 'H_COMP_5_SN_P', 'H_QUIET_HSP_SN_P', " +
                        "'H_CLEAN_HSP_U_P', 'H_COMP_1_U_P', 'H_COMP_2_U_P', 'H_COMP_3_U_P', 'H_COMP_4_U_P', 'H_COMP_5_U_P', 'H_QUIET_HSP_U_P', " +
                        "'H_COMP_6_Y_P', 'H_HSP_RATING_0_6', 'H_HSP_RATING_7_8', 'H_HSP_RATING_9_10', 'H_RECMND_DY', 'H_RECMND_PY', 'H_RECMND_DN')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        from rate in dtRates.AsEnumerable()
                        select new
                        {
                            MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-")
                                .Replace("-A-P", "").Replace("-9-10", "").Replace("-DY", "").Replace("-Y-P", ""),
                            Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1)
                        };

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        DataRow sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["BenchmarkID"] = "US";
                        sqlRow["ConditionCode"] = 0;
                        sqlRow["MeasureCode"] = row.MeasureCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = row.Rate;
                        sqlRow["Sample"] = -1;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    //NotifyUi(this, new ExtendedEventArgs<Action>(ImportHospitalImagingCrosswalkTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing National Hospital Consumer Assessment of Healthcare Providers and Systems:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Import the Hospital Imaging from Access database to SQL server.
        /// </summary>
        public void ImportHospitalImagingCrosswalkTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing Hospital Imaging.");

                    // Get the measures from the Access table.
                    string sqlStatement = "SELECT [Provider Number] AS CMSProviderID, [Measure Code] AS MeasureCode, Score AS Rate, Sample " +
                        "FROM [dbo_vwHQI_HOSP_IMG_XWLK] " +
                        "WHERE [Measure Code] IN ('OP_8', 'OP_10', 'OP_11', 'OP_13', 'OP_14')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        from rate in dtRates.AsEnumerable()
                        select new
                        {
                            CMSProviderID = rate.Field<string>("CMSProviderID").Trim().Replace("'", ""),
                            MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                            Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1),
                            Sample = GetIntFromString(rate.Field<string>("Sample"), -1)
                        };


                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        DataRow sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["CMSProviderID"] = row.CMSProviderID;
                        sqlRow["ConditionCode"] = 6;
                        sqlRow["MeasureCode"] = row.MeasureCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = ((row.MeasureCode == "OP-10" || row.MeasureCode == "OP-11") && row.Rate != -1) ? row.Rate * 100 : row.Rate;
                        sqlRow["Sample"] = row.Sample;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    // NotifyUi(this, new ExtendedEventArgs<Action>(ImportStateImagingCrosswalkTable));
                    ImportCompleted(true);

                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing Hospital Imaging:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Imports the state imaging crosswalk table.
        /// </summary>
        public void ImportStateImagingCrosswalkTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing State Imaging.");

                    // Get the measures from the Access table.
                    string sqlStatement = "SELECT State AS BenchmarkID, [Measure Code] AS MeasureCode, Score AS Rate " +
                        "FROM [dbo_vwHQI_STATE_IMG_AVG] " +
                        "WHERE [Measure Code] IN ('OP_8', 'OP_10', 'OP_11', 'OP_13', 'OP_14')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        from rate in dtRates.AsEnumerable()
                        select new
                        {
                            BenchmarkID = rate.Field<string>("BenchmarkID").Trim().Replace("'", ""),
                            MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                            Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1)
                        };


                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        DataRow sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["BenchmarkID"] = row.BenchmarkID;
                        sqlRow["ConditionCode"] = 6;
                        sqlRow["MeasureCode"] = row.MeasureCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = ((row.MeasureCode == "OP-10" || row.MeasureCode == "OP-11") && row.Rate != -1) ? row.Rate * 100 : row.Rate;
                        sqlRow["Sample"] = -1;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    // NotifyUi(this, new ExtendedEventArgs<Action>(ImportNationalImagingCrosswalkTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing State Imaging:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Imports the national imaging crosswalk table.
        /// </summary>
        public void ImportNationalImagingCrosswalkTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing National Imaging.");

                    // Get the measures from the Access table.
                    string sqlStatement = "SELECT [Measure Code] AS MeasureCode, Score AS Rate " +
                        "FROM [dbo_vwHQI_US_NATIONAL_IMG_AVG] " +
                        "WHERE [Measure Code] IN ('OP_8', 'OP_10', 'OP_11', 'OP_13', 'OP_14')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        from rate in dtRates.AsEnumerable()
                        select new
                        {
                            MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                            Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1)
                        };


                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        DataRow sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["BenchmarkID"] = "US";
                        sqlRow["ConditionCode"] = 6;
                        sqlRow["MeasureCode"] = row.MeasureCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = ((row.MeasureCode == "OP-10" || row.MeasureCode == "OP-11") && row.Rate != -1) ? row.Rate * 100 : row.Rate;
                        sqlRow["Sample"] = -1;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    // NotifyUi(this, new ExtendedEventArgs<Action>(ImportHospitalHaiTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing National Imaging:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(() => AbortImport()));
                }
            }
        }

        /// <summary>
        /// Import the Hospital Healthcare-Associated Infections from Access database to SQL server.
        /// </summary>
        public void ImportHospitalHaiTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing Hospital Healthcare-Associated Infections.");

                    // Get the measures from the Access table.
                    string sqlStatement = "SELECT prvdr_id AS CMSProviderID, LEFT(msr_cd, LEN(msr_cd) - 4) AS MeasureCode, scr AS Rate " +
                        "FROM [vwHQI_HOSP_HAI] WHERE RIGHT(msr_cd, 4) = '_SIR' " +
                        "AND msr_cd IN ('HAI_1_SIR', 'HAI_2_SIR', 'HAI_5_SIR', 'HAI_6_SIR')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    sqlStatement = "SELECT prvdr_id AS CMSProviderID, LEFT(msr_cd, LEN(msr_cd) - 9) AS MeasureCode, scr AS LowerCI " +
                        "FROM [vwHQI_HOSP_HAI] WHERE RIGHT(msr_cd, 9) = '_CI_LOWER' " +
                        "AND msr_cd IN ('HAI_1_CI_LOWER', 'HAI_2_CI_LOWER', 'HAI_5_CI_LOWER', 'HAI_6_CI_LOWER')";
                    adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsLowerCI = new DataSet();
                    adapter.Fill(dsLowerCI);
                    DataTable dtLowerCI = dsLowerCI.Tables[0];

                    sqlStatement = "SELECT prvdr_id AS CMSProviderID, LEFT(msr_cd, LEN(msr_cd) - 9) AS MeasureCode, scr AS UpperCI " +
                        "FROM [vwHQI_HOSP_HAI] WHERE RIGHT(msr_cd, 9) = '_CI_UPPER' " +
                        "AND msr_cd IN ('HAI_1_CI_UPPER', 'HAI_2_CI_UPPER', 'HAI_5_CI_UPPER', 'HAI_6_CI_UPPER')";
                    adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsUpperCI = new DataSet();
                    adapter.Fill(dsUpperCI);
                    DataTable dtUpperCI = dsUpperCI.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        (from rate in dtRates.AsEnumerable()
                         join lowerCI in dtLowerCI.AsEnumerable()
                             on new { CMSProviderID = rate.Field<string>("CMSProviderID"), MeasureCode = rate.Field<string>("MeasureCode") }
                                 equals new { CMSProviderID = lowerCI.Field<string>("CMSProviderID"), MeasureCode = lowerCI.Field<string>("MeasureCode") }
                         join upperCI in dtUpperCI.AsEnumerable()
                             on new { CMSProviderID = rate.Field<string>("CMSProviderID"), MeasureCode = rate.Field<string>("MeasureCode") }
                                 equals new { CMSProviderID = upperCI.Field<string>("CMSProviderID"), MeasureCode = upperCI.Field<string>("MeasureCode") }
                         select new
                         {
                             CMSProviderID = rate.Field<string>("CMSProviderID").Trim().Replace("'", ""),
                             MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-").Replace("-SIR", ""),
                             Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1),
                             LowerCI = GetDoubleFromString(lowerCI.Field<string>("LowerCI"), -1),
                             UpperCI = GetDoubleFromString(upperCI.Field<string>("UpperCI"), -1)
                         }).ToList();

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        var sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["CMSProviderID"] = row.CMSProviderID;
                        sqlRow["ConditionCode"] = 0;
                        sqlRow["MeasureCode"] = row.MeasureCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = row.Rate;
                        sqlRow["Sample"] = -1;
                        sqlRow["Lower"] = row.LowerCI;
                        sqlRow["Upper"] = row.UpperCI;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    // NotifyUi(this, new ExtendedEventArgs<Action>(ImportStateHaiTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing Hospital Healthcare-Associated Infections:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Imports the state hai table.
        /// </summary>
        public void ImportStateHaiTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing State Healthcare-Associated Infections.");

                    // Get the measures from the Access table.
                    var sqlStatement = "SELECT state AS BenchmarkID, LEFT(msr_cd, LEN(msr_cd) - 4) AS MeasureCode, scr AS Rate " +
                        "FROM [vwHQI_HOSP_HAI_STATE] WHERE RIGHT(msr_cd, 4) = '_SIR' " +
                        "AND msr_cd IN ('HAI_1_SIR', 'HAI_2_SIR', 'HAI_5_SIR', 'HAI_6_SIR')";
                    var adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    var dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    var dtRates = dsRates.Tables[0];

                    sqlStatement = "SELECT state AS BenchmarkID, LEFT(msr_cd, LEN(msr_cd) - 9) AS MeasureCode, scr AS LowerCI " +
                        "FROM [vwHQI_HOSP_HAI_STATE] WHERE RIGHT(msr_cd, 9) = '_CI_LOWER' " +
                        "AND msr_cd IN ('HAI_1_CI_LOWER', 'HAI_2_CI_LOWER', 'HAI_5_CI_LOWER', 'HAI_6_CI_LOWER')";
                    adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    var dsLowerCI = new DataSet();
                    adapter.Fill(dsLowerCI);
                    var dtLowerCI = dsLowerCI.Tables[0];

                    sqlStatement = "SELECT state AS BenchmarkID, LEFT(msr_cd, LEN(msr_cd) - 9) AS MeasureCode, scr AS UpperCI " +
                        "FROM [vwHQI_HOSP_HAI_STATE] WHERE RIGHT(msr_cd, 9) = '_CI_UPPER' " +
                        "AND msr_cd IN ('HAI_1_CI_UPPER', 'HAI_2_CI_UPPER', 'HAI_5_CI_UPPER', 'HAI_6_CI_UPPER')";
                    adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    var dsUpperCI = new DataSet();
                    adapter.Fill(dsUpperCI);
                    var dtUpperCI = dsUpperCI.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        (from rate in dtRates.AsEnumerable()
                         join lowerCI in dtLowerCI.AsEnumerable()
                             on new { BenchmarkID = rate.Field<string>("BenchmarkID"), MeasureCode = rate.Field<string>("MeasureCode") }
                                 equals new { BenchmarkID = lowerCI.Field<string>("BenchmarkID"), MeasureCode = lowerCI.Field<string>("MeasureCode") }
                         join upperCI in dtUpperCI.AsEnumerable()
                             on new { BenchmarkID = rate.Field<string>("BenchmarkID"), MeasureCode = rate.Field<string>("MeasureCode") }
                                 equals new { BenchmarkID = upperCI.Field<string>("BenchmarkID"), MeasureCode = upperCI.Field<string>("MeasureCode") }
                         select new
                         {
                             BenchmarkID = rate.Field<string>("BenchmarkID").Trim(),
                             MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-").Replace("-SIR", ""),
                             Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1),
                             LowerCI = GetDoubleFromString(lowerCI.Field<string>("LowerCI"), -1),
                             UpperCI = GetDoubleFromString(upperCI.Field<string>("UpperCI"), -1)
                         }).ToList();

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        var sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["BenchmarkID"] = row.BenchmarkID;
                        sqlRow["ConditionCode"] = 0;
                        sqlRow["MeasureCode"] = row.MeasureCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = row.Rate;
                        sqlRow["Sample"] = -1;
                        sqlRow["Lower"] = row.LowerCI;
                        sqlRow["Upper"] = row.UpperCI;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    //NotifyUi(this, new ExtendedEventArgs<Action>(ImportNationalHaiTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing State Healthcare-Associated Infections:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Imports the national hai table.
        /// </summary>
        public void ImportNationalHaiTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing National Healthcare-Associated Infections.");

                    // Get the measures from the Access table.
                    var sqlStatement = "SELECT LEFT(msr_cd, LEN(msr_cd) - 4) AS MeasureCode, scr AS Rate " +
                        "FROM [vwHQI_HOSP_HAI_National] WHERE RIGHT(msr_cd, 4) = '_SIR' " +
                        "AND msr_cd IN ('HAI_1_SIR', 'HAI_2_SIR', 'HAI_5_SIR', 'HAI_6_SIR')";
                    var adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    var dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    var dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        (from rate in dtRates.AsEnumerable()
                         select new
                         {
                             MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-").Replace("-SIR", ""),
                             Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1)
                         }).ToList();

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        var sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["BenchmarkID"] = "US";
                        sqlRow["ConditionCode"] = 0;
                        sqlRow["MeasureCode"] = row.MeasureCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = row.Rate;
                        sqlRow["Sample"] = -1;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    // NotifyUi(this, new ExtendedEventArgs<Action>(ImportHospitalEDTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing National Healthcare-Associated Infections:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Import the Hospital Emergency Discharge from Access database to SQL server.
        /// </summary>
        public void ImportHospitalEDTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing Hospital Emergency Discharge.");

                    // Get the measures from the Access table.
                    var sqlStatement = "SELECT prvdr_id AS CMSProviderID, msr_cd AS MeasureCode, scr AS Rate, Sample " +
                        "FROM [dbo_vwHQI_HOSP_ED] " +
                        "WHERE msr_cd IN ('ED_1b', 'ED_2b', 'OP_18b', 'OP_20', 'OP_21', 'OP_22')";
                    var adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    var dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    var dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        (from rate in dtRates.AsEnumerable()
                         select new
                         {
                             CMSProviderID = rate.Field<string>("CMSProviderID").Trim().Replace("'", ""),
                             MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                             Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1),
                             Sample = GetIntFromString(rate.Field<string>("Sample"), -1)
                         }).ToList();

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        var sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["CMSProviderID"] = row.CMSProviderID;
                        sqlRow["ConditionCode"] = 0;
                        sqlRow["MeasureCode"] = row.MeasureCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = row.Rate;
                        sqlRow["Sample"] = row.Sample;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    // NotifyUi(this, new ExtendedEventArgs<Action>(ImportStateEDTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing Hospital Emergency Discharge:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Imports the state ed table.
        /// </summary>
        public void ImportStateEDTable()
        {
            if (Cancelled) return;

            try
            {
                AppendLog("   Importing State Emergency Discharge.");

                // Get the measures from the Access table.
                var sqlStatement = "SELECT prvdr_id AS BenchmarkID, msr_cd AS MeasureCode, scr AS Rate " +
                                      "FROM [vwHQI_HOSP_ED_State] " +
                                      "WHERE msr_cd IN ('ED_1b', 'ED_2b', 'OP_18b', 'OP_20', 'OP_21', 'OP_22')";
                var adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                var dsRates = new DataSet();
                adapter.Fill(dsRates);
                var dtRates = dsRates.Tables[0];

                // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                var query =
                    (from rate in dtRates.AsEnumerable()
                     select new
                     {
                         BenchmarkID = rate.Field<string>("BenchmarkID").Trim().Replace("'", ""),
                         MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                         Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1)
                     }).ToList();

                // Add the data to the hospital compare data table.
                foreach (var row in query)
                {
                    var sqlRow = HospCompDataDt.NewRow();
                    sqlRow["Dataset_id"] = DatasetID;
                    sqlRow["BenchmarkID"] = row.BenchmarkID;
                    sqlRow["ConditionCode"] = 0;
                    sqlRow["MeasureCode"] = row.MeasureCode;
                    sqlRow["CategoryCode"] = 0;
                    sqlRow["Rate"] = row.Rate;
                    sqlRow["Sample"] = -1;
                    sqlRow["Lower"] = -1;
                    sqlRow["Upper"] = -1;
                    HospCompDataDt.Rows.Add(sqlRow);
                }
                HospCompDataBc.WriteToServer(HospCompDataDt);
                HospCompDataDt.Clear();
                AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                //NotifyUi(this, new ExtendedEventArgs<Action>(ImportNationalEDTable));
                ImportCompleted(true);
            }
            catch (Exception e)
            {
                AppendLog("******************************************************************************");
                AppendLog("Error importing State Emergency Discharge:");
                AppendLog(e.Message);
                AppendLog("******************************************************************************");
                NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
            }
        }

        /// <summary>
        /// Imports the national ed table.
        /// </summary>
        public void ImportNationalEDTable()
        {
            if (!Cancelled)
            {
                try
                {
                    AppendLog("   Importing National Emergency Discharge.");

                    // Get the measures from the Access table.
                    string sqlStatement = "SELECT msr_cd AS MeasureCode, scr AS Rate " +
                        "FROM [vwHQI_HOSP_ED_National] " +
                        "WHERE msr_cd IN ('ED_1b', 'ED_2b', 'OP_18b', 'OP_20', 'OP_21', 'OP_22')";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                    DataSet dsRates = new DataSet();
                    adapter.Fill(dsRates);
                    DataTable dtRates = dsRates.Tables[0];

                    // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                    var query =
                        (from rate in dtRates.AsEnumerable()
                         select new
                         {
                             MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                             Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1)
                         }).ToList();

                    // Add the data to the hospital compare data table.
                    foreach (var row in query)
                    {
                        var sqlRow = HospCompDataDt.NewRow();
                        sqlRow["Dataset_id"] = DatasetID;
                        sqlRow["BenchmarkID"] = "US";
                        sqlRow["ConditionCode"] = 0;
                        sqlRow["MeasureCode"] = row.MeasureCode;
                        sqlRow["CategoryCode"] = 0;
                        sqlRow["Rate"] = row.Rate;
                        sqlRow["Sample"] = -1;
                        sqlRow["Lower"] = -1;
                        sqlRow["Upper"] = -1;
                        HospCompDataDt.Rows.Add(sqlRow);
                    }
                    HospCompDataBc.WriteToServer(HospCompDataDt);
                    HospCompDataDt.Clear();
                    AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                    //  NotifyUi(this, new ExtendedEventArgs<Action>(ImportHospitalIMMTable));
                    ImportCompleted(true);
                }
                catch (Exception e)
                {
                    AppendLog("******************************************************************************");
                    AppendLog("Error importing National Emergency Discharge:");
                    AppendLog(e.Message);
                    AppendLog("******************************************************************************");
                    NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                }
            }
        }

        /// <summary>
        /// Import the Hospital Immunizations from Access database to SQL server.
        /// </summary>
        public void ImportHospitalIMMTable()
        {
            if (Cancelled) return;
            try
            {
                AppendLog("   Importing Hospital Immunizations.");

                // Get the measures from the Access table.
                var sqlStatement = "SELECT prvdr_id AS CMSProviderID, msr_cd AS MeasureCode, scr AS Rate, Sample " +
                                   "FROM [dbo_vwHQI_HOSP_IMM] " +
                                   "WHERE msr_cd IN ('IMM_1a', 'IMM_2')";
                var adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                var dsRates = new DataSet();
                adapter.Fill(dsRates);
                var dtRates = dsRates.Tables[0];

                // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                var query =
                    (from rate in dtRates.AsEnumerable()
                     select new
                     {
                         CMSProviderID = rate.Field<string>("CMSProviderID").Trim().Replace("'", ""),
                         MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                         Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1),
                         Sample = GetIntFromString(rate.Field<string>("Sample"), -1)
                     }).ToList();

                // Add the data to the hospital compare data table.
                foreach (var row in query)
                {
                    var sqlRow = HospCompDataDt.NewRow();
                    sqlRow["Dataset_id"] = DatasetID;
                    sqlRow["CMSProviderID"] = row.CMSProviderID;
                    sqlRow["ConditionCode"] = 0;
                    sqlRow["MeasureCode"] = row.MeasureCode;
                    sqlRow["CategoryCode"] = 0;
                    sqlRow["Rate"] = row.Rate;
                    sqlRow["Sample"] = row.Sample;
                    sqlRow["Lower"] = -1;
                    sqlRow["Upper"] = -1;
                    HospCompDataDt.Rows.Add(sqlRow);
                }
                HospCompDataBc.WriteToServer(HospCompDataDt);
                HospCompDataDt.Clear();
                AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                //NotifyUi(this, new ExtendedEventArgs<Action>(ImportStateIMMTable));
                ImportCompleted(true);
            }
            catch (Exception e)
            {
                AppendLog("******************************************************************************");
                AppendLog("Error importing Hospital Immunization table:");
                AppendLog(e.Message);
                AppendLog("******************************************************************************");
                NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
            }
        }

        /// <summary>
        /// Imports the state imm table.
        /// </summary>
        public void ImportStateIMMTable()
        {
            if (Cancelled) return;
            try
            {
                AppendLog("   Importing State Immunizations.");

                // Get the measures from the Access table.
                var sqlStatement = "SELECT prvdr_id AS BenchmarkID, msr_cd AS MeasureCode, scr AS Rate " +
                                      "FROM [vwHQI_HOSP_IMM_State] " +
                                      "WHERE msr_cd IN ('IMM_1a', 'IMM_2')";
                var adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                var dsRates = new DataSet();
                adapter.Fill(dsRates);
                var dtRates = dsRates.Tables[0];

                // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                var query =
                    (from rate in dtRates.AsEnumerable()
                     select new
                     {
                         BenchmarkID = rate.Field<string>("BenchmarkID").Trim().Replace("'", ""),
                         MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                         Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1)
                     }).ToList();

                // Add the data to the hospital compare data table.
                foreach (var row in query)
                {
                    var sqlRow = HospCompDataDt.NewRow();
                    sqlRow["Dataset_id"] = DatasetID;
                    sqlRow["BenchmarkID"] = row.BenchmarkID;
                    sqlRow["ConditionCode"] = 0;
                    sqlRow["MeasureCode"] = row.MeasureCode;
                    sqlRow["CategoryCode"] = 0;
                    sqlRow["Rate"] = row.Rate;
                    sqlRow["Sample"] = -1;
                    sqlRow["Lower"] = -1;
                    sqlRow["Upper"] = -1;
                    HospCompDataDt.Rows.Add(sqlRow);
                }
                HospCompDataBc.WriteToServer(HospCompDataDt);
                HospCompDataDt.Clear();
                AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                // NotifyUi(this, new ExtendedEventArgs<Action>(ImportNationalIMMTable));
                ImportCompleted(true);
            }
            catch (Exception e)
            {
                AppendLog("******************************************************************************");
                AppendLog("Error importing State Immunization table:");
                AppendLog(e.Message);
                AppendLog("******************************************************************************");
                NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
            }
        }
        #endregion

        /// <summary>
        /// Imports the national imm table.
        /// </summary>
        public void ImportNationalIMMTable()
        {
            if (Cancelled) return;
            try
            {
                AppendLog("   Importing National Immunizations.");

                // Get the measures from the Access table.
                var sqlStatement = "SELECT msr_cd AS MeasureCode, scr AS Rate " +
                                      "FROM [vwHQI_HOSP_IMM_National] " +
                                      "WHERE msr_cd IN ('IMM_1a', 'IMM_2')";
                var adapter = new OleDbDataAdapter(sqlStatement, AccessConn);
                var dsRates = new DataSet();
                adapter.Fill(dsRates);
                var dtRates = dsRates.Tables[0];

                // Reformat the data with changes in measure name, convert types, and assign default values if no data.
                var query =
                    (from rate in dtRates.AsEnumerable()
                     select new
                     {
                         MeasureCode = rate.Field<string>("MeasureCode").Trim().Replace("_", "-"),
                         Rate = GetDoubleFromString(rate.Field<string>("Rate"), -1)
                     }).ToList();

                // Add the data to the hospital compare data table.
                foreach (var row in query)
                {
                    var sqlRow = HospCompDataDt.NewRow();
                    sqlRow["Dataset_id"] = DatasetID;
                    sqlRow["BenchmarkID"] = "US";
                    sqlRow["ConditionCode"] = 0;
                    sqlRow["MeasureCode"] = row.MeasureCode;
                    sqlRow["CategoryCode"] = 0;
                    sqlRow["Rate"] = row.Rate;
                    sqlRow["Sample"] = -1;
                    sqlRow["Lower"] = -1;
                    sqlRow["Upper"] = -1;
                    HospCompDataDt.Rows.Add(sqlRow);
                }
                HospCompDataBc.WriteToServer(HospCompDataDt);
                HospCompDataDt.Clear();
                AppendLog(string.Format("      {0:n0} records loaded.", query.Count()));

                NotifyUi(this, new ExtendedEventArgs<Action>(() => ImportCompleted(true)));
            }
            catch (Exception e)
            {
                AppendLog("******************************************************************************");
                AppendLog("Error importing National Immunization table:");
                AppendLog(e.Message);
                AppendLog("******************************************************************************");
                NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
            }
        }

        
        /// <summary>
        /// Handles the case when the database is not recognized.
        /// </summary>
        private void DatabaseNotRecognized()
        {
            AppendLog("Database not recognized.");
            AbortImport();
        }

        private int GetCondition(string s)
        {
            return
                (s.StartsWith("Heart Attack")) ? (byte)1 :
                (s.StartsWith("Heart Failure")) ? 2 :
                (s.StartsWith("Pneumonia")) ? 3 :
                (s.StartsWith("Surgical")) ? 4 :
                (s.StartsWith("Children")) ? 5 : // has HTML symbol for Children's Asthma Care
                                                 // 6 = HOSP_IMG_XWLK
                (s.StartsWith("Hospital-Wide")) ? 7 :
                0;
        }

        private float? GetFloatFromString(string inputString, float? defaultVal)
        {
            if (inputString == null)
            {
                return defaultVal;
            }

            float tempFloat;
            return float.TryParse(inputString, out tempFloat) ? tempFloat : defaultVal;
        }

        private double? GetDoubleFromString(string inputString, double? defaultVal)
        {
            if (inputString == null)
            {
                return defaultVal;
            }
            double tempDouble;
            return double.TryParse(inputString, out tempDouble) ? tempDouble : defaultVal;
        }

        private static int? GetIntFromString(string inputString, int? defaultVal)
        {
            if (inputString == null)
            {
                return defaultVal;
            }
            int tempInt;
            return int.TryParse(inputString, out tempInt) ? tempInt : defaultVal;
        }*/

        /// <summary>
        /// Appends a log to the logfile.
        /// </summary>
        /// <param name="message">The message.</param>
        void AppendLog(string message)
        {
            Debug.WriteLine(message);
            Application.Current.Dispatcher.Invoke(() => LogFile.Add(message));
            //System.Windows.Forms.Application.DoEvents();
        }
        
        /// <summary>
        /// Aborts the import.
        /// </summary>
        private void AbortImport()
        {
            if (Cancelled) return;
            try
            {
                DataContextObject.NotifyDeleteEntry();
                AppendLog("Import aborted.");
            }
            catch (Exception)
            {
                AppendLog("Error aborting import.");
            }
            ImportCompleted(false);
        }


        /// <summary>
        /// Imports the completed.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        protected override void ImportCompleted(bool success)
        {
            end = DateTime.Now;
            var elapsed = end.Subtract(start);
            AppendLog(elapsed.ToString("hh\\:mm\\:ss") + " elapsed.");

            try
            {
                if (success)
                {
                    var rootXml = new XElement("LogLines");
                    foreach (var line in LogFile)
                    {
                        rootXml.Add(new XElement("LogLine", line));
                    }
                    DataContextObject.Summary = rootXml.ToString();

                    var fi = new DateTimeFormatInfo();
                    DataContextObject.DatasetItem.VersionMonth = fi.GetMonthName(DataContextObject.Month);
                    DataContextObject.DatasetItem.VersionYear = DataContextObject.Year;

                    if (DataContextObject.DatasetItem.IsReImport)
                    {
                        var linesImported = Session.Query<HospitalCompareTarget>()
                            .Count(hc => hc.Dataset.Id == DataContextObject.DatasetItem.Id);

                        if (DataContextObject.DatasetItem.File.Contains(" (#"))
                            DataContextObject.DatasetItem.File = DataContextObject.DatasetItem.File.SubStrBefore(" (#");

                        DataContextObject.DatasetItem.File += " (# Rows Imported: " + linesImported + ")";
                    }
                    AppendLog("Import completed successfully.");
                }
                else
                {
                    AppendLog("Import was not completed successfully.");
                }

                Done = true;
                // WPF CommandManager periodically calls IsValid to see if the Next/Done button should be enabled. 
                // In multi-threaded wizard steps, IsValid returns the value of the Done flag. Call InvalidateRequerySuggested here
                // on the UI thread after setting the Done flag to force WPF to call IsValid now so the Next/Done button will become enabled. 
                //Application.Current.DoEvents();
                NotifyUi(this, new ExtendedEventArgs<Action>(CommandManager.InvalidateRequerySuggested));
            }
            finally
            {
                base.ImportCompleted(success);
            }
        }
    }
}
