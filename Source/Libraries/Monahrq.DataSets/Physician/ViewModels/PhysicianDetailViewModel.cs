using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Infrastructure.Types;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Types;
using Monahrq.Sdk.ViewModels;
using NHibernate;
using NHibernate.Linq;
using PropertyChanged;
using db = Monahrq.Infrastructure.Domain.Physicians;

namespace Monahrq.DataSets.Physician.ViewModels
{
    [Export]
    [ImplementPropertyChanged]
    public class PhysicianDetailViewModel : DetailsViewModel<db.Physician>
    {
        #region Fields and Constants

        private string _npiStringValue;
        private List<dynamic> _medicalAssignementList;
        private string _searchText;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the index of the selected tab.
        /// </summary>
        /// <value>
        /// The index of the selected tab.
        /// </value>
        public int SelectedTabIndex { get; set; }

        /// <summary>
        /// Gets or sets the view label.
        /// </summary>
        /// <value>
        /// The view label.
        /// </value>
        public string ViewLabel { get; set; }

        /// <summary>
        /// Gets or sets the available states.
        /// </summary>
        /// <value>
        /// The available states.
        /// </value>
        public ObservableCollection<State> AvailableStates { get; set; }

        /// <summary>
        /// Gets or sets the gender list.
        /// </summary>
        /// <value>
        /// The gender list.
        /// </value>
        public List<string> GenderList { get; set; }
        /// <summary>
        /// Gets or sets the specialty list.
        /// </summary>
        /// <value>
        /// The specialty list.
        /// </value>
        public List<string> SpecialtyList { get; set; }

        //[RegularExpression(@"[0-9]*", ErrorMessage = @"NPI must be number.")]
        //[Unique("Physicians", "NPI", "NPI must be unique.")]
        //[Required]
        /// <summary>
        /// Gets or sets the npi string value.
        /// </summary>
        /// <value>
        /// The npi string value.
        /// </value>
        public string NpiStringValue
        {
            get { return _npiStringValue; }
            set
            {
                _npiStringValue = value;
                int val;
                if (int.TryParse(_npiStringValue, out val))
                {
                    Model.Npi = val;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected gender.
        /// </summary>
        /// <value>
        /// The selected gender.
        /// </value>
        public string SelectedGender { get; set; }

        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        /// <value>
        /// The search text.
        /// </value>
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if ((_searchText != null && _searchText == value)) return;
                if (HospitalAffiliationList.Any(x => x.IsSelected)) return;

                _searchText = value;
                FindHospitals(_searchText);
            }
        }

        /// <summary>
        /// Gets or sets the selected hospital.
        /// </summary>
        /// <value>
        /// The selected hospital.
        /// </value>
        public string SelectedHospital { get; set; }

        /// <summary>
        /// Gets or sets the hospital affiliation list.
        /// </summary>
        /// <value>
        /// The hospital affiliation list.
        /// </value>
        public ObservableCollection<SelectListItem> HospitalAffiliationList { get; set; }

        /// <summary>
        /// Gets or sets the search medical practice text.
        /// </summary>
        /// <value>
        /// The search medical practice text.
        /// </value>
        public string SearchMedicalPracticeText { get; set; }

        /// <summary>
        /// Gets or sets the hospitals.
        /// </summary>
        /// <value>
        /// The hospitals.
        /// </value>
        public ObservableCollection<string> Hospitals { get; set; }

        /// <summary>
        /// Gets or sets the show address popup.
        /// </summary>
        /// <value>
        /// The show address popup.
        /// </value>
        public Visibility ShowAddressPopup { get; set; }

        /// <summary>
        /// Gets or sets the selected address.
        /// </summary>
        /// <value>
        /// The selected address.
        /// </value>
        public db.PhysicianAddress SelectedAddress { get; set; }

        /// <summary>
        /// Gets or sets the selected medical assignement.
        /// </summary>
        /// <value>
        /// The selected medical assignement.
        /// </value>
        public dynamic SelectedMedicalAssignement { get; set; }
        //public SelectListItem SelectedMedicalAssignement { get; set; }

        /// <summary>
        /// Gets the medical assignement list.
        /// </summary>
        /// <value>
        /// The medical assignement list.
        /// </value>
        public List<dynamic> MedicalAssignementList
        {
            get
            {
                return (_medicalAssignementList ??
                       (_medicalAssignementList = EnumExtensions.GetEnumDynamicDTOs<db.MedicalAssignmentEnum>()));
                //		EnumExtensions.GetEnumFormattedStrings<db.MedicalAssignmentEnum>("{0} - {1}") ));
                //		EnumExtensions.GetEnumStringValues<db.MedicalAssignmentEnum>()));
            }
        }

        /// <summary>
        /// Gets or sets the possible medical practices.
        /// </summary>
        /// <value>
        /// The possible medical practices.
        /// </value>
        public ICollectionView PossibleMedicalPractices { get; set; }

        /// <summary>
        /// Gets or sets the languages.
        /// </summary>
        /// <value>
        /// The languages.
        /// </value>
        public string Languages { get; set; }

        /// <summary>
        /// Gets or sets the physician address list.
        /// </summary>
        /// <value>
        /// The physician address list.
        /// </value>
        public ObservableCollection<Address> PhysicianAddressList { get; set; }

        /// <summary>
        /// Gets or sets the deleted physician address list.
        /// </summary>
        /// <value>
        /// The deleted physician address list.
        /// </value>
        public ObservableCollection<Address> DeletedPhysicianAddressList { get; set; }

        /// <summary>
        /// Gets or sets the new location options.
        /// </summary>
        /// <value>
        /// The new location options.
        /// </value>
        public List<SelectListItem> NewLocationOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is searching medical practice.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is searching medical practice; otherwise, <c>false</c>.
        /// </value>
        public bool IsSearchingMedicalPractice { get; set; }

        #endregion

        #region Imports

        /// <summary>
        /// Gets the base data service.
        /// </summary>
        /// <value>
        /// The base data service.
        /// </value>
        [Import]
        public IBaseDataService BaseDataService { get; private set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the remove hospital affiliation.
        /// </summary>
        /// <value>
        /// The remove hospital affiliation.
        /// </value>
        public DelegateCommand<string> RemoveHospitalAffiliation { get; set; }

        /// <summary>
        /// Gets or sets the add hospital affiliation.
        /// </summary>
        /// <value>
        /// The add hospital affiliation.
        /// </value>
        public DelegateCommand<string> AddHospitalAffiliation { get; set; }

        /// <summary>
        /// Gets or sets the add new address.
        /// </summary>
        /// <value>
        /// The add new address.
        /// </value>
        public DelegateCommand AddNewAdress { get; set; }

        /// <summary>
        /// Gets or sets the edit address.
        /// </summary>
        /// <value>
        /// The edit address.
        /// </value>
        public DelegateCommand<db.PhysicianAddress> EditAddress { get; set; }

        /// <summary>
        /// Gets or sets the remove address.
        /// </summary>
        /// <value>
        /// The remove address.
        /// </value>
        public DelegateCommand<Address> RemoveAddress { get; set; }

        /// <summary>
        /// Gets or sets the save address.
        /// </summary>
        /// <value>
        /// The save address.
        /// </value>
        public DelegateCommand SaveAddress { get; set; }

        /// <summary>
        /// Gets or sets the cancel address.
        /// </summary>
        /// <value>
        /// The cancel address.
        /// </value>
        public DelegateCommand CancelAddress { get; set; }

        /// <summary>
        /// Gets or sets the lookup medical practice.
        /// </summary>
        /// <value>
        /// The lookup medical practice.
        /// </value>
        public DelegateCommand LookupMedicalPractice { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Called when [load].
        /// </summary>
        /// <param name="id">The identifier.</param>
        public override void LoadModel(object id)
        {
            if (id == null) return; // THROW AN USER READABLE ERROR AND LOG.
            using (var session = DataServiceProvider.SessionFactory.OpenSession())
            {
                ExecLoad(session, id);
            }
        }

        /// <summary>
        /// Executes the load of the physician entity and all associated items as well.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="id">The identifier.</param>
        protected override void ExecLoad(ISession session, object id)
        {
            base.ExecLoad(session, id);

            //Model.PhysicianMedicalPractices = Model.PhysicianMedicalPractices.RemoveDuplicates().ToObservableList();
            //ListExtensions.ForEach(Model.Practices, mp => mp.Addresses = mp.Addresses.ToObservableList());
            Model.PhysicianMedicalPractices = Model.PhysicianMedicalPractices.ToObservableCollection();
            Model.AffiliatedHospitals = Model.AffiliatedHospitals.ToObservableCollection();
            Model.Addresses = Model.Addresses.RemoveNullValues().ToObservableCollection();
            Model.ForeignLanguages = Model.ForeignLanguages.ToObservableCollection();

            foreach (var pmp in Model.PhysicianMedicalPractices)
            {
                if (!pmp.MedicalPractice.HasAddresses()) continue;

                pmp.MedicalPractice.Addresses = pmp.MedicalPractice.Addresses.ToObservableCollection();
            }

            foreach (var aff in Model.AffiliatedHospitals)
            {
                if (aff == null) continue;

                var hospital = session.Query<Hospital>()
                                      .Where(x => x.CmsProviderID == aff.HospitalCmsProviderId && !x.IsArchived && !x.IsDeleted)
                                      .Select(x => new Tuple<string, string, string>(x.Name, x.CmsProviderID, x.State))
                                      .FirstOrDefault();

                var displayName = hospital == null ? string.Format("Unknown Hospital - {0} - Unknown State", aff.HospitalCmsProviderId)
                                                   : string.Format("{0} - {1} - {2}", hospital.Item1, hospital.Item2, hospital.Item3);
                Hospitals.Add(displayName);
            }
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected override void InitCommands()
        {
            base.InitCommands();

            RemoveHospitalAffiliation = new DelegateCommand<string>(OnRemoveHospitalAffiliation);
            AddHospitalAffiliation = new DelegateCommand<string>(OnAddHospitalAffiliation);
            AddNewAdress = new DelegateCommand(OnNewAddAdress);
            EditAddress = new DelegateCommand<db.PhysicianAddress>(OnEditAddress);
            CancelAddress = new DelegateCommand(OnCancelAddress);
            SaveAddress = new DelegateCommand(OnSaveNewAddress);
            RemoveAddress = new DelegateCommand<Address>(OnRemoveAddress);
            LookupMedicalPractice = new DelegateCommand(OnMedicalPracticeLookup);
        }

        /// <summary>
        /// Initializes the properties.
        /// </summary>
        protected override void InitProperties()
        {
            base.InitProperties();

            PhysicianAddressList = new ObservableCollection<Address>();
            DeletedPhysicianAddressList = new ObservableCollection<Address>();
        }

        /// <summary>
        /// Called when [medical practice lookup].
        /// </summary>
        private void OnMedicalPracticeLookup()
        {
            if (String.IsNullOrEmpty(SearchMedicalPracticeText)) return;

            using (var session = DataServiceProvider.SessionFactory.OpenStatelessSession())
            {

                var possibleAddresses = new List<Address>();
                session.Query<db.MedicalPractice>()
                    .Where(x => x.Name != null && x.Name.ToUpper().StartsWith(SearchMedicalPracticeText.ToUpper()))
                    .Take(10).ToList().ForEach(mp =>
                    {
                        // Get associated addresses for MedicalPractice.
                        var pmp = Model.PhysicianMedicalPractices.FirstOrDefault(pmpx => pmpx.MedicalPractice.Id == mp.Id);
                        var associatedPmpAddresses = (pmp == null) ? new List<int>() : pmp.AssociatedPMPAddresses;

                        var addresses = mp.Addresses.ToList();
                        addresses.ForEach(add =>
                        {
                            if (add == null) return;
                            add.MedicalPractice = mp;
                            add.MedicalPracticeName = mp.Name;
                            add.IsSelected = associatedPmpAddresses.Contains(add.Id);	// PhysicianAddressList.Any(pa => pa.Id == add.Id);
                            possibleAddresses.Add(add);
                        });
                    });

                PossibleMedicalPractices = CollectionViewSource.GetDefaultView(new ListCollectionView(possibleAddresses));
                PossibleMedicalPractices.GroupDescriptions.Add(new PropertyGroupDescription("MedicalPracticeName"));
            }
        }

        /// <summary>
        /// Called when [remove address].
        /// </summary>
        /// <param name="obj">The object.</param>
        private void OnRemoveAddress(Address obj)
        {
            if (obj == null) return;

            var result = MessageBox.Show(@"Are you sure you want to delete this address?", @"Delete Confirmation", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.Cancel) return;

            PhysicianAddressList.Remove(obj);
            DeletedPhysicianAddressList.Add(obj);
        }

        /// <summary>
        /// Called when [save new address].
        /// </summary>
        private void OnSaveNewAddress()
        {
            if (IsSearchingMedicalPractice)
            {
                PossibleMedicalPractices.OfType<db.MedicalPracticeAddress>().Where(x => x.IsSelected).ToList().ForEach(add =>
                {
                    // Get associated addresses for MedicalPractice.
                    var pmp = Model.PhysicianMedicalPractices.Where(pmpx => pmpx.MedicalPractice.Id == add.MedicalPractice.Id).FirstOrDefault();
                    if (pmp == null)
                    {
                        pmp = new db.PhysicianMedicalPractice()
                        {
                            Physician = Model,
                            MedicalPractice = add.MedicalPractice,
                            AssociatedPMPAddresses = new List<int>()
                        };
                        Model.PhysicianMedicalPractices.Add(pmp);
                    }

                    // Add the associated address if needed.
                    if (!pmp.AssociatedPMPAddresses.Contains(add.Id))
                        pmp.AssociatedPMPAddresses.Add(add.Id);
                });
            }
            else
            {
                if (SelectedAddress == null) return;

                SelectedAddress.Validate();
                if (SelectedAddress.HasErrors) return;

                Model.Addresses.Add(SelectedAddress);
            }
            ShowAddressPopup = Visibility.Collapsed;
            ProcessAddressForDisplay();
        }

        /// <summary>
        /// Called when [cancel address].
        /// </summary>
        private void OnCancelAddress()
        {
            ShowAddressPopup = Visibility.Collapsed;
        }

        /// <summary>
        /// Called when [edit address].
        /// </summary>
        /// <param name="obj">The object.</param>
        private void OnEditAddress(db.PhysicianAddress obj)
        {
            SelectedAddress = obj;
            ShowAddressPopup = Visibility.Visible;
        }

        /// <summary>
        /// Called when [new add adress].
        /// </summary>
        private void OnNewAddAdress()
        {
            SelectedAddress = null;
            SelectedAddress = new db.PhysicianAddress();
            if (ShowAddressPopup != Visibility.Visible)
            {
                SearchMedicalPracticeText = null;
                PossibleMedicalPractices = new CollectionView(new List<Address>());
            }
            ShowAddressPopup = Visibility.Visible;
        }

        /// <summary>
        /// Called when [add hospital affiliation].
        /// </summary>
        /// <param name="obj">The object.</param>
        private void OnAddHospitalAffiliation(string obj)
        {
            Hospitals.Add(HospitalAffiliationList.Any(x => obj != null && x.Value.ToString().ToLower().Contains(obj.ToLower())) ?
               HospitalAffiliationList.First(x => obj != null && x.Value.ToString().ToLower().Contains(obj.ToLower())).Value.ToString()
               : string.Format("Unknown Hospital - {0} - Unknown State", SearchText));
        }

        /// <summary>
        /// Called when [remove hospital affiliation].
        /// </summary>
        /// <param name="obj">The object.</param>
        private void OnRemoveHospitalAffiliation(string obj)
        {
            if (Hospitals.Contains(obj)) Hospitals.Remove(obj);
        }

        /// <summary>
        /// Called when [save].
        /// </summary>
        /// <param name="enableNotificantions">if set to <c>true</c> [enable notificantions].</param>
        public override async void OnSave(bool enableNotificantions = true)
        {
            Validate();
            Model.Validate();

            if (HasErrors || Model.HasErrors) return;

            //TODO: uncomment this code for npi uniqueness validation
            //if (!IsNpiUnique())
            //{
            //    var result = MessageBox.Show("There is already a physician with the same NPI. NPI must be unique.", "Notification");
            //    return;
            //}

            if (!ValidateAddresses())
            {
                if (MessageBox.Show(string.Format("A location/address nor a medical practice with a defined locations/addresses has not been associated with physician \"{1}\".{0}Please press Cancel and go to the \"Hospital Affliation and Locations\" tab{0}to associate a location/address or medical practice with this physician \"{1}\".{0}If proceeding to save the physician \"{1}\" with out associating a location or a medical practice,{0}this physician may not appear in your published website.", Environment.NewLine, Model.Name), "Physical Details Validation", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    return;
            }

            try
            {
                var errorOccurred = false;
                Exception errorException = null;
                var progressService = new ProgressService();

                progressService.SetProgress("Saving physician", 0, false, true);

                //await Task.Delay(500);

                GenderEnum val;
                if (Enum.TryParse(SelectedGender, true, out val)) Model.Gender = val;
                if (SelectedMedicalAssignement != null && !string.IsNullOrEmpty(SelectedMedicalAssignement.EnumName))
                    Model.AcceptsMedicareAssignment =
                        EnumExtensions.GetEnumValueFromString<db.MedicalAssignmentEnum>(
                            SelectedMedicalAssignement.EnumName);

                Model.ForeignLanguages = new List<string>();
                if (!string.IsNullOrEmpty(Languages))
                    Languages.Split(',').ToList().ForEach(x => Model.ForeignLanguages.Add(x));

                Model.AffiliatedHospitals.Clear();
                Hospitals.ToList().ForEach(x => Model.AffiliatedHospitals.Add(new db.PhysicianAffiliatedHospital
                {
                    HospitalCmsProviderId = GetCmsProviderId(x)
                }));

                //	DeletedPhysicianAddressList was acting as an omni buffer for both Physician and MedicalPractice addresses.
                //	This is going to change for a buffer for both Physician and 'Associated' MedicalPractice addresses.
                //	Scrap This:  If the address.MedicalPractice name is null, delete from Physician Addresses; else delete from MPAssociated Addresses
                //	Scrap This:  Current logic is fine.  It deletes from both list in either case.  Since a single Address(Id) object cannot be both
                //				 Physician and MedicalPractice, there is no issue.
                foreach (var address in DeletedPhysicianAddressList)
                {
                    var physicianaAd = Model.Addresses != null
                        ? Model.Addresses.FirstOrDefault(x => x.Id == address.Id)
                        : null;
                    if (physicianaAd != null) Model.Addresses.Remove(physicianaAd);

                    Model.PhysicianMedicalPractices.ToList().ForEach(pmp =>
                    {
                        if (pmp.AssociatedPMPAddresses.Contains(address.Id))
                            pmp.AssociatedPMPAddresses.Remove(address.Id);
                    });
                    Model.PhysicianMedicalPractices.ToList().RemoveAll(pmp => pmp.AssociatedPMPAddresses.Count == 0);

                    //	var medAddress = address as Monahrq.Infrastructure.Domain.Physicians.MedicalPracticeAddress;
                    //	if (medAddress != null)
                    //	{
                    //		var currentPMP = Model.PhysicianMedicalPractices.Where(pmp => pmp.MedicalPractice.Id == medAddress.MedicalPractice.Id).Single();
                    //		currentPMP.AssociatedPMPAddresses.Remove(address.Id);
                    //		if (currentPMP.AssociatedPMPAddresses.Count == 0)
                    //			Model.PhysicianMedicalPractices.Remove(currentPMP);
                    //	}
                }


                if (Model.States == null) Model.States = new List<string>();

                if (Model.Addresses == null) Model.Addresses = new List<db.PhysicianAddress>();

                if (Model.Addresses.Any() ||
                    Model.PhysicianMedicalPractices.Any(pmp => pmp.MedicalPractice.Addresses.Any()))
                    Model.States.Clear();

                if (Model.Addresses != null && Model.Addresses.Any())
                    Model.Addresses.DistinctBy(x => x.State).Select(x => x.State).ToList().ForEach(add =>
                    {
                        if (!Model.States.Contains(add))
                            Model.States.Add(add);
                    });
                if (Model.PhysicianMedicalPractices.Any(pmp => pmp.MedicalPractice.Addresses.Any()))
                    Model.PhysicianMedicalPractices.Select(pmp => pmp.MedicalPractice).ToList()
                        .ForEach(mp => mp.Addresses.DistinctBy(x => x.State)
                            .Select(x => x.State).ToList().ForEach(add =>
                            {
                                if (!Model.States.Contains(add))
                                    Model.States.Add(add);
                            }));

                if (!Model.IsPersisted && !Model.States.Any())
                    Model.States.Add(ConfigurationService.HospitalRegion.DefaultStates.OfType<string>().ToList().First());

                if (Model.States != null && Model.States.Any())
                {
                    Model.States = Model.States.OrderBy(state => state).ToList();
                }

                DeletedPhysicianAddressList = new ObservableCollection<Address>();

                var operationComplete = await progressService.Execute(() =>
                {
                    //var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();

                    Model.IsEdited = true;

                    using (var session = DataServiceProvider.SessionFactory.OpenSession())
                    {
                        //using (var trans = session.BeginTransaction())
                        {
                            try
                            {
                                OnSaveInternal(session, Model);

                                //trans.Commit();
                                session.Flush();
                                //
                            }
                            catch (Exception exc)
                            {
                                Logger.Log((exc.InnerException ?? exc).Message, Category.Exception, Priority.High);
                                throw;
                            }
                        }
                    }

                    //base.OnSave(false);
                },
                    opResult =>
                    {
                        if (!opResult.Status && opResult.Exception != null)
                        {
                            errorOccurred = true;
                            errorException = opResult.Exception.GetBaseException();
                        }
                        else
                        {
                            errorOccurred = false;
                            errorException = null;
                        }

                    }, new CancellationToken());

                if (operationComplete)
                {
                    progressService.SetProgress("Completed", 100, true, false);

                    if (!errorOccurred && errorException == null)
                    {
                        Notify(string.Format("{1} \"{0}\" has been successfully saved.", Model.Name, typeof(db.Physician).Name));
                        NavigateBack();
                    }
                    else
                    {
                        var exc = errorException.GetBaseException();
                        Logger.Log(exc.Message, Category.Exception, Priority.High);
                        NotifyError(exc, Model.GetType(), Model.Name);
                    }
                }

            }
            catch (Exception exc)
            {
                Logger.Log(exc.GetBaseException().Message, Category.Exception, Priority.High);
                NotifyError(exc.GetBaseException(), Model.GetType(), Model.Name);
                //Notify(String.Format("{0} has not been saved", Model.Name));
            }
        }

        /// <summary>
        /// Validates the addresses.
        /// </summary>
        /// <returns></returns>
        private bool ValidateAddresses()
        {
            if (!Model.HasAddresses() && !Model.HasMedicalPractices())
                return false;

            var mpAddressTest = true;
            if (Model.HasMedicalPractices())
            {
                foreach (var pmp in Model.PhysicianMedicalPractices)
                {
                    if (pmp.MedicalPractice.HasAddresses())
                        break;

                    mpAddressTest = false;
                }
            }

            return mpAddressTest;
        }

        /// <summary>
        /// Called when [save internal].
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="model">The model.</param>
        protected override void OnSaveInternal(ISession session, db.Physician model)
        {
            foreach (var pmp in model.PhysicianMedicalPractices)
            {
                pmp.CleanBeforeSave();

                //var pmpx = pmp;

                //if (!pmp.IsPersisted)
                //    session.SaveOrUpdate(pmpx);
                //else
                //    pmpx = session.Merge(pmpx);
            }

            model.IsEdited = true;

            base.OnSaveInternal(session, model);
        }

        /// <summary>
        /// Called when [cancel].
        /// </summary>
        public override void OnCancel()
        {
            if (IsDirty(Model))
            {
                if (MessageBox.Show("Your changes will be lost if you cancel now. Do you want to Continue?", "Modification Verification", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }
            NavigateBack();
        }

        /// <summary>
        /// Navigates the back.
        /// </summary>
        private void NavigateBack()
        {
            var q = new UriQuery { { "TabIndex", "3" } };
            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MainDataSetView + q, UriKind.Relative));
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            ShowAddressPopup = Visibility.Collapsed;
            SearchText = null;

            var physicianId = (navigationContext.Parameters["PhysicianId"] != null)
                                 ? int.Parse(navigationContext.Parameters["PhysicianId"])
                                 : (int?)null;

            try
            {
                Hospitals = new ObservableCollection<string>();
                DeletedPhysicianAddressList = new ObservableCollection<Address>();
                SelectedTabIndex = 0;
                if (physicianId.HasValue && physicianId.Value > 0)
                {
                    LoadModel(physicianId);
                    ViewLabel = string.Format("Edit Physician: {0}, {1}", Model.LastName, Model.FirstName);
                }
                else
                {
                    Model = new db.Physician();
                    ViewLabel = "Add New Physician";
                }

                SelectedMedicalAssignement = MedicalAssignementList.Find(ma => ma.EnumName.Equals(Model.AcceptsMedicareAssignment.ToString()));
                AvailableStates = BaseDataService.States(state => state.Name != null)
                                                 .Where(x => x.Data != null).Select(x => x.Data).ToObservableCollection();
                GenderList = EnumExtensions.GetEnumStringValues<GenderEnum>();
                SpecialtyList = GetProviderSpecialties();
                SelectedGender = Model.Gender.ToString();
                NpiStringValue = Model.Npi != 0 ? Model.Npi.ToString() : string.Empty;
                HospitalAffiliationList = new ObservableCollection<SelectListItem>();
                if (Model.ForeignLanguages != null) Languages = string.Join(",", Model.ForeignLanguages);
                PropertyChanged += (o, e) => Validate();
                Model.PropertyChanged += (o, e) => Model.Validate();

                // Correlate Model.*Specialties casing with specialties in DB base data.
                Model.PrimarySpecialty = SpecialtyList.Find(x => x.EqualsIgnoreCase(Model.PrimarySpecialty));
                Model.SecondarySpecialty1 = SpecialtyList.Find(x => x.EqualsIgnoreCase(Model.SecondarySpecialty1));
                Model.SecondarySpecialty2 = SpecialtyList.Find(x => x.EqualsIgnoreCase(Model.SecondarySpecialty2));
                Model.SecondarySpecialty3 = SpecialtyList.Find(x => x.EqualsIgnoreCase(Model.SecondarySpecialty3));
                Model.SecondarySpecialty4 = SpecialtyList.Find(x => x.EqualsIgnoreCase(Model.SecondarySpecialty4));
                foreach (var pmp in Model.PhysicianMedicalPractices)
                {
                    pmp.MedicalPractice.Addresses.ToList().ForEach(x => x.PropertyChanged += (o, e) => x.Validate());
                }

                var opt1 = new SelectListItem { Text = "Search Medical Practice", Value = "Search Medical Practice" };
                var opt2 = new SelectListItem { Text = "Add New Address", Value = "Add New Address" };
                opt1.ValueChanged += (sender, args) => { IsSearchingMedicalPractice = opt1.IsSelected; };
                opt2.ValueChanged += (sender, args) => { IsSearchingMedicalPractice = opt1.IsSelected; };
                opt1.IsSelected = true;
                NewLocationOptions = new List<SelectListItem> { opt1, opt2 };
                ProcessAddressForDisplay();

                OriginalModelHashCode = GetModelHashCode(Model);
            }
            catch (Exception exec)
            {
                Logger.Log((exec.InnerException ?? exec).Message, Category.Exception, Priority.High);
                Notify("An error occurred while loading the physician, please restart the app.");
            }
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
            //Clean up added medical practices or addresses that are not saved to the db
            Model.PhysicianMedicalPractices = Model.PhysicianMedicalPractices.ToList();
            var physicicanMedicalPracticesToRemove = Model.PhysicianMedicalPractices.Where(x => !x.IsPersisted).ToList();
            var medicalPracticesToRemove = Model.PhysicianMedicalPractices.Select(x => x.MedicalPractice).Where(x => !x.IsPersisted).ToList();
            var addressToRemove = Model.PhysicianMedicalPractices.Where(x => x.IsPersisted).Select(x => x.MedicalPractice).Where(mp => mp.Addresses.Any(ad => !ad.IsPersisted)).ToList();
            medicalPracticesToRemove.ForEach(mp => Model.PhysicianMedicalPractices.Select(x => x.MedicalPractice).ToList().Remove(mp));
            physicicanMedicalPracticesToRemove.ForEach(pmp => Model.PhysicianMedicalPractices.Remove(pmp));

            addressToRemove.ForEach(mp =>
            {
                var addresses = mp.Addresses.Where(a => !a.IsPersisted).ToList();
                addresses.ForEach(a => mp.Addresses.Remove(a));
            });
            ClearErrors(() => NpiStringValue);

            //Nullify the model so that we force a load next time we come back to this page.  
            //Otherwise the Cancel Button leaves the edited data  fields in place. 
            Model = null;
        }

        /// <summary>
        /// Finds the hospitals.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        private void FindHospitals(string searchText)
        {
            if (string.IsNullOrEmpty(searchText) || searchText.Length <= 3 || HospitalAffiliationList.Any(x => x.Value.ToString().Contains(searchText))) return;

            using (var session = DataServiceProvider.SessionFactory.OpenStatelessSession())
            {
                HospitalAffiliationList.Clear();
                //var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
                var hospitals = new List<Tuple<string, string, string>>();
                foreach (var selectedState in ConfigurationService.HospitalRegion.DefaultStates.OfType<string>())
                {
                    var hosps = session.Query<Hospital>()
                        .Where(item => !item.IsArchived && !item.IsDeleted && item.State == selectedState
                                       &&
                                       ((item.CmsProviderID != null &&
                                         item.CmsProviderID.ToLower().StartsWith(searchText.ToLower()))
                                        ||
                                        (item.Name != null && item.Name.ToLower().StartsWith(searchText.ToLower()))))
                        .Select(h => Tuple.Create(h.CmsProviderID, h.Name, h.State))
                        .ToList();

                    if (hosps.Any())
                        hospitals.AddRange(hosps);
                }
                var selections = hospitals.DistinctBy(h => h.Item1)
                      .Select(h => new SelectListItem
                      {
                          Value = string.Format("{0} - {1} - {2}", h.Item2 ?? string.Empty, h.Item1, h.Item3),
                          Text = h.Item1 ?? "N/A"
                      });

                HospitalAffiliationList.AddRange(selections);
            }
        }

        /// <summary>
        /// Gets the provider specialties.
        /// </summary>
        /// <returns></returns>
        private List<String> GetProviderSpecialties()
        {
            using (var session = DataServiceProvider.SessionFactory.OpenStatelessSession())
            {
                return
                    session.Query<ProviderSpeciality>()
                        .Select(x => x.Name).ToList();
            }
        }

        /// <summary>
        /// Gets the CMS provider identifier.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        private static string GetCmsProviderId(string val)
        {
            string[] displayStrings = val.Split('-');
            if (displayStrings.Length > 1) return displayStrings[1].Replace(" ", "").Trim();
            //int limit = 7;

            return val.Substring(0, val.Length > 7 ? 7 : val.Length).Replace(" ", "").Trim();
        }

        /// <summary>
        /// Processes the address for display.
        /// </summary>
        private void ProcessAddressForDisplay()
        {
            if (Model == null) return;

            PhysicianAddressList.Clear();
            Model.PhysicianMedicalPractices.ToList().ForEach(pmp =>
            {
                var mpAddresses = pmp.MedicalPractice.Addresses.ToList();
                mpAddresses.ForEach(a =>
                {
                    a.MedicalPracticeName = pmp.MedicalPractice.Name;
                    a.IsSelected = pmp.AssociatedPMPAddresses.Contains(a.Id);
                    if (a.IsSelected) PhysicianAddressList.Add(a);
                });
            });


            Model.Addresses.ToList().ForEach(add =>
            {
                add.IsSelected = true;
                if (PhysicianAddressList.Contains(add)) return;
                PhysicianAddressList.Add(add);

            });
        }

        /// <summary>
        /// Determines whether [is npi unique].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is npi unique]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsNpiUnique()
        {
            using (var session = DataServiceProvider.SessionFactory.OpenStatelessSession())
            {
                return !session.Query<db.Physician>().Any(x => x.Npi == Model.Npi && x.Id != Model.Id);
            }
        }

        #endregion

    }
}
