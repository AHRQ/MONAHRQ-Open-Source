using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Model;
using Monahrq.DataSets.Services;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Validation;
using Monahrq.Sdk.Extensions;
using Monahrq.Theme.Controls.Wizard.Models;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using NHibernate.Linq;
using NHibernate;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Events;
using System.Data.SqlClient;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Theme.Controls.Wizard.Models.Data;

namespace Monahrq.DataSets.ViewModels.Validation
{
    /// <summary>
    /// The dataset import wizard validation and import view model.
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.DataSets.Model.DatasetContext}" />
    /// <seealso cref="Monahrq.DataSets.ViewModels.Validation.IValidationViewModel" />
    [ImplementPropertyChanged]
    public class ValidationViewModel
            : WizardStepViewModelBase<DatasetContext>, IValidationViewModel
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
        /// Gets the start command.
        /// </summary>
        /// <value>
        /// The start command.
        /// </value>
        public ICommand StartCommand { get; set; }

        /// <summary>
        /// Occurs when [validation started].
        /// </summary>
        public event EventHandler ValidationStarted = delegate { };
        /// <summary>
        /// Occurs when [notify progress].
        /// </summary>
        public event EventHandler<ExtendedEventArgs<Action>> NotifyProgress = delegate { };
        /// <summary>
        /// Occurs when [validation succeeded].
        /// </summary>
        public event EventHandler ValidationSucceeded = delegate { };
        /// <summary>
        /// Occurs when [validation failed].
        /// </summary>
        public event EventHandler ValidationFailed = delegate { };
        /// <summary>
        /// Occurs when [validation complete].
        /// </summary>
        public event EventHandler ValidationComplete = delegate { };

        /// <summary>
        /// Gets the results view.
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
        /// Gets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        IEventAggregator EventAggregator { get { return ServiceLocator.Current.GetInstance<IEventAggregator>(); } }

        /// <summary>
        /// Gets the results summary.
        /// </summary>
        /// <value>
        /// The results summary.
        /// </value>
        public IValidationResultsSummary ResultsSummary { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationViewModel"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ValidationViewModel(DatasetContext context)
            : base(context)
        {

            ApplyButtonCaption();

            if (DataContextObject.TargetElements == null || !DataContextObject.TargetElements.Any())
            {
                 var prov = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
                using (var sess = prov.SessionFactory.OpenSession())
                {
                    var target = DataContextObject.SelectedDataType.Target;
                    var elements = sess.Query<Element>().Where(e => e.Owner.Id == target.Id).ToFuture();

                    ElementDictionary = elements.ToList()
                        .ToDictionary(elem => elem.Name.ToLower());
                }
            }
            else
            {
                ElementDictionary = DataContextObject.TargetElements.ToDictionary(t => t.Name);
            }
           
            ApplyResults(null, 0);
            StartCommand = new DelegateCommand(ValidateDataset, () => !IsRunning);
        }

        /// <summary>
        /// Validates the dataset.
        /// </summary>
        private void ValidateDataset()
        {
            var mapperFactory = new TargetMapperFactory(DataContextObject.SelectedDataType.Target);
            DataContextObject.Mapper = mapperFactory.CreateMapper(DataContextObject.CurrentCrosswalk);
            var type = Type.GetType(DataContextObject.SelectedDataType.Target.ClrType);
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
                            .Where(xact => xact.Dataset.Id == id)
                            .Take(1000)
                            .Select(err => new ValidationResultViewModel(err));

                    ApplyResults(errors, NumberRecordsInserted);
                }
            }
        }

        /// <summary>
        /// Gets the current progress.
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
            ProgressState interimProgress = new ProgressState(0, CurrentProgress.Total);
            NotifyProgress(this, new ExtendedEventArgs<Action>(() => { }));
            Application.Current.DoEvents();
            Action notifyAction = () =>
                {
                    //{
                    lock (sync)
                    {
                        CurrentProgress = interimProgress;
                        Feedback = string.Format("Progress: {0}", interimProgress.Ratio.ToString("P0"));
                    }
                   // }
                };
            var timer = new Timer((state) => NotifyProgress(this, new ExtendedEventArgs<Action>(notifyAction)), null, 1, 10000);

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
                        notifyAction();
                        Application.Current.DoEvents();
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

        /// <summary>
        /// Gets or sets the number records inserted.
        /// </summary>
        /// <value>
        /// The number records inserted.
        /// </value>
        int NumberRecordsInserted { get; set; }

        /// <summary>
        /// Deletes the existing.
        /// </summary>
        private void DeleteExisting()
        {
            var targetType = Type.GetType(DataContextObject.SelectedDataType.Target.ClrType, true, true);
            var targetDelete = string.Format("delete from {0} entity where entity.Dataset.Id = {1}"
                    , targetType.Name, DataContextObject.DatasetItem.Id);
            var transactionType = typeof(DatasetTransactionRecord);
            var transactionDelete = string.Format("delete from {0} entity where entity.Dataset.Id = {1}"
                    , transactionType.Name, DataContextObject.DatasetItem.Id);

            using (var session = DataContextObject.Provider.SessionFactory.OpenStatelessSession())
            {
                session.CreateQuery(targetDelete)
                .SetTimeout(3500)
                .ExecuteUpdate();

                session.CreateQuery(transactionDelete)
                    .SetTimeout(3500)
                    .ExecuteUpdate();
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
        /// Gets or sets a value indicating whether this <see cref="ValidationViewModel"/> is cancelled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancelled; otherwise, <c>false</c>.
        /// </value>
        bool Cancelled { get; set; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return ModelIsValid;
        }

        /// <summary>
        /// Gets the button caption.
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
                        CurrentProgress = new ProgressState(0, temp); // TODO: based on line count and then perform multi-threading for validating and importing dataset.
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether [model is valid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [model is valid]; otherwise, <c>false</c>.
        /// </value>
        public bool ModelIsValid { get; private set; }

        /// <summary>
        /// The notification synchronize
        /// </summary>
        object notificationSync = new object();

        /// <summary>
        /// Runs the worker.
        /// </summary>
        /// <param name="worker">The worker.</param>
        /// <param name="args">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        public async void RunWorker(BackgroundWorker worker, DoWorkEventArgs args)
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
            int current = 0;
            int total = CurrentProgress.Total;

            var mappings = DataContextObject.RequiredMappings.Concat(DataContextObject.OptionalMappings)
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value) && kvp.Key != null)
                .ToDictionary(k => k.Key, v => v.Value);

            var connElem = Dataservices.Configuration;
            NotifyProgress(this, new ExtendedEventArgs<Action>(ApplyNullResults));
            ValidationStarted(this, EventArgs.Empty);

            using (var session = DataContextObject.Provider.SessionFactory.OpenStatelessSession())
            {
                var importer = DataContextObject.Mapper.CreateBulkImporter(session.Connection);

                try
                {
                    EventHandler<ExtendedEventArgs<SqlConnection>> connectionRequested =
                        (o, e) => e.Data = session.Connection as SqlConnection;
                    importer.ConnectionRequested += connectionRequested;
                    importer.Prepare();
                    using (var conn = Dataservices.ConnectionFactory())
                    {
                        conn.Open();
                        try
                        {
                            using (var cmd = conn.CreateCommand() as OleDbCommand)
                            {
                                cmd.CommandText = string.Format("select {1} from [{0}]", connElem.SelectFrom,
                                    FactorySelectFields(mappings));

                                using (var rdr = cmd.ExecuteReader())
                                {
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

                                                current++;
                                                var progress = new ProgressState(current, total);
                                                worker.ReportProgress((int)Math.Floor(progress.Ratio * 100d), progress);

                                                foreach (var kvp in mappings)
                                                {
                                                    var ordinal = rdr.GetOrdinal(kvp.Value);
                                                    var value = rdr.GetValue(ordinal);
                                                    DataContextObject.Mapper[kvp.Key] = value;
                                                }

                                                //current++;
                                                var currentErrors = DataContextObject.Mapper.Errors.ToList();

                                                var hasErrors = currentErrors.Any();
                                                if (hasErrors)
                                                {
                                                    guardErrors(currentErrors, session, current);
                                                }

                                                if (!hasErrors)
                                                {
                                                    dynamic target = DataContextObject.Mapper.Target;
                                                    var temp = (ValidationStatus)ValidateTarget(target, current, rdr, mappings, session);
                                                    hasErrors |= temp.HasErrors;
                                                    if (!hasErrors)
                                                    {
                                                        (target as DatasetRecord).Dataset =
                                                            DataContextObject.DatasetItem;
                                                        try
                                                        {
                                                            importer.Insert(target);
                                                            NumberRecordsInserted++;
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            GuardError("DATA ERROR", ex.Message, current,
                                                                RecordState.CriticalError, session);
                                                        }
                                                    }
                                                }
                                                DataContextObject.Mapper.Reset();
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        importer.Dispose();
                                    }
                                }
                            }
                        }
                        finally
                        {
                            conn.Close();
                            EventAggregator.GetEvent<DisableWizardButtons>().Publish(true);
                        }
                    }
                }
                finally
                {
                    importer.Dispose();
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
                RecordNumber = DataContextObject.HasHeader ? recordNum + 1 : recordNum
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
        /// 
        /// </summary>
        struct ValidationStatus
        {
            /// <summary>
            /// Gets or sets a value indicating whether this instance has errors.
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
            //bool hasError = false;
            var validationResult = ValidationEngine.ValidateInstance(temp);
           // var updateCount = validationResult.PropertyErrors.Count + validationResult.PropertyWarnings.Count;
            bool hasError = validationResult.PropertyErrors.Any() || validationResult.ClassErrors.Any();

            if (validationResult.ClassErrors.Any())
            {
                hasError = true;
                foreach (var res in validationResult.ClassErrors)
                {                   
                    string member = string.Empty;
                    string msg = null;
                    object value = null;
                    if (res.Property != null)
                    {
                        member = res.Property.Name;

                        if (mappings.ContainsKey(member))
                        {
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
                    }


                    if (res.ErrorState == ValidationErrorState.ValidationError && !string.IsNullOrEmpty(res.Message))
                    {
                        msg = string.Format("{1} (Value excluded: {0})",
                               (value == null
                               ? "<<NULL>>"
                               : string.IsNullOrEmpty(value.ToString()) ? @"""" : value.ToString()),
                               res.Message);
                    }

                    if (res.ErrorState == ValidationErrorState.ValidationError && !string.IsNullOrEmpty(res.Message))
                    {
                        member = "Row Level";
                        msg = res.Message;
                    }

                    GuardError(member, msg, current, res.ErrorState == ValidationErrorState.ExcludedByCrosswalk ? RecordState.ExcludedByCrosswalk : RecordState.ValidationError, session); //RecordState.ExcludedByCrosswalk
                }
            }

            if (validationResult.PropertyErrors.Any())
            {
               // updateCount += validationResult.PropertyErrors.Count;
                hasError = true;
                foreach (var err in validationResult.PropertyErrors.ToList())
                {
                    Element elemTest;
                    ElementDictionary.TryGetValue(err.Property.Name.ToLower(), out elemTest);
                    if (elemTest != null)
                    {
                        GuardError(elemTest.Description,
                        ElementMappingModel.ParseValueError(err.AttemptedValue, err.Message),
                        current, RecordState.ValidationError, session);
                    }
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
        /// Factories the select fields.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <returns></returns>
        private string FactorySelectFields(IEnumerable<KeyValuePair<string, string>> mappings)
        {

            return string.Join(",", mappings.Select(kvp => kvp.Value).Distinct()
                    .Select(val => string.Format(" CVAR([{0}]) as [{0}]", val))).Trim();
        }

        /// <summary>
        /// Gets the feedback.
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
