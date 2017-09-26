using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Services.Import;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Physician.Physicians.Models;
using NHibernate;
using NHibernate.Linq;
using PropertyChanged;
//using physician = Monahrq.Infrastructure.Domain.Physicians.Physician;

namespace Monahrq.Wing.Physician.Physicians.ViewModels
{
    [ImplementPropertyChanged]
    public class ProcessFileViewModel : WizardStepViewModelBase<WizardContext>
    {
        private const int BATCH_SIZE = 5000;
        ISession _session;
        CountdownEvent _importCompletedCountdownEvent;
        private CancellationTokenSource _cts;
        //private int _totalFileLines;

        protected DataTable PhysicianDataTable;

        //private readonly string[] _tableNames = new[]
        //    {
        //        typeof (physician).EntityTableName(), 
        //        typeof (MedicalPractice).EntityTableName(),
        //        typeof (PhysicianAffiliatedHospital).EntityTableName(),
        //        typeof (MedicalPracticeAddress).EntityTableName(),
        //        typeof(PhysicianAuditLog).EntityTableName(),
        //        "Physicians_MedicalPractices"
        //    };

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        protected ILogWriter Logger { get; set; }

        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        protected IEventAggregator EventAggregator { get; set; }


        public ObservableCollection<string> LogFile { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessFileViewModel"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ProcessFileViewModel(WizardContext context)
            : base(context)
        {
            Done = false;
            PhysicianDataTable = new DataTable("PhysicianTarget")
            {
                TableName = typeof(PhysicianTarget).EntityTableName()
            };
            LogFile = new ObservableCollection<string>();
        }

        /// <summary>
        /// Starts the import.
        /// </summary>
        public async void StartImport()
        {
            _importCompletedCountdownEvent = new CountdownEvent(1);
            _cts = new CancellationTokenSource();

            if (UseApiForPhysicians) {
                Done = true;
                DataContextObject.DatasetItem.Name += " (Generated Website Real Time)";
                DataContextObject.DatasetItem.UseRealtimeData = true;
                ImportCompleted(true);
                CommandManager.InvalidateRequerySuggested();
                return; 
            }
            
            if (Logger == null)
                Logger = ServiceLocator.Current.GetInstance<ILogWriter>();

            if (EventAggregator == null)
                EventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

            EventAggregator.GetEvent<WizardCancelEvent>().Subscribe(e => Cancelled(this, EventArgs.Empty));

            try
            {
                
                _session = DataContextObject.Provider.SessionFactory.OpenSession();

                await ImportFileAsync(_cts.Token);

                if (!_cts.IsCancellationRequested)
                    ImportCompleted(true);
                //else
                //    ImportCompleted(false);

                // WPF CommandManager periodically calls IsValid to see if the Next/Done button should be enabled. 
                // In multi-threaded wizard steps, IsValid returns the value of the Done flag. Call InvalidateRequerySuggested here
                // on the UI thread after setting the Done flag to force WPF to call IsValiphysicinMedPracticeTabled now so the Next/Done button will become enabled. 
                CommandManager.InvalidateRequerySuggested();
                
            }
            catch (OperationCanceledException)
            {
                ImportCompleted(false);
                MessageBox.Show(AppendLog("Import cancelled."));
            }
            catch(Exception exc)
            {
                ImportCompleted(false);
                AppendLog("Import failed due to the following reason:" + exc.GetBaseException().Message);
            }
            finally
            {
                _session.Dispose();
                _cts.Dispose();
            }
        }

        /// <summary>
        /// Cancelleds the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        // ReSharper disable UnusedParameter.Local
        void Cancelled(object sender, EventArgs e)
        // ReSharper restore UnusedParameter.Local
        {
            // don't use cancellation token if user cancelled too late
            try
            {
                if (_importCompletedCountdownEvent.CurrentCount > 0)
                _cts.Cancel();
            }
            catch(ObjectDisposedException ode)
            {
                this.Logger.Write(ode);
            }
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
            var targetTableName = string.Format("dbo.{0}", typeof(PhysicianTarget).EntityTableName());

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
        /// <param name="npi">The npi.</param>
        /// <param name="pacId">The pac identifier.</param>
        /// <param name="ProfEnrollId">The prof enroll identifier.</param>
        /// <returns></returns>
        protected bool Any<T>(int npi, string pacId, string ProfEnrollId) where T : PhysicianTarget
        {
            return _session.Query<T>().Any(m => m.Npi == npi &&
                                                m.PacId.ToLower() == pacId.ToLower() &&
                                                m.ProfEnrollId.ToLower() == ProfEnrollId.ToLower());
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

        //string _headerLine;
        //private Func<string, bool> _lineFunction;

        /// <summary>
        /// Initializes the specified header line.
        /// </summary>
        /// <param name="headerLine">The header line.</param>
        /// <param name="lineFunction">The line function.</param>
        public void Initialize(string headerLine, Func<string, bool> lineFunction)
        {
            //_headerLine = headerLine;
            //_lineFunction = lineFunction;
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
            var uiContext = SynchronizationContext.Current;

            
            return await Task.Run(() =>
            {
                var import = new PhysiciansImporter(); // Create importer and set ValueChanged Event.
                try
                {
                    //DisableTableIndexes(_tableNames);
                    if (LogFile != null && LogFile.Any())
                    {
                        uiContext.Send(x => LogFile.Clear(), null);
                        uiContext.Send(x => AppendLog(""), null);
                    }

                    var obj = new object();
                   
                    import.FetchingBatchChanged += (o, e) =>
                        {
                            lock (obj)
                            {
                                _activeState = import.CurrentState;
                                uiContext.Send(x => AppendLog(string.Format("{2} {0:0,0} of {1:0,0}", import.ImportedPhysicianCount,
                                                              import.TotalPhysicians, string.Format(API_RECORDS_PROCESS_MESSAGE, import.CurrentState)), import.FetchingBatch), null);
                            }
                        };

                    import.ValueChanged += (o, e) =>
                        {
                            if (import.ImportedPhysicianCount == 0 ||
                                import.ImportedPhysicianCount > import.TotalPhysicians) return;

                            lock (obj)
                            {
                                uiContext.Send(x => AppendLog(string.Format("{2} {0:0,0} of {1:0,0}", import.ImportedPhysicianCount,
                                                              import.TotalPhysicians, string.Format(API_RECORDS_PROCESS_MESSAGE, import.CurrentState))), null);
                            }
                        };

                    lock (obj)
                    {
                        uiContext.Send(x => AppendLog(string.Format("--- Getting Physician Data...")), null);
                    }

                    var stopWatch = new Stopwatch();
                    foreach (var state in DataContextObject.SelectedStates.ToList()) // Iterate through user selected states
                    {
                        if(ct != null && ct.IsCancellationRequested) break;


                        _activeState = state;

                        stopWatch.Reset();
                        stopWatch.Start();

                        var displayState = state;
                        lock (obj)
                        {
                            uiContext.Send(x => AppendLog(string.Format("Calling API to fetch {0} state physicians", displayState)), null);
                        }
                        //import.ImportPhysicians(state); // Import per state
                        import.PhysicianBulkImportTargetName = DataContextObject.TargetType.EntityTableName();
                        import.DatasetId = DataContextObject.DatasetItem.Id;

                        // remove false parameter to run asynchronously.
                        import.ImportPhysiciansTest3(state, ct, false);

                        if (import.HasErrors)
                        {
                            uiContext.Send(x => AppendLog("Error occurred. Continuing on."), null);
                            continue;
                        }
                        import.FetchingBatch = false;
                        uiContext.Send(x => AppendLog(string.Format("{2} {0} of {1}", import.ImportedPhysicianCount,
                                                              import.TotalPhysicians, string.Format(API_RECORDS_PROCESS_MESSAGE, displayState))), null);
                        stopWatch.Stop();

                        lock (obj)
                        {
                            uiContext.Send(x => AppendLog(string.Format("--- Total # of Physicans records saved: {0:0,0} ", import.TotalPhysiciansSaved)), null);
                            uiContext.Send(x => AppendLog(string.Format("--- Total # of Medical Practices records saved: {0:0,0}", import.TotalMedicalPracticesSaved)), null);
                            uiContext.Send(x => AppendLog(string.Format("--- Total execution time in seconds: {0}", Math.Round(TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds).TotalSeconds,0))), null);
                        }
                    }

                    import.DeleteTempDirectory();

                    stopWatch = null;
                }
                catch (Exception exc)
                {
                    _errorsOccurred = true;
                    var excToUse = exc.GetBaseException();
                    if (exc is AggregateException)
                    {
                        excToUse = (exc as AggregateException).GetBaseException();
                    }
                    Logger.Write(excToUse.GetBaseException());

                    if(excToUse is OperationCanceledException)
                    {
                        _isUserCancelled = true;
                        return 0;
                    }   

                    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(excToUse.GetBaseException());
                    return 0;
                }
                finally
                {
                    //EnableTableIndexes(_tableNames);
                    //TruncateTable(DataContextObject.TargetType.EntityTableName());

                    if (_isUserCancelled)
                        uiContext.Send(x => AppendLog(string.Format("User cancelled physician API import process for State(s): {0}", string.Join(",",DataContextObject.SelectedStates))), null);
                    else
                    {
                        if (!_errorsOccurred)
                            uiContext.Send(x => AppendLog(string.Format("Finished Downloading {0} API records.", import.TotalCombinedPhysicians)), null);
                        else
                            uiContext.Send(x => AppendLog(string.Format("An error occurred while process physician API records for State(s): {0}", string.Join(",",DataContextObject.SelectedStates))), null);
                    }

                    LogFile?.ToList().ForEach(log => Logger.Information(log));
                }
                return 1;
            }, ct);
        }

        private bool _isUserCancelled = false;
        private bool _errorsOccurred = false;

        //private int _totalCombined;

        private void TruncateTable(string tableName)
        {
            var query = new StringBuilder();
            query.AppendLine("IF EXISTS (select TOP 1 [Id] FROM [dbo].[" + tableName + "])");
            query.AppendLine("BEGIN");
            query.AppendLine("  TRUNCATE TABLE [dbo].[" + tableName + "];");
            query.AppendLine("end");

            _session.CreateSQLQuery(query.ToString())
                    .ExecuteUpdate();
        }

        private IConfigurationService configurationService;

        public IConfigurationService ConfigurationService {
            get {
                if (configurationService == null) 
                    configurationService = ServiceLocator.Current.GetInstance<IConfigurationService>();
                
                return configurationService;
            }
        }
        
        public bool UseApiForPhysicians {
            get { return !this.DataContextObject.IsPhysicianManagedInMONAHRQ; }
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
                if (success && !_errorsOccurred)
                {
					var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
					var selectedStates = this.DataContextObject.SelectedStates.ToArray();

					if (!configService.HospitalRegion.IsDefined)
                    {
						configService.HospitalRegion.SelectedRegionType = typeof(CustomRegion);
						configService.HospitalRegion.DefaultStates = new StringCollection();
                    }
					configService.HospitalRegion.AddStates(selectedStates);
					configService.Save();

                    //HospitalRegion.Default.DefaultStates.AddRange(selectedStates);
                    //HospitalRegion.Default.SelectedStates = contextStates.ToList();
                    Task.Delay(1000).Wait();

                    DataContextObject.Finish();

                    //if (UseApiForPhysicians)
                    AppendLog("Import completed successfully.");
                }
                else if (success && _errorsOccurred)
                {
                    DataContextObject.DatasetItem.Name = string.Format("{0} (Partial Success)", DataContextObject.DatasetItem.Name);
                    DataContextObject.Finish();

                    AppendLog("Import partially completed successfully. Please retry to perform the import. If problem persists, then please contact MONAHQ Technical Support.");
                }
                else
                {
                    AppendLog("Import completed unsuccessfully. Please retry to perform the import. If problem persists, then please contact MONAHQ Technical Support.");
                }

                if (_session != null && _session.IsOpen)
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
                if (_session != null) _session.Dispose();
            }
        }

        /// <summary>
        /// Appends the log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected string AppendLog(string message, bool FetchNextBatch = false)
        {
            if (LogFile == null) return message;
            
            if (LogFile.Any(s => s.StartsWith(string.Format(API_RECORDS_PROCESS_MESSAGE, _activeState))) && message.StartsWith(string.Format(API_RECORDS_PROCESS_MESSAGE, _activeState)))
            {
                var index = new List<string>(LogFile).FindIndex(FindIndex);

                if (index > -1)
                    LogFile[index] = message + ((FetchNextBatch) ? " Please Stand by fetching next batch..." : null);
            }
            else
            {
                LogFile.Add(message);
            }

            return message;
        }

        private bool FindIndex(string message)
        {
            return message.StartsWith(string.Format(API_RECORDS_PROCESS_MESSAGE, _activeState));
        }

        private string _activeState = null;

        private const string API_RECORDS_PROCESS_MESSAGE = "--- API records processed for state {0}:";

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
            return float.TryParse(strFloat, out tempFloat) ? (float?)tempFloat : null;
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
            return int.TryParse(strFloat, out tempInt) ? (int?)tempInt : null;
        }

        #endregion

        #region Index Manipulation

        protected void EnableTableIndexes(params string[] tableNames)
        {
            EnableDisableTableIndexes(false, tableNames);
        }

        protected void DisableTableIndexes(params string[] tableNames)
        {
            EnableDisableTableIndexes(true, tableNames);
        }

        private void EnableDisableTableIndexes(bool disable, params string[] tableNames)
        {
            //using (var session = Provi.SessionFactory.OpenStatelessSession())
            //{
            foreach (var tableName in tableNames)
            {
                _session.CreateSQLQuery(EnableDisableNonClusteredIndexes(tableName, disable)) //ALL
                    .SetTimeout(5000)
                    .ExecuteUpdate();
            }
            //}
        }

        private string EnableDisableNonClusteredIndexes(string tableName, bool disable = false)
        {
            var sqlStatement = new StringBuilder();

            sqlStatement.AppendLine("DECLARE @sql AS VARCHAR(MAX)='';" + System.Environment.NewLine);
            sqlStatement.Append("SELECT	 @sql = @sql + 'ALTER INDEX ' + sys.indexes.Name + ' ON " + tableName + " " + (disable ? "DISABLE" : "REBUILD") + "; ");

            if (!disable)
            {
                sqlStatement.Append(" ALTER INDEX ' + sys.indexes.Name + ' ON " + tableName + " REORGANIZE;");
            }

            sqlStatement.Append("' + CHAR(13) + CHAR(10)");
            sqlStatement.AppendLine();

            sqlStatement.AppendLine("FROM	 sys.indexes" + System.Environment.NewLine);
            sqlStatement.AppendLine("JOIN    sys.objects ON sys.indexes.object_id = sys.objects.object_id");
            sqlStatement.AppendLine("WHERE sys.indexes.type_desc = 'NONCLUSTERED'");
            sqlStatement.AppendLine("  AND sys.objects.type_desc = 'USER_TABLE'");
            sqlStatement.AppendLine("  AND sys.objects.Name = '" + tableName + "';");

            sqlStatement.AppendLine();
            sqlStatement.AppendLine("exec(@sql);");

            return sqlStatement.ToString();
        }
        #endregion Index Manipulation
    }
}
