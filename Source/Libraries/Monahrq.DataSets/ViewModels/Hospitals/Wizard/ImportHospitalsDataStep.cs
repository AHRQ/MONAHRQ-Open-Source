using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Win32;
using Monahrq.DataSets.Context;
using Monahrq.DataSets.Services;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Services.Import;
using Monahrq.Theme.Controls.Wizard.Helpers;
using Monahrq.Theme.Controls.Wizard.Models;
using PropertyChanged;

namespace Monahrq.DataSets.ViewModels.Hospitals.Wizard
{
    [Export]
    [ImplementPropertyChanged]
    public class ImportHospitalsDataStep : WizardStepViewModelBase<GeographicContext>, IPartImportsSatisfiedNotification
    {
        private GeographicContext Context { get; set; }
        public DelegateCommand ImportDataFileCommand { get; set; }



        [ImportingConstructor]
        public ImportHospitalsDataStep(GeographicContext dataContextObject)
            : base(dataContextObject)
        {
            HospitalImporter = ServiceLocator.Current.GetInstance<IEntityFileImporter>(ImporterContract.Hospital);
            ImportDataFileCommand = new DelegateCommand(ExecuteOpenFileDialog);

            EventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

            HospitalImporter.Imported += delegate
            {
                EventAggregator.GetEvent<ResumeNormalProcessingEvent>().Publish(Empty.Value);
            };

            HospitalImporter.Importing += delegate
            {
                EventAggregator.GetEvent<PleaseStandByEvent>().Publish(HospitalImporter.CreatePleaseStandByEventPayload());
            };
        }

        private bool CanOpen()
        {
            return SelectedPath.Length > 5;
        }


        public DelegateCommand OpenCommand { get; set; }
        private string _selectedPath;
        public string SelectedPath
        {
            get { return _selectedPath; }
            set
            {
                _selectedPath = value;
                RaisePropertyChanged(() => SelectedPath);
                ImportDataFileCommand.RaiseCanExecuteChanged();
            }
        }

        private string _defaultPath;
        //todo bad code alert, why can we use MSFT parsers and a descent . The type should be injected, and not used by inheretence , it makes it harder to reuse
        //todo 1. 1 commom lib for this action
        //todo 1. Messaging interface on methods, with interceptors to handle : exceptions and notifificatios
        //todo 2. Read Castle project expception handling. SINGLE RESPONSOBILITY PRINCIPLE
        //TODO: WHAT IS THE DIFFRENCE BETWEEN WINDOWS.FORM VERSUS WIN.32 ON WPF APP? HOW will it affect RT, can u writte your own dialog, the default ones are ugly

        private void ExecuteOpenFileDialog()
        {
            HospitalImporter.Execute();
        }

        public override string DisplayName
        {
            get { return "Hospitals Data"; }
        }

        public override bool IsValid()
        {
            return true;
        }

        public override RouteModifier OnNext()
        {
            DataContextObject.Reset();
            return base.OnNext();
        }

        [Import(ImporterContract.Hospital)]
        IEntityFileImporter HospitalImporter { get; set; }

        public override void BeforeShow()
        {

        }


        public void OnImportsSatisfied()
        {


        }


    }
}