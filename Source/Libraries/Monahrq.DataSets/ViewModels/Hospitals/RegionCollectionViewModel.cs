using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.DataSets.Events;
using Monahrq.DataSets.Services;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Hospitals.Mapping;
using Monahrq.Sdk.Events;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Sdk.Services.Import;
using System.Collections.Specialized;

namespace Monahrq.DataSets.ViewModels.Hospitals
{
    [Export]
    public class RegionCollectionViewModel : BaseViewModel, INavigationAware, IPartImportsSatisfiedNotification
    {
        public DelegateCommand ImportRegionCommand{ get; set; }
        public DelegateCommand DeleteRegionCommand { get; set; }
        public DelegateCommand EditRegionCommand { get; set; }
        public DelegateCommand NewRegionCommand { get; set; }

       

        [Import]
        public IRegionManager RegionManager { get; set; }

        public ObservableCollection<RegionViewModel> RegionViewModels { get; set; }

        [Import(DataContracts.MAPPING_REFERENCE, AllowRecomposition = true)]
        public HospitalMappingReference RegionMappingReference { get; set; }

        [Import]
        public IHospitalDataService HospitalDataService { get; set; }

        [Import(ImporterContract.CustomRegion)]
        public IEntityFileImporter RegionImporter { get; set; }

        [ImportingConstructor]
        public RegionCollectionViewModel(

            [Import(DataContracts.MAPPING_REFERENCE, AllowRecomposition = true)]HospitalMappingReference RegionMappingReference)
        {
            var x = RegionMappingReference;
            ImportRegionCommand = new DelegateCommand(OnRegionImport, CanImport);
            DeleteRegionCommand = new DelegateCommand(OnRegionDelete, CanDelete);
            NewRegionCommand = new DelegateCommand(OnNewRegion, CanNewRegion);
            EventAggregator.GetEvent<GeorgraphicalContextChangeEvent>().Subscribe(OnContextChanged);
            EventAggregator.GetEvent<RegionCollectionChangedEvent>().Subscribe(OnRegionCollectionChanged);
            EventAggregator.GetEvent<EntityImportedEvent<CustomRegion>>().Subscribe(OnCustomRegionImported);
        }
  

        #region Commands
        private void _reset()
        {
            RegionTitle = string.Empty;
            SelectedState = null;
            ClearErrors(ExtractPropertyName(() => RegionTitle));
            Committed = true;
        }
        private bool CanNewRegion()
        {
            return !Committed && !HasErrors && SelectedState != null;
        }

        private void OnNewRegion()
        {
            var newRegion = HospitalDataService.AddNewRegion(RegionTitle, SelectedState);
            if (newRegion == null) return;

            var msg = String.Format("Custom region {0} with state {1} has been added", newRegion.Name, newRegion.Region.State);
            EventAggregator.GetEvent<GenericNotificationEvent>().Publish(msg);
            _reset();
        }

        private bool CanDelete()
        {
            return true;
        }

        private bool CanImport()
        {
            return true;
        }

        private void OnRegionDelete()
        {
            var regionToDelete = SelectedRegion;
               
            regionToDelete.DeleteRegionCommand.Execute();
        }

        private void OnRegionImport()
        {
            RegionImporter.Execute();
        }
      
        #endregion


        #region Properties

        private string _regionTitle;
        public string RegionTitle
        {
            get { return _regionTitle; }
            set
            {
                _regionTitle = value;
                RaisePropertyChanged(() => RegionTitle);
                _validateName(ExtractPropertyName(() => RegionTitle), RegionTitle);
                NewRegionCommand.RaiseCanExecuteChanged();
            }
        }

        private void _validateName(string property, string val)
        {
            ClearErrors(property);
            if (string.IsNullOrWhiteSpace(val))
            {
                SetError(property, "Region name cannot be empty");
            }
        }

        private ObservableCollection<State> _stateABCollection;
        public ObservableCollection<State> StateABCollection
        {
            get { return _stateABCollection; }
            set
            {
                _stateABCollection = value;

                RaisePropertyChanged(() => StateABCollection);
            }
        }

        private State _selectedState;
        public State SelectedState
        {
            get { return _selectedState; }
            set
            {
                _selectedState = value;

                RaisePropertyChanged(() => SelectedState);
                NewRegionCommand.RaiseCanExecuteChanged();

            }
        }

        private RegionViewModel _selectedRegion;
        public RegionViewModel SelectedRegion
        {
            get { return _selectedRegion; }
            set
            {
                _selectedRegion = value;
                RaisePropertyChanged(() => SelectedRegion);
            }
        }

        private ObservableCollection<RegionViewModel> _regionCollection;
        public ObservableCollection<RegionViewModel> RegionCollection
        {
            get { return _regionCollection; }
            set
            {
                _regionCollection = value;
                RaisePropertyChanged(() => RegionCollection);
            }
        }

        //public ListCollectionView RegionCollection
        //{
        //    get;
        //    set;
        //}

        #endregion

        #region Navigation

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //todo
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //todo
        }

        #endregion

        public void OnImportsSatisfied()
        {
            EventAggregator.GetEvent<GeorgraphicalContextChangeEvent>().Subscribe(OnContextChanged);
            EventAggregator.GetEvent<RegionCollectionChangedEvent>().Subscribe(OnRegionCollectionChanged);
            _initData();
        }

        private void _initData()
        {
         
            RegionImporter.Imported +=
                delegate { EventAggregator.GetEvent<ResumeNormalProcessingEvent>().Publish(Empty.Value); };

            RegionImporter.Importing +=
                delegate
                    {
                        EventAggregator.GetEvent<PleaseStandByEvent>().Publish(RegionImporter.CreatePleaseStandByEventPayload());
                    };

           // RegionViewModels.CollectionChanged += (o, e) => RegionCollectionChanged(this, e);
        }

        private void OnRegionCollectionChanged(IEnumerable<RegionViewModel> nRegionViewModels)
        {
            _refreshCollection(null);
        }

        
        private void OnCustomRegionImported(CustomRegion newRegion)
        {
            var vm = new RegionViewModel(newRegion);
            HospitalDataService.RegionViewModels.Add(vm);

            _refreshCollection(vm);
        }

        
        private void _refreshCollection(RegionViewModel vm)
        {
            if(StateABCollection==null)
            StateABCollection=  HospitalDataService.RegionMappingReference == null ? 
                        new ObservableCollection<State>()
                        : new ObservableCollection<State>(HospitalDataService.RegionMappingReference.States);

            if (vm != null)
            {
                SelectedRegion = vm;
            }
            if (SelectedRegion == null && HospitalDataService.RegionViewModels.Any())
                SelectedRegion = HospitalDataService.RegionViewModels.FirstOrDefault();

                //RegionCollection = CollectionViewSource.GetDefaultView(HospitalDataService.RegionViewModels) as ListCollectionView;
            RegionCollection = HospitalDataService.RegionViewModels;
            
            RaisePropertyChanged(() => StateABCollection);
            RaisePropertyChanged(() => RegionCollection);
        }

     
        private void OnContextChanged(bool refresh)
        { 
            _refreshCollection(null);
        }
    }
}
