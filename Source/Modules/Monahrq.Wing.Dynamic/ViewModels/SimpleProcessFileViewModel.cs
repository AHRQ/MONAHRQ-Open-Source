using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Dynamic.Models;
using NHibernate;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.Wing.Dynamic.ViewModels
{
    /// <summary>
    /// File process class
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.Wing.Dynamic.Models.WizardContext}" />
    [ImplementPropertyChanged]
    public class SimpleProcessFileViewModel : WizardStepViewModelBase<WizardContext>
    {

        /// <summary>
        /// The session
        /// </summary>
        ISession _session;
        /// <summary>
        /// The import completed countdown event
        /// </summary>
        CountdownEvent _importCompletedCountdownEvent;
        /// <summary>
        /// The Cancellation  token source
        /// </summary>
        private CancellationTokenSource _cts;
        /// <summary>
        /// The total file lines
        /// </summary>
        private int _totalFileLines;

        /// <summary>
        /// The dynamic wing target data table
        /// </summary>
        protected DataTable DynamicWingTargetDataTable;
        /// <summary>
        /// The dynamic wing data columns
        /// </summary>
        protected List<DataColumn> DynamicWingDataColumns; 

        //protected const string DB_COLUMN_ID = "Id";
        //protected const string DB_COLUMN_DRG_ID = "DRG_Id";
        //protected const string DB_COLUMN_DRG = "DRG";
        //protected const string DB_COLUMN_PROVIDER_ID = "Provider_Id";
        //protected const string DB_COLUMN_PROVIDER_STATE_ABBREV = "Provider_StateAbbrev";
        //protected const string DB_COLUMN_TOTAL_DISCHARGES = "TotalDischarges";
        //protected const string DB_COLUMN_AVERAGE_COVERED_CHARGES = "AverageCoveredCharges";
        //protected const string DB_COLUMN_AVERAGE_TOTAL_PAYMENTS = "AverageTotalPayments";
        //protected const string DB_COLUMN_CONTENT_ITEM_RECORD_ID = "ContentItemRecord_Id";

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleProcessFileViewModel"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public SimpleProcessFileViewModel(WizardContext context)
            : base(context)
        {
            Done = false;
            DynamicWingTargetDataTable = new DataTable(DataContextObject.CustomTarget.DbSchemaName)
            {

                TableName = DataContextObject.CustomTarget.DbSchemaName
            };
            DynamicWingDataColumns = new List<DataColumn>();
        }

        protected virtual int BatchSize { get { return 1000; }}

        /// <summary>
        /// Starts the import.
        /// </summary>
        public async void StartImport()
        {
            _importCompletedCountdownEvent = new CountdownEvent(1);

            _cts = new CancellationTokenSource();
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<WizardCancelEvent>().Subscribe(e => Cancelled(this, EventArgs.Empty));

            _session = DataContextObject.Provider.SessionFactory.OpenSession();

            try
            {
                await ImportFileAsync(_cts.Token);
                // WPF CommandManager periodically calls IsValid to see if the Next/Done button should be enabled. 
                // In multi-threaded wizard steps, IsValid returns the value of the Done flag. Call InvalidateRequerySuggested here
                // on the UI thread after setting the Done flag to force WPF to call IsValid now so the Next/Done button will become enabled. 
                CommandManager.InvalidateRequerySuggested();
                ImportCompleted(true);
            }
            catch (OperationCanceledException)
            {
               ImportCompleted(false);
               MessageBox.Show(AppendLog("Import cancelled."));
            }
            finally
            {
                _cts.Dispose();
            }
        }

        /// <summary>
        /// Cancels the file procssing if import completed count is greater than zero.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void Cancelled(object sender, EventArgs e)
        {
            // don't use cancellation token if user cancelled too late
            if (_importCompletedCountdownEvent.CurrentCount > 0)
                _cts.Cancel();
        }

        // overloads to handle each type of target, and the ImportType, and ImportTypeRecord...
        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected virtual void Insert(Dataset entity)
        {
            //_session.SaveOrUpdate(entity);
        }

        // overloads to handle each type of target, and the ImportType, and ImportTypeRecord...
        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="targetDateTable">The target date table.</param>
        protected void BulkInsert(DataTable targetDateTable)
        {
            var targetTableName = string.Format("dbo.{0}", DataContextObject.CustomTarget.DbSchemaName);

            // Open a sourceConnection to the AdventureWorks database. 
            using (var connection = new SqlConnection(_session.Connection.ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = targetTableName;
                        bulkCopy.BatchSize = BatchSize;
                        bulkCopy.BulkCopyTimeout = 10000;

                        foreach (DataColumn column in targetDateTable.Columns)
                            bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName,
                                                                                     column.ColumnName));

                        // Write from the source to the destination.
                        bulkCopy.WriteToServer(targetDateTable);
                    }

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Saves the import entry.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected void SaveImportEntry(Dataset entity)
        {
            _session.SaveOrUpdate(entity);
        }

        ///// <summary>
        ///// Anies the specified provider identifier.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="providerId">The provider identifier.</param>
        ///// <param name="drg">The DRG.</param>
        ///// <returns></returns>
        //protected bool Any<T>(string providerId , string drg) where T:MedicareProviderChargeTarget
        //{
        //    return _session.Query<T>()
        //                   .Any(m => m.ProviderId.ToLower() == providerId.ToLower() && m.DRG.ToLower() == drg.ToLower());
        //}

        /// <summary>
        /// Queries for single record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="querySpec">The query spec.</param>
        /// <returns></returns>
        protected T QueryForSingle<T>(Expression<Func<T, bool>> querySpec) where T : class, IEntity
        {
            if (querySpec == null)
            {
                return default(T);
            }

            return _session.Query<T>().FirstOrDefault(querySpec);
        }

        /// <summary>
        /// Saves the import type record.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected void SaveImportTypeRecord(Target entity)
        {
            _session.SaveOrUpdate(entity);
        }

        /// <summary>
        /// Gets or sets a value indicating whether file processing is done or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if file processing is done; otherwise, <c>false</c>.
        /// </value>
        public bool Done { get; set; }

        List<string> _logFile;
        string _headerLine;
        private Func<string, bool> _lineFunction;

        /// <summary>
        /// Initializes the specified header line.
        /// </summary>
        /// <param name="headerLine">The header line.</param>
        /// <param name="lineFunction">The line function.</param>
        public void Initialize(string headerLine, Func<string,bool> lineFunction)
        {
            _headerLine = headerLine;
            _lineFunction = lineFunction;
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public override string DisplayName
        {
            get { return "Import Data"; }
        }

        /// <summary>
        /// Determines whether this instance is valid.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return Done;
        }

        /// <summary>
        /// Imports the file asynchronous.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        async Task<int> ImportFileAsync(CancellationToken ct)
        {
              return await Task.Run(() =>
                {
                    try
                    {
                        _logFile = new List<string>();

                        if (null == DataContextObject || 
                            null == DataContextObject.File)
                        {
                            AppendLog(@"No file to import !");
                            return 0 ;
                        }

                        if (CountLines() < 1)
                        {
                            ImportCompleted(false);
                            MessageBox.Show(AppendLog("Input file(s) appears to be empty."), "Error importing file", MessageBoxButton.OK, MessageBoxImage.Error);
                            return 0;
                        }

                        AppendLog("Starting import: " + DataContextObject.File );
                        ProcessFile(DataContextObject.File);

                        //if(!string.IsNullOrEmpty(DataContextObject.CustomTarget.ImportSQLScript) )
                        BulkInsert(DynamicWingTargetDataTable);
                    }
                    catch (Exception e)
                    {
                       ImportCompleted(false);
                       MessageBox.Show(AppendLog("Error importing file: " + e.Message), "Error importing file");
                       return 0;
                    }

                    return 1;
                }, ct);
        }

        // read the file once (without processing) to count lines for progress bar
        /// <summary>
        /// Counts the lines.
        /// </summary>
        /// <returns></returns>
        private  int CountLines()
        {
            if (null == DataContextObject || null == DataContextObject.File) return 0;

            _totalFileLines += DataContextObject.File.LinesCount() - 1;
            AppendLog(string.Format("Finished scanning import file [{0}]. Number of lines: {1}",
                                    Path.GetFileName(DataContextObject.File.FileName),
                                    _totalFileLines));

            AppendLog(string.Format("Finished scanning import file(s). Total lines: {0}", _totalFileLines));
            return _totalFileLines;
        }

        /// <summary>
        /// Processes the file.
        /// </summary>
        /// <param name="fileProgress">The file progress.</param>
        private void ProcessFile(FileProgress fileProgress)
        {
            try
            {
                AppendLogLinesDone(0);

                string sqlStatement = string.Format("SELECT TOP 0 * FROM [dbo].[{0}] ", DataContextObject.CustomTarget.DbSchemaName);
                // Dummy select to return 0 rows.
                using (SqlDataAdapter sqlDa = new SqlDataAdapter(sqlStatement, _session.Connection as SqlConnection))
                {
                    sqlDa.Fill(DynamicWingTargetDataTable);
                }

                var headerItems = DynamicWingTargetDataTable.Columns.OfType<DataColumn>()
                    .Where(dc => !dc.ColumnName.EqualsIgnoreCase("Id") && !dc.ColumnName.EqualsIgnoreCase("Dataset_id"))
                    .Select(dc => dc.ColumnName)
                    .ToArray();

                using (var fs = new FileStream(fileProgress.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var inputFile = new StreamReader(fs))
                    {
                        // search for the header line
                        string inputLine;
                        var foundHeader = false;
                        while ((inputLine = inputFile.ReadLine()) != null)
                        {
                            fileProgress.LinesDone++;
                            fileProgress.PercentComplete = (fileProgress.LinesDone*100)/fileProgress.TotalLines;
                            AppendLogLinesDone(fileProgress.LinesDone);

                            if (string.IsNullOrEmpty(_headerLine))
                            {
                                if (headerItems.Any())
                                {
                                    _headerLine = string.Join(",", headerItems);
                                }
                            }
                            else
                            {
                                //_headerLine = Regex.Replace(_headerLine, "Id, ", string.Empty);
                                //_headerLine = Regex.Replace(_headerLine, ", Dataset_id", string.Empty);

                                _headerLine = _headerLine.Replace(" ", null); 
                                    //_headerLine.Replace("Id, ", null)
                                    //                     .Replace(", Dataset_id", null)
                                    //                     .Replace(" ", null);
                            }

                            if (
                                inputLine.Replace(" ", null)
                                         .ToUpperInvariant()
                                         .StartsWith(_headerLine.ToUpperInvariant()))
                            {


                                //var headerColumns = new List<DataColumn>();

                                //foreach (var line in _headerLine.Split(',').ToList())
                                //{
                                //    //DataContextObject.CustomTarget.Elements = DataContextObject.CustomTarget.Elements.ToList();

                                //    var element = DataContextObject.CustomTarget
                                //                                   .Elements.OrderBy(x => x.Ordinal)
                                //                                   .FirstOrDefault(e => e.Name.ToUpper() == line.ToUpper());

                                //    if(element == null) continue;

                                //    var column = new DataColumn(element.Name, typeof (int)) {AllowDBNull = element.};
                                //}

                                //DynamicWingTargetDataTable.Columns.Clear();
                                //DynamicWingTargetDataTable.Columns.AddRange(DynamicWingDataColumns.ToArray());
                                // Found the file token in the header. Ready to start import.
                                foundHeader = true;
                                break;
                            }

                            if (fileProgress.LinesDone > 50)
                            {
                                break;
                            }
                        }

                        if (!foundHeader)
                        {
                            // Didn't find file token in input file before EOF
                            MessageBox.Show(
                                AppendLog(
                                    string.Format(
                                        "The input file [{0}] does not appear to be of the correct file type.",
                                        fileProgress.FileName)), "Error importing file");
                            return;
                        }

                        AppendLog(string.Format("Header row found. Continuing from line {0}.", fileProgress.LinesDone));

                        // save owner before inserting all rows
                        //if (DataContextObject.CurrentImportType == null)
                        //{
                        //    DataContextObject.CurrentImportType = new ContentTypeRecord
                        //        {
                        //            Name = DataContextObject.SelectedDataType.DataTypeName
                        //        };
                        //    SaveImportTypeRecord(DataContextObject.CurrentImportType);
                        //}

                        // import each line
                        var lineCount = 0;
                        while ((inputLine = inputFile.ReadLine()) != null)
                        {
                            try
                            {
                                if (_lineFunction(inputLine))
                                {
                                    fileProgress.LinesProcessed++;
                                    lineCount++;
                                }
                                //else 
                                //    fileProgress.LinesDuplicated++;
                                if (lineCount == BatchSize || _totalFileLines == fileProgress.LinesProcessed)
                                {
                                    if (!string.IsNullOrEmpty(base.DataContextObject.CustomTarget.ImportSQLScript))
                                    {
                                        InsertViaCustomImportSQLScript(DynamicWingTargetDataTable);
                                    }
                                    else
                                    {
                                        BulkInsert(DynamicWingTargetDataTable);
                                    }

                                    DynamicWingTargetDataTable.Rows.Clear();
                                    lineCount = 0;
                                }
                            }
                            catch (Exception e)
                            {
                                fileProgress.LinesErrors++;
                                AppendLog("Error : " + e.Message);
                            }
                            finally
                            {
                                fileProgress.LinesDone++;
                                AppendLogLinesDone(fileProgress.LinesDone);
                            }
                        }
                    }
                }

                AppendLog(fileProgress.LinesDone + " lines processed.");
            }
            catch (Exception e)
            {
                MessageBox.Show(AppendLog("Error : " + e.Message),
                                string.Format("Error importing file [{0}]", fileProgress.FileName));
            }
            finally
            {
                //this.
            }
        }

        /// <summary>
        /// Inserts the via custom import SQL script.
        /// </summary>
        /// <param name="targetDataTable">The target data table.</param>
        private void InsertViaCustomImportSQLScript(DataTable targetDataTable)
        {
            var insertquery = new StringBuilder();

            if(DynamicWingDataColumns == null || !DynamicWingDataColumns.Any())
                DynamicWingDataColumns = targetDataTable.Columns.OfType<DataColumn>().Where(dc => dc.ColumnName.ToUpper() != "ID").ToList();

            foreach (DataRow row in targetDataTable.Rows)
            {
                var script = DynamicWingDataColumns.Aggregate(DataContextObject.CustomTarget.ImportSQLScript,
                                                              (current, dynamicWingDataColumn) =>
                                                              current.Replace("@@" + dynamicWingDataColumn.ColumnName + "@@",
                                                                              row[dynamicWingDataColumn.ColumnName].ToString()));
                insertquery.AppendLine(script);
            }

            using (var trans = _session.BeginTransaction())
            {
                _session.CreateSQLQuery(insertquery.ToString())
                        .ExecuteUpdate();

                trans.Commit();
            }
        }

        #region LOGGING 

        /// <summary>
        /// Logging the stats once import is completed.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        protected void ImportCompleted(bool success)
        {
            // signal import complete so cancellation token won't be used if user cancelled too late
            _importCompletedCountdownEvent.Signal();

            try
            {
                if (success)
                {
                    var rootXml = new XElement("LogLines");
                    foreach (string line in _logFile)
                    {
                        rootXml.Add(new XElement("LogLine", line));
                    }
                    DataContextObject.Summary = rootXml.ToString();

                    if (DataContextObject.DatasetItem.IsReImport)
                    {
                        var linesImported = _session.CreateSQLQuery(string.Format("select count(o.Id) from {0} o where o.Dataset_Id={1}", DataContextObject.CustomTarget.DbSchemaName, DataContextObject.DatasetItem.Id))
                                                    .UniqueResult<int>();

                        if (DataContextObject.DatasetItem.File.Contains(" (#"))
                            DataContextObject.DatasetItem.File = DataContextObject.DatasetItem.File.SubStrBefore(" (#");

                        DataContextObject.DatasetItem.File += " (# Rows Imported: " + linesImported + ")";
                    }

                    DataContextObject.Finish();
                    AppendLog("Import completed successfully.");
                }
                else
                {
                    AppendLog("Import was not completed successfully.");
                }

                if (_session.IsOpen)
                {
                    _session.Close();
                }
                Done = true;
            }
            catch (Exception)
            {
                Done = true;
            }
            finally
            {
                _session.Dispose();
            }
        }

        /// <summary>
        /// Appends the log lines done.
        /// </summary>
        /// <param name="linesDone">The lines done.</param>
        void AppendLogLinesDone(int linesDone)
        {
            // Update the count of lines processed in the logfile.
            if (_logFile.Last().StartsWith("Lines Loaded:"))
            {
                _logFile.RemoveAt(_logFile.Count() - 1);
            }
            _logFile.Add("Lines Loaded: " + linesDone);
        }

        /// <summary>
        /// Appends the log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected string AppendLog(string message)
        {
            _logFile.Add(message);
            return message;
        }

        #endregion

        #region DATA_CONVERTERS

        /// <summary>
        /// Gets the float from string.
        /// </summary>
        /// <param name="strFloat">The string float.</param>
        /// <returns></returns>
        protected float? GetFloatFromString(string strFloat)
        {
            float tempFloat;
            return float.TryParse(strFloat, out tempFloat) ? (float?) tempFloat : null;
        }

        /// <summary>
        /// Gets the double from string.
        /// </summary>
        /// <param name="strDouble">The string double.</param>
        /// <returns></returns>
        protected double? GetDoubleFromString(string strDouble)
        {
            double tempDouble;
            return double.TryParse(strDouble, out tempDouble) ? (double?)tempDouble : null;
        }

        /// <summary>
        /// Gets the int from string.
        /// </summary>
        /// <param name="strFloat">The string float.</param>
        /// <returns></returns>
        protected int? GetIntFromString(string strFloat)
        {
            int tempInt;
            return int.TryParse(strFloat, out tempInt) ? (int?) tempInt : null;
        }

        #endregion
    }
}
