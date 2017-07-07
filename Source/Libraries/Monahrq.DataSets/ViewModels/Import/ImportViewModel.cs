using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Model;
using Monahrq.DataSets.Services;
using Monahrq.Default.DataProvider.Administration.File;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.Events;
using Monahrq.Theme.Controls.Wizard.Models;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Extensibility.Data.Providers;
using Monahrq.Sdk.Extensibility.ContentManagement.Records;

namespace Monahrq.DataSets.ViewModels.Import
{
    [ImplementPropertyChanged]
    public class ImportViewModel :
        WizardStepViewModelBase<DatasetContext> 
    {
        public string DataSetDate { get; set; }
        public string DataSetName { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public int DatabaseErrorMessages { get; set; }
        public string Status { get; set; }

        public event EventHandler ImportStarted = delegate { };
        public event EventHandler<EventArgsBase<Action>> NotifyProgress = delegate { };
        public event EventHandler ImportSucceeded = delegate { };
        public event EventHandler ImportFailed = delegate { };
        public event EventHandler ImportComplete = delegate { };

        public ProgressState CurrentProgress
        {
            get;
            set;
        }

        public ImportViewModel(DatasetContext c)
            : base(c)
        {
            ApplyResults();
        }

        private void ImportTargets()
        {
            var type = Type.GetType(DataContextObject.SelectedDataType.Target.ClrType);
            ApplyNullResults();
            RunImport();
        }

        private void RunImport()
        {
            IsRunning = true;
            ImportStarted(this, EventArgs.Empty);
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
                NotifyProgress(this, new EventArgsBase<Action>(
                    () =>
                    {
                        notifyAction();
                    }));
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
                timer.Dispose();
                timer = null;
                var wait = new ManualResetEvent(false);
                var thr = new Thread(() =>
                    NotifyProgress(this, new EventArgsBase<Action>(
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
                    if (ImportResultsView.Count > 0)
                    {
                        ImportSucceeded(this, EventArgs.Empty);
                    }
                    else
                    {
                        ImportFailed(this, EventArgs.Empty);
                    }
                    OnImportCompleted(EventArgs.Empty);
                    Application.Current.DoEvents();
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

        protected virtual void OnImportCompleted(EventArgs eventArgs)
        {
            ImportComplete(this, eventArgs);
        }

        private void GuardError(int recordNum, Exception ex)
        {
            var errors = ImportResults ?? new List<ImportResultViewModel>();
            var result = new ImportResultViewModel(recordNum, ex);
            errors.Add(result);
            if (ImportResults == null)
            {
                ImportResults = errors;
            }
        }

        public void RunWorker(BackgroundWorker worker, DoWorkEventArgs args)
        {
            var getProgress = args.Argument as Func<ProgressState>
                        ?? new Func<ProgressState>(() => ProgressState.Empty);
            CurrentProgress = getProgress() ?? ProgressState.Empty;
            ImportStarted(this, EventArgs.Empty);
            var type = Type.GetType(DataContextObject.SelectedDataType.Target.ClrType);
            var sessionFactory = DataContextObject.ShellContext.SessionFactoryHolder.GetSessionFactory();
            CurrentProgress = new ProgressState(0, DataContextObject.TargetsToImport.Count);
            using (NHibernate.ISession session = sessionFactory.OpenSession())
            {
                var current = 0;
                var total = DataContextObject.TargetsToImport.Count;
                int batchSize = Settings.Default.BatchSize;
                using (var tx = session.BeginTransaction())
                {
                    while (current < total && !args.Cancel && !Cancelled)
                    {
                        if (worker.CancellationPending)
                        {
                            args.Cancel = true;
                        }
                        else
                        {
                            var target = DataContextObject.TargetsToImport[current];
                            (target as ContentPartRecord).ContentItemRecord = DataContextObject.CurrentContentItem;
                            try
                            {
                                session.Save(DataContextObject.TargetsToImport[current]);
                            }
                            catch (Exception ex)
                            {
                                GuardError(current, ex);
                            }
                            finally
                            {
                                if (current % batchSize == 0)
                                { //20, same as the ADO batch size
                                    session.Flush();
                                    session.Clear();
                                }
                                current++;
                                var progress = new ProgressState(current, total);
                                worker.ReportProgress((int)Math.Floor(progress.Ratio * 100d), progress);
                                if (current % 1000 == 0)
                                {
                                    Application.Current.DoEvents();
                                }
                            }
                        }
                    }
                    session.Flush();
                    session.Clear();
                    tx.Commit();
                }
                session.Close();
            }
        }

        private void ApplyNullResults()
        {
            ImportResults = null;
            ApplyResults();
        }

        private void ApplyResults()
        {
            var results = ImportResults ?? Enumerable.Empty<ImportResultViewModel>();
            LazyResults = new Lazy<ListCollectionView>(() =>
            {
                return CollectionViewSource
                    .GetDefaultView(new ObservableCollection<ImportResultViewModel>(results))
                    as ListCollectionView;
            });
        }

        List<ImportResultViewModel> ImportResults
        {
            get;
            set;
        }

        Lazy<ListCollectionView> LazyResults { get; set; }
        ListCollectionView ImportResultsView
        {
            get
            {
                return LazyResults == null ?
                    new ListCollectionView(Enumerable.Empty<ImportResultViewModel>().ToList())
                    : LazyResults.Value;
            }
        }

        public override string DisplayName
        {
            get { return "Import data"; }
        }

        public override bool IsValid()
        {
            return !IsRunning;
        }

        IDataContextServices Dataservices { get; set; }

        public void LoadModel()
        {
            Dataservices = DataContextObject.Services;
            DataSetName = DataContextObject.SelectedDataType.DataTypeName;
            DataSetDate = DataContextObject.Entry.TimePeriod;
            CurrentProgress = new ProgressState(0, DataContextObject.TargetsToImport.Count);
            var fi = new FileInfo(DataContextObject.DatasourceDefinition.CurrentFile);
            FileSize = fi.Length;
            FileName = DataContextObject.DatasourceDefinition.CurrentFile;
            DatabaseErrorMessages = 0;
            RunImport();
        }


        public string CurrentFile
        {
            get
            {
                var ds = Dataservices ?? this.DataContextObject.Services;
                return ds.CurrentFile;
            }
        }
        

        public bool IsRunning { get; set; }

        public bool Cancelled { get; set; }
    }

}
