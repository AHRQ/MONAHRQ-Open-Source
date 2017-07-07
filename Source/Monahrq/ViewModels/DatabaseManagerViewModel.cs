using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Domain;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Resources;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Generators;
using Monahrq.Sdk.Modules;
using Monahrq.Sdk.Regions;
using PropertyChanged;
using NHibernate.Linq;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using NHibernate;
using NHibernate.Transform;

namespace Monahrq.ViewModels
{

    /// <summary>
    /// Enu m for view mode
    /// </summary>
    public enum ViewMode
    {
        None,
        Normal,
        Upgrade,
        ReBuild
    }

    /// <summary>
    /// View model for data base manage screen
    /// </summary>
    /// <seealso cref="Monahrq.ViewModels.SettingsViewModel" />
    [Export(typeof(DatabaseManagerViewModel)), ImplementPropertyChanged]
    [DataContract]
    [Serializable]
    public class DatabaseManagerViewModel : SettingsViewModel
    {
        #region Fields and Constants

        /// <summary>
        /// The is active
        /// </summary>
        private bool _isActive;
        /// <summary>
        /// The server name
        /// </summary>
        private string _serverName;
        /// <summary>
        /// The database name
        /// </summary>
        private string _databaseName;
        /// <summary>
        /// The database user name
        /// </summary>
        private string _dbUserName;
        /// <summary>
        /// The database user password
        /// </summary>
        private string _dbUserPassword;
        /// <summary>
        /// The is SQL server security
        /// </summary>
        private bool _isSqlServerSecurity;
        /// <summary>
        /// The skip test fail message
        /// </summary>
        private bool _skipTestFailMessage;
        /// <summary>
        /// The isloading
        /// </summary>
        private bool _isloading;
        /// <summary>
        /// The database operations
        /// </summary>
        private string _databaseOperations;
        /// <summary>
        /// The enable recreate button
        /// </summary>
        private bool _enableRecreateButton;
        /// <summary>
        /// The mode
        /// </summary>
        private ViewMode _mode;
        /// <summary>
        /// The major upgrade version
        /// </summary>
        private const string MajorUpgradeVersion = "6.0.0";
        /// <summary>
        /// The latest release version
        /// </summary>
        private static Version LatestReleaseVersion = new Version("6.0.1");
        /// <summary>
        /// The is major upgrade
        /// </summary>
        private bool _isMajorUpgrade;
        /// <summary>
        /// The original hash value
        /// </summary>
        private string _originalHashValue;

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        [IgnoreDataMember]
        [Import]
        IConfigurationService ConfigService { get; set; }

        /// <summary>
        /// Gets or sets the data services provider.
        /// </summary>
        /// <value>
        /// The data services provider.
        /// </value>
        [IgnoreDataMember]
        [Import]
        IDataServicesProvider DataServicesProvider { get; set; }

        /// <summary>
        /// Gets or sets the data provider.
        /// </summary>
        /// <value>
        /// The data provider.
        /// </value>
        [IgnoreDataMember]
        [Import]
        IDomainSessionFactoryProvider DataProvider { get; set; }

        ///// <summary>
        ///// Gets or sets the configuration service.
        ///// </summary>
        ///// <value>
        ///// The configuration service.
        ///// </value>
        //[Import(RequiredCreationPolicy = CreationPolicy.NonShared)]
        //IConfigurationService ConfigurationService { get; set; }


        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        [IgnoreDataMember]
        IEventAggregator EventAggregator { get; set; }


        /// <summary>
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        [IgnoreDataMember]
        IRegionManager RegionManager { get; set; }

        /// <summary>
        /// Gets or sets the database creator.
        /// </summary>
        /// <value>
        /// The database creator.
        /// </value>
        [Import(CreatorNames.Sql)]
        [IgnoreDataMember]
        IDatabaseCreator DatabaseCreator { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        [IgnoreDataMember]
        ILoggerFacade Logger { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseManagerViewModel"/> class.
        /// </summary>
        public DatabaseManagerViewModel()
        {
            TestDatabaseCommand = new DelegateCommand(OnTestDatabase, () => true);
            CreateOverwriteDatabaseCommand = new DelegateCommand(OnCreateOverwriteDatabase, () => true);
            DeleteDatabaseCommand = new DelegateCommand(OnDeleteDatabase, () => true);
            CancelCommand = new DelegateCommand(OnCancel, () => true);
            SaveCommand = new DelegateCommand(OnSave, () => true);
            SwitchDatabaseCommand = new DelegateCommand(OnSwitchDatabase, () => true);
            //    UpgradeDatabaseCommand = new DelegateCommand(OnUpgradeDatabase, () => true);
            UpgradeDatabaseCommand = new DelegateCommand(OnUpgradeDatabase2, () => true);

            IsActiveChanged += OnIsActiveChanged;
            DatabaseOperations = string.Empty;
        }

        #endregion

        #region Commands

        /// <summary>
        /// The test database command
        /// </summary>
        [IgnoreDataMember]
        public DelegateCommand TestDatabaseCommand { get; set; }


        /// <summary>
        /// Gets or sets the switch database command.
        /// </summary>
        /// <value>
        /// The switch database command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand SwitchDatabaseCommand { get; set; }


        /// <summary>
        /// Gets or sets the upgrade database command.
        /// </summary>
        /// <value>
        /// The upgrade database command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand UpgradeDatabaseCommand { get; set; }

        /// <summary>
        /// Gets or sets the create and/or overwrite database command.
        /// </summary>
        /// <value>
        /// The create overwrite database command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand CreateOverwriteDatabaseCommand { get; set; }

        /// <summary>
        /// The delete database command
        /// </summary>
        [IgnoreDataMember]
        public DelegateCommand DeleteDatabaseCommand { get; set; }

        /// <summary>
        /// The cancel command
        /// </summary>
        [IgnoreDataMember]
        public DelegateCommand CancelCommand { get; set; }

        /// <summary>
        /// The save command
        /// </summary>
        [IgnoreDataMember]
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public override int Index { get { return 0; } set {} }

        /// <summary>
        /// Gets sql connection.
        /// </summary>
        /// <value>
        /// The sql connection string.
        /// </value>
        public SqlConnectionStringBuilder AsBuilder
        {
            get
            {
                var builder = new SqlConnectionStringBuilder
                {
                    ApplicationIntent = ApplicationIntent.ReadWrite,
                    DataSource = ServerName.Replace(@"&quot;", null),
                    InitialCatalog = DatabaseName.Replace(@"&quot;", null),
                    IntegratedSecurity = !IsSqlServerSecurity,
                    PersistSecurityInfo = true,
                    AsynchronousProcessing = true,
                    Pooling = true,
                    MaxPoolSize = 40
                };
                //builder.ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                if (!builder.IntegratedSecurity)
                {
                    builder.UserID = DbUserName;
                    builder.Password = DbUserPassword;
                }

                return builder;
            }
        }

        //public bool ShowForceRecreate { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to enable recreate button or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if recreate button is enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableRecreateButton
        {
            get
            {
                if (IsBusy && !MonahrqContext.ForceDbRecreate)
                    return false;

                return _enableRecreateButton;
            }
            set { _enableRecreateButton = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether one can edit database name.
        /// </summary>
        /// <value>
        /// <c>true</c> if one can edit database name; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CanEditDatabaseName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show force upgrade or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if show force upgrade is enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowForceUpgrade { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is updating.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is updating; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsUpdating { get; set; }

        public SqlConnectionStringBuilder OldConnectionStringbuilder { get; private set; }

        /// <summary>
        /// Gets or sets the view mode.
        /// </summary>
        /// <value>
        /// The view mode.
        /// </value>
        [DataMember(EmitDefaultValue = false)]
        public ViewMode Mode
        {
            get { return _mode; }
            set
            {
                if (_mode != value)
                {
                    _mode = value;

                    if (_mode == ViewMode.Upgrade)
                    {
                        EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = true });
                        DisableUIElements = IsBusy = true;
                        ShowForceUpgrade = true;
                        EnableRecreateButton = IsUpdating = false;
                    }
                    else if (_mode == ViewMode.ReBuild)
                    {
                        EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = true });
                        DisableUIElements = IsBusy = true;
                        ShowForceUpgrade = false;
                        EnableRecreateButton = true;
                    }
                    else
                    {
                        DisableUIElements = IsBusy = false;
                        ShowForceUpgrade = false;
                        EnableRecreateButton = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether database is busy or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if database is busy; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsBusy { get; set; }

        [DataMember]
        public bool DisableUIElements { get; set; }


        /// <summary>
        /// Gets or sets the database operations.
        /// </summary>
        /// <value>
        /// The database operations.
        /// </value>
        public string DatabaseOperations
        {
            get { return _databaseOperations; }
            set
            {
                _databaseOperations = value;
                Application.Current.DoEvents();
            }
        }


        /// <summary>
        /// Gets or sets the name of the server.
        /// </summary>
        /// <value>
        /// The name of the server.
        /// </value>
        [DataMember(EmitDefaultValue = false)]
        public string ServerName
        {
            get { return _serverName; }
            set
            {
                _serverName = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        [DataMember(EmitDefaultValue = false)]
        public string DatabaseName
        {
            get { return _databaseName; }
            set
            {
                _databaseName = value;
            }
        }


        /// <summary>
        /// Gets or sets the name of the database user.
        /// </summary>
        /// <value>
        /// The name of the database user.
        /// </value>
        [DataMember(EmitDefaultValue = false)]
        public string DbUserName
        {
            get { return _dbUserName; }
            set
            {
                _dbUserName = value;
            }
        }


        /// <summary>
        /// Gets or sets the database user password.
        /// </summary>
        /// <value>
        /// The database user password.
        /// </value>
        [DataMember(EmitDefaultValue = false)]
        public string DbUserPassword
        {
            get { return _dbUserPassword; }
            set
            {
                _dbUserPassword = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether SQL server security is enabled or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if SQL server security is enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSqlServerSecurity
        {
            get { return _isSqlServerSecurity; }
            set
            {
                _isSqlServerSecurity = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to show SQL credentials panel or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if SQL credentials panel is visible; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSqlCredentialsPanel { get { return IsSqlServerSecurity; } }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        [IgnoreDataMember]
        public BaseViewModel Parent { get; set; }

        /// <summary>
        /// Gets or sets the original hash value.
        /// </summary>
        /// <value>
        /// The original hash value.
        /// </value>
        public override string OriginalHashValue
        {
            get { return _originalHashValue; }
            set
            {
                _originalHashValue = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when [upgrade database2].
        /// </summary>
        private void OnUpgradeDatabase2()
        {
            DatabaseOperations = string.Empty;

            if (MessageBox.Show(string.Format("Are you sure you want to upgrade your database '{0}' on server: {1}?", AsBuilder.InitialCatalog, AsBuilder.DataSource),
                "MONAHRQ Database Upgrade", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = true });

            IsUpdating = DisableUIElements = true;
            SetBusyState(true);

            Notify(string.Format("Starting upgrade of database '{0}'", AsBuilder.InitialCatalog), Category.Info, Priority.High);

            //Commented below four lines till DatabaseCreator.DeleteAndCreate

            //OldConnectionStringbuilder = new SqlConnectionStringBuilder(AsBuilder.ConnectionString);

            //DatabaseName = string.Format("{0}_v{1}", AsBuilder.InitialCatalog, ConfigService.CurrentSchemaVersion);

            //SetConnectionString();

            // DatabaseCreator.DeleteAndCreate(AsBuilder.ConnectionString, ConfigService.CurrentSchemaVersion);

            PerformDatabaseUpgrade();

            //EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = string.Format("Finished Deleting / Creating database '{0}'", AsBuilder.DataSource) });

            SetBusyState(false);
            IsUpdating = DisableUIElements = false;
            EnableRecreateButton = true;
        }

        /// <summary>
        /// Called when [upgrade database].
        /// </summary>
        private void OnUpgradeDatabase()
        {
            DatabaseOperations = string.Empty;

            if (MessageBox.Show(string.Format("Are you sure you want to upgrade your database '{0}' on server: {1}?",
                              AsBuilder.InitialCatalog,
                              AsBuilder.DataSource),
                "MONAHRQ Database Upgrade",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = true });

            IsUpdating = DisableUIElements = true;
            SetBusyState(true);

            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = string.Format("Starting upgrade of database '{0}'", AsBuilder.InitialCatalog) });


            OldConnectionStringbuilder = new SqlConnectionStringBuilder(AsBuilder.ConnectionString);

            DatabaseName = string.Format("{0}_v{1}", AsBuilder.InitialCatalog, ConfigService.CurrentSchemaVersion);

            SetConnectionString();

            DatabaseCreator.DeleteAndCreate(AsBuilder.ConnectionString, ConfigService.CurrentSchemaVersion);
            //EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = string.Format("Finished Deleting / Creating database '{0}'", AsBuilder.DataSource) });

            SetBusyState(false);
            IsUpdating = DisableUIElements = false;
            EnableRecreateButton = true;
        }

        /// <summary>
        /// Called when is active field is changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        public override void OnIsActiveChanged(object sender, EventArgs eventArgs)
        {
            ResetAll();

            if (IsActive)
            {
                WireEvents();
                Load();
            }
        }

        #region IActiveAware & IPartImportsSatisfiedNotification

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public new void OnImportsSatisfied()
        {
            WireEvents();
        }


        #endregion

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public override void OnSave()
        {
            //var temp = ConfigurationService.ConnectionSettings;
            //temp.ConnectionString = AsBuilder.ConnectionString;
            //ConfigurationService.ConnectionSettings = temp;

            var canLeave = true;
            var isDirty = GetViewModelHashCode(this, GetType()) != OriginalHashValue;
            if (isDirty && MessageBox.Show("The data has been edited. Are you sure you want to exit?", "Modification Verification", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = false });


            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri("WelcomeView", UriKind.Relative)); ;
        }

        /// <summary>
        /// Sets the connection string.
        /// </summary>
        private void SetConnectionString()
        {
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            var temp = configService.ConnectionSettings;
            if (temp == null)
            {
                temp = new System.Configuration.ConnectionStringSettings();
                temp.ProviderName = "System.Data.SqlClient";
            }

            // new InvalidOperationException("Error when trying to update connection string.");
            temp.ConnectionString = AsBuilder.ConnectionString;
            if (!temp.ConnectionString.EqualsIgnoreCase(configService.ConnectionSettings.ConnectionString))
            {
                configService.ConnectionSettings = temp;
                //ConfigService.ConnectionSettings = temp;
            }
            //ConfigurationService.MonahrqSettings.EntityConnectionSettings.ConnectionString = temp.ConnectionString;
            //ConfigurationService.Save(ConfigurationService.MonahrqSettings);
            //Thread.Sleep(10);
        }

        /// <summary>
        /// Called when reset operation is performed.
        /// </summary>
        public override void OnReset()
        {
            this.ResetAll();
            this.Load();
        }

        /// <summary>
        /// Called when cancel operation is performed.
        /// </summary>
        public override void OnCancel()
        {}

        /// <summary>
        /// Called when [test database].
        /// </summary>
        public void OnTestDatabase()
        {
            DatabaseOperations = string.Empty;

            SetBusyState(true);

            DatabaseCreator.Test(AsBuilder.ConnectionString);

            SetBusyState(false);

            OriginalHashValue = GetViewModelHashCode(this, GetType());
        }

        /// <summary>
        /// Called when create or overwite database.
        /// </summary>
        public void OnCreateOverwriteDatabase()
        {
            DatabaseOperations = string.Empty;
            if (AsBuilder.DataSource.Length > 250)
            {
                MessageBox.Show("Server name length can not be more than 250 characters", "Validation Error!!!", MessageBoxButton.OK);
                return;
            }

            if (MessageBox.Show(string.Format("Are you sure you want to create and/or overwrite database '{0}' on server: {1}?",
                              AsBuilder.InitialCatalog,
                              AsBuilder.DataSource),
                "Create and/or overwrite database?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = true });

            MonahrqContext.ForceDbRecreate = false;
            MonahrqContext.ForceDbUpGrade = false;

            DisableUIElements = true;
            SetBusyState(true);

            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = string.Format("Starting create and/or overwrite database '{0}'", AsBuilder.InitialCatalog) });

            SetConnectionString();

            DatabaseCreator.DeleteAndCreate(AsBuilder.ConnectionString, ConfigService.CurrentSchemaVersion);
            //EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = string.Format("Finished Deleting / Creating database '{0}'", AsBuilder.DataSource) });

            if (!MonahrqContext.ForceDbUpGrade)
            {
                SetBusyState(false);

                DisableUIElements = false;
            }
            OriginalHashValue = GetViewModelHashCode(this, GetType());
        }

        /// <summary>
        /// Gets the modules.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IModule> GetModules()
        {
            var sortedModules = new Dictionary<string, IModule>();
            var otherModules = new List<IModule>();
            var modules = ServiceLocator.Current.GetAllInstances<IModule>().ToList();

            foreach (var module in modules) //.OrderByDescending(c => c.GetType().Name).ToList()
            {
                var wingAttribute = module.GetType().GetCustomAttributes(typeof(WingModuleAttribute), false)[0] as WingModuleAttribute;
                if (wingAttribute != null)
                {
                    switch (wingAttribute.ModuleName.ToUpper())
                    {
                        case "CONFIGURATION MANAGEMENT":
                            sortedModules.Add("mod_0", module);
                            break;
                        case "TOPICS":
                            sortedModules.Add("mod_1", module);
                            break;
                        case "BASE DATA":
                            sortedModules.Add("mod_2", module);
                            break;
                        case "BASE DATA LOADER":
                            sortedModules.Add("mod_3", module);
                            break;
                        case "CLINICAL DIMENSIONS":
                            sortedModules.Add("mod_4", module);
                            break;
                        case "HOSPITALS AND REGIONS":
                            sortedModules.Add("mod_5", module);
                            break;
                        //case "DATASETS MANAGEMENT":
                        //    sortedModules.Insert(5, module);
                        //    break;
                        default:
                            otherModules.Add(module);
                            break;
                    }
                }
            }

            otherModules.InsertRange(0, sortedModules.OrderBy(k => k.Key).Select(x => x.Value));

            return otherModules.ToList();
        }

        /// <summary>
        /// Gets the installers.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IModuleInstaller> GetInstallers()
        {
            var installers = new List<IModuleInstaller>();

            var modules = GetModules();

            if (modules.Any())
            {
                installers.AddRange(modules.OfType<IModuleInstaller>());
            }
            return installers;
        }

        /// <summary>
        /// Called when delete database action is performed.
        /// </summary>
        public void OnDeleteDatabase()
        {
            DatabaseOperations = string.Empty;

            if (MessageBox.Show(
                string.Format("Are you sure you want to delete database '{0}' on server: {1}?", AsBuilder.InitialCatalog,
                              AsBuilder.DataSource),
                "Delete SQL Server Database?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = true });

            SetBusyState(true);

            DatabaseCreator.Delete(AsBuilder.ConnectionString);

            DisableUIElements = true;

            SetBusyState(false);
        }

        ///// <summary>
        ///// Determines whether this instance [can items filled].
        ///// </summary>
        ///// <returns></returns>
        //private bool CanItemsFilled()
        //{
        //    return true;
        //}

        ///// <summary>
        ///// Doeses the database exist.
        ///// </summary>
        ///// <returns></returns>
        //private bool DoesDbExist()
        //{
        //    return true;
        //}

        /// <summary>
        /// Wires the events.
        /// </summary>
        private void WireEvents()
        {
            DatabaseCreator.Created += (o, e) =>
                {
                    var temp = ConfigService.ConnectionSettings;
                    temp.ConnectionString = AsBuilder.ConnectionString;
                    ConfigService.ConnectionSettings = temp;
                    //ConnectionMessage = e.Message;
                    //    MessageBox.Show(e.Message, "SQL Server Connection", MessageBoxButton.OK, MessageBoxImage.Information);
                    //EventAggregator.GetEvent<GenericNotificationEvent>().Publish(e.Message);

                    DatabaseOperations += e.Message;
                    Application.Current.DoEvents();
                };

            //DatabaseCreator.CreateFailed += (o, e) => EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e.ConnectionException);
            //DatabaseCreator.Tested += (o, e) => EventAggregator.GetEvent<GenericNotificationEvent>().Publish(e.Message);
            //DatabaseCreator.TestFailed += (o, e) => EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e.ConnectionException);
            //DatabaseCreator.Deleted += (o, e) => EventAggregator.GetEvent<ConnectionSuccessEvent>().Publish(e);
            //DatabaseCreator.DeleteFailed += (o, e) => EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e.ConnectionException);

            DatabaseCreator.CreateFailed += (o, e) => { DatabaseOperations += e.ConnectionException.Message; };
            DatabaseCreator.Tested += (o, e) => { DatabaseOperations = e.Message; };
            DatabaseCreator.TestFailed += (o, e) => { DatabaseOperations = e.ConnectionException.Message.Contains("Login failed for user") ? "Login failed for the user: "+ DbUserName + ". Please provide correct credentials to login to the database server." : e.ConnectionException.Message; };
            DatabaseCreator.Deleted += (o, e) =>
                {
                    DatabaseOperations += e.Message;
                };
            DatabaseCreator.DeleteFailed += (o, e) =>
                {
                    EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = false });
                    DatabaseOperations += e.ConnectionException.Message;
                };

            DatabaseCreator.DeleteAndCreated += DatabaseCreatorOnDropAndCreated;
            DatabaseCreator.DeleteAndCreatedFailed += (o, e) => EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent
            {
                Message = "An error occurred while recreating database: " + e.ConnectionException.Message
            });

            DatabaseCreator.UpgradeCompleted += DatabaseCreatorOnDropAndCreated;
            DatabaseCreator.UpgradeCompletedFailed += (o, e) => EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent
            {
                Message = "An error occurred while recreating database: " + e.ConnectionException.Message
            });

            EventAggregator.GetEvent<MessageUpdateEvent>().Subscribe(
                mue =>
                {
                    if (DatabaseOperations != null && !DatabaseOperations.Contains(mue.Message))
                    {
                        DatabaseOperations += string.Format("{0}...{1}{1}", mue.Message, Environment.NewLine);
                        Application.Current.DoEvents();
                    }

                    //Thread.Sleep(0);
                });
        }

        /// <summary>
        /// Called when switch database action is invoked.
        /// </summary>
        public void OnSwitchDatabase()
        {
            DatabaseOperations = null;

            var oldBuilder = new SqlConnectionStringBuilder(ConfigService.MonahrqSettings.EntityConnectionSettings.ConnectionString);

            if (MessageBox.Show(
               string.Format("Are you sure you want to switch databases from '{0}' to '{1}'?", oldBuilder.InitialCatalog,
                             AsBuilder.InitialCatalog),
               "Switch SQL Server Database?",
               MessageBoxButton.YesNo,
               MessageBoxImage.Question) == MessageBoxResult.No)
                return;


            if (!TestDatabaseSwitchSuccess(AsBuilder.ConnectionString))
            {
                if (MonahrqContext.ForceDbUpGrade || MonahrqContext.ForceDbRecreate)
                {
                    SetConnectionString();

                    RegionManager.RequestNavigate(RegionNames.MainContent,
                                                  new Uri("ManageSettingsView", UriKind.Relative));
                    return;
                }

                DatabaseOperations = string.Format("The database '{0}' does not exist or user does not have permissions to connection to database and/or the database is of the wrong version. if wrong version, either switch to an existing database of the correct version or overwrite existing \"{0}\" database.",
                                                   AsBuilder.InitialCatalog);

                return;
            }

            SetBusyState(true);

            SetConnectionString();

            ConfigService.ForceRefresh();

            DatabaseOperations += string.Format("Re-initializing reporting database objects for \"{0}\" database.", AsBuilder.InitialCatalog);

            var reportGenerators = ServiceLocator.Current.GetAllInstances<IReportGenerator>().ToList();
			reportGenerators.Add(ServiceLocator.Current.GetInstance<IReportGenerator>("BaseNursingHomeReportGenerator"));
			reportGenerators.Add(ServiceLocator.Current.GetInstance<IReportGenerator>("QualityReportGenerator"));
			reportGenerators.ForEach(rg => rg.InitGenerator());

            SetBusyState(false);

            var finishedMessage = string.Format("Database successfully switched from databases from '{0}' to '{1}'.",
                                                oldBuilder.InitialCatalog,
                                                AsBuilder.InitialCatalog);
            DatabaseOperations += finishedMessage;

            DataProvider.SessionFactory.ClearAllNhibernateCaches();

            // Truncate/Shrink DB Log files
            TruncateLogFile(AsBuilder.ConnectionString, 5000);
            OriginalHashValue = GetViewModelHashCode(this, GetType());
            LogDatabaseInfo();

            if (MessageBox.Show(string.Format("{0} Click Ok to restart the app or Cancel to remain in the application.", finishedMessage), "Successful Upgrade", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                var postupgradePath = Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\Version");
                Restart(postupgradePath);
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Tests the database switch success.
        /// </summary>
        /// <param name="newConnection">The new connection.</param>
        /// <returns></returns>
        private bool TestDatabaseSwitchSuccess(string newConnection)
        {
            var builder = new SqlConnectionStringBuilder(newConnection);
            try
            {
                var schemaVersion = string.Empty;
                using (var con = new SqlConnection(builder.ConnectionString))
                {
                    con.Open();
                    schemaVersion = DataServicesProvider.GetDBSchemaVersion(con);
                }

                var actual = new Version(schemaVersion);
                var expected = new Version(ConfigService.CurrentSchemaVersion);

                if (expected > actual)
                {
                    var msg =
                       string.Format(
                            "Your current database \"{0}\" needs to be upgraded. Please click \"Yes\" to upgrade now or \"No\" to create / overwrite database via the database connection manager.",
                            AsBuilder.InitialCatalog);

                    var msgResult = MessageBox.Show(msg, "MONAHRQ Database Update Required!",
                                                    MessageBoxButton.YesNo);

                    if (msgResult == MessageBoxResult.Yes)
                    {
                        MonahrqContext.ForceDbUpGrade = true;
                        MonahrqContext.ForceDbRecreate = false;
                        MonahrqContext.DbNameToUpGrade = AsBuilder.InitialCatalog;
                        Mode = ViewMode.Upgrade;
                    }
                    else
                    {
                        MonahrqContext.ForceDbUpGrade = false;
                        MonahrqContext.ForceDbRecreate = false;
                        MonahrqContext.DbNameToUpGrade = null;
                        Mode = ViewMode.Normal;
                    }
                    //else
                    //{
                    //    MonahrqContext.ForceDbRecreate = true;
                    //    MonahrqContext.ForceDbUpGrade = false;
                    //    MonahrqContext.DbNameToUpGrade = null;
                    //    Mode = ViewMode.ReBuild;
                    //}


                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.GetBaseException().Message, Category.Exception, Priority.High);
                return false;
            }
        }

        /// <summary>
        /// Databases the creator on drop and created.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="ConnectionStringSuccessEventArgs"/> instance containing the event data.</param>
        private void DatabaseCreatorOnDropAndCreated(object sender, ConnectionStringSuccessEventArgs eventArgs)
        {
            if (eventArgs.Success)
            {
                try
                {
                    SetConnectionString();

                    EventAggregator.GetEvent<MessageUpdateEvent>()
                                   .Publish(new MessageUpdateEvent { Message = "Database '" + AsBuilder.InitialCatalog + "' successfully created" });

                    EventAggregator.GetEvent<MessageUpdateEvent>()
                                   .Publish(new MessageUpdateEvent { Message = "Building database tables" });

                    DataProvider.SessionFactory.ClearAllNhibernateCaches(); // flush nhibernate caches
                    MonahrqNHibernateProvider.ForceRefresh();

                    InitModules();
                }
                catch (Exception exc)
                {
                    Logger.Log(exc.GetBaseException().Message, Category.Exception, Priority.High);

                    EventAggregator.GetEvent<MessageUpdateEvent>()
                                   .Publish(new MessageUpdateEvent
                                   {
                                       Message =
                                               string.Format("Database \"{0}\" unsuccessfully created/updated",
                                                             AsBuilder.InitialCatalog)
                                   });
                    EventAggregator.GetEvent<DisableNavigationEvent>()
                                   .Publish(new DisableNavigationEvent { DisableUIElements = false });
                }
            }

            //if (MonahrqContext.ForceDbUpGrade && !string.IsNullOrEmpty(MonahrqContext.DbNameToUpGrade ?? string.Empty))
            //{
            //    PerformDatabaseUpgrade();
            //    return;
            //}

            EnableRecreateButton = true;
            DisableUIElements = IsBusy = false;
            MonahrqContext.ForceDbRecreate = false;
            MonahrqContext.DbNameToUpGrade = null;

            EventAggregator.GetEvent<MessageUpdateEvent>()
                            .Publish(new MessageUpdateEvent { Message = string.Format("Database \"{0}\" successfully created/updated", AsBuilder.InitialCatalog) });

            EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = false });

            // Truncate/Shrink DB Log files
            TruncateLogFile(AsBuilder.ConnectionString, 5000);

            var postupgradePath = Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\Version");
            MessageBox.Show("Database \"" + AsBuilder.InitialCatalog + "\" successfully created/update. Click Ok to restart the app.", "Successful Database Create/OverWrite", MessageBoxButton.OK);
            Restart(postupgradePath);
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Truncates the log file.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="waitInterval">The wait interval.</param>
        private void TruncateLogFile(string connectionString, int waitInterval = 0)
        {
            if (waitInterval > 0)
                Thread.SpinWait(waitInterval);

            Task.Factory.StartNew(() => DatabaseCreator.TruncateDbLogFile(connectionString), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Performs the database upgrade.
        /// </summary>
        private void PerformDatabaseUpgrade()
        {
            Notify(string.Format("Begin Upgrade database \"{0}\".", OldConnectionStringbuilder.InitialCatalog), Category.Info, Priority.High);

            using (var dbUpgradeCon = new SqlConnection(OldConnectionStringbuilder.ConnectionString))
            {
                dbUpgradeCon.Open();
                var dbUpgradeTblquery = "SELECT COUNT(o.name) FROM sys.objects o WHERE o.name='DBUpgradeLog'";
                using (var cmd = dbUpgradeCon.CreateCommand())
                {
                    cmd.CommandText = dbUpgradeTblquery;
                    var id = (int)cmd.ExecuteScalar();
                    if (id == 0)
                    {
                        dbUpgradeTblquery = "CREATE TABLE DBUpgradeLog (LastExecutedDBUpgradeFile VARCHAR(600),ExecutionDateTime DATETIME,ExecutionStatus BIT)";
                        cmd.CommandText = dbUpgradeTblquery;
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        dbUpgradeTblquery = "SELECT LastExecutedDBUpgradeFile FROM DBUpgradeLog";
                        cmd.CommandText = dbUpgradeTblquery;
                        var obj = cmd.ExecuteScalar();
                    }
                }
            }

            var scriptsPath = Path.Combine(MonahrqContext.BinFolderPath, "Resources\\Database\\Version");
            BackupDatabase(scriptsPath);

            var actualVersion = GetSchemaVersion();
            var currentVersion = new Version(ConfigService.CurrentSchemaVersion);

            if (!UpgradeDatabase(Path.Combine(scriptsPath, "60"), actualVersion, LatestReleaseVersion)) return;

            if (!UpgradeDatabase(Path.Combine(scriptsPath, "70"), actualVersion, currentVersion)) return;

            actualVersion = GetSchemaVersion();

            if (currentVersion.CheckForUpdate(actualVersion) == UpgradeTypeEnum.None)
            {
                MonahrqContext.ForceDbUpGrade = false;
                MonahrqContext.ForceDbRecreate = false;
                MonahrqContext.DbNameToUpGrade = null;
                DisableUIElements = ShowForceUpgrade = IsUpdating = false;
                EnableRecreateButton = true;
                CanEditDatabaseName = true;
            }

            Notify(string.Format("Database \"{0}\" successfully upgraded", AsBuilder.InitialCatalog), Category.Info, Priority.None);

            EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = false });

            // Truncate/Shrink DB Log files
            TruncateLogFile(AsBuilder.ConnectionString, 5000);
            System.Windows.MessageBox.Show(@"Monahrq has been upgraded to a newer version. Click Ok to restart the app.", "Successful Upgrade", MessageBoxButton.OK);
            Restart(scriptsPath);
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Backups the database.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        private void BackupDatabase(string filePath)
        {
            var scripts = GetUpgradeFileNames(filePath);
            ProcessUpgradeScripts(filePath, scripts);
        }

        /// <summary>
        /// Upgrades the database.
        /// </summary>
        /// <param name="updateScriptsPath">The update scripts path.</param>
        /// <param name="actualVersion">The actual version.</param>
        /// <param name="upgradeToVersion">The upgrade to version.</param>
        /// <returns></returns>
        private bool UpgradeDatabase(string updateScriptsPath, Version actualVersion, Version upgradeToVersion)
        {

            try
            {
                var upgradeType = upgradeToVersion.CheckForUpdate(actualVersion);
                if (upgradeType == UpgradeTypeEnum.None) return true;

                var isProcessed = false;

                var commonUpdateScriptsPath = Path.Combine(updateScriptsPath, "Common");
                if (Directory.Exists(commonUpdateScriptsPath))
                {
                    var commonUpgradeScripts = GetUpgradeFileNames(commonUpdateScriptsPath);
                    isProcessed = ProcessUpgradeScripts(commonUpdateScriptsPath, commonUpgradeScripts);
                    if (!isProcessed)
                    {
                        Notify("There was an error running common upgrade scripts.", Category.Warn, Priority.High);
                        return false;
                    }
                }

                while (upgradeType != UpgradeTypeEnum.None)
                {
                    try
                    {
                        switch (upgradeType)
                        {
                            case UpgradeTypeEnum.Major:
                                var dbUpgradeFileNames = GetUpgradeFileNames(updateScriptsPath);

                                isProcessed = ProcessUpgradeScripts(updateScriptsPath, dbUpgradeFileNames);

                                if (isProcessed)
                                {
                                    MonahrqContext.ForceDbUpGrade = false;

                                    if (ConfigService.CurrentSchemaVersion != MajorUpgradeVersion)
                                    {
                                        UpdateReportAudiences();
                                    }

                                    InitModules();

                                    var postupgradePath = Path.Combine(updateScriptsPath, "PostUpgrade");
                                    var postUpgradeScripts = GetAllSqlFiles(postupgradePath);
                                    ProcessUpgradeScripts(postupgradePath, postUpgradeScripts);
                                }
                                else
                                {
                                    Notify("There was an error running the post upgrade script", Category.Warn, Priority.High);
                                }
                                _isMajorUpgrade = true;
                                break;
                            case UpgradeTypeEnum.Minor:
                                var minorUpgradescriptsPath = Path.Combine(updateScriptsPath, "Minor");
                                var dbminorUpgradeFileNames = GetUpgradeFileNames(minorUpgradescriptsPath);
                                if (ProcessUpgradeScripts(minorUpgradescriptsPath, dbminorUpgradeFileNames))
                                {
                                    MonahrqContext.ForceDbUpGrade = false;
                                    if (!_isMajorUpgrade) InitModules();
                                    var postUpgradePath = Path.Combine(minorUpgradescriptsPath, "PostUpgrade");
                                    dbminorUpgradeFileNames = GetUpgradeFileNames(postUpgradePath);
                                    ProcessUpgradeScripts(postUpgradePath, dbminorUpgradeFileNames);
                                }
                                break;
                        }
                        actualVersion = GetSchemaVersion();
                        upgradeType = upgradeToVersion.CheckForUpdate(actualVersion);
                    }
                    catch (Exception)
                    {
                        Notify("Error occured while upgrading database.", Category.Warn, Priority.High);
                        upgradeType = UpgradeTypeEnum.None;
                    }
                }

                _isMajorUpgrade = false;
            }
            catch (Exception ex)
            {
                var exe = ex.GetBaseException();
                Logger.Log(string.Format("There was a error running upgrade scripts {0}{1}", Environment.NewLine, exe), Category.Warn, Priority.High);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Updates the report audiences.
        /// </summary>
        private void UpdateReportAudiences()
        {
            using (var con = new SqlConnection(AsBuilder.ConnectionString))
            {
                con.Open();
                using (var sqlCommand = con.CreateCommand())
                {
                    sqlCommand.CommandText = @"UPDATE Reports
		                                       SET Audiences = REPLACE(Audiences,'AllAudiences','Professionals')
		                                       FROM [Reports]
		                                       WHERE Audiences like '%AllAudiences%'";
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"UPDATE reports 
                                        	   SET SourceTemplateXml = CAST(REPLACE(CAST(SourceTemplateXml AS NVARCHAR(MAX)), 'AudienceType=""AllAudiences""','AudienceType=""Professionals""') AS XML)";
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.ExecuteNonQuery();


                }
                con.Close();
            }
        }

        /// <summary>
        /// Updates the schema version.
        /// </summary>
        /// <param name="version">The version.</param>
        private void UpdateSchemaVersion(string version)
        {
            using (var con = new SqlConnection(AsBuilder.ConnectionString))
            {
                con.Open();
                using (var sqlCommand = con.CreateCommand())
                {
                    sqlCommand.CommandText = string.Format("UPDATE SchemaVersions SET Version = '{0}' where [Name] = 'Database Schema'", version);
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.ExecuteNonQuery();
                }
                con.Close();
            }
        }

        /// <summary>
        /// Gets the upgrade file names.
        /// </summary>
        /// <param name="updateScriptsPath">The update scripts path.</param>
        /// <returns></returns>
        private static IEnumerable<string> GetUpgradeFileNames(string updateScriptsPath)
        {
            var dbUpgradeFileNames = new List<string>();

            var dataUpdateScripts = new DirectoryInfo(updateScriptsPath).GetFiles("*.sql").ToList();
            dataUpdateScripts.ForEach(file =>
            {
                var fileName = Path.GetFileNameWithoutExtension(file.ToString().Split('_')[0]);
                Int64 upgradeDate;
                if (Int64.TryParse(fileName, out upgradeDate))
                {
                    dbUpgradeFileNames.Add(file.ToString());
                }
            });

            dbUpgradeFileNames.Sort();

            return dbUpgradeFileNames;
        }

        /// <summary>
        /// Gets all SQL files.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static IEnumerable<string> GetAllSqlFiles(string path)
        {
            if (!Directory.Exists(path)) return Enumerable.Empty<string>();

            return new DirectoryInfo(path)
                .EnumerateFiles()
                .Where(x => x.Extension == ".sql")
                .OrderBy(x => x.Name)
                .Select(x => x.Name);
        }

        /// <summary>
        /// Gets the schema version.
        /// </summary>
        /// <returns></returns>
        private Version GetSchemaVersion()
        {
            using (var connection = new SqlConnection(OldConnectionStringbuilder.ConnectionString))
            {
                connection.Open();
                return new Version(DataServicesProvider.GetDBSchemaVersion(connection));
                //var expected = new Version(ConfigService.CurrentSchemaVersion);
                //return expected.CheckForUpdate(actual);
            }
        }

        /// <summary>
        /// Restarts the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        private static void Restart(string path)
        {
            var start = new ProcessStartInfo
            {
                FileName = Path.Combine(path, "Restart.bat"),
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true

            };
            Process.Start(start);
        }

        /// <summary>
        /// Processes the upgrade scripts.
        /// </summary>
        /// <param name="updateScriptsPath">The update scripts path.</param>
        /// <param name="dbUpgradeFileNames">The database upgrade file names.</param>
        /// <returns></returns>
        private bool ProcessUpgradeScripts(string updateScriptsPath, IEnumerable<string> dbUpgradeFileNames)
        {
            var isUpgradeSucceeded = 0;
            foreach (var fileToExecute in dbUpgradeFileNames)
            {
                if (fileToExecute == null) continue;

                string fileToExecuteWithFullPath = string.Concat(updateScriptsPath, "\\");
                fileToExecuteWithFullPath = string.Concat(fileToExecuteWithFullPath, fileToExecute);
                Logger.Log(string.Format("running {0}", fileToExecute), Category.Info, Priority.Low);

                var query = File.ReadAllText(fileToExecuteWithFullPath);

                // Below line is required to replace database name used in "Backup Database" script
                query = query.Replace("@@DESTINATION@@", AsBuilder.InitialCatalog)
                             .Replace("@@SOURCE@@", OldConnectionStringbuilder.InitialCatalog);

                var fileNames = fileToExecute.Split('_');
                var userfriendlyFileName = string.Empty;
                if (fileNames.Count() > 1)
                    userfriendlyFileName = Regex.Replace(fileNames[1], @"(([A-Z][a-z])+\s+)", " $1", RegexOptions.Compiled);
                else
                    userfriendlyFileName = fileNames[0];

                Notify(string.Format("Start upgrade over data for {0}", userfriendlyFileName), Category.Info, Priority.High);

                if (!string.IsNullOrEmpty(query))
                {
                    using (var con = new SqlConnection(AsBuilder.ConnectionString))
                    {
                        con.Open();

                        try
                        {
                            using (var cmd = con.CreateCommand())
                            {
                                cmd.CommandText = query;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandTimeout = ConfigService.MonahrqSettings.LongTimeout.Seconds;
                                cmd.ExecuteNonQuery();

                                query = string.Format(@"IF NOT EXISTS (SELECT 1 FROM DBUpgradeLog)
                                                        INSERT INTO DBUpgradeLog(LastExecutedDBUpgradeFile,ExecutionDateTime,ExecutionStatus) VALUES('{0}',GETDATE(),1)
                                                         ELSE 
	                                                     UPDATE DBUpgradeLog SET LastExecutedDBUpgradeFile = '{0}',ExecutionDateTime = GETDATE(),ExecutionStatus = 1", fileToExecute);
                                cmd.CommandText = query;
                                cmd.CommandType = CommandType.Text;
                                cmd.ExecuteNonQuery();

                                isUpgradeSucceeded = 1;
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorToUse = ex.GetBaseException();
                            var message = string.Format("Error during upgrade. ({0}){1}{2}", errorToUse.Message,
                                                        Environment.NewLine, errorToUse.StackTrace);
                            Logger.Log(message, Category.Exception, Priority.High);

                            // In Case of exception, set result value to False
                            isUpgradeSucceeded = 0;
                            break;
                        }
                    }
                }

                Notify(string.Format("Finished upgrade data for {0}", userfriendlyFileName), Category.Info, Priority.High);

            }

            return isUpgradeSucceeded == 1;
        }

        /// <summary>
        /// Notifies the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="category">The category.</param>
        /// <param name="priority">The priority.</param>
        private void Notify(string message, Category category, Priority priority)
        {
            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = message });
            Logger.Log(message, category, priority);
        }

        /// <summary>
        /// Initializes the modules.
        /// </summary>
        private void InitModules()
        {
            MonahrqNHibernateProvider.ForceRefresh();

            IList<IModule> modules = GetModules().ToList();

            //Thread.Sleep(1);

            EventAggregator.GetEvent<MessageUpdateEvent>()
                           .Publish(new MessageUpdateEvent { Message = "Initializing system data" });

            ListExtensions.ForEach(modules, mod => mod.Initialize());

            EventAggregator.GetEvent<MessageUpdateEvent>()
                           .Publish(new MessageUpdateEvent
                           {
                               Message = "Start installing reports database elements"
                           });

            var reportGenerators = ServiceLocator.Current.GetAllInstances<IReportGenerator>().ToList();
			reportGenerators.Add(ServiceLocator.Current.GetInstance<IReportGenerator>("BaseNursingHomeReportGenerator"));
			reportGenerators.Add(ServiceLocator.Current.GetInstance<IReportGenerator>("QualityReportGenerator"));
			reportGenerators.ForEach(rg => rg.InitGenerator());

            EventAggregator.GetEvent<MessageUpdateEvent>()
                           .Publish(new MessageUpdateEvent
                           {
                               Message = "Finished installing reports database elements successfully"
                           });

            // FinalIze DB
            var factory = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            EventAggregator.GetEvent<MessageUpdateEvent>()
                           .Publish(new MessageUpdateEvent { Message = "Finalizing database" });

            Thread.Sleep(500);

            using (var sess = factory.SessionFactory.OpenSession())
            {
                new FinalizeScript(sess.Connection).Execute();
            }

        }

        /// <summary>
        /// Gets the update item name for script.
        /// </summary>
        /// <param name="fileToExecute">The file to execute.</param>
        /// <returns></returns>
        private string GetUpdateItemNameForScript(FileInfo fileToExecute)
        {
            string result = string.Empty;
            switch (fileToExecute.Name.ToUpper())
            {
                case "DB_MONAHRQ_BUILD2_UPDATE_1.SQL":
                    //result = "Inpatient Discharge datasets";
                    result = "Hospitals and Regions";
                    break;
                case "DB_MONAHRQ_BUILD2_UPDATE_2.SQL":
                    result = "AHRQ-QI Area Data, AHRQ-QI Composite Data and AHRQ-QI Provider Data datasets";
                    break;
                case "DB_MONAHRQ_BUILD2_UPDATE_3.SQL":
                    result = "ED Treat And Release datasets";
                    break;
                case "DB_MONAHRQ_BUILD2_UPDATE_4.SQL":
                    result = "Medicare Provider Cost dataset";
                    break;
                case "DB_MONAHRQ_BUILD2_UPDATE_5.SQL":
                    //result = "Hospitals and Regions";
                    result = "Inpatient Discharge datasets";
                    break;
                case "DB_MONAHRQ_BUILD2_UPDATE_6.SQL":
                    result = "Measures and Topics";
                    break;
                case "DB_MONAHRQ_BUILD2_UPDATE_7.SQL":
                    result = "Reports";
                    break;
                case "DB_MONAHRQ_BUILD2_UPDATE_8.SQL":
                    result = "Websites";
                    break;
            }

            return result;
        }

        /// <summary>
        /// Sets the database schema current version.
        /// </summary>
        private void SetDbSchemaCurrentVersion()
        {
            var sessionProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            using (var session = sessionProvider.SessionFactory.OpenStatelessSession())
            {
                if (!session.Query<SchemaVersion>().Any(sv => sv.Version == ConfigService.CurrentSchemaVersion))
                {
                    using (var trans = session.BeginTransaction())
                    {
                        var schemaVersion = new SchemaVersion { Version = ConfigService.CurrentSchemaVersion, ActiveDate = DateTime.Now };
                        session.Insert(schemaVersion);

                        trans.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Sets the busystate to busy or not busy.
        /// </summary>
        /// <param name="busy">if set to <c>true</c> the application is now busy.</param>
        private void SetBusyState(bool busy)
        {
            IsBusy = busy;
            Mouse.OverrideCursor = busy ? Cursors.Wait : Cursors.Arrow;
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        private void Load()
        {
            _isloading = true;
            var connectionString = ConfigService.ConnectionSettings.ConnectionString;
            OldConnectionStringbuilder = new SqlConnectionStringBuilder(connectionString);
            var builder = new SqlConnectionStringBuilder(connectionString);

            ServerName = builder.DataSource;
            DatabaseName = builder.InitialCatalog;
            IsSqlServerSecurity = !builder.IntegratedSecurity;
            DbUserName = IsSqlServerSecurity ? builder.UserID : string.Empty;
            DbUserPassword = IsSqlServerSecurity ? builder.Password : string.Empty;
            DatabaseOperations = null;
            IsUpdating = false;
            CanEditDatabaseName = true;

            if (MonahrqContext.ForceDbUpGrade)
            {
                Mode = ViewMode.Upgrade;
                CanEditDatabaseName = false;
                //EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = true });
                //DisableUIElements = IsBusy = true;
                //ShowForceUpgrade = true;
                //EnableRecreateButton = false;
            }
            else if (MonahrqContext.ForceDbRecreate)
            {

                Mode = ViewMode.ReBuild;
                //EventAggregator.GetEvent<DisableNavigationEvent>().Publish(new DisableNavigationEvent { DisableUIElements = true });
                //DisableUIElements = IsBusy = true;
                ////ShowForceUpgrade = true;
                //EnableRecreateButton = true;
            }
            else
            {
                Mode = ViewMode.Normal;
                //DisableUIElements = IsBusy = false;
                //ShowForceUpgrade = false;
                //EnableRecreateButton = true;
            }

            _isloading = false;
        }

        /// <summary>
        /// Resets all.
        /// </summary>
        private void ResetAll()
        {
            DatabaseOperations = string.Empty;
            ServerName = null;
            DatabaseName = null;
            IsSqlServerSecurity = false;
            DbUserName = null;
            DbUserPassword = null;
        }

        ///// <summary>
        ///// Tests this instance.
        ///// </summary>
        //private void Test()
        //{
        //    //NotBusy = false;
        //    //Connecting(this, EventArgs.Empty);
        //    //DatabaseCreator.Test(AsBuilder.ConnectionString);
        //    //NotBusy = true;
        //}

        ///// <summary>
        ///// Creates the database.
        ///// </summary>
        //public void CreateDatabase()
        //{
        //    //NotBusy = false;
        //    //Connecting(this, EventArgs.Empty);
        //    //DbCreator.Create(AsBuilder.ConnectionString);
        //    //NotBusy = true;
        //}

        //// Delete the database if the user confirms
        //public void DeleteDatabase()
        //{
        //    //NotBusy = false;
        //    //if (MessageBox.Show(
        //    //    string.Format("Are you sure you want to delete database '{0}' on server: {1}?", AsBuilder.InitialCatalog, AsBuilder.DataSource),
        //    //    "Delete SQL Server Database?",
        //    //    MessageBoxButton.YesNo,
        //    //    MessageBoxImage.Question) == MessageBoxResult.Yes)
        //    //{
        //    //    Connecting(this, EventArgs.Empty);
        //    //    DbCreator.Delete(AsBuilder.ConnectionString);
        //    //}
        //    //NotBusy = true;
        //}
        /// <summary>
        /// Called when [navigated to].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            ResetAll();
            Load();
        }

        /// <summary>
        /// Determines whether the navigation is valid or not.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        ///   <c>true</c> if navigation is valid ; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// Called when [navigated from].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            ResetAll();
        }

        /// <summary>
        /// Logs the database information.
        /// </summary>
        public void LogDatabaseInfo()
        {
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
                var result = session.CreateSQLQuery(query)
                      .AddScalar("Version", NHibernateUtil.String)
                      .AddScalar("ServerName", NHibernateUtil.String)
                      .AddScalar("Edition", NHibernateUtil.String)
                      .SetResultTransformer(new AliasToBeanResultTransformer(typeof(DatabaseInfo)))
                      .List<DatabaseInfo>();

                Logger.Log(string.Format(result[0].ToString() + "Database Name: {0}", AsBuilder.InitialCatalog), Category.Info, Priority.High);
            }
        }

        /// <summary>
        /// Database info struct
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

      

      
        #endregion

    }
}