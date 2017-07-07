using System;
using System.ComponentModel.Composition;
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
        public ILoggerFacade SessionLogger
        {
            get;
            set;
        }

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
            };
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
        /// Logs the specified message.  Called by the CallbackLogger.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="category">The category.</param>
        /// <param name="priority">The priority.</param>
        public void Log(string message, Category category, Priority priority)
        {
            SessionLogger.Log(message, category, priority);
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
