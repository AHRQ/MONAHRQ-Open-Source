using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.Events;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace Monahrq.Configuration.HostConnection
{
    /// <summary>
    /// This is the view model for the dialog to connect to the Monahrq SQL Server database
    /// </summary>
    [ImplementPropertyChanged]
    [Export]
    public class HostConnectionViewModel
    {
        public event EventHandler Connecting = delegate { };
        public event EventHandler Connected = delegate { };
        public event EventHandler TestFailed = delegate { };

        // If IsDirty == false when the user clicks OK to close the form, then we should not display an additional messagebox.
        // Successfully creating or testing the database are the only things that set IsDirty = false.
        // Changing any of the textboxes or deleting the database sets IsDirty = true.
        public bool IsDirty
        {
            get;
            private set;
        }

        // NotBusy is false when the Create/Test/Delete buttons are executing commands. During that time, OK button should be disabled.
        // It's called NotBusy so the IsEnabled binding uses it without converter.
        public bool NotBusy
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostConnectionViewModel"/> class.
        /// </summary>
        /// <param name="configService">The config service.</param>
        /// <param name="events">The events.</param>
        /// <param name="dbCreator">The db creator.</param>
        [ImportingConstructor]
        public HostConnectionViewModel(
            IConfigurationService configService,
            IEventAggregator events,
            [Import(CreatorNames.Sql)] IDatabaseCreator dbCreator)
        {
            ConfigurationService = configService;
            Events = events;
            DbCreator = dbCreator;
            WireCommandsAndEvents();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostConnectionViewModel"/> class.
        /// </summary>
        public HostConnectionViewModel()
        {
            if (IsDesignMode) return;

            ConfigurationService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            //var constring = ConfigurationService.MonahrqConnectionString;
            Events = ServiceLocator.Current.GetInstance<IEventAggregator>();
            DbCreator = ServiceLocator.Current.GetInstance<IDatabaseCreator>(CreatorNames.Sql);
            WireCommandsAndEvents();
        }

        /// <summary>
        /// Wires the commands and events.
        /// </summary>
        private void WireCommandsAndEvents()
        {
            TestCommand = new DelegateCommand(Test);
            CreateCommand = new DelegateCommand(CreateDatabase);
            DeleteCommand = new DelegateCommand(DeleteDatabase);

            OkCommand = new DelegateCommand(() =>
                {
                    var temp = ConfigurationService.ConnectionSettings;
                    temp.ConnectionString = AsBuilder.ConnectionString;
                    ConfigurationService.ConnectionSettings = temp;
                    ConfigurationService.ConnectionSettings = temp;

                    var dataServicesProvider = ServiceLocator.Current.GetInstance<IDataServicesProvider>();

                    using (var con = new SqlConnection(temp.ConnectionString))
                    {
                        //con.Open();
                        dataServicesProvider.UpgradeDatabase(con);
                        //schemaVersion = dataServicesProvider.GetDBSchemaVersion(con);
                    }
                    //var actual = new Version(schemaVersion);
                    //var expected = new Version(ConfigurationService.CurrentSchemaVersion);

                    //if (expected > actual)
                    //{
                    //    var msg =
                    //       string.Format(
                    //            "Your current database \"{0}\" needs to be upgraded. Please click \"Yes\" to upgrade now or \"No\" to create a new database via the database connection manager.",
                    //            AsBuilder.InitialCatalog);

                    //    var msgResult = MessageBox.Show(msg, "MONAHRQ Database Update Required!",
                    //                                    MessageBoxButton.YesNo);

                    //    if (msgResult == MessageBoxResult.Yes)
                    //    {
                    //        MonahrqContext.ForceDbUpGrade = true;
                    //        MonahrqContext.DbNameToUpGrade = AsBuilder.InitialCatalog;
                    //    }
                    //    else
                    //    {
                    //        MonahrqContext.ForceDbRecreate = true;

                    //    }
                    //}
                });

            Load();
            WireEvents();
        }

        // TODO: to fit MVVM, enhance this ViewModel to use a MessageBoxService instead of calling MessageBox directly.
        // This View was using a TextBlock for error/success messages. But I replaced that with MessageBox because the
        // TextBlock had bugs and because MessageBox is standard in SQL connection dialog boxes for success/error messages.

        /// <summary>
        /// Wires the events.
        /// </summary>
        private void WireEvents()
        {
            DbCreator.Created += (o, e) =>
            {
                var temp = ConfigurationService.ConnectionSettings;
                temp.ConnectionString = AsBuilder.ConnectionString;
                ConfigurationService.ConnectionSettings = temp;
                ConnectionMessage = e.Message;
                MessageBox.Show(e.Message, "SQL Server Connection", MessageBoxButton.OK, MessageBoxImage.Information);
                Events.GetEvent<ConnectionSuccessEvent>().Publish(e);
                Connected(this, EventArgs.Empty);

                // only successful create/test can set IsDirty = false
                IsDirty = false;
            };
            DbCreator.CreateFailed += (o, e) =>
            {
                ConnectionMessage = e.ConnectionException.Message;
                MessageBox.Show(e.ConnectionException.Message, "SQL Server Connection", MessageBoxButton.OK, MessageBoxImage.Error);
                Events.GetEvent<ConnectionFailedEvent>().Publish(e);
                Connected(this, EventArgs.Empty);
            };

            DbCreator.Tested += (o, e) =>
            {
                ConnectionMessage = e.Message;
                MessageBox.Show(e.Message, "SQL Server Connection", MessageBoxButton.OK, MessageBoxImage.Information);
                Events.GetEvent<ConnectionSuccessEvent>().Publish(e);
                Connected(this, EventArgs.Empty);

                IsDirty = false;
            };
            DbCreator.TestFailed += (o, e) =>
            {
                ConnectionMessage = e.ConnectionException.Message;
                MessageBox.Show(e.ConnectionException.Message, "SQL Server Connection", MessageBoxButton.OK, MessageBoxImage.Error);
                Events.GetEvent<ConnectionFailedEvent>().Publish(e);
                TestFailed(this, EventArgs.Empty);
                Connected(this, EventArgs.Empty);
            };

            DbCreator.Deleted += (o, e) =>
            {
                ConnectionMessage = e.Message;
                MessageBox.Show(e.Message, "Delete SQL Server Database", MessageBoxButton.OK, MessageBoxImage.Information);
                Events.GetEvent<ConnectionSuccessEvent>().Publish(e);
                Connected(this, EventArgs.Empty);

                IsDirty = true;
            };
            DbCreator.DeleteFailed += (o, e) =>
            {
                ConnectionMessage = e.ConnectionException.Message;
                MessageBox.Show(e.ConnectionException.Message, "Delete SQL Server Database", MessageBoxButton.OK, MessageBoxImage.Error);
                Events.GetEvent<ConnectionFailedEvent>().Publish(e);
                Connected(this, EventArgs.Empty);
            };
        }

        /// <summary>
        /// Gets as builder.
        /// </summary>
        /// <value>
        /// As builder.
        /// </value>
        public SqlConnectionStringBuilder AsBuilder
        {
            get
            {
                var builder = new SqlConnectionStringBuilder
                {
                    ApplicationIntent = ApplicationIntent.ReadWrite,
                    DataSource = Host,
                    InitialCatalog = this.Database,
                    ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName,
                    IntegratedSecurity = UseIntegratedSecurity,
                    PersistSecurityInfo = true,
                    AsynchronousProcessing = true,
                    Pooling = true,
                    MaxPoolSize = 40
                };

                if (!builder.IntegratedSecurity)
                {
                    builder.UserID = User;
                    builder.Password = Password;
                }


                return builder;
            }
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        public void Load()
        {
            var connectionString = ConfigurationService.ConnectionSettings.ConnectionString;
            var builder = new SqlConnectionStringBuilder(connectionString);
            Host = builder.DataSource;
            Database = builder.InitialCatalog;
            UseIntegratedSecurity = builder.IntegratedSecurity;
            User = UseIntegratedSecurity ? string.Empty : builder.UserID;
            Password = UseIntegratedSecurity ? string.Empty : builder.Password;

            IsDirty = true;
            NotBusy = true;
        }

        /// <summary>
        /// Tests this instance.
        /// </summary>
        private void Test()
        {
            NotBusy = false;
            Connecting(this, EventArgs.Empty);
            DbCreator.Test(AsBuilder.ConnectionString);
            NotBusy = true;
        }

        /// <summary>
        /// Creates the database.
        /// </summary>
        public void CreateDatabase()
        {
            NotBusy = false;
            Connecting(this, EventArgs.Empty);
            DbCreator.Create(AsBuilder.ConnectionString, ConfigurationService.CurrentSchemaVersion);
            NotBusy = true;
        }

        // Delete the database if the user confirms
        public void DeleteDatabase()
        {
            NotBusy = false;
            if (MessageBox.Show(
                string.Format("Are you sure you want to delete database '{0}' on server: {1}?", AsBuilder.InitialCatalog, AsBuilder.DataSource),
                "Delete SQL Server Database?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Connecting(this, EventArgs.Empty);
                DbCreator.Delete(AsBuilder.ConnectionString);
            }
            NotBusy = true;
        }

        // These are the commands on the HostConnectionView user control
        public DelegateCommand TestCommand { get; set; }
        public DelegateCommand CreateCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }

        // These are the commands on the GetDatabaseConnection window that hosts the user control
        public DelegateCommand OkCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        public IEventAggregator Events { get; set; }

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        /// <value>
        /// The host.
        /// </value>
        string _Host;
        public string Host
        {
            get { return _Host; }
            set
            {
                SetProperty(ref _Host, value);
            }
        }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        string _User;
        public string User
        {
            get { return _User; }
            set
            {
                SetProperty(ref _User, value);
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        string _Password;
        public string Password
        {
            get { return _Password; }
            set
            {
                SetProperty(ref _Password, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use integrated security].
        /// </summary>
        /// <value>
        /// <c>true</c> if [use integrated security]; otherwise, <c>false</c>.
        /// </value>
        bool _UseIntegratedSecurity;
        public bool UseIntegratedSecurity
        {
            get { return _UseIntegratedSecurity; }
            set
            {
                SetProperty(ref _UseIntegratedSecurity, value);
            }
        }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        string _Database;
        public string Database
        {
            get { return _Database; }
            set
            {
                SetProperty(ref _Database, value);
            }
        }

        /// <summary>
        /// Gets or sets the last error.
        /// </summary>
        /// <value>
        /// The last error.
        /// </value>
        public string ConnectionMessage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is design mode.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is design mode; otherwise, <c>false</c>.
        /// </value>
        bool IsDesignMode
        {
            get
            {
                return System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            }
        }

        /// <summary>
        /// Gets or sets the db creator.
        /// </summary>
        /// <value>
        /// The db creator.
        /// </value>
        IDatabaseCreator DbCreator { get; set; }

        private IConfigurationService ConfigurationService { get; set; }

        // This is for INotifyPropertyChanged with .NET 4.5 CallerMemberName
        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                IsDirty = true;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }
    }
}
