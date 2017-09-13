using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Domain.Websites.Maps;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Services.Hospitals;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Utilities;
using Monahrq.Sdk.ViewModels;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Domain.BaseData;

using NHibernate;
using NHibernate.Linq;
using Monahrq.Infrastructure.Configuration;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Types;
using PropertyChanged;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Monahrq.DataSets.NHC.ViewModels
{
    /// <summary>
    /// The nursing home details view model. Handles the add/edit crud opterations on nursing homes.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.ViewModels.DetailsViewModel{Monahrq.Infrastructure.Domain.NursingHomes.NursingHome}" />
    [Export]
    [ImplementPropertyChanged]
    public class NursingHomeDetailViewModel : DetailsViewModel<NursingHome>
    {
        #region Fields and Constants

        private const string DateStringPattern = @"^([0]?[1-9]|[1][0-2])[/]([0]?[1-9]|[1|2][0-9]|[3][0|1])[/]([0-9]{4}|[0-9]{2})$";
        private string _dateApproved;
        private string _previousProviderId;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the yes no drop down options.
        /// </summary>
        /// <value>
        /// The yes no drop down options.
        /// </value>
        public List<String> YesNoDropDownOptions
        {
            get
            {
                var list = EnumExtensions.GetEnumStringValues<SprinklerStatusEnum>();
                list.Remove(SprinklerStatusEnum.Partial.ToString());
                list.Remove(SprinklerStatusEnum.DataNotAvailable.ToString());
                list.Add(SprinklerStatusEnum.DataNotAvailable.GetDescription());
                return list;
            }
        }

        /// <summary>
        /// Gets the accreditation options.
        /// </summary>
        /// <value>
        /// The accreditation options.
        /// </value>
        public List<string> AccreditationOptions
        {
            get
            {
                return new List<string> { "Not Available", "CMS recognized state survey agencies", "Joint Commission", "Commision on Accreditation of Rehabilitation Facilities" };
            }
        }

        /// <summary>
        /// Gets the resource fam council options.
        /// </summary>
        /// <value>
        /// The resource fam council options.
        /// </value>
        public List<string> ResFamCouncilOptions { get { return EnumExtensions.GetEnumStringValues<ResFamCouncilEnum>(); } }

        /// <summary>
        /// Gets the type of the nursing home.
        /// </summary>
        /// <value>
        /// The type of the nursing home.
        /// </value>
        public List<string> NursingHomeType { get { return new List<string> { "Data Not Available", "Medicare", "Medicaid", "Medicare and Medicaid" }; } }

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
        public ObservableCollection<State> AvailableStates { get; set; }

        /// <summary>
        /// Gets or sets the CMS collection.
        /// </summary>
        /// <value>
        /// The CMS collection.
        /// </value>
        public ObservableCollection<string> CmsCollection { get; set; }

        /// <summary>
        /// Gets or sets the view label.
        /// </summary>
        /// <value>
        /// The view label.
        /// </value>
        public string ViewLabel { get; set; }

        /// <summary>
        /// Gets or sets the date approved.
        /// </summary>
        /// <value>
        /// The date approved.
        /// </value>
        [RegularExpression(DateStringPattern, ErrorMessage = @"Please enter dates in the correct format (mm/dd/yyyy).”")]
        public string DateApproved
        {
            get
            {
                return _dateApproved;
            }
            set
            {
                _dateApproved = value;
                DateTime futureDate;
                if (!DateTime.TryParse(_dateApproved, out futureDate)) return;
                if (futureDate > DateTime.Now)
                    MessageBox.Show("The date cannot be a future date.", "Invalid Date");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has provider identifier changed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has provider identifier changed; otherwise, <c>false</c>.
        /// </value>
        public bool HasProviderIdChanged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is provider identifier new.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is provider identifier new; otherwise, <c>false</c>.
        /// </value>
        public bool IsProviderIdNew { get; set; }

        /// <summary>
        /// Gets or sets the selected provider changed ownership.
        /// </summary>
        /// <value>
        /// The selected provider changed ownership.
        /// </value>
        public string SelectedProviderChangedOwnership { get; set; }

        /// <summary>
        /// Gets or sets the selected council.
        /// </summary>
        /// <value>
        /// The selected council.
        /// </value>
        public string SelectedCouncil { get; set; }

        /// <summary>
        /// Gets or sets the selected sprinkler system.
        /// </summary>
        /// <value>
        /// The selected sprinkler system.
        /// </value>
        public string SelectedSprinklerSystem { get; set; }

        /// <summary>
        /// Gets or sets the selected in hospital.
        /// </summary>
        /// <value>
        /// The selected in hospital.
        /// </value>
        public string SelectedInHospital { get; set; }

        /// <summary>
        /// Gets or sets the selected in retirement community.
        /// </summary>
        /// <value>
        /// The selected in retirement community.
        /// </value>
        public string SelectedInRetirementCommunity { get; set; }

        /// <summary>
        /// Gets or sets the selected special focus.
        /// </summary>
        /// <value>
        /// The selected special focus.
        /// </value>
        public string SelectedSpecialFocus { get; set; }

        //[Required(ErrorMessage = @"SSA County is required field.")]
        /// <summary>
        /// Gets or sets the selected county.
        /// </summary>
        /// <value>
        /// The selected county.
        /// </value>
        [CustomValidation(typeof(NursingHomeDetailViewModel), "IsCountySelected", ErrorMessage = @"SSA County is a required field")]
        public County SelectedCounty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded { get; set; }

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        [Import]
        public IHospitalRegistryService Service { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether [is county selected] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static ValidationResult IsCountySelected(object value, ValidationContext context)
        {
            var model = context.ObjectInstance as NursingHomeDetailViewModel ?? new NursingHomeDetailViewModel();
            var nursingHome = model.Model;
            if (model.SelectedCounty == null && nursingHome != null &&
                (model.IsLoaded || !string.IsNullOrEmpty(nursingHome.Name) || !string.IsNullOrEmpty(nursingHome.State) || !string.IsNullOrEmpty(nursingHome.ProviderId)))
            {
                model.IsLoaded = true;
                var result = new ValidationResult("SSA County is a required.", new List<string> { "SelectedCounty" });
                return result;
            }
            return ValidationResult.Success;
        }

        //private bool _saving;
        /// <summary>
        /// Called when [save].
        /// </summary>
        /// <param name="enableNotificantions">if set to <c>true</c> [enable notificantions].</param>
        public override async void OnSave(bool enableNotificantions = false)
        {
            _saveInitiated = true;
           // if (_saving) return;
            Model.Validate();
            if (HasErrors || Model.HasErrors) return;
            DateTime participationDate;
            DateTime.TryParse(DateApproved, out participationDate);
            Model.ParticipationDate = participationDate;

            try
            {
                //_saving = true;
                var errorOccurred = false;
                Exception operationException = null;
                var progressService = new ProgressService();

                progressService.SetProgress("Saving Nursing Home", 0, false, true);

                await Task.Delay(500);

                var operationComplete = await progressService.Execute(() =>
                {
                    if (Model.Certification == NursingHomeType[0]) Model.Certification = null;
                    Model.ChangedOwnership_12Mos = ConvertToBoolean(SelectedProviderChangedOwnership) ?? false;
                    Model.ResFamCouncil = EnumExtensions.GetEnumValueFromString<ResFamCouncilEnum>(SelectedCouncil);
                    Model.SprinklerStatus = SelectedSprinklerSystem ==
                                            SprinklerStatusEnum.DataNotAvailable.GetDescription()
                        ? SprinklerStatusEnum.DataNotAvailable
                        : EnumExtensions.GetEnumValueFromString<SprinklerStatusEnum>(SelectedSprinklerSystem);
                    Model.InHospital = ConvertToBoolean(SelectedInHospital);
                    Model.InRetirementCommunity = ConvertToBoolean(SelectedInRetirementCommunity);
                    Model.HasSpecialFocus = ConvertToBoolean(SelectedSpecialFocus);
                    if (Model.Accreditation == AccreditationOptions[0]) Model.Accreditation = null;
                    if (SelectedCounty != null)
                    {
                        Model.CountySSA = SelectedCounty.CountySSA;
                        Model.CountyName = !string.IsNullOrEmpty(SelectedCounty.Name)
                            ? SelectedCounty.Name.Replace("County", "")
                            : null;
                    }

                    if (HasProviderIdChanged)
                    {
                        using (var session = DataServiceProvider.SessionFactory.OpenSession())
                        {
                            var nursingHome =
                                session.Query<NursingHome>()
                                    .FirstOrDefault(x => x.ProviderId == Model.ProviderId && !x.IsDeleted);
                            if (nursingHome != null && nursingHome.Id != Model.Id)
                            {
                                var msg = string.Format("Assigning a different CMS Provider Number will un-assign this number from one of the existing nursing homes, {0}. You must re-assign a different CMS Provider ID to {1}.",
                                        nursingHome.Name, nursingHome.Name);
                                var prompt = MessageBox.Show(msg, @"Warning", MessageBoxButtons.OKCancel);
                                if (prompt == DialogResult.Cancel) return;

                                using (var trans = session.BeginTransaction())
                                {
                                    nursingHome.ProviderId = null;
                                    session.SaveOrUpdate(nursingHome);
                                    trans.Commit();
                                }
                            }
                        }
                    }

                    base.OnSave(false);

                }, opResult =>
                {
                    if (opResult.Status)
                    {
                        _saveInitiated = false;
                        errorOccurred = false;
                        operationException = null;
                    }
                    else if (!opResult.Status && opResult.Exception != null)
                    {
                        errorOccurred = true;
                        operationException = opResult.Exception;
                    }
                    
                }, new CancellationToken());

                if (operationComplete)
                {
                    EventAggregator.GetEvent<StatusbarUpdateEvent>().Publish(new StatusbarUpdateEventObject { ProcessingText = "Complete", Progress = 100, Reset = true, IsIndeterminate = false });

                    if(!errorOccurred && operationException == null)
                    {
                        Notify(string.Format("\"{0}\" Nursing Home has been saved successfully.", Model.Name));
                        NavigateBack();
                    }
                    else
                    {
                        Logger.Write(operationException, "Error saving Nursing Home \"{0}\"", Model.Name);
                        NotifyError(operationException, typeof(NursingHome), Model.Name);
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Write(exc, "Error saving Nursing Home \"{0}\"", Model.Name);
                NotifyError(exc, typeof(NursingHome), Model.Name);
            }
        }

        /// <summary>
        /// Called when [save internal].
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="model">The model.</param>
        protected override void OnSaveInternal(ISession session, NursingHome model)
        {
            base.OnSaveInternal(session, model);

            if (string.IsNullOrEmpty(_previousProviderId)) return;

            session.CreateSQLQuery(
                string.Format("UPDATE {0} SET NursingHome_ProviderId = '{1}' WHERE NursingHome_ProviderId = '{2}'",
                    Inflector.Pluralize(WebsiteTableNames.WebsiteNursingHomesTable), model.ProviderId, _previousProviderId))
                    .ExecuteUpdate();
        }

        /// <summary>
        /// Called when [cancel].
        /// </summary>
        public override void OnCancel()
        {
            if (IsDirty(Model))
            {
                if (System.Windows.MessageBox.Show("Your changes will be lost if you cancel now. Do you want to Continue?", "Modification Verification", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }
            NavigateBack();
        }

        /// <summary>
        /// Loads the CMS collection.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<string> LoadCmsCollection()
        {
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            var cmsProviderList = new ObservableCollection<string>();
            var states = configService.HospitalRegion.DefaultStates.OfType<string>().ToList();

            using (var session = DataServiceProvider.SessionFactory.OpenStatelessSession())
            {
                states.ForEach(state => cmsProviderList.AddRange(session.Query<NursingHome>().Where(h => h.State == state && !h.IsDeleted).Select(x => x.ProviderId).ToObservableCollection()));
            }
            return cmsProviderList;
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            var nursingHomeId = (navigationContext.Parameters["NursingHomeId"] != null)
                                       ? int.Parse(navigationContext.Parameters["NursingHomeId"])
                                       : (int?)null;

            AvailableStates = Service.GetStates(configService.HospitalRegion.DefaultStates.Cast<string>().ToArray()).ToObservableCollection();
            AvailableCounties = Service.GetCounties(configService.HospitalRegion.DefaultStates.Cast<string>().ToArray()).ToObservableCollection();
            CmsCollection = LoadCmsCollection();

            if (nursingHomeId.HasValue && nursingHomeId.Value > 0)
            {
                LoadModel(nursingHomeId);
                ViewLabel = string.Format("Edit Nursing Home: {0}", Model.Name);
            }
            else
            {
                ViewLabel = "Add New Nursing Home";
                Model = new NursingHome();
            }

            IsProviderIdNew = false;
            DateApproved = Model.ParticipationDate.HasValue ? Model.ParticipationDate.Value.ToString("MM/dd/yyyy") : string.Empty;
            _previousProviderId = Model.ProviderId;


            SelectedProviderChangedOwnership = ConvertSprinklerStatusToString(Model.ChangedOwnership_12Mos);
            SelectedCouncil = Model.ResFamCouncil.HasValue ? Model.ResFamCouncil.ToString() : ResFamCouncilEnum.None.ToString();
            SelectedSprinklerSystem = Model.SprinklerStatus.HasValue && Model.SprinklerStatus != SprinklerStatusEnum.DataNotAvailable ? Model.SprinklerStatus.ToString() : SprinklerStatusEnum.DataNotAvailable.GetDescription();
            SelectedInHospital = ConvertSprinklerStatusToString(Model.InHospital);
            SelectedInRetirementCommunity = ConvertSprinklerStatusToString(Model.InRetirementCommunity);
            SelectedSpecialFocus = ConvertSprinklerStatusToString(Model.HasSpecialFocus);
            if (string.IsNullOrEmpty(Model.Accreditation))
                Model.Accreditation = AccreditationOptions.First();
            if (string.IsNullOrEmpty(Model.Certification))
                Model.Certification = NursingHomeType.First();


            SelectedCounty = Model.IsPersisted ? AvailableCounties.FirstOrDefault(x => x.CountySSA == Model.CountySSA && x.Name != null && x.Name.ContainsCaseInsensitive(Model.CountyName))
                : null;
            OriginalModelHashCode = GetModelHashCode(Model);

            PropertyChanged += (o, e) => Validate();
            Model.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName.Equals("ProviderId"))
                {
                    HasProviderIdChanged = true;
                    IsProviderIdNew = !string.IsNullOrEmpty(Model.ProviderId) && Model.ProviderId.Length <= 6 && !CmsCollection.Contains(Model.ProviderId);
                }

                Validate();
                Model.Validate();
            };

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
            Model = null;
            IsLoaded = false;
        }

        /// <summary>
        /// Navigates the back.
        /// </summary>
        private void NavigateBack()
        {
            var q = new UriQuery();
            q.Add("TabIndex", "2");
            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MainDataSetView + q, UriKind.Relative));
        }

        /// <summary>
        /// Converts to boolean.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool? ConvertToBoolean(string value)
        {
            if (value == SprinklerStatusEnum.Yes.ToString())
                return true;
            if (value == SprinklerStatusEnum.No.ToString())
                return false;

            return null;
        }

        /// <summary>
        /// Converts the sprinkler status to string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string ConvertSprinklerStatusToString(bool? value)
        {
            return !value.HasValue ? SprinklerStatusEnum.DataNotAvailable.GetDescription()
                : value.Value ? SprinklerStatusEnum.Yes.ToString() : SprinklerStatusEnum.No.ToString();
        }

        #endregion
    }
}
