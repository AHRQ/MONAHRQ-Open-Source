using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Diagnostics;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Services.Generators;
using Monahrq.Sdk.Utilities;
using Monahrq.Websites.Events;
using Monahrq.Websites.Services;
using Monahrq.Websites.ViewModels.Publish;
using NHibernate.Criterion;
using NHibernate.Linq;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Windows.Data;

namespace Monahrq.Websites.ViewModels
{
	[Export(typeof(WebsitePublishViewModel))]
	public class WebsitePublishViewModel : WebsiteTabViewModel, IWebsitePublishViewModel
	{
		#region Fields and Constants

		private IEnumerable<ValidationResultViewModel> _dependencyCheckResults;
		private int _publishProgress;
		private Visibility _resultsVisibility;
		private Visibility _publishLogVisibility;
		private Visibility _reviewVisibility;
		private Visibility _showMessage;
		private Visibility _showRunCheckSuccessMessage;
		private string _outPutWebSiteFolder;
		private string _reviewWarning;
		private bool _isAutoSave;
		private static readonly List<string> _qiMeasuresDatasetsRequiringLocalHospitalId = new List<string> { "AHRQ-QI Provider Data", "AHRQ-QI Composite Data", "AHRQ-QI Area Data" }; //
		private static readonly List<string> _datasetsRequiringLocalHospitalId = new List<string> { "Inpatient Discharge", "ED Treat And Release" };
		private const string AVAILABLE_SPACE_MESSAGE = @"Physicial Diskspace warning:{0}Based on your data the free space requirements to generate your website is estimated to <<{1} GB>> including the temporary space needed for processing . Your generated website may be much smaller than the minimum space requirement. The current free space in your machine seems to be lower than the estimated level. To successfully generate the website, you may like to free up some space before generating or else the website generation may experience some space related errors.{0}";
		private const string AVAILABLE_MEMORY_MESSAGE = @"Virtual Memory Warning:{0}It seems your machine is running low on memory. The website generation may take longer than expected. Please close some programs and try again.{0}";
		private const int MINIMUM_SPACE_REQUIREMENT = 16;
		private const int MINIMUM_VIRTUAL_SPACE_REQUIREMENT = 2;

		#endregion

		public WebsitePublishViewModel()
		{
			ShowMessage = Visibility.Collapsed;
			ShowRunCheckSuccessMessage = Visibility.Collapsed;
		}

		#region Properties

		public static string DefaultWebsiteOutputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), MonahrqContext.ApplicationName, "Websites");

		public string RunDependencyCheckButtonCaption { get; set; }

		public bool WasRun { get; set; }

		public bool IsDependencyCheckRunning { get; set; }

		public bool IsPublishOptionsExpanded { get; set; }

		public IEnumerable<ValidationResultViewModel> DependencyCheckResults
		{
			get { return _dependencyCheckResults; }
			set
			{
				_dependencyCheckResults = value;
				RaisePropertyChanged(() => DependencyCheckResults);
			}
		}
		public Visibility ShowMessage
		{
			get { return _showMessage; }
			set
			{
				_showMessage = value;
				RaisePropertyChanged(() => ShowMessage);
			}

		}

		public Visibility ShowRunCheckSuccessMessage
		{
			get { return _showRunCheckSuccessMessage; }
			set
			{
				_showRunCheckSuccessMessage = value;
				RaisePropertyChanged(() => ShowRunCheckSuccessMessage);
			}

		}


		private ValidationResultViewModelFactory ValidationFactory { get; set; }

		public Visibility ResultsVisibility
		{
			get { return _resultsVisibility; }
			set
			{
				_resultsVisibility = value;
				RaisePropertyChanged(() => ResultsVisibility);
			}
		}

		public Visibility PublishLogVisibility
		{
			get { return _publishLogVisibility; }
			set
			{
				_publishLogVisibility = value;
				RaisePropertyChanged(() => PublishLogVisibility);
			}
		}

		public Visibility ReviewVisibility
		{
			get { return _reviewVisibility; }
			set
			{
				_reviewVisibility = value;
				RaisePropertyChanged(() => ReviewVisibility);
			}
		}

		public Visibility DependencyCheckControl { get; set; }

		public Visibility DataChangeWarningVisibility { get; set; }

		public class WebsitePublishEventLog
		{
			public WebsitePublishEventRegion Region { get; set; }
			public string RegionName { get { return Region.Name; } }
			public string Message { get; set; }
			public PubishMessageTypeEnum MessageType { get; set; }
			public DateTime EventTime { get; set; }
            public int Order { get; set; }
            public string RegionSortText
			{
				get
				{
				    if (RegionName.EqualsIgnoreCase("Website Completed"))
				    {
                        return string.Format("{0}{1:000}-{2}-{3}",
                        9999,
                        9999,
                        Region.Name,
                        EventTime.ToString("o"));
                    }

					return string.Format("{0}{1:000}-{2}-{3}", 
						Region.Order < 0 ? 0 : 1, 
						Region.Order < 0 ? 100 + Region.Order : Region.Order,
						Region.Name,
						EventTime.ToString("o"));
				}
			}

			public WebsitePublishEventLog() { }
			public WebsitePublishEventLog(WebsitePublishEventRegion region,String message, PubishMessageTypeEnum messageType, DateTime eventTime)
			{
				Region = region;
				Message = message;
				MessageType = messageType;
				EventTime = eventTime;
			}

			public static bool UILogFilter(object log)
			{
				return (log as WebsitePublishEventLog).MessageType != PubishMessageTypeEnum.Error;
			}
		}
		public ICollectionView UIPublishLogsView { get; set; }
		public ObservableCollection<WebsitePublishEventLog> PublishLogs { get; set; }
	
		public string OutPutWebSiteFolder
        {
            get { return _outPutWebSiteFolder; }
            set
            {
                _outPutWebSiteFolder = value;
                RaisePropertyChanged(() => OutPutWebSiteFolder);
            }
        }

        public string ReviewWarning
        {
            get { return _reviewWarning; }
            set
            {
                _reviewWarning = value;
                RaisePropertyChanged(() => ReviewWarning);
            }
        }

        public int PublishProgress
        {
            get { return _publishProgress; }
            set
            {
                _publishProgress = value;
                RaisePropertyChanged(() => PublishProgress);
            }
        }

        public DataGrid LogDataGrid { get; set; }

        private string _headerText;
        public string HeaderText
        {
            get
            {
                _headerText = "Publish Site";
                return _headerText;
            }
            set { _headerText = value; }
        }

        public bool IsExiting { get; set; }

        #endregion

        #region Commands

        public DelegateCommand ConfirmPublish { get; set; }

        public DelegateCommand CancelPublish { get; set; }

        public DelegateCommand OpenOutPutFolderCommand { get; set; }

        public DelegateCommand RunDependencyCheckCommand { get; set; }

        public DelegateCommand<ValidationResultViewModel> NavigateToFixTab { get; set; }

        #endregion

        #region Imports

        [Import]
        public IWebsiteDataService DataService { get; set; }

        [Import]
        public WebsiteGenerator Generator { get; set; }

        #endregion

        #region Methods

        protected override void InitCommands()
        {
            RunDependencyCheckCommand = new DelegateCommand(RunDependencyCheck, () => !IsDependencyCheckRunning);
            EventAggregator.GetEvent<WebsitePublishEvent>().Subscribe(OnStartGeneration);

            NavigateToFixTab = new DelegateCommand<ValidationResultViewModel>(OpenHowToFixFile);//vvm =>

            OpenOutPutFolderCommand = new DelegateCommand(() =>
            {
                try
                {
                    Process.Start(OutPutWebSiteFolder);
                }
                catch (Exception e)
                {
                    EventAggregator
                          .GetEvent<ErrorNotificationEvent>()
                          .Publish(e);
                }
            });
        }

        public override void OnHostNavigatedTo(NavigationContext navigationContext)
        {
            base.OnHostNavigatedTo(navigationContext);
            IsExiting = false;
        }

        public override void OnHostNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnHostNavigatedFrom(navigationContext);
            IsExiting = true;
        }

        private void OpenHowToFixFile(ValidationResultViewModel result)
        {
            //help = string.Format("{0}.html", help);

            if (!string.IsNullOrWhiteSpace(result.HelpTopic))
            EventAggregator.GetEvent<OpenContextualHelpContextEvent>().Publish(result.HelpTopic);
            //switch (result.Result)
            //{
            //    case ValidationOutcome.StaleBaseData:
            //        try
            //        {
            //            Process.Start(string.Format("mailto:monahrq@ahrq.gov?Subject=Base data (census population ) is out of date "));
            //        }
            //        catch (Exception ex)
            //        {
            //            EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
            //        }
            //        break;
            //    case ValidationOutcome.EmergencyDepartmentTreatAndReleaseReportEDServices:
            //        RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MainDataSetView, UriKind.Relative));
            //        break;
            //    case ValidationOutcome.MissgingCustomRegion:
            //    case ValidationOutcome.CustomRegionMissingPopulationCount:
            //    case ValidationOutcome.CustomRegionNotDefined:
            //    case ValidationOutcome.QIUnMappedHospitalLocalId:
            //    case ValidationOutcome.UnMappedHospitalLocalId:
            //    case ValidationOutcome.UnMappedHospitalProfileLocalId:
            //    case ValidationOutcome.CheckDatasetMappingForCounty:
            //        var q = new UriQuery() { { "TabIndex", "1" } };
            //        RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MainDataSetView + q, UriKind.Relative));
            //        break;
            //    case ValidationOutcome.AhrqQiDbConnection:
            //        var index = new UriQuery { { "TabIndex", "1" } };
            //        RegionManager.RequestNavigate(RegionNames.MainContent, new Uri("ManageSettingsView" + index, UriKind.Relative));
            //        break;
            //}
          
            //Forms.Help.ShowHelp(null, MonahrqContext.HelpFolderPath); //, helpPage);
            //Help.ShowHelp(null, MonahrqContext.WebsiteHelpFolderPath, helpPage);
            //Process.Start(MonahrqContext.WebsiteHelpFolderPath);
        }

        protected override void InitProperties()
        {
            IsDependencyCheckRunning = false;
            RunDependencyCheckButtonCaption = " RUN CHECK  ";
            DependencyCheckResults = Enumerable.Empty<ValidationResultViewModel>();
            RunDependencyCheckCommand.RaiseCanExecuteChanged();
			PublishLogs = new ObservableCollection<WebsitePublishEventLog>();
			UIPublishLogsView = CollectionViewSource.GetDefaultView(PublishLogs);
			UIPublishLogsView.Filter = WebsitePublishEventLog.UILogFilter;

			ResultsVisibility = Visibility.Hidden;
            PublishLogVisibility = Visibility.Hidden;
            ReviewVisibility = Visibility.Hidden;
            DependencyCheckControl = Visibility.Visible;

            PublishProgress = 0;

            DataChangeWarningVisibility = Visibility.Hidden;
        }

        public override void Continue()
        {
            //EventAggregator.GetEvent<UpdateWebsiteTabContextEvent>().Publish(new UpdateTabContextEventArgs { WebsiteViewModel = ManageViewModel.WebsiteViewModel, ExecuteViewModel = WebsiteTabViewModels.ReportsPublish });

        }

        public override void Refresh()
        {
            base.Refresh();

            //ManageViewModel.TabItems.ForEach(tab =>
            //{
            //    if(tab.GetType() == this.GetType()) Continue();

            //    tab.Refresh();
            //});

            if (CurrentWebsite == null || CurrentWebsite.CurrentStatus < WebsiteState.CompletedDependencyCheck)
            {
                InitProperties();
            }
            else
            {
                switch (CurrentWebsite.CurrentStatus)
                {
                    case WebsiteState.Generated:
                        PublishProgress = 100;
                        SetToGeneratedMode();
                        break;
                    case WebsiteState.Published:
                        PublishProgress = 100;
                        ReviewSite();
                        break;
                    case WebsiteState.CompletedDependencyCheck:
                        if (PublishLogs == null || !PublishLogs.Any())
                        {
                            SetToRunDependencyCheckMode();
                        }
                        else
                        {
                            PublishProgress = 50;
                            SetToCompletedDependencyCheckMode();
                        }
                        break;
                    default:
                        PublishProgress = 0;
                        break;
                }
            }
            ResultsVisibility = Visibility.Visible;
            IsPublishOptionsExpanded = true;
            //ShowMessage = Visibility.Collapsed;
        }

        /// <summary>
        /// Automatics the save.
        /// </summary>
        private void AutoSave()
        {
            try
            {
                if (CurrentWebsite == null || WebsiteDataService == null || CurrentWebsite.CurrentStatus == WebsiteState.PublishCancelled) return;

                _isAutoSave = true;

                ManageViewModel.ExecuteSaveCommand(false);

                _isAutoSave = false;

            }
            catch (Exception e)
            {
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
            }
        }

        public override void Save()
        {
            string message;
            bool errorsOccurredWhileSaving;
            if (!CurrentWebsite.IsPersisted)
            {
                errorsOccurredWhileSaving = WebsiteDataService.SaveNewWebsite(CurrentWebsite);
                message = string.Format("Website {0} has been added", CurrentWebsite.Name);
            }
            else
            {
                errorsOccurredWhileSaving = WebsiteDataService.UpdateWebsite(CurrentWebsite);
                message = string.Format("Website {0} has been updated", CurrentWebsite.Name);
            }

            if (!errorsOccurredWhileSaving && !_isAutoSave)
            {
                var eventArgs = new ExtendedEventArgs<GenericWebsiteEventArgs>
                {
                    Data = new GenericWebsiteEventArgs { Website = ManageViewModel.WebsiteViewModel, Message = message }
                };

                EventAggregator.GetEvent<WebsiteCreatedOrUpdatedEvent>().Publish(eventArgs);
            }
        }

        private void SetToCompletedDependencyCheckMode()
        {
            RunDependencyCheckButtonCaption = " RE-RUN CHECK  ";
            IsDependencyCheckRunning = false;
            RunDependencyCheckCommand.RaiseCanExecuteChanged();
            WasRun = true;
            //ShowMessage = Visibility.Collapsed;
        }

        private void SetToGeneratedMode()
        {
            ResultsVisibility = Visibility.Hidden;
            DependencyCheckControl = Visibility.Hidden;
            PublishLogVisibility = Visibility.Visible;
            ReviewVisibility = Visibility.Hidden;
            DependencyCheckControl = Visibility.Hidden;
        }

        private void SetToRunDependencyCheckMode()
        {
            PublishLogVisibility = Visibility.Hidden;
            ReviewVisibility = Visibility.Hidden;
            DependencyCheckControl = Visibility.Visible;
            PublishProgress = 0;
            DataChangeWarningVisibility = Visibility.Hidden;
            // ShowMessage = Visibility.Visible;
        }

		private void AddPublishEventToLogs(WebsitePublishEventArgs wpArg)
		{
            Application.Current.DoEventsUI();
            if (PublishLogs == null)
				PublishLogs = new ObservableCollection<WebsitePublishEventLog>();

			var log = new WebsitePublishEventLog(wpArg.Region, wpArg.Message, wpArg.MessageType, wpArg.EventTime);

			PublishLogs.Add(log);

            PublishLogs = PublishLogs//.OrderBy(pl => pl.RegionName)
                                     .OrderBy(pl => pl.RegionSortText)
                                     .ThenBy(pl => pl.EventTime)
                                     .ToObservableCollection();

		    UIPublishLogsView = PublishLogs.ToListCollectionView();

            //LogDataGrid.Refresh();
            LogDataGrid.ScrollIntoView(log);

            Application.Current.DoEventsUI();
        }

        private void AddMessageToLogs(WebsitePublishEventRegion region,string message)
		{
			var wpArg = new WebsitePublishEventArgs(
				region,
				message,
				PubishMessageTypeEnum.Information,
				WebsiteGenerationStatus.ReportsGenerationInProgress,
				DateTime.Now,
				PublishTask.Full);
			AddPublishEventToLogs(wpArg);
		}
		private void AddMessageToLogs(string message)
		{
			AddMessageToLogs(WebsitePublishEventRegion.DefaultRegion, message);
		}

		private void OnStartGeneration(ExtendedEventArgs<WebsitePublishEventArgs> wpArg)
        {
            if (wpArg.Data.PublishTask == PublishTask.PreviewOnly ||
                wpArg.Data.PublishTask == PublishTask.BaseCMSZoneOnly)
                return;


            if (ManageViewModel.WebsiteViewModel == null ||
                ManageViewModel.WebsiteViewModel.Website.CurrentStatus == WebsiteState.PublishCancelled)
                return;

            if (ManageViewModel.WebsiteViewModel.Website.CurrentStatus != WebsiteState.Generating)
                ManageViewModel.ChangeWebsiteStatus(WebsiteState.Generating);

            switch (wpArg.Data.Status)
            {
                case WebsiteGenerationStatus.Error:
					AddPublishEventToLogs(wpArg.Data);
					Logger.Warning(wpArg.Data.Message);
                    break;
                case WebsiteGenerationStatus.Complete:
					AddPublishEventToLogs(wpArg.Data);
                    Logger.Information(wpArg.Data.Message);
                    break;
                default:
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Render,
                        new DispatcherOperationCallback(delegate
                        {
                            AddPublishEventToLogs(wpArg.Data);

                            // Application.Current.DoEventsUI();
                            //	var itemToAdd = new Tuple<string, DateTime>(wpArg.Data.Message, wpArg.Data.EventTime);
                            //	PublishLogs.Add(itemToAdd);
                            //	LogDataGrid.ScrollIntoView(itemToAdd);

                            if (wpArg.Data.MessageType == PubishMessageTypeEnum.Warning)
                                Logger.Warning(wpArg.Data.Message);
                            else
                                Logger.Information(wpArg.Data.Message);
                            return null;
                        }), null);

                    break;
            }
        }

        //private void ChangeWebSiteStatus(WebsiteState websiteState)
        //{
        //    if (CurrentWebsite != null && CurrentWebsite.CurrentStatus != websiteState)
        //    {
        //        CurrentWebsite.CurrentStatus = websiteState;

        //        AutoSave();

        //        //EventAggregator.GetEvent<UpdateWebsiteTabContextEvent>()
        //        //    .Publish(new UpdateTabContextEventArgs()
        //        //    {
        //        //        WebsiteViewModel = ManageViewModel.WebsiteViewModel,
        //        //        ExecuteViewModel = WebsiteTabViewModels.ReportsPublish
        //        //    });
        //    }
        //}

        public void GenerateSite()
        {
            Application.Current.DoEvents();

            IsPublishOptionsExpanded = false;
            ResultsVisibility = Visibility.Hidden;
            DependencyCheckControl = Visibility.Hidden;
            PublishLogVisibility = Visibility.Visible;
            ReviewVisibility = Visibility.Hidden;

            //Uri outputUri = new Uri(CurrentWebsite.OutPutDirectory);
            Uri outputUri = new Uri(MonahrqContext.BinFolderPath);
            var driveInfo = new DriveInfo(outputUri.LocalPath);
            var availableDiskSpace = MonahrqDiagnostic.GetHDAvailableFreeSpaceAsString(driveInfo.Name);
            var availableVirtualMemory = MonahrqDiagnostic.GetTotalAmountOfFreeVirtualMemoryAsString();

            if (CheckEnviroment(availableDiskSpace, availableVirtualMemory)) return;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate
            {
				PublishLogs.Clear();
                PublishProgress = 50;

				AddMessageToLogs("*********************************************************************************");
                Application.Current.DoEvents();
				//AddMessageToLogs("Selected Audience - " + Inflector.Titleize(CurrentWebsite.Audience.ToString()));
				AddMessageToLogs("Selected Audience(s) - " + Inflector.Titleize(string.Join(",", CurrentWebsite.Audiences.ToList())));
                Application.Current.DoEvents();
                if (CurrentWebsite.DefaultAudience.HasValue)
                {
					AddMessageToLogs("Default Audience - " + Inflector.Titleize(CurrentWebsite.DefaultAudience.Value.ToString()));
                    Application.Current.DoEvents();
                }

                AddMessageToLogs("Selected State(s) - " + string.Join(",", CurrentWebsite.SelectedReportingStates));
                Application.Current.DoEvents();
                AddMessageToLogs("Selected theme(s) - " + string.Join(",", CurrentWebsite.Themes.Select(x => x.SelectedTheme)));
                Application.Current.DoEvents();
                AddMessageToLogs("Selected Brand color(s) - " + string.Join(",", CurrentWebsite.Themes.Select(x => ColorTranslator.FromHtml(x.BrandColor))));
                Application.Current.DoEvents();
                AddMessageToLogs("Saving JSON to the folder - " + CurrentWebsite.OutPutDirectory);
                Application.Current.DoEvents();
                AddMessageToLogs("*********************************************************************************");
                Application.Current.DoEvents();
                AddMessageToLogs("Available Free Disk space - " + availableDiskSpace);
                Application.Current.DoEvents();
                AddMessageToLogs("Available Virtual Memory - " + availableVirtualMemory);
                Application.Current.DoEvents();
                AddMessageToLogs("*********************************************************************************");
                Application.Current.DoEvents();

                //WebsiteDataService.Refresh(CurrentWebsite);
                Mouse.OverrideCursor = Cursors.Wait;

                return null;
            }), null);

            Application.Current.DoEvents();

            try
            {
                EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = true });
                Generator.Publish(CurrentWebsite);
                EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = false });
            }
            catch (ThreadAbortException)
            {
                //Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                ManageViewModel.ChangeWebsiteStatus(WebsiteState.Generated);
                //ChangeWebSiteStatus(WebsiteState.Generated); // will work for now as Publish is single threaded , should move to  the event handler once multithreaded 
                PublishProgress = 100;
                //base.GenerateSite();
                return null;
            }), null);


			//	Sort and Write logs.
			//Task.Run(() =>
			//{
			//	var orderedLogs = PublishLogs.OrderBy(log => log.RegionSortText);
			//	orderedLogs.ForEach(log =>
			//	{
			//		base.Logger.Write(
			//			log.Message, 
			//			log.MessageType == PubishMessageTypeEnum.Warning ? Category.Warn :
			//			log.MessageType == PubishMessageTypeEnum.Error ? Category.Exception :
			//			Category.Info,
			//			Priority.High);
			//	});

			//});


			Refresh();
        }

        private bool CheckEnviroment(string availableDiskSpace, string availableVirtualMemory)
        {
            double diskSpace = 15.0;
            double memorySpace = 4.0;
            var diskspaceUnit = availableDiskSpace.Split(' ')[1];
            var memorySpaceUnit = availableVirtualMemory.Split(' ')[1];

            double.TryParse(availableDiskSpace.Split(' ')[0], out diskSpace);
            double.TryParse(availableVirtualMemory.Split(' ')[0], out memorySpace);

            IList<int> EDDatasetIds = new List<int>();
            int edRowCount = 0;
            if (CurrentWebsite.Datasets.Any(d => d.Dataset.ContentType.Name.ContainsCaseInsensitive("ED Treat And Release")))
            {
                EDDatasetIds = CurrentWebsite.Datasets.Where(d => d.Dataset.ContentType.Name.ContainsCaseInsensitive("ED Treat And Release"))
                                                      .Select(d => d.Dataset.Id)
                                                      .ToList();

                using (var session = this.DataserviceProvider.SessionFactory.OpenStatelessSession())
                    edRowCount = session.CreateSQLQuery(string.Format("select count([id]) 'Count' from Targets_TreatAndReleaseTargets where dataset_id in ({0});",
                                                                      string.Join(",", EDDatasetIds))).UniqueResult<int>();
            }

            IList<int> IPDatasetIds = new List<int>();
            var ipRowCount = 0;
            if (CurrentWebsite.Datasets.Any(d => d.Dataset.ContentType.Name.ContainsCaseInsensitive("Inpatient Discharge")))
            {
                IPDatasetIds = CurrentWebsite.Datasets.Where(d => d.Dataset.ContentType.Name.ContainsCaseInsensitive("Inpatient Discharge"))
                                                      .Select(d => d.Dataset.Id)
                                                      .ToList();

                using (var session = this.DataserviceProvider.SessionFactory.OpenStatelessSession())
                    ipRowCount = session.CreateSQLQuery(string.Format("select count([id]) 'Count' from Targets_InpatientTargets where dataset_id in ({0});",
                                                                      string.Join(",", IPDatasetIds))).UniqueResult<int>();
            }

            if (!EDDatasetIds.Any() && !IPDatasetIds.Any()) return false;

            var totalCount = (double) ipRowCount + (double) edRowCount;


            double amountNeededForTempFiles = (totalCount / 1000000) * 8000;
            var calculatedAmount = Math.Round(amountNeededForTempFiles / 1000, 2);

            var message = new List<string>
            {
               diskspaceUnit != "GB" || ( calculatedAmount > diskSpace && diskspaceUnit == "GB")? string.Format(AVAILABLE_SPACE_MESSAGE, Environment.NewLine, calculatedAmount <= 1 ? 1 : calculatedAmount) : string.Empty,
               memorySpaceUnit != "GB" ||  ( memorySpace < MINIMUM_VIRTUAL_SPACE_REQUIREMENT && memorySpaceUnit == "GB")? string.Format(AVAILABLE_MEMORY_MESSAGE, Environment.NewLine) : string.Empty
            };

            if (message.All(string.IsNullOrEmpty)) return false;

            var finalMessage = string.Join(Environment.NewLine, message) + Environment.NewLine + "Please click “Ok” to continue and “Cancel” to stop the generation.";

            return MessageBox.Show(finalMessage, "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel;
        }

        public void ReviewSite()
        {
            OutPutWebSiteFolder = CurrentWebsite.OutPutDirectory;
            //ChangeWebSiteStatus(WebsiteState.Published);
            ManageViewModel.ChangeWebsiteStatus(WebsiteState.Published);

            ResultsVisibility = Visibility.Hidden;
            DependencyCheckControl = Visibility.Hidden;
            PublishLogVisibility = Visibility.Hidden;
            ReviewVisibility = Visibility.Visible;
            DependencyCheckControl = Visibility.Hidden;
            //base.ReviewSite();

        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            Index = 5;
        }

        private static Tuple<string, DateTime> GetUIMessage(string message)
        {
            return new Tuple<string, DateTime>(message, DateTime.Now);
        }



        #endregion

        #region Website Publish Validation

	    [ImportMany(typeof(IWebsiteDependencyCheck))] private IWebsiteDependencyCheck[] dependencyChecks;

        public void RunDependencyCheck()
        {
            SetToRunDependencyCheckMode();
            AutoSave();
            DependencyCheckResults = Enumerable.Empty<ValidationResultViewModel>(); // start fresh 
            ValidationFactory = new ValidationResultViewModelFactory(ManageViewModel.WebsiteViewModel);
            IsDependencyCheckRunning = true;
            ResultsVisibility = Visibility.Visible;
            ShowRunCheckSuccessMessage = Visibility.Collapsed;
            ShowMessage = Visibility.Visible;
            UpdateUI();
           // Thread.Sleep(5000);
            try
            {

                var results = new List<ValidationResultViewModel>();
                ValidateStateNotSelected().ForEach(results.Add); // TODO add comment
                ValidateDatasetsMissing().ForEach(results.Add); // TODO add comment
                ValidateMeasuresMissing().ForEach(results.Add); // TODO add comment
                ValidateReportsMissing().ForEach(results.Add); // TODO add comment
                ValidateOutputFolder().ForEach(results.Add); // TODO add comment
                ValidateAboutUsContent().ForEach(results.Add); // TODO add comment
                //ValidateCmsProviderIdUnmapped().ForEach(results.Add); // TODO add comment
                ValidateCostToChargeUndefined().ForEach(results.Add); // TODO add comment
                ValidateHospitalsMissingRegions().ForEach(results.Add); // TODO add comment
                //ValidateStaleBaseData().ForEach(results.Add); // TODO add comment
                ValidateDataYearMismatchForInpatientDischargeFile().ForEach(results.Add);
                ValidateDataYearMismatchForEmergencyDepartmentFile().ForEach(results.Add);
                ValidateDatasetMappingForCounty().ForEach(results.Add);
                ValidateEmergencyDepartmentTreatAndReleaseReportEdServices().ForEach(results.Add);
                ValidateEmergencyDepartmentTreatAndReleaseReportIpDataSet().ForEach(results.Add);
                ValidateNoMeasuresForTheDataSet().ForEach(results.Add);
                ValidateCustomRegionToPopulationMapping().ForEach(results.Add);
                ValidatePopualtionRegions().ForEach(results.Add);
                //ValidateForLocalHospIdAndCmsProvIdForCostCal().ForEach(results.Add);
                ValidateCustomRegionDefinition().ForEach(results.Add);
                ValidateAhrqQiDbConnection().ForEach(results.Add);
                ValidateBothCustomRegionsAndPopulationStatsDefinition().ForEach(results.Add);
                //ValidateHospitalProfileReport().ForEach(results.Add);
                ValidateForHospitalProfileReport().ForEach(results.Add);
                // Run validation for Hospital Profile Report Jason
                ValidateUnMappedHopitalLocalId().ForEach(results.Add);
                ValidateCostQualityReport().ForEach(results.Add);
                ValidateCustomHeatMap().ForEach(results.Add);
                ValidateHospitalMapping().ForEach(results.Add);
                ValidateCountyFips().ForEach(results.Add);
                ValidateRegionIds().ForEach(results.Add);
                ValidateEDDischargeQuarter().ForEach(results.Add);
                ValidateTrendingQuarters().ForEach(results.Add);
                ValidatePhysicianHedisDataset().ForEach(results.Add);
				ValidateRealtimePhysicianSubReports().ForEach(results.Add);

				ValidateCostQualityAllFamilySelected().ForEach(results.Add);
                ValidateCostQualityQiDbConnection().ForEach(results.Add);
                
                dependencyChecks?.SelectMany(c => c.Check(this.CurrentWebsite)).ForEach(results.Add);

                DependencyCheckResults = results.Where(r => r.Quality != ValidationLevel.Success);
                ReviewWarning = string.Join(Environment.NewLine,
                    results.Where(r => r.Quality == ValidationLevel.Warning).Select(r => r.Message));
                ShowMessage = Visibility.Collapsed;
                if (results.All(r => r.Quality != ValidationLevel.Error)) // No Error ! , we can move to next step 
                {
                    ManageViewModel.ChangeWebsiteStatus(WebsiteState.CompletedDependencyCheck);
                    PublishProgress = 50;
                }
              
                if (DependencyCheckResults.Count() == 0)
                {
                    ResultsVisibility = Visibility.Hidden;
                    ShowRunCheckSuccessMessage = Visibility.Visible;
                }              
                LogDependencyChecks(results);
            }
            catch (Exception exception)
            {
                if (!IsExiting)
                {
                    Logger.Write(exception.GetBaseException());
                    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(exception.GetBaseException());
                }
            }

            finally
            {
                SetToCompletedDependencyCheckMode();
                ShowMessage = Visibility.Collapsed;
            }
        }

        public static void UpdateUI()
        {
            DispatcherFrame frame = new DispatcherFrame();

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
        }
        
        private List<ValidationResultViewModel> ValidatePhysicianHedisDataset()
        {
            var result = new List<ValidationResultViewModel>();
            var hedisDataSet = CurrentWebsite.Datasets.FirstOrDefault(x => x.Dataset.ContentType.Name.ContainsIgnoreCase("Medical Practice HEDIS Measures Data"));
            var isDatasetIncluded = hedisDataSet != null;
            if (isDatasetIncluded && CurrentWebsite.Measures.Any(x => x.ReportMeasure.Source != null && x.ReportMeasure.Source.Contains("Medical Practice HEDIS")))
            {
                using (var session = DataserviceProvider.SessionFactory.OpenSession())
                {
                    var query = string.Format(@"SELECT TOP 1 case when count(PhyNpi) > 1 then 0 else 1 end IsValid
                                  FROM Targets_PhysicianHEDISTargets
                                  WHERE Dataset_Id = {0}
                                  GROUP BY PhyNpi
                                  ORDER BY count(PhyNpi) desc", hedisDataSet.Dataset.Id);


                    var isValid = Convert.ToBoolean(session.CreateSQLQuery(query).UniqueResult<int>());
                    if (!isValid)
                        result.Add(ValidationFactory.BuildResult(ValidationOutcome.InValidHedisDataset));
                }
            }

            if (isDatasetIncluded && CurrentWebsite.Datasets.All(x => !x.Dataset.ContentType.Name.ContainsIgnoreCase("Physician Data")))
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.MissingPhysicianReport));
                return result;
            }

            return result;
        }
		private List<ValidationResultViewModel> ValidateRealtimePhysicianSubReports()
		{
            var result = new List<ValidationResultViewModel>();
			var physDatasets = CurrentWebsite.Datasets.Where(x => x.Dataset.ContentType.Name.EqualsIgnoreCase("Physician Data")).ToList();
			var subPhysDatasets = CurrentWebsite.Datasets.Where(x => x.Dataset.ContentType.Name.EqualsAnyIgnoreCase("Medical Practice HEDIS Measures Data", "CG-CAHPS Survey Results Data")).ToList();
			var isPhysDatasetsIncluded = physDatasets != null && physDatasets.Count > 0;
			var isAllPhysDatasetsUsingRealtime = physDatasets.All(pds => pds.Dataset.UseRealtimeData);
			var isSubPhysDatasetsIncluded = subPhysDatasets != null && subPhysDatasets.Count > 0;

			if (isPhysDatasetsIncluded && isAllPhysDatasetsUsingRealtime && isSubPhysDatasetsIncluded)
			{
				var subReportNames =
					subPhysDatasets.Select(ds =>
					{
						switch (ds.Dataset.ContentType.Name.ToUpper())
						{
							case "MEDICAL PRACTICE HEDIS MEASURES DATA": return "Medical Group's HEDIS";
							case "CG-CAHPS SURVEY RESULTS DATA": return "CG-CAHPS survey";
							default: return "";
						}
					}).ToList();
				result.Add(ValidationFactory.BuildResult(
					ValidationOutcome.RealtimePhysicianDataCannotHaveSubReports,
					null,
					string.Join(" and ", subReportNames)));
				return result;
			}
			return result;
		}

		private List<ValidationResultViewModel> ValidateDatasetMappingForCounty()
        {
            var result = new List<ValidationResultViewModel>();
            var ipDatasetIds = CurrentWebsite.Datasets.Where(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge")).Select(wd => wd.Dataset.Id).ToList();

            if (!ipDatasetIds.Any()) return result;

            var hqlQuery = string.Format("select count(*) from Targets_InpatientTargets where Dataset_Id in ({0}) and PatientStateCountyCode is not null", string.Join(",", ipDatasetIds));

            if (!WebsiteDataService.IsDatasetHasCountyData(hqlQuery))
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.CheckDatasetMappingForCounty));

            return result;
        }

        private List<ValidationResultViewModel> ValidateEmergencyDepartmentTreatAndReleaseReportEdServices()
        {
            var result = new List<ValidationResultViewModel>();
            var edDataset = CurrentWebsite.Datasets.FirstOrDefault(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("ED Treat And Release"));

            if (edDataset == null || !CurrentWebsite.Datasets.Any(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge")))
                return result;

            var ipDatasetIds = CurrentWebsite.Datasets.Where(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge")).Select(wd => wd.Dataset.Id).ToList();
            var hqlQuery = string.Format("select count(*) from Targets_InpatientTargets where Dataset_Id in ({0}) and EDServices is not null;", string.Join(",", ipDatasetIds));

            if (WebsiteDataService.ExecuteScalar<int>(hqlQuery) == 0)
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.EmergencyDepartmentTreatAndReleaseReportEDServices));
            return result;
        }

        private List<ValidationResultViewModel> ValidateEmergencyDepartmentTreatAndReleaseReportIpDataSet()
        {
            var result = new List<ValidationResultViewModel>();
            // var idDataset = CurrentWebsite.Datasets.FirstOrDefault(d => d.Dataset.ContentType.Name.Equals("ED Treat And Release"));

            if (CurrentWebsite.Reports.Any(r => r.Report.Datasets.Any(d => d.Equals("ED Treat And Release"))) &&
                CurrentWebsite.Datasets.Any(d => d.Dataset.ContentType.Name.Equals("Inpatient Discharge")) == false)
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.EmergencyDepartmentTreatAndReleaseReportIPDataSet));
            }
            return result;
        }

        private List<ValidationResultViewModel> ValidateForHospitalProfileReport()
        {
            var result = new List<ValidationResultViewModel>();
            var websiteReport = CurrentWebsite.Reports.FirstOrDefault(d => d.Report.ReportType.EqualsIgnoreCase("Hospital Profile Report"));

            if (websiteReport != null)
            {
                var displayFilter = websiteReport.Report.Filters.FirstOrDefault(rf => rf.Type == ReportFilterTypeEnum.Display);
                if (displayFilter != null)
                {
                    var filtersValueToCheck = displayFilter.Values.Where(fv => fv.Name.In(new[] { "Cost and Charge Data (Medicare)", "Cost and Charge Data (All Patients)" })).ToList();
                    if (filtersValueToCheck.Any())
                    {
                        var filter = filtersValueToCheck.SingleOrDefault(fv => fv.Value);
                        if (filter != null)
                        {
                            if (filter.Name.EqualsIgnoreCase("Cost and Charge Data (Medicare)"))
                            {
                                if (!CurrentWebsite.Datasets.Any(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("Medicare Provider Charge Data")))
                                    result.Add(ValidationFactory.BuildResult(ValidationOutcome.HospitalProfileMissingMedicareDataset));

                                if (CurrentWebsite.Hospitals != null && CurrentWebsite.Hospitals.Any())
                                {
                                    if (CurrentWebsite.Hospitals.Any(wHosp => string.IsNullOrEmpty(wHosp.Hospital.LocalHospitalId) || string.IsNullOrEmpty(wHosp.Hospital.CmsProviderID)))
                                    {
                                        result.Add(ValidationFactory.BuildResult(ValidationOutcome.HospitalProfileMissingLocalHospIdOrProviderId, "", websiteReport.Report.Name));
                                    }
                                }
                            }
                            else if (filter.Name.EqualsIgnoreCase("Cost and Charge Data (All Patients)"))
                            {
                                if (!CurrentWebsite.Datasets.Any(d => d.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge")))
                                    result.Add(ValidationFactory.BuildResult(ValidationOutcome.HospitalProfileMissingIPDataset));
                            }
                        }
                    }
                }
            }

            return result;
        }

        //private List<IValidationResultViewModel> ValidateForLocalHospIdAndCmsProvIdForCostCal() 
        //{
        //    var result = new List<IValidationResultViewModel>();
        //    var websiteReport = CurrentWebsite.Reports.FirstOrDefault(d => d.Report.ReportType.EqualsIgnoreCase("Hospital Profile Report"));

        //    if (websiteReport != null)
        //    {
        //        if (CurrentWebsite.Hospitals != null && CurrentWebsite.Hospitals.Any())
        //        {
        //            if(CurrentWebsite.Hospitals.Any(wHosp => string.IsNullOrEmpty(wHosp.Hospital.LocalHospitalId) || string.IsNullOrEmpty(wHosp.Hospital.CmsProviderID)))
        //                {
        //                    result.Add(ValidationFactory.BuildResult(ValidationOutcome.HospitalProfileMissingLocalHospIdOrProviderId));
        //                } 

        //        }
        //    }

        //    return result;
        //}

        private List<ValidationResultViewModel> ValidateStateNotSelected()
        {
            var result = new List<ValidationResultViewModel>();
            if (!ManageViewModel.WebsiteViewModel.HasState())
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.StateNotSelected));
            }
            return result;
        }

        private List<ValidationResultViewModel> ValidateDataYearMismatchForInpatientDischargeFile()
        {
            const String datasetName = "Inpatient Discharge";       // (IPDis)
            return ValidateDataYearMismatchForXFile(datasetName, datasetName, datasetName);
        }

        private List<ValidationResultViewModel> ValidateDataYearMismatchForEmergencyDepartmentFile()
        {
            const String datasetName = "ED Treat And Release";      // (EDTaR)
            return ValidateDataYearMismatchForXFile(datasetName, "Emergency Department treat-and-release", "Emergency Department");
        }

        private List<ValidationResultViewModel> ValidateDataYearMismatchForXFile(String datasetName, String datasetAlias1, String datasetAlias2)
        {
            var results = new List<ValidationResultViewModel>();

            // Obtain all EDTaR datasets.
            // Obtain any reports associated with a EDTaR dataset.
            // Check if any obtained reports are Trending.
            var edDatasets = CurrentWebsite.Datasets.Where(d => d.Dataset.ContentType.Name.Equals(datasetName));
            var edtarReports = CurrentWebsite.Reports.Where(r => r.Report.Datasets.Any(d => d.Equals(datasetName)));
            var hasTrendingReport = edtarReports.Any(r => r.Report.IsTrending);

            // If any trending EDTaR report is selected, only 1 of the selected EDTaR datasets need to be in website year.
            // Otherwise, all Datasets need to be in website year.
            if (hasTrendingReport)
            {
                if (!edDatasets.Any(d => d.Dataset.ReportingYear.Equals(CurrentWebsite.ReportedYear)))
                {
                    var result = (ValidationResultViewModel)ValidationFactory.BuildResult(ValidationOutcome.DataYearMismatchForTrendingDataSetFile);
                    result.Message = String.Format(result.Message, datasetAlias1, datasetAlias2);
                    results.Add(result);
                }
            }
            else
            {
                if (!edDatasets.All(d => d.Dataset.ReportingYear.Equals(CurrentWebsite.ReportedYear)))
                {
                    var result = (ValidationResultViewModel)ValidationFactory.BuildResult(ValidationOutcome.DataYearMismatchForDataSetFile);
                    result.Message = String.Format(result.Message, datasetAlias1, datasetAlias2);
                    results.Add(result);
                }
            }

            return results;
        }

        //private List<IValidationResultViewModel> ValidateStaleBaseData()
        //{
        //    var result = new List<IValidationResultViewModel>();
        //    if (ManageViewModel.WebsiteViewModel.Website.Datasets.Any())
        //    {
        //        foreach (var ds in CurrentWebsite.Datasets)
        //        {
        //            if (CurrentWebsite.ReportedYear != ds.Dataset.ReportingYear)
        //            {
        //                result.Add(ValidationFactory.BuildResult(ValidationOutcome.StaleBaseData, ds.Dataset.ReportingYear, "-----"));
        //            }
        //        }
        //    }
        //    return result;
        //}

        private List<ValidationResultViewModel> ValidateReportsMissing()
        {
            var result = new List<ValidationResultViewModel>();
            if (!CurrentWebsite.Reports.Any())
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.ReportsMissing));
            }
            return result;
        }

        private List<ValidationResultViewModel> ValidateOutputFolder()
        {
            var result = new List<ValidationResultViewModel>();
            var outputDIr = CurrentWebsite.OutPutDirectory;
            if (Directory.Exists(outputDIr)) return result;

            if (!string.IsNullOrEmpty(outputDIr) && !Directory.Exists(Directory.GetParent(outputDIr).FullName))
                Directory.CreateDirectory(Directory.GetParent(outputDIr).FullName);

            if (outputDIr == null || !Directory.Exists(Directory.GetParent(outputDIr).FullName))
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.OutputFolder));
            }
            else
            {
                string dirname = GetFolderName(new DirectoryInfo(outputDIr).Name);
                Directory.CreateDirectory(Path.Combine(Directory.GetParent(outputDIr).FullName, dirname));
            }

            return result;
        }

        //private List<IValidationResultViewModel> ValidateWebsiteName()
        //{
        //    var result = new List<IValidationResultViewModel>();
        //    if (string.IsNullOrWhiteSpace(CurrentWebsite.Name))
        //    {
        //        result.Add(ValidationFactory.BuildResult(ValidationOutcome.OutputFolder));
        //    }
        //    return result;
        //}

        private List<ValidationResultViewModel> ValidateMeasuresMissing()
        {
            var result = new List<ValidationResultViewModel>();

            // *******************************************************************
            // Temporary check to allow us to bypass this dependency check if only. Jason
            // *******************************************************************
            // - variable 'reportsSansMeasures' holds Reports that do not have any defined Measures.
            // - logic is that if only Reports without any defined Measures are selected (i.e. Total Measure count == reportsSansMeasures count),
            //	  then we shouldn't expect Measures;  Otherwise, fail if no Measures selected.
            // *******************************************************************
            var reportsSansMeasures = CurrentWebsite.Reports.Where(wr =>
                                                        wr.Report.Datasets.Any(d => d.EqualsIgnoreCase("Physician Data")) ||
                                                        wr.Report.SourceTemplate.Name.EqualsIgnoreCase("Hospital Profile Report") ||
                                                        wr.Report.IsCustom)
                                                          .Select(wr => wr.Report).ToList();

            if (reportsSansMeasures.Any() && reportsSansMeasures.Count == ManageViewModel.WebsiteViewModel.Website.Reports.Count)
            {
                return result;
            }

            if (!CurrentWebsite.Measures.Any())
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.MeasuresMissing));
            }
            return result;
        }

        private List<ValidationResultViewModel> ValidateNoMeasuresForTheDataSet()
        {
            var result = new List<ValidationResultViewModel>();

            foreach (var wd in CurrentWebsite.Datasets)
            {
                if (wd.Dataset.ContentType.Name.StartsWith("Physician", StringComparison.InvariantCultureIgnoreCase) || wd.Dataset.ContentType.IsCustom) continue;

                if (!CurrentWebsite.Measures.Any(m => m.OriginalMeasure.Owner.Name.Equals(wd.Dataset.ContentType.Name) && m.IsSelected))
                    result.Add(ValidationFactory.BuildResult(ValidationOutcome.NoMeasuresForTheDataSet, datasetName: wd.Dataset.ContentType.Name));
            }

            return result;
        }

        private List<ValidationResultViewModel> ValidateHospitalsMissingRegions()
        {
            var result = new List<ValidationResultViewModel>();
            if (!ManageViewModel.WebsiteViewModel.HasRegions())
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.HospitalsMissingRegions));
            }
            return result;
        }

        private List<ValidationResultViewModel> ValidateDatasetsMissing()
        {
            var result = new List<ValidationResultViewModel>();
            if (!CurrentWebsite.Datasets.Any())
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.DatasetsMissing));
            }
            return result;
        }

        private List<ValidationResultViewModel> ValidateCostToChargeUndefined()
        {
            var result = new List<ValidationResultViewModel>();
            if (!CurrentWebsite.Reports.Any()) return result;
            bool costToChargeMissing = ManageViewModel.WebsiteViewModel.HospitalMissingCostToCharge();

            if (costToChargeMissing)
            {
                var reportNames = CurrentWebsite.Reports.Where(r => r.Report.RequiresCostToChargeRatio)
                 .Select(s => s.Report.Name)
                 .ToList();
                if (!reportNames.Any()) return result;

                result.Add(
                    this.ValidationFactory.BuildResult(
                        ValidationOutcome.CostToChargeUndefined,
                        reportName: string.Join(", ", reportNames)));
            }

            return result;
        }

        //private List<IValidationResultViewModel> ValidateCmsProviderIdUnmapped()
        //{
        //    var result = new List<IValidationResultViewModel>();
        //    if (!ManageViewModel.WebsiteViewModel.IsMissingCmsProviderIds()) return result;

        //    foreach (var report in CurrentWebsite.Reports)
        //    {
        //        if (report.RequiresCmsId())
        //        {
        //            result.Add(ValidationFactory.BuildResult(ValidationOutcome.CmsProviderIdUnmapped, reportName: report.Report.Name));
        //        }
        //    }
        //    return result;
        //}

        private List<ValidationResultViewModel> ValidateAboutUsContent()
        {
            var result = new List<ValidationResultViewModel>();
            if (string.IsNullOrWhiteSpace(CurrentWebsite.AboutUsSectionText))
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.AboutUsContent));
            }
            return result;
        }

        private List<ValidationResultViewModel> ValidateCustomRegionToPopulationMapping()
        {
            var result = new List<ValidationResultViewModel>();
            if (!ManageViewModel.WebsiteViewModel.HasCustomRegionSelected()) return result;

            if (!CurrentWebsite.Datasets.Any(x => x.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge"))) return result;

            var isRegionalReportSelected = CurrentWebsite != null && CurrentWebsite.Reports.Count(x => (x.Report.ReportType.ContainsIgnoreCase("Region Rates Detail Report")
                || (x.Report.Name.ContainsIgnoreCase("Region Rates Report"))
                || (x.Report.Name.ContainsIgnoreCase("Region Rates Trending Report")))) > 0;

            if (!isRegionalReportSelected) return result;

            var customRegionInfo = ManageViewModel.WebsiteViewModel.WebsiteDataService.GetCustomRegionToPopulationMappingInfo();

            //if (customRegionInfo.Item1 == 0 && customRegionInfo.Item2 == 0)
            //    result.Add(ValidationFactory.BuildResult(ValidationOutcome.MissingCustomRegionAndPopulationMapping));
            if (customRegionInfo.Item1 > 0 && customRegionInfo.Item2 == 0) result.Add(ValidationFactory.BuildResult(ValidationOutcome.CustomRegionMissingPopulationCount));
            if (customRegionInfo.Item1 == 0 && customRegionInfo.Item2 > 0) result.Add(ValidationFactory.BuildResult(ValidationOutcome.MissgingCustomRegion));

            return result;
        }

        private List<ValidationResultViewModel> ValidatePopualtionRegions()
        {
            var result = new List<ValidationResultViewModel>();
            var regionType = string.Empty;

            if (MonahrqContext.SelectedRegionType == typeof(HealthReferralRegion))
                regionType = "HRRRegionId";
            else if (MonahrqContext.SelectedRegionType == typeof(HospitalServiceArea))
                regionType = "HSAREgionId";
            else
                regionType = "CustomRegionId";

            bool isIPDatasetIncluded = CurrentWebsite.Reports.Any(x => x.Report.Datasets.Any(d => d.Contains("Inpatient Discharge")) && x.Report.Name.Contains("Region Rates"));

            if (!isIPDatasetIncluded || WebsiteDataService.HasImportedRegionId(regionType, ManageViewModel.WebsiteViewModel.Website.Datasets[0].Dataset.Id)) return result;

            result.Add(ValidationFactory.BuildResult(ValidationOutcome.PopulationMissingRegions));

            return result;
        }

        private List<ValidationResultViewModel> ValidateCustomRegionDefinition()
        {
            var results = new List<ValidationResultViewModel>();

            if (CurrentWebsite.Datasets.All(x => x.Dataset.ContentType.Name.Contains("Nursing Home Compare Data") || x.Dataset.ContentType.Name.Contains("Physician Data"))) return results;

            if (CurrentWebsite.RegionTypeContext.EqualsIgnoreCase(typeof(CustomRegion).Name) && !WebsiteDataService.IsCustomRegionDefined())
                results.Add(ValidationFactory.BuildResult(ValidationOutcome.CustomRegionNotDefined));

            return results;
        }

        private List<ValidationResultViewModel> ValidateBothCustomRegionsAndPopulationStatsDefinition()
        {
            var results = new List<ValidationResultViewModel>();

            bool isIPDatasetIncluded = CurrentWebsite.Reports.Any(x => x.Report.Datasets.Any(d => d.Contains("Inpatient Discharge")) && (x.Report.Name.Contains("Region Rates")));

            if (isIPDatasetIncluded)
            {
                var ipDatasets = CurrentWebsite.Datasets.Where(ds => ds.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge"))
                                                        .Select(d => d.Dataset.Id)
                                                        .ToList();

                if (!WebsiteDataService.CheckIfCustomRegionsDefined(ipDatasets, ManageViewModel.WebsiteViewModel.Website.StateContext.ToList(), ManageViewModel.WebsiteViewModel.Website.RegionTypeContext))
                    results.Add(ValidationFactory.BuildResult(ValidationOutcome.CustomRegionOrCustomPopulationNotDefined));

                if (!WebsiteDataService.CheckIfCustomRegionsMatchLibrary(ipDatasets, ManageViewModel.WebsiteViewModel.Website.StateContext.ToList(), ManageViewModel.WebsiteViewModel.Website.RegionTypeContext))
                    results.Add(ValidationFactory.BuildResult(ValidationOutcome.CustomRegionAndIPCustomRegionNotMatched));
            }
            return results;
        }

        private List<ValidationResultViewModel> ValidateAhrqQiDbConnection()
        {
            var result = new List<ValidationResultViewModel>();
            var connectionString = ConfigurationService.WinQiConnectionSettings;
            if (!CurrentWebsite.Reports.Any(x => x.Report.Name.Contains("Avoidable"))) return result;

            try
            {
                if (connectionString == null || string.IsNullOrEmpty(connectionString.ConnectionString))
                    result.Add(ValidationFactory.BuildResult(ValidationOutcome.AhrqQiDbConnection));
                else
                    using (var con = new SqlConnection(connectionString.ConnectionString))
                    {
                        con.Open();
                    }
            }
            catch (Exception)
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.AhrqQiDbConnection));
            }
            return result;
        }

        //private List<IValidationResultViewModel> ValidateHospitalProfileReport()
        //{
        //    var results = new List<IValidationResultViewModel>();

        //    if (!ManageViewModel.WebsiteViewModel.Website.Reports.Any(x => x.Report.Name.Contains("Hospital Profile")) ||
        //        !ManageViewModel.WebsiteViewModel.Website.Datasets.Any(x => x.Dataset.ContentType.Name.Equals("Inpatient Discharge"))) return results;

        //    if (ManageViewModel.WebsiteViewModel.Website.Datasets.Any(x => x.Dataset.ContentType.Name.Equals("Medicare Provider Charge Data"))) return results;

        //    results.Add(ValidationFactory.BuildResult(ValidationOutcome.MissingMedicareDataset));

        //    return results;
        //}

        private List<ValidationResultViewModel> ValidateUnMappedHopitalLocalId()
        {
            var result = new List<ValidationResultViewModel>();

            var datasets = _qiMeasuresDatasetsRequiringLocalHospitalId.Union(_datasetsRequiringLocalHospitalId);
            if (!CurrentWebsite.Datasets.Any(x => datasets.Contains(x.Dataset.ContentType.Name))) return result;

            if (CurrentWebsite.Hospitals.Any(x => string.IsNullOrEmpty(x.Hospital.LocalHospitalId)))
            {
                CurrentWebsite.Datasets.ForEach(x =>
                    {
                        if (_qiMeasuresDatasetsRequiringLocalHospitalId.Contains(x.Dataset.ContentType.Name))
                        {
                            result.Add(ValidationFactory.BuildResult(ValidationOutcome.QIUnMappedHospitalLocalId, "", "", x.Dataset.ContentType.Name));
                        }
                    });

                ManageViewModel.WebsiteViewModel.Website.Reports.ForEach(r =>
                    {
                        if (r.Report.Datasets.Any(x => _datasetsRequiringLocalHospitalId.Contains(x)) &&
                            (r.Report.Name.Contains("County Rates") || r.Report.Name.Contains("Region Rates") || r.Report.Name.Contains("ED Utilization Detailed Report")
                            || r.Report.Name.Contains("Inpatient Hospital Discharge Utilization Report") || r.Report.Name.Contains("Inpatient Utilization Detail Report")
                            || r.Report.Name.Contains("Emergency Department Treat-and-Release (ED) Utilization Report")))
                        {
                            result.Add(ValidationFactory.BuildResult(ValidationOutcome.UnMappedHospitalLocalId, "", "", r.Report.Name));
                        }
                    });
            }
            return result;
        }

        private List<ValidationResultViewModel> ValidateCostQualityReport()
        {
            var result = new List<ValidationResultViewModel>();

            if (CurrentWebsite.Reports.All(x => !x.Report.ReportType.EqualsIgnoreCase("Cost and Quality Report – Side By Side Display"))) return result;

            //if (!CurrentWebsite.Measures.Any(m => (m.OriginalMeasure != null && m.OriginalMeasure.SupportsCost)
            //                              || (m.OverrideMeasure != null && m.OverrideMeasure.SupportsCost)))
            if (!CurrentWebsite.Measures.Any(m => m.ReportMeasure != null && m.ReportMeasure.SupportsCost))
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.MeasuresDoNotSupportCost));
            }

            if (CurrentWebsite.Datasets.All(d => !d.Dataset.ContentType.Name.Contains("AHRQ-QI Provider Data")))
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.CostQualityQiFileNotImported));
            }

            if (CurrentWebsite.Datasets.All(d => !d.Dataset.ContentType.Name.Contains("Inpatient Discharge")))
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.CostQualityIpFileNotImported));
            }

            return result;
        }

        private List<ValidationResultViewModel> ValidateCustomHeatMap()
        {
            var result = new List<ValidationResultViewModel>();

            if (CurrentWebsite.RegionTypeContext != typeof(CustomRegion).Name || CurrentWebsite.Datasets.All(x => !x.Dataset.ContentType.Name.Contains("Inpatient Discharge"))) return result;

            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                var customHopitalCount = session.CreateCriteria<Hospital>()
                       .Add(Restrictions.Eq("IsArchived", false))
                       .Add(Restrictions.Eq("IsDeleted", false))
                    .Add(Restrictions.IsNotNull(string.Format("{0}", CurrentWebsite.RegionTypeContext)))
                    .Add(Restrictions.In("State", CurrentWebsite.SelectedReportingStates))
                       .SetProjection(Projections.Count(Projections.Id()))
                       .FutureValue<int>();

                var customPopulationCount = session.CreateCriteria<RegionPopulationStrats>()
                    .Add(Restrictions.Eq("RegionType", 0))
                       .SetProjection(Projections.Count(Projections.Id()))
                       .FutureValue<int>();

                var customRegionCount = session.CreateCriteria<CustomRegion>()
                    .SetProjection(Projections.Count(Projections.Id()))
                    .FutureValue<int>();

                if (customRegionCount.Value != 0 && customPopulationCount.Value != 0 && customHopitalCount.Value != 0)
                {
                    result.Add(ValidationFactory.BuildResult(ValidationOutcome.CustomHeatMap));
                }
            }

            return result;
        }

        private List<ValidationResultViewModel> ValidateHospitalMapping()
        {
            var result = new List<ValidationResultViewModel>();
            var regionType = string.IsNullOrEmpty(CurrentWebsite.RegionTypeContext)
                    ? "CustomRegion"
                    : CurrentWebsite.RegionTypeContext;

            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                var customRegionCount = session.CreateCriteria<CustomRegion>()
                    .Add(Restrictions.In("State", CurrentWebsite.SelectedReportingStates))
                    .SetProjection(Projections.Count(Projections.Id()))
                    .FutureValue<int>();


                var mappedHospitalCount = session.CreateCriteria<Hospital>()
                    .Add(Restrictions.IsNotNull(regionType))
                    .Add(Restrictions.In("State", CurrentWebsite.SelectedReportingStates))
                    .SetProjection(Projections.Count(Projections.Id()))
                    .FutureValue<int>();

                if (customRegionCount.Value > 0 && mappedHospitalCount.Value == 0)
                {
                    result.Add(ValidationFactory.BuildResult(ValidationOutcome.UnMappdedHospitals));
                }
            }

            return result;
        }

        private IList<ValidationResultViewModel> ValidateRegionIds()
        {
            var result = new List<ValidationResultViewModel>();
            if (!CurrentWebsite.Datasets.Any(
               x => x.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge"))) return result;

            var regionContext = CurrentWebsite.RegionTypeContext;
            var columnName = string.IsNullOrEmpty(regionContext)
                             || regionContext.EqualsIgnoreCase(typeof(CustomRegion).Name)
                                 ? "CustomRegionID"
                                 : regionContext.EqualsIgnoreCase(typeof(HealthReferralRegion).Name)
                                       ? "HRRRegionID"
                                       : "HSARegionID";

            var query = string.Format(@"select count(case when {0} is null then 0 else 1 end) as InvalidRegionID 
                                       from Targets_InpatientTargets ip
                                       where not exists
                                           (
                                               select * 
                                               from Regions r
                                               where r.ImportRegionId = ip.{0}  
                                               and r.RegionType = '{1}'
                                           )", columnName, regionContext);

            using (var session = this.DataserviceProvider.SessionFactory.OpenSession())
            {
                if (session.CreateSQLQuery(query).UniqueResult<int>() > 0)
                {
                    result.Add(this.ValidationFactory.BuildResult(ValidationOutcome.InvalidRegionId));
                }
            }

            return result;
        }

        private IList<ValidationResultViewModel> ValidateCountyFips()
        {
            var result = new List<ValidationResultViewModel>();
            if (!CurrentWebsite.Datasets.Any(x => x.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge"))) return result;
            var query = string.Format(@"select COUNT(PatientStateCountyCode) AS InvalidCountyFipsCount
                                        from Targets_InpatientTargets ip 
                                        where not exists
                                        	(
                                        		select * 
                                        		from Base_Counties bc 
                                        		where bc.CountyFIPS = ip.PatientStateCountyCode and bc.State  in ('{0}')
                                            )", string.Join("','", CurrentWebsite.SelectedReportingStates));
            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                if (session.CreateSQLQuery(query).UniqueResult<int>() > 0)
                {
                    result.Add(ValidationFactory.BuildResult(ValidationOutcome.InvalidCountyFips));
                }
            }

            return result;
        }

        private IList<ValidationResultViewModel> ValidateEDDischargeQuarter()
        {
            var result = new List<ValidationResultViewModel>();
            //checkif included ED dataset has more than one quarter
            if (!CurrentWebsite.Datasets.Any(x => x.Dataset.ContentType.Name.EqualsIgnoreCase("ED Treat And Release"))) return result;

            //Check if any ED trending report is selected
            if (!CurrentWebsite.Reports.Any(x => x.Report.IsTrending && x.Report.Datasets.Contains("ED Treat And Release"))) return result;

            var datasetIds = CurrentWebsite.Datasets.Select(x => x.Dataset.Id);
            var edTargetTable = "Targets_TreatAndReleaseTargets";
            //Check if any of Discharge Quarter field is null for included datasets
            var query = string.Format(@";WITH TempQuartersCTE (DatasetId, QuartersCount)
                                         AS
                                         (
                                         	SELECT Dataset_Id, COUNT(DISTINCT DischargeQuarter) 
                                         	FROM {0}
                                         	WHERE Dataset_Id in ({1})
                                         	GROUP BY Dataset_Id 
                                         ), TempDischargeCTE (DasetId, QuartersCount, DischargeQuarter)
                                         as
                                         (
                                         	SELECT cte.DatasetId, cte.QuartersCount, DischargeQuarter 
                                         	FROM TempQuartersCTE cte
                                         		inner join Targets_TreatAndReleaseTargets t on cte.DatasetId = t.Dataset_Id
                                         	WHERE DischargeQuarter is null
                                         	GROUP BY  cte.DatasetId, cte.QuartersCount, DischargeQuarter 
                                         )
                                         
                                         SELECT COUNT(*) 
                                         FROM TempDischargeCTE
                                         WHERE QuartersCount > 0", edTargetTable, string.Join(",", datasetIds));

            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                var count = session.CreateSQLQuery(query)
                     .UniqueResult<int>();

                if (count > 0)
                    result.Add(ValidationFactory.BuildResult(ValidationOutcome.MissingDischargeQuarter));
            }

            return result;
        }

        private IList<ValidationResultViewModel> ValidateTrendingQuarters()
        {
            var result = new List<ValidationResultViewModel>();

            var reportSelectedYears = CurrentWebsite.Reports.Where(x => x.Report.IsTrending && x.SelectedYears != null).Select(x => x.SelectedYears);
            var invalidQuarter = false;

            foreach (var x in reportSelectedYears)
            {
                var period = x.FirstOrDefault(p => p.IsSelected) as TrendingYear;

                foreach (Period qrt in period.Quarters)
                {
                    var allQuarters = x.Where(p => p != period).Select(q => q.Quarters).ToList();

                    foreach (var quarter in allQuarters)
                    {
                        if (!quarter.Any(y => y.Text == qrt.Text && y.IsSelected == qrt.IsSelected))
                        {
                            invalidQuarter = true;
                            result.Add(ValidationFactory.BuildResult(ValidationOutcome.TrendingQuarters));
                            break;
                        }
                        if (invalidQuarter) break;
                    }
                    if (invalidQuarter) break;
                }
                if (invalidQuarter) break;
            }

            return result;
        }

        private List<ValidationResultViewModel> ValidateCostQualityAllFamilySelected()
        {
            var result = new List<ValidationResultViewModel>();
            if (!CurrentWebsite.Reports.Any(x => x.Report.ReportType.Contains("Cost and Quality"))) return result;
            if (!CurrentWebsite.Datasets.Any(x => x.Dataset.ContentType.Name.Contains("Inpatient Discharge"))) return result;
            if (!CurrentWebsite.Datasets.Any(x => x.Dataset.ContentType.Name.Contains("AHRQ-QI Provider Data"))) return result;


            try
            {
                var measureNames = CurrentWebsite.Measures.Where(m => m.IsSelected).Select(m => m.ReportMeasure.Name);

                using (var session = DataserviceProvider.SessionFactory.OpenSession())
                {
                    if (measureNames.ContainsAny("IQI 12", "IQI 12_QNTY", "IQI 12_COST") &&
                        !measureNames.ContainsAll("IQI 12", "IQI 12_QNTY", "IQI 12_COST"))
                    {
                        var measure = session.Query<Measure>().Where(m => m.Name == "IQI 12_COST").FirstOrDefault();
                        if (measure != null)
                        {
                            result.Add(
                                ValidationFactory.BuildResult(
                                    ValidationOutcome.CostQualityAllFamilySelected,
                                    reportName: measure.MeasureTopics[0].Topic.Name));
                        }
                    }
                    if (measureNames.ContainsAny("IQI 14", "IQI 14_QNTY", "IQI 14_COST") &&
                        !measureNames.ContainsAll("IQI 14", "IQI 14_QNTY", "IQI 14_COST"))
                    {
                        var measurex = session.Query<Measure>().Where(m => m.Name == "IQI 14_COST").FirstOrDefault();
                        if (measurex != null)
                        {
                            result.Add(
                                ValidationFactory.BuildResult(
                                    ValidationOutcome.CostQualityAllFamilySelected,
                                    reportName: measurex.MeasureTopics[0].Topic.Name));
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return result;
        }

        private List<ValidationResultViewModel> ValidateCostQualityQiDbConnection()
        {
            var result = new List<ValidationResultViewModel>();
            var connectionString = ConfigurationService.WinQiConnectionSettings;
            if (!CurrentWebsite.Reports.Any(x => x.Report.Name.Contains("Cost and Quality"))) return result;

            try
            {
                if (connectionString == null || string.IsNullOrEmpty(connectionString.ConnectionString))
                    result.Add(ValidationFactory.BuildResult(ValidationOutcome.CostQualityQiDbConnection));
                else
                    using (var con = new SqlConnection(connectionString.ConnectionString))
                    {
                        con.Open();
                    }
            }
            catch (Exception)
            {
                result.Add(ValidationFactory.BuildResult(ValidationOutcome.CostQualityQiDbConnection));
            }
            return result;
        }

        private void LogDependencyChecks(IEnumerable<ValidationResultViewModel> results)
        {
            Logger.Information("{0}{1} ------- {2} website Dependency Checks -------", Environment.NewLine, Environment.NewLine, CurrentWebsite.Name);
            Task.Run(() => results.ToList().ForEach(x => Logger.Write(x.Message)));
        }

        #endregion
    }
}
