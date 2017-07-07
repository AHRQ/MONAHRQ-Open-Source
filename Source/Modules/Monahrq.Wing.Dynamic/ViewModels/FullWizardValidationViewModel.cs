using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Model;
using Monahrq.DataSets.Services;
using Monahrq.DataSets.ViewModels.Validation;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Validation;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Validation;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Dynamic.Models;
using NHibernate;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.Wing.Dynamic.ViewModels
{
    /// <summary>
    /// Validation view model class for full wizard
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.Wing.Dynamic.Models.WizardContext}" />
    /// <seealso cref="Monahrq.DataSets.ViewModels.Validation.IValidationViewModel" />
    [ImplementPropertyChanged]
    public class FullWizardValidationViewModel
            : WizardStepViewModelBase<WizardContext>, IValidationViewModel
    {
        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public override string DisplayName
        {
            get
            {
                return "Import Data";      // this text cannot be too long or it doesn't fit on progress bar
            }
        }

        /// <summary>
        /// Gets or sets the start command.
        /// </summary>
        /// <value>
        /// The start command.
        /// </value>
        public ICommand StartCommand { get; set; }

        /// <summary>
        /// Occurs when validation starts.
        /// </summary>
        public event EventHandler ValidationStarted = delegate { };
        /// <summary>
        /// Occurs when to handle notify progress event.
        /// </summary>
        public event EventHandler<ExtendedEventArgs<Action>> NotifyProgress = delegate { };
        /// <summary>
        /// Occurs when validation succeededs.
        /// </summary>
        public event EventHandler ValidationSucceeded = delegate { };
        /// <summary>
        /// Occurs when validation fails.
        /// </summary>
        public event EventHandler ValidationFailed = delegate { };
        /// <summary>
        /// Occurs when validation is completed.
        /// </summary>
        public event EventHandler ValidationComplete = delegate { };

        /// <summary>
        /// Gets or sets the results view.
        /// </summary>
        /// <value>
        /// The results view.
        /// </value>
        public ListCollectionView ResultsView
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the dummy results.
        /// </summary>
        /// <value>
        /// The dummy results.
        /// </value>
        static IEnumerable<ValidationResultViewModel> DummyResults
        {
            get { return Enumerable.Empty<ValidationResultViewModel>(); }
        }

        /// <summary>
        /// Gets the results summary.
        /// </summary>
        /// <value>
        /// The results summary.
        /// </value>
        public IValidationResultsSummary ResultsSummary { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullWizardValidationViewModel"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public FullWizardValidationViewModel(WizardContext context)
            : base(context)
        {

            ApplyButtonCaption();

            if (DataContextObject.CustomTarget == null || !DataContextObject.CustomTarget.Elements.Any())
            {
                var prov = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
                using (var sess = prov.SessionFactory.OpenSession())
                {
                    var target = DataContextObject.CustomTarget;
                    var elements = sess.Query<Element>().Where(e => e.Owner.Id == target.Id).ToFuture();

                    ElementDictionary = elements.ToList()
                        .ToDictionary(elem => elem.Name.ToLower());
                }
            }
            else
            {
                ElementDictionary = DataContextObject.CustomTarget.Elements.ToList()
                                                                  .ToDictionary(elem => elem.Name.ToLower());
            }
            
            ApplyResults(null, 0);
            StartCommand = new DelegateCommand(ValidateDataset, () => !IsRunning);
        }

        /// <summary>
        /// Validates the dataset.
        /// </summary>
        private void ValidateDataset()
        {
            var mapperFactory = new TargetMapperFactory(DataContextObject.CustomTarget, DataContextObject);
            DataContextObject.Mapper = mapperFactory.CreateMapper(DataContextObject.CurrentCrosswalk);
            var type = DataContextObject.TargetType;
            ValidationEngine = new InstanceValidator(type);
            ApplyNullResults();
            RunValidation();
        }

        /// <summary>
        /// Applies the button caption.
        /// </summary>
        private void ApplyButtonCaption()
        {
            ButtonCaption = ResultsSummary == null
                ? "Import Data"
                : "Re-import Data";
        }

        /// <summary>
        /// Applies the results.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="numInserts">The number inserts.</param>
        private void ApplyResults(IEnumerable<ValidationResultViewModel> data, int numInserts)
        {
            var temp = (data ?? DummyResults).ToList();
            var observ = new ObservableCollection<ValidationResultViewModel>(temp);
            ResultsView = (ListCollectionView)CollectionViewSource.GetDefaultView(observ);
            ResultsSummary = data == null ? null : new ValidationResultsSummary(numInserts, CurrentProgress.Total, DataContextObject);
        }

        /// <summary>
        /// Applies the null results.
        /// </summary>
        private void ApplyNullResults()
        {
            ApplyResults(null, 0);
        }

        /// <summary>
        /// Applies the results.
        /// </summary>
        private void ApplyResults()
        {
            var id = DataContextObject.DatasetItem == null ? int.MinValue : DataContextObject.DatasetItem.Id;
            if (id == int.MinValue)
            {
                ApplyResults(null, NumberRecordsInserted);
            }
            else
            {
                using (var session = DataContextObject.Provider.SessionFactory.OpenSession())
                {
                    
                    var errors = session.Query<DatasetTransactionRecord>()
                            .Where(dt => dt.Dataset.Id == id)
                            .Take(1000)
                            .Select(err => new ValidationResultViewModel(err));
                    ApplyResults(errors, NumberRecordsInserted);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current progress.
        /// </summary>
        /// <value>
        /// The current progress.
        /// </value>
        public IProgressState CurrentProgress
        {
            get; set;
        }

        /// <summary>
        /// Runs the validation.
        /// </summary>
        public void RunValidation()
        {
            IsRunning = true;
            ValidationStarted(this, EventArgs.Empty);
            var worker = new BackgroundWorker();
            var sync = new object();
            var interimProgress = new ProgressState(0, CurrentProgress.Total);
            NotifyProgress(this, new ExtendedEventArgs<Action>(() => { }));
            Application.Current.DoEvents();
            Action notifyAction = () =>
                {
                    {
                        lock (sync)
                        {
                            CurrentProgress = interimProgress;
                            Feedback = string.Format("Progress: {0}", interimProgress.Ratio.ToString("P0"));
                        }
                    }
                };
            var timer = new Timer(state => NotifyProgress(this, new ExtendedEventArgs<Action>(notifyAction)), null, 1, 10000);

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += (o, e) => RunWorker(worker, e);
            worker.ProgressChanged += (o, e) =>
                {
                    var temp = (e.UserState as ProgressState) ?? ProgressState.Empty;
                    if (temp.Current > interimProgress.Current)
                    {
                        lock (sync)
                        {
                            interimProgress = temp;
                        }
                    }
                };
            worker.RunWorkerCompleted += (o, e) =>
               {
                   ModelIsValid = !Cancelled;
                   timer.Dispose();
                   timer = null;
                   var wait = new ManualResetEvent(false);
                   var thr = new Thread(() =>
                       NotifyProgress(this, new ExtendedEventArgs<Action>(
                           () =>
                           {
                               notifyAction();
                               wait.Set();
                           })));
                   thr.Start();
                   var thr2 = new Thread(() =>
                       {
                           wait.WaitOne();
                           wait.Dispose();
                           ApplyResults();
                           IsRunning = false;
                           if (ModelIsValid)
                           {
                               NotifyProgress(this, new ExtendedEventArgs<Action>(() =>
                               {
                                   if (DataContextObject.DatasetItem.IsReImport)
                                   {
                                       using (var session = DataContextObject.Provider.SessionFactory.OpenStatelessSession())
                                       {
                                           var linesImported = session.CreateSQLQuery(string.Format("select count(o.Id) from {0} o where o.Dataset_Id={1}",
                                                                                                    DataContextObject.CustomTarget.DbSchemaName,
                                                                                                    DataContextObject.DatasetItem.Id))
                                                                      .UniqueResult<int>();

                                           if (DataContextObject.DatasetItem.File.Contains(" (#"))
                                               DataContextObject.DatasetItem.File = DataContextObject.DatasetItem.File.SubStrBefore(" (#");

                                           DataContextObject.DatasetItem.File += " (# Rows Imported: " + linesImported +
                                                                                 ")";
                                       }
                                   }

                                   DataContextObject.Finish();
                                   Feedback = "Import  Process Complete";
                                   ValidationSucceeded(this, EventArgs.Empty);
                               }));
                           }
                           else
                           {
                               NotifyProgress(this, new ExtendedEventArgs<Action>(() =>
                               {
                                   Feedback = "Import process failed";
                                   ValidationFailed(this, EventArgs.Empty);
                               }));
                           }
                           NotifyProgress(this, new ExtendedEventArgs<Action>(() => OnValidationCompleted(EventArgs.Empty)));

                           // WPF CommandManager periodically calls IsValid to see if the Next/Done button should be enabled. 
                           // In multi-threaded wizard steps, IsValid returns the value of the Done flag. Call InvalidateRequerySuggested here
                           // on the UI thread after setting the Done flag to force WPF to call IsValid now so the Next/Done button will become enabled. 
                           //Application.Current.DoEvents();
                           NotifyProgress(this, new ExtendedEventArgs<Action>(CommandManager.InvalidateRequerySuggested));

                       });
                   thr2.Start();
                   Thread.Sleep(0);
               };

            ServiceLocator.Current.GetInstance<IEventAggregator>()
                .GetEvent<WizardCancelEvent>().Subscribe(evnt =>
                {
                    Cancelled = true;
                    if (worker.CancellationPending)
                    {
                        return;
                    }
                    worker.CancelAsync();
                });

            ServiceLocator.Current.GetInstance<IEventAggregator>()
                .GetEvent<WizardCancelEvent>().Subscribe(evnt =>
                    {
                        Cancelled = true;
                    });
            worker.RunWorkerAsync(new Func<ProgressState>(() => interimProgress));
            Thread.Sleep(0);
        }

        int NumberRecordsInserted { get; set; }

        /// <summary>
        /// Deletes the existing datacontext's data set.
        /// </summary>
        private void DeleteExisting()
        {
            try
            {
                var targetDbName = DataContextObject.CustomTarget.DbSchemaName;
                var targetDelete = string.Format("delete from {0} where Dataset_Id={1}", targetDbName, DataContextObject.DatasetItem.Id);

                var transactionType = typeof(DatasetTransactionRecord);
                var transactionDelete = string.Format("delete from {0} e where e.Dataset.Id = {1}", transactionType.Name, DataContextObject.DatasetItem.Id);

                using (var session = DataContextObject.Provider.SessionFactory.OpenStatelessSession())
                {
                    session.CreateSQLQuery(targetDelete)
                    .SetTimeout(50000)
                    .ExecuteUpdate();

                    session.CreateQuery(transactionDelete)
                        .SetTimeout(50000)
                        .ExecuteUpdate();
                }
            }
            catch (Exception exc)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Raises the <see cref="E:ValidationCompleted" /> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnValidationCompleted(EventArgs eventArgs)
        {
            ApplyButtonCaption();
            ValidationComplete(this, eventArgs);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FullWizardValidationViewModel"/> is cancelled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancelled; otherwise, <c>false</c>.
        /// </value>
        bool Cancelled { get; set; }

        /// <summary>
        /// Returns true if  <see cref="FullWizardValidationViewModel"/>  is valid.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsValid()
        {
            return ModelIsValid;
        }

        /// <summary>
        /// Gets or sets the button caption.
        /// </summary>
        /// <value>
        /// The button caption.
        /// </value>
        public string ButtonCaption
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the dataservices.
        /// </summary>
        /// <value>
        /// The dataservices.
        /// </value>
        IDataContextServices Dataservices { get; set; }
        /// <summary>
        /// Loads the model.
        /// </summary>
        public void LoadModel()
        {
            Dataservices = DataContextObject.Services;
            var providerFactory = new DataproviderFactory(Dataservices);
            providerFactory.InitDataProvider();
            Feedback = string.Format(@"Press ""{0}"" to begin", ButtonCaption);
            Application.Current.DoEvents();
            Feedback = String.Empty;
            using (var conn = Dataservices.ConnectionFactory())
            {
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("select count(*) from [{0}]", Dataservices.Configuration.SelectFrom);
                        conn.Open();
                        var temp = (int)cmd.ExecuteScalar();
                        CurrentProgress = new ProgressState(0, temp);
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether model is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if model is valid; otherwise, <c>false</c>.
        /// </value>
        public bool ModelIsValid { get; private set; }

        // object notificationSync = new object();
        /// <summary>
        /// Runs the worker.
        /// </summary>
        /// <param name="worker">The worker.</param>
        /// <param name="args">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        public void RunWorker(BackgroundWorker worker, DoWorkEventArgs args)
        {
            NumberRecordsInserted = 0;
            DeleteExisting();
            Action<IEnumerable<ElementMappingValueException>, IStatelessSession, int>
                    guardErrors = (errorList, session, recordnum) =>
                {
                    foreach (var item in errorList)
                    {
                        GuardError(item.Element.Description, item.Message,
                                    recordnum, RecordState.CriticalError, session);
                    }
                };

            var getProgress = args.Argument as Func<ProgressState> ?? (() => ProgressState.Empty);

            CurrentProgress = getProgress() ?? ProgressState.Empty;
            var current = 0;
            var total = CurrentProgress.Total;

            var mappings = DataContextObject.RequiredMappings.Concat(DataContextObject.OptionalMappings)
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                .ToDictionary(k => k.Key, v => v.Value);


            //DataTable IPDataTableForGrouper = new DataTable();

            //var factory = Dataservices.Controller.ProviderFactory;
            var connElem = Dataservices.Configuration;
            NotifyProgress(this, new ExtendedEventArgs<Action>(ApplyNullResults));
            ValidationStarted(this, EventArgs.Empty);
            using (var conn = Dataservices.ConnectionFactory())
            {
                conn.Open();
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("select {1} from [{0}]", connElem.SelectFrom,
                            FactorySelectFields(mappings));

                        using (var rdr = cmd.ExecuteReader())
                        {
                            using (var session = DataContextObject.Provider.SessionFactory.OpenStatelessSession())
                            {
                                var importer = DataContextObject.Mapper.CreateBulkImporter(session.Connection, DataContextObject.CustomTarget);

                                EventHandler<ExtendedEventArgs<SqlConnection>> connectionRequested = (o, e) => e.Data = session.Connection as SqlConnection;
                                importer.ConnectionRequested += connectionRequested;
                                importer.Prepare();

                                try
                                {
                                    while (rdr.Read() && !args.Cancel && !Cancelled)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            args.Cancel = true;
                                        }
                                        else
                                        {
                                            foreach (var kvp in mappings)
                                            {
                                                var ordinal = rdr.GetOrdinal(kvp.Value);
                                                DataContextObject.Mapper[kvp.Key] = rdr.GetValue(ordinal);
                                            }
                                            current++;

                                            var currentErrors = DataContextObject.Mapper.Errors;

                                            var hasErrors = currentErrors.Any();
                                            if (hasErrors)
                                            {
                                                guardErrors(currentErrors, session, current);
                                            }

                                            if (!hasErrors)
                                            {
                                                dynamic target = DataContextObject.Mapper.Target;
                                                var temp = ValidateTarget(target, current, rdr, mappings, session);
                                                hasErrors |= temp.HasErrors;
                                                if (!hasErrors)
                                                {
                                                    //importer.DataContextObject.DatasetItem =
                                                    //    DataContextObject.DatasetItem;
                                                    (target as DynamicDatasetRecord).Dataset = DataContextObject.DatasetItem;
                                                    try
                                                    {
                                                        importer.Insert(target);
                                                        NumberRecordsInserted++;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        GuardError("DATA ERROR", ex.Message, current, RecordState.CriticalError, session);
                                                    }
                                                }
                                            }
                                            DataContextObject.Mapper.Reset();
                                            DataContextObject.Finish();
                                    
                                            var progress = new ProgressState(current, total);
                                            worker.ReportProgress((int) Math.Floor(progress.Ratio*100d), progress);
                                        }
                                    }
                                }
                                finally
                                {
                                  
                                    importer.Dispose();
                                    //importer = null;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    conn.Close();
                    
                }
            }
        }

        /// <summary>
        /// Guards the error.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="error">The error.</param>
        /// <param name="recordNum">The record number.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="session">The session.</param>
        private void GuardError(string elementName, string error, int recordNum, RecordState resultType, IStatelessSession session)
        {
            var result = new ValidationResultViewModel(DataContextObject.DatasetItem)
            {
                ElementName = elementName,
                Message = error,
                ResultType = resultType,
                RecordNumber = recordNum,
            };
            session.Insert(result.TransactionRecord);
        }

        /// <summary>
        /// Gets or sets the validation engine.
        /// </summary>
        /// <value>
        /// The validation engine.
        /// </value>
        InstanceValidator ValidationEngine { get; set; }

        /// <summary>
        /// struct indicating the validation status
        /// </summary>
        struct ValidationStatus
        {
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="FullWizardValidationViewModel"/> has errors.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance has errors; otherwise, <c>false</c>.
            /// </value>
            public bool HasErrors { get; set; }
        }

        /// <summary>
        /// The validation log message
        /// </summary>
        private const string VALIDATION_LOG_MESSAGE = "{0} Import Validation \"{1}\": ({2}) {3}";

        /// <summary>
        /// Validates the target.
        /// </summary>
        /// <param name="temp">The temporary.</param>
        /// <param name="current">The current.</param>
        /// <param name="rdr">The RDR.</param>
        /// <param name="mappings">The mappings.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        private ValidationStatus ValidateTarget(object temp, int current, IDataReader rdr,
                IDictionary<string, string> mappings, IStatelessSession session)
        {
            bool hasError = false;
            var validationResult = ValidationEngine.ValidateInstance(temp);
            var updateCount = validationResult.PropertyErrors.Count + validationResult.PropertyWarnings.Count;
            hasError = validationResult.PropertyErrors.Any() || validationResult.ClassErrors.Any();

            if (validationResult.ClassErrors.Any())
            {
                updateCount += validationResult.ClassErrors.Count;
                hasError = true;

                foreach (var res in validationResult.ClassErrors)
                {
                    string member = string.Empty;
                    string msg = null;
                    object value = null;

                    if (res.Property != null)
                    {
                        member = res.Property.Name;
                        var column = mappings[member];
                        var ord = rdr.GetOrdinal(column);
                        value = rdr.GetValue(ord);
                        msg = string.Format("Value excluded: {0}",
                            value == null
                                ? "<<NULL>>"
                                : string.IsNullOrEmpty(value.ToString())
                                    ? @""""
                                    : value.ToString());
                    }

                    if (res.ErrorState == ValidationErrorState.ValidationError && !string.IsNullOrEmpty(res.Message))
                    {
                        msg = string.Format("{1} (Value excluded: {0})",
                               (value == null
                               ? "<<NULL>>"
                               : string.IsNullOrEmpty(value.ToString()) ? @"""" : value.ToString()),
                               res.Message);
                    }

                    GuardError(member, msg, current, RecordState.ExcludedByCrosswalk, session);
                    //TODO: Output messages to screen, other log file per import, someplace besited the main session.log
                    //this.DataContextObject.Logger.Log(string.Format(VALIDATION_LOG_MESSAGE, DataContextObject.SelectedDataType.DataTypeName, DataContextObject.Title, "Class Error", msg), Category.Info, Priority.High);
                }
            }

            if (validationResult.PropertyErrors.Any())
            {
                hasError = true;
                
                foreach (var err in validationResult.PropertyErrors)
                {
                    Element elemTest;
                    ElementDictionary.TryGetValue(err.Property.Name.ToLower(), out elemTest);
                    if (elemTest != null)
                    {
                        GuardError(elemTest.Description,
                        ElementMappingModel.ParseValueError(err.AttemptedValue, err.Message),
                        current, RecordState.ValidationError, session);
                    }
                    //TODO: Output messages to screen, other log file per import, someplace besited the main session.log
                    //this.DataContextObject.Logger.Log(string.Format(VALIDATION_LOG_MESSAGE, DataContextObject.SelectedDataType.DataTypeName, DataContextObject.Title, "Error", err.Message), Category.Info, Priority.High);
                }
            }

            if (validationResult.PropertyWarnings.Any())
            {
                foreach (var err in validationResult.PropertyWarnings)
                {
                    Element elemTest;
                    ElementDictionary.TryGetValue(err.Property.Name.ToLower(), out elemTest);
                    if (elemTest != null)
                    {
                        GuardError(elemTest.Description,
                            ElementMappingModel.ParseValueError(err.AttemptedValue, err.Message),
                            current, RecordState.Warning, session);
                    }
                    //TODO: Output messages to screen, other log file per import, someplace besited the main session.log
                    //this.DataContextObject.Logger.Log(string.Format(VALIDATION_LOG_MESSAGE, DataContextObject.SelectedDataType.DataTypeName, DataContextObject.Title, "Warning", err.Message), Category.Info, Priority.High);
                }
            }

          
            var result = new ValidationStatus();
            result.HasErrors = hasError;
            return result;
        }

        /// <summary>
        /// Validates the dataset row rejections.
        /// </summary>
        /// <param name="temp">The temporary.</param>
        /// <returns></returns>
        private ValidationResult ValidateDatasetRowRejections(object temp)
        {
            var ctxt = new ValidationContext(temp);
            var attr = temp.GetType().GetCustomAttribute<RejectIfAnyPropertyHasValueAttribute>();
            return
                attr == null ? ValidationResult.Success
                : (
                    attr.GetValidationResult(temp, ctxt) ?? ValidationResult.Success
                );
        }

        /// <summary>
        /// Gets the Factory select fields.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <returns></returns>
        private string FactorySelectFields(IEnumerable<KeyValuePair<string, string>> mappings)
        {

            return string.Join(",", mappings.Select(kvp => kvp.Value).Distinct()
                    .Select(val => string.Format(" CVAR([{0}]) as [{0}]", val))).Trim();
        }

        /// <summary>
        /// Gets or sets the feedback.
        /// </summary>
        /// <value>
        /// The feedback.
        /// </value>
        public string Feedback
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the element dictionary.
        /// </summary>
        /// <value>
        /// The element dictionary.
        /// </value>
        private Dictionary<string, Element> ElementDictionary { get; set; }

        /// <summary>
        /// Gets the current file.
        /// </summary>
        /// <value>
        /// The current file.
        /// </value>
        public string CurrentFile
        {
            get
            {
                var ds = Dataservices ?? this.DataContextObject.Services;
                return ds.CurrentFile;
            }
        }
    }
}
