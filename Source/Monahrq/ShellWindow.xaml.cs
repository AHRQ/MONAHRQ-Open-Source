using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Sdk.Regions;
using Monahrq.ViewModels;
using Monahrq.Views;
using Monahrq.Infrastructure;
using System.Windows.Media;
using Microsoft.Practices.Prism.Events;
using Monahrq.Sdk.Events;


namespace Monahrq
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    [Export(typeof(ShellWindow))]
    public partial class ShellWindow : IPartImportsSatisfiedNotification 
    {
        //// Imported by MEF
        //// The shell imports IModuleTracker once to record updates as modules are downloaded.        
        //[Import]
        //private IModuleTracker _moduleTracker;

        // The shell imports IModuleManager once to load modules on-demand.
        [Import]
        private IModuleManager _moduleManager;

        [Import]
        private SplashView SplashView {get;set;}

        [Import]
        public IRegionManager RegionManager { get; set; }

        [Import(LogNames.Session)]
        public ILogWriter SessionLogger
        {
            get;
            set;
        }

        [Import]
        public IEventAggregator EventManager { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellWindow"/> class.
        /// </summary>
        public ShellWindow()
        {
            InitializeComponent();
            Loaded += delegate
            {
                Application.Current.MainWindow = this;
                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                SplashView.Visibility = Visibility.Collapsed;

                if (MonahrqContext.ForceDbUpGrade || MonahrqContext.ForceDbRecreate)
                    RegionManager.RequestNavigate(RegionNames.MainContent,
                        new Uri("ManageSettingsView", UriKind.Relative));

                this.MonitorRegions(RegionManager.Regions);
                RegionManager.Regions.CollectionChanged += PrismRegionsChanged;
            };
        }

        /// <summary>
        /// Attach a navigation failed event handler to every region
        /// </summary>
        /// <param name="regions"></param>
        private void MonitorRegions(IEnumerable<IRegion> regions)
        {
            foreach (var region in regions)
            {
                Debug.WriteLine("RegionNavigationService: {0:X}", region.NavigationService.GetHashCode()); // prove that they're all unique
                var r = region;
                region.NavigationService.NavigationFailed += (s, e) =>
                {
                    var msg = $"Error navigating to view {e.NavigationContext.Uri} for region {r.Name}";
                    SessionLogger.Write(e.Error, msg);
                    EventManager.GetEvent<GenericNotificationEvent>().Publish(msg);
                };
                region.NavigationService.Navigating += (s, e) =>
                {
                    SessionLogger.Debug("Loading view: '{1}' for region '{0}'", r.Name,
                        e.NavigationContext.Uri);
                };
                region.NavigationService.Navigated += (s, e) =>
                {
                    SessionLogger.Debug("Loading complete: view '{1}' for region '{0}'", r.Name,
                        e.NavigationContext.Uri);
                };
            }
        }
        
        private void PrismRegionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.MonitorRegions(e.NewItems.OfType<IRegion>());
        }

        [Import]
        public ShellViewModel Model
        {
            get
            {
                return DataContext as ShellViewModel;
            }
            set
            {
                DataContext = value;
            }
        }
        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            // IPartImportsSatisfiedNotification is useful when you want to coordinate doing some work
            // with imported parts independent of when the UI is visible.

            //// I subscribe to events to help track module loading/loaded progress.
            //// The ModuleManager uses the Async Events Pattern.
            _moduleManager.LoadModuleCompleted += ModuleManager_LoadModuleCompleted;
            _moduleManager.ModuleDownloadProgressChanged += ModuleManager_ModuleDownloadProgressChanged;

         
        }

        /// <summary>
        /// Handles the LoadModuleProgressChanged event of the ModuleManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ModuleDownloadProgressChangedEventArgs"/> instance containing the event data.</param>
        private void ModuleManager_ModuleDownloadProgressChanged(object sender, ModuleDownloadProgressChangedEventArgs e)
        {
           // _moduleTracker.RecordModuleDownloading(e.ModuleInfo.ModuleName, e.BytesReceived, e.TotalBytesToReceive);
        }

        /// <summary>
        /// Handles the LoadModuleCompleted event of the ModuleManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LoadModuleCompletedEventArgs"/> instance containing the event data.</param>
        private void ModuleManager_LoadModuleCompleted(object sender, LoadModuleCompletedEventArgs e)
        {
          // _moduleTracker.RecordModuleLoaded(e.ModuleInfo.ModuleName);
        }

        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            if (regionManager != null)
            {
                var view = ServiceLocator.Current.GetInstance<AboutViewWindow>();
                if (!regionManager.Regions[RegionNames.Modal].Views.Contains(view))
                {
                    regionManager.Regions[RegionNames.Modal].Add(view);
                }
                regionManager.Regions[RegionNames.Modal].Activate(view);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            //if (_logger != null)
            //    _logger.Dispose();
        }

        private void btnWelcome_Click(object sender, RoutedEventArgs e)
        {
            var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            regionManager.RequestNavigate(RegionNames.MainContent, new Uri("WelcomeView", UriKind.Relative));
        }

		public SolidColorBrush NotificationBrush
		{
			get
			{
				switch ((DataContext as ShellViewModel).GenericNotificationType)
				{
					case Sdk.Events.ENotificationType.Info: return (SolidColorBrush)FindResource("MDGreen");
					case Sdk.Events.ENotificationType.Error: return (SolidColorBrush)FindResource("MPink");
					default: return (SolidColorBrush)FindResource("MDGreen");
				}
			}
		}
    }


}
