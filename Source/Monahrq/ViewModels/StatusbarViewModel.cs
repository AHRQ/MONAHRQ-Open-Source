using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Diagnostics;
using Monahrq.Infrastructure.Types;
using Monahrq.Sdk.Extensions;
using PropertyChanged;

namespace Monahrq.ViewModels
{
    /// <summary>
    /// View model for status bar.
    /// </summary>
    /// <seealso cref="Monahrq.ViewModels.BaseNotify" />
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    [ImplementPropertyChanged]
    [Export(typeof(StatusbarViewModel))]
    public class StatusbarViewModel : BaseNotify, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// The statusbar processing default text
        /// </summary>
        private const string STATUSBAR_PROCESSING_DEFAULT_TEXT = "Ready";

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusbarViewModel"/> class.
        /// </summary>
        public StatusbarViewModel()
        {
            ResetStatusbar();
            GetFreeAvailableTotalSpace();
            GetAvailableVirtualMemory();
        }

        #region Properties
        /// <summary>
        /// Gets or sets the processing text.
        /// </summary>
        /// <value>
        /// The processing text.
        /// </value>
        public string ProcessingText { get; set; }
        /// <summary>
        /// Gets or sets the progress.
        /// </summary>
        /// <value>
        /// The progress.
        /// </value>
        public int Progress { get; set; }
        /// <summary>
        /// Gets or sets the show progressbar.
        /// </summary>
        /// <value>
        /// The show progressbar.
        /// </value>
        public Visibility ShowProgressbar { get; set; }
        /// <summary>
        /// Gets or sets the avaliable space.
        /// </summary>
        /// <value>
        /// The avaliable space.
        /// </value>
        public string AvaliableSpace { get; set; }
        /// <summary>
        /// Gets or sets the avaliable memory.
        /// </summary>
        /// <value>
        /// The avaliable memory.
        /// </value>
        public string AvaliableMemory { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is indeterminate.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is indeterminate; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndeterminate { get; set; }

        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import]
        public IEventAggregator EventAggregator { get; set; }
        #endregion

        #region Methods & Events
        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            EventAggregator.GetEvent<StatusbarUpdateEvent>().Subscribe(HandleUpdate);
            RefreshAvailableTotalSpace();
        }

        /// <summary>
        /// Handles the update.
        /// </summary>
        /// <param name="p">The p.</param>
        private async void HandleUpdate(StatusbarUpdateEventObject p)
        {
            //Application.Current.DoEvents();
            //await Application.Current.Dispatcher.InvokeAsync(()=>
            //{
                Application.Current.DoEventsUI();

                if (p == null)
                    return;

                if (ShowProgressbar == Visibility.Hidden)
                    ShowProgressbar = Visibility.Visible;

                RaisePropertyChanged(() => ShowProgressbar);

                IsIndeterminate = p.IsIndeterminate;
                RaisePropertyChanged(() => IsIndeterminate);

                ProcessingText = !string.IsNullOrEmpty(p.ProgressText)
                    ? p.ProgressText
                    : STATUSBAR_PROCESSING_DEFAULT_TEXT;

                RaisePropertyChanged(() => ProcessingText);

                if (p.Reset)
                {
                    ResetStatusbar();
                    Application.Current.DoEventsUI();
                    return;
                }

                if (p.Progress >= 0)
                {
                    Progress = p.Progress;
                    RaisePropertyChanged(() => Progress);
                }
                Application.Current.DoEventsUI();
                return;
            //}, DispatcherPriority.Normal );
        }

        /// <summary>
        /// Resets the statusbar.
        /// </summary>
        internal void ResetStatusbar()
        {
            Progress = 0;
            RaisePropertyChanged(() => Progress);
            ShowProgressbar = Visibility.Hidden;
            RaisePropertyChanged(() => ShowProgressbar);
            //await Task.Delay(1000);

            ProcessingText = STATUSBAR_PROCESSING_DEFAULT_TEXT;
            RaisePropertyChanged(() => ProcessingText);
        }

        /// <summary>
        /// Refreshes the available total space.
        /// </summary>
        private void RefreshAvailableTotalSpace()
        {
            //  DispatcherTimer setup
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(300000);
            dispatcherTimer.Start();
        }

        /// <summary>
        /// Handles the Tick event of the dispatcherTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            GetFreeAvailableTotalSpace();
            GetAvailableVirtualMemory();
        }

        /// <summary>
        /// Gets the free available total space.
        /// </summary>
        private async void GetFreeAvailableTotalSpace()
        {
            await Task.Run(() =>
            {
                AvaliableSpace = MonahrqDiagnostic.GetHDAvailableFreeSpaceAsString();
            });
        }

        /// <summary>
        /// Gets the available virtual memory.
        /// </summary>
        private async void GetAvailableVirtualMemory()
        {
            await Task.Run(() =>
            {
                AvaliableMemory = MonahrqDiagnostic.GetTotalAmountOfFreeVirtualMemoryAsString();

            });
        }

        #endregion
    }
}