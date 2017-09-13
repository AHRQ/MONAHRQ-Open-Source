using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Types;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Services.Generators;
using Monahrq.Sdk.Types;
using Monahrq.Websites.Events;
using NHibernate.Util;
using PropertyChanged;
using Application = System.Windows.Application;
using Cursor = System.Windows.Forms.Cursor;
using Cursors = System.Windows.Forms.Cursors;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using MenuItem = Monahrq.Infrastructure.Domain.Websites.Menu;
using Monahrq.Infrastructure.Utility;
using System.Drawing;
using Microsoft.Practices.Prism.Regions;

namespace Monahrq.Websites.ViewModels
{
    using System.Collections;

    using LinqKit;

    [Export(typeof(WebsiteSettingsViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ImplementPropertyChanged]
    public class WebsiteSettingsViewModel : WebsiteTabViewModel
    {
        #region Fields and Constants

        /* const string VALID_URL_MESSAGE = "Please provide a valid URL, beginning with http:// or https://.";*/

        private bool _loadingHospitalSelection;

        private bool? _isAllRadiusSelected = null;

        private string _selectedNursingHomeFilter;

        private bool _showWarningButtonOverlays = true;

        private string _selectedTheme;

        private bool _isAllHospitalsSelected;

        private SelectListItem _selectedHospitalFilter;

        private bool _isAllNursingHomesSelected;

        private readonly SelectListItem _defaultFilterItem = new SelectListItem
                                                                 {
                                                                     Model = HospitalFilterByEnum.None,
                                                                     Text =
                                                                         HospitalFilterByEnum.None
                                                                         .GetDescription(),
                                                                     Value = (int)HospitalFilterByEnum.None,
                                                                     IsSelected = true
                                                                 };

        private bool _isInitialLoad;

        private bool _isReseting;

        private MonahrqBannerElement _selectedBanner;

        private string _outputDirectoryPath;

        private const string USER_SELECTED_BANNER_NAME = "Custom Image";

        private static readonly Regex _regex =
            new Regex(
                @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        private string __radiusSelectionButtonContent = string.Empty;

        private const string ThemeSelectionConfirmation =
            "Changing the color theme for your site will modify the Brand Colors, Accent Colors, selected in the Advanced Color Options subsection.  Would you like to continue?";

        private enum PopupView
        {
            WebsitePreview,

            BannerImagePreview,

            LogoImagePreview,

            ThemePreview,

            HospitalView,

            NursingHomeView,

            AdvancedOptionsView,
        }


        #endregion

        #region  Commands

        //private readonly SelectListItem _defaultReportingState = new SelectListItem { Text = "Select a state to report on", Value = null };




        public ICommand SelectDirectoryCommand { get; set; }

        public ICommand AddFeedBackTopic { get; set; }

        public ICommand RemoveFeedBackTopic { get; set; }

        public ICommand SelectImageFileCommand { get; set; }

        public DelegateCommand<string> PreviewImageFileCommand { get; set; }

        public DelegateCommand<string> PreviewFontFamilyCommand { get; set; }

        public DelegateCommand<string> PreviewThemeCommand { get; set; }

        public ICommand OpenHospitalSelectionCommand { get; set; }

        public DelegateCommand<string> CloseHospitalSelectionCommand { get; set; }

        public ICommand HospitalSelectionCompleteCommand { get; set; }

        public DelegateCommand<HospitalFilterByEnum> FilterHospitalSelectionCommand { get; set; }

        public ICommand WarningButtonCommand { get; set; }

        public ICommand PreviewStandadFeedbackFormCommand { get; set; }

        public ICommand WebsitePreviewCommand { get; set; }

        public ICommand CloseWebsitePreviewCommand { get; set; }

        public DelegateCommand<string> RadiusSelectionCommand { get; set; }

        public DelegateCommand<string> NursingHomeSelectionCompleteCommand { get; set; }

        public DelegateCommand OpenNursingHomeSelectionCommand { get; set; }

        public DelegateCommand<string> ClosePreviewImageCommand { get; set; }

        public DelegateCommand CloseThemePreviewPopup { get; private set; }

        public DelegateCommand CloseHospitalSelectionPopup { get; private set; }

        public DelegateCommand<MenuItem> SaveMenuItemCommand { get; set; }

        public DelegateCommand<MenuItem> CancelMenuEditCommand { get; set; }

        public DelegateCommand ViewWithBrowserCommand { get; set; }

        public DelegateCommand AdvancedOptionsViewCommand { get; set; }

        public DelegateCommand<object> ZipcodeListBoxItemsSelectionChangedCommand { get; set; }

        #endregion

        #region Imports

        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        IConfigurationService ConfigService { get; set; }

        [Import]
        public WebsiteGenerator Generator { get; set; }

        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public WebsitePagesViewModel WebsitePagesViewModel { get; set; }

        #endregion

        #region Properties

        public string ThemePreviewFilePath { get; set; }

        public bool OpenThemePreviewPopup { get; set; }

        public bool IsWebsitePreviewOpen { get; set; }

        [DoNotCheckEquality]
        public bool IsAllHospitalsSelected
        {
            //get
            //{
            //    if (HospitalsCollectionView == null || !HospitalsCollectionView.Any()) return false;

            //    return HospitalsCollectionView.Count == HospitalsCollectionView.OfType<HospitalViewModel>().Count(h => h.IsSelected);
            //}
            //set
            //{
            //    //_isAllHospitalsSelected = value;

            //    if (_loadingHospitalSelection) return;

            //    if (HospitalsCollectionView == null || !HospitalsCollectionView.OfType<HospitalViewModel>().Any())
            //        return;

            //    foreach (var hospital in HospitalsCollectionView.OfType<HospitalViewModel>().ToList())
            //    {
            //        hospital.IsSelected = value;
            //    }

            //    HospitalsCollectionView.Refresh();
            //}

            get
            {
                if (HospitalsCollectionView == null || !HospitalsCollectionView.Any()) return false;
                return HospitalsCollectionView.SelectedItems.Count == HospitalsCollectionView.Count;
            }
            set
            {
                if (HospitalsCollectionView == null || !HospitalsCollectionView.OfType<HospitalViewModel>().Any()) return;

                HospitalsCollectionView.OfType<HospitalViewModel>().ToList().ForEach(nh => { nh.IsSelected = value; });

                RaisePropertyChanged(() => HospitalsCollectionView);
                RaisePropertyChanged(() => IsAllHospitalsSelected);
            }
        }

        public bool IsAllRadiusSelected
        {
            get
            {
                return !_isAllRadiusSelected.HasValue || _isAllRadiusSelected.Value;
            }
            set
            {
                _isAllRadiusSelected = value;
            }
        }

        public string RadiusSelectionButtonContent
        {
            get
            {
                return __radiusSelectionButtonContent != string.Empty ? __radiusSelectionButtonContent : "UnSelect All";
            }
            set
            {
                __radiusSelectionButtonContent = value;
            }
        }

        public override void OnHostNavigatedTo(NavigationContext navigationContext)
        {
            base.OnHostNavigatedTo(navigationContext);

            //Refresh();
        }

        /// <summary>
        /// Gets or sets the filter by.
        /// </summary>
        /// <value>
        /// The filter by.
        /// </value>
        public ObservableCollection<SelectListItem> FilterBy { get; set; }

        /// <summary>
        /// Gets or sets the selected hospital filter.
        /// </summary>
        /// <value>
        /// The selected hospital filter.
        /// </value>
        public SelectListItem SelectedHospitalFilter
        {
            get
            {
                return _selectedHospitalFilter;
            }
            set
            {
                _selectedHospitalFilter = value;
                if (_loadingHospitalSelection) return;

                if (_selectedHospitalFilter == null) return;

                var filter = (HospitalFilterByEnum)_selectedHospitalFilter.Value;
                OnFilterHospitals(filter);
                //FilterHospitalSelectionCommand.Execute(filter);
            }
        }

        /// <summary>
        /// Gets or sets the hospitals collection view.
        /// </summary>
        /// <value>
        /// The hospitals collection view.
        /// </value>
        public MultiSelectCollectionView<HospitalViewModel> HospitalsCollectionView { get; set; }

        /// <summary>
        /// Gets or sets the nursing home collection view.
        /// </summary>
        /// <value>
        /// The nursing home collection view.
        /// </value>
        public MultiSelectCollectionView<NursingHome> NursingHomeCollectionView { get; set; }

        /// <summary>
        /// Gets or sets the number of avaliable hospitals.
        /// </summary>
        /// <value>
        /// The number of avaliable hospitals.
        /// </value>
        public int NumberOfAvaliableHospitals { get; set; }

        /// <summary>
        /// Gets or sets the show hospital selection view.
        /// </summary>
        /// <value>
        /// The show hospital selection view.
        /// </value>
        public bool ShowHospitalSelectionView { get; set; }

        public bool ShowNursingSelectionView { get; set; }

        /// <summary>
        /// Gets a value indicating whether [enable hospital section link].
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable hospital section link]; otherwise, <c>false</c>.
        /// </value>
        //public bool EnableHospitalSectionLink { get; set; }
        public bool EnableHospitalSectionLink { get; set;
            //get
            //{
            //    var result = ApplicableReportingStates != null &&
            //                 ApplicableReportingStates.OfType<SelectListItem>().ToList().Any(s => s.IsSelected);

            //    NumberOfAvaliableHospitals = GetSelectableHospitalCount();

            //    return result;
            //}
        }

        /// <summary>
        /// Gets or sets the applicable zip code radii.
        /// </summary>
        /// <value>
        /// The applicable zip code radii.
        /// </value>
        [CustomValidation(typeof(WebsiteSettingsViewModel), "IsWebsiteRadiusSelected")]
        public ObservableCollection<SelectListItem> ApplicableRadiusMiles { get; set; }

        /// <summary>
        /// Gets or sets the applicable reporting states.
        /// </summary>
        /// <value>
        /// The applicable reporting states.
        /// </value>
        public ObservableCollection<SelectListItem> ApplicableReportingStates { get; set; }

        /// <summary>
        /// Gets or sets the applicable site themes.
        /// </summary>
        /// <value>
        /// The applicable site themes.
        /// </value>
        public ObservableCollection<string> ApplicableSiteThemes { get; set; }

        /// <summary>
        /// Gets or sets the applicable site fonts.
        /// </summary>
        /// <value>
        /// The applicable site fonts.
        /// </value>
        public ObservableCollection<SelectListItem> ApplicableSiteFonts { get; set; }

        public string WebBrowserSourceUrl { get; set; }

        //public bool IsStandardFeedbackFormSelected { get; set; }

        //public bool IncludeFeedbackFormInYourWebsite { get; set; }

        public bool IsCustomFeedbackFormSelected
        {
            get
            {
                return CurrentWebsite != null && !CurrentWebsite.IsStandardFeedbackForm;
            }
            set
            {
                CurrentWebsite.IsStandardFeedbackForm = !value;
            }
        }

        [RegularExpression(
            @"^(http|https)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$"
            )]
        public string CustomFeedbackFormUrl { get; set; }

        public List<string> NursingHomeFilterBy { get; set; }

        public string SelectedNursingHomeFilter
        {
            get
            {
                return _selectedNursingHomeFilter;
            }
            set
            {
                _selectedNursingHomeFilter = value;
                NursingHomeCollectionView.Filter = null;
                NursingHomeCollectionView.Filter = NursingHomeCompositeFilter;
            }
        }

        [DoNotCheckEquality]
        public bool IsAllNursingHomesSelected
        {
            get
            {
                if (NursingHomeCollectionView == null || !NursingHomeCollectionView.Any()) return false;
                return NursingHomeCollectionView.SelectedItems.Count == NursingHomeCollectionView.Count;
            }
            set
            {
                if (NursingHomeCollectionView == null || !NursingHomeCollectionView.OfType<NursingHome>().Any()) return;

                NursingHomeCollectionView.OfType<NursingHome>().ToList().ForEach(nh => { nh.IsSelected = value; });

                RaisePropertyChanged(() => NursingHomeCollectionView);
                RaisePropertyChanged(() => IsAllNursingHomesSelected);

                //_isAllNursingHomesSelected = value;
                //if (NursingHomeCollectionView != null)
                //{
                //    ListExtensions.ForEach(NursingHomeCollectionView.OfType<NursingHome>(), x => x.IsSelected = value);
                //}
            }
        }

        private string _headerText;

        public string HeaderText
        {
            get
            {
                _headerText = "Customize Site";
                return _headerText;
            }
            set
            {
                _headerText = value;
            }
        }

        /// <summary>
        /// Gets or sets the feedback topics.
        /// </summary>
        /// <value>
        /// The feedback topics.
        /// </value>
        public ObservableCollection<string> FeedbackTopics { get; set; }

        /// <summary>
        /// Gets or sets the output directory path.
        /// </summary>
        /// <value>
        /// The output directory path.
        /// </value>
        //[DoNotSetChanged]
        [Required(ErrorMessage = @"Please Provide an output folder.", AllowEmptyStrings = false)]
        [CustomValidation(typeof(WebsiteSettingsViewModel), "IsValidOutputFolder")]
        public string OutputDirectoryPath
        {
            get
            {
                return _outputDirectoryPath;
            }
            set
            {
                _outputDirectoryPath = value;

                _outputDirectoryPath = value;

                RaisePropertyChanged(() => OutputDirectoryPath);
                if (CurrentWebsite == null) return;

                CurrentWebsite.OutPutDirectory = _outputDirectoryPath;
                RaisePropertyChanged(() => CurrentWebsite.OutPutDirectory);
            }
        }


        /// <summary>
        /// Gets or sets the selected theme.
        /// </summary>
        /// <value>
        /// The selected theme.
        /// </value> 
        //[DoNotSetChanged]
        [Required(ErrorMessage = @"Please select a theme.")]
        public string SelectedTheme
        {
            get
            {
                return _selectedTheme ?? ConfigService.MonahrqSettings.Themes.DefaultTheme.Name;
            }
            set
            {
                var oldValue = _selectedTheme;
                if (!_isReseting)
                {
                    if (!_isInitialLoad
                        && MessageBox.Show(ThemeSelectionConfirmation, "Confirmation", MessageBoxButton.YesNo)
                        == MessageBoxResult.No)
                    {
                        Dispatcher.CurrentDispatcher.BeginInvoke(
                            new Action(
                                () =>
                                    {
                                        _selectedTheme = oldValue;
                                        RaisePropertyChanged(() => SelectedTheme);

                                    }));
                        return;
                    }
                }


                _selectedTheme = value;

                ThemePreviewFilePath = MonahrqContext.BinFolderPath + @"\Resources\ThemePreviews\" + _selectedTheme
                                       + ".png";
                PreviewThemeCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(() => SelectedTheme);

                var theme =
                    ConfigService.MonahrqSettings.Themes.OfType<MonahrqThemeElement>()
                        .SingleOrDefault(t => t.Name.EqualsIgnoreCase(_selectedTheme));

                if (theme == null || _isInitialLoad) return;

                BrandColor = theme.BrandColor;
                AccentColor = theme.AccentColor;
                ConsumerBrandColor = theme.Brand2Color;
            }
        }

        public string BrandColor { get; set; }

        public string AccentColor { get; set; }

        public string SelectedFont { get; set; }

        public string ConsumerBrandColor { get; set; }

        /// <summary>
        /// Gets or sets the logo image.
        /// </summary>
        /// <value>
        /// The logo image.
        /// </value>
        public WebsiteImage LogoImage { get; set; }

        /// <summary>
        /// Gets or sets the banner image.
        /// </summary>
        /// <value>
        /// The banner image.
        /// </value>
        [Required(ErrorMessage = @"Please provide a banner image file.")]
        public WebsiteImage BannerImage { get; set; }

        /// <summary>
        /// Gets or sets the homepage content image.
        /// </summary>
        /// <value>
        /// The homepage content image.
        /// </value>
        public WebsiteImage HomepageContentImage { get; set; }

        public List<MonahrqBannerElement> AvailableBanners
        {
            get
            {
                return ConfigService.MonahrqSettings.Banners.OfType<MonahrqBannerElement>().ToList();
            }
        }

        public MonahrqBannerElement SelectedBanner
        {
            get
            {
                return _selectedBanner;
            }
            set
            {
                _selectedBanner = value;
                if (_selectedBanner == null) return;

                var fileName = !string.IsNullOrEmpty(_selectedBanner.Value)
                                   ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _selectedBanner.Value)
                                   : string.Empty;
                SetBannerImage(fileName, _selectedBanner.Name);
                PreviewImageFileCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsUserSelectedBanner { get; set; }

        public MonahrqBannerElement DefaultBanner
        {
            get
            {
                return
                    ConfigService.MonahrqSettings.Banners.OfType<MonahrqBannerElement>()
                        .FirstOrDefault(x => x.Name.ToLower().EqualsIgnoreCase("Healthcare Providers"));
            }
        }

        public bool IsPreviewingBannerImage { get; set; }

        public bool IsPreviewingLogoImage { get; set; }

        [CustomValidation(typeof(WebsiteSettingsViewModel), "ValidateFeedBackEmail")]
        public string FeedBackEmailTemp
        {
            get
            {
                return CurrentWebsite != null ? CurrentWebsite.FeedBackEmail : string.Empty;
            }
            set
            {
                if (CurrentWebsite == null) return;

                CurrentWebsite.FeedBackEmail = value;
                RaisePropertyChanged(() => FeedBackEmailTemp);
                Validate();
            }
        }

        public IList<MenuItem> AllAvailableMenuItems { get; set; }

        public ListCollectionView ProfessionalWebsiteMenuItems { get; set; }

        public ListCollectionView ConsumerWebsiteMenuItems { get; set; }

        public bool WebsiteMenuHasErrors { get; set; }

        public bool WebsiteContentHasErrors { get; set; }

        public bool WebsitePagesHasErrors { get; set; }

        public bool WebsiteThemesHasErrors { get; set; }

        public bool IsAdvancedOptionsImageVisible { get; set; }

        #endregion

        #region Methods

        private bool NursingHomeCompositeFilter(object obj)
        {
            var nursingHome = obj as NursingHome;
            if (nursingHome == null) return true;

            var selectedEnumValue = _selectedNursingHomeFilter.GetValueFromDescription<NursingHomeFilterByEnum>();
            switch (selectedEnumValue)
            {
                case NursingHomeFilterByEnum.UnSelected:
                    return !nursingHome.IsSelected;
                case NursingHomeFilterByEnum.Selected:
                    return nursingHome.IsSelected;
                default:
                    return true;
            }
        }

        public bool AreProfessionalMenuItemsAdded { get; set; }

        public bool AreConsumerMenuItemsAdded { get; set; }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public override void Save()
        {
            UpdateSettingsProperties();

            CurrentWebsite.ActivityLog.Add(
                new ActivityLogEntry("Settings have be saved and/our updated.", DateTime.Now));

            string message;
            bool errorsOccurredWhileSaving;
            if (!CurrentWebsite.IsPersisted)
            {
                errorsOccurredWhileSaving = WebsiteDataService.SaveNewWebsite(CurrentWebsite);
                message = String.Format("Website {0} has been added", CurrentWebsite.Name);
            }
            else
            {
                // If the website is edited, the current status must change in order to allow for the dependency check to be readily available when publishing
                ManageViewModel.WebsiteViewModel.Website.CurrentStatus = WebsiteState.Initialized;

                errorsOccurredWhileSaving = WebsiteDataService.UpdateWebsite(CurrentWebsite);
                message = String.Format("Website {0} has been updated", CurrentWebsite.Name);
            }

            if (!errorsOccurredWhileSaving)
            {
                var eventArgs = new ExtendedEventArgs<GenericWebsiteEventArgs>
                                    {
                                        Data =
                                            new GenericWebsiteEventArgs
                                                {
                                                    Website
                                                        =
                                                        ManageViewModel
                                                        .WebsiteViewModel,
                                                    Message
                                                        =
                                                        message
                                                }
                                    };

                EventAggregator.GetEvent<WebsiteCreatedOrUpdatedEvent>().Publish(eventArgs);
            }
        }

        /// <summary>
        /// Continues this to the next tab in the website creation process.
        /// </summary>
        public override void Continue()
        {
            UpdateSettingsProperties();
            //EventAggregator.GetEvent<UpdateWebsiteTabContextEvent>()
            //    .Publish(new UpdateTabContextEventArgs
            //    {
            //        WebsiteViewModel = ManageViewModel.WebsiteViewModel,
            //        ExecuteViewModel = WebsiteTabViewModels.Settings
            //    });
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected override void InitCommands()
        {
            SelectDirectoryCommand = new DelegateCommand(SelectOutPutDirectoryCommand, () => true);
            PreviewStandadFeedbackFormCommand = new DelegateCommand(ExecutePreviewStandardFeedbackFormCommand);
            WebsitePreviewCommand = new DelegateCommand(ExecuteWebsitePreviewCommand);
            AddFeedBackTopic = new DelegateCommand<string>(ExecuteAddFeedbackTopicCommand);
            RemoveFeedBackTopic = new DelegateCommand<string>(ExecuteRemoveFeedbackTopicCommand);
            SelectImageFileCommand = new DelegateCommand<string>(ExecuteSelectImageFileCommand);
            WarningButtonCommand = new DelegateCommand(ExecuteWarningCommand);
            AdvancedOptionsViewCommand = new DelegateCommand(ExecuteAdvancedOptionsViewCommand);
            CloseWebsitePreviewCommand = new DelegateCommand(ExecuteCloseWebsitePreviewCommand);
            CloseThemePreviewPopup = new DelegateCommand(CloseThemePreview);
            CloseHospitalSelectionPopup = new DelegateCommand(CloseHospitalSelection);
            CloseHospitalSelectionCommand = new DelegateCommand<string>(OnCloseHopitalSelection);
            ClosePreviewImageCommand = new DelegateCommand<string>(
                (param) =>
                    {
                        IsPreviewingBannerImage = false;
                        IsPreviewingLogoImage = false;
                        IsAdvancedOptionsImageVisible = false;
                    });
            PreviewImageFileCommand = new DelegateCommand<string>(
                param =>
                    {
                        if (!string.IsNullOrEmpty(param) && param == "BannerImage") OpenPopup(PopupView.BannerImagePreview);
                        else if (!string.IsNullOrEmpty(param) && param == "LogoImage") OpenPopup(PopupView.LogoImagePreview);
                    },
                param => !string.IsNullOrEmpty(param));

            PreviewThemeCommand = new DelegateCommand<string>(ExecutePreviewThemeCommand);
            RadiusSelectionCommand = new DelegateCommand<string>(OnRadiusSelection);
            OpenHospitalSelectionCommand = new DelegateCommand<object>(OnOpenHopitalSelection);
            HospitalSelectionCompleteCommand = new DelegateCommand(OnHopitalSelectionComplete);
            //FilterHospitalSelectionCommand = new DelegateCommand<HospitalFilterByEnum>(OnFilterHospitals, (o) => HospitalsCollectionView != null && HospitalsCollectionView.OfType<HospitalViewModel>().Any());
            OpenNursingHomeSelectionCommand = new DelegateCommand(OnNursingHomeSelection);
            NursingHomeSelectionCompleteCommand = new DelegateCommand<string>(OnNursingHomeSelectionComplete);
            SaveMenuItemCommand = new DelegateCommand<MenuItem>(OnSaveMenuEdit, CanSaveMenuItem);
            CancelMenuEditCommand = new DelegateCommand<MenuItem>(OnCancelMenuEdit);
            ViewWithBrowserCommand = new DelegateCommand(new Action(() => { Process.Start(WebBrowserSourceUrl); }));
            ZipcodeListBoxItemsSelectionChangedCommand =
                new DelegateCommand<object>(OnZipCodeRadaiiListBoxSelectionChanged);
        }

        private void OnZipCodeRadaiiListBoxSelectionChanged(object selectedItems)
        {
            IsAllRadiusSelected = ApplicableRadiusMiles.All(items => items.IsSelected);
            SetRadiusStateSelectionButtonContent(IsAllRadiusSelected);
        }

        private void ExecuteAdvancedOptionsViewCommand()
        {
            OpenPopup(PopupView.AdvancedOptionsView);
        }

        private void OpenPopup(PopupView view)
        {
            IsWebsitePreviewOpen = false;
            IsPreviewingBannerImage = false;
            IsPreviewingLogoImage = false;
            ShowHospitalSelectionView = false;
            ShowNursingSelectionView = false;
            OpenThemePreviewPopup = false;
            IsAdvancedOptionsImageVisible = false;

            switch (view)
            {
                case PopupView.WebsitePreview:
                    IsWebsitePreviewOpen = true;
                    break;
                case PopupView.BannerImagePreview:
                    IsPreviewingBannerImage = true;
                    break;
                case PopupView.LogoImagePreview:
                    IsPreviewingLogoImage = true;
                    break;
                case PopupView.HospitalView:
                    ShowHospitalSelectionView = true;
                    break;
                case PopupView.NursingHomeView:
                    ShowNursingSelectionView = true;
                    break;
                case PopupView.ThemePreview:
                    OpenThemePreviewPopup = true;
                    break;
                case PopupView.AdvancedOptionsView:
                    IsAdvancedOptionsImageVisible = true;
                    break;
                default:
                    break;
            }
        }

        private void CloseHospitalSelection()
        {
            ShowHospitalSelectionView = false;
        }

        private void CloseThemePreview()
        {
            OpenThemePreviewPopup = false;
        }

        private void OnNursingHomeSelectionComplete(string isDefaultAction)
        {
            var defaultVaue = !string.IsNullOrEmpty(isDefaultAction) && Convert.ToBoolean(isDefaultAction);

            CurrentWebsite.NursingHomes.Clear();

            if (NursingHomeCollectionView == null
                || !NursingHomeCollectionView.OfType<NursingHome>().Any(h => h.IsSelected))
            {
                ShowNursingSelectionView = false;
                return;
            }

            foreach (
                var nursingHome in NursingHomeCollectionView.OfType<NursingHome>().Where(h => h.IsSelected).ToList()) CurrentWebsite.NursingHomes.Add(new WebsiteNursingHome { NursingHome = nursingHome });

            if (!defaultVaue)
            {
                var message = string.Format(
                    "{0} nursing homes selected for website \"{1}\".",
                    CurrentWebsite.NursingHomes.Count,
                    CurrentWebsite.Name);
                EventAggregator.GetEvent<GenericNotificationEvent>().Publish(message);
            }

            ShowNursingSelectionView = false;
        }

        private void OnNursingHomeSelection()
        {
            PopulateNursingHomes();
            OpenPopup(PopupView.NursingHomeView);
        }

        public Task<int> PopulateNursingHomes()
        {
            var selectedStates = ManageViewModel.WebsiteViewModel.Website.StateContext.ToList();

            var nursingHomes = new List<NursingHome>();
            if (selectedStates.Any())
            {
                nursingHomes = WebsiteDataService.GetNursingHomesForWebsite(selectedStates.ToArray());
                NursingHomeCollectionView = new MultiSelectCollectionView<NursingHome>(nursingHomes);

                ListExtensions.ForEach(
                    nursingHomes,
                    nh =>
                        {
                            nh.IsValueChanged += NursingHome_IsValueChanged;
                        });

                if (CurrentWebsite.NursingHomes != null && CurrentWebsite.NursingHomes.Any())
                {
                    CurrentWebsite.NursingHomes = CurrentWebsite.NursingHomes.RemoveNullValues();
                    //var nursingHomeList = NursingHomeCollectionView.OfType<NursingHome>().ToList();
                    if (NursingHomeCollectionView.Count == CurrentWebsite.NursingHomes.Count)
                    {
                        IsAllNursingHomesSelected = false;
                        IsAllNursingHomesSelected = NursingHomeCollectionView.Count == CurrentWebsite.NursingHomes.Count;
                        return Task.FromResult(1);
                    }
                    ListExtensions.ForEach(NursingHomeCollectionView.OfType<NursingHome>(), x => x.IsSelected = false);
                    foreach (var nursingHome in NursingHomeCollectionView.OfType<NursingHome>().ToList())
                    {
                        var wh =
                            CurrentWebsite.NursingHomes.DistinctBy(x => x.Id)
                                .SingleOrDefault(h => h.NursingHome != null && h.NursingHome.Id == nursingHome.Id);
                        nursingHome.IsSelected = wh != null;
                    }
                }
                else
                {
                    IsAllNursingHomesSelected = false;
                    IsAllNursingHomesSelected = true;
                }
            }

            NursingHomeCollectionView = new MultiSelectCollectionView<NursingHome>(nursingHomes);

            //((System.Collections.Specialized.INotifyCollectionChanged)NursingHomeCollectionView).CollectionChanged += (s, e) =>
            //{
            //    //this. = HospitalsCollectionView.OfType<WebsiteMeasure>().Count(item => item.IsSelected);
            //    RaisePropertyChanged(() => IsAllNursingHomesSelected);
            //};

            return Task.FromResult(1);

        }

        private void NursingHome_IsValueChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => IsAllNursingHomesSelected);
        }

        /// <summary>
        /// Called when [filter hospitals].
        /// </summary>
        /// <param name="obj">The object.</param>
        private void OnFilterHospitals(HospitalFilterByEnum obj)
        {
            switch (obj)
            {
                case HospitalFilterByEnum.Mapped_Local_And_CMS_Provider_ID:
                    HospitalsCollectionView.Filter = BothLocalAndCmsIdsExists;
                    break;
                case HospitalFilterByEnum.CMS_IDs_Only:
                    HospitalsCollectionView.Filter = OnlyCmsIdExists;
                    break;
                case HospitalFilterByEnum.Local_Hospital_IDs_Only:
                    HospitalsCollectionView.Filter = OnlyLocalIdExists;
                    break;
                default:
                    HospitalsCollectionView.Filter = null;
                    HospitalsCollectionView.Refresh();
                    break;
            }
            HospitalsCollectionView.MoveCurrentToFirst();
        }

        /// <summary>
        /// Bothes the local and CMS ids exists.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private bool BothLocalAndCmsIdsExists(object obj)
        {
            var hospital = obj as HospitalViewModel;

            if (hospital != null)
            {
                return !string.IsNullOrEmpty(hospital.Hospital.LocalHospitalId)
                       && !string.IsNullOrEmpty(hospital.Hospital.CmsProviderID);
            }
            return false;

        }

        /// <summary>
        /// Called when [local identifier exists].
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private bool OnlyLocalIdExists(object obj)
        {
            var hospital = obj as HospitalViewModel;

            return hospital != null
                   && (!string.IsNullOrEmpty(hospital.Hospital.LocalHospitalId)
                       && string.IsNullOrEmpty(hospital.Hospital.CmsProviderID));
        }

        /// <summary>
        /// Called when [CMS identifier exists].
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private bool OnlyCmsIdExists(object obj)
        {
            var hospital = obj as HospitalViewModel;

            return hospital != null
                   && (string.IsNullOrEmpty(hospital.Hospital.LocalHospitalId)
                       && !string.IsNullOrEmpty(hospital.Hospital.CmsProviderID));
        }

        private void OnOpenHopitalSelection(object obj)
        {
            _loadingHospitalSelection = true;

            PopulateHospitals();

            _loadingHospitalSelection = false;

            OpenPopup(PopupView.HospitalView);
        }

        private void PopulateHospitals()
        {
            var selectedStates = CurrentWebsite.StateContext.ToList();

            if (selectedStates.Any())
            {
                var hospitals =
                    WebsiteDataService.GetHospitalsForWebsite(selectedStates.ToArray()).ToObservableCollection();
                HospitalsCollectionView = new MultiSelectCollectionView<HospitalViewModel>(hospitals);

                if (ManageViewModel.WebsiteViewModel.Website.Hospitals != null
                    && ManageViewModel.WebsiteViewModel.Website.Hospitals.Any())
                {
                    ManageViewModel.WebsiteViewModel.Website.Hospitals =
                        ManageViewModel.WebsiteViewModel.Website.Hospitals.RemoveNullValues();
                    //var hospitalList = HospitalsCollectionView.OfType<HospitalViewModel>().ToList();
                    foreach (var hospital in HospitalsCollectionView.OfType<HospitalViewModel>().ToList())
                    {
                        var wh =
                            ManageViewModel.WebsiteViewModel.Website.Hospitals.DistinctBy(h => h.Hospital.Id)
                                .SingleOrDefault(h => h.Hospital != null && h.Hospital.Id == hospital.Hospital.Id);

                        hospital.IsSelected = wh != null;

                        if ((wh != null && !string.IsNullOrEmpty(wh.CCR) && wh.CCR != "0")) hospital.CCR = wh.CCR;
                        else
                        {
                            if (string.IsNullOrEmpty(hospital.CCR) || hospital.CCR == "0")
                                hospital.CCR = GetCCRForHospitals(
                                    hospital.Hospital.CmsProviderID,
                                    ManageViewModel.WebsiteViewModel.Website.ReportedYear);
                        }
                    }
                    // IsAllHospitalsSelected = HospitalsCollectionView.Count == hospitals.Count;
                }
                else
                {
                    foreach (var hospital in HospitalsCollectionView.OfType<HospitalViewModel>().ToList())
                    {
                        hospital.IsSelected = true;
                        if (hospital.Hospital.CCR.HasValue) hospital.CCR = hospital.Hospital.CCR.Value.ToString(CultureInfo.InvariantCulture);
                        else if (string.IsNullOrEmpty(hospital.CCR))
                            hospital.CCR = GetCCRForHospitals(
                                hospital.Hospital.CmsProviderID,
                                CurrentWebsite.ReportedYear);
                    }
                    //IsAllHospitalsSelected = true;
                }
            }
            else
            {
                HospitalsCollectionView =
                    new MultiSelectCollectionView<HospitalViewModel>(new ObservableCollection<HospitalViewModel>());
                IsAllHospitalsSelected = false;
            }

            ListExtensions.ForEach(
                HospitalsCollectionView.OfType<HospitalViewModel>(),
                hosp =>
                    {
                        hosp.IsValueChanged += (sender, args) => { RaisePropertyChanged(() => IsAllHospitalsSelected); };
                    });
        }

        /// <summary>
        /// Gets the CCR for hospitals.
        /// </summary>
        /// <param name="cmsProviderId">The CMS provider identifier.</param>
        /// <param name="reportingYr">The reporting yr.</param>
        /// <returns></returns>
        private string GetCCRForHospitals(string cmsProviderId, string reportingYr)
        {
            return WebsiteDataService.GetCCRForHospital(cmsProviderId, reportingYr);
        }

        /// <summary>
        /// Called when [hopital selection complete].
        /// </summary>
        private void OnHopitalSelectionComplete()
        {
            ManageViewModel.WebsiteViewModel.Website.Hospitals.Clear();

            if (HospitalsCollectionView == null
                || !HospitalsCollectionView.OfType<HospitalViewModel>().Any(h => h.IsSelected))
            {
                ShowHospitalSelectionView = false;
                return;
            }

            foreach (
                var hospital in HospitalsCollectionView.OfType<HospitalViewModel>().Where(h => h.IsSelected).ToList())
            {
                ManageViewModel.WebsiteViewModel.Website.Hospitals.Add(
                    new WebsiteHospital { Hospital = hospital.Hospital, CCR = hospital.CCR });
            }
            ManageViewModel.WebsiteViewModel.Website.HospitalsChangedWarning = false;

            RaisePropertyChanged(() => CurrentWebsite);
            RaisePropertyChanged(() => CurrentWebsite.Hospitals);
            RaisePropertyChanged(() => ManageViewModel.WebsiteViewModel);
            RaisePropertyChanged(() => ManageViewModel.WebsiteViewModel.Website);
            RaisePropertyChanged(() => ManageViewModel.WebsiteViewModel.Website.Hospitals);
            RaisePropertyChanged(() => ManageViewModel.WebsiteViewModel.Website.HospitalsChangedWarning);

            var message = string.Format(
                "{0} hospitals selected for website \"{1}\".",
                base.ManageViewModel.WebsiteViewModel.Website.Hospitals.Count,
                base.ManageViewModel.WebsiteViewModel.Website.Name);
            EventAggregator.GetEvent<GenericNotificationEvent>().Publish(message);

            ShowHospitalSelectionView = false;
        }

        private void OnCloseHopitalSelection(string obj)
        {
            if (string.IsNullOrEmpty(obj)) return;

            switch (obj)
            {
                case "HospitalSelection":
                    ShowHospitalSelectionView = false;
                    break;
                case "NursingHomeSelection":
                    ShowNursingSelectionView = false;
                    break;
            }
        }

        private void ExecutePreviewThemeCommand(string selectedTheme)
        {
            OpenPopup(PopupView.ThemePreview);
        }

        // Per jason this is NOT right !! this is more of hack !! He prefers to drive the colorpicker , and add a command property to it 
        // marked for refactoring 
        private void ExecuteWarningCommand()
        {
            const string msg =
                "MONAHRQ recommends you use one of the preset themes, "
                + "but you can choose to change colors to fit your brand. When changing color scheme, "
                + "please ensure they adhere to the 508 accessibility compliance policies.  "
                + "Are you sure you want to proceed?";

            ShowWarningButtonOverlays = MessageBox.Show(
                msg,
                @"Warning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.No;

            Dispatcher.CurrentDispatcher.BeginInvoke(
                new Action(() => RaisePropertyChanged(() => ShowWarningButtonOverlays)));
        }

        /// <summary>
        /// Initializes the properties.
        /// </summary>
        protected override void InitProperties()
        {
            ShowHospitalSelectionView = false;
            ShowNursingSelectionView = false;

            _loadingHospitalSelection = true;
            FilterBy = new ObservableCollection<SelectListItem>
                           {
                               _defaultFilterItem,
                               new SelectListItem
                                   {
                                       Model =
                                           HospitalFilterByEnum
                                           .Mapped_Local_And_CMS_Provider_ID,
                                       Text =
                                           HospitalFilterByEnum
                                           .Mapped_Local_And_CMS_Provider_ID
                                           .GetDescription(),
                                       Value =
                                           (int)
                                           HospitalFilterByEnum
                                               .Mapped_Local_And_CMS_Provider_ID
                                   },
                               new SelectListItem
                                   {
                                       Model =
                                           HospitalFilterByEnum
                                           .Local_Hospital_IDs_Only,
                                       Text =
                                           HospitalFilterByEnum
                                           .Local_Hospital_IDs_Only
                                           .GetDescription(),
                                       Value =
                                           (int)
                                           HospitalFilterByEnum
                                               .Local_Hospital_IDs_Only
                                   },
                               new SelectListItem
                                   {
                                       Model =
                                           HospitalFilterByEnum
                                           .CMS_IDs_Only,
                                       Text =
                                           HospitalFilterByEnum
                                           .CMS_IDs_Only
                                           .GetDescription(),
                                       Value =
                                           (int)
                                           HospitalFilterByEnum
                                               .CMS_IDs_Only
                                   }
                           };

            SelectedHospitalFilter = _defaultFilterItem;
            _loadingHospitalSelection = false;

            NursingHomeFilterBy = EnumExtensions.GetEnumDescriptions<NursingHomeFilterByEnum>();

            LoadData();


            base.InitProperties();
        }


        /// <summary>
        /// Executes the add feedback topic command.
        /// </summary>
        /// <param name="feedbackTopic">The feedback topic.</param>
        private void ExecuteAddFeedbackTopicCommand(string feedbackTopic)
        {
            if (string.IsNullOrEmpty(feedbackTopic)) return;

            if (FeedbackTopics != null)
            {
                FeedbackTopics.Add(feedbackTopic);
            }
        }

        /// <summary>
        /// Executes the remove feedback topic command.
        /// </summary>
        /// <param name="feedbackTopic">The feedback topic.</param>
        private void ExecuteRemoveFeedbackTopicCommand(string feedbackTopic)
        {
            if (string.IsNullOrEmpty(feedbackTopic)) return;

            if (FeedbackTopics != null)
            {
                FeedbackTopics.Remove(feedbackTopic);
            }
        }

        /// <summary>
        /// Selects the out put directory command.
        /// </summary>
        private void SelectOutPutDirectoryCommand()
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.RootFolder = Environment.SpecialFolder.Desktop;
                dlg.ShowNewFolderButton = true;
                dlg.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // Get the selected file name and display in a TextBox
                if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedPath)) OutputDirectoryPath = dlg.SelectedPath;
            }

        }

        private void ExecuteWebsitePreviewCommand()
        {
            //var websitePreviewTask = new System.Threading.Thread(delegate()
            //{
            try
            {
                UpdateSettingsProperties();
                EventAggregator.GetEvent<DisableNavigationEvent>()
                    .Publish(new DisableNavigationEvent { DisableUIElements = true });
                Generator.Publish(ManageViewModel.WebsiteViewModel.Website, PublishTask.PreviewOnly);
                EventAggregator.GetEvent<DisableNavigationEvent>()
                    .Publish(new DisableNavigationEvent { DisableUIElements = false });
                OpenPopup(PopupView.WebsitePreview);
            }

            catch (ThreadAbortException exc)
            {
                //Thread.ResetAbort();
                Logger.Write(exc);
                Logger.Write(exc);
            }
            catch (Exception exc)
            {
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new DispatcherOperationCallback(
                        delegate
                            {
                                Cursor.Current = Cursors.Arrow;
                                return null;
                            }),
                    null);
            }

            //});

            Cursor.Current = Cursors.WaitCursor;
            //websitePreviewTask.Start();
            // Events.GetEvent<StartingWebsitePublishingEvent>().Publish(new StartingWebsitePublishingEvent() { Thread = websitePreviewTask });
        }

        private void ExecuteCloseWebsitePreviewCommand()
        {
            IsWebsitePreviewOpen = false;
        }

        private void ExecutePreviewStandardFeedbackFormCommand()
        {
            try
            {
                var filePath = String.Format(
                    @"{0}\Resources\Feedback\endusersurvey.docx",
                    new FileInfo(System.Windows.Forms.Application.ExecutablePath).Directory.FullName);
                Process proc = new Process();
                proc.StartInfo = new ProcessStartInfo() { FileName = filePath };
                proc.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(
                        "There was a problem opening up the file. Please see your administrator : {Error }",
                        ex.Message),
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        //private void ValidateUrl(string p, string value)
        //{
        //    ClearErrors(p);
        //    if (string.IsNullOrWhiteSpace(CustomFeedbackFormUrl) && string.IsNullOrWhiteSpace(CustomFeedbackFormUrl)) return;

        //    if (string.IsNullOrWhiteSpace(value))
        //    {
        //        SetError(p, "Please provide a URL.");
        //        return;
        //    }
        //    // user MUST type http: or https:
        //    var regexUrl = new Regex(@"^(http|https)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$");

        //    if (!regexUrl.IsMatch(value))
        //    {
        //        SetError(p, ValidUrlMessage);
        //    }
        //}

        /// <summary>
        /// Executes the select image file command.
        /// </summary>
        /// <param name="fileType">Type of the file.</param>
        private void ExecuteSelectImageFileCommand(string fileType)
        {
            var dlg = new OpenFileDialog
                          {
                              InitialDirectory =
                                  Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                              Filter = "Images (*.JPG;*.JPEG;*.PNG)|*.JPG;*.JPEG;*.PNG",
                              FilterIndex = 0
                          };

            // open in the same folder as last time, or the user's document folder by default

            // Set filter and file extension for the document types we can read
            //"All Files (*.*)|*.*|All Files (*.png)|*.png|PNG Files (*.gif)|*.gif|GIF Files (*.jpg)|*.jpg|JPEG Files";

            if (dlg.ShowDialog() == true)
            {
                if (fileType.EqualsIgnoreCase("logo"))
                {
                    LogoImage = new WebsiteImage
                                    {
                                        ImagePath = dlg.FileName,
                                        Image = File.ReadAllBytes(dlg.FileName),
                                        MemeType = dlg.FileName.SubStrAfterLast(".")
                                    };
                    PreviewImageFileCommand.RaiseCanExecuteChanged();

                }
                else if (fileType.EqualsIgnoreCase("banner"))
                {
                    SetBannerImage(dlg.FileName, SelectedBanner.Name);

                    PreviewImageFileCommand.RaiseCanExecuteChanged();
                }
                else if (fileType.EqualsIgnoreCase("homepage"))
                {
                    HomepageContentImage = new WebsiteImage
                                               {
                                                   ImagePath = dlg.FileName,
                                                   Image = File.ReadAllBytes(dlg.FileName),
                                                   MemeType = dlg.FileName.SubStrAfterLast(".")
                                               };
                    PreviewImageFileCommand.RaiseCanExecuteChanged();

                }
            }
        }

        private void SetBannerImage(string fileName, string bannerName, WebsiteImage websiteImage = null)
        {
            IsUserSelectedBanner = !string.IsNullOrEmpty(bannerName)
                                   && USER_SELECTED_BANNER_NAME.ToLower().Equals(bannerName.ToLower());

            if ((websiteImage != null && string.IsNullOrEmpty(websiteImage.ImagePath)) || string.IsNullOrEmpty(fileName))
            {
                BannerImage = CurrentWebsite.IsPersisted && SelectedBanner != null
                              && !string.IsNullOrEmpty(SelectedBanner.Name)
                              && !SelectedBanner.Name.EqualsIgnoreCase("Custom Image")
                                  ? CurrentWebsite.BannerImage
                                  : null;
                Validate();
                return;
            }

            if (websiteImage == null && !string.IsNullOrEmpty(fileName))
            {
                BannerImage = new WebsiteImage
                                  {
                                      ImagePath = fileName,
                                      Image = File.ReadAllBytes(fileName),
                                      MemeType = fileName.SubStrAfterLast("."),
                                      Name = bannerName
                                  };
            }
            else if (websiteImage != null)
            {
                // var imageFilePath = websiteImage.ImagePath;
                if (!string.IsNullOrEmpty(websiteImage.ImagePath) && websiteImage.Image != null)
                {
                    if (!File.Exists(websiteImage.ImagePath))
                    {
                        websiteImage.ImagePath = string.Format(
                            "{0}\\{1}",
                            MonahrqContext.MyDocumentsApplicationDirPath,
                            websiteImage.ImagePath.SubStrAfterLast("\\"));

                        Task.Run(
                            () =>
                                {
                                    File.WriteAllBytes(websiteImage.ImagePath, websiteImage.Image);
                                });
                    }
                }

                BannerImage = new WebsiteImage
                                  {
                                      ImagePath = websiteImage.ImagePath,
                                      Image = websiteImage.Image ?? File.ReadAllBytes(websiteImage.ImagePath),
                                      MemeType =
                                          !string.IsNullOrEmpty(websiteImage.MemeType)
                                              ? websiteImage.MemeType
                                              : websiteImage.ImagePath.SubStrAfterLast("."),
                                      Name = bannerName
                                  };
            }

            Validate();
        }


        public override void Refresh()
        {
            _isInitialLoad = true;
            base.Refresh();
            IsTabVisited = true;

            UpdateSettingsVmProperties();
            RaisePropertyChanged(() => CurrentWebsite.GeographicDescription);
            _isInitialLoad = false;
            ValidateAllRequiredFields();

            if (CurrentWebsite == null) return;

            CurrentWebsite.PropertyChanged -= (o, e) => ValidateAllRequiredFields();
            CurrentWebsite.PropertyChanged += (o, e) => ValidateAllRequiredFields();
        }

        /// <summary>
        /// Updates the settings vm properties.
        /// </summary>
        private void UpdateSettingsVmProperties()
        {
            IsWebsitePreviewOpen = false;

            CurrentWebsite.BrowserTitle = CurrentWebsite.BrowserTitle ?? "MONAHRQ Website";
            SelectZipRadii();
            FeedbackTopics = CurrentWebsite.FeedbackTopics.ToObservableCollection();
            FeedbackTopics.CollectionChanged -= FeedBackTopicCollectionChanged;
            FeedbackTopics.CollectionChanged += FeedBackTopicCollectionChanged;
            CurrentWebsite.Keywords = CurrentWebsite.Keywords
                                      ?? @"Hospital, Hospital Reports, Hospital Quality, and Hospital Costs";


            OutputDirectoryPath = GetOutputFolder();

            RaisePropertyChanged(() => OutputDirectoryPath);
            HomepageContentImage = CurrentWebsite.HomepageContentImage ?? new WebsiteImage();
            NumberOfAvaliableHospitals = GetSelectableHospitalCount();
            EnableHospitalSectionLink = CurrentWebsite.StateContext != null && CurrentWebsite.StateContext.Any();
            CustomFeedbackFormUrl = CurrentWebsite.CustomFeedbackFormUrl;
            FeedBackEmailTemp = CurrentWebsite.FeedBackEmail;


            ConsumerWebsiteMenuItems = null;
            ProfessionalWebsiteMenuItems = null;

            if (CurrentWebsite.HasProfessionalsAudience)
            {
                var menuItems = GetWebsiteMenuItems(Audience.Professionals);
                ListExtensions.ForEach(
                    menuItems,
                    x =>
                        {
                            AddLabelChangedListner(x);
                            ListExtensions.ForEach(x.SubMenus, this.AddLabelChangedListner);
                        });

                ProfessionalWebsiteMenuItems = new ListCollectionView(menuItems);
            }

            if (CurrentWebsite.HasConsumersAudience)
            {
                var menuItems = GetWebsiteMenuItems(Audience.Consumers);
                ListExtensions.ForEach(
                    menuItems,
                    x =>
                        {
                            AddLabelChangedListner(x);
                            ListExtensions.ForEach(x.SubMenus, this.AddLabelChangedListner);
                        });

                ConsumerWebsiteMenuItems = new ListCollectionView(menuItems);
            }

            var professionalTheme = GetWebsiteTheme(Audience.Professionals);
            var consumerTheme = GetWebsiteTheme(Audience.Consumers);
            SelectedTheme = professionalTheme.SelectedTheme;
            AccentColor = professionalTheme.AccentColor;
            BrandColor = professionalTheme.BrandColor;
            SelectedFont = professionalTheme.SelectedFont;
            ConsumerBrandColor = consumerTheme.Brand2Color;

			//	If using old Default Theme/ switch to new theme.
			if (SelectedTheme.EqualsIgnoreCase("Default (Blue)") &&
				AccentColor.EqualsIgnoreCase("#f8e098") &&
				BrandColor.EqualsIgnoreCase("#a0d2ec"))
			{
				AccentColor = ConfigurationService.MonahrqSettings.Themes.DefaultTheme.AccentColor;
				BrandColor = ConfigurationService.MonahrqSettings.Themes.DefaultTheme.BrandColor;
				ConsumerBrandColor = ConfigurationService.MonahrqSettings.Themes.DefaultTheme.Brand2Color;
			}

            if (CurrentWebsite.LogoImage != null)
            {
                if (CurrentWebsite.LogoImage.Image != null && !string.IsNullOrEmpty(CurrentWebsite.LogoImage.Name)
                    && !File.Exists(Path.Combine(CurrentWebsite.LogoImage.ImagePath, CurrentWebsite.LogoImage.Name)))
                {
                    CurrentWebsite.LogoImage.ImagePath = string.Format(
                        "{0}\\{1}",
                        MonahrqContext.MyDocumentsApplicationDirPath,
                        CurrentWebsite.LogoImage.ImagePath.SubStrAfterLast("\\"));

                    Task.Run(
                        () =>
                            {
                                File.WriteAllBytes(CurrentWebsite.LogoImage.ImagePath, CurrentWebsite.LogoImage.Image);
                            });
                }
                LogoImage = CurrentWebsite.LogoImage;
            }
            else
            {
                LogoImage = new WebsiteImage();
            }

            if (CurrentWebsite.BannerImage == null)
            {
                if (!CurrentWebsite.IsPersisted || SelectedBanner == null) SelectedBanner = DefaultBanner;
                return;
            }

            var installedBanners = ConfigService.MonahrqSettings.Banners.OfType<MonahrqBannerElement>().ToList();
            var bannerName = !string.IsNullOrEmpty(CurrentWebsite.BannerImage.Name)
                                 ? CurrentWebsite.BannerImage.Name
                                 : USER_SELECTED_BANNER_NAME;
            var websiteBanner = installedBanners.FirstOrDefault(x => x.Name.ToLower().Equals(bannerName.ToLower()));
            SelectedBanner = websiteBanner;
            if (bannerName != USER_SELECTED_BANNER_NAME) return;

            SetBannerImage(CurrentWebsite.BannerImage.ImagePath, USER_SELECTED_BANNER_NAME, CurrentWebsite.BannerImage);
        }

        private WebsiteTheme GetWebsiteTheme(Audience websiteAudience)
        {
            return CurrentWebsite.Themes.FirstOrDefault(x => x.AudienceType == websiteAudience)
                   ?? new WebsiteTheme
                          {
                              SelectedFont = "'Droid Sans', Arial, sans-serif",
                              AudienceType = websiteAudience,
                              AccentColor =
                                  CurrentWebsite.IsPersisted
                                      ? AccentColor
                                      : ConfigurationService.MonahrqSettings.Themes.DefaultTheme.AccentColor,
                              BrandColor =
                                  CurrentWebsite.IsPersisted
                                      ? BrandColor
                                      : ConfigurationService.MonahrqSettings.Themes.DefaultTheme.BrandColor,
                              SelectedTheme =
                                  CurrentWebsite.IsPersisted
                                      ? SelectedTheme
                                      : ApplicableSiteThemes.FirstOrDefault(),
                              BackgroundColor =
                                  ConfigurationService.MonahrqSettings.Themes.DefaultTheme.BackgroundColor,
                              BodyTextColor =
                                  ConfigurationService.MonahrqSettings.Themes.DefaultTheme.BodyTextColor,
                              LinkTextColor =
                                  ConfigurationService.MonahrqSettings.Themes.DefaultTheme.LinkTextColor,
                              Brand2Color =
                                  ConfigurationService.MonahrqSettings.Themes.DefaultTheme.Brand2Color
                          };
        }

        private void FeedBackTopicCollectionChanged(
            object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Validate();
        }

        private int GetSelectableHospitalCount()
        {
            if (ManageViewModel.WebsiteViewModel.Website.StateContext == null
                || !ManageViewModel.WebsiteViewModel.Website.StateContext.Any()) return 0;

            var selectedStates = ManageViewModel.WebsiteViewModel.Website.StateContext.ToList();


            //if (ApplicableReportingStates.Any(s => s.IsSelected))
            //{
            //    selectedStates = ApplicableReportingStates.Where(item => item.IsSelected)
            //                                              .Select(reportingState => reportingState.Value.ToString())
            //                                              .ToList();
            //}
            //else
            //{
            //    selectedStates = ApplicableReportingStates.Select(reportingState => reportingState.Value.ToString())
            //                                              .ToList();
            //}

            if (selectedStates.Any())
            {
                return WebsiteDataService.GetHospitalsForWebsiteCount(selectedStates.ToArray());
            }

            if (!selectedStates.Any() && ConfigService.HospitalRegion.DefaultStates.Count > 0)
            {
                return
                    WebsiteDataService.GetHospitalsForWebsiteCount(
                        ConfigService.HospitalRegion.DefaultStates.OfType<string>().ToArray());
            }

            return 0;
        }

        private void SelectZipRadii()
        {
            if (CurrentWebsite.SelectedZipCodeRadii != null && CurrentWebsite.SelectedZipCodeRadii.Any())
            {
                foreach (
                    var zipCodeRadii in
                        ApplicableRadiusMiles.OfType<SelectListItem>()
                            .Where(zip => CurrentWebsite.SelectedZipCodeRadii.Contains(int.Parse(zip.Model.ToString())))
                            .ToList())
                {
                    zipCodeRadii.IsSelected = true;
                }
            }
            else
            {
                if (CurrentWebsite == null || CurrentWebsite.SelectedZipCodeRadii.Any()) return;
                foreach (var zipCodeRadii in ApplicableRadiusMiles.ToList())
                {
                    zipCodeRadii.IsSelected = true;
                }
            }
            RaisePropertyChanged(() => ApplicableRadiusMiles);
        }

        /// <summary>
        /// Updates the settings properties.
        /// </summary>
        private void UpdateSettingsProperties()
        {

            CurrentWebsite.SelectedZipCodeRadii.Clear();
            var selectedZipCodeRadii =
                ApplicableRadiusMiles.Where(item => item.IsSelected)
                    .ToList()
                    .Select(reportingZipRadii => int.Parse(reportingZipRadii.Text))
                    .ToList();
            CurrentWebsite.SelectedZipCodeRadii = selectedZipCodeRadii;
            RaisePropertyChanged(() => CurrentWebsite.SelectedZipCodeRadii);

            var selectFeedbackTopics = new List<string>();
            foreach (var feedbackTopic in
                FeedbackTopics.Where(
                    feedbackTopic => !selectFeedbackTopics.Any(ft => ft.EqualsIgnoreCase(feedbackTopic))).ToList()) selectFeedbackTopics.Add(feedbackTopic);

            CurrentWebsite.FeedbackTopics = selectFeedbackTopics;
            RaisePropertyChanged(() => CurrentWebsite.FeedbackTopics);

            CurrentWebsite.OutPutDirectory = OutputDirectoryPath;
            RaisePropertyChanged(() => CurrentWebsite.OutPutDirectory);

            var professionalTheme = GetWebsiteTheme(Audience.Professionals);
            var consumerTheme = GetWebsiteTheme(Audience.Consumers);

            if (!professionalTheme.IsPersisted && CurrentWebsite.HasProfessionalsAudience) CurrentWebsite.Themes.Add(professionalTheme);
            if (!consumerTheme.IsPersisted && CurrentWebsite.HasConsumersAudience) CurrentWebsite.Themes.Add(consumerTheme);

            SetProfessionaWebsiteTheme();

            var theme =
                ConfigService.MonahrqSettings.Themes.OfType<MonahrqThemeElement>()
                    .SingleOrDefault(t => t.Name.EqualsIgnoreCase(SelectedTheme));
            if (theme==null || theme.BrandColor != professionalTheme.BrandColor) ConsumerBrandColor = GetIdealForegroundColor(professionalTheme.BrandColor, 0.15F);

            SetConsumerWebsiteTheme();

            RaisePropertyChanged(() => SelectedFont);
            RaisePropertyChanged(() => SelectedTheme);
            RaisePropertyChanged(() => BrandColor);
            RaisePropertyChanged(() => AccentColor);

            CurrentWebsite.LogoImage = LogoImage;
            RaisePropertyChanged(() => CurrentWebsite.LogoImage);

            CurrentWebsite.BannerImage = BannerImage;
            RaisePropertyChanged(() => CurrentWebsite.BannerImage);
            CurrentWebsite.HomepageContentImage = HomepageContentImage;
            RaisePropertyChanged(() => CurrentWebsite.HomepageContentImage);

            // End Themes related

            CurrentWebsite.Menus.Clear();
            if (CurrentWebsite.HasProfessionalsAudience && ProfessionalWebsiteMenuItems != null)
            {
                CurrentWebsite.Menus.AddRange(
                    GetWebsiteMenuItems(Audience.Professionals, true).Select(m => new WebsiteMenu { Menu = m }).ToList());
            }
            if (ConsumerWebsiteMenuItems != null && CurrentWebsite.HasConsumersAudience)
            {
                CurrentWebsite.Menus.AddRange(
                    GetWebsiteMenuItems(Audience.Consumers, true).Select(m => new WebsiteMenu { Menu = m }).ToList());
            }


            if ((!CurrentWebsite.Hospitals.Any() && CurrentWebsite.StateContext.Any()) ||
                        (CurrentWebsite.HospitalsChangedWarning.HasValue && CurrentWebsite.HospitalsChangedWarning.Value))
            {
                var selectedHospitals = WebsiteDataService.GetHospitalsForWebsite(CurrentWebsite.StateContext.ToArray());

                if (selectedHospitals != null && selectedHospitals.Any())
                {
                    if (CurrentWebsite.HospitalsChangedWarning.HasValue && CurrentWebsite.HospitalsChangedWarning.Value)
                        CurrentWebsite.Hospitals.Clear();

                    foreach (var hospitalViewModel in selectedHospitals)
                    {
                        if (CurrentWebsite.Hospitals.All(wh => wh.Hospital.Id != hospitalViewModel.Hospital.Id))
                        {
                            CurrentWebsite.Hospitals.Add(new WebsiteHospital
                            {
                                Hospital = hospitalViewModel.Hospital,
                                CCR = hospitalViewModel.Hospital.CCR.HasValue
                                                       ? hospitalViewModel.Hospital.CCR.Value.ToString()
                                                       : WebsiteDataService.GetCCRForHospital(hospitalViewModel.Hospital.CmsProviderID, ManageViewModel.WebsiteViewModel.Website.ReportedYear)
                            });
                        }
                        else
                        {
                            var webHosp = CurrentWebsite.Hospitals.FirstOrDefault(wh => wh.Hospital.Id == hospitalViewModel.Hospital.Id);

                            if (webHosp != null && !webHosp.CCR.EqualsIgnoreCase(hospitalViewModel.CCR))
                            {
                                if (string.IsNullOrEmpty(hospitalViewModel.CCR) || hospitalViewModel.CCR == "0")
                                {
                                    var ccr = WebsiteDataService.GetCCRForHospital(hospitalViewModel.Hospital.CmsProviderID, ManageViewModel.WebsiteViewModel.Website.ReportedYear);

                                    webHosp.CCR = hospitalViewModel.Hospital.CCR.HasValue
                                                                    ? hospitalViewModel.Hospital.CCR.Value.ToString()
                                                                    : !string.IsNullOrEmpty(ccr) ? ccr : "0";
                                }
                                else
                                {
                                    webHosp.CCR = hospitalViewModel.CCR;
                                }
                            }
                            //if (webHosp != null && webHosp.CCR.EqualsIgnoreCase(hospitalViewModel.CCR))
                            //     webHosp.CCR = hospitalViewModel.CCR;

                        }
                    }
                }
                CurrentWebsite.HospitalsChangedWarning = false;
            }
            RaiseErrorsChanged(() => CurrentWebsite.Hospitals);
            RaiseErrorsChanged(() => CurrentWebsite);

            if (!CurrentWebsite.NursingHomes.Any() && CurrentWebsite.SelectedReportingStates.Any())
            {
                var selectedNursingHomes = WebsiteDataService.GetNursingHomesForWebsite(CurrentWebsite.SelectedReportingStates.ToArray());
                if (selectedNursingHomes != null && selectedNursingHomes.Any())
                {
                    foreach (var nursingHome in selectedNursingHomes)
                    {
                        CurrentWebsite.NursingHomes.Add(new WebsiteNursingHome
                        {
                            NursingHome = nursingHome,
                        });
                    }
                    //RaiseErrorsChanged(() => CurrentWebsite.NursingHomes);
                }
            }
            RaiseErrorsChanged(() => CurrentWebsite.NursingHomes);
        }

        public bool ShowWarningButtonOverlays
        {
            get { return _showWarningButtonOverlays; }
            set { _showWarningButtonOverlays = value; }
        }

        public void LoadData()
        {
            FeedbackTopics = new ObservableCollection<string>();
            ApplicableRadiusMiles = WebsiteDataService.ApplicableZipCodeRadii.ToObservableCollection();

            foreach (var radius in ApplicableRadiusMiles)
            {
                radius.ValueChanged -= ValidateUserInput;
                radius.ValueChanged += ValidateUserInput;

            }


            ApplicableSiteThemes = WebsiteDataService.ApplicableWebsiteThemes.ToObservableCollection();
            ApplicableSiteFonts = WebsiteDataService.ApplicableSiteFonts.ToObservableCollection();
            AllAvailableMenuItems = WebsiteDataService.GetApplicableMenuItems().ToObservableCollection();
        }

        private void OnRadiusSelection(string radiusSelectionButtonContent)
        {
            if (radiusSelectionButtonContent == "Select All")
            {
                ApplicableRadiusMiles.ToList().ForEach(x => x.IsSelected = true);
                SetRadiusStateSelectionButtonContent(true);
            }
            else if (radiusSelectionButtonContent == "UnSelect All")
            {
                ApplicableRadiusMiles.ToList().ForEach(x => x.IsSelected = false);
                SetRadiusStateSelectionButtonContent(false);
            }
        }

        public override void Reset()
        {
            _isReseting = true;

            base.Reset();

            if (ApplicableRadiusMiles != null)
            {
                ListExtensions.ForEach(ApplicableRadiusMiles, x =>
                {
                    x.IsSelected = false;
                });
            }

            SelectedTheme = ConfigService.MonahrqSettings.Themes.DefaultTheme.Name;

            _isReseting = false;


            //if (ManageViewModel.WebsiteViewModel != null && ManageViewModel.WebsiteViewModel.Website != null)
            //{

            //    CurrentWebsite.SelectedZipCodeRadii.Clear();
            //    ManageViewModel.WebsiteViewModel.Website.SelectedZipCodeRadii.Clear();

            //    RaisePropertyChanged(() => CurrentWebsite.SelectedZipCodeRadii);

            //    ManageViewModel.WebsiteViewModel.Website = null;
            //    RaisePropertyChanged(() => CurrentWebsite);
            //    RaisePropertyChanged(() => ManageViewModel.WebsiteViewModel.Website);
            //}
        }

        public void SetRadiusStateSelectionButtonContent(bool isAllselected)
        {
            RadiusSelectionButtonContent = isAllselected ? "UnSelect All" : "Select All";
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
            if (CurrentWebsite == null) return true;

            CurrentWebsite.Validate();
            Validate();

            return !HasErrors && !CurrentWebsite.HasErrors && !WebsiteMenuHasErrors;

        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            Index = 4;
            EventAggregator.GetEvent<WebsitePublishEvent>().Subscribe(e =>
            {
                if (e.Data.Status != WebsiteGenerationStatus.PreviewComplete || CurrentWebsite == null) return;

                IsWebsitePreviewOpen = true;
                WebBrowserSourceUrl = null;
                WebBrowserSourceUrl = string.Format(@"{0}\index.html", CurrentWebsite.OutPutDirectory);
            });

            //	Create/Init SubListTabViewModels.
            WebsitePagesViewModel.ManageViewModel = ManageViewModel;
            AddSubListTabViewModel(new SubListTabViewModel(WebsitePagesViewModel) { SyncStateActions = true });
        }

        public override bool TabChanged()
        {
            UpdateSettingsProperties();

            ShowWarningButtonOverlays = false;

            return true;
        }

        public static ValidationResult ValidateFeedBackEmail(object value, ValidationContext context)
        {
            var settings = context.ObjectInstance as WebsiteSettingsViewModel ?? new WebsiteSettingsViewModel();
            if (settings.FeedbackTopics.Any() && string.IsNullOrEmpty(settings.FeedBackEmailTemp))
            {
                var result = new ValidationResult("Feedback email address is required.", new List<string> { "FeedBackEmailTemp" });
                return result;
            }

            if (!string.IsNullOrEmpty(settings.FeedBackEmailTemp) && _regex.Match(settings.FeedBackEmailTemp).Length == 0)
            {
                var result = new ValidationResult("Please enter a valid email address.", new List<string> { "FeedBackEmailTemp" });
                return result;
            }

            return ValidationResult.Success;
        }

        public static ValidationResult IsWebsiteRadiusSelected(object value, ValidationContext context)
        {
            var settingsModel = context.ObjectInstance as WebsiteSettingsViewModel ?? new WebsiteSettingsViewModel();
            if (!settingsModel.ApplicableRadiusMiles.Any(r => r.IsSelected))
            {
                var result =
                    new ValidationResult("Please select zip code radii options to be included in your website.",
                        new List<string> { "ApplicableRadiusMiles" });
                return result;
            }
            return ValidationResult.Success;
        }

        public static ValidationResult IsValidOutputFolder(object sender, ValidationContext context)
        {
            var settingsModel = context.ObjectInstance as WebsiteSettingsViewModel ?? new WebsiteSettingsViewModel();
            if (string.IsNullOrEmpty(settingsModel.OutputDirectoryPath)) return ValidationResult.Success;

            var drive = settingsModel.OutputDirectoryPath.Split(new char[] { '\\' }).FirstOrDefault().Select(x => string.IsNullOrEmpty(x.ToString()) ? x.ToString() : x.ToString() + ":").FirstOrDefault();

            if (settingsModel.OutputDirectoryPath.Length > 80)
                return new ValidationResult("Please provide a valid output folder name fewer than 80 characters.", new List<string> { "OutputDirectoryPath" });


            var userEnteredDrive = Path.IsPathRooted(settingsModel.OutputDirectoryPath) ? Directory.GetDirectoryRoot(settingsModel.OutputDirectoryPath) : string.Empty;
            var drivers = Directory.GetLogicalDrives();

            if (string.IsNullOrEmpty(drive) || !drivers.Contains(userEnteredDrive) || settingsModel.OutputDirectoryPath.ToCharArray().ContainsAny(Path.GetInvalidPathChars().ToArray()))
            {
                return new ValidationResult("Please provide a valid output folder name fewer than 80 characters.", new List<string> { "OutputDirectoryPath" });
            }

            return ValidationResult.Success;
        }

        private void SetProfessionaWebsiteTheme()
        {
            var professional = CurrentWebsite.Themes.FirstOrDefault(x => x.AudienceType == Audience.Professionals);
            if (professional == null) return;

            professional.SelectedTheme = SelectedTheme;
            professional.BrandColor = BrandColor;
            professional.AccentColor = AccentColor;
            professional.SelectedFont = SelectedFont;
        }

        private void SetConsumerWebsiteTheme()
        {
            var consumer = CurrentWebsite.Themes.FirstOrDefault(x => x.AudienceType == Audience.Consumers);
            if (consumer == null) return;

            consumer.SelectedTheme = SelectedTheme;
            consumer.AccentColor = AccentColor;
            consumer.BrandColor = BrandColor;
            consumer.Brand2Color = ConsumerBrandColor;
            consumer.SelectedFont = SelectedFont;
        }

        private ObservableCollection<MenuItem> GetWebsiteMenuItems(Audience audience, bool isSave = false)
        {
            var datasetTypes =
                CurrentWebsite.Datasets.DistinctBy(d => d.Dataset.ContentType.Name)
                    .Select(d => d.Dataset.ContentType.Name).ToList();

            var audiences = audience == Audience.Professionals ? "professional" : "consumer";

            if (!datasetTypes.Any())
            {
                var defaultMenus = AllAvailableMenuItems.Where(menu =>
                menu.DataSets.Contains("All") && audiences.EqualsIgnoreCase(menu.Product)).ToList();
                defaultMenus.ForEach(x =>
                        {
                            x.IsSelected = true;
                            x.IsVisible = true;
                        });

                return defaultMenus.ToObservableCollection();
            }

            var allMenuItems = GetAllMenuItemOptions(datasetTypes, audiences);
            allMenuItems.ForEach(item => item.IsVisible = true);
            var unselectedMenuItemNamesList = GetListofUnselectedMenuItems(audience, isSave);

            foreach (var menu in allMenuItems)
                if (unselectedMenuItemNamesList.Contains(menu.Label))
                {
                    menu.IsSelected = false;
                    if (menu.SubMenus.Any()) menu.SubMenus.ForEach(s => s.UnSelectAll());
                }
                else
                {
                    menu.IsSelected = true;
                    if (menu.SubMenus.Any()) menu.SubMenus.ForEach(s => s.SelectAll());
                }

            var result = allMenuItems.Where(x => x.Owner == null).ToList();
            return result.DistinctBy(m => m.Id).OrderBy(x => x.Id).ThenBy(x => x.Product).ToObservableCollection();
        }

        private List<MenuItem> GetAllMenuItemOptions(List<string> datasetTypes, string audiences)
        {
            var availableParentMenuItemsList = AllAvailableMenuItems
                  .Where(
                      menu =>
                      (menu.DataSets.Contains("All") | menu.DataSets.ContainsAny(datasetTypes))
                      && audiences.EqualsIgnoreCase(menu.Product)).ToList();

            var allAvailableMenus = new List<MenuItem>();
            availableParentMenuItemsList.ForEach(p => p.FindAllChildren(ref allAvailableMenus));
            return allAvailableMenus;
        }



        private List<string> GetListofUnselectedMenuItems(Audience audience, bool isSave = false)
        {
            var audiences = audience == Audience.Professionals ? "professional" : "consumer";
            List<MenuItem> parentList = null;

            if (!isSave)
            {
                parentList = CurrentWebsite.Menus.Where(m => audiences.EqualsIgnoreCase(m.Menu.Product) && !m.Menu.IsSelected)
                    .Select(m => m.Menu)
                    .ToList();
            }
            if (isSave)
            {
                if (audiences.EqualsIgnoreCase("professional"))
                    parentList = ProfessionalWebsiteMenuItems != null
                         ? ProfessionalWebsiteMenuItems.OfType<MenuItem>()
                               .Where(m => audiences.EqualsIgnoreCase(m.Product) && !m.IsSelected)
                               .ToList() : new List<MenuItem>();

                if (audiences.EqualsIgnoreCase("consumer"))
                    parentList = ConsumerWebsiteMenuItems != null
                         ? ConsumerWebsiteMenuItems.OfType<MenuItem>()
                               .Where(m => audiences.EqualsIgnoreCase(m.Product) && !m.IsSelected)
                               .ToList() : new List<MenuItem>();
            }

            var result = new List<MenuItem>();
            if (parentList != null && parentList.Any()) parentList.ForEach(p => p.FindAllChildren(ref result));

            return result.Select(r => r.Label).ToList();
        }

        private void OnCancelMenuEdit(MenuItem obj)
        {
            obj.NewLabel = obj.Label;
        }

        private void OnSaveMenuEdit(MenuItem obj)
        {
            obj.Validate();
            if (obj.HasErrorExcluding(new List<string> { "Label" }))
            {
                if (string.IsNullOrEmpty(obj.NewLabel))
                {
                    MessageBox.Show("Please provide text as menu items cannot be blank.", "Error", MessageBoxButton.OK);
                    obj.NewLabel = obj.Label;
                }
                if (obj.NewLabel.Length >= 30)
                {
                    WebsiteMenuHasErrors = true;
                    obj.Label = obj.NewLabel;
                }

                obj.Validate();
                return;
            }

            if (CurrentWebsite.Menus != null)
            {
                ListExtensions.ForEach(CurrentWebsite.Menus, x => x.Menu.Validate());
                WebsiteMenuHasErrors = CurrentWebsite.Menus.Any(x => x.Menu != obj && x.Menu.HasErrors);
            }

            obj.Label = obj.NewLabel;
            obj.Validate();
        }

        private bool CanSaveMenuItem(MenuItem arg)
        {
            if (arg != null) arg.Validate();

            return arg != null && !string.IsNullOrEmpty(arg.NewLabel) && arg.NewLabel != arg.Label && !arg.HasErrors;
        }

        private void ValidateUserInput(object sender, EventArgs e)
        {

            if (CurrentWebsite != null)
                CurrentWebsite.Validate();

            Validate();
        }

        public override void Validate()
        {
            base.Validate();

            if (CurrentWebsite.Menus != null)
            {
                ListExtensions.ForEach(CurrentWebsite.Menus, x => { x.Menu.Validate(); ListExtensions.ForEach(x.Menu.SubMenus, sub => sub.Validate()); });
                WebsiteMenuHasErrors = CurrentWebsite.IsPersisted ? CurrentWebsite.Menus.Any(x => x.Menu.HasErrors || x.Menu.SubMenus.Any(sub => sub.HasErrors))
                    : (ProfessionalWebsiteMenuItems != null && ProfessionalWebsiteMenuItems.SourceCollection.Cast<MenuItem>().Any(x => x.HasErrors || x.SubMenus.Any(sub => sub.HasErrors))) ||
                    (ConsumerWebsiteMenuItems != null && ConsumerWebsiteMenuItems.SourceCollection.Cast<MenuItem>().Any(x => x.HasErrors || x.SubMenus.Any(sub => sub.HasErrors)));
            }

            WebsiteContentHasErrors = HasError(new List<string> { "OutputDirectoryPath", "ApplicableRadiusMiles", "FeedBackEmailTemp" }) ||
                    CurrentWebsite.HasError(new List<string>()
                {
                    "GeographicDescription","BrowserTitle","OutPutDirectory"

                });

            WebsiteThemesHasErrors = HasError(new List<string> { "SelectedTheme", "BannerImage", "ImagePath" }) || CurrentWebsite.HasError(new List<string>()
            {
                "BrandColor", "AccentColor","BannerImage", "HeaderTitle"
            });

            WebsitePagesHasErrors = false;
        }

        private void AddLabelChangedListner(MenuItem item)
        {
            item.PropertyChanged -= (o, e) => { SaveMenuItemCommand.RaiseCanExecuteChanged(); Validate(); };
            item.PropertyChanged += (o, e) => { SaveMenuItemCommand.RaiseCanExecuteChanged(); Validate(); };
        }

        private string GetOutputFolder()
        {
            if (CurrentWebsite.IsPersisted) return CurrentWebsite.OutPutDirectory;

            var path = string.Empty;
            path = Path.Combine(MonahrqContext.MyDocumentsApplicationDirPath, "Websites", GetFolderName(ManageViewModel.WebsiteViewModel.Website.Name));

            return path.Length > 75 ? path.Substring(0, 75) : path;
        }

        private static string GetIdealForegroundColor(string brandColor, float percentage)
        {
            return ColorTranslator.ToHtml(ColorHelper.Darken(ColorTranslator.FromHtml(brandColor), percentage));
        }

        #endregion

    }

    public enum HospitalFilterByEnum
    {
        [Description("Filter By")]
        None = 0,
        [Description("Hospital with Local and CMS ID Mapped")]
        Mapped_Local_And_CMS_Provider_ID = 1,
        [Description("Local Hospitals only (not mapped with CMS ID)")]
        Local_Hospital_IDs_Only = 2,
        [Description("Hospitals with CMS ID only (not in the local hospital list)")]
        CMS_IDs_Only = 3
    }

    public enum NursingHomeFilterByEnum
    {
        [Description("All")]
        All,
        [Description("Selected for reporting")]
        Selected,
        [Description("Unselected for reporting")]
        UnSelected,
    }
}