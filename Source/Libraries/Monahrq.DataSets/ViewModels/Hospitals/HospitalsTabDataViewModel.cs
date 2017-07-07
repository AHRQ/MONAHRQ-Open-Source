using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Services;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Hospitals.Mapping;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Events; 
using Monahrq.Sdk.Regions;
using NHibernate.Linq;
using Region = Monahrq.Infrastructure.Entities.Domain.Hospitals.Region;
using Monahrq.Sdk.Services.Import;
using PropertyChanged;
using Monahrq.DataSets.Events;

namespace Monahrq.DataSets.ViewModels.Hospitals
{
    [ImplementPropertyChanged]
    [Export(typeof(HospitalsTabDataViewModel))]
    public class HospitalsTabDataViewModel : BaseViewModel, INavigationAware, IPartImportsSatisfiedNotification
    {
        [Import(DataContracts.MAPPING_REFERENCE, AllowRecomposition = true)]
        public HospitalMappingReference RegionMappingReference { get; set; }

        public DelegateCommand NewGeoContextCommand { get; set; }

        public DelegateCommand ImportDataFileCommand { get; set; }

        public IRegionManager RegionManager { get; set; }
        public IHospitalDataService HospitalDataService { get; set; }


        private IEntityFileImporter RegionImporter { get; set; }

        private IEntityFileImporter HospitalImporter { get; set; }


        [ImportingConstructor]
        public HospitalsTabDataViewModel(IRegionManager regionManager, IHospitalDataService hospitalDataService, IEventAggregator eventAggregator)
        {
            NewGeoContextCommand = new DelegateCommand(OnNewGeoContext);
            RegionManager = regionManager;
            HospitalDataService = hospitalDataService;
        }

        private void OnNewGeoContext()
        {
            //ServiceLocator.Current.GetInstance<IEventAggregator>()
            //    .GetEvent<WizardNavigateSelectStatesEvent>().Publish( new Uri(ViewNames.GeoContextWizard, UriKind.Relative));
        }







        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var x = navigationContext;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            var x = navigationContext;
        }


    }
}
