using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Services;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Services.Hospitals;
using Monahrq.Infrastructure.Types;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.ViewModels;
using NHibernate;
using NHibernate.Linq;
using PropertyChanged;
using Category = Monahrq.Infrastructure.Domain.Categories.Category;
using Region = Monahrq.Infrastructure.Domain.Regions.Region;

namespace Monahrq.DataSets.HospitalRegionMapping.Hospitals.Details
{
    /// <summary>
    /// The hospitals details view model. This view model is responsibile for all add/edit functionality on a new and/or specified hospital.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.ViewModels.DetailsViewModel{Monahrq.Infrastructure.Entities.Domain.Hospitals.Hospital}" />
    [Export(typeof(DetailsViewModel))]
    [ImplementPropertyChanged]
    public class DetailsViewModel : DetailsViewModel<Hospital> //HospitalRegionMappingViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailsViewModel"/> class.
        /// </summary>
        public DetailsViewModel()
        {
            EditingHospitalIdsList = new List<int>();
        }

        #endregion

        #region Imports
        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public IHospitalRegistryService Service { get; set; }

        /// <summary>
        /// Gets or sets the hospitals.
        /// </summary>
        /// <value>
        /// The hospitals.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public HospitalsViewModel Hospitals { get; set; }

        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public IConfigurationService ConfigService { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the navigate back command.
        /// </summary>
        /// <value>
        /// The navigate back command.
        /// </value>
        public DelegateCommand NavigateBackCommand { get; set; }
        /// <summary>
        /// The cancel command
        /// </summary>
        /// <value>
        /// The cancel command.
        /// </value>
        public DelegateCommand CancelCommand { get; set; }
        /// <summary>
        /// Gets or sets the go to previous hospital command.
        /// </summary>
        /// <value>
        /// The go to previous hospital command.
        /// </value>
        public DelegateCommand GoToPreviousHospitalCommand { get; set; }
        /// <summary>
        /// Gets or sets the go to next hospital command.
        /// </summary>
        /// <value>
        /// The go to next hospital command.
        /// </value>
        public DelegateCommand GoToNextHospitalCommand { get; set; }
        /// <summary>
        /// Gets or sets the save go to next hospital command.
        /// </summary>
        /// <value>
        /// The save go to next hospital command.
        /// </value>
        public DelegateCommand SaveGoToNextHospitalCommand { get; set; }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected override void InitCommands()
        {
            base.InitCommands();
            NavigateBackCommand = new DelegateCommand(ExecuteNavigateBack, CanNavigateBack);
            CancelCommand = new DelegateCommand(OnCancel, () => true);
            GoToPreviousHospitalCommand = new DelegateCommand(GoToPreviousHospital, () => true);
            GoToNextHospitalCommand = new DelegateCommand(GoToNextHospital, () => true);
            SaveGoToNextHospitalCommand = new DelegateCommand(SaveGoToNextHospital, () => true);
        }

        /// <summary>
        /// Executes the navigate back.
        /// </summary>
        private void ExecuteNavigateBack()
        {
            var q = new UriQuery();
            q.Add("TabIndex", "1");
            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MainDataSetView + q, UriKind.Relative));
        }

        /// <summary>
        /// Determines whether this instance [can navigate back].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can navigate back]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanNavigateBack()
        {
            return !IsBusy;
        }

        /// <summary>
        /// Called when [cancel].
        /// </summary>
        public override void OnCancel()
        {
            // TODO: do we need any code here to discard in-progress edits before navigating back (like in Measures tab)?
            Model = null;
            ExecuteNavigateBack();
        }

        /// <summary>
        /// Goes to next hospital.
        /// </summary>
        public void GoToNextHospital()
        {
            using (ApplicationCursor.SetCursor(Cursors.Wait))
            {
                var nextHospitalIx = CurrentHospitalIx + 1;
                if (nextHospitalIx >= EditingHospitalIdsList.Count) return;
                CurrentHospitalId = EditingHospitalIdsList.ElementAt(nextHospitalIx);
                OnHospitalsEditing(EditingHospitalIdsList);
            }
        }

        /// <summary>
        /// Saves the go to next hospital.
        /// </summary>
        public async void SaveGoToNextHospital()
        {
            using (ApplicationCursor.SetCursor(Cursors.Wait))
            {
                HospitalToSave = CurrentHospitalViewModel;
                var result = await OnSave2(false, true);
                if (result)
                {
                    var nextHospitalIx = CurrentHospitalIx + 1;
                    if (nextHospitalIx >= EditingHospitalIdsList.Count) return;
                    CurrentHospitalId = EditingHospitalIdsList.ElementAt(nextHospitalIx);
                    OnHospitalsEditing(EditingHospitalIdsList);
                }
            }
        }
        /// <summary>
        /// Determines whether this instance [can go to next hospital].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can go to next hospital]; otherwise, <c>false</c>.
        /// </returns>
        public bool CanGoToNextHospital()
        {
            return true;
            //if (CurrentHospitalIx + 1 >= EditingHospitalsList.Count) return false;
            //else return true;
        }
        /// <summary>
        /// Goes to previous hospital.
        /// </summary>
        public void GoToPreviousHospital()
        {
            using (ApplicationCursor.SetCursor(Cursors.Wait))
            {
                var prevHospitalIx = CurrentHospitalIx - 1;
                if (prevHospitalIx < 0) return;
                CurrentHospitalId = EditingHospitalIdsList.ElementAt(prevHospitalIx);
                OnHospitalsEditing(EditingHospitalIdsList);
            }
        }
        /// <summary>
        /// Determines whether this instance [can go to previous hospital].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can go to previous hospital]; otherwise, <c>false</c>.
        /// </returns>
        public bool CanGoToPreviousHospital()
        {
            return true;
            //if (CurrentHospitalIx - 1 < 0) return false;
            //else return true;
        }
        #endregion

        #region Properties

        ///// <summary>
        ///// Gets or sets the name of new CMS provider.
        ///// </summary>
        ///// <value>
        ///// The name of new CMS provider.
        ///// </value>
        //public string NameOfNewCmsProvider { get; set; }

        // get a string for the View that says either "Add New" or "Edit..."
        /// <summary>
        /// Gets the view label.
        /// </summary>
        /// <value>
        /// The view label.
        /// </value>
        public string ViewLabel
        {
            get
            {
                return (CurrentHospitalViewModel == null || CurrentHospitalViewModel.Hospital.Id == 0)
                    ? "Add New Hospital"
                    : string.Format("Edit Hospital: {0}", CurrentHospitalViewModel.Hospital.Name);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is creating new hospital.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is creating new hospital; otherwise, <c>false</c>.
        /// </value>
        public bool IsCreatingNewHospital { get; set; }

        /// <summary>
        /// Gets or sets the saved county.
        /// </summary>
        /// <value>
        /// The saved county.
        /// </value>
        public string SavedCounty { get; set; }

        // TODO: apply these on textblocks in the xaml
        /// <summary>
        /// Gets the maximum length of the name.
        /// </summary>
        /// <value>
        /// The maximum length of the name.
        /// </value>
        public int NameMaxLength { get { return 255; } }

        /// <summary>
        /// Gets the maximum length of the CMS provider.
        /// </summary>
        /// <value>
        /// The maximum length of the CMS provider.
        /// </value>
        public int CmsProviderMaxLength { get { return 1000; } }

        /// <summary>
        /// Gets the maximum length of the address.
        /// </summary>
        /// <value>
        /// The maximum length of the address.
        /// </value>
        public int AddressMaxLength { get { return 1000; } }

        /// <summary>
        /// Gets the maximum length of the city.
        /// </summary>
        /// <value>
        /// The maximum length of the city.
        /// </value>
        public int CityMaxLength { get { return 1000; } }

        /// <summary>
        /// Gets the maximum length of the zip.
        /// </summary>
        /// <value>
        /// The maximum length of the zip.
        /// </value>
        public int ZipMaxLength { get { return 25; } }

        /// <summary>
        /// Gets the maximum length of the description.
        /// </summary>
        /// <value>
        /// The maximum length of the description.
        /// </value>
        public int DescriptionMaxLength { get { return 1000; } }

        /// <summary>
        /// Gets the maximum length of the hospital ownership.
        /// </summary>
        /// <value>
        /// The maximum length of the hospital ownership.
        /// </value>
        public int HospitalOwnershipMaxLength { get { return 1000; } }

        /// <summary>
        /// Gets the maximum length of the phone.
        /// </summary>
        /// <value>
        /// The maximum length of the phone.
        /// </value>
        public int PhoneMaxLength { get { return 25; } }


        /// <summary>
        /// Gets or sets the current hospital view model.
        /// </summary>
        /// <value>
        /// The current hospital view model.
        /// </value>
        public HospitalViewModel CurrentHospitalViewModel { get; set; }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public ObservableCollection<Category> Categories { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is busy; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusy { get; set; }

        /// <summary>
        /// Gets or sets the regions.
        /// </summary>
        /// <value>
        /// The regions.
        /// </value>
        public ObservableCollection<Region> Regions { get; set; }

        /// <summary>
        /// Gets or sets the available counties.
        /// </summary>
        /// <value>
        /// The available counties.
        /// </value>
        public ObservableCollection<County> AvailableCounties { get; set; }

        /// <summary>
        /// Gets or sets the available states.
        /// </summary>
        /// <value>
        /// The available states.
        /// </value>
        public ObservableCollection<string> AvailableStates { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the Model entity by id during the onLoad and/or OnRefresh method is called.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public override void LoadModel(object id)
        {
            if (id == null) return; // THROW AN USER READABLE ERROR AND LOG.
            using (var session = DataServiceProvider.SessionFactory.OpenSession())
            {
                //using (var trans = session.BeginTransaction())
                //{
                    ExecLoad(session, id);

                //    trans.Commit();
                //}
            }
        }

        /// <summary>
        /// Executes the load.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="id">The identifier.</param>
        protected override void ExecLoad(ISession session, object id)
        {
            var hospitalId = id != null ? Convert.ToInt32(id) : -1;
            if (hospitalId > 0)
            {
                Model = session.Query<Hospital>().FirstOrDefault(x => x.Id == hospitalId);
            }
            else
            {
                Model = new Hospital();
            }

            Regions = Service.GetRegions(session, ConfigService.HospitalRegion.DefaultStates.OfType<string>().ToArray(),
                                                 ConfigService.HospitalRegion.SelectedRegionType).ToObservableCollection();

            //if (Categories == null) //load only once
            Categories = session.Query<HospitalCategory>().Cast<Category>().ToObservableCollection();

            if (AvailableCounties == null || AvailableCounties.Count == 0)
                AvailableCounties = Service.GetCounties(ConfigService.HospitalRegion.DefaultStates.Cast<string>().ToArray()).ToObservableCollection();
        }

        /// <summary>
        /// Gets or sets the editing hospitals list.
        /// </summary>
        /// <value>
        /// The editing hospitals list.
        /// </value>
        public List<Hospital> EditingHospitalsList { get; set; }

        /// <summary>
        /// Gets or sets the editing hospital ids list.
        /// </summary>
        /// <value>
        /// The editing hospital ids list.
        /// </value>
        public List<int> EditingHospitalIdsList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is editing hospitals list.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is editing hospitals list; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditingHospitalsList { get; set; }

        /// <summary>
        /// Gets or sets the current hospital identifier.
        /// </summary>
        /// <value>
        /// The current hospital identifier.
        /// </value>
        public int CurrentHospitalId { get; set; }

        /// <summary>
        /// Gets or sets the current hospital ix.
        /// </summary>
        /// <value>
        /// The current hospital ix.
        /// </value>
        public int CurrentHospitalIx { get; set; }

        /// <summary>
        /// Called when [hospitals editing].
        /// </summary>
        /// <param name="hospitalIdsList">The hospital ids list.</param>
        private void OnHospitalsEditing(/*List<Hospital> hospitalsList*/ List<int> hospitalIdsList)
        {
            if (!IsEditingHospitalsList) return;

            EditingHospitalIdsList = hospitalIdsList;

            var hospId = EditingHospitalIdsList.FirstOrDefault(hId => hId == CurrentHospitalId);
            CurrentHospitalIx = hospId == null ? 0 : EditingHospitalIdsList.IndexOf(hospId);

            LoadModel(CurrentHospitalId);

            InitializeHospitalModel(ConfigService, CurrentHospitalId);
        }

        /// <summary>
        /// Gets or sets the index of the current edit hospital identifier.
        /// </summary>
        /// <value>
        /// The index of the current edit hospital identifier.
        /// </value>
        public int CurrentEditHospitalIdIndex { get; set; }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            AvailableCounties = null;
            //CurrentHospitalViewModel.AvailableStates = null;
            AvailableStates = null;
            CurrentHospitalViewModel = null;

            using (ApplicationCursor.SetCursor(Cursors.Wait))
            {                
                CurrentHospitalId = (navigationContext.Parameters["HospitalId"] != null && int.Parse(navigationContext.Parameters["HospitalId"]) > 0)
                                            ? int.Parse(navigationContext.Parameters["HospitalId"])
                                            : -1;
                IsEditingHospitalsList = (navigationContext.Parameters["IsHospitalsListEditing"] != null && bool.Parse(navigationContext.Parameters["IsHospitalsListEditing"]));

                var hospIdsList = navigationContext.Parameters["HospitalIds"] != null ? navigationContext.Parameters["HospitalIds"].Split(',').ToList() : new List<string>();

                if (hospIdsList.Any())
                {
                    if (EditingHospitalIdsList == null) EditingHospitalIdsList = new List<int>();
                    EditingHospitalIdsList.Clear();

                    hospIdsList.ForEach(id => EditingHospitalIdsList.Add(int.Parse(id)));

                    CurrentHospitalIx = EditingHospitalIdsList.Contains(CurrentHospitalId) ? EditingHospitalIdsList.FindIndex(id => id == CurrentHospitalId) : 0;
                }

                //if (!IsEditingHospitalsList)
                {
                    IsCreatingNewHospital = CurrentHospitalId == -1;
                    LoadModel(CurrentHospitalId);
                    InitializeHospitalModel(ConfigService, CurrentHospitalId);
                }
            }
        }

        /// <summary>
        /// Initializes the hospital model.
        /// </summary>
        /// <param name="configService">The configuration service.</param>
        /// <param name="hospitalId">The hospital identifier.</param>
        private void InitializeHospitalModel(IConfigurationService configService, int hospitalId)
        {
            if (!Model.CCR.HasValue && !string.IsNullOrEmpty(Model.CmsProviderID))
            {
                Model.CCR = Service.GetLatestCostToChargeRatio(Model.CmsProviderID);
            }

            CurrentHospitalViewModel = CreateHospitalViewModel(Model);
            CurrentHospitalViewModel.RegionViewModels = Regions;

            if (!CurrentHospitalViewModel.Hospital.IsPersisted && hospitalId == -1)
                CurrentHospitalViewModel.Hospital.ForceLocalHospitalIdValidation = true;

            CurrentHospitalViewModel.InEditCreateMode = true;

            //CurrentHospitalViewModel.AvailableStates = configService.HospitalRegion.SelectedStates.ToObservableCollection();
            if (AvailableStates == null || AvailableStates.Count == 0)
                AvailableStates = configService.HospitalRegion.DefaultStates.OfType<string>().ToObservableCollection();

            SavedCounty = CurrentHospitalViewModel.Hospital.County;

            CurrentHospitalViewModel.Hospital.County = SavedCounty;

            if (CurrentHospitalViewModel.Hospital.Categories != null && CurrentHospitalViewModel.Hospital.Categories.Any())
            {
                foreach (var category in CurrentHospitalViewModel.Hospital.Categories)
                {
                    var selectCategory = CurrentHospitalViewModel.Categories
                                                                 .FirstOrDefault(c => c.Text.ToLower() == category.Name.ToLower());

                    if (selectCategory != null)
                    {
                        selectCategory.IsSelected = true;
                    }
                }
            }
            if (AvailableCounties != null && AvailableCounties.Count > 0)
                AvailableCounties = CurrentHospitalViewModel.AvailableCounties;
            //AvailableCounties = HospitalRegistryService.GetCounties(configService.HospitalRegion.SelectedStates.Select(s => s.ToString()).ToArray()).ToObservableCollection();
        }

        /// <summary>
        /// Creates the hospital view model.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <returns></returns>
        private HospitalViewModel CreateHospitalViewModel(Hospital hospital)
        {
            var cmsCollection = Hospitals.CmsCollection.Distinct().Select(cms => new CmsViewModel(Convert.ToInt32(cms.Id), cms.CmsProviderID, cms.HospitalName)).ToObservableCollection();

            return new HospitalViewModel(EventAggregator, hospital, Regions, cmsCollection, AvailableCounties, Categories);
        }

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        /// <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //EventAggregator.GetEvent<HospitalsListEditingEvent>().Unsubscribe(OnHospitalsEditing);
            //CurrentHospitalViewModel.InEditCreateMode = false;
            //CurrentHospitalViewModel.AvailableCounties = null;
            //AvailableCounties = null;
            ////CurrentHospitalViewModel.AvailableStates = null;
            //AvailableStates = null;
            //CurrentHospitalViewModel = null;
        }

        /// <summary>
        /// Validates the hospital.
        /// </summary>
        /// <returns></returns>
        private bool ValidateHospital()
        {
            if (HospitalToSave.NameOfNewCmsProvider != null && HospitalToSave.NameOfNewCmsProvider.Length < 6)
            {
                MessageBox.Show("A valid CMS Provider Name requires a minimum of 6 characters", "Text length restriction", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            if (!string.IsNullOrEmpty(HospitalToSave.Hospital.LocalHospitalId) && HospitalToSave.Hospital.LocalHospitalId.Length > 15)
            {
                MessageBox.Show("A valid Hospital Id requires fewer than 15 characters.", "Text length restriction", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            if (string.IsNullOrEmpty(HospitalToSave.Hospital.Name))
            {
                MessageBox.Show("A valid Name is required.", "Required Field", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (HospitalToSave.Hospital.ForceLocalHospitalIdValidation && !HospitalToSave.Hospital.IsSourcedFromBaseData && string.IsNullOrEmpty(HospitalToSave.Hospital.LocalHospitalId))
            {
                MessageBox.Show("A valid Hospital ID is required.", "Required Field", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (HospitalToSave.Hospital.State == null)
            {
                MessageBox.Show("A valid FIPS State is required.", "Required Field", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (HospitalToSave.Hospital.County == null)
            {
                MessageBox.Show("A valid FIPS County is required.", "Required Field", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (!HospitalToSave.Hospital.Zip.IsValidZip())
            {
                MessageBox.Show("A valid Zip is required.", "Required Field", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            //if (!CurrentHospitalViewModel.Hospital.County.State.Abbreviation.EqualsIgnoreCase(CurrentHospitalViewModel.Hospital.State.Abbreviation))
            //{
            //    MessageBox.Show("Please select a county that is located in the selected state.", "Required Field", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //    return false;
            //}

            if (HospitalToSave.SelectedRegionViewModel != null &&
                HospitalToSave.SelectedRegionViewModel != null &&
                !HospitalToSave.SelectedRegionViewModel.State.EqualsIgnoreCase(HospitalToSave.Hospital.State))
            {
                MessageBox.Show("Please select a region that is in located in the selected state.", "Required Field", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(HospitalToSave.Hospital.Description) && HospitalToSave.Hospital.Description.Length > 1000)
            {
                MessageBox.Show(string.Format("Please enter a description using fewer than {0} characters.", 1000), "Text length restriction", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(HospitalToSave.Hospital.PhoneNumber) && !HospitalToSave.Hospital.PhoneNumber.IsValidPhoneNumber())
            {
                MessageBox.Show("A valid Phone number is required. Please enter a valid numeric 10 digit phone number.", "Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(HospitalToSave.Hospital.FaxNumber) && !HospitalToSave.Hospital.FaxNumber.IsValidFaxNumber())
            {
                MessageBox.Show("A valid Fax number is required. Please enter a valid numeric 10 digit fax number.", "Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }


            return true;
        }

        /// <summary>
        /// Checks the selected region has changed.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <param name="selectedRegion">The selected region.</param>
        /// <returns></returns>
        private static bool CheckSelectedRegionHasChanged(Hospital hospital, Region selectedRegion)
        {
            if (selectedRegion == null) return true;

            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            if (configService.HospitalRegion.SelectedRegionType == typeof(HealthReferralRegion))
                return hospital.HealthReferralRegion == null || hospital.HealthReferralRegion.Id != selectedRegion.Id;

            if (configService.HospitalRegion.SelectedRegionType == typeof(HospitalServiceArea))
                return hospital.HospitalServiceArea == null || hospital.HospitalServiceArea.Id != selectedRegion.Id;

            return hospital.CustomRegion == null || hospital.CustomRegion.Id != selectedRegion.Id;
        }

        /// <summary>
        /// Called when [save].
        /// </summary>
        /// <param name="enableNotificantions">if set to <c>true</c> [enable notificantions].</param>
        public override async void OnSave(bool enableNotificantions = false)
        {
            HospitalToSave = CurrentHospitalViewModel;
            await OnSave2(true, true);
        }

        /// <summary>
        /// Gets or sets the hospital to save.
        /// </summary>
        /// <value>
        /// The hospital to save.
        /// </value>
        private HospitalViewModel HospitalToSave { get; set; }

        /// <summary>
        /// Called when [save2].
        /// </summary>
        /// <param name="navigateback">if set to <c>true</c> [navigateback].</param>
        /// <param name="enableNotificantions">if set to <c>true</c> [enable notificantions].</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Hospital can not be null.</exception>
        public async Task<bool> OnSave2(bool navigateback = false, bool enableNotificantions = false)
        {
            try
            {
                if (HospitalToSave == null)
                {
                    throw new ArgumentNullException(@"Hospital can not be null.");
                }

                if (!ValidateHospital())
                    return false;

                var errorOccurred = false;
                Exception saveException = null;
                var progressService = new ProgressService();

                progressService.SetProgress("Saving Hospital", 0, false, true);

                await Task.Delay(500);

                var operationComplete = await progressService.Execute( () =>
                {
                    var selectedRegionTypeContext = ConfigService.HospitalRegion.SelectedRegionType;

                    Hospital archivedHospital = null;
                    if (HospitalToSave.Hospital != null &&
                        HospitalToSave.Hospital.IsSourcedFromBaseData)
                    {
                        archivedHospital = Service.CreateHospitalArchive(HospitalToSave.Hospital);
                        HospitalToSave.Hospital.Id = 0;
                        HospitalToSave.Hospital.IsSourcedFromBaseData = false;
                    }

                    // Categories
                    HospitalToSave.Hospital.Categories.Clear();

                    var selecterdCategories =
                        HospitalToSave.Categories.Where(c => c.IsSelected)
                            .Select(item => ((HospitalCategory) item.Model))
                            .ToList();
                    foreach (var category in selecterdCategories)
                    {
                        HospitalToSave.Hospital.Categories.Add(category);
                    }

                    // Selected Region
                    var selectedRegion = HospitalToSave.SelectedRegionViewModel;
                    if (CheckSelectedRegionHasChanged(HospitalToSave.Hospital, selectedRegion))
                        // Check to see that the regions are different.
                    {
                        if (selectedRegion != null)
                        {
                            if (selectedRegion is CustomRegion)
                            {
                                HospitalToSave.Hospital.CustomRegion = selectedRegion as CustomRegion;
                            }
                            else if (selectedRegion is HealthReferralRegion)
                            {
                                HospitalToSave.Hospital.HealthReferralRegion = selectedRegion as HealthReferralRegion;
                                //HospitalToSave.Hospital.CustomRegion = null;
                            }
                            else if (selectedRegion is HospitalServiceArea)
                            {
                                HospitalToSave.Hospital.HospitalServiceArea = selectedRegion as HospitalServiceArea;
                                //HospitalToSave.Hospital.CustomRegion = null;
                            }
                        }
                        else
                        {
                            if (selectedRegionTypeContext == typeof (CustomRegion))
                            {
                                HospitalToSave.Hospital.CustomRegion = null;
                            }
                            else if (selectedRegionTypeContext == typeof (HospitalServiceArea))
                            {
                                HospitalToSave.Hospital.HospitalServiceArea = null;
                                //HospitalToSave.Hospital.CustomRegion = null;
                            }
                            else if (selectedRegionTypeContext == typeof (HealthReferralRegion))
                            {
                                HospitalToSave.Hospital.HealthReferralRegion = null;
                                //HospitalToSave.Hospital.CustomRegion = null;
                            }
                        }
                    }

                    // CMS PRovider Ids

                    if (HospitalToSave.NameOfNewCmsProvider != null && !HospitalToSave.Hospital.CmsProviderID.EqualsIgnoreCase(HospitalToSave.NameOfNewCmsProvider))
                    {
                        var existingHospital =
                            Service.Get<Hospital>(
                                h =>
                                    h.CmsProviderID.ToLower() == HospitalToSave.NameOfNewCmsProvider.ToLower() &&
                                    h.State.ToLower() == HospitalToSave.Hospital.State.ToLower() &&
                                    !h.IsArchived && !h.IsDeleted);

                        if (existingHospital != null && existingHospital.IsSourcedFromBaseData)
                        {
                            Hospital archivedHospital2 = Service.CreateHospitalArchive(existingHospital);
                            existingHospital.Id = 0;
                            existingHospital.IsSourcedFromBaseData = false;
                            existingHospital.CmsProviderID = null;
                            Service.Save(existingHospital);
                            archivedHospital2.LinkedHospitalId = existingHospital.Id;
                            Service.Save(archivedHospital2);
                        }
                        else if (existingHospital != null && !existingHospital.IsSourcedFromBaseData)
                        {
                            existingHospital.CmsProviderID = null;
                            Service.Save(existingHospital);
                        }

                        HospitalToSave.Hospital.CmsProviderID = CurrentHospitalViewModel.NameOfNewCmsProvider;
                    }
                    else if (HospitalToSave.NameOfNewCmsProvider == null)
                    {
                        HospitalToSave.Hospital.CmsProviderID = null;
                    }

                    Service.Save(HospitalToSave.Hospital);

                    //HospitalToSave.Hospital = result.Model;
                    if (archivedHospital != null)
                    {
                        archivedHospital.LinkedHospitalId = HospitalToSave.Hospital.Id;
                        Service.Save(archivedHospital);
                    }

                    //Thread.Sleep(20000);

                    //await Service.SaveAsync(HospitalToSave.Hospital, result =>
                    //{
                    //    if (result.Status)
                    //    {
                    //        HospitalToSave.Hospital = result.Model;
                    //        if (archivedHospital != null)
                    //        {
                    //            archivedHospital.LinkedHospitalId = HospitalToSave.Hospital.Id;
                    //            Service.Save(archivedHospital);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (!result.Status && result.Exception != null)
                    //        {
                    //            throw result.Exception;
                    //        }
                    //    }
                    //});
                }, result =>
                {
                    if (result.Status)
                    {
                        errorOccurred = false;
                        saveException = null;

                    }
                    else
                    {
                        if (result.Exception != null)
                        {
                            errorOccurred = true;
                            saveException = result.Exception.GetBaseException();
                        }
                    }
                }, new CancellationToken());

                if (operationComplete)
                {
                    progressService.SetProgress("Completed", 100, true, false);

                    if (errorOccurred && saveException != null)
                    {
                        Logger.Log(saveException.Message, Microsoft.Practices.Prism.Logging.Category.Exception, Priority.High);
                        EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(saveException);
                    }
                    else
                    {
                        if (navigateback)
                        {
                            EventAggregator.GetEvent<GenericNotificationEvent>().Publish(string.Format("{2}Hospital {0} has been {1}", HospitalToSave.Hospital.Name, IsCreatingNewHospital ? "added" : "updated", IsEditingHospitalsList && !navigateback ? "Previous " : string.Empty));

                            CurrentHospitalViewModel.InEditCreateMode = false;
                            ExecuteNavigateBack();
                        }
                        else
                        {
                            EventAggregator.GetEvent<GenericNotificationEvent>().Publish(string.Format("{2}Hospital {0} has been {1}", HospitalToSave.Hospital.Name, IsCreatingNewHospital ? "added" : "updated", IsEditingHospitalsList && !navigateback ? "Previous " : string.Empty));
                        }
                        HospitalToSave = null;
                    }
                }
            }
            catch (Exception ex)
            {
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex.GetBaseException());
                return false;
            }

            return true;
        }

        #endregion

    }
}
