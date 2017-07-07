using System.Windows;
using Monahrq.DataSets.Model;
using Monahrq.Theme.Controls.Wizard.Models;
using PropertyChanged;
using System.IO;
using System.Data.Common;
using System;
using System.Linq;
using System.Threading;
using System.ComponentModel;
using System.Data;
using Monahrq.Sdk.Extensions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Prism.Events;
using Monahrq.DataSets.Services;
using System.Windows.Input;
using Monahrq.Infrastructure.Entities.Events;

namespace Monahrq.DataSets.ViewModels
{
    [ImplementPropertyChanged]
    public class DefaultWizardFileReadabilityViewModel :
        WizardStepViewModelBase<DatasetContext>
    {
        /// <summary>
        /// Occurs when [notify progress].
        /// </summary>
        public event EventHandler<ExtendedEventArgs<Action>> NotifyProgress = delegate { };
        /// <summary>
        /// Occurs when [verified].
        /// </summary>
        public event EventHandler Verified = delegate { };
        /// <summary>
        /// Occurs when [failed].
        /// </summary>
        public event EventHandler Failed = delegate { };

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWizardFileReadabilityViewModel"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public DefaultWizardFileReadabilityViewModel(DatasetContext dataContext)
            : base(dataContext)
        {
            CurrentProgress = ProgressState.Empty;
            _isValid = false;
        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }
        /// <summary>
        /// Gets or sets the dataservices.
        /// </summary>
        /// <value>
        /// The dataservices.
        /// </value>
        IDataContextServices Dataservices { get; set; }

        /// <summary>
        /// Loadeds this instance.
        /// </summary>
        public void Loaded()
        {
            Dataservices = DataContextObject.Services;
            var providerFactory = new DataproviderFactory(Dataservices);
            providerFactory.InitDataProvider();
            InitModel();
            if (DataContextObject.Histogram != null) return;
            var worker = new BackgroundWorker();
            var sync = new object();
            ProgressState interimProgress = new ProgressState(0, CurrentProgress.Total);

            Action notifyAction = () =>
                {
                    {
                        lock (sync)
                        {
                            CurrentProgress = interimProgress;
                            Status = string.Format("Progress: {0}", interimProgress.Ratio.ToString("P0"));
                        }
                    }
                };
            var timer = new Timer((state) =>
               {
                   NotifyProgress(this, new ExtendedEventArgs<Action>(notifyAction));
               }, null, 1, 1000);

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += (o, e) =>
                {
                    RunWorker(worker, e);
                };
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
                   _isValid = !Cancelled;
                   timer.Dispose();
                   timer = null;
                   NotifyProgress(this, new ExtendedEventArgs<Action>(notifyAction));
                   if (_isValid)
                   {
                       Verified(this, EventArgs.Empty);
                   }
                   OnPropertyChanged();
                   // WPF CommandManager periodically calls IsValid to see if the Next/Done button should be enabled. 
                   // In multi-threaded wizard steps, IsValid returns the value of the Done flag. Call InvalidateRequerySuggested here
                   // on the UI thread after setting the Done flag to force WPF to call IsValid now so the Next/Done button will become enabled. 
                   //Application.Current.DoEvents();
                   CommandManager.InvalidateRequerySuggested();
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
                .GetEvent<WizardBackEvent>().Subscribe(evnt =>
                {
                    Cancelled = true;
                    if (worker.CancellationPending)
                    {
                        return;
                    }
                    worker.CancelAsync();
                });


            worker.RunWorkerAsync(new Func<ProgressState>(() => interimProgress));
            Thread.Sleep(0);
        }

        /// <summary>
        /// Runs the worker.
        /// </summary>
        /// <param name="worker">The worker.</param>
        /// <param name="args">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        private void RunWorker(BackgroundWorker worker, DoWorkEventArgs args)
        {
            var getProgress = args.Argument as Func<ProgressState>
                            ?? new Func<ProgressState>(() => ProgressState.Empty);

            CurrentProgress = getProgress() ?? ProgressState.Empty;

            int current = 0;
            int total = CurrentProgress.Total;
            using (var conn = Dataservices.ConnectionFactory())
            {
                try
                {
                    using (var cmd = Dataservices.Controller.ProviderFactory.CreateCommand())
                    {
                        cmd.Connection = conn as DbConnection;
                        cmd.CommandText = string.Format("select * from [{0}]", Dataservices.Configuration.SelectFrom);
                        conn.Open();
                        using (var da = Dataservices.Controller.ProviderFactory.CreateDataAdapter())
                        {
                            da.SelectCommand = cmd;
                            using (var dt = new DataTable())
                            {
                                da.FillSchema(dt, SchemaType.Source);
                                DataContextObject.Histogram = new Histogram(dt.Columns.OfType<DataColumn>());
                            }
                        }
                        var histogram = DataContextObject.Histogram;
                        using (var rdr = cmd.ExecuteReader())
                        {
                            var arry = Array.CreateInstance(typeof(object), histogram.FieldCount) as object[];
                            while (rdr.Read() && !args.Cancel)
                            {
                                if (worker.CancellationPending)
                                {
                                    args.Cancel = true;
                                }
                                else
                                {
                                    current++;
                                    var cnt = rdr.GetValues(arry);
                                    histogram.Load(arry);
                                    var temp = new ProgressState(current, total);
                                    worker.ReportProgress((int)Math.Floor(temp.Ratio * 100d), temp);
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
        /// Initializes the model.
        /// </summary>
        private void InitModel()
        {
            if (DataContextObject.Histogram != null)
            {
                Status = "Data Loaded";
                return;
            }
            Status = "Initializing dataset...";
            Application.Current.DoEvents();
            Feedback = String.Empty;
            FileSize = new FileInfo(CurrentFile).Length;
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
        /// Gets or sets a value indicating whether this <see cref="DefaultWizardFileReadabilityViewModel"/> is cancelled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancelled; otherwise, <c>false</c>.
        /// </value>
        bool Cancelled { get; set; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public override string DisplayName
        {
            get { return "Processing File"; }
        }

        /// <summary>
        /// The is valid
        /// </summary>
        private bool _isValid;

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return _isValid;
        }

        /// <summary>
        /// Gets or sets the current progress.
        /// </summary>
        /// <value>
        /// The current progress.
        /// </value>
        public ProgressState CurrentProgress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        /// <value>
        /// The size of the file.
        /// </value>
        public long FileSize { get; set; }
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
                return  ds.CurrentFile;
            }
        }

        /// <summary>
        /// Gets or sets the feedback.
        /// </summary>
        /// <value>
        /// The feedback.
        /// </value>
        public string Feedback { get; set; }
    }
}
