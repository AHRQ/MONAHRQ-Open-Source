using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Context;
using Monahrq.DataSets.Services;
using Monahrq.Infrastructure.Domain.Import;
using Monahrq.Sdk.Events;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using Monahrq.Theme.Controls.Wizard.Helpers;
using Monahrq.Theme.Controls.Wizard.Models;
using PropertyChanged;
using Monahrq.Sdk.Services.Import;

namespace Monahrq.DataSets.ViewModels.Hospitals.Wizard
{
    [Export(typeof(ImportRegionsDataStep))]
    [ImplementPropertyChanged]
    public class ImportRegionsDataStep : WizardStepViewModelBase<GeographicContext>, IPartImportsSatisfiedNotification
    {
        private GeographicContext Context { get; set; }
        public DelegateCommand ImportDataFileCommand { get; set; }

        
        [Import(ImporterContract.CustomRegion)]
        IEntityFileImporter RegionImporter { get; set; }
 
        public ImportRegionsDataStep(GeographicContext dataContextObject)
            : base(dataContextObject)
        {
            RegionImporter =
                ServiceLocator.Current.GetInstance<IEntityFileImporter>(ImporterContract.CustomRegion);
            ImportDataFileCommand = new DelegateCommand(ExecuteOpenFileDialog);
        }

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

        //todo bad code alert fix it
        private void ExecuteOpenFileDialog()
        {
            RegionImporter.Execute();
        }

        public override string DisplayName
        {
            get { return "Regions Data"; }
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

        public override void BeforeShow()
        {
            base.BeforeShow();

        }



        public void OnImportsSatisfied()
        {
            RegionImporter.Imported += delegate
            {
                EventAggregator.GetEvent<ResumeNormalProcessingEvent>().Publish(Empty.Value);
            };

            RegionImporter.Importing += delegate
            {
                EventAggregator.GetEvent<PleaseStandByEvent>().Publish(RegionImporter.CreatePleaseStandByEventPayload());
            };

            ImportDataFileCommand.RaiseCanExecuteChanged(); 
        }
    }
}