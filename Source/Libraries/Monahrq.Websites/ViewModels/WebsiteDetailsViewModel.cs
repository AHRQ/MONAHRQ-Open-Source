using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Types;
using NHibernate.Util;
using PropertyChanged;
using Monahrq.Sdk.Utilities;
using Monahrq.Websites.Events;

namespace Monahrq.Websites.ViewModels
{
    [Export(typeof(WebsiteDetailsViewModel))]
    [ImplementPropertyChanged]
    public class WebsiteDetailsViewModel : WebsiteTabViewModel //BaseTabViewModel, INavigationAware
    {
        #region Fields and Constants

        private SelectListItem _selectedState;

        #endregion

        #region Properties
        //public string Name { get; set; }
        //public string Description { get; set; }
        //public string SelectedYear { get; set; }
        //public virtual WebsiteState? CurrentStatus { get; set; }
        //public int? SelectedQuarter { get; set; }
        //public string SelectedAudience { get; set; }

        public ObservableCollection<SelectListItem> Quarters { get; set; }

        public ObservableCollection<SelectListItem> Years { get; set; }

        public ObservableCollection<SelectListItem> Audiences { get; set; }

        public string ViewTitle { get; set; }

        [CustomValidation(typeof(WebsiteDetailsViewModel), "IsRegionContextSelected")]
        public SelectListItem SelectedRegionContext { get; set; }

        public ObservableCollection<SelectListItem> RegionContextItems { get; set; }

        [CustomValidation(typeof(WebsiteDetailsViewModel), "IsStateSelected")]
        public ObservableCollection<SelectListItem> StateContextItems { get; set; }

        public SelectListItem SelectedState
        {
            get { return _selectedState; }
            set
            {
                _selectedState = value;

                if (_selectedState != null && _selectedState.Value != null)
                    AddStateToContextCommand.Execute();
            }
        }

        public ObservableCollection<SelectListItem> SelectedStateItems { get; set; }

        //public ObservableCollection<SelectListItem> SelectedStateItems { get; set; }

        public DelegateCommand AddStateToContextCommand { get; set; }

        public DelegateCommand<SelectListItem> RemoveStateFromContextCommand { get; set; }

        private string _headerText;
        public string HeaderText
        {
            get { _headerText = "Overview"; return _headerText; }
            set
            {
                _headerText = value;
            }
        }

        [Required(ErrorMessage = @"Please enter a valid name.", AllowEmptyStrings = false)]
        [StringLength(255, ErrorMessage = @"Please enter a valid website name fewer than 255 characters.")]
        [DoNotCheckEquality]
        public string WebsiteName
        {
            get { return CurrentWebsite != null ? CurrentWebsite.Name : null; }
            set
            {
                if (CurrentWebsite == null) return;
                CurrentWebsite.Name = value;
            }
        }

        #endregion

        #region Fields and Constants

        private const string DEFAULT_YEAR_SELECT_TEXT = "Please Select Reporting Year";
        private const string DEFAULT_AUDIENCE_SELECT_TEXT = "Please Select Audience";
        private const string DEFAULT_QUARTER_SELECT_TEXT = "All Quarters";

        #endregion

        #region Methods

        public override void Refresh()
        {
            base.Refresh();
            IsTabVisited = true;

            if (MonahrqContext.IsInializing) return;

            FillCombos(true);
            InitiateWebsiteProperties();

            if (CurrentWebsite == null) return;

            CurrentWebsite.PropertyChanged -= (o, e) => ValidateAllRequiredFields();
            CurrentWebsite.PropertyChanged += (o, e) =>
            {
                ValidateAllRequiredFields();
                if (!string.IsNullOrEmpty(e.PropertyName) && (e.PropertyName.Contains("HasProfessionalsAudience") || e.PropertyName.Contains("HasConsumersAudience") || e.PropertyName.Contains("ReportedYear")))
                {
                    if (e.PropertyName.Contains("ReportedYear"))
                    {
                        ManageViewModel.IsTrendingYearUpdated = true;
                    }
                    else
                    {
                        ManageViewModel.IsAudienceChanged = true;

                    }
                }
            };
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected override void InitCommands()
        {
            //InitProperties();
            AddStateToContextCommand = new DelegateCommand(OnAddState, CanAddState);
            RemoveStateFromContextCommand = new DelegateCommand<SelectListItem>(OnRemoveStateContext, CanRemoveState);
        }

        /// <summary>
        /// Initializes the properties.
        /// </summary>
        protected override void InitProperties()
        {
            ViewTitle = "Edit Name, Description for the Website";
            FillCombos();
            SelectedStateItems = new ObservableCollection<SelectListItem>();
        }

        public override void Continue()
        {
            if (CurrentWebsite == null) return;

            UpdateModelFromProperties();
        }

        public override void Save()
        {
            // TODO: change this to preferred validation model. These string messages are per the I-4 spec.

            if (!ValidateAllRequiredFields()) return;

            UpdateModelFromProperties();

            CurrentWebsite.ActivityLog.Add(new ActivityLogEntry("Details created and/or updated", DateTime.Now));

            bool saveErrorsOccurred;
            string eventMessage;
            if (!CurrentWebsite.IsPersisted) // If the website is not persisted, then add new, otherwise, update
            {
                // Add New
                saveErrorsOccurred = WebsiteDataService.SaveNewWebsite(ManageViewModel.WebsiteViewModel.Website);
                eventMessage = String.Format("Website {0} has been added", CurrentWebsite.Name);
            }
            else
            {
                // Update
                //// If the website is edited, the current status must change in order to allow for the dependency check to be readily available when publishing
                //WebsiteViewModel.Website.CurrentStatus = WebsiteState.Initialized;

                saveErrorsOccurred = WebsiteDataService.UpdateWebsite(CurrentWebsite);
                eventMessage = String.Format("Website {0} has been updated", CurrentWebsite.Name);
            }

            if (!saveErrorsOccurred) // If no errors in saving website, then publish events
            {
                // publish events so all the associated view models will get the updated item
                EventAggregator.GetEvent<WebsiteCreatedOrUpdatedEvent>().Publish(new ExtendedEventArgs<GenericWebsiteEventArgs>(new GenericWebsiteEventArgs() { Website = base.ManageViewModel.WebsiteViewModel, Message = eventMessage }));
                // EventAggregator.GetEvent<UpdateWebsiteTabContextEvent>().Publish(
                //new UpdateTabContextEventArgs()
                //{
                //    WebsiteViewModel = base.ManageViewModel.WebsiteViewModel,
                //    ExecuteViewModel = WebsiteTabViewModels.Details
                //});
            }
        }

        /// <summary>
        /// Updates the website domain model properties from website details view model data properties.
        /// </summary>
        private void UpdateModelFromProperties()
        {
            //ManageViewModel.WebsiteViewModel.Website.Name = Name;
            //ManageViewModel.WebsiteViewModel.Website.Description = Description;
            //ManageViewModel.WebsiteViewModel.Website.ReportedYear = SelectedYear;
            //ManageViewModel.WebsiteViewModel.Website.ReportedQuarter = SelectedQuarter == -1 ? null : SelectedQuarter;
            //ManageViewModel.WebsiteViewModel.Website.Audience = (!string.IsNullOrWhiteSpace(SelectedAudience))
            //                                        ? (Audience)Enum.Parse(typeof(Audience), SelectedAudience)
            //                                        : Audience.None;

            if (SelectedRegionContext != null && CurrentWebsite != null)
            {
                CurrentWebsite.RegionTypeContext = SelectedRegionContext.Value.ToString();
            }

            if (SelectedStateItems != null && StateContextItems.Any() && CurrentWebsite != null)
            {
                CurrentWebsite.StateContext.Clear();

                ListExtensions.ForEach(SelectedStateItems, sli => CurrentWebsite.StateContext.Add(sli.Value.ToString()));

                CurrentWebsite.SelectedReportingStates = CurrentWebsite.StateContext.ToList();
            }
        }

        /// <summary>
        /// Clears the website details view model data properties.
        /// </summary>
        private void ClearDataProperties()
        {
            CurrentWebsite.Name = null;
            CurrentWebsite.Description = null;
            CurrentWebsite.ReportedYear = null;
            CurrentWebsite.ReportedQuarter = null;
            //CurrentWebsite.Audience = Audience.None;
            CurrentWebsite.Audiences.Clear();

            //SelectedAudience = null;
            SelectedRegionContext = null;
            SelectedStateItems = new ObservableCollection<SelectListItem>();
            SelectedState = null;
        }

        public override void ValidateOnChange()
        {
            IsValid = ValidateAllRequiredFields();
        }

        public override bool ShouldValidate
        {
            get { return true; }
        }

        public override bool ValidateAllRequiredFields()
        {
            if (CurrentWebsite == null) return false;

            CurrentWebsite.Validate();
            Validate();

            var websitehasErrors = CurrentWebsite.HasErrorExcluding(new List<string>()
            {
                "SelectedTheme","GeographicDescription","BrowserTitle",
                "HeaderTitle","BrandColor", "AccentColor",
                "BannerImage", "OutPutDirectory"
            });

            return !HasErrors && !websitehasErrors;

            //if (string.IsNullOrWhiteSpace(CurrentWebsite.Name) || string.IsNullOrEmpty(CurrentWebsite.Name))
            //{
            //    MessageBox.Show("Please enter a valid name.");
            //    return false;
            //}

            //if (CurrentWebsite.Audipaintence == Audience.None)
            //{
            //    MessageBox.Show("Please select a Target Audience.");
            //    return false;
            //}
            //if (CurrentWebsite.Audiences.Count == 0)
            //{
            //    MessageBox.Show("Please select a Target Audience.");
            //    return false;
            //}
            //if (CurrentWebsite.Audiences.Count > 1 && CurrentWebsite.DefaultAudience == null)
            //{
            //    MessageBox.Show("Please select a Default Target Audience.");
            //    return false;
            //}

            //if (string.IsNullOrWhiteSpace(CurrentWebsite.ReportedYear) || string.IsNullOrEmpty(CurrentWebsite.ReportedYear))
            //{
            //    MessageBox.Show("Please select a reporting year to continue.");
            //    return false;
            //}

            //if (SelectedRegionContext == null || SelectedRegionContext.Value == null)
            //{
            //    MessageBox.Show("Please select a regional context.");
            //    return false;
            //}

            //if (SelectedStateItems == null || !SelectedStateItems.Any())
            //{
            //    MessageBox.Show("Please select a state context.");
            //    return false;
            //}
            //return true;
        }

        void InitiateWebsiteProperties()
        {
            if (MonahrqContext.IsInializing) return;

            if (CurrentWebsite == null) return;

            RaisePropertyChanged(() => ManageViewModel.WebsiteViewModel);
            RaisePropertyChanged(() => ManageViewModel.WebsiteViewModel.Website);

            RaisePropertyChanged(() => CurrentWebsite);
            RaisePropertyChanged(() => CurrentWebsite.Name);
            RaisePropertyChanged(() => WebsiteName);
            RaisePropertyChanged(() => CurrentWebsite.Description);
            RaisePropertyChanged(() => CurrentWebsite.CurrentStatus);
            RaisePropertyChanged(() => CurrentWebsite.ReportedYear);
            RaisePropertyChanged(() => CurrentWebsite.ReportedQuarter);
            RaisePropertyChanged(() => CurrentWebsite.DefaultAudience);
            RaisePropertyChanged(() => CurrentWebsite.HasConsumersAudience);
            RaisePropertyChanged(() => CurrentWebsite.HasProfessionalsAudience);

            //SelectedAudience = CurrentWebsite.Audience.ToString();

            SelectedRegionContext = RegionContextItems.FirstOrDefault(reg => reg.Value.ToString().EqualsIgnoreCase(CurrentWebsite.RegionTypeContext));

            if (CurrentWebsite.StateContext != null && CurrentWebsite.StateContext.Any())
            {
                SelectedStateItems.Clear();

                //StateContextItems = StateContextItems.Where(sli => base.ManageViewModel.WebsiteViewModel.Website.StateContext.Any(s => s.EqualsIgnoreCase(sli.Value.ToString()))).ToObservableCollection();

                foreach (var stateContext in CurrentWebsite.StateContext)
                {
                    var contextItem = StateContextItems.FirstOrDefault(s => s.Value != null && s.Value.ToString().EqualsIgnoreCase(stateContext));

                    if (contextItem == null) return;

                    SelectedStateItems.Add(contextItem);
                    StateContextItems.Remove(contextItem);
                }

                RaisePropertyChanged(() => SelectedStateItems);
            }
            else
            {
                SelectedState = null;
            }
        }

        void FillCombos(bool refresh = false)
        {
            if (!refresh)
            {
                Audiences = WebsiteDataService.Audiences.Select(o => new SelectListItem { Value = ((Audience)o.Value).ToString(), Text = o.Name, Model = o }).ToObservableCollection();
                Audiences.RemoveAt(0);
                Audiences.Insert(0, new SelectListItem { Value = Audience.None.ToString(), Text = DEFAULT_AUDIENCE_SELECT_TEXT, Model = null });

                Years = BaseDataService.ReportingYears.Select(o => new SelectListItem { Value = o, Text = o, Model = o }).ToObservableCollection();
                Years.RemoveAt(0);
                Years.Insert(0, new SelectListItem { Value = string.Empty, Text = DEFAULT_YEAR_SELECT_TEXT, Model = null });

                Quarters = BaseDataService.ReportingQuarters.Select(o => new SelectListItem { Value = o.Id, Text = o.Text, Model = o }).ToObservableCollection();
                Quarters.RemoveAt(0);
                Quarters.Insert(0, new SelectListItem { Value = -1, Text = DEFAULT_QUARTER_SELECT_TEXT, Model = null });

                RegionContextItems = new ObservableCollection<SelectListItem>();
                // RegionContextItems.Insert( 0, new SelectListItem { Text= "Please Select Region", Value = null, Model = null } );
                RegionContextItems.Add(new SelectListItem { Text = Inflector.Titleize(typeof(HospitalServiceArea).Name), Value = typeof(HospitalServiceArea).Name, Model = null });
                RegionContextItems.Add(new SelectListItem { Text = Inflector.Titleize(typeof(HealthReferralRegion).Name), Value = typeof(HealthReferralRegion).Name, Model = null });
                RegionContextItems.Add(new SelectListItem { Text = Inflector.Titleize(typeof(CustomRegion).Name), Value = typeof(CustomRegion).Name, Model = null });
            }

            StateContextItems = new ObservableCollection<SelectListItem>();
            // StateContextItems.Insert(0, new SelectListItem { Text = "Please Select State(s)", Value = null, Model = null });

            StateContextItems = WebsiteDataService.GetStates().ToObservableCollection();
            // else
            //    StateContextItems =
            //        WebsiteDataService.GetApplicableReportingStates(
            //            ManageViewModel.WebsiteViewModel.Website.StateContext.ToArray()).ToObservableCollection();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            // ClearDataProperties();
            // if (navigationContext.Parameters["WebsiteId"] != null)
            // {
            //    var websiteId = int.Parse(navigationContext.Parameters["WebsiteId"]);

            //    if (websiteId > 0)
            //    {
            //        Website ws;
            //        WebsiteDataService.GetEntityById<Website>(websiteId, (result, error) =>
            //            {
            //                if (error == null)
            //                {
            //                    ws = result;
            //                }
            //                else
            //                {
            //                    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(error);
            //                }
            //            });
            //    }

            // }
            ClearDataProperties();

            if (MonahrqContext.IsInializing) return;

            //InitiateWebsiteProperties();
        }

        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        private void OnAddState()
        {
            if (SelectedState == null || SelectedState.Value == null) return;
            AddStateToContext(SelectedState);
        }

        private void AddStateToContext(SelectListItem state)
        {
            if (!SelectedStateItems.Contains(state))
                SelectedStateItems.Add(state);

            if (StateContextItems.Contains(state))
                StateContextItems.Remove(state);

            SelectedState = null;
        }

        private bool CanAddState()
        {
            var selectStateBlank = StateContextItems[0];
            return SelectedState != null && SelectedState.Value != null &&
                   selectStateBlank != null && !SelectedState.Value.Equals(selectStateBlank.Value);
        }

        private bool CanRemoveState(SelectListItem state)
        {
            return true; // state != null && state.Value != null;
        }

        private void OnRemoveStateContext(SelectListItem state)
        {
            SelectedStateItems.Remove(state);
            SelectedState = null;

            StateContextItems.Add(state);
            StateContextItems = StateContextItems
                .OrderBy(item => item.Value != null)
                .ThenBy(item => item.Value).ToObservableCollection();
        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            Index = 0;
            EventAggregator.GetEvent<SetContextualHelpContextEvent>().Publish("Using the Website Wizard");

        }

        public override bool TabChanged()
        {
            UpdateModelFromProperties();
            return true; // ValidateAllRequiredFields();
        }

        public static ValidationResult IsRegionContextSelected(object sender, ValidationContext context)
        {
            var viewModel = context.ObjectInstance as WebsiteDetailsViewModel ?? new WebsiteDetailsViewModel();
            if (viewModel.SelectedRegionContext == null || viewModel.SelectedRegionContext.Value == null)
            {
                var result = new ValidationResult("Please select a regional context.", new List<string> { "SelectedRegionContext" }); ;
                return result;
            }
            return ValidationResult.Success;
        }

        public static ValidationResult IsStateSelected(object sender, ValidationContext context)
        {
            var viewModel = context.ObjectInstance as WebsiteDetailsViewModel ?? new WebsiteDetailsViewModel();
            if (viewModel.SelectedStateItems == null || !viewModel.SelectedStateItems.Any())
            {
                var result = new ValidationResult("Please select a state context.", new List<string> { "StateContextItems" }); ;
                return result;
            }
            return ValidationResult.Success;
        }

        #endregion
    }
}
