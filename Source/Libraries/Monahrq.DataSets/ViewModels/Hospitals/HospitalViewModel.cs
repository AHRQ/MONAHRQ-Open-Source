using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Events;
using Monahrq.DataSets.Services;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Entities.Domain;
using System.Collections.Generic;

namespace Monahrq.DataSets.ViewModels.Hospitals
{
    using Monahrq.Infrastructure.Entities.Domain.Hospitals;
    using Monahrq.Infrastructure.Services.Hospitals;
    using System.Windows.Data;
    using System.ComponentModel;
    using Monahrq.Sdk.Modules.Settings;
    using Monahrq.DataSets.HospitalRegionMapping.Categories;
    using Monahrq.DataSets.HospitalRegionMapping.Events;
    using Monahrq.DataSets.HospitalRegionMapping.Hospitals;
    using Monahrq.DataSets.HospitalRegionMapping.CustomRegions;
    using Monahrq.DataSets.HospitalRegionMapping;

    public interface IHospitalViewModel
    {
        Hospital Hospital { get; set; }
        ObservableCollection<HospitalCategory> AvailableCategories { get; set; }
        ObservableCollection<RegionViewModel> AvailableRegions { get; set; }
        ObservableCollection<string> AvailableProviders { get; set; }
        ObservableCollection<string> AvailableStates { get; set; }
        ObservableCollection<string> AvailableCounties { get; set; }
    }

    [Export(typeof(HospitalViewModel))]
    public class HospitalViewModel : HospitalRegionMappingViewModel
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public IHospitalDataService HospitalDataService
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IHospitalDataService>();
            }
        }

        public HospitalViewModel()
        {
            EventAggregator.GetEvent<CustomRegionsViewModelSavedEvent>().Subscribe(ReconcileRegions);
            EventAggregator.GetEvent<CategoriesViewModelSavedEvent>().Subscribe(ReconcileCategories);
        }

        public ICollectionView RegionViewModels
        {
            get;
            private set;
        }

        IModuleController ModuleController { get; set; }

        public HospitalViewModel(Hospital hospital, IEnumerable<RegionViewModel> regionViewModels, CmsCollectionViewModel cms)
            : this()
        {

            ModuleController = ServiceLocator.Current.GetInstance<IModuleController>();
            Hospital = hospital;
            CmsCollection = new CmsCollectionViewModel();
            RegionViewModels = CollectionViewSource.GetDefaultView(regionViewModels);
            var region = HospitalRegion.Default.SelectedRegionType == typeof(HealthReferralRegion)
                ? (RegionWithCity)hospital.HealthReferralRegion
                : (RegionWithCity)hospital.HospitalServiceArea;
            if (Hospital.SelectedRegion == null)
            {
                Hospital.SelectedRegion = region;
            }

            var viewModels = regionViewModels as IList<RegionViewModel> ?? regionViewModels.ToList();
            if (viewModels.Any() && Hospital.SelectedRegion != null)
            {
                SelectedRegionViewModel = viewModels.FirstOrDefault(x => x.Region.Id == Hospital.SelectedRegion.Id);
            }
            if (cms == null) return;

            CmsCollection = cms ?? new CmsCollectionViewModel();
            CmsCollection.SelectedCMS = CmsCollection.CmsViewModels.FirstOrDefault(x => x.Name == hospital.CmsProviderID);

            ReconcileCategories(ModuleController.Categories);
            RaisePropertyChanged(() => CmsCollection);
        }

        #region Properties

        private CmsCollectionViewModel _cmsCollection;
        public CmsCollectionViewModel CmsCollection
        {
            get { return _cmsCollection; }
            set
            {
                _cmsCollection = value;
                RaisePropertyChanged(() => CmsCollection);
            }
        }

        private RegionViewModel _selectedRegionViewModel;
        public RegionViewModel SelectedRegionViewModel
        {
            get { return _selectedRegionViewModel; }
            set
            {
                if (value == _selectedRegionViewModel) return;
                if (value.Region == null)
                {
                    Debug.WriteLine(string.Format("Hospital id {0} : {1} has no selected region", Hospital.Id, Hospital.Name));
                }
                _selectedRegionViewModel = value;
                RaisePropertyChanged(() => SelectedRegionViewModel);
                using (var session = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>().SessionFactory.OpenSession())
                {
                    Hospital.SelectedRegion = SelectedRegionViewModel.Region;
                    //session.Save(Hospital);
                }
            }
        }

        private Hospital _hospital;
        public Hospital Hospital
        {
            get { return _hospital; }
            set
            {
                _hospital = value;
                RaisePropertyChanged(() => Hospital);
            }
        }

        //private ObservableCollection<CategoryViewModel> _availableCategories;
        //public ObservableCollection<CategoryViewModel> AvailableCategories
        //{
        //    get { return _availableCategories; }
        //    set
        //    {
        //        _availableCategories = value;
        //        RaisePropertyChanged(() => AvailableCategories);
        //    }
        //}

        private ObservableCollection<RegionViewModel> _availableRegions;
        public ObservableCollection<RegionViewModel> AvailableRegions
        {
            get { return _availableRegions; }
            set
            {
                _availableRegions = value;
                RaisePropertyChanged(() => AvailableRegions);
            }
        }

        private ObservableCollection<string> _availableProviders;
        public ObservableCollection<string> AvailableProviders
        {
            get { return _availableProviders; }
            set
            {
                _availableProviders = value;
                RaisePropertyChanged(() => AvailableProviders);
            }
        }

        private ObservableCollection<string> _availableStates;
        public ObservableCollection<string> AvailableStates
        {
            get { return _availableStates; }
            set
            {
                _availableStates = value;
                RaisePropertyChanged(() => AvailableStates);
            }
        }

        private ObservableCollection<string> _availableCounties;
        public ObservableCollection<string> AvailableCounties
        {
            get { return _availableCounties; }
            set
            {
                _availableCounties = value;
                RaisePropertyChanged(() => AvailableCounties);
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
                EventAggregator.GetEvent<HospitalViewModelSelectedEvent>().Publish(this);
            }
        }

        private string _categories;
        public string Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                RaisePropertyChanged(() => Categories);
            }
        }

        #endregion

    
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            if (HospitalDataService != null && HospitalDataService.ModuleController.CurrentRegions.Any())
                ReconcileRegions(HospitalDataService.ModuleController.CustomRegions);
        }

        private void ReconcileRegions(CustomRegionsViewModel customRegionsViewModel)
        {
            if (Hospital.SelectedRegion != null)
            {

                foreach (var vm in customRegionsViewModel.RegionCollection.Where(vm => Hospital.SelectedRegion.Id == vm.Region.Id))
                {
                    SelectedRegionViewModel = vm;
                }
            }

            RaisePropertyChanged(ExtractPropertyName(() => HospitalDataService.ModuleController.CurrentRegions));
        }

        private void ReconcileCategories(CategoriesViewModel categories)
        {
            Categories = categories != null && categories.CategoryListView.OfType<CategoryViewModel>().Any()
                ? string.Join(", ", Hospital.Categories)
                : string.Empty;
            RaisePropertyChanged(() => Hospital);
        }


        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

    }
}
