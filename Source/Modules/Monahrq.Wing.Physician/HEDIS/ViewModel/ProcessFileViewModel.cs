using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
using Monahrq.Wing.Physician.HEDIS.Model;
using NHibernate;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.Wing.Physician.HEDIS.ViewModel
{
    [ImplementPropertyChanged]
    public class ProcessFileViewModel : WizardStepViewModelBase<WizardContext>
    {
        private const int BATCH_SIZE = 5000;
        ISession _session;
        CountdownEvent _importCompletedCountdownEvent;
        private CancellationTokenSource _cts;

        protected DataTable MedicalPracticeHedisDataTable;
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessFileViewModel"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ProcessFileViewModel(WizardContext context)
            : base(context)
        {
            Done = false;
            MedicalPracticeHedisDataTable = new DataTable(typeof(HEDISTarget).EntityTableName())
            {
                TableName = typeof(HEDISTarget).EntityTableName()
            };
        }

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
        /// Cancelleds the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void Cancelled(object sender, EventArgs e)
        {
            if (DataContextObject.DatasetItem.IsFinished)
            {
                DataContextObject.DatasetItem.IsFinished = false;
                using (var session = DataContextObject.Provider.SessionFactory.OpenSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        session.SaveOrUpdate(DataContextObject.DatasetItem);
                        trans.Commit();
                    }  
                }
            }

            // don't use cancellation token if user cancelled too late
            if (_importCompletedCountdownEvent.CurrentCount > 0)
                _cts.Cancel();
        }

        // overloads to handle each type of target, and the ImportType, and ImportTypeRecord...
        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected void Insert(DatasetRecord entity)
        {
            _session.SaveOrUpdate(entity);
        }

        // overloads to handle each type of target, and the ImportType, and ImportTypeRecord...
        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="targetDateTable">The target date table.</param>
        protected void BulkInsert(DataTable targetDateTable)
        {
            var targetTableName = string.Format("dbo.{0}", typeof (HEDISTarget).EntityTableName());

            // Open a sourceConnection to the AdventureWorks database. 
            using (var connection = new SqlConnection(_session.Connection.ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = targetTableName;
                        bulkCopy.BatchSize = BATCH_SIZE;
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

        /// <summary>
        /// Anies the specified provider identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="phyNpi">The phy npi.</param>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <returns></returns>
        protected bool Any<T>(long phyNpi, int datasetId) where T: HEDISTarget
        {
            if (_session.Query<T>().Any())
                return _session.Query<T>().Any(m => m.PhyNpi == phyNpi && m.Dataset.Id == datasetId);

            return false;
        }

        /// <summary>
        /// Queries for single.
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
        /// Gets or sets a value indicating whether [done].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [done]; otherwise, <c>false</c>.
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
                        //BulkInsert(MedicareProviderDataTable);
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

        public int TotalLineCount { get; set; }

        // read the file once (without processing) to count lines for progress bar
        /// <summary>
        /// Counts the lines.
        /// </summary>
        /// <returns></returns>
        private  int CountLines()
        {
            if (null == DataContextObject || null == DataContextObject.File) return 0;

            TotalLineCount += DataContextObject.File.LinesCount() - 1;
            AppendLog(string.Format("Finished scanning import file [{0}]. Number of lines: {1}",
                                    Path.GetFileName(DataContextObject.File.FileName),
                                    TotalLineCount));

            AppendLog(string.Format("Finished scanning import file(s). Total lines: {0}", TotalLineCount));
            return TotalLineCount;
        }

        /// <summary>
        /// Processes the file.
        /// </summary>
        /// <param name="fileProgress">The file progress.</param>
        private void ProcessFile(FileProgress fileProgress)
        {
            try
            {
                using(var fs = new FileStream(fileProgress.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var inputFile = new StreamReader(fs))
                    {
                        // search for the header line
                        string inputLine;
                        var foundHeader = false;
                        while ((inputLine = inputFile.ReadLine()) != null)
                        {
                            var actualInputColumns = inputLine.Split(',').ToList();
                            var expectedInputColumns = _headerLine.Split(',').ToList();

                            //if (inputLine.Replace(" ", null).EqualsIgnoreCase(_headerLine.Replace(" ", null)))
                            if(expectedInputColumns.Count == actualInputColumns.Count && actualInputColumns.Select(col => col.Trim().ToUpper()).ToList().SequenceEqual(expectedInputColumns))
                            {
                                var headerColumns = new[]
                                    {
                                        new DataColumn("MedicalPracticeId", typeof(string)){ AllowDBNull = false, Unique = true},
                                        new DataColumn("PhyNpi", typeof(long)){ AllowDBNull = true}, 
                                        new DataColumn("DiabHbA1CTest", typeof (double)) { AllowDBNull = true },
                                        new DataColumn("DiabHbA1CControl", typeof (double)) { AllowDBNull = true },
                                        new DataColumn("DiabBPControl", typeof (double)) { AllowDBNull = true },
                                        new DataColumn("AsthMedicationRatio", typeof (double)) { AllowDBNull = true },
                                        // new DataColumn("CardCondLdlCScreening", typeof (double)) { AllowDBNull = true },
                                        // new DataColumn("CardConditionsLdlCControl", typeof (double)) { AllowDBNull = true },
                                        new DataColumn("HyperBPControl", typeof (double)) { AllowDBNull = true },
                                        new DataColumn("COPD", typeof (double)) { AllowDBNull = true },
                                        new DataColumn("DiabHbA1CTestSTAVG", typeof (double)) { AllowDBNull = true },
                                        new DataColumn("DiabHbA1CControlSTAVG", typeof (double)) { AllowDBNull = true },
                                        new DataColumn("DiabBPControlSTAVG", typeof (double)) { AllowDBNull = true },
                                        new DataColumn("AsthMedicationRatioSTAVG", typeof (double)) { AllowDBNull = true },
                                        // new DataColumn("CardCondLdlCScreeningSTAVG", typeof (double)) { AllowDBNull = true },
                                        // new DataColumn("CardConditionsLdlCControlSTAVG", typeof (double)) { AllowDBNull = true },
                                        new DataColumn("HyperBPControlSTAVG", typeof (double)) { AllowDBNull = true },
                                        new DataColumn("COPDSTAVG", typeof (double)) { AllowDBNull = true },
                                        new DataColumn("Dataset_Id", typeof (int)) { AllowDBNull = true }
                                    };

                                MedicalPracticeHedisDataTable.Columns.Clear();
                                MedicalPracticeHedisDataTable.Columns.AddRange(headerColumns);
                                // Found the file token in the header. Ready to start import.
                                foundHeader = true;
                                break;
                            }

                            if (fileProgress.LinesDone > fileProgress.TotalLines)
                            {
                                break;
                            }
                        }

                        if (!foundHeader)
                        {
                            // Didn't find file token in input file before EOF
                            MessageBox.Show(AppendLog(string.Format("The input file [{0}] does not appear to be of the correct file type.",
                                                      fileProgress.FileName)), "Error importing file");
                            return;
                        }

                        AppendLog(string.Format("Header row found. Continuing from line {0}.", fileProgress.LinesDone));

						// Get Version info (using file CreationDate)
						DateTime? dbCreationDate = File.GetCreationTime(fileProgress.FileName);
						DataContextObject.DatasetItem.VersionMonth = dbCreationDate.Value.ToString("MMMM");
						DataContextObject.DatasetItem.VersionYear = dbCreationDate.Value.ToString("yyyy");

                        // save owner before inserting all rows
                        DataContextObject.CurrentImportType = DataContextObject.GetTargetByName(DataContextObject.SelectedDataType.DataTypeName);

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
                                else
                                    fileProgress.LinesDuplicated++;

                                if (lineCount == BATCH_SIZE || TotalLineCount == fileProgress.LinesDone)
                                {
                                    //var distinctRows = PhysicianHedisDataTable.Rows.OfType<DataRow>().Distinct(new DistinctRowByPhyNpiComparer()).ToList();

                                    //if (distinctRows.Count > 0)
                                    //{
                                    //    PhysicianHedisDataTable.Rows.Clear();

                                    //    foreach (var row in distinctRows)
                                    //        PhysicianHedisDataTable.Rows.Add(row);
                                    //}


                                    BulkInsert(MedicalPracticeHedisDataTable);
                                    MedicalPracticeHedisDataTable.Rows.Clear();
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
                                //fileProgress.PercentComplete = fileProgress.LinesDone * 100 / fileProgress.TotalLines;
                                AppendLogLinesDone(fileProgress.LinesDone);
                            }
                        }
                    }
                }

                if (MedicalPracticeHedisDataTable.Rows.Count > 0)
                {
                    BulkInsert(MedicalPracticeHedisDataTable);
                }

                AppendLog(fileProgress.LinesDone + " lines processed.");
            }
            catch (Exception e)
            {
                if (DataContextObject.DatasetItem.IsFinished)
                {
                    DataContextObject.DatasetItem.IsFinished = false;
                    using (var session = DataContextObject.Provider.SessionFactory.OpenSession())
                    {
                        using (var trans = session.BeginTransaction())
                        {
                            session.SaveOrUpdate(DataContextObject.DatasetItem);
                            trans.Commit();
                        }
                    }
                }
                MessageBox.Show(AppendLog("Error : " + e.Message), string.Format("Error importing file [{0}]",fileProgress.FileName));
            }
        }

        #region LOGGING 

        /// <summary>
        /// Imports the completed.
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
                        var linesImported = _session.CreateSQLQuery(string.Format("select count(o.[Id]) from {0} o where o.Dataset_Id={1}", DataContextObject.TargetType.EntityTableName(), DataContextObject.DatasetItem.Id))
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
            catch
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

    class DistinctRowByPhyNpiComparer : IEqualityComparer<DataRow>
    {

        public bool Equals(DataRow x, DataRow y)
        {
            return x.Field<long>("PhyNpi") == y.Field<long>("PhyNpi");
        }

        public int GetHashCode(DataRow obj)
        {
            return obj.Field<long>("PhyNpi").GetHashCode();
        }
    }
}
