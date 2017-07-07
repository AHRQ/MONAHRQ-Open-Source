using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Infrastructure.Types;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Types;
using Monahrq.Sdk.ViewModels;
using Monahrq.Websites.Events;
using Monahrq.Websites.Services;
using PropertyChanged;
using Application = System.Windows.Application;
using BaseViewModel = Monahrq.Default.ViewModels.BaseViewModel;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

// Prism tabcontrol binding: http://stackoverflow.com/questions/5650812/how-do-i-bind-a-tabcontrol-to-a-collection-of-viewmodels
// with themes, but code is incomplete: http://www.codeproject.com/Tips/134930/MVVM-TabControl-Binding-keeping-Theme

namespace Monahrq.Websites.ViewModels
{
    [Export(typeof(WebsiteManageViewModel)), ImplementPropertyChanged]
    public class WebsiteManageViewModel : BaseViewModel, INavigationAware
    {
        #region Fields and Constants

        private int _activeTabItemIndex;
        private string _wsDisplayName;
        bool _globalContextDefined;
        private bool _isLeaving;

        #endregion

        #region Constructor

        public WebsiteManageViewModel()
        {
            StatusLog = new ObservableCollection<string>();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the cancel command.
        /// </summary>
        /// <value>
        /// The cancel command.
        /// </value>
        public DelegateCommand CancelCommand { get; set; }
        /// <summary>
        /// Gets or sets the show all websites command.
        /// </summary>
        /// <value>
        /// The show all websites command.
        /// </value>
        public DelegateCommand ShowAllWebsitesCommand { get; set; }
        /// <summary>
        /// Gets or sets the save command.
        /// </summary>
        /// <value>
        /// The save command.
        /// </value>
        public DelegateCommand<object> SaveCommand { get; set; }
        /// <summary>
        /// Gets or sets the tab selection changed command.
        /// </summary>
        /// <value>
        /// The tab selection changed command.
        /// </value>
        public DelegateCommand TabSelectionChangedCommand { get; set; }

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public IRegionManager RegionManager { get; set; }

        /// <summary>
        /// Gets or sets the website data service.
        /// </summary>
        /// <value>
        /// The website data service.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public IWebsiteDataService WebsiteDataService { get; set; }

        /// <summary>
        /// Gets or sets the base data service.
        /// </summary>
        /// <value>
        /// The base data service.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        protected IBaseDataService BaseDataService { get; set; }

        /// <summary>
        /// Gets or sets the website details view model.
        /// </summary>
        /// <value>
        /// The website details view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        WebsiteDetailsViewModel WebsiteDetailsViewModel { get; set; }

        /// <summary>
        /// Gets or sets the website datasets view model.
        /// </summary>
        /// <value>
        /// The website datasets view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        WebsiteDatasetsViewModel WebsiteDatasetsViewModel { get; set; }

        /// <summary>
        /// Gets or sets the website measures view model.
        /// </summary>
        /// <value>
        /// The website measures view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        WebsiteMeasuresViewModel WebsiteMeasuresViewModel { get; set; }

        /// <summary>
        /// Gets or sets the website reports view model.
        /// </summary>
        /// <value>
        /// The website reports view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        WebsiteReportsViewModel WebsiteReportsViewModel { get; set; }

        /// <summary>
        /// Gets or sets the website settings view model.
        /// </summary>
        /// <value>
        /// The website settings view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public WebsiteSettingsViewModel WebsiteSettingsViewModel { get; set; }

        /// <summary>
        /// Gets or sets the website publish view model.
        /// </summary>
        /// <value>
        /// The website publish view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        WebsitePublishViewModel WebsitePublishViewModel { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether [is website created].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is website created]; otherwise, <c>false</c>.
        /// </value>
        public bool IsWebsiteCreated
        {
            get
            {
                return WebsiteViewModel != null && WebsiteViewModel.Website != null && WebsiteViewModel.Website.IsPersisted;
            }
        }

        public bool HasDatasets
        {
            get
            {
                return IsWebsiteCreated && WebsiteViewModel.Website.Datasets.Any();
            }
        }

        public bool HasMeasures
        {
            get
            {
                return IsWebsiteCreated && WebsiteViewModel.Website.Measures.Any();
            }
        }

        public bool HasReports
        {
            get
            {
                return IsWebsiteCreated && WebsiteViewModel.Website.Reports.Any();
            }
        }

        public bool HasCMWebsitePages
        {
            get
            {
                return IsWebsiteCreated && WebsiteViewModel.Website.WebsitePages.Any();
            }
        }

        public string RegionName
        {
            get { return RegionNames.WebsiteManageRegion; }
        }

        /// <summary>
        /// Gets or sets the index of the active tab item.
        /// </summary>
        /// <value>
        /// The index of the active tab item.
        /// </value>
        [DoNotCheckEquality]
        public int ActiveTabItemIndex
        {
            get { return _activeTabItemIndex; }
            set
            {
                if (!_isLeaving)
                {
                    if (_activeTabItemIndex != -1)
                    {
                        //var perviousTab = TabItems != null ? TabItems[_activeTabItemIndex] : null;
                        //if (perviousTab == null || !perviousTab.TabChanged()) return;
                    }
                    //if (_activeTabItemIndex != -1)
                    //{
                    //    var perviousTab = TabItems != null ? TabItems[_activeTabItemIndex] : null;
                    //    if (perviousTab == null || !perviousTab.TabChanged()) return;
                    //}
                    //if last tab is selected then refresh all the tabs 
                    //if (TabItems != null && TabItems.Count - 1 == value)
                    //{
                    //    Task.Run(() => TabItems.Where(x => x.Index > 0 && x.Index != value).ForEach(x => x.Refresh()));
                    //}
                }

                _activeTabItemIndex = value;

                if (!_isLeaving)
                {
                    //TabItems.ForEach(x => { x.IsActive = x.Index == value; });

                    RaisePropertyChanged(() => SaveButtonVisibility);
                    RaisePropertyChanged(() => ActiveTabItemIndex);

                    SetContextualHelp(_activeTabItemIndex);
                }
            }
        }

        private void SetContextualHelp(int tabItemIndex)
        {
            string helpConext;

            switch (tabItemIndex)
            {
                case 0:
                    helpConext = "Customizing Your Website -- Website Content Section";
                    break;
                case 1:
                    helpConext = "Choosing Website Datasets";
                    break;
                case 2:
                    helpConext = "Modify Measures Screen";
                    break;
                case 3:
                    helpConext = "Select Reports Screen";
                    break;
                case 4:
                    helpConext = "Customizing Your Website -- Website Content Section";
                    break;
                case 5:
                    helpConext = "Publishing Your Website";
                    break;
                default:
                    helpConext = "Websites";
                    break;
            }
            Events.GetEvent<SetContextualHelpContextEvent>().Publish(helpConext);
        }

        public Visibility SaveButtonVisibility
        {
            get
            {
                return !MonahrqContext.IsInializing && ActiveTabItemIndex == TabItems.Count - 1 ? Visibility.Hidden : Visibility.Visible;
            }
        }

        /// <summary>
        /// Gets or sets the tab items.
        /// </summary>
        /// <value>
        /// The tab items.
        /// </value>
        public ObservableCollection<ITabItem> TabItems { get; set; }

        /// <summary>
        /// Gets or sets the website view model.
        /// </summary>
        /// <value>
        /// The website view model.
        /// </value>
        public WebsiteViewModel WebsiteViewModel { get; set; }

        /// <summary>
        /// Gets or sets the display name of the website.
        /// </summary>
        /// <value>
        /// The display name of the website.
        /// </value>
        public string WebsiteDisplayName
        {
            get
            {
                _wsDisplayName = ((WebsiteViewModel == null || WebsiteViewModel.DisplayName == "[New Website]"))
                                     ? "[New Website]"
                                     : WebsiteViewModel.DisplayName;

                return _wsDisplayName;
            }
            set
            {
                if (_wsDisplayName == null || !_wsDisplayName.EqualsIgnoreCase(value))
                {
                    _wsDisplayName = value;
                    RaisePropertyChanged(() => WebsiteDisplayName);
                }
            }
        }

        public bool IsTabControlEnabled { get; set; }

        public ObservableCollection<ReportViewModel> AllAvailableReports { get; set; }

        public IEnumerable<MeasureModel> AllAvailableMeasures { get; set; }

        public ObservableCollection<WebsitePageModel> AllAvailableWebsitePages { get; set; }

        public Website CurrentWebsite
        {
            get
            {
                return WebsiteViewModel != null && WebsiteViewModel.Website != null ? WebsiteViewModel.Website : null;
            }
        }

        public Visibility WebsiteManageViewActionsVisibility { get; set; }

        public bool IsNewDatasetIncluded { get; set; }

        public bool IsAudienceChanged { get; set; }

        public bool IsTrendingYearUpdated { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitCommands()
        {
            CancelCommand = new DelegateCommand(ExecuteCancelCommand, CanCancel);
            ShowAllWebsitesCommand = new DelegateCommand(ExecuteShowAllWebsitesCommand, CanCancel);
            SaveCommand = new DelegateCommand<object>(x => ExecuteSaveCommand(), x => CanExecute());
            TabSelectionChangedCommand = new DelegateCommand(ExecuteTabSelectionChangedCommand, CanExecute);

            ImportCommand = new DelegateCommand(OnShowImportWindow, () => true);
            ExecuteImportCommand = new DelegateCommand(OnImportWebsite, () => true);
            CloseImportWindowCommand = new DelegateCommand(OnCancelImport, () => true);
            SelectImportFileCommand = new DelegateCommand(OnSelectImportFile, () => true);
        }

        private bool CanCancel()
        {
            return true;
        }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public override void OnImportsSatisfied()
        {
            Events.GetEvent<WebsiteNextTabEvent>().Subscribe(OnNextTab);
            Events.GetEvent<WebsiteCreatedOrUpdatedEvent>().Subscribe(OnWebsiteCreatedOrUpdated);
            //Events.GetEvent<WebsiteCreatedOrUpdatedExEvent>().Subscribe(OnWebsiteCreatedOrUpdatedEx);
            //Events.GetEvent<UpdateWebsiteTabContextEvent>().Subscribe(OnUpdateWebsiteTabContext);
            Events.GetEvent<DisableNavigationEvent>().Subscribe(d => Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate
                {
                    IsTabControlEnabled = !d.DisableUIElements;
                    return null;
                }), null));

            Events.GetEvent<CancellingWebsitePublishingEvent>().Subscribe(c =>
            {
                Cursor.Current = Cursors.Arrow;
                System.Windows.Forms.Application.DoEvents();
                Events.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent() { DisableUIElements = false });
                ThreadManager.KillAll();

            });

            Events.GetEvent<StartingWebsitePublishingEvent>().Subscribe(s => ThreadManager.ConcurrentThreads.Enqueue(s.Thread));

            TabItems = new ObservableCollection<ITabItem>
                {
                    WebsiteDetailsViewModel,
                    WebsiteDatasetsViewModel,
                    WebsiteMeasuresViewModel,
                    WebsiteReportsViewModel,
                    WebsiteSettingsViewModel,
                    WebsitePublishViewModel
                };

            TabItems.ForEach(x =>
            {
                var tab = x as WebsiteTabViewModel;
                if (tab != null) tab.ManageViewModel = this;
            });

            //if (MonahrqContext.IsInializing) return;

            InitCommands();
            IsTabControlEnabled = true;
        }

        /// <summary>
        /// Called when [website created or updated].
        /// </summary>
        /// <param name="args">The arguments.</param>
        void OnWebsiteCreatedOrUpdated(ExtendedEventArgs<GenericWebsiteEventArgs> args)
        {
            if (args == null)
                return;

            //var msg = String.Format("Website {0} has been added", vm.Website.Name);
            Events.GetEvent<GenericNotificationExEvent>().Publish(new NotificationMessage
            {
                Message = args.Data.Message,
                NotificationType = args.Data.NotificationType
            });
        }

        /// <summary>
        /// Called when [next tab].
        /// </summary>
        /// <param name="vm">The vm.</param>
        public void OnNextTab(WebsiteViewModel vm)
        {
            //if (ActiveTabItemIndex >= TabItems.Count)
            //    ActiveTabItemIndex = TabItems.Count - 1;

            if (ActiveTabItemIndex >= 0 && ActiveTabItemIndex <= (TabItems.Count - 1))
            {
                TabItems[ActiveTabItemIndex].TabChanged();
            }

            //ActiveTabItemIndex++;
            if (ActiveTabItemIndex == -1)
                Events.GetEvent<UpdateTabIndexEvent>().Publish(new TabIndexSelecteor { TabName = "WebsiteTabs", TabIndex = TabItems[ActiveTabItemIndex].Index++ });
        }

        /// <summary>
        /// Called when [update website tab context].
        /// </summary>
        /// <param name="eventArgs">The <see cref="UpdateTabContextEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void OnUpdateWebsiteTabContext(UpdateTabContextEventArgs eventArgs)
        {
            if (eventArgs.ExecuteViewModel == WebsiteTabViewModels.Manage)
                return;

            WebsiteViewModel = eventArgs.WebsiteViewModel;

            if (WebsiteViewModel != null)
            {
                WebsiteDisplayName = WebsiteViewModel.DisplayName;
                RaisePropertyChanged(() => NextStepButtonCaption);
                TabSelectionChangedCommand.RaiseCanExecuteChanged(); // with a new vm , we need to check if can enable the button 
            }
        }

        /// <summary>
        /// Executes the show all websites command.
        /// </summary>
        public void ExecuteShowAllWebsitesCommand()
        {
            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.WebsiteCollectionView, UriKind.Relative));
        }

        /// <summary>
        /// Executes the cancel command.
        /// </summary>
        public void ExecuteCancelCommand()
        {

            //TODO WebsiteViewModel Need to support Undo 
            if (CurrentWebsite != null && !CurrentWebsite.IsPersisted)
                if (MessageBox.Show(@"Your site is currently being generated." + Environment.NewLine + "This will cancel the current operation." + Environment.NewLine + " Do you want to cancel?",
                                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                    MessageBoxResult.No)
                    return;

            if (CurrentWebsite != null)
                CurrentWebsite.CurrentStatus = WebsiteState.PublishCancelled;

            TabItems.ForEach(x =>
            {
                var tab = (x as WebsiteTabViewModel);
                tab.IsTabVisited = false;
                if (tab is WebsitePublishViewModel)
                {
                    (tab as WebsitePublishViewModel).ShowMessage = Visibility.Collapsed;
                }
            });

            Events.GetEvent<CancellingWebsitePublishingEvent>().Publish(new CancellingWebsitePublishingEvent());
            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.WebsiteCollectionView, UriKind.Relative));
            Cursor.Current = Cursors.Arrow;
            System.Windows.Forms.Application.DoEvents();
        }

        /// <summary>
        /// Determines whether this instance can execute.
        /// </summary>
        /// <returns></returns>
        public bool CanExecute(/*bool fakeValue = false*/)
        {
            if (!MonahrqContext.IsInializing && ActiveTabItemIndex == TabItems.Count - 1) // we are On The last tab
            {
                switch (WebsiteViewModel.Website.CurrentStatus)
                {
                    case WebsiteState.HasDatasources:
                    case WebsiteState.HasMeasures:
                    case WebsiteState.HasReports:
                    case WebsiteState.Generated:
                    case WebsiteState.Published:
                    case WebsiteState.CompletedDependencyCheck:
                        return true;
                    default:
                        return false;
                }

            }
            return true;
        }

        public string NextStepButtonCaption
        {
            get
            {
                if (!MonahrqContext.IsInializing && ActiveTabItemIndex == TabItems.Count - 1) // we are On The last tab
                {
                    if (CurrentWebsite != null)
                    {
                        switch (CurrentWebsite.CurrentStatus)
                        {
                            case WebsiteState.Generating:
                            case WebsiteState.Generated:
                                return "Review Site";
                            case WebsiteState.Published:
                                return "Re-Publish Site";
                            default:
                                return "Publish Site";
                        }

                    }
                }
                return "Continue to next step";
            }
        }

        /// <summary>
        /// Executes the save command.
        /// </summary>
        public void ExecuteSaveCommand(bool runAynchronously = true)
        {
            if (CurrentWebsite == null) return;

            foreach (var item in TabItems)
            {
                var tab = item as WebsiteTabViewModel;
                if (tab == null || tab.Index > 4 || tab.Index < 0) continue;

                if (tab.Index == 3 && !CurrentWebsite.IsPersisted && (CurrentWebsite.Reports == null || !CurrentWebsite.Reports.Any()))
                    tab.OnPreSave();

                if (tab.Index == 3 && (CurrentWebsite.Reports == null || !CurrentWebsite.Reports.Any() || IsTrendingYearUpdated))
                    tab.Refresh();

                if (tab.Index != ActiveTabItemIndex && tab.Index == 4 && (string.IsNullOrEmpty(CurrentWebsite.GeographicDescription) || string.IsNullOrEmpty(CurrentWebsite.OutPutDirectory) || !CurrentWebsite.SelectedZipCodeRadii.Any() || IsAudienceChanged || IsNewDatasetIncluded))
                    tab.Refresh();

                if (!(tab as WebsiteTabViewModel).IsTabVisited) tab.Refresh();

                if (tab.Index == 3)
                {
                    if (!tab.IsTabVisited)
                        tab.TabChanged();
                }
                else
                {
                    tab.TabChanged();
                }
            }

            if (ActiveTabItemIndex > 0)
            {

                if (ActiveTabItemIndex == 4 && !WebsiteSettingsViewModel.ValidateAllRequiredFields()) return;

                // If and existing website is edited, the current status must change in order to allow for the dependency check to be readily available when publishing
                if (CurrentWebsite.IsPersisted && CurrentWebsite.CurrentStatus == null)
                {
                    CurrentWebsite.CurrentStatus = WebsiteState.Initialized;
                }
            }

            if (!string.IsNullOrEmpty(CurrentWebsite.Name) && CurrentWebsite.Name.Length > 255)
                CurrentWebsite.Name = CurrentWebsite.Name.Substring(0, 255);

            SaveCurrentWebsite(runAynchronously);

            if (!_globalContextDefined && CurrentWebsite.StateContext != null &&
                CurrentWebsite.StateContext.Any())
            {
                Task.Run(() =>
                {
                    var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
                    configService.HospitalRegion.SelectedRegionType = Type.GetType(string.Format("Monahrq.Infrastructure.Domain.Regions.{0}, Monahrq.Infrastructure", WebsiteViewModel.Website.RegionTypeContext));
                    configService.HospitalRegion.AddStates(CurrentWebsite.StateContext.ToList());
                    configService.Save();
                });
            }
            IsAudienceChanged = false;
            IsTrendingYearUpdated = false;
            IsNewDatasetIncluded = false;
        }

        private async void SaveCurrentWebsite(bool runAynchronously = true)
        {
            bool errorsOccurredWhenSaving;
            var uiMessage = string.Format("Website {0} has been {1}.", CurrentWebsite.Name,
                CurrentWebsite.IsPersisted ? "updated" : "added");
            var activityMessage = string.Empty;

            //if (!CurrentWebsite.IsPersisted)
            //{
            //    errorsOccurredWhenSaving = WebsiteDataService.SaveNewWebsite(CurrentWebsite);
            //}
            //else
            //{
            // If the website is edited, the current status must change in order to allow for the dependency check to be readily available when publishing
            if (CurrentWebsite.CurrentStatus == WebsiteState.HasDatasources ||
                CurrentWebsite.CurrentStatus == WebsiteState.HasMeasures ||
                CurrentWebsite.CurrentStatus == WebsiteState.HasReports)
                CurrentWebsite.CurrentStatus = WebsiteState.Initialized;

            switch (ActiveTabItemIndex)
            {
                case 0:
                    activityMessage = "Details created and/or updated.";
                    //TabItems[ActiveTabItemIndex].OnTabChanged();
                    break;
                case 1:
                    activityMessage = "Datsets selected and/or updated.";
                    //TabItems[ActiveTabItemIndex].OnTabChanged();
                    break;
                case 2:
                    activityMessage = "Measures selected and/or updated.";
                    //TabItems[ActiveTabItemIndex].OnTabChanged();
                    break;
                case 3:
                    activityMessage = "Reports selected and/or updated.";
                    //TabItems[ActiveTabItemIndex].OnTabChanged();
                    break;
                case 4:
                    activityMessage = "Settings have be saved and/or updated.";
                    //TabItems[ActiveTabItemIndex].OnTabChanged();
                    break;
            }

            if (!string.IsNullOrEmpty(activityMessage))
                CurrentWebsite.ActivityLog.Add(new ActivityLogEntry(activityMessage, DateTime.Now));

            errorsOccurredWhenSaving = false;
            Exception errorOccurred = null;
            bool operationExecuted;

            var progressService = new ProgressService();
            if (runAynchronously)
            {

                progressService.SetProgress("Saving website", 0, false, true);

                operationExecuted = await progressService.Execute(() => WebsiteDataService.SaveOrUpdateWebsite(CurrentWebsite),
                        async opResult =>
                        {
                            errorsOccurredWhenSaving = !opResult.Status && opResult.Exception != null;

                            if (errorsOccurredWhenSaving && opResult.Exception != null)
                            {
                                errorsOccurredWhenSaving = true;
                                errorOccurred = opResult.Exception.GetBaseException();
                            }
                            else
                            {
                                errorsOccurredWhenSaving = false;
                                if (opResult.Model != null)
                                {
                                    WebsiteViewModel.Website = (Website)opResult.Model;
                                }
                            }

                            await Task.Delay(1000);

                        }, new CancellationToken());
            }
            else
            {
                try
                {
                    WebsiteDataService.SaveOrUpdateWebsite(CurrentWebsite);
                }
                catch (Exception exc)
                {
                    errorOccurred = exc;
                    errorsOccurredWhenSaving = true;
                }
                finally
                {
                    operationExecuted = true;
                }
            }


            if (operationExecuted && runAynchronously)
                progressService.SetProgress("Completed", 100, true, false);

            if (errorOccurred == null && !errorsOccurredWhenSaving)
            {
                RaisePropertyChanged(() => WebsiteViewModel.Website);
                RaisePropertyChanged(() => CurrentWebsite);
                RaisePropertyChanged(() => CurrentWebsite.Datasets);
                RaisePropertyChanged(() => CurrentWebsite.Measures);
                RaisePropertyChanged(() => CurrentWebsite.Reports);

                // If no errors, move to the next tab or last tab 
                var tabCount = TabItems.Count - 1;
                if (!errorsOccurredWhenSaving && ActiveTabItemIndex < tabCount)
                {
                    Events.GetEvent<GenericNotificationEvent>().Publish(uiMessage);
                    //TabItems[ActiveTabItemIndex].Refresh();
                }

                if (!errorsOccurredWhenSaving && CurrentWebsite.IsPersisted)
                {
                    ShowImportEnabled = !CurrentWebsite.IsPersisted;
                }
            }
            else if (errorsOccurredWhenSaving && errorOccurred != null)
            {
                Events.GetEvent<ErrorNotificationEvent>().Publish(errorOccurred.GetBaseException());
            }
        }

        /// <summary>
        /// Executes the tab selection changed command.
        /// </summary>
        //private void ExecuteTabSelectionChangedCommand()
        //{

        //    if (ActiveTabItemIndex == TabItems.Count - 1) // we are On The last tab
        //    {

        //        switch (WebsiteViewModel.Website.CurrentStatus)
        //        {
        //            case WebsiteState.CompletedDependencyCheck: // generate or regenerate the site 
        //                var generateTask = new System.Threading.Thread(delegate()
        //                {
        //                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate
        //                    {
        //                        var task = Task.Factory.StartNew(() =>
        //                        {
        //                            TabItems[ActiveTabItemIndex].GenerateSite();
        //                        });

        //                        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Arrow;
        //                        return null;
        //                    }), null);
        //                });

        //                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
        //                generateTask.Start();
        //                Events.GetEvent<StartingWebsitePublishingEvent>().Publish(new StartingWebsitePublishingEvent() { Thread = generateTask });

        //                break;
        //            case WebsiteState.Published:
        //                TabItems[ActiveTabItemIndex].RunDependencyCheck();
        //                break;
        //            case WebsiteState.Generated: // Go To Review 
        //                TabItems[ActiveTabItemIndex].ReviewSite();
        //                break;
        //        }
        //    }
        //    else
        //    {

        //        if (!TabItems[ActiveTabItemIndex].ValidateAllRequiredFields())
        //            return;

        //        OnNextTab(WebsiteViewModel);
        //    }
        //}

        private void ExecuteTabSelectionChangedCommand()
        {
            var tabIndex = ActiveTabItemIndex;
            if (tabIndex != TabItems.Count - 1)
                Events.GetEvent<UpdateTabIndexEvent>().Publish(new TabIndexSelecteor { TabName = "WebsiteTabs", TabIndex = (tabIndex + 1) });  // ActiveTabItemIndex++;

            if ((tabIndex == TabItems.Count - 2) || ActiveTabItemIndex != TabItems.Count - 1) return;

            var publishingTab = TabItems[tabIndex] as WebsitePublishViewModel;
            if (publishingTab == null) return;

            var previousWebsiteStatus = CurrentWebsite.CurrentStatus;

            switch (CurrentWebsite.CurrentStatus)
            {
                case WebsiteState.HasDatasources:
                case WebsiteState.HasMeasures:
                case WebsiteState.HasReports:
                    return;
                case WebsiteState.CompletedDependencyCheck: // generate or regenerate the site 
                    var background = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(publishingTab.GenerateSite));
                    background.Completed += (o, e) =>
                    {
                        RaisePropertyChanged(() => CurrentWebsite);
                        RaisePropertyChanged(() => NextStepButtonCaption);
                    };
                    break;
                case WebsiteState.Published:
                    publishingTab.RunDependencyCheck();
                    break;
                case WebsiteState.Generated: // Go To Review 
                    publishingTab.ReviewSite();
                    break;
            }

            if (previousWebsiteStatus != CurrentWebsite.CurrentStatus)
                publishingTab.Refresh();

            RaisePropertyChanged(() => CurrentWebsite);
            RaisePropertyChanged(() => NextStepButtonCaption);
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (MonahrqContext.IsInializing) return;

            WebsiteManageViewActionsVisibility = Visibility.Visible;

            _isLeaving = false;
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            if (WebsiteViewModel != null)
            {
                WebsiteViewModel.Website = null;
                WebsiteViewModel = null;
            }

            string regionalContextType = null;
            IList<string> stateContext = new List<string>();

            AllAvailableReports = WebsiteDataService.GetReports().ToObservableCollection();
            AllAvailableMeasures = WebsiteDataService.GetMeasureViewModels(m => m.IsOverride == false);

            if (configService.HospitalRegion.IsDefined)
            {
                regionalContextType = configService.HospitalRegion.SelectedRegionType != null ? configService.HospitalRegion.SelectedRegionType.Name : null;

                if (configService.HospitalRegion.DefaultStates != null && configService.HospitalRegion.DefaultStates.Count > 0)
                {
                    stateContext = configService.HospitalRegion.DefaultStates.OfType<string>().Select(state => state).ToList();
                    _globalContextDefined = true;
                }
                else
                {
                    stateContext = new List<string>();
                    _globalContextDefined = false;
                }
            }

            Website ws = null;
            if (navigationContext.Parameters["WebsiteId"] != null)
            {
                var websiteId = int.Parse(navigationContext.Parameters["WebsiteId"]);

                if (websiteId > 0)
                {
                    WebsiteDataService.GetEntityById<Website>(websiteId, (result, error) =>
                    {
                        if (error == null)
                        {
                            ws = result;
                        }
                        else
                        {
                            Events.GetEvent<ErrorNotificationEvent>().Publish(error);
                        }
                    });
                }

                if (WebsiteViewModel == null || websiteId <= 0 || (WebsiteViewModel.Website != null && WebsiteViewModel.Website.Id != websiteId))
                {
                    WebsiteViewModel = new WebsiteViewModel
                    {
                        RegionManager = RegionManager,
                        WebsiteDataService = WebsiteDataService,
                        BaseDataService = BaseDataService
                    };
                }

                if (WebsiteViewModel.Website == null && ws != null || (WebsiteViewModel.Website != null && WebsiteViewModel.Website.Id != websiteId))
                {
                    if (ws.Datasets != null && ws.Datasets.Any(x => x == null))
                        ws.Datasets = ws.Datasets.RemoveNullValues();
                    if (ws.Hospitals != null && ws.Hospitals.Any(x => x == null))
                        ws.Hospitals = ws.Hospitals.RemoveNullValues();
                    if (ws.Reports != null && ws.Reports.Any(x => x == null))
                        ws.Reports = ws.Reports.RemoveNullValues();
                    if (ws.WebsitePages != null && ws.WebsitePages.Any(x => x == null))
                        ws.WebsitePages = ws.WebsitePages.RemoveNullValues();

                    if (ws.UtilizationReportCompression == null)
                        ws.UtilizationReportCompression = false;

                    if (ws.PublishIframeVersion == null)
                        ws.PublishIframeVersion = false;

                    if (!string.IsNullOrEmpty(ws.OutPutDirectory) &&
                        ws.OutPutDirectory.Contains(MonahrqContext.OldApplicationName))
                    {
                        ws.OutPutDirectory = ws.OutPutDirectory.Replace(MonahrqContext.OldApplicationName, "Monahrq");
                    }

                    if (string.IsNullOrEmpty(ws.RegionTypeContext))
                        ws.RegionTypeContext = regionalContextType;

                    if (ws.StateContext == null || !ws.StateContext.Any())
                    {
                        if (ws.SelectedReportingStates != null && ws.SelectedReportingStates.Any())
                        {
                            ws.StateContext = new List<string>(ws.SelectedReportingStates);
                        }
                        else
                        {
                            ws.StateContext = stateContext;
                            ws.SelectedReportingStates = stateContext.ToList();
                        }
                    }

                    WebsiteViewModel.Website = ws;
                    WebsiteDisplayName = ws.Name;
                    //ShowImportEnabled = false;
                }
                else
                {
                    WebsiteViewModel.Website = new Website(string.Empty, string.Empty, string.Empty, null, WebsiteState.Initialized)
                    {
                        RegionTypeContext = regionalContextType,
                        StateContext = stateContext,
                        SelectedReportingStates = stateContext.ToList()
                    };
                    //ShowImportEnabled = true;
                }

                ShowImportEnabled = !WebsiteViewModel.Website.IsPersisted;
            }


            if (WebsiteViewModel.Website.HospitalsChangedWarning.HasValue && WebsiteViewModel.Website.HospitalsChangedWarning.Value)
            {
                Events.GetEvent<UpdateTabIndexEvent>().Publish(new TabIndexSelecteor { TabName = "WebsiteTabs", TabIndex = 4 });
                TabItems[4].IsActive = true;
                //TabItems[4].OnIsActive();
                //ActiveTabItemIndex = 4;
                //RaisePropertyChanged(() => ActiveTabItemIndex);
            }
            else
            {
                if (ActiveTabItemIndex == -1)
                    Events.GetEvent<UpdateTabIndexEvent>().Publish(new TabIndexSelecteor { TabName = "WebsiteTabs", TabIndex = 0 });

                TabItems[0].IsActive = true;
                //TabItems[0].OnIsActive();

                //TabItems[4].Refresh();
            }


            // Notify each control that the Hosting VM has been navigated to.
            TabItems.Cast<WebsiteTabViewModel>().ForEach(ti => ti.OnHostNavigatedTo(navigationContext));

            //ActiveTabItemIndex = 0;

            //if (TabItems.All(x => !x.IsActive))
            //TabItems.ForEach(tab => tab.IsActive = ActiveTabItemIndex == tab.Index);
            //TabItems[ActiveTabItemIndex].IsActive = true;

            //RaisePropertyChanged(() => WebsiteViewModel);
            //RaisePropertyChanged(() => WebsiteViewModel.Website);

            RaisePropertyChanged(() => CurrentWebsite);
            RaisePropertyChanged(() => CurrentWebsite.Name);
            RaisePropertyChanged(() => CurrentWebsite.RegionTypeContext);
            RaisePropertyChanged(() => CurrentWebsite.StateContext);
            RaisePropertyChanged(() => CurrentWebsite.Datasets);
            RaisePropertyChanged(() => CurrentWebsite.SelectedZipCodeRadii);

            RaisePropertyChanged("SelectedIndex");
        }

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        ///   <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
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
            _isLeaving = true;


            TabItems.ForEach(x =>
            {
                var tab = x as WebsiteTabViewModel;
                if (tab == null) return;

                tab.IsTabVisited = false;
                tab.Reset();
            });

            WebsiteViewModel.Website = null;
            WebsiteViewModel = null;

            RaisePropertyChanged(() => WebsiteViewModel);
            RaisePropertyChanged(() => WebsiteViewModel.Website);

            IsAudienceChanged = false;
            IsTrendingYearUpdated = false;
            IsNewDatasetIncluded = false;
            //RaisePropertyChanged(() => WebsiteViewModel.Website.Datasets);
            //RaisePropertyChanged(() => WebsiteViewModel.Website.Measures);
            //RaisePropertyChanged(() => WebsiteViewModel.Website.Reports);
            //RaisePropertyChanged(() => WebsiteViewModel.Website.Hospitals);
            //RaisePropertyChanged(() => WebsiteViewModel.Website.NursingHomes);
            //RaisePropertyChanged(() => WebsiteViewModel.Website.SelectedZipCodeRadii);

            //ActiveTabItemIndex = -1;
            Events.GetEvent<UpdateTabIndexEvent>().Publish(new TabIndexSelecteor { TabName = "WebsiteTabs", TabIndex = -1 });

        }

        //public void OnTabChanged()
        //{}

        public void ChangeWebsiteStatus(WebsiteState status)
        {
            if (CurrentWebsite == null) return;

            var updateQuery = string.Format(@"UPDATE Websites SET CurrentStatus ='{0}' WHERE Id = {1}", status, CurrentWebsite.Id);

            using (var session = BaseDataService.SessionFactoryProvider.SessionFactory.OpenSession())
            {
                using (var trx = session.BeginTransaction())
                {
                    session.CreateSQLQuery(updateQuery).ExecuteUpdate();
                    trx.Commit();
                }
            }

            CurrentWebsite.CurrentStatus = status;
            RaisePropertyChanged(() => CurrentWebsite);
        }

        #endregion

        #region Import Website

        public ObservableCollection<string> StatusLog { get; set; }

        /// <summary>
        /// Gets or sets the import command.
        /// </summary>
        /// <value>
        /// The import command.
        /// </value>
        public DelegateCommand ImportCommand { get; set; }

        /// <summary>
        /// Gets or sets the import command.
        /// </summary>
        /// <value>
        /// The import command.
        /// </value>
        public DelegateCommand ExecuteImportCommand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show import enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show import enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowImportEnabled { get; set; }

        /// <summary>
        /// Gets or sets the selected import file.
        /// </summary>
        /// <value>
        /// The selected import file.
        /// </value>
        public string SelectedImportFile { get; set; }

        /// <summary>
        /// Gets or sets the selected existing website identifier.
        /// </summary>
        /// <value>
        /// The selected existing website identifier.
        /// </value>
        public string SelectedExistingWebsiteId { get; set; }

        /// <summary>
        /// Gets or sets the type of the selected import.
        /// </summary>
        /// <value>
        /// The type of the selected import.
        /// </value>
        public WebsiteImportTypeEnum SelectedImportType { get; set; }

        /// <summary>
        /// Gets or sets the avaiable websites.
        /// </summary>
        /// <value>
        /// The avaiable websites.
        /// </value>
        public ObservableCollection<SelectListItem> AvaiableWebsites { get; set; }

        /// <summary>
        /// Gets or sets the close import window command.
        /// </summary>
        /// <value>
        /// The close import window command.
        /// </value>
        public DelegateCommand CloseImportWindowCommand { get; set; }

        /// <summary>
        /// Gets or sets the select import file command.
        /// </summary>
        /// <value>
        /// The select import file command.
        /// </value>
        public DelegateCommand SelectImportFileCommand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show import website panel].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show import website panel]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowImportWebsitePanel { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        /// <value>
        /// The cancellation token.
        /// </value>
        protected CancellationTokenSource CancellationToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable complete button].
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable complete button]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableCompleteButton { get; set; }

        /// <summary>
        /// Called when [import website].
        /// </summary>
        private void OnImportWebsite()
        {
            var uiContext = SynchronizationContext.Current;

            if (string.IsNullOrEmpty(SelectedImportFile) && string.IsNullOrEmpty(SelectedExistingWebsiteId)) return;

            StatusLog.Clear();

            CancellationToken = new CancellationTokenSource();

            var fileContents = !string.IsNullOrEmpty(SelectedImportFile) ? File.ReadAllText(SelectedImportFile, UTF8Encoding.UTF8) : null;

            //Website importedWebsite = default(Website); 
            var request = new ImportRequest
            {
                Provider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>(),
                Logger = ServiceLocator.Current.GetInstance<ILogWriter>(LogNames.Session),
                ImportType = SelectedImportType,
                ExistingWebsiteId = !string.IsNullOrEmpty(SelectedExistingWebsiteId) && SelectedImportType == WebsiteImportTypeEnum.Existing ? int.Parse(SelectedExistingWebsiteId) : (int?)null,
                WebsiteXmlFromFile = SelectedImportType == WebsiteImportTypeEnum.File ? fileContents : null
            };

            WebsiteExporter.Import(request, CancellationToken.Token, response =>
            {
                if (response.IsComplete && response.Website != null)
                {
                    uiContext.Send(x =>
                    {   //if (WebsiteViewModel != null && importedWebsite != null)
                        {
                            WebsiteViewModel.Website = response.Website;
                            var errorWhileSaving = WebsiteDataService.SaveOrUpdateWebsite(WebsiteViewModel.Website);
                            if (errorWhileSaving)
                            {
                                StatusLog.Add(
                                    "An error occurred while importing the website file. Please use a valid import file (exported through MONAHRQ) and try again.");
                            }
                            RaisePropertyChanged(() => WebsiteViewModel.Website);
                            RaisePropertyChanged(() => WebsiteDisplayName);
                            RaisePropertyChanged(() => WebsiteDisplayName);
                            RaisePropertyChanged(() => CurrentWebsite);
                            RaisePropertyChanged(() => CurrentWebsite.Name);
                            RaisePropertyChanged(() => CurrentWebsite.Description);
                            RaisePropertyChanged(() => CurrentWebsite.CurrentStatus);
                            RaisePropertyChanged(() => CurrentWebsite.ReportedYear);
                            RaisePropertyChanged(() => CurrentWebsite.ReportedQuarter);
                            RaisePropertyChanged(() => CurrentWebsite.DefaultAudience);
                            RaisePropertyChanged(() => CurrentWebsite.HasConsumersAudience);
                            RaisePropertyChanged(() => CurrentWebsite.HasProfessionalsAudience);
                            RaisePropertyChanged(() => CurrentWebsite.AboutUsSectionSummary);
                            RaisePropertyChanged(() => CurrentWebsite.AboutUsSectionText);
                            TabItems[ActiveTabItemIndex].Refresh();

                            //EnableCompleteButton = true;
                        }
                    }, null);
                    //    uiContext.Send(x => StatusLog.Add("Import Completed Succesfully."), null);
                }
                //else
                //{
                uiContext.Send(x => StatusLog.Add(response.Message), null);
                //}

            }, exceptionResponse =>
            {
                request.Logger.Write(exceptionResponse.Exception);
                uiContext.Send(x => StatusLog.Add("An error occurred while importing the website file. Please use a valid import file (exported through MONAHRQ) and try again."), null);
            });
            EnableCompleteButton = true;

            //if (WebsiteViewModel != null && importedWebsite != null)
            //{
            //    WebsiteViewModel.Website = importedWebsite;
            //    RaisePropertyChanged(() => WebsiteViewModel);
            //    RaisePropertyChanged(() => WebsiteDisplayName);
            //}
        }

        //public void ImportCallBack(ImportResponse response)
        //{
        //    if (response.IsComplete && response.Website != null)
        //    {
        //        if (WebsiteViewModel != null)
        //        {
        //            WebsiteViewModel.Website = response.Website;
        //        }
        //    }
        //}

        //public void ImportExceptionCallBack(ImportResponse response)
        //{
        //    StatusLog.Add(response.Message);
        //}

        private void OnSelectImportFile()
        {
            // Set filter for file extension and default file extension 
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();

            var path = MonahrqContext.FileExportsDirPath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var dlg = new OpenFileDialog
            {
                DefaultExt = "*.txt",
                InitialDirectory = path,
                Filter = "Text Files (*.txt)|*.txt|All Files Files (*.*)|*.*",
                FilterIndex = 1,
                Multiselect = false
            };

            if (dlg.ShowDialog() != true) return;

            SelectedImportFile = dlg.FileName;
            configService.LastDataFolder = Path.GetDirectoryName(SelectedImportFile);
        }

        private void OnShowImportWindow()
        {
            ShowImportWebsitePanel = true;
            EnableCompleteButton = false;

            StatusLog.Clear();

            SelectedImportType = WebsiteImportTypeEnum.Existing;
            AvaiableWebsites = WebsiteDataService.GetAllWebsites().Select(web => new SelectListItem
            {
                Text = web.Website.Name,
                Value = web.Website.Id
            }).ToObservableCollection();

            if (!string.IsNullOrEmpty(SelectedImportFile)) SelectedImportFile = null;
        }

        private void OnCancelImport()
        {
            if (CancellationToken != null)
                CancellationToken.Cancel();

            if (!string.IsNullOrEmpty(SelectedImportFile)) SelectedImportFile = null;
            StatusLog.Clear();
            ShowImportWebsitePanel = false;
        }

        #endregion

    }
}
