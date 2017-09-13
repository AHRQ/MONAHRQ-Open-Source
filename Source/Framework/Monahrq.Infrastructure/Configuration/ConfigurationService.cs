using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Linq;

namespace Monahrq.Infrastructure.Configuration
{
    /// <summary>
    /// The monahrq configuration service
    /// </summary>
    [Export(typeof(IConfigurationService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class ConfigurationService : IConfigurationService
    {
        private MonahrqConfigurationSection _monahrqConfigurationSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
        /// </summary>
        public ConfigurationService()
        {
            Load();
        }

        /// <summary>
        /// Gets the current schema version. When we upgrade the DB schema version we generally just increase the milestone. Only on majar upgrades do we 
        /// change the major version. Minor version are generally not increased as that is deemed a major upgrade.
        /// </summary>
        /// <value>
        /// The current schema version.
        /// </value>
        public string CurrentSchemaVersion
        {
            get
            {
                return "7.4.0.6";
            }
        }

        /// <summary>
        /// Gets or sets the last data folder.
        /// </summary>
        /// <value>
        /// The last data folder.
        /// </value>
        public string LastDataFolder
        {
            get
            {
                var directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (Directory.Exists(_monahrqConfigurationSettings.LastFolder))
                    directoryPath = _monahrqConfigurationSettings.LastFolder;

                return directoryPath;
            }
            set
            {
                if (_monahrqConfigurationSettings.LastFolder == value) return;

                _monahrqConfigurationSettings.LastFolder = value;
                Save(_monahrqConfigurationSettings);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [data access components installed].
        /// </summary>
        /// <value>
        /// <c>true</c> if [data access components installed]; otherwise, <c>false</c>.
        /// </value>
        public bool DataAccessComponentsInstalled
        {
            get
            {
                return _monahrqConfigurationSettings.DataAccessComponentsInstalled;
            }
            set
            {
                if (_monahrqConfigurationSettings.DataAccessComponentsInstalled == value) return;

                _monahrqConfigurationSettings.DataAccessComponentsInstalled = value;
                Save(_monahrqConfigurationSettings);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use API for physicians].
        /// </summary>
        /// <value>
        /// <c>true</c> if [use API for physicians]; otherwise, <c>false</c>.
        /// </value>
        public bool UseApiForPhysicians
        {
            get
            {
                return _monahrqConfigurationSettings.UseApiForPhysicians;
            }
            set
            {
                if (_monahrqConfigurationSettings.UseApiForPhysicians == value) return;

                _monahrqConfigurationSettings.UseApiForPhysicians = value;
                Save(_monahrqConfigurationSettings);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [rebuild database].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [rebuild database]; otherwise, <c>false</c>.
        /// </value>
        public bool RebuildDatabase
        {
            get
            {
                return _monahrqConfigurationSettings.RebuildDatabase;
            }
            set
            {
                if (_monahrqConfigurationSettings.UseApiForPhysicians == value) return;

                _monahrqConfigurationSettings.RebuildDatabase = value;
                Save(_monahrqConfigurationSettings);
            }
        }

        /// <summary>
        /// Gets or sets the connection settings.
        /// </summary>
        /// <value>
        /// The connection settings.
        /// </value>
        public ConnectionStringSettings ConnectionSettings
        {
            get
            {
                ForceRefresh();
                var element = _monahrqConfigurationSettings.EntityConnectionSettings ?? ConnectionStringSettingsElement.Default;
                return new ConnectionStringSettings("Monahrq", element.ConnectionString, element.ProviderName);
            }
            set
            {
                var element = new ConnectionStringSettingsElement
                {
                    ConnectionString = value.ConnectionString,
                    ProviderName = value.ProviderName
                };

                if (_monahrqConfigurationSettings.EntityConnectionSettings.ConnectionString == element.ConnectionString) return;

                _monahrqConfigurationSettings.EntityConnectionSettings = element;
                Save(_monahrqConfigurationSettings);
            }
        }

        /// <summary>
        /// Gets the default WinQI connection string.
        /// </summary>
        /// <value>
        /// The default WinQI connection string.
        /// </value>
        public ConnectionStringSettings DefaultWinQiConnectionString
        {
            get
            {
                var element = ConnectionStringSettingsElement.WinQiDefault;
                return new ConnectionStringSettings("WinQi", element.ConnectionString, element.ProviderName);
            }
        }

        /// <summary>
        /// Gets or sets the WinQI connection settings.
        /// </summary>
        /// <value>
        /// The WinQI connection settings.
        /// </value>
        public ConnectionStringSettings WinQiConnectionSettings
        {
            get
            {
                var element = _monahrqConfigurationSettings.WinQiConnectionSettings;

                if (element != null && !string.IsNullOrEmpty(element.ConnectionString))
                    return new ConnectionStringSettings("WinQi", element.ConnectionString, element.ProviderName);

                return null;
            }
            set
            {
                var element = new ConnectionStringSettingsElement
                {
                    ConnectionString = value.ConnectionString,
                    ProviderName = value.ProviderName
                };
                if (_monahrqConfigurationSettings.WinQiConnectionSettings.ConnectionString == element.ConnectionString) return;

                _monahrqConfigurationSettings.WinQiConnectionSettings = element;
                Save(_monahrqConfigurationSettings);
            }
        }

        #region HospitalRegion Items.
        /// <summary>
        /// Gets or sets the hospital region.
        /// </summary>
        /// <value>
        /// The hospital region.
        /// </value>
        public HospitalRegionElement HospitalRegion
        {
            get
            {
                if (_monahrqConfigurationSettings.HospitalRegion == null)
                    _monahrqConfigurationSettings.HospitalRegion = new HospitalRegionElement();
                return _monahrqConfigurationSettings.HospitalRegion;
            }
            set
            {
                if (_monahrqConfigurationSettings.HospitalRegion.Equals(value)) return;

                _monahrqConfigurationSettings.HospitalRegion = value;
            }
        }
        #endregion


        public void ForceRefresh()
        {
            _monahrqConfigurationSettings = null;
            Load();
        }

        /// <summary>
        /// Loads an instance of the monahrq settings configuration if not already loaded.
        /// </summary>
        private void Load()
        {
            if (_monahrqConfigurationSettings == null)
                _monahrqConfigurationSettings = MonahrqConfiguration.SettingsGroup.MonahrqSettings();
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public IMonahrqSettings MonahrqSettings
        {
            get
            {
                return _monahrqConfigurationSettings;
            }
        }

        #region Save Methods.
        /// <summary>
        /// Saves the monahrq settings instance.
        /// </summary>
        public void Save()
        {
            Save(_monahrqConfigurationSettings);
        }
        /// <summary>
        /// Saves the specified monahrq settings instance.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void Save(IMonahrqSettings settings)
        {
            MonahrqConfiguration.Save(settings);
            ForceRefresh();
        }
        /// <summary>
        /// Saves the specified named connection element.
        /// </summary>
        /// <param name="namedConnectionElement">The named connection element.</param>
        public void Save(NamedConnectionElement namedConnectionElement)
        {
            var current = _monahrqConfigurationSettings.NamedConnections.OfType<NamedConnectionElement>().FirstOrDefault(elem => elem.Name == namedConnectionElement.Name);
            if (current != null)
            {
                _monahrqConfigurationSettings.NamedConnections.Remove(current);
            }
            _monahrqConfigurationSettings.NamedConnections.Add(namedConnectionElement);
            MonahrqConfiguration.Save(_monahrqConfigurationSettings);
        }
        #endregion

        /// <summary>
        /// Gets the entity provider factory.
        /// </summary>
        /// <value>
        /// The entity provider factory.
        /// </value>
        public DbProviderFactory EntityProviderFactory
        {
            get
            {
                return DbProviderFactories.GetFactory(ConnectionSettings.ProviderName);
            }
        }
    }

    /// <summary>
    /// The monahrq connection string settings.
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationElement" />
    public partial class ConnectionStringSettingsElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStringSettingsElement"/> class.
        /// </summary>
        public ConnectionStringSettingsElement() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStringSettingsElement"/> class.
        /// </summary>
        /// <param name="connecitonStirng">The conneciton stirng.</param>
        /// <param name="providerName">Name of the provider.</param>
        public ConnectionStringSettingsElement(string connecitonStirng, string providerName)
        {
            ProviderName = providerName;
            ConnectionString = connecitonStirng;
        }

        /// <summary>
        /// Gets the default connectionstring setting.
        /// </summary>
        /// <value>
        /// The default.
        /// </value>
        public static ConnectionStringSettingsElement Default
        {
            get
            {
                return new ConnectionStringSettingsElement(string.Empty, "System.Data.SqlClient");
            }
        }

        /// <summary>
        /// Gets the win qi default.
        /// </summary>
        /// <value>
        /// The win qi default.
        /// </value>
        public static ConnectionStringSettingsElement WinQiDefault
        {
            get
            {
                return new ConnectionStringSettingsElement(@"Data Source=.\SqlExpress;Initial Catalog=qualityindicators;Integrated Security=True", @"System.Data.SqlClient");
            }
        }
    }
}
