using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Diagnostics;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Exceptions;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.SysTray.Grouper;

namespace Monahrq.SysTray.trayNotification
{
    /// <summary>
    /// Class for SysTrayApplicationContext
    /// </summary>
    /// <seealso cref="System.Windows.Forms.ApplicationContext" />
    public class SysTrayApplicationContext : ApplicationContext
    {
        #region Private Members
        private readonly IContainer _components;   //List of components
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _contextMenu;
        //private readonly ToolStripMenuItem _displayForm;
        private readonly ToolStripMenuItem _exitApplication;
        private readonly int _currentIpTargetImportId;
        private readonly string _configuredConnectionString;
        private SysTrayDbHelper _dbHelper;
        private bool _exitImmediately;

        private readonly HashSet<KeyValuePair<string, Exception>> _errorsDictionary = new HashSet<KeyValuePair<string, Exception>>(); 
        private static string _logFilePath = AppDomain.CurrentDomain.BaseDirectory;

        private readonly string TABLE_SCHEMA_NAME = "[dbo].[" + typeof(Dataset).EntityTableName() + "]";
        private const string TABLE_STATUS_COLUMN_NAME = "[DRGMDCMappingStatus]";
        private const string TABLE_STATUS_MESSAGE_COLUMN_NAME = "[DRGMDCMappingStatusMessage]";

        public const string SYSTRAY_LOGFILE_NAME = "Monahrq.SysTray.Grouper.log";
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SysTrayApplicationContext"/> class.
        /// </summary>
        public SysTrayApplicationContext()
        {
            //Instantiate the component Module to hold everything
            _components = new Container();


            //Instantiate the NotifyIcon attaching it to the components container and 
            //provide it an icon, note, you can imbed this resource 
            _notifyIcon = new NotifyIcon(_components)
                                                    {
                                                        Text = @"MONAHRQ - DRG/MDC Mapping",
                                                        Visible = true
                                                    };

            //Instantiate the context menu and items
            _contextMenu = new ContextMenuStrip();
            //_displayForm = new ToolStripMenuItem();
            _exitApplication = new ToolStripMenuItem();

            //Attach the menu to the notify icon
            _notifyIcon.ContextMenuStrip = _contextMenu;

            //Setup the items and add them to the menu strip, adding handlers to be created later
            //_displayForm.Text = @"Display Form";
            //_displayForm.Click += displayForm_Click;
            //_contextMenu.Items.Add(_displayForm);

            _exitApplication.Text = "Exit";
            _exitApplication.Click += exitApplication_Click;
            _contextMenu.Items.Add(_exitApplication);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SysTrayApplicationContext"/> class.
        /// </summary>
        /// <param name="configuredConnectionString">The configured connection string.</param>
        /// <param name="currentIpTargetImportId">The current ip target import identifier.</param>
        /// <param name="logFilePath">The log file path.</param>
        public SysTrayApplicationContext(string configuredConnectionString, int currentIpTargetImportId, string logFilePath = null)
            : this()
        {
            _configuredConnectionString = configuredConnectionString;
            _currentIpTargetImportId = currentIpTargetImportId;

            if (!string.IsNullOrEmpty(logFilePath))
                _logFilePath = logFilePath;

            _dbHelper = new SysTrayDbHelper(_configuredConnectionString);
            _notifyIcon.Icon = GetApplicationIcon();

            RunGrouper();
        }

        /// <summary>
        /// Infinites the specified page count.
        /// </summary>
        /// <param name="pageCount">The page count.</param>
        /// <returns></returns>
        private static IEnumerable<bool> Infinite(int? pageCount = null)
        {
            if(!pageCount.HasValue)
                while (true) yield return true;

            while (pageCount.Value > 0)
            {
                yield return true;
            }
        }

        /// <summary>
        /// Infinites the specified page count.
        /// </summary>
        /// <param name="pageCount">The page count.</param>
        /// <returns></returns>
        private static IEnumerable<bool> Infinite(int pageCount)
        {
            var pageId = 0;
            while (pageId <= pageCount - 1)
            {
                pageId = pageId++;
                yield return true;
            }
            yield return false;
        }

        /// <summary>
        /// The indexing lock object
        /// </summary>
        private static readonly object _indexingLockObject = new object();
        /// <summary>
        /// The is performing indexing task
        /// </summary>
        private static bool _isPerformingIndexingTask;

        /// <summary>
        /// Runs the grouper.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// No Inpatient dataset found with the Id: " + _currentIpTargetImportId
        /// or
        /// </exception>
        private void RunGrouper()
        {
            try
            {
                _notifyIcon.ShowBalloonTip(2000, "Monahrq: DRG/MDC Grouper", "Grouper Successfully started", ToolTipIcon.Info);

                var datasetQuery =
                    string.Format(@"select c.[Id], c.[File] 'Dataset_Name', c.[DRGMDCMappingStatus], t.[Name] 'Type_Name' 
                                    from {0} c WITH (NOLOCK) 
                                        left outer join [dbo].[Wings_Targets] t on t.Id = c.ContentType_Id 
                                    where c.[Id] = {1} and c.[IsFinished] = 1 
                                    and c.[DRGMDCMappingStatus] not in ('Completed','InProgress')",
                                  TABLE_SCHEMA_NAME, _currentIpTargetImportId);

                DatasetImport dataset = null;

                var dataSetItemTable = _dbHelper.ExecuteDataTable(datasetQuery);

                if (dataSetItemTable != null && dataSetItemTable.Rows.Count > 0)
                {
                    dataset = new DatasetImport();
                    foreach (DataRow item in dataSetItemTable.Rows)
                    {
                        dataset.Id = int.Parse(item[0].ToString());
                        dataset.DatasetName = item[1].ToString();
                        dataset.Status = (DrgMdcMappingStatusEnum)Enum.Parse(typeof(DrgMdcMappingStatusEnum), item[2].ToString());
                        dataset.TypeName = item[3].ToString();
                    }
                }

                if (dataset == null)
                    throw new InvalidOperationException("No Inpatient dataset found with the Id: " + _currentIpTargetImportId);

                if (dataset.Status == DrgMdcMappingStatusEnum.Completed)
                    throw new InvalidOperationException(string.Format("Inpatient dataset \"{0}\" can't be processed as it has already successfully completed DRG/MDC mapping process."));

                UpdateStatus(DrgMdcMappingStatusEnum.InProgress, null);
                // AND (tipt.[DRG] is null OR tipt.[MDC] is null)
                var datasetRowCountQuery = string.Format(@"SELECT count(tipt.[Id]) FROM Targets_InpatientTargets tipt WITH (NOLOCK) WHERE tipt.[{1}_Id] = {0};", _currentIpTargetImportId, typeof(Dataset).Name);
                var datasetRowCount = _dbHelper.ExecuteScalar<int>(datasetRowCountQuery);

                LogMessage(string.Format("Number of rows being processed: {0};", datasetRowCount));
                // Declare an initialize variable to be used to control extent of while loop
                int numberOfItemsInPage = 5000;

                if (datasetRowCount >= 2000000)
                    numberOfItemsInPage = 10000;

                if (datasetRowCount >= 3000000)
                    numberOfItemsInPage = 8000;
                
                //int numberOfItemsInPage = 5000;

               // int pages = ((datasetRowCount - 1) / numberOfItemsInPage) + 1;

                int pageId = 0;
                var batchedInputRecords = new HashSet<List<DatasetInput>>();
                long recordsCount = 1, recordsDisplayCount = 1, recordsToGroupCount = 0;

                // start looping
                Parallel.ForEach(Infinite(), new ParallelOptions { MaxDegreeOfParallelism = MonahrqDiagnostic.LogicalCPUsCount }, (ignored, loopState) =>
                 {
                     DataTable inpatientDataTable = null;
                     lock (_indexingLockObject)
                     {
                         
                         pageId++;

                         //if (datasetRowCount < 1500000)
                         {
                             if (datasetRowCount < 1000000 && pageId%160 == 0) /*Index after every 25k records*/
                             {
                                 // Conditially perform indexing task
                                 var indexingTask = Task.Factory.StartNew(() =>
                                 {
                                     _isPerformingIndexingTask = true;
                                     const string indexScript =
                                         @"  IF NOT EXISTS(SELECT *  FROM sys.indexes WHERE name='IX_Targets_InpatientTargets_DRG_MDC_Dataset_Id' AND object_id = OBJECT_ID('Targets_InpatientTargets'))
                                            BEGIN
	                                            CREATE INDEX IX_Targets_InpatientTargets_DRG_MDC_Dataset_Id ON [Targets_InpatientTargets] (DRG, MDC, Dataset_Id)
                                            END
                                            ELSE
	                                            BEGIN
		                                            DBCC DBREINDEX (Targets_InpatientTargets, IX_Targets_InpatientTargets_DRG_MDC_Dataset_Id, 80);
	                                            END
                                    ";
                                     _dbHelper.ExecuteNonQuery(indexScript);
                                     _isPerformingIndexingTask = false;
                                 });
                                 indexingTask.Wait(5);
                             }
                         }

                         var impatientDatasetQuery = string.Format(@" SELECT * 
                                                                 FROM
                                                                 (
                                                                    SELECT tipt.*, ROW_NUMBER() OVER (ORDER BY tipt.Id) rn
                                                                    FROM Targets_InpatientTargets tipt WITH (NOLOCK) 
                                                                    WHERE [{1}_Id] = {0} 
                                                                      AND [DRG] IS NULL OR [MDC] IS NULL
                                                                 ) sq
                                                                 WHERE 
                                                                 sq.rn BETWEEN {2} AND {3} 
                                                            ",
                                                               _currentIpTargetImportId
                                                               , typeof(Dataset).Name
                                                               , ((pageId - 1) * numberOfItemsInPage) + 1
                                                               , pageId * numberOfItemsInPage);

                         inpatientDataTable = _dbHelper.ExecuteDataTable(impatientDatasetQuery);

                         if (inpatientDataTable == null || inpatientDataTable.Rows.Count < 1)
                         {
                            loopState.Break();
                         }

                         if (inpatientDataTable == null)
                             throw new Exception("Something went wrong, for no records could be found.");

                     }

                     #region Grouper
                     var inputRecords = new ConcurrentQueue<DatasetInput>();
                     
                     foreach (DataRow row in inpatientDataTable.Rows)
                     {
                         var grouperRecord = new DatasetInput();

                         grouperRecord.OptionalInformation = row["Id"].ToString();
                        //grouperRecord.PatientName = row["PatientID"] != null ? row["PatientID"].ToString(): null;
                        //grouperRecord.MedicalRecordNumber = "";
                        //grouperRecord.AccountNumber = "";

                         if (row["LengthOfStay"] != null)
                         {
                             int intLOS;
                             if (int.TryParse(row["LengthOfStay"].ToString(), out intLOS))
                                 grouperRecord.LOS = intLOS;
                         }

                         var icdCodeType = row["ICDCodeType"];
                         double? calculatedLOS = 10;

                         if (icdCodeType != null && icdCodeType.ToString() == "9")
                         {
                             DateTime dtAdmissionDate;
                             DateTime.TryParse(row["AdmissionDate"].ToString(), out dtAdmissionDate);

                             if (dtAdmissionDate != DateTime.MinValue && dtAdmissionDate != DateTime.MaxValue)
                                 grouperRecord.AdmitDate = dtAdmissionDate;
                             else
                             {
                                 grouperRecord.AdmitDate = DateTime.Parse(string.Format("05/01/{0}", row["DischargeYear"]));
                                 if (grouperRecord.LOS > 0)
                                     calculatedLOS = grouperRecord.LOS;
                             }
                         }
                         else
                         {
                             if (row["AdmissionDate"] != null)
                             {
                                 DateTime dtAdmissionDate;
                                 if (DateTime.TryParse(row["AdmissionDate"].ToString(), out dtAdmissionDate))
                                     if (dtAdmissionDate != DateTime.MinValue && dtAdmissionDate != DateTime.MaxValue)
                                     {
                                         grouperRecord.AdmitDate = dtAdmissionDate;
                                         if (grouperRecord.LOS > 0)
                                            calculatedLOS = grouperRecord.LOS;
                                    }
                                         
                             }
                         }


                         //var icdCodeType = row["ICDCodeType"];
                         if (icdCodeType != null && icdCodeType.ToString() == "9")
                         {
                             DateTime dtDischargeDate;
                             DateTime.TryParse(row["DischargeDate"].ToString(), out dtDischargeDate);

                             if (dtDischargeDate != DateTime.MinValue && dtDischargeDate != DateTime.MaxValue)
                             {
                                     if (dtDischargeDate != DateTime.MinValue && dtDischargeDate != DateTime.MaxValue)
                                         grouperRecord.DischargeDate = dtDischargeDate;
                             }
                             else
                             {
                                 grouperRecord.DischargeDate = grouperRecord.AdmitDate.Value.AddDays(calculatedLOS.Value);
                             }
                         }
                         else
                         {
                            if (row["DischargeDate"] != null)
                            {
                                DateTime dtDischargeDate;
                                if (DateTime.TryParse(row["DischargeDate"].ToString(), out dtDischargeDate))
                                    if (dtDischargeDate != DateTime.MinValue && dtDischargeDate != DateTime.MaxValue)
                                        grouperRecord.DischargeDate = dtDischargeDate;
                            }
                            //else
                            //{
                            //    grouperRecord.DischargeDate = DateTime.Parse("09/30/2015");
                            //}
                        }

                         if (icdCodeType != null && icdCodeType.ToString() == "9")
                         {
                             grouperRecord.LOS = grouperRecord.DischargeDate.Value.Subtract(grouperRecord.AdmitDate.Value).Days;
                         }

                         // Discharge Disposition
                         if (row["DischargeDisposition"] != null)
                         {
                             int? dischargeDisposition;

                             DischargeDisposition tempDischargeDisposition;
                             if (!string.IsNullOrEmpty(row["DischargeDisposition"].ToString()) && Enum.TryParse(row["DischargeDisposition"].ToString(), true, out tempDischargeDisposition))
                             {
                                 var actualDischargeDispositionValue = tempDischargeDisposition;

                                 switch (actualDischargeDispositionValue.ToString().ToUpper())
                                 {
                                     case "EXCLUDE":
                                         dischargeDisposition = -1;
                                         break;
                                     case "MISSING":
                                         dischargeDisposition = null;
                                         break;
                                     case "ROUTINE":
                                         dischargeDisposition = 1;
                                         break;
                                     case "SHORTTERM":
                                         dischargeDisposition = 2;
                                         break;
                                     case "NURSINGFACILITY":
                                         dischargeDisposition = 3;
                                         break;
                                     case "INTERMEDIATECARE":
                                         dischargeDisposition = 4;
                                         break;
                                     case "OTHERFACILITY":
                                         dischargeDisposition = 5;
                                         break;
                                     case "HOMEHEALTHCARE":
                                         dischargeDisposition = 6;
                                         break;
                                     case "AMA":
                                         dischargeDisposition = 7;
                                         break;
                                     case "DECEASED":
                                         dischargeDisposition = 20;
                                         break;
                                     case "DISCHARGEDALIVEDESTUNKNOWN":
                                         dischargeDisposition = 99;
                                         break;
                                     default:
                                         dischargeDisposition = null;
                                         break;
                                 }
                             }
                             else
                                 dischargeDisposition = null;

                            grouperRecord.DischargeStatus = dischargeDisposition;
                            // grouperRecord.DischargeStatus = 0;
                         }

                         if (row["BirthDate"] != null)
                         {
                             DateTime dtBirthDate;
                             if (DateTime.TryParse(row["BirthDate"].ToString(), out dtBirthDate))
                                 if (dtBirthDate != DateTime.MinValue && dtBirthDate != DateTime.MaxValue)
                                     grouperRecord.BirthDate = dtBirthDate;
                         }

                         if (row["Age"] != null)
                         {
                             int intAge;
                             if (int.TryParse(row["Age"].ToString(), out intAge))
                                 grouperRecord.Age = intAge;
                         }
                         if (row["Sex"] != null && !row["Sex"].ToString().EqualsIgnoreCase("0"))
                         {
                             grouperRecord.Sex = row["Sex"].ToString().EqualsIgnoreCase("1") ? 1 : 2;
                         }
                         else
                         {
                             grouperRecord.Sex = 0;
                         }

                        // TODO: Optional?
                        //grouperRecord.AdmitDiagnosis = "";

                        // primary payer TODO: Correct this functionality Jason
                        grouperRecord.PrimaryPayer = 1;
                        if (row["PrimaryPayer"] != null)
                        {
                             int primaryPayer;

                             PrimaryPayer tempPayer;
                             if (!string.IsNullOrEmpty(row["PrimaryPayer"].ToString()) && Enum.TryParse(row["PrimaryPayer"].ToString(), true, out tempPayer))
                             {
                                 var actualPrimaryPayerValue = tempPayer; // Enum.Parse(typeof(PrimaryPayer), row["PrimaryPayer"].ToString());

                                 //switch (row["PrimaryPayer"].ToString().ToUpperInvariant())
                                 switch (actualPrimaryPayerValue.ToString().ToUpperInvariant())
                                 {
                                     case "EXCLUDE":
                                         primaryPayer = -1;
                                         break;
                                     case "MISSING":
                                         primaryPayer = 0;
                                         break;
                                     case "MEDICARE":
                                         primaryPayer = 1;
                                         break;
                                     case "MEDICAID":
                                         primaryPayer = 2;
                                         break;
                                     case "PRIVATE":
                                         primaryPayer = 3;
                                         break;
                                     case "SELFPAY":
                                         primaryPayer = 4;
                                         break;
                                     case "NOCHARGE":
                                         primaryPayer = 5;
                                         break;
                                     case "OTHER":
                                         primaryPayer = 6;
                                         break;
                                     case "RETAIN":
                                         primaryPayer = 99;
                                         break;
                                     default:
                                         primaryPayer = 6;
                                         break;
                                 }
                             }
                             else
                                 primaryPayer = 6;

                             grouperRecord.PrimaryPayer = primaryPayer;
                         }

                         // principal Diagnostics
                         grouperRecord.PrimaryDiagnosis = row["PrincipalDiagnosis"].ToString();

                         // secondary diagnositics
                         for (var i = 1; i <= 24; i++)
                         {
                             var count = i + 1;
                             if (count > 24) break;

                             var diagnosisColumn = string.Format("DiagnosisCode{0}", count);
                             var diagnosis = row[diagnosisColumn];
                             if (diagnosis == null) continue;
                             grouperRecord.SecondaryDiagnosis.Add(diagnosis.ToString());
                         }

                         // principal procedure
                         if (row["PrincipalProcedure"] != null)
                         {
                             grouperRecord.PrincipalProcedure = row["PrincipalProcedure"].ToString();
                         }

                         // procedures
                         for (var i = 1; i <= 24; i++)
                         {
                             var count = i + 1;
                             if (count > 24) break;

                             var procedureColumn = string.Format("ProcedureCode{0}", count);
                             var procedure = row[procedureColumn];
                             if (procedure == null) continue;
                             grouperRecord.SecondaryProcedures.Add(procedure.ToString());
                         }

                         inputRecords.Enqueue(grouperRecord);
                         recordsToGroupCount++;

                         //if (recordsToGroupCount == 10000 || recordsCount == totalNumberOfRecords)
                         //{
                         //    batchedInputRecords.Add(new List<DatasetInput>(inputRecords));

                         //    recordsToGroupCount = 0;

                         //    inputRecords.Clear();
                         //}
                     }


                     //foreach (var record in inputRecords)
                     //{
                     var outputRecords = new ConcurrentQueue<DatasetOutput>();

                     using (var grouper = new Grouper.Grouper())
                     {
                        Parallel.ForEach(inputRecords, new ParallelOptions { MaxDegreeOfParallelism = MonahrqDiagnostic.LogicalCPUsCount }, record =>
                        //foreach(var record in inputRecords)
                        {
                             try
                             {
                             //GrouperInputRecord grouperRecord;
                                 var grouperRecord = new GrouperInputRecord
                                 {
                                     MedicalRecordNumber = record.OptionalInformation,
                                     OptionalInformation = record.OptionalInformation,
                                     AdmitDate = record.AdmitDate,
                                     Age = record.Age,
                                     BirthDate = record.BirthDate,
                                     DischargeDate = record.DischargeDate,
                                     DischargeStatus = record.DischargeStatus,
                                     PrimaryPayer = record.PrimaryPayer,
                                     LOS = record.LOS,
                                     Sex = record.Sex,
                                     PrimaryDiagnosis = record.PrimaryDiagnosis.ToLower(),
                                     AdmitDiagnosis = record.PrimaryDiagnosis.ToLower(),
                                     ApplyHACLogic = string.Empty
                                 };


                                 var diagnosisCount = 0;
                                 foreach (var diagnosis in record.SecondaryDiagnosis.ToList())
                                 {
                                     grouperRecord.SetSecondaryDiagnoses((diagnosisCount + 1), diagnosis);
                                     diagnosisCount++;
                                 }

                                 grouperRecord.PrincipalProcedure = record.PrincipalProcedure;
                                 var procedureCount = 0;
                                 Parallel.ForEach(record.SecondaryProcedures.ToList(), procedures =>
                                 {
                                     grouperRecord.SetSecondaryProcedures((procedureCount + 1), procedures);
                                     procedureCount++;
                                 });


                                 if (grouperRecord.IsValid())
                                 {
                                     grouper.AddRecordToBeGrouped(grouperRecord);

                                     //_notifyIcon.Text = string.Format("Monahrq - DRG/MDC {1}# of records are processing: {0}", recordsDisplayCount, Environment.NewLine);
                                     recordsDisplayCount++;
                                 }
                                 else
                                 {
                                     grouperRecord.Errors.ForEach(gRecord =>
                                     {
                                         _errorsDictionary.Add(new KeyValuePair<string, Exception>(string.Format("{0}:{1}", record.OptionalInformation, gRecord.PropertyName), gRecord));
                                     });
                                     //
                                 }
                             }
                             catch (Exception exc)
                             {
                                 Exception excToUse = exc.GetBaseException();
                                 if (exc is AggregateException)
                                 {
                                     excToUse = ((AggregateException) exc).Flatten().GetBaseException();
                                 }
                                 //LogMessage(string.Format("Error creating grouper input record (IP Dataset Row Id: {1}): {0}", excToUse.Message, record.OptionalInformation));
                                 _errorsDictionary.Add(new KeyValuePair<string, Exception>(string.Format("{0}:{1}", record.OptionalInformation, Guid.NewGuid().ToString().Substring(0, 8)), excToUse));
                             }
                         });



                         var grouperRunStartTime = DateTime.Now;
                         //_notifyIcon.Text = string.Format("Monahrq - DRG/MDC Start running Grouper @ {0}...", grouperRunStartTime.ToString("hh:mm:ss"), Environment.NewLine);
                         //_notifyIcon.Text = string.Format("Monahrq - {0} DRG/MDC Records ready for grouping:", recordsDisplayCount);

                         if (grouper.RunGrouper())
                         {
                             //_notifyIcon.Text =
                             //    string.Format("Monahrq - DRG/MDC {0}Records are finished processing...",
                             //                  Environment.NewLine);

                             var grouperRunEndTime = DateTime.Now;
                             var grouperOutputRecords = new ConcurrentQueue<GrouperOutputRecord>(grouper.GetGrouperOutputRecords());

                             if (grouperOutputRecords != null && grouperOutputRecords.Any())
                             {
                                 // _notifyIcon.Text = string.Format("Monahrq - DRG/MDC Grouper successfully ran (time taken: {0})...", grouperRunEndTime.Subtract(grouperRunStartTime));

                                 Parallel.ForEach(grouperOutputRecords, record2 =>
                                 //foreach (var record2 in grouperOutputRecords)
                                 {
                                     var outputDataset = new DatasetOutput();
                                     outputDataset.Id = long.Parse(record2.MedicalRecordNumber);
                                     outputDataset.DatasetId = _currentIpTargetImportId;
                                     outputDataset.DRG = record2.FinalDRG;
                                     outputDataset.MDC = record2.FinalMDC;

                                     if (!outputRecords.Any(c =>
                                                            c.Id == outputDataset.Id &&
                                                            c.DatasetId == outputDataset.DatasetId))
                                         outputRecords.Enqueue(outputDataset);
                                 });
                             }
                         }

                         // }

                         if (outputRecords != null && outputRecords.Any())
                         {
                            Parallel.ForEach(outputRecords, new ParallelOptions { MaxDegreeOfParallelism = MonahrqDiagnostic.LogicalCPUsCount }, record3 =>
                            //foreach(var record3 in outputRecords)
                            {
                                 try
                                 {
                                     var datasetUpdatedQuery = string.Format(@"update [dbo].[Targets_InpatientTargets]
                                                            set [DRG] = {3}, [MDC] = {4}
                                                            where [Id] = {2} 
                                                            and [{0}_Id] = {1};", typeof (Dataset).Name,
                                         _currentIpTargetImportId, record3.Id, record3.DRG,
                                         record3.MDC);

                                     // Update syncrounously OR asyncronously based on indexing task
                                     if (_isPerformingIndexingTask)
                                     {
                                         lock (_indexingLockObject)
                                         {
                                             var synchronousUpdateTask = Task.Factory.StartNew(() =>
                                             {
                                                 _dbHelper.ExecuteNonQuery(datasetUpdatedQuery);
                                                 recordsCount++;
                                             });
                                             synchronousUpdateTask.Wait(10);
                                         }
                                     }
                                     else
                                     {
                                         var asynchronousUpdateTask = Task.Factory.StartNew(() =>
                                         {
                                             _dbHelper.ExecuteNonQuery(datasetUpdatedQuery);
                                             recordsCount++;
                                         });

                                         asynchronousUpdateTask.Wait(10);
                                     }


                                    // Notify the user4
                                    if(datasetRowCount < 1000000)
                                        _notifyIcon.Text = string.Format("Monahrq - {0} out of {1} DRG/MDC records processed", recordsCount, datasetRowCount);
                                     //var notificationTask = Task.Factory.StartNew(() => _notifyIcon.Text = string.Format("Monahrq - {0} out of {1} DRG/MDC records processed", recordsCount, datasetRowCount));
                                    // notificationTask.Wait(10);
                                 }
                                 catch (Exception exc)
                                 {
                                     var excToUse = exc.GetBaseException();
                                     if (exc is AggregateException)
                                     {
                                         excToUse = ((AggregateException) exc).Flatten().GetBaseException();
                                     }
                                     //LogMessage(string.Format("Error updating dataset record (Id: {1}): {0}", excToUse.Message, record3.Id));
                                     _errorsDictionary.Add(new KeyValuePair<string, Exception>(string.Format("{0}:{1}", record3.Id, Guid.NewGuid().ToString().Substring(0, 8)), excToUse));
                                 }
                             });

                         }
                     }

                     #endregion


                     // Exit the loop on the last iteration
                     if (inpatientDataTable.Rows.Count < numberOfItemsInPage)
                     {
                         loopState.Break();
                     }

                 });

                if (datasetRowCount >= 1500000)
                {
                    // Conditially perform indexing task
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            _isPerformingIndexingTask = true;
                            const string indexScript =
                                @"  IF NOT EXISTS(SELECT *  FROM sys.indexes WHERE name='IX_Targets_InpatientTargets_DRG_MDC_Dataset_Id' AND object_id = OBJECT_ID('Targets_InpatientTargets'))
                                        BEGIN
	                                        CREATE INDEX IX_Targets_InpatientTargets_DRG_MDC_Dataset_Id ON [Targets_InpatientTargets] (DRG, MDC, Dataset_Id)
                                        END
                                        ELSE
	                                        BEGIN
		                                        DBCC DBREINDEX (Targets_InpatientTargets, IX_Targets_InpatientTargets_DRG_MDC_Dataset_Id, 80);
	                                        END
                                ";
                            _dbHelper.ExecuteNonQuery(indexScript);
                        }
                        catch (Exception exc)
                        {
                            var excToUse = exc.GetBaseException();
                            if (exc is AggregateException)
                            {
                                var message = string.Empty;
                                foreach (var exception in ((AggregateException)exc).InnerExceptions.ToList())
                                {
                                    if (exception == null) continue;
                                    message += exception.Message;
                                    LogMessage(message);
                                }
                                message = null;
                            }
                            else
                            {
                                LogMessage(excToUse.Message);
                            }
                        }
                    });
                }

                if (_errorsDictionary != null && _errorsDictionary.Count > 0)
                {
                    foreach (var entry in _errorsDictionary)
                    {
                        var rowId = entry.Key.Split(':')[0];

                        if (entry.Value is GrouperRecordException)
                            LogMessage(string.Format("Error creating grouper input record (IP Dataset Row Id: {0}): {1}", rowId, entry.Value.GetBaseException().Message));
                        else
                            LogMessage(string.Format("Error while processing dataset record (Id: {1}): {0}", rowId, entry.Value.GetBaseException().Message));
                    }
                }

                // 
                UpdateStatus(DrgMdcMappingStatusEnum.Completed, "DRG/MDC Mapping successfully completed.");

                _notifyIcon.ShowBalloonTip(2000, "Monahrq: DRG/MDC Mapping successfully completed",
                                                 string.Format("{3} Data: \"{0}\" Successfully processed.{1}# of records processed successfully: {2}", 
                                                 dataset.DatasetName, Environment.NewLine, recordsCount, dataset.TypeName), ToolTipIcon.Info);
            }
            catch (Exception exc)
            {
                _notifyIcon.ShowBalloonTip(3000, "Monahrq: DRG/MDC Mapping Processing",
                                           "An unexpected error occurred.", ToolTipIcon.Error);
                UpdateStatus(DrgMdcMappingStatusEnum.Error, "An unexpected error occurred.");

                var excToUse = exc.GetBaseException();
                if (exc is AggregateException)
                {
                    var message = string.Empty;
                    foreach (var exception in ((AggregateException)exc).InnerExceptions.ToList())
                    {
                        if(exception == null) continue;
                        message += exception.Message;
                        LogMessage(message);
                    }
                    message = null;
                }
                else
                {
                    LogMessage(excToUse.Message);
                }
                
            }
            finally
            {
                Grouper.Grouper.CleanUpInputOutputFiles();

                if(!string.IsNullOrEmpty(_logFilePath))
                    Grouper.Grouper.CopyAndDeleteGrouperLogFiles(_logFilePath);

                Thread.Sleep(5000);
                ExitThreadCore();
            }

            #region Old Code
            //            try
            //            {
            //                var datasetQuery =
            //                    string.Format(@"select c.[Id], c.[File] 'Dataset_Name', c.[DRGMDCMappingStatus], ct.[Name] 'Type_Name' 
            //                                                   from {0} c left outer join ContentTypeRecords ct on ct.Id = c.ContentType_Id 
            //                                                   where c.[Id] = {1} and c.[IsFinished] = 1 
            //                                                   and c.[DRGMDCMappingStatus] not in ('Completed','InProgress')",
            //                                  TABLE_SCHEMA_NAME, _currentIpTargetImportId);

            //                DatasetImport dataset = null;

            //                var dataSetItemTable = _dbHelper.ExecuteDataTable(datasetQuery);

            //                if (dataSetItemTable != null && dataSetItemTable.Rows.Count > 0)
            //                {
            //                    dataset = new DatasetImport();
            //                    foreach (DataRow item in dataSetItemTable.Rows)
            //                    {
            //                        dataset.Id = int.Parse(item[0].ToString());
            //                        dataset.DatasetName = item[1].ToString();
            //                        dataset.Status =
            //                            (DrgMdcMappingStatus) Enum.Parse(typeof (DrgMdcMappingStatus), item[2].ToString());
            //                        dataset.TypeName = item[3].ToString();
            //                    }

            //                }

            //                if (dataset == null)
            //                    throw new InvalidOperationException("No Inpatient dataset found with the Id: " +
            //                                                        _currentIpTargetImportId);

            //                if (dataset.Status == DrgMdcMappingStatus.Completed)
            //                    throw new InvalidOperationException(
            //                        string.Format(
            //                            "Inpatient dataset \"{0}\" can't be processed as it has already successfully completed DRG/MDC mapping process."));

            //                UpdateStatus(DrgMdcMappingStatus.InProgress, null);

            //                var impatientDatasetQuery = string.Format(@"select * from [dbo].[Targets_InpatientTargets] 
            //                                                            where [{1}_Id] = {0} 
            //                                                            and [DRG] is null 
            //                                                            and [MDC] is null",
            //                                                          _currentIpTargetImportId, typeof (Dataset).Name);

            //                DataTable inpatientDataSet = _dbHelper.ExecuteDataTable(impatientDatasetQuery);


            //                if (inpatientDataSet == null)
            //                    throw new Exception("Something went wrong, for no records could be found.");

            //                // var totalNumberOfRecords = (inpatientDataSet.Rows.Count > 0) ? inpatientDataSet.Rows.Count : 750000;
            //                long recordsCount = 0, recordsDisplayCount = 0;

            //                #region Mock code

            //                //var totalNumberOfRecords = (inpatientDataSet.Rows.Count > 0) ? inpatientDataSet.Rows.Count : 750000;
            //                //long recordsCount = 0, itemsCount = 0;

            //                //while (totalNumberOfRecords > 0)
            //                //{
            //                //    Thread.Sleep(3);

            //                //    _notifyIcon.Text = string.Format("Monahrq - DRG/MDC Mapping{1}# of records processed: {0}", recordsCount, Environment.NewLine);
            //                //    if (itemsCount == 10000 || recordsCount == totalNumberOfRecords)
            //                //    {
            //                //        _notifyIcon.ShowBalloonTip(2000, "MONAHRQ: DRG/MDC Mapping Processing", string.Format("{3} dataset: \"{0}\"{1}# of records processed: {2}", dataset.DatasetName, Environment.NewLine, recordsCount, dataset.TypeName), ToolTipIcon.Info);
            //                //        //_notifyIcon.Visible = true;
            //                //        itemsCount = 0;
            //                //    }

            //                //    itemsCount++;
            //                //    recordsCount++;
            //                //    totalNumberOfRecords--;
            //                //}

            //                #endregion

            //                #region Grouper
            //                List<DatasetOutput> outputRecords = new List<DatasetOutput>();
            //                using (var grouper = new Grouper.Grouper())
            //                {
            //                    foreach (DataRow row in inpatientDataSet.Rows)
            //                    {
            //                        var grouperRecord = new GrouperInputRecord();

            //                        grouperRecord.OptionalInformation = row["Id"].ToString();
            //                        //grouperRecord.PatientName = row["PatientID"] != null ? row["PatientID"].ToString(): null;
            //                        //grouperRecord.MedicalRecordNumber = "";
            //                        //grouperRecord.AccountNumber = "";

            //                        if (row["AdmissionDate"] != null)
            //                        {
            //                            DateTime dtAdmissionDate;
            //                            if (DateTime.TryParse(row["AdmissionDate"].ToString(), out dtAdmissionDate))
            //                                if (dtAdmissionDate != DateTime.MinValue && dtAdmissionDate != DateTime.MaxValue)
            //                                    grouperRecord.AdmitDate = dtAdmissionDate;
            //                        }

            //                        if (row["DischargeDate"] != null)
            //                        {
            //                            DateTime dtDischargeDate;
            //                            if (DateTime.TryParse(row["DischargeDate"].ToString(), out dtDischargeDate))
            //                                if (dtDischargeDate != DateTime.MinValue && dtDischargeDate != DateTime.MaxValue)
            //                                    grouperRecord.DischargeDate = dtDischargeDate;
            //                        }

            //                        // TODO: finish
            //                        if (row["DischargeDisposition"] != null)
            //                        {
            //                            int? dischargeDisposition;
            //                            switch (row["DischargeDisposition"].ToString().ToUpper())
            //                            {
            //                                case "EXCLUDE":
            //                                    dischargeDisposition = -1;
            //                                    break;
            //                                case "MISSING":
            //                                    dischargeDisposition = null;
            //                                    break;
            //                                case "ROUTINE":
            //                                    dischargeDisposition = 1;
            //                                    break;
            //                                case "SHORTTERM":
            //                                    dischargeDisposition = 2;
            //                                    break;
            //                                case "NURSINGFACILITY":
            //                                    dischargeDisposition = 3;
            //                                    break;
            //                                case "INTERMEDIATECARE":
            //                                    dischargeDisposition = 4;
            //                                    break;
            //                                case "OTHERFACILITY":
            //                                    dischargeDisposition = 5;
            //                                    break;
            //                                case "HOMEHEALTHCARE":
            //                                    dischargeDisposition = 6;
            //                                    break;
            //                                case "AMA":
            //                                    dischargeDisposition = 7;
            //                                    break;
            //                                case "DECEASED":
            //                                    dischargeDisposition = 20;
            //                                    break;
            //                                case "DISCHARGEDALIVEDESTUNKNOWN":
            //                                    dischargeDisposition = 99;
            //                                    break;
            //                                default:
            //                                    dischargeDisposition = null;
            //                                    break;
            //                            }
            //                            grouperRecord.DischargeStatus = dischargeDisposition;
            //                        }


            //                        grouperRecord.PrimaryPayer = 1;
            //                        if (row["LengthOfStay"] != null)
            //                        {
            //                            int intLOS;
            //                            if (int.TryParse(row["LengthOfStay"].ToString(), out intLOS))
            //                                grouperRecord.LOS = intLOS;
            //                        }

            //                        if (row["BirthDate"] != null)
            //                        {
            //                            DateTime dtBirthDate;
            //                            if (DateTime.TryParse(row["BirthDate"].ToString(), out dtBirthDate))
            //                                if (dtBirthDate != DateTime.MinValue && dtBirthDate != DateTime.MaxValue)
            //                                    grouperRecord.BirthDate = dtBirthDate;
            //                        }

            //                        if (row["Age"] != null)
            //                        {
            //                            int intAge;
            //                            if (int.TryParse(row["Age"].ToString(), out intAge))
            //                                grouperRecord.Age = intAge;
            //                        }
            //                        if (row["Sex"] != null)
            //                        {
            //                            grouperRecord.Sex = row["Sex"].ToString().EqualsIgnoreCase("MALE") ? 1 : 2;
            //                        }
            //                        else
            //                        {
            //                            grouperRecord.Sex = 0;
            //                        }

            //                        // TODO: Optional?
            //                        //grouperRecord.AdmitDiagnosis = "";

            //                        // primary payer TODO: Correct this functionality Jason
            //                        int primaryPayer;
            //                        switch (row["PrimaryPayer"].ToString().ToUpperInvariant())
            //                        {
            //                            case "Exclude":
            //                                primaryPayer = -1;
            //                                break;
            //                            case "Missing":
            //                                primaryPayer = 0;
            //                                break;
            //                            case "Medicare":
            //                                primaryPayer = 1;
            //                                break;
            //                            case "Medicaid":
            //                                primaryPayer = 2;
            //                                break;
            //                            case "Private":
            //                                primaryPayer = 3;
            //                                break;
            //                            case "SelfPay":
            //                                primaryPayer = 4;
            //                                break;
            //                            case "NoCharge":
            //                                primaryPayer = 5;
            //                                break;
            //                            case "Other":
            //                                primaryPayer = 6;
            //                                break;
            //                            case "Retain":
            //                                primaryPayer = 99;
            //                                break;
            //                            default:
            //                                primaryPayer = 6;
            //                                break;
            //                        }

            //                        grouperRecord.PrimaryPayer = primaryPayer;

            //                        // principal Diagnostics
            //                        grouperRecord.PrimaryDiagnosis = row["PrincipalDiagnosis"].ToString();

            //                        // secondary diagnositics
            //                        for (var i = 1; i <= 24; i++)
            //                        {
            //                            if (row["DiagnosisCode" + (i + 1)] == null) continue;

            //                            var diagnosis = row["DiagnosisCode" + (i + 1)].ToString();
            //                            grouperRecord.SetSecondaryDiagnoses(i, diagnosis);
            //                        }

            //                        // principal procedure
            //                        if (row["PrincipalProcedure"] != null)
            //                        {
            //                            grouperRecord.PrincipalProcedure = row["PrincipalProcedure"].ToString();
            //                        }

            //                        // procedures
            //                        for (var i = 1; i <= 24; i++)
            //                        {
            //                            if (row["ProcedureCode" + (i + 1)] == null) continue;

            //                            var procedure = row["ProcedureCode" + (i + 1)].ToString();
            //                            grouperRecord.SetSecondaryProcedures(i, procedure);
            //                        }


            //                        if (grouperRecord.IsValid())
            //                        {
            //                            grouper.AddRecordToBeGrouped(grouperRecord);
            //                            recordsCount++;
            //                            recordsDisplayCount++;
            //                        }

            //                        // TODO: Flag to remove from database. - Jason & Jon
            //                        //else
            //                        //{
            //                        //    throw new InvalidGrouperRecordException("Grouper record {0}");
            //                        //}

            //                        _notifyIcon.Text = string.Format("Monahrq - DRG/MDC {1}# of records to be processed: {0}",
            //                                                         recordsCount, Environment.NewLine);


            //                        if (recordsDisplayCount == 10000)
            //                        {
            //                            _notifyIcon.ShowBalloonTip(2000, "Monahrq: DRG/MDC Mapping Processing",
            //                                                   string.Format("Monahrq - DRG/MDC {1}# of records to be processed: {0}", recordsCount, Environment.NewLine),
            //                                                   ToolTipIcon.Info);
            //                            recordsDisplayCount = 0;
            //                        }

            //                    }

            //                    // _notifyIcon.Text = string.Format("Monahrq - DRG/MDC {1}# of records currently processed: {0}", recordsCount, Environment.NewLine);

            //                    _notifyIcon.Text = string.Format("Monahrq - DRG/MDC {0}Records being processed now...", Environment.NewLine);
            //                    if (grouper.RunGrouper())
            //                    {
            //                        _notifyIcon.Text = string.Format("Monahrq - DRG/MDC {0}Records are finished processing...", Environment.NewLine);


            //                        IEnumerable<GrouperOutputRecord> grouperOutputRecords = grouper.GetGrouperOutputRecords().ToList();

            //                        if (grouperOutputRecords != null && grouperOutputRecords.Any())
            //                        {
            //                            _notifyIcon.Text = string.Format("Monahrq - DRG/MDC {0}Retrieving results...", Environment.NewLine);
            //                            _notifyIcon.ShowBalloonTip(2000, "Monahrq: DRG/MDC Mapping Processing",
            //                                                   string.Format("Monahrq - DRG/MDC {0}Records are finished processing. Retrieving results...", Environment.NewLine),
            //                                                   ToolTipIcon.Info);
            //                            foreach (var record in grouperOutputRecords)
            //                            {
            //                                var outputDataset = new DatasetOutput();
            //                                outputDataset.Id = int.Parse(record.OptionalInformation);
            //                                outputDataset.DatasetId = _currentIpTargetImportId;
            //                                outputDataset.DRG = record.FinalDRG;
            //                                outputDataset.MDC = record.FinalMDC;

            //                                if (!outputRecords.Any(c =>
            //                                                       c.Id == outputDataset.Id &&
            //                                                       c.DatasetId == outputDataset.DatasetId))
            //                                    outputRecords.Add(outputDataset);
            //                            }
            //                        }
            //                    }
            //                }

            //                if (outputRecords != null && outputRecords.Any())
            //                {
            //                    foreach (var record in outputRecords)
            //                    {
            //                        var datasetUpdatedQuery = string.Format(@"update [dbo].[Targets_InpatientTargets]
            //                                                            set [DRG] = {3}, [MDC] = {4}
            //                                                            where [Id] = {2} 
            //                                                            and [{0}_Id] = {1};", typeof (Dataset).Name,
            //                                                                _currentIpTargetImportId, record.Id, record.DRG,
            //                                                                record.MDC);

            //                        _dbHelper.ExecuteNonQuery(datasetUpdatedQuery);
            //                    }

            //                    _notifyIcon.ShowBalloonTip(3000, "Monahrq: DRG/MDC Mapping Processing",
            //                                               string.Format(
            //                                                   "{3} Data: \"{0}\" Successfully processed.{1}# of records processed successfully: {2}",
            //                                                   dataset.DatasetName, Environment.NewLine, recordsCount,
            //                                                   dataset.TypeName),
            //                                               ToolTipIcon.Info);
            //                    UpdateStatus(DrgMdcMappingStatus.Completed, "DRG/MDC Mapping successfully completed.");

            //                }
            //                else
            //                {
            //                    _notifyIcon.ShowBalloonTip(3000, "Monahrq: DRG/MDC Mapping Processing",
            //                                               string.Format(
            //                                                   "{3} Data: \"{0}\" Successfully processed.{1}# of records processed successfully: {2}",
            //                                                   dataset.DatasetName, Environment.NewLine, 0, dataset.TypeName),
            //                                               ToolTipIcon.Info);

            //                    UpdateStatus(DrgMdcMappingStatus.Error,
            //                                 "DRG/MDC Mapping successfully completed, but some unexpected occurred with the DRG/MDC Mapping Grouper.");
            //                }

            //                #endregion


            //                //_notifyIcon.ShowBalloonTip(2000, "Monahrq: DRG/MDC Mapping Processing",
            //                //                           string.Format(
            //                //                               "{3} Data: \"{0}\" Successfully processed.{1}# of records processed successfully: {2}",
            //                //                               dataset.DatasetName, Environment.NewLine, recordsCount, dataset.TypeName),
            //                //                           ToolTipIcon.Info);
            //            }
            //            catch(Exception exc)
            //            {
            //                _notifyIcon.ShowBalloonTip(3000, "Monahrq: DRG/MDC Mapping Processing",
            //                                           "An unexpected error occurred.", ToolTipIcon.Error);
            //                UpdateStatus(DrgMdcMappingStatus.Error, "An unexpected error occurred.");
            //                LogExceptions(exc);
            //            }
            //            finally
            //            {
            //                Thread.Sleep(5000);
            //                ExitThreadCore();
            //            }
            #endregion
        }

        /// <summary>
        /// The file lock
        /// </summary>
        private static readonly object[] _fileLock = { };
        //private static readonly ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">The message.</param>
        private static async void LogMessage(string message)
        {
            //_readWriteLock.EnterWriteLock();

            //try
            //{
                var filePath = string.IsNullOrEmpty(_logFilePath)
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SYSTRAY_LOGFILE_NAME)
                    : Path.Combine(_logFilePath, SYSTRAY_LOGFILE_NAME);

                //lock (_fileLock)
                //{
                using (var file = new StreamWriter(filePath, true))
                {
                    await file.WriteLineAsync(message);

                    file.Close();
                }
                //}
            //}
            //finally
            //{
            //    _readWriteLock.ExitReadLock();
            //}
        }

        /// <summary>
        /// Updates the status.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="statusMessage">The status message.</param>
        public void UpdateStatus(DrgMdcMappingStatusEnum status, string statusMessage)
        {
            var updateStatusScript = string.Format(@"UPDATE {0} SET {1}= '{2}' WHERE [Id] = {3}", TABLE_SCHEMA_NAME,
                                                   TABLE_STATUS_COLUMN_NAME, status.ToString(), _currentIpTargetImportId);

            if (!string.IsNullOrEmpty(statusMessage))
            {
                updateStatusScript = string.Format(@"UPDATE {0} SET {1}= '{2}', {4}= '{5}' WHERE [Id] = {3}", TABLE_SCHEMA_NAME,
                                                   TABLE_STATUS_COLUMN_NAME, status.ToString(), _currentIpTargetImportId, TABLE_STATUS_MESSAGE_COLUMN_NAME, statusMessage);
            }

            _dbHelper.ExecuteNonQuery(updateStatusScript);
        }

        /// <summary>
        /// Gets the application icon.
        /// </summary>
        /// <returns></returns>
        private Icon GetApplicationIcon()
        {
            Icon applicationIcon;
            using (var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Monahrq.SysTray.trayNotification.Butterfly.ico"))
            {
                applicationIcon = new Icon(iconStream);
            }
            return applicationIcon;
        }

        //[Import]
        //IDomainSessionFactoryProvider SessionProvider { get; set; }

        //[Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        //public ILoggerFacade Logger { get; private set; }

        //void displayForm_Click(object sender, EventArgs e)
        //{
        //    //throw new Exception("The method or operation is not implemented.");
        //}

        void exitApplication_Click(object sender, EventArgs e)
        {
            //Call our overridden exit thread core method!
            _exitImmediately = true;
            ExitThreadCore();
        }

        /// <summary>
        /// Terminates the message loop of the thread.
        /// </summary>
        protected override void ExitThreadCore()
        {
            //Clean up any references needed
            //At this time we do not have any
            _dbHelper = null;

            _notifyIcon.Dispose();

            //Call the base method to exit the application
            //base.ExitThreadCore();

            Environment.Exit(0);
        }


        /// <summary>
        /// Class for DatasetImport
        /// </summary>
        class DatasetImport
        {
            public int Id;
            public string DatasetName;
            public DrgMdcMappingStatusEnum Status;
            public string TypeName;
        }

        /// <summary>
        /// Class for  DatasetInput
        /// </summary>
        class DatasetInput
        {
            public string OptionalInformation;
            public int DatasetId;

            public DateTime? AdmitDate;
            public DateTime? DischargeDate;

            public int? DischargeStatus;

            public int? PrimaryPayer;
            public int? LOS;
            public DateTime? BirthDate;
            public int? Age;
            public int? Sex;
            public string PrimaryDiagnosis;

            public readonly List<string> SecondaryDiagnosis = new List<string>();

            public string PrincipalProcedure;
            public readonly List<string> SecondaryProcedures = new List<string>();
        }

        /// <summary>
        /// Class for DatasetOutput
        /// </summary>
        class DatasetOutput
        {
            public long Id;
            public int DatasetId;

            public int? DRG;
            public int? MDC;
        }
    }

    /// <summary>
    /// Class for InvalidGrouperRecordException
    /// </summary>
    /// <seealso cref="System.Exception" />
    internal class InvalidGrouperRecordException : Exception
    {
        public InvalidGrouperRecordException(string message)
            : base(message)
        {

        }
    }
}
