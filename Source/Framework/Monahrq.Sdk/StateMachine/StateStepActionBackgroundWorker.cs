using System;
using System.ComponentModel;

namespace Monahrq.Sdk.StateMachine
{

    /// <summary>
    ///     Workflow for running a background process
    /// </summary>
    public class StateStepBackgroundWorker : IStateStep
    {
        private readonly BackgroundWorker _bg;
        private readonly Action<BackgroundWorker> _doWork;
        private readonly Action<BackgroundWorker, ProgressChangedEventArgs> _reportProgress;

        /// <summary>
        ///     No progress to report
        /// </summary>
        /// <param name="doWork">Worker</param>
        public StateStepBackgroundWorker(Action<BackgroundWorker> doWork)
            : this(doWork, null)
        {
        }

        /// <summary>
        ///     Spawn a workflow background
        /// </summary>
        /// <param name="doWork">Work to do</param>
        /// <param name="reportProgress">Progress to report</param>
        public StateStepBackgroundWorker(Action<BackgroundWorker> doWork, Action<BackgroundWorker, ProgressChangedEventArgs> reportProgress)
        {
            _bg = new BackgroundWorker { WorkerReportsProgress = reportProgress != null, WorkerSupportsCancellation = false };
            _bg.DoWork += BgDoWork;
            _bg.RunWorkerCompleted += BgRunWorkerCompleted;
            if (reportProgress != null)
            {
                _reportProgress = reportProgress;
                _bg.ProgressChanged += BgProgressChanged;
            }
            _doWork = doWork;
        }

        /// <summary>
        ///     Supply your own BackgroundWorker.
        /// </summary>
        /// <param name="backgroundWorker"></param>
        public StateStepBackgroundWorker(BackgroundWorker backgroundWorker)
        {
            _bg = backgroundWorker;
            _bg.RunWorkerCompleted += BgRunWorkerCompleted;
        }

        /// <summary>
        ///     Progress  change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BgProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _reportProgress(_bg, e);
        }

        /// <summary>
        /// Bgs the run worker completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        void BgRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _bg.DoWork -= BgDoWork;
            _bg.RunWorkerCompleted -= BgRunWorkerCompleted;
            if (_reportProgress != null)
            {
                _bg.ProgressChanged -= BgProgressChanged;
            }
            Invoked();
        }

        /// <summary>
        /// Bgs the do work.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        void BgDoWork(object sender, DoWorkEventArgs e)
        {
            _doWork(_bg);
        }

        /// <summary>
        /// Invokes this instance.
        /// </summary>
        public void Invoke()
        {
            _bg.RunWorkerAsync();
        }

        /// <summary>
        /// Gets or sets the invoked.
        /// </summary>
        /// <value>
        /// The invoked.
        /// </value>
        public Action Invoked { get; set; }
    }
}
