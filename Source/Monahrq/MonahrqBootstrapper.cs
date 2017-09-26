using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Castle.Windsor;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Events;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Integration;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.FileSystem;
using Monahrq.Infrastructure.Modules;
using Monahrq.Infrastructure.Properties;
using Monahrq.Infrastructure.Utility;
using Monahrq.Resources;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.Extensibility;
using Monahrq.Sdk.Generators;
using Monahrq.Sdk.Logging;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Services.Import;
using Monahrq.Theme;
using Monahrq.Theme.Adapters;
using Monahrq.Theme.Behaviors;
using Monahrq.Theme.Controls;
using Monahrq.ViewModels;
using Monahrq.Views;
using NHibernate;
using NHibernate.Transform;
using System.Data.SqlClient;
using System.Windows.Threading;
using Monahrq.Infrastructure.Services;

//using LinqKit;

namespace Monahrq
{
    /// <summary>
    /// See Prism bootstrapper docs.
    /// </summary>
    public class MonahrqBootstrapper : MefBootstrapper
    {
        private readonly SessionLogger _sessionLogger = new SessionLogger(new CallbackLogger());
        private IConfigurationService _configurationService = new ConfigurationService();

        /// <summary>
        /// Initializes a new instance of the <see cref="MonahrqBootstrapper"/> class.
        /// </summary>
        public MonahrqBootstrapper()
        {
            RenameMyDocumentsDirectoryIfNeeded();

            // Abort the app if Access 2010 isn't installed
            if (!CheckOledbComponentsExist())
            {
                var msg = "OleDb components are not installed. MONAHRQ is shutting down.";
                _sessionLogger.Write(msg, TraceEventType.Critical);

                // Show the error window with URL where user can install it
                var window = new BootstrapErrorWindow();
                window.ShowDialog();

                // bypass further initialization
                Application.Current.Shutdown();

                // Force the program to quit without continuing the Prism bootstrapper sequence which loads NHibernate, etc.
                // This is usually not recommended, but in this case, we should not have any files open now,
                // and the program cannot continue without the database connection.
                //                Process.GetCurrentProcess().Kill();
                Environment.Exit(0);
            }

            _sessionLogger.Debug("Check for Oledb components succeeded.", TraceEventType.Information);

            var modulesPath = Path.GetFullPath("Modules");
            if (Directory.Exists(modulesPath))
            {
                AppDomain.CurrentDomain.AppendPrivatePath(modulesPath);
            }
        }
        
        /// <summary>
        /// Creates the shell or main window of the application.
        /// </summary>
        /// <returns>The shell of the application.</returns>
        /// <remarks>
        /// If the returned instance is a DependencyObject, the
        /// will attach the default Microsoft.Practices.Composite.Regions.IRegionManager
        /// of the application in its Microsoft.Practices.Composite.Presentation.Regions.RegionManager.RegionManagerProperty
        /// attached property in order to be able to add regions by using the 
        /// Microsoft.Practices.Composite.Presentation.Regions.RegionManager.RegionNameProperty
        /// attached property from XAML.
        /// </remarks>
        protected override DependencyObject CreateShell()
        {
            var userfolder = new UserFolder();
            userfolder.EmptyScratchPad();
            EventAggregator.GetEvent<ShutdownEvent>().Subscribe((pl) =>
                userfolder.EmptyScratchPad()
                );

            return Container.GetExportedValue<ShellWindow>();
        }

        /// <summary>
        /// Initializes the MainWindow.
        /// </summary>
        /// <remarks>
        /// The base implemention ensures the shell is composed in the container.
        /// </remarks>
        protected override void InitializeShell()
        {
            base.InitializeShell();
            //ServiceLocator.Current.GetInstance<IMonahrqShell>().SessionFactoryHolder.Reinitialize();

            Logger.Log("Initialize Default regions for Shell View", Category.Info, Priority.Low);

            // Initialize Default regions for Shell.
            var _regionManager = Container.GetExportedValue<IRegionManager>();
            _regionManager.RegisterViewWithRegion(RegionNames.Navigation, typeof(LeftNavigationControl));
            _regionManager.RegisterViewWithRegion(RegionNames.HelpContent, typeof(HelpView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainContent, typeof(WelcomeView));
            _regionManager.RegisterViewWithRegion("StatusBarRegion", typeof(StatusbarView));


            // Get Selected Reporting States
            if (!MonahrqContext.ForceDbUpGrade && !MonahrqContext.ForceDbRecreate)
            {
                //if (!HospitalRegion.Default.IsDefined)
                //{
                //    var configService = Container.GetExportedValue<IConfigurationService>();
                //    var test = configService.MonahrqSettings;
                //    var config = MonahrqConfiguration.UserApplicationConfig;
                //    HospitalRegion.Default.SelectedRegionType = typeof(CustomRegion);
                //    HospitalRegion.Default.DefaultStates = new System.Collections.Specialized.StringCollection();
                //    HospitalRegion.Default.Save();
                //}
                //var configService = Container.GetExportedValue<IConfigurationService>();

                //if (configService.HospitalRegion.SelectedStates.ToList().Any())
                //{
                //    MonahrqContext.ReportingStatesContext = configService.HospitalRegion.DefaultStatesProxy.OfType<StateElement>().Select(s => s.StateName).ToList();
                //    MonahrqContext.SelectedRegionType = configService.HospitalRegion.SelectedRegionType;
                //}

                // Initialize Report Generators
                var reportGenerators = Container.GetExportedValues<IReportGenerator>().ToList();
                reportGenerators.Add(ServiceLocator.Current.GetInstance<IReportGenerator>("BaseNursingHomeReportGenerator"));
                reportGenerators.Add(ServiceLocator.Current.GetInstance<IReportGenerator>("QualityReportGenerator"));

                reportGenerators.ForEach(rg => rg.InitGenerator());

                var eventAggregator = Container.GetExportedValue<IEventAggregator>();
                eventAggregator.GetEvent<ServiceErrorEvent>().Subscribe(this.HandleServiceErrorEvent);
                eventAggregator.GetEvent<ErrorNotificationEvent>().Subscribe(this.HandleGenericExceptionEvent);
                eventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Finalizing application initialization" });

                var regionRegistrars = Container.GetExportedValues<IModuleRegionRegistrar>().ToList();
                if (regionRegistrars.Any())
                    regionRegistrars.ForEach(rr => rr.RegisterRegions(_regionManager));

                // Run DB finalization script.
                FinalizeDb();

                MonahrqContext.MonahrqShell = Task<IMonahrqShell>.Run(() => LoadMonahrqShellAsync(Container)).Result;
            }

            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            Application.Current.MainWindow = (ShellWindow)this.Shell;
            Application.Current.MainWindow.Show();

            // Add some information about the application to the log file.
            var version = string.Format("MONAHRQ ver. {0}", MonahrqContext.ApplicationVersion);
            Logger.Log(version, Category.Info, Priority.Low);


            if (Stopwatch != null)
            {
                Stopwatch.Stop();
                Logger.Log(string.Format("Time to load: {0}", Stopwatch.Elapsed), Category.Info, Priority.Low);
            }
        }

        private void HandleGenericExceptionEvent(Exception obj)
        {
            _sessionLogger.Write(obj, $"Generic exception event raised");
        }

        private void HandleServiceErrorEvent(ServiceErrorEventArgs obj)
        {
            _sessionLogger.Write(obj.Exception, $"Service exception for data type {obj.EnitytType} named \"{obj.EntityName}\" (ID# {obj.EntityId ?? "?"})");
        }

        /// <summary>
        /// Loads the monahrq shell asynchronous.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        private IMonahrqShell LoadMonahrqShellAsync(CompositionContainer container)
        {
            _sessionLogger.Write("Loading MONAHRQ Shell...");
            return container.GetExportedValue<IMonahrqShell>();
        }

        /// <summary>
        /// Finalizes the database.
        /// </summary>
        private void FinalizeDb()
        {
            var factory = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            Task.Factory.StartNew(() =>
            {
                using (var sess = factory.SessionFactory.OpenStatelessSession())
                {
                    new FinalizeScript(sess.Connection).Execute();
                }
            }, TaskCreationOptions.LongRunning);

            var dbCreator = Container.GetExportedValue<IDatabaseCreator>(CreatorNames.Sql);
            Task.Factory.StartNew(() =>
            {
                dbCreator.TruncateDbLogFile(_configurationService.ConnectionSettings.ConnectionString);
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Shows the splash.
        /// </summary>
        void ShowSplash()
        {
            //ThreadStart showSplash = () =>
            //    {
            //Dispatcher.CurrentDispatcher.BeginInvoke(
            //        (Action)(() =>
            //        {
            var splash = Container.GetExportedValue<SplashView>();
            ((SplashViewModel)splash.DataContext).RegisteredUserName = !string.IsNullOrEmpty(Settings.Default.RegisteredUser) ? Settings.Default.RegisteredUser : "Test User";
            ((SplashViewModel)splash.DataContext).RegisteredUserCompany = !string.IsNullOrEmpty(Settings.Default.RegisteredCompany) ? Settings.Default.RegisteredCompany : "Test Company Inc.";
            splash.Show();
            //        }));
            //Dispatcher.Run();
            //    };

            //var thread = new Thread(showSplash) { Name = "Splash Thread", IsBackground = true };
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();          
            //Thread.Sleep(0);
        }

        /// <summary>
        /// Configures the <see cref="AggregateCatalog"/> used by MEF.
        /// </summary>
        /// <remarks>
        /// The base implementation does nothing.
        /// </remarks>
        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();

            // Add this assembly to export ModuleTracker
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(GetType().Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(IWindow).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(IEntity).Assembly));
            //AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(IBasedataImporter).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(IEntityFileImporter).Assembly));

            // These plugin modules are not referenced in the project and are discovered by inspecting a directory.
            // All plugin module projects have a post-build step to copy themselves into that directory.
            var modulesCatalog = new DirectoryCatalog("Modules");
            AggregateCatalog.Catalogs.Add(modulesCatalog);
        }

        /// <summary>
        /// Configures the <see cref="CompositionContainer"/>.
        /// May be overwritten in a derived class to add specific type mappings required by the application.
        /// </summary>
        /// <remarks>
        /// The base implementation registers all the types direct instantiated by the bootstrapper with the container.
        /// The base implementation also sets the ServiceLocator provider singleton.
        /// </remarks>
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            //var locator = Container.GetExportedValue<IServiceLocator>();
            //var cont = locator.GetInstance<CompositionContainer>();
            // Because we created the SessionbackLogger and it needs to be used immediately, we compose it to satisfy any imports it has.
            // within this file, use _sessionLogger instead of this 
            //Container.ComposeExportedValue<ILoggerFacade>(LogNames.Session, _sessionLogger);

            Container.ComposeExportedValue<IWindsorContainer>(new WindsorContainer());
            //var defMod = Container.GetExport<DefaultModule>();
            //IEnumerable<ComposablePartDefinition> catalog = Container.Catalog;
            //var mods = catalog.Select(part => ReflectionModelServices.GetPartType(part))
            //            .SelectMany(part => part.Value.GetCustomAttributes(true)
            //                    .OfType<DefaultModule>());

            Container.ComposeExportedValue<ILifetimeController>(new LifetimeController(() => Container));
        }

        private readonly object[] _configLock = { };

        /// <summary>
        /// Runs the scripts on application start.
        /// </summary>
        protected void RunScriptsOnAppStart()
        {
            //_configurationService = Container.GetExportedValue<IConfigurationService>();

            if (!MonahrqContext.ForceDbUpGrade && !MonahrqContext.ForceDbRecreate)
            {
                if (!_configurationService.MonahrqSettings.IsFirstRun) return;

                var provider = Container.GetExportedValue<IDomainSessionFactoryProvider>();

                if (!string.IsNullOrEmpty(_configurationService.MonahrqSettings.UpdateScriptToRunAtStartup))
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            var scriptPath = Path.Combine(MonahrqContext.BinFolderPath,
                                _configurationService.MonahrqSettings.UpdateScriptToRunAtStartup);

                            if (!File.Exists(scriptPath)) return;

                            var queryFileText = File.ReadAllText(scriptPath);
                            string[] queries = queryFileText.Split(new string[] { "GO\r\n", "GO ", "GO\t" }, StringSplitOptions.RemoveEmptyEntries);

                            using (var session = provider.SessionFactory.OpenStatelessSession())
                            {
                                foreach (var query in queries)
                                    session.CreateSQLQuery(query).ExecuteUpdate();
                            }
                        }
                        catch (Exception exc)
                        {
                            _sessionLogger.Write(exc.GetBaseException());
                            return;
                        }

                        lock (_configLock)
                        {
                            _configurationService.MonahrqSettings.UpdateScriptToRunAtStartup = null;
                            _configurationService.MonahrqSettings.IsFirstRun = false;
                            _configurationService.Save(_configurationService.MonahrqSettings);
                        }

                    });
                }
            }
            else
            {
                lock (_configLock)
                {
                    _configurationService.MonahrqSettings.UpdateScriptToRunAtStartup = null;
                    _configurationService.MonahrqSettings.IsFirstRun = false;
                    _configurationService.Save(_configurationService.MonahrqSettings);
                }
            }
        }

        /// <summary>
        /// Tests the database.
        /// </summary>
        /// <returns></returns>
        private bool TestDb()
        {
            var success = false;
            var creator = Container.GetExportedValue<IDatabaseCreator>(CreatorNames.Monahrq);
            //_configurationService = Container.GetExportedValue<IConfigurationService>();

            creator.Tested += delegate
            {
                success = true;
            };
            //var settings = MonahrqConfiguration.SettingsGroup.MonahrqSettings();
            var connectionStringSettings = _configurationService.MonahrqSettings.EntityConnectionSettings;
            creator.Test(connectionStringSettings.ConnectionString);

            return success;
        }

        /// <summary>
        /// Configures the LocatorProvider for the <see cref="T:Microsoft.Practices.ServiceLocation.ServiceLocator" />.
        /// </summary>
        /// <remarks>
        /// The base implementation also sets the ServiceLocator provider singleton.
        /// </remarks>
        protected override void ConfigureServiceLocator()
        {
            base.ConfigureServiceLocator();

            //_sessionLogger.Write("DEBUG: ConfigureServiceLocator()", Category.Debug, Priority.High);

            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            CheckUpdate(); // Perform Auto update check...

            if (!TestDb())
            {
                var msg = "MONAHRQ was unable to connect to the database. Please restart MONAHRQ and specify a database connection.";
                Logger.Log(msg, Category.Info, Priority.High);

                MessageBox.Show(msg, "MONAHRQ 6.0");
                Application.Current.Shutdown();

                // Force the program to quit without continuing the Prism bootstrapper sequence which loads NHibernate, etc.
                // This is usually not recommended, but in this case, we should not have any files open now,
                // and the program cannot continue without the database connection.
                //                Process.GetCurrentProcess().Kill();
                Environment.Exit(0);

                return;
            }
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
            ShowSplash();

            LogDatabaseInfo();
            //RunScriptsOnAppStart();
        }


        /// <summary>
        /// Gets or sets the stopwatch.
        /// </summary>
        /// <value>
        /// The stopwatch.
        /// </value>
        Stopwatch Stopwatch { get; set; }

        /// <summary>
        /// Creates the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> that will be used as the default container.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.
        /// </returns>
        /// <remarks>
        /// The base implementation registers a default MEF catalog of exports of key Prism types.
        /// Exporting your own types will replace these defaults.
        /// </remarks>
        protected override CompositionContainer CreateContainer()
        {
            return new CompositionContainer(AggregateCatalog);
        }

        /// <summary>
        /// Gets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        private IEventAggregator EventAggregator
        {
            get { return Container.GetExportedValue<IEventAggregator>(); }
        }

        /// <summary>
        /// Initializes the modules. May be overwritten in a derived class to use a custom Modules Catalog
        /// </summary>
        protected override void InitializeModules()
        {
            //_sessionLogger.Write("DEBUG: InitializeModules()", Category.Debug, Priority.High);

            if (this.Shell != null)
            {
                EventAggregator.GetEvent<MessageUpdateEvent>()
                               .Publish(new MessageUpdateEvent { Message = @"Initializing Modules" });
                //Application.Current.DoEvents();
                base.InitializeModules();
            }
            else
            {
                Logger.Log("Shell is null in InitializeModules", Category.Info, Priority.Low);
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Creates the <see cref="IModuleCatalog"/> used by Prism.
        /// </summary>
        /// <remarks>
        /// The base implementation returns a new ModuleCatalog.
        /// </remarks>
        /// <returns>
        /// A ConfigurationModuleCatalog.
        /// </returns>
        protected override IModuleCatalog CreateModuleCatalog()
        {
            // When using MEF, the existing Prism ModuleCatalog is still the place to configure modules via configuration files.
            return new ConfigurationModuleCatalog();
        }

        //protected override void ConfigureModuleCatalog()
        //{
        //    base.ConfigureModuleCatalog();
        //    //ModuleCatalog moduleCatalog = (ModuleCatalog)this.ModuleCatalog;
        //    //moduleCatalog.AddModule(typeof(DefaultModule));
        //    //moduleCatalog.AddModule(typeof(DataSetsModule));
        //}

        /// <summary>
        /// Create the <see cref="ILoggerFacade"/> used by the bootstrapper.
        /// </summary>
        /// <remarks>
        /// The base implementation returns a new TextLogger.
        /// </remarks>
        /// <returns>
        /// A CallbackLogger.
        /// </returns>
        protected override ILoggerFacade CreateLogger()
        {
            // Because the Shell is displayed after most of the interesting boostrapper work has been performed,
            // this quickstart uses a special logger class to hold on to early log entries and display them 
            // after the UI is visible.
            return _sessionLogger;
        }

        //string ScratchPad {get;set;}

        //private void CleanupScratchPad()
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Configures the default region adapter mappings to use in the application, in order
        /// to adapt UI controls defined in XAML to use a region and register it automatically.
        /// May be overwritten in a derived class to add specific mappings required by the application.
        /// </summary>
        /// <returns>
        /// The <see cref="RegionAdapterMappings" /> instance containing all the mappings.
        /// </returns>
        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
           // _sessionLogger.Write("DEBUG: ConfigureRegionAdapterMappings()", Category.Debug, Priority.High);
            RegionAdapterMappings regionAdapterMappings = base.ConfigureRegionAdapterMappings();

            var serviceLocator = Container.GetExportedValue<IServiceLocator>();
            if (serviceLocator != null)
            {
                regionAdapterMappings.RegisterMapping(typeof(StackPanel), serviceLocator.GetInstance<StackPanelRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof(DockPanel), serviceLocator.GetInstance<DockPanelRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof(TabControl), serviceLocator.GetInstance<TabControlRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof(MonahrqTabControl), serviceLocator.GetInstance<MonahrqTabControlRegionAdapter>());
            }
            return regionAdapterMappings;
        }

        /// <summary>
        /// Configures the <see cref="T:Microsoft.Practices.Prism.Regions.IRegionBehaviorFactory" />.
        /// This will be the list of default behaviors that will be added to a region.
        /// </summary>
        /// <returns></returns>
        protected override IRegionBehaviorFactory ConfigureDefaultRegionBehaviors()
        {
            //_sessionLogger.Write("DEBUG: ConfigureDefaultRegionBehaviors()", Category.Debug, Priority.High);
            var regionBehaviorTypesDictionary = base.ConfigureDefaultRegionBehaviors();
            regionBehaviorTypesDictionary.AddIfMissing(ClearChildViewsRegionBehavior.BEHAVIOR_KEY, typeof(ClearChildViewsRegionBehavior));
            return regionBehaviorTypesDictionary;
        }

        // ReSharper disable RedundantOverridenMember
        /// <summary>
        /// Helper method for configuring the <see cref="CompositionContainer" />.
        /// Registers all the types direct instantiated by the bootstrapper with the container.
        /// </summary>
        protected override void RegisterBootstrapperProvidedTypes()
        {
            //_sessionLogger.Write("DEBUG: ConfigureDefaultRegionBehaviors()", Category.Debug, Priority.High);
            base.RegisterBootstrapperProvidedTypes();
        }
        // ReSharper restore RedundantOverridenMember

        /// <summary>
        /// Test if we can open a tiny csv file in the app folder and return false if it gets an exception.
        /// It means the user probably doesn't have the Access 2010 Oledb components installed, as required by Monahrq.
        /// </summary>
        /// <returns></returns>
        bool CheckOledbComponentsExist()
        {
           // _sessionLogger.Write("DEBUG: CheckOledbComponentsExist()", Category.Debug, Priority.High);
            const string baseFilename = "Monahrq-Test.csv";
            string fullPath = Path.Combine(Environment.CurrentDirectory, baseFilename);
            bool tempTestFileCreated = false;
            //var configService = new ConfigurationService();
            _configurationService = new ConfigurationService();

            // Enable this line to see how the BootstrapErrorWindow looks, how the error is logged, and how the application shuts down in this case

            try
            {
                if (_configurationService.DataAccessComponentsInstalled) return true;

                // See if the tiny test file still exists in the app root folder. But in case the user deleted it, write a new one.
                if (!File.Exists(fullPath))
                {
                    tempTestFileCreated = true;
                    fullPath = CreateTempTestFile(baseFilename);
                }

                // Try to open the file using Oledb. If it fails, return false.
                var fileinfo = new FileInfo(fullPath);

                // This OleDbConnection must use the same Provider string that we require to load CSV files in the Wings.
                // The Provider is NOT "Microsoft.Jet.OLEDB.4.0", which is built into .NET.
                // Test this by uninstalling the "Microsoft 32-bit Access Engine".
                using (var con = new OleDbConnection(string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\"{0}\";Extended Properties='text;HDR=Yes;FMT=Delimited(,)';",
                                                                    fileinfo.DirectoryName)))
                {
                    using (OleDbCommand cmd = new OleDbCommand(string.Format("SELECT * FROM [{0}]", baseFilename), con))
                    {
                        con.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            // if we get here without exception, declare success (no need to loop thru the file)
                            if (reader != null)
                                reader.Close();
                        }
                    }
                }

                _sessionLogger.Log("Access Data Components installed on local machine.", Category.Info, Priority.High);
                _configurationService.DataAccessComponentsInstalled = true;
                // _configurationService.DataAccessComponentsInstalled = true;
            }
            catch (Exception)
            {
                _configurationService.DataAccessComponentsInstalled = false;
                // _configurationService.DataAccessComponentsInstalled = false;
                _sessionLogger.Log("Access Data Components not installed on local machine.", Category.Info, Priority.High);

                return false;
            }
            finally
            {
                // If temp file was created this time, user probably installed Oledb now so temp file won't be needed again, so delete it
                if (tempTestFileCreated)
                    File.Delete(fullPath);
            }

            return true;
        }

        // User must have deleted the tiny test file in app root folder, so write a new one in the MyDocuments folder and return the complete path.
        /// <summary>
        /// Creates the temporary test file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        string CreateTempTestFile(string filename)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            filename = Path.Combine(path, filename);

            var lines = new[] {
                    "col1,col2,col3",
                    "abc,xyz,pqr"
                };

            File.WriteAllLines(filename, lines);
            return filename;
        }


        #region Update Notification
        /// <summary>
        /// Checks the update.
        /// </summary>
        private void CheckUpdate()
        {
            _sessionLogger.Write("Checking for MONAHRQ Update...");
            //_configurationService = Container.GetExportedValue<IConfigurationService>();
            if (string.IsNullOrWhiteSpace(_configurationService.MonahrqSettings.UpdateCheckUrl))
                return;

            string downloadUrl = null;
            string latestVersionVal = null;
            string updateMessage = null;

            using (var webClient = new WebClient())
            {
                try
                {
                    //if (string.IsNullOrEmpty(configService.MonahrqSettings.UpdateCheckUrl))
                    //    return;

                    var updateUrl = _configurationService.MonahrqSettings.UpdateCheckUrl;
                    var updatexmlstr = webClient.DownloadString(updateUrl);
                    var updatexml = XDocument.Parse(updatexmlstr);

                    var root = updatexml.Element("monahrq");

                    if (root == null || !root.Elements().ToList().Any()) return;

                    var allowUpgrade = bool.Parse(root.Element("allow-upgrade").Value);
                    latestVersionVal = root.Element("version").Value;
                    downloadUrl = root.Element("download-url").Value;
                    var messageElement = root.Element("message");
                    if (messageElement != null)
                    {
                        updateMessage = messageElement.Value;
                    }

                    if (!allowUpgrade || latestVersionVal.IsNullOrEmpty() || downloadUrl.IsNullOrEmpty())
                        return;

                    var installedVersionVal = _configurationService.MonahrqSettings.MonahrqVersion; //MonahrqContext.ApplicationVersion; 

                    if (!installedVersionVal.IsNullOrEmpty())
                    {
                        var installedVersion = new Version(installedVersionVal);
                        var latestVersion = new Version(latestVersionVal);

                        if (installedVersion.CompareTo(latestVersion) >= 0)
                            return;
                    }
                }
                catch (Exception ex)
                {
                    _sessionLogger.Write(ex);
                    return;
                }
            }

            var message = new StringBuilder();
            message.AppendLine(string.Format("New Updates for MONAHRQ ® is now available for download. Do you want to download and install this update?", latestVersionVal));

            if (!string.IsNullOrEmpty(updateMessage))
            {
                message.AppendLine();
                message.AppendLine(updateMessage);
                message.AppendLine();
            }
            message.AppendLine(string.Format("{0}{0}Click Yes to download the latest version.", Environment.NewLine));

            if (MessageBox.Show(message.ToString(), "MONAHRQ Updates" , MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var updateView = Container.GetExportedValue<CheckAndDownloadUpdateView>();
                ((CheckAndDownloadUpdateViewModel)updateView.DataContext).DownloadUrl = downloadUrl;
                ((CheckAndDownloadUpdateViewModel)updateView.DataContext).DownloadLocation = Path.GetTempPath();
                ((CheckAndDownloadUpdateViewModel)updateView.DataContext).DownloadFile();
                updateView.ShowDialog();

                Process.Start(downloadUrl);

                // bypass further initialization
                Application.Current.Shutdown();
                // Force the program to quit without continuing the Prism bootstrapper sequence which loads NHibernate, etc.
                // This is usually not recommended, but in this case, we should not have any files open now,
                // and the program cannot continue without the database connection.
                Environment.Exit(0);
            }
        }


        #endregion

        /// <summary>
        /// Renames my documents directory if needed.
        /// </summary>
        private void RenameMyDocumentsDirectoryIfNeeded()
        {
            try
            {
               // _sessionLogger.Write("DEBUG: RenameMyDocumentsDirectoryIfNeeded()", Category.Debug, Priority.High);

                var directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var oldDirectoryPath = Path.Combine(directoryPath, MonahrqContext.OldApplicationName);
                var newDirectoryPath = Path.Combine(directoryPath, "Monahrq");
                var configFilePath = MonahrqConfiguration.ConfigFilePath;

                if (Directory.Exists(oldDirectoryPath) && !Directory.Exists(newDirectoryPath)) return;
                if (Directory.Exists(newDirectoryPath) && File.Exists(configFilePath)) return;

                IOHelper.RenameDirectory(new DirectoryInfo(oldDirectoryPath), new DirectoryInfo(newDirectoryPath), "Logs", null, true);
            }
            catch (Exception exc)
            {
                _sessionLogger.Write(exc, "Error renaming MONAHRQ directory in My Documents");
               // throw;
            }
        }

        /// <summary>
        /// Logs the database information.
        /// </summary>
        public void LogDatabaseInfo()
        {
            //_sessionLogger.Write("DEBUG: LogDatabaseInfo()", Category.Debug, Priority.High);

            var dataService = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            if (dataService == null) return;


            var query = @"SELECT SERVERPROPERTY('ProductVersion') AS [Version], SERVERPROPERTY('servername') AS [ServerName], 
	  CASE SERVERPROPERTY('EngineEdition')    
			WHEN 1 THEN 'Personal Edition'
			WHEN 2 THEN 'Standard Edition'
			WHEN 3 THEN 'Enterprise Edition'
			WHEN 4 THEN 'Express Edition'
			ELSE 'SQL Database'
	   END [Edition]";

            using (var session = dataService.SessionFactory.OpenStatelessSession())
            {
                var connection = new SqlConnectionStringBuilder(session.Connection.ConnectionString);

                var result = session.CreateSQLQuery(query)
                      .AddScalar("Version", NHibernateUtil.String)
                      .AddScalar("ServerName", NHibernateUtil.String)
                      .AddScalar("Edition", NHibernateUtil.String)
                      .SetResultTransformer(new AliasToBeanResultTransformer(typeof(DatabaseInfo)))
                      .List<DatabaseInfo>();

                Logger.Log(string.Format(result[0].ToString() + "Database Name: {0}", connection.InitialCatalog.Replace(@"&quot;", null)), Category.Info, Priority.High);
            }
        }

        /// <summary>
        /// Data base details class
        /// </summary>
        public struct DatabaseInfo
        {
            /// <summary>
            /// Gets or sets the version.
            /// </summary>
            /// <value>
            /// The version.
            /// </value>
            public string Version { get; set; }
            /// <summary>
            /// Gets or sets the name of the server.
            /// </summary>
            /// <value>
            /// The name of the server.
            /// </value>
            public string ServerName { get; set; }
            /// <summary>
            /// Gets or sets the edition.
            /// </summary>
            /// <value>
            /// The edition.
            /// </value>
            public string Edition { get; set; }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format("{3}Server Name: {0}{3}Version: {1}{3}Edition: {2}{3}", ServerName, Version, Edition, "<br>");
            }
        }
    }
}
