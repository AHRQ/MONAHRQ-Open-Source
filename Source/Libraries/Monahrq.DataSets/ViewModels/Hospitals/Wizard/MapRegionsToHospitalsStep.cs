using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using Monahrq.DataSets.Context;
using Monahrq.Theme.Controls.Wizard.Helpers;
using Monahrq.Theme.Controls.Wizard.Models;
using PropertyChanged;

namespace Monahrq.DataSets.ViewModels.Hospitals.Wizard
{
    [Export]
    [ImplementPropertyChanged]
    public class MapRegionsToHospitalsStep : WizardStepViewModelBase<GeographicContext>
    {
        public MapRegionsToHospitalsStep(GeographicContext dataContextObject)
            : base(dataContextObject)
        {
            Completed = string.Empty;
        }

        private void _bgwLoadClients_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
        }

        private void _bgwLoadClients_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            Thread.Sleep(10);
            Completed = "MAPPING IS COMPLETED";
            DataContextObject.Finish();

        }

        private void _bgwLoadClients_DoWork(object sender, DoWorkEventArgs e)
        {
            for(int i=0;i<100;i++)
            {
                Thread.Sleep(10);
                _worker.ReportProgress(i, null);
            }

            isFinished = true;

        }

        private string _completed;
        public string Completed
        {
            get { return _completed; }
            set { _completed = value;
                RaisePropertyChanged(()=>Completed);
            }

        }
        private int _progress;
        public int Progress
        {
            get { return _progress; }
            set { _progress = value;
                RaisePropertyChanged(()=>Progress);
            }
        }
        BackgroundWorker _worker;


        public override string DisplayName
        {
            get { return "Map Regions to Hospitals"; }
        }

        private bool isFinished;
        public override bool IsValid()
        {
            return isFinished;
        }

        public override RouteModifier OnNext()
        {
            DataContextObject.Reset();
            return base.OnNext();
        }

        public override void BeforeShow()
        {
            Thread.Sleep(100);
            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.DoWork += new DoWorkEventHandler(_bgwLoadClients_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bgwLoadClients_RunWorkerCompleted);
            _worker.ProgressChanged += new ProgressChangedEventHandler(_bgwLoadClients_ProgressChanged);
            _worker.RunWorkerAsync();

        }
    }
}