using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Regions;
using Monahrq.DataSets.Services;
using Monahrq.Default.ViewModels;
using System.Collections.Generic;
using Monahrq.Infrastructure.Domain.Categories;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Types;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;

using Monahrq.DataSets.HospitalRegionMapping.Events;
using System;
using Monahrq.Infrastructure.Services;
using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Events;
using PropertyChanged;
using Region = Monahrq.Infrastructure.Domain.Regions.Region;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;

namespace Monahrq.DataSets.HospitalRegionMapping.Hospitals
{
    /// <summary>
    /// The hospitals listing view model.
    /// </summary>
    /// <seealso cref="Monahrq.Default.ViewModels.BaseViewModel" />
    /// <seealso cref="Microsoft.Practices.Prism.Regions.INavigationAware" />
    [Export(typeof(HospitalViewModel))]
    [ImplementPropertyChanged]
    public class HospitalViewModel : BaseViewModel, INavigationAware
    {
        #region Fields and Constants

        private Region _selectedRegionViewModel;
        private readonly bool _initialLoad;
        private bool _isSelected;
        private ObservableCollection<SelectListItem> _categories;
        private CmsViewModel _selectedCmsProviderID;

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the PRISM region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        [Import]
        public IRegionManager RegionManager { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalViewModel"/> class.
        /// </summary>
        /// <param name="events">The events.</param>
        public HospitalViewModel(IEventAggregator events)
            : base(events)
        {
            _initialLoad = true;
            Events.GetEvent<RegionsViewModelSavedEvent>().Subscribe(ReconcileRegions);
            Events.GetEvent<CategoriesViewModelSavedEvent>().Subscribe(cvm => ReconcileCategories(cvm.CollectionItems.OfType<Category>()));

            // Get the list of all state abbreviations from the HospitalsViewModel (so the query only happens once)
            var args = new ExtendedEventArgs<List<string>>();
            Events.GetEvent<StateListRequestEvent>().Publish(args);
            _initialLoad = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalViewModel"/> class.
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="hospital">The hospital.</param>
        /// <param name="regionViewModels">The region view models.</param>
        /// <param name="cms">The CMS.</param>
        /// <param name="counties">The counties.</param>
        /// <param name="categories">The categories.</param>
        public HospitalViewModel(IEventAggregator events, Hospital hospital, IEnumerable<Region> regionViewModels, IEnumerable<CmsViewModel> cms, IEnumerable<County> counties,
           IEnumerable<Category> categories)
            : this(events)
        {
            _initialLoad = true;

            CanEditCmsProvider = !hospital.IsSourcedFromBaseData;

            Hospital = hospital;

            SelectedCategories = new ObservableCollection<SelectListItem>();
            Categories = new ObservableCollection<SelectListItem>(categories.ToList().Select(c => new SelectListItem
                                                                                                      {
                                                                                                          Text = c.Name,
                                                                                                          Value = c.Id,
                                                                                                          Model = c
                                                                                                      }));

            CategoriesForGrid = new ObservableCollection<string>(hospital.Categories.Where(c => c != null).Select(c => c.Name).ToList());

			var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
			var selectedRegionType = configService.HospitalRegion.SelectedRegionType;

            if (hospital.CustomRegion != null)
            {
                SelectedRegion = hospital.CustomRegion;
            }

            if (SelectedRegion == null)
            {
                if (selectedRegionType == typeof(HealthReferralRegion))
                    SelectedRegion = hospital.HealthReferralRegion;
                else if (selectedRegionType == typeof(HospitalServiceArea))
                    SelectedRegion = hospital.HospitalServiceArea;
            }

            List<Region> viewModels;

            if (hospital.IsPersisted && hospital.State != null)
            {
                viewModels = regionViewModels != null
                                            ? regionViewModels.Where(r => r.State.Equals(hospital.State)).ToList()
                                            : new List<Region>();

                viewModels.Insert(0, new Region());
            }
            else
            {
                viewModels = regionViewModels.ToList();
                viewModels.Insert(0, new Region());
            }

            if (viewModels.Any() && SelectedRegion != null)
            {
                SelectedRegionViewModel = viewModels.FirstOrDefault(x => x != null && x.Id == SelectedRegion.Id);
            }

            RegionViewModels = new ObservableCollection<Region>(viewModels);

           
            AvailableCmsProviderIds = cms != null ? cms.RemoveDuplicates().Select(c => c.Name).ToObservableCollection() : new ObservableCollection<string>();
            NameOfNewCmsProvider = hospital.CmsProviderID;

            AvailableCounties = counties.ToObservableCollection();
            
            RaisePropertyChanged(() => AvailableCmsProviderIds);

            Init();

            _initialLoad = false;

        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the selected region.
        /// </summary>
        /// <value>
        /// The selected region.
        /// </value>
        public Region SelectedRegion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [in edit create mode].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in edit create mode]; otherwise, <c>false</c>.
        /// </value>
        public bool InEditCreateMode { get; set; }

        /// <summary>
        /// Gets or sets the hospital.
        /// </summary>
        /// <value>
        /// The hospital.
        /// </value>
        public Hospital Hospital { get; set; }

        /// <summary>
        /// Gets or sets the region view models.
        /// </summary>
        /// <value>
        /// The region view models.
        /// </value>
        public ObservableCollection<Region> RegionViewModels
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the selected region view model.
        /// </summary>
        /// <value>
        /// The selected region view model.
        /// </value>
        public Region SelectedRegionViewModel
        {
            get { return _selectedRegionViewModel; }
            set
            {
                try
                {
                    if (value == _selectedRegionViewModel) return;

                    _selectedRegionViewModel = value;

                    if (_initialLoad) return;
                    RaisePropertyChanged(() => SelectedRegionViewModel);

                    if (_initialLoad || InEditCreateMode) return;

                }
                catch (Exception ex)
                {
                    Events.GetEvent<ErrorNotificationEvent>().Publish(ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the available providers.
        /// </summary>
        /// <value>
        /// The available providers.
        /// </value>
        public ObservableCollection<string> AvailableProviders { get; set; }

        /// <summary>
        /// Gets or sets the available states.
        /// </summary>
        /// <value>
        /// The available states.
        /// </value>
        public ObservableCollection<State> AvailableStates { get; set; }

        /// <summary>
        /// Gets or sets the available counties.
        /// </summary>
        /// <value>
        /// The available counties.
        /// </value>
        public ObservableCollection<County> AvailableCounties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
                Events.GetEvent<HospitalViewModelSelectedEvent>().Publish(this);
            }
        }

        // TODO: where do we plan to use this??? Can it be deleted?
        // a comma-separated list of category names
        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public ObservableCollection<SelectListItem> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value.ToObservableCollection();
                CategoriesForGrid = _categories.Select(c => c.Text).ToObservableCollection();
                //RaisePropertyChanged(() => Categories);
            }
        }

        /// <summary>
        /// Gets the show custom hospital elements.
        /// </summary>
        /// <value>
        /// The show custom hospital elements.
        /// </value>
        public Visibility ShowCustomHospitalElements
        {
            get
            {
                return !Hospital.IsSourcedFromBaseData ? Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// Gets the show base hospital elements.
        /// </summary>
        /// <value>
        /// The show base hospital elements.
        /// </value>
        public Visibility ShowBaseHospitalElements
        {
            get
            {
                return Hospital.IsSourcedFromBaseData ? Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// Gets or sets the categories for grid.
        /// </summary>
        /// <value>
        /// The categories for grid.
        /// </value>
        public ObservableCollection<string> CategoriesForGrid { get; set; }

        /// <summary>
        /// Gets or sets the selected categories.
        /// </summary>
        /// <value>
        /// The selected categories.
        /// </value>
        public ObservableCollection<SelectListItem> SelectedCategories { get; set; }


        /// <summary>
        /// Gets or sets the CMS collection.
        /// </summary>
        /// <value>
        /// The CMS collection.
        /// </value>
        public ObservableCollection<CmsViewModel> CmsCollection { get; set; }

        /// <summary>
        /// Gets or sets the available CMS provider ids.
        /// </summary>
        /// <value>
        /// The available CMS provider ids.
        /// </value>
        public ObservableCollection<string> AvailableCmsProviderIds { get; set; }

        /// <summary>
        /// Gets or sets the name of new CMS provider.
        /// </summary>
        /// <value>
        /// The name of new CMS provider.
        /// </value>
        public string NameOfNewCmsProvider { get; set; }

        /// <summary>
        /// Gets or sets the selected CMS provider identifier.
        /// </summary>
        /// <value>
        /// The selected CMS provider identifier.
        /// </value>
        public CmsViewModel SelectedCmsProviderID
        {
            get { return _selectedCmsProviderID; }
            set
            {
                try
                {
                    if (value == _selectedCmsProviderID) return;

                    _selectedCmsProviderID = value;
                    //RaisePropertyChanged(() => SelectedCmsProviderID);

                    if (_initialLoad || InEditCreateMode) return;
                    //if (_selectedCmsProviderID == null) return;
                }
                catch (Exception ex)
                {
                    Events.GetEvent<ErrorNotificationEvent>().Publish(ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected CMS provider.
        /// </summary>
        /// <value>
        /// The selected CMS provider.
        /// </value>
        public CmsViewModel SelectedCmsProvider
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can edit CMS provider.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can edit CMS provider; otherwise, <c>false</c>.
        /// </value>
        public bool CanEditCmsProvider { get; set; }

        /// <summary>
        /// Gets a value indicating whether [CMS provider is new].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [CMS provider is new]; otherwise, <c>false</c>.
        /// </value>
        public bool CmsProviderIsNew
        {
            get
            {
                if (string.IsNullOrEmpty(NameOfNewCmsProvider)) return false;
                if (Hospital == null) return false;
                //Minimum 6 characters long and not found
                return NameOfNewCmsProvider.Length >= 6 &&
                       AvailableCmsProviderIds != null &&
                       !AvailableCmsProviderIds.Any(cms => cms.EqualsIgnoreCase(NameOfNewCmsProvider));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        void Init()
        {
            RefreshCategories();
        }

        private void ReconcileRegions(Region regionsViewModel)
        {
            //if (/*Hospital.SelectedRegion != null && */ SelectedRegion != null)
            //{
            //    SelectedRegionViewModel = regionsViewModel.SingleOrDefault(vm => SelectedRegion.Id == vm.Region.Id);
            //}

            //RaisePropertyChanged(() => SelectedRegionViewModel);
        }

        /// <summary>
        /// Refreshes the categories. Called from ctor and from Commit.
        /// TODO: do we need this method??? If not, uncomment it and optimize (use in-memory list). This is too slow to call from HospitalViewModel constructor.
        /// </summary>
        public void RefreshCategories()
        {
            //var cats = Controller.Service.GetCategories(this.Hospital);
            ReconcileCategories(Hospital.Categories.ToList());
            CategoriesForGrid = Hospital.Categories.Select(c => c.Name).ToObservableCollection();
        }

        /// <summary>
        /// Reconciles the categories.
        /// ReconcileCategories is called from CategoriesViewModelSavedEvent, 
        /// and OnImportsSatisfied (currently not getting hit!), 
        /// and RefreshCategories (from ctor), 
        /// and HospitalsViewModel.ApplySelectedCategoriesToSelectedHospitals 
        /// </summary>
        /// <param name="categories">The categories.</param>
        public void ReconcileCategories(IEnumerable<Category> categories)
        {
            var hospitalCategories = categories as IList<Category> ?? categories.ToList();
            InitSelectedCategories(hospitalCategories);
        }

        /// <summary>
        /// Initializes the selected categories.
        /// </summary>
        /// <param name="categories">The categories.</param>
        private void InitSelectedCategories(IEnumerable<Category> categories)
        {
            // SelectedCategories is an ObservableCollection of checkboxes for the ItemsControl in DetailsView.xaml
            var categoryList = categories.ToList();
            foreach (var selectedCategory in categoryList.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id,
                    Model = c,
                    IsSelected = categoryList.Any(x => x.Id == c.Id)
                }).ToList())
            {
                // does this hospital currently have this category?
                SelectedCategories.Add(selectedCategory);
            }
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        /// <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        #endregion
    }

    /// <summary>
    /// The Selected category class need for checkboxes in DetailsView.xaml
    /// </summary>
    public class SelectedCategory
    {
        /// <summary>
        /// The hospital category
        /// </summary>
        public Category HospitalCategory;
        /// <summary>
        /// The is selected
        /// </summary>
        public bool IsSelected;
    }
}
