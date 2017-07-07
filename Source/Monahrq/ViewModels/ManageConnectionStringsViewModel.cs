using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Types;
using Monahrq.Sdk.ViewModels;
using PropertyChanged;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;
using System.Runtime.Serialization;

namespace Monahrq.ViewModels
{
    /// <summary>
    /// View model class of managing the connection string
    /// </summary>
    /// <seealso cref="Monahrq.ViewModels.SettingsViewModel" />
    [Export(typeof(ManageConnectionStringsViewModel)), ImplementPropertyChanged]
    [DataContract]
    [Serializable]
    public class ManageConnectionStringsViewModel : SettingsViewModel
    {
        #region Fields and Constants

        private bool _isloading;
        private string _serverName;
        private string _databaseName;
        private string _dbUserName;
        private string _dbUserPassword;
        private bool _isSqlServerSecurity;
        private bool _isActive;
        private SelectListItem _selectedConnStrName;
       // public event EventHandler IsActiveChanged;
        private string _originalHashValue;

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the save command.
        /// </summary>
        /// <value>
        /// The save command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand SaveCommand { get; set; }

        /// <summary>
        /// Gets or sets the test command.
        /// </summary>
        /// <value>
        /// The test command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand TestCommand { get; set; }

        /// <summary>
        /// Gets or sets the cancel command.
        /// </summary>
        /// <value>
        /// The cancel command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand CancelCommand { get; set; }

        /// <summary>
        /// Gets or sets the commplete command.
        /// </summary>
        /// <value>
        /// The commplete command.
        /// </value>
        [IgnoreDataMember]
        public DelegateCommand CommpleteCommand { get; set; }

        #endregion

        #region Imports

        [Import]
        [IgnoreDataMember]
        IConfigurationService ConfigurationService { get; set; }

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
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        [IgnoreDataMember]
        IRegionManager RegionManager { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public override int Index { get { return 1; } set {} }

        /// <summary>
        /// Gets as builder.
        /// </summary>
        /// <value>
        /// As builder.
        /// </value>
        [DataMember]
        SqlConnectionStringBuilder AsBuilder { get; set; }

        /// <summary>
        /// Gets or sets the database operations.
        /// </summary>
        /// <value>
        /// The database operations.
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
        /// Gets or sets the database operations.
        /// </summary>
        /// <value>
        /// The database operations.
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
        /// Gets or sets the database operations.
        /// </summary>
        /// <value>
        /// The database operations.
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
        /// Gets or sets the database operations.
        /// </summary>
        /// <value>
        /// The database operations.
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
        /// Gets or sets a value indicating whether [is SQL server security].
        /// </summary>
        /// <value>
        /// <c>true</c> if [is SQL server security]; otherwise, <c>false</c>.
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
        /// Gets a value indicating whether [show SQL credentials panel].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show SQL credentials panel]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSqlCredentialsPanel { get { return IsSqlServerSecurity; } }

        [DataMember]
        public ObservableCollection<SelectListItem> ApplicableConnStrings { get; set; }

        [DataMember]
        public SelectListItem SelectedConnStr
        {
            get { return _selectedConnStrName; }
            set
            {
                _selectedConnStrName = value;

                if (_selectedConnStrName != null && _selectedConnStrName.Value != null)
                    OnConnectionStringChanged(_selectedConnStrName.Value.ToString());
            }
        }

        //[DataMember]
        //public bool IsActive
        //{
        //    get { return _isActive; }
        //    set
        //    {
        //        _isActive = value;

        //        if (_isActive)
        //        {
        //            OnIsActiveChanged(this, new EventArgs());
        //        }
        //    }
        //}

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

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageConnectionStringsViewModel"/> class.
        /// </summary>
        public ManageConnectionStringsViewModel()
        {
            AsBuilder = new SqlConnectionStringBuilder();
            IsActiveChanged += OnIsActiveChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads this instance.
        /// </summary>
        private void OnConnectionStringChanged(string selectedConnStrName)
        {
            if (selectedConnStrName == null)
                return;

            _isloading = true;

            var connectionString = string.Empty;
            if (selectedConnStrName.EqualsIgnoreCase(ConnectionStringKeyEnum.WinQI.ToString()))
            {
                var connection = ConfigurationService.WinQiConnectionSettings ?? ConfigurationService.DefaultWinQiConnectionString;
                connectionString = connection.ConnectionString;
            }
            // TODO: Later when refactoring we will add functionality to add name valued pair connection strings

            AsBuilder = new SqlConnectionStringBuilder(connectionString);

            ServerName = AsBuilder.DataSource;
            DatabaseName = AsBuilder.InitialCatalog;
            IsSqlServerSecurity = !AsBuilder.IntegratedSecurity;
            DbUserName = IsSqlServerSecurity ? AsBuilder.UserID : string.Empty;
            DbUserPassword = IsSqlServerSecurity ? AsBuilder.Password : string.Empty;

            _isloading = false;
        }

        /// <summary>
        /// Sets the conntection string.
        /// </summary>
        public void SetConntectionString()
        {
            AsBuilder = new SqlConnectionStringBuilder();
            AsBuilder.DataSource = ServerName;
            AsBuilder.InitialCatalog = DatabaseName;
            AsBuilder.IntegratedSecurity = !IsSqlServerSecurity;
            AsBuilder.UserID = IsSqlServerSecurity ? DbUserName : string.Empty;
            AsBuilder.Password = IsSqlServerSecurity ? DbUserPassword : string.Empty;
        }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            TestCommand = new DelegateCommand(OnTest, () => true);
            SaveCommand = new DelegateCommand(OnSave, () => true);
            CancelCommand = new DelegateCommand(OnCancel, () => true);
            CommpleteCommand = new DelegateCommand(OnComplete, () => true);

            //InitialLoad();
        }

        /// <summary>
        /// Initials the load.
        /// </summary>
        private void InitialLoad()
        {
            ApplicableConnStrings = new ObservableCollection<SelectListItem>
                {
                   // _defaultListItem,
                    new SelectListItem { IsSelected = false, Text = ConnectionStringKeyEnum.WinQI.GetDescription(), Value = ConnectionStringKeyEnum.WinQI.ToString()}
                };

            SelectedConnStr = ApplicableConnStrings[0]; // _defaultListItem;
        }

        /// <summary>
        /// Called when [complete].
        /// </summary>
        private void OnComplete()
        {
            var canLeave = true;
            var isDirty = GetViewModelHashCode(this, GetType()) != OriginalHashValue;

            if (isDirty && MessageBox.Show("The data has been edited. Are you sure you want to leave before saving?", "Modification Verification", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            ResetAll(true);
            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri("WelcomeView", UriKind.Relative));
        }

        /// <summary>
        /// Called when [save].
        /// </summary>
        public override void OnSave()
        {
            if (!OnValidate())
                return;

            // var temp = ConfigurationService.WinQiConnectionSettings;

            SetConntectionString();

            var temp = ConfigurationService.WinQiConnectionSettings ?? new ConnectionStringSettings("WinQi", "", "");
            temp.ConnectionString = AsBuilder.ConnectionString;

            ConfigurationService.WinQiConnectionSettings = temp;
            ConfigurationService.WinQiConnectionSettings = temp;

            OriginalHashValue = GetViewModelHashCode(this, GetType());

            EventAggregator.GetEvent<GenericNotificationEvent>().Publish(string.Format("The connection for '{0}' was successfully saved.", SelectedConnStr.Text.SubStrBefore(" - ")));
        }

        /// <summary>
        /// Called when [test].
        /// </summary>
        private void OnTest()
        {
            if (OnValidate())
            //{
            //    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(new Exception(string.Format("Fialed to connect to database '{0}' with the provider connection information.", SelectedConnStr.Text.SubStrBefore(" - "))));
            //}
            //else
            {
                EventAggregator.GetEvent<GenericNotificationEvent>().Publish(string.Format("Successfully connected to database '{0}' with the provided connection information.", SelectedConnStr.Text.SubStrBefore(" - ")));
            }


            //SetConntectionString();
        }

        /// <summary>
        /// Tests the connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        private bool TestConnection(string connectionString)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Resets all.
        /// </summary>
        /// <param name="resetSelectedItem">if set to <c>true</c> [reset selected item].</param>
        private void ResetAll(bool resetSelectedItem = false)
        {
            ServerName = null;
            DatabaseName = null;
            IsSqlServerSecurity = false;
            DbUserName = null;
            DbUserPassword = null;

            if (resetSelectedItem)
                SelectedConnStr = _selectedConnStrName;

            InitialLoad();
        }

        /// <summary>
        /// Called when [cancel].
        /// </summary>
        public override void OnCancel()
        {
            var isDirty = GetViewModelHashCode(this, GetType()) != OriginalHashValue;
            if (isDirty && MessageBox.Show("The data has been edited. Are you sure you want to leave before saving?", "Modification Verification", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            ResetAll(true);
        }

        /// <summary>
        /// Called when [reset].
        /// </summary>
        public override void OnReset()
        {
            ResetAll(true);
            //
        }

        /// <summary>
        /// Called when [is active changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        public override void OnIsActiveChanged(object sender, EventArgs eventArgs)
        {
            
            ResetAll();

            if(IsActive)
                InitialLoad();
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            //InitialLoad();
        }

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        ///   <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            // throw new NotImplementedException();
            return true;
        }

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            ResetAll();
        }

        /// <summary>
        /// Called when [validate].
        /// </summary>
        /// <returns></returns>
        public bool OnValidate()
        {
            if (string.IsNullOrEmpty(ServerName))
            {
                MessageBox.Show("Please provide a server name.", "Validation Error", MessageBoxButton.OK);
                return false;
            }

            if (string.IsNullOrEmpty(DatabaseName))
            {
                MessageBox.Show("Please provide a database name.", "Validation Error", MessageBoxButton.OK);
                return false;
            }

            if (IsSqlServerSecurity)
            {
                if (string.IsNullOrEmpty(DbUserName))
                {
                    MessageBox.Show("Please provide the SQL server username.", "Validation Error", MessageBoxButton.OK);
                    return false;
                }

                if (string.IsNullOrEmpty(DbUserPassword))
                {
                    MessageBox.Show("Please provide the SQL server password.", "Validation Error", MessageBoxButton.OK);
                    return false;
                }
            }

            SetConntectionString();

            if (!TestConnection(AsBuilder.ConnectionString))
            {
                //MessageBox.Show("The database does not exists. Please provide the connection information for a database that currently exists.", "Validation Error", MessageBoxButton.OK);
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(new Exception(string.Format("Failed to connect to database '{0}' with the provided connection information.", SelectedConnStr.Text.SubStrBefore(" - "))));
                return false;
            }

            return true;
        }

        #endregion

    }

    /// <summary>
    /// Connection string key enum
    /// </summary>
    [Serializable]
    public enum ConnectionStringKeyEnum
    {
        [Description("AHRQ Quality Indicator Database - (Used by MONAHRQ to get data used in cost savings for Avoidable Stays calculations)")]
        WinQI
    }
}
