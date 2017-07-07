using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Logging;

namespace Monahrq.Infrastructure.Configuration
{
    /// <summary>
    /// The Monahrq configuation class. This is the entry point into the custom application/user configuration classes
    /// </summary>
    public static class MonahrqConfiguration
    {
        /// <summary>
        /// The logger
        /// </summary>
        static internal ILoggerFacade Logger = new SessionLogger(new CallbackLogger());


        /// <summary>
        /// Configurations the debug log.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="message">The message.</param>
        static internal void ConfigDebugLog(String method, String message)
        {
            if (Logger == null)
                return;
            Logger.Log(
                String.Format("MonahrqConfig::{0}(): {1}", method, message),
                Category.Info,
                Priority.High);
        }

        /// <summary>
        /// Tries to find user.config file in User's Roaming/Local folders.  (This folder is versioned).  If
        /// the current version of the app's folder doesn't contain the user.config file, the file is copied
        /// from the previous versioned folder to the current.
        /// Next the user.config file is opened and an attempt to read the MonahrqConfigurationSectionGroup
        /// from the file is made.  If the attempt fails, an Execption is thrown; at which point the catch
        /// block is used to remove the SectionGroup from the file and resave the user.config file without it.
        /// </summary>
        static void MonahrqConfigurationX()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                if (!File.Exists(config.FilePath))
                {
                    var dirPath = config.FilePath.Replace("\\user.config", "\\..");
                    if (Directory.Exists(dirPath))
                    {
                        List<string> dirs = new List<string>(Directory.EnumerateDirectories(dirPath));
                        var versions = dirs.Select(d => new Version(d.Substring(d.LastIndexOf("\\") + 1))).ToList();
                        var latestVersionIndex = versions.IndexOf(versions.Max());
                        if (latestVersionIndex > -1)
                        {
                            FileInfo finfo = new FileInfo(dirs[latestVersionIndex] + "\\user.config");
                            Directory.CreateDirectory(config.FilePath.Replace("\\user.config", ""));
                            finfo.CopyTo(config.FilePath);
                        }
                    }
                }
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                var group = config.GetMonahrqSectionGroup();
                var temp = group.Sections.Get(MonahrqConfigurationSection.MonahrqConfigurationSectionSectionName)
                    as MonahrqConfigurationSection;

            }
            catch (ConfigurationErrorsException ex)
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                var group = config.GetMonahrqSectionGroup();
                group.Sections.Remove(MonahrqConfigurationSection.MonahrqConfigurationSectionSectionName);
                config.Save(ConfigurationSaveMode.Minimal);
            }
        }
        /// <summary>
        /// Initializes the <see cref="MonahrqConfiguration"/> class.
        /// </summary>
        static MonahrqConfiguration()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    //	Create Config Directory if not present.
                    if (!Directory.Exists(ConfigDirectoryPath))
                    {
                        Directory.CreateDirectory(ConfigDirectoryPath);
                    }

                    //	Copy the config file from the User Roaming Directory, if present.
                    var curUserConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                    if (File.Exists(curUserConfig.FilePath))
                    {
                        FileInfo finfo = new FileInfo(curUserConfig.FilePath);
                        finfo.CopyTo(ConfigFilePath);
                    }
                    //	File not present in User Roamin Directory; try to find it in the last 'versioned' directory for app.
                    else
                    {
                        var dirPath = curUserConfig.FilePath.Replace("\\user.config", "\\..");
                        if (Directory.Exists(dirPath))
                        {
                            List<string> dirs = new List<string>(Directory.EnumerateDirectories(dirPath));
                            var versions = dirs.Select(d => new Version(d.Substring(d.LastIndexOf("\\") + 1))).ToList();
                            var latestVersionIndex = versions.IndexOf(versions.Max());
                            if (latestVersionIndex > -1)
                            {
                                //FileInfo finfo = new FileInfo(dirs[latestVersionIndex] + "\\user.config");
                                //finfo.CopyTo(ConfigFilePath);

                                var monahrqConfig = UserSettingsConfig.GetMonahrqSectionGroup().Sections.Get(MonahrqConfigurationSection.MonahrqConfigurationSectionSectionName)
                    as MonahrqConfigurationSection;
                                TransformToNewConfig(dirs[latestVersionIndex] + "\\user.config", ConfigFilePath, ref monahrqConfig);

                                Save(monahrqConfig);
                            }
                        }
                    }
                }

                //  Test that section is valid.
                var config = UserSettingsConfig;
                var group = config.GetMonahrqSectionGroup();
                var tempMonahrqSetcion = group.Sections.Get(MonahrqConfigurationSection.MonahrqConfigurationSectionSectionName)
                    as MonahrqConfigurationSection;

                //if (tempMonahrqSetcion != null && !string.IsNullOrEmpty(tempMonahrqSetcion.UpdateScriptToRunAtStartup))
                //    config.Save();
            }
            catch (ConfigurationErrorsException ex)
            {
                var config = UserSettingsConfig;
                var group = config.GetMonahrqSectionGroup();
                group.Sections.Remove(MonahrqConfigurationSection.MonahrqConfigurationSectionSectionName);
                config.Save(ConfigurationSaveMode.Minimal);
            }
            catch (Exception ex)
            {
                var config = UserSettingsConfig;
                var group = config.GetMonahrqSectionGroup();
                group.Sections.Remove(MonahrqConfigurationSection.MonahrqConfigurationSectionSectionName);
                config.Save(ConfigurationSaveMode.Minimal);
            }

            ConfigDebugLog("MonahrqConfiguration.Ctor", String.Format(""));//States: {0}", DefaultStateCount));

        }

        /// <summary>
        /// Transforms to new configuration.
        /// </summary>
        /// <param name="oldConfigFilePath">The old configuration file path.</param>
        /// <param name="monahrqConfigurationFilePath">The monahrq configuration file path.</param>
        /// <param name="monahrqConfiguration">The monahrq configuration.</param>
        /// <exception cref="ArgumentNullException">
        /// oldConfigFilePath;oldConfigFilePath can not be null.
        /// or
        /// monahrqConfigurationFilePath;monahrqConfigurationFilePath can not be null.
        /// </exception>
        private static void TransformToNewConfig(string oldConfigFilePath, string monahrqConfigurationFilePath, ref MonahrqConfigurationSection monahrqConfiguration)
        {
            if (string.IsNullOrEmpty(oldConfigFilePath))
                throw new ArgumentNullException("oldConfigFilePath", @"oldConfigFilePath can not be null.");
            if (string.IsNullOrEmpty(monahrqConfigurationFilePath))
                throw new ArgumentNullException("monahrqConfigurationFilePath", @"monahrqConfigurationFilePath can not be null.");

            if (monahrqConfiguration == null)
                monahrqConfiguration = new MonahrqConfigurationSection();

            if (monahrqConfiguration.SectionInformation.AllowExeDefinition != ConfigurationAllowExeDefinition.MachineToLocalUser)
                monahrqConfiguration.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;

            var configDoc = XDocument.Load(oldConfigFilePath);

            var rootElement = configDoc.Element("configuration");
            if (rootElement != null)
            {
                var monahrqConfigSectionElement = rootElement.Descendants(XName.Get("MonahrqConfigurationSection")).First();
                if (monahrqConfigSectionElement != null)
                {
                    monahrqConfiguration.RebuildDatabase = bool.Parse(monahrqConfigSectionElement.Attribute("rebuildDatabase").Value);
                    monahrqConfiguration.LongTimeout = TimeSpan.Parse(monahrqConfigSectionElement.Attribute("longTimeout").Value);
                    monahrqConfiguration.ShortTimeout = TimeSpan.Parse(monahrqConfigSectionElement.Attribute("shortTimeout").Value);
                    monahrqConfiguration.LastFolder = monahrqConfigSectionElement.Attribute("lastFolder").Value;
                    monahrqConfiguration.BatchSize = int.Parse(monahrqConfigSectionElement.Attribute("batchSize").Value);
                    monahrqConfiguration.DebugSql = bool.Parse(monahrqConfigSectionElement.Attribute("debugSql").Value);
                    monahrqConfiguration.DataAccessComponentsInstalled =
                        bool.Parse(monahrqConfigSectionElement.Attribute("dataAccessComponentsInstalled").Value);
                    monahrqConfiguration.UseApiForPhysicians =
                        bool.Parse(monahrqConfigSectionElement.Attribute("useApiForPhysicians").Value);

                    monahrqConfiguration.UpdateScriptToRunAtStartup =
                        monahrqConfigSectionElement.Attribute("updateScriptToRunAtStartup").Value;

                    if (monahrqConfigSectionElement.HasElements)
                    {
                        if (monahrqConfiguration.EntityConnectionSettings == null)
                            monahrqConfiguration.EntityConnectionSettings = new ConnectionStringSettingsElement();

                        var entityDbSettings = monahrqConfigSectionElement.Element("entityConnectionSettings");

                        if (entityDbSettings != null)
                        {
                            monahrqConfiguration.EntityConnectionSettings.ConnectionString =
                                entityDbSettings.Attribute("connectionString").Value;
                            monahrqConfiguration.EntityConnectionSettings.ProviderName =
                                entityDbSettings.Attribute("providerName").Value;
                        }

                        if (monahrqConfiguration.WinQiConnectionSettings == null)
                            monahrqConfiguration.WinQiConnectionSettings = new ConnectionStringSettingsElement();

                        var winQIDbSettings = monahrqConfigSectionElement.Element("winQiConnectionSettings");

                        if (winQIDbSettings != null)
                        {
                            monahrqConfiguration.WinQiConnectionSettings.ConnectionString =
                                winQIDbSettings.Attribute("connectionString").Value;
                            monahrqConfiguration.WinQiConnectionSettings.ProviderName =
                                winQIDbSettings.Attribute("providerName").Value;
                        }
                    }
                }

                var hospitalRegionElement = rootElement.Descendants(XName.Get("Monahrq.Sdk.Modules.Settings.HospitalRegion")).First();
                if (hospitalRegionElement != null && hospitalRegionElement.HasElements)
                {
                    if (monahrqConfiguration.HospitalRegion == null) monahrqConfiguration.HospitalRegion = new HospitalRegionElement();

                    var regionTypeSetting =
                        hospitalRegionElement.Elements()
                            .FirstOrDefault(e => e.Attribute("name").Value == "DefaultRegionTypeName");
                    if (regionTypeSetting != null)
                        monahrqConfiguration.HospitalRegion.SelectedRegionType = Type.GetType(regionTypeSetting.Value);

                    var defaultStatesSetting =
                        hospitalRegionElement.Elements().FirstOrDefault(e => e.Attribute("name").Value == "DefaultStates");
                    if (defaultStatesSetting != null && !string.IsNullOrEmpty(defaultStatesSetting.Element("value").Value))
                    {
                        var statesXmlDoc = new XmlDocument();
                        statesXmlDoc.LoadXml(defaultStatesSetting.Element("value").Element("ArrayOfString").ToString());
                        var states = XmlHelper.Deserialize<List<string>>(statesXmlDoc).ToArray();
                        monahrqConfiguration.HospitalRegion.DefaultStates = new StringCollection();
                        monahrqConfiguration.HospitalRegion.DefaultStates.AddRange(states);
                    }
                    else
                    {
                        monahrqConfiguration.HospitalRegion.DefaultStates = new StringCollection();
                    }
                }
            }

            //return monahrqConfiguration;
        }

        /// <summary>
        /// Gets the configuration directory path.
        /// </summary>
        /// <value>
        /// The configuration directory path.
        /// </value>
        public static String ConfigDirectoryPath
        {
            get { return Path.Combine(MonahrqContext.MyDocumentsApplicationDirPath, "Configure"); }
        }
        /// <summary>
        /// Gets the configuration file path.
        /// </summary>
        /// <value>
        /// The configuration file path.
        /// </value>
        public static String ConfigFilePath
        {
            get { return Path.Combine(ConfigDirectoryPath, "user.config"); }
        }

        /// <summary>
        /// Gets the application configuration path.
        /// </summary>
        /// <value>
        /// The application configuration path.
        /// </value>
        public static string ApplicationConfigPath
        {
            get { return Path.Combine(MonahrqContext.BinFolderPath, "Monahrq.exe.config"); }
        }

        /// <summary>
        /// Gets the user application data folder.
        /// </summary>
        /// <returns></returns>
        public static string GetUserApplicationDataFolder()
        {
            var assy = Assembly.GetExecutingAssembly();
            //var AssemblyCompany = assy.GetCustomAttribute<AssemblyCompanyAttribute>()
            //    ?? new AssemblyCompanyAttribute("{39049C7F-E216-4404-8FA7-4A213365DD19}");

            var AssemblyProduct = assy.GetCustomAttribute<AssemblyProductAttribute>()
                    ?? new AssemblyProductAttribute("{9022C9CA-A2E7-46CD-9FBE-AE7A77C44F53}");

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                //AssemblyCompany.Company,
                                    AssemblyProduct.Product);

            return path;
        }

        /// <summary>
        /// Gets the user application configuration.
        /// </summary>
        /// <value>
        /// The user application configuration.
        /// </value>
        public static string UserApplicationConfig
        {
            get { return Path.Combine(GetUserApplicationDataFolder(), "user.config"); }
        }


        /// <summary>
        /// Gets the settings group.
        /// </summary>
        /// <value>
        /// The settings group.
        /// </value>
        public static ConfigurationSectionGroup SettingsGroup
        {
            get
            {
                return UserSettingsGroup;

                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                return config.GetMonahrqSectionGroup();
            }
        }

        /// <summary>
        /// The user settings configuration
        /// </summary>
        private static System.Configuration.Configuration _userSettingsConfig;

        /// <summary>
        /// Gets the USER settings group.
        /// </summary>
        /// <value>
        /// The settings group.
        /// </value>
        public static System.Configuration.Configuration UserSettingsConfig
        {
            get
            {
                if (_userSettingsConfig != null) return _userSettingsConfig;

                ExeConfigurationFileMap configFile = new ExeConfigurationFileMap();
                configFile.ExeConfigFilename = ApplicationConfigPath;
                configFile.LocalUserConfigFilename = ConfigFilePath;
                configFile.RoamingUserConfigFilename = ConfigFilePath;
                _userSettingsConfig = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.PerUserRoamingAndLocal);

                return _userSettingsConfig;
            }
        }

        /// <summary>
        /// The application configuration
        /// </summary>
        private static System.Configuration.Configuration _applicationConfig;

        /// <summary>
        /// Gets the USER settings group.
        /// </summary>
        /// <value>
        /// The settings group.
        /// </value>
        public static System.Configuration.Configuration ApplicationConfig
        {
            get
            {
                if (_applicationConfig != null) return _applicationConfig;

                ExeConfigurationFileMap configFile = new ExeConfigurationFileMap();
                configFile.ExeConfigFilename = ApplicationConfigPath;
                configFile.LocalUserConfigFilename = ConfigFilePath;
                configFile.RoamingUserConfigFilename = ConfigFilePath;
                _applicationConfig = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None);
                return _applicationConfig;
            }
        }


        /// <summary>
        /// Gets the USER settings group.
        /// </summary>
        /// <value>
        /// The settings group.
        /// </value>
        public static ConfigurationSectionGroup UserSettingsGroup
        {
            get
            {
                var userConfig = UserSettingsConfig;
                return userConfig.GetMonahrqSectionGroup();
            }
        }

        /// <summary>
        /// Monahrqs the settings.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public static MonahrqConfigurationSection MonahrqSettings(this ConfigurationSectionGroup group)
        {
            if (_monahrqConfigurationSection == null /*|| _reset*/)
            {
                lock (_objLock)
                {
                    _monahrqConfigurationSection = group.Sections.OfType<MonahrqConfigurationSection>().FirstOrDefault();
                }
                //_reset = false;
            }
            return _monahrqConfigurationSection;
        }

        /// <summary>
        /// Gets the monahrq section group.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns></returns>
        public static ConfigurationSectionGroup GetMonahrqSectionGroup(this System.Configuration.Configuration config)
        {
            var group = config.GetSectionGroup("MonahrqConfigurationSectionGroup");
            if (group == null)
            {
                group = new ConfigurationSectionGroup();
                config.SectionGroups.Add("MonahrqConfigurationSectionGroup", group);
                var section = new MonahrqConfigurationSection();
                section.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;
                group.Sections.Add(MonahrqConfigurationSection.MonahrqConfigurationSectionSectionName, section);

                config.Save(ConfigurationSaveMode.Minimal);
            }
            return group;
        }

        /// <summary>
        /// The object lock
        /// </summary>
        private static readonly object[] _objLock = new object[] { };
        /// <summary>
        /// The monahrq configuration section
        /// </summary>
        private static MonahrqConfigurationSection _monahrqConfigurationSection;
        /// <summary>
        /// The reset
        /// </summary>
        private static bool _reset;

        /// <summary>
        /// Saves the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public static void Save(IMonahrqSettings settings)
        {
            try
            {
                //wha???
                lock (_objLock)
                {
                    ConfigDebugLog("Save", String.Format("States: {0}", settings.HospitalRegion.DefaultStateCount));

                    settings.HospitalRegion.SyncDefaultStatesToConfig();

                    _userSettingsConfig = null;
                    _monahrqConfigurationSection = null;

                    var config = UserSettingsConfig;
                    var group = config.GetMonahrqSectionGroup();
                    var temp = ConnectionStringSettingsElement.Default;
                    temp.ConnectionString = settings.EntityConnectionSettings.ConnectionString;
                    group.MonahrqSettings().EntityConnectionSettings = temp;

                    var temp2 = ConnectionStringSettingsElement.WinQiDefault;
                    temp2.ConnectionString = settings.WinQiConnectionSettings.ConnectionString;
                    group.MonahrqSettings().WinQiConnectionSettings = temp2;

                    group.MonahrqSettings().HospitalRegion.SelectedRegionType = settings.HospitalRegion.SelectedRegionType;
                    group.MonahrqSettings().HospitalRegion.DefaultStates = settings.HospitalRegion.DefaultStates;

                    group.MonahrqSettings().NamedConnections = settings.NamedConnections;
                    group.MonahrqSettings().ShortTimeout = settings.ShortTimeout;
                    group.MonahrqSettings().LongTimeout = settings.LongTimeout;
                    group.MonahrqSettings().LastFolder = settings.LastFolder;
                    group.MonahrqSettings().BatchSize = settings.BatchSize;
                    group.MonahrqSettings().DebugSql = settings.DebugSql;
                    group.MonahrqSettings().RebuildDatabase = settings.RebuildDatabase;
                    group.MonahrqSettings().UseApiForPhysicians = settings.UseApiForPhysicians;
                    group.MonahrqSettings().DataAccessComponentsInstalled = settings.DataAccessComponentsInstalled;
                    group.MonahrqSettings().UpdateScriptToRunAtStartup = settings.UpdateScriptToRunAtStartup;
                    group.MonahrqSettings().IsFirstRun = settings.IsFirstRun;

                    config.Save(ConfigurationSaveMode.Modified);
                }
                Thread.Sleep(300);
            }
            catch (Exception ex)
            {
                ex.GetType();
            }

            _reset = true;
        }

        /// <summary>
        /// Deletes the specified named connection element.
        /// </summary>
        /// <param name="namedConnectionElement">The named connection element.</param>
        public static void Delete(NamedConnectionElement namedConnectionElement)
        {
            var section = SettingsGroup.MonahrqSettings();
            section.NamedConnections.Remove(section.NamedConnections[namedConnectionElement.Name]);
            Save(section);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationElement" />
    public partial class SchemaIniElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaIniElement"/> class.
        /// </summary>
        public SchemaIniElement()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaIniElement"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public SchemaIniElement(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// The custom Named Connection Configuration Element.
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationElement" />
    /// <seealso cref="Monahrq.Infrastructure.Configuration.INamedConnectionElement" />
    public partial class NamedConnectionElement : INamedConnectionElement
    {
        /// <summary>
        /// The null
        /// </summary>
        static NamedConnectionElement _null = new NamedConnectionElement();
        /// <summary>
        /// Gets the empty.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static NamedConnectionElement Empty
        {
            get
            {
                return _null;
            }
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="target">The target.</param>
        public void CopyTo(NamedConnectionElement target)
        {
            target.CopyFrom(this);
        }

        /// <summary>
        /// Copies from.
        /// </summary>
        /// <param name="source">The source.</param>
        private void CopyFrom(NamedConnectionElement source)
        {
            Copy(source, this);
        }

        /// <summary>
        /// Copies the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        public static void Copy(NamedConnectionElement source, NamedConnectionElement target)
        {
            target.Name = source.Name;
            target.ConnectionString = source.ConnectionString;
            target.ControllerType = source.ControllerType;
            target.SelectFrom = source.SelectFrom;
            target.HasHeader = source.HasHeader;
            target.SchemaIniSettings = new SchemaIniElementCollection();
            foreach (var element in source.SchemaIniSettings.OfType<SchemaIniElement>())
            {
                target.SchemaIniSettings.Add(new SchemaIniElement(element.Name, element.Value));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is changed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is changed; otherwise, <c>false</c>.
        /// </value>
        public bool IsChanged { get; set; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                return (Name ?? string.Empty).Trim() != string.Empty;
            }
        }

        /// <summary>
        /// Creates the provider.
        /// </summary>
        /// <returns></returns>
        public object CreateProvider()
        {
            var thetype = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.ExportedTypes)
                .FirstOrDefault(t => t.AssemblyQualifiedName == ControllerType);
            return Activator.CreateInstance(thetype);
        }
    }

    /// <summary>
    /// The custom Monahrq settings element collection class.
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationElementCollection" />
    public partial class MonahrqSettingElementCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonahrqSettingElementCollection"/> class.
        /// </summary>
        public MonahrqSettingElementCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonahrqSettingElementCollection"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public MonahrqSettingElementCollection(MonahrqSettingElementCollection settings)
        {
            settings.OfType<MonahrqSettingElement>().ToList()
                .ForEach(e => this.Add(e.Clone() as MonahrqSettingElement));
        }

        /// <summary>
        /// Applies the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void Apply(string name, string value)
        {
            var element = GetItemByKey(name);
            if (element != null)
            {
                Remove(element);
            }
            else
            {
                element = new MonahrqSettingElement(name, value);
            }
            Add(element);
        }
    }

    /// <summary>
    /// The custom Monahrq settings configuration element.
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationElement" />
    /// <seealso cref="System.ICloneable" />
    public partial class MonahrqSettingElement : ICloneable
    {
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return new MonahrqSettingElement(this.Name, this.Value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonahrqSettingElement"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public MonahrqSettingElement(string name, string value)
        {
            Name = name; Value = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MonahrqSettingElement"/> class.
        /// </summary>
        public MonahrqSettingElement() { }
    }

    /// <summary>
    /// The custom Monahrq Configuration Section
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationSection" />
    /// <seealso cref="Monahrq.Infrastructure.Configuration.IMonahrqSettings" />
    public partial class MonahrqConfigurationSection : IMonahrqSettings
    {
        #region UpdateScriptToRunAtStartup Property
        /// <summary>
        /// The XML name of the <see cref="UpdateScriptToRunAtStartup" /> property.
        /// </summary>
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        internal const string UPDATE_SCRIPT_TO_RUN_AT_STARTUP_PROPERTY = "updateScriptToRunAtStartup";

        /// <summary>
        /// Gets or sets the LastFolder.
        /// </summary>
        /// <value>
        /// The update script to run at startup.
        /// </value>
        [UserScopedSetting]
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        [Description("The update script to run on application startup if need.")]
        [ConfigurationProperty(UPDATE_SCRIPT_TO_RUN_AT_STARTUP_PROPERTY, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "")]
        public virtual string UpdateScriptToRunAtStartup
        {
            get
            {
                return ((string)(base[UPDATE_SCRIPT_TO_RUN_AT_STARTUP_PROPERTY]));
            }
            set
            {
                base[UPDATE_SCRIPT_TO_RUN_AT_STARTUP_PROPERTY] = value;
            }
        }
        #endregion

        #region IsFirstRun Property
        /// <summary>
        /// The XML name of the <see cref="IS_FIRST_RUN" /> property.
        /// </summary>
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        internal const string IS_FIRST_RUN = "isFirstRun";

        /// <summary>
        /// Gets or sets the IsFirstRun.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is first run; otherwise, <c>false</c>.
        /// </value>
        [ApplicationScopedSetting]
        [GeneratedCode("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.1.7")]
        [Description("Is the first Application Run. Can be reset by Monahrq Developers via App.config file.")]
        [ConfigurationProperty(IS_FIRST_RUN, IsRequired = false, IsKey = false, IsDefaultCollection = false)]
        public virtual bool IsFirstRun
        {
            get
            {
                return ((bool)(base[IS_FIRST_RUN]));
            }
            set
            {
                base[IS_FIRST_RUN] = value;
            }
        }
        #endregion
    }
    /// <summary>
    /// The custom Monahrq theme configuration element.
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationElementCollection" />
    public partial class MonahrqThemeElementCollection : ConfigurationElementCollection
    {
        #region Default Theme
        /// <summary>
        /// Gets the default theme.
        /// </summary>
        /// <value>
        /// The default theme.
        /// </value>
        public MonahrqThemeElement DefaultTheme
        {
            get { return this.OfType<MonahrqThemeElement>().FirstOrDefault(t => t.IsDefault); }
        }
        #endregion
    }

    /// <summary>
    /// The custom hospital region configuration element.
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationElement" />
    public partial class HospitalRegionElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalRegionElement"/> class.
        /// </summary>
        public HospitalRegionElement()
        {
            //	this.PropertyChanged += (o, e) =>
            //	{
            //	    if (string.Equals(e.PropertyName, "DefaultStates", StringComparison.OrdinalIgnoreCase))
            //	    {
            //	        InitLazySelectedStates();
            //	    }
            //	};

            //	if (DefaultStates == null)
            //	{
            //		DefaultStates = new StringCollection();
            //	}

        }

        /// <summary>
        /// Gets a value indicating whether this instance is defined.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is defined; otherwise, <c>false</c>.
        /// </value>
        public bool IsDefined
        {
            get
            {
                return SelectedRegionType != typeof(object) && DefaultStates.Count > 0;
            }
        }

        /// <summary>
        /// The default states
        /// </summary>
        private StringCollection _defaultStates;
        /// <summary>
        /// Gets or sets the default states.
        /// </summary>
        /// <value>
        /// The default states.
        /// </value>
        public StringCollection DefaultStates
        {
            get
            {

                if (_defaultStates == null || _defaultStates.Count == 0)
                {
                    SyncDefaultStatesFromConfig();
                }
#if DEBUG
                MonahrqConfiguration.ConfigDebugLog("HospitalRegion.DefaultStates.Get", String.Format("States: {0}", DefaultStateCount));
#endif
                return _defaultStates;
            }
            set
            {
                _defaultStates = value;
                SyncDefaultStatesToConfig();

#if DEBUG
                MonahrqConfiguration.ConfigDebugLog("HospitalRegion.DefaultStates.Set", String.Format("States: {0}", DefaultStateCount));
#endif
            }
        }
        /// <summary>
        /// Gets the default state count.
        /// </summary>
        /// <value>
        /// The default state count.
        /// </value>
        internal int DefaultStateCount
        {
            get
            {
                if (_defaultStates == null || _defaultStates.Count == 0)
                {
                    return 0;
                }
                return _defaultStates.Count;
            }
        }

        /// <summary>
        /// Synchronizes the default states from configuration.
        /// </summary>
        private void SyncDefaultStatesFromConfig()
        {
            _defaultStates = new StringCollection();
            foreach (var stateProxy in DefaultStatesProxy.OfType<StateElement>().ToList())
            {
                _defaultStates.Add(stateProxy.StateName);
            }
            //InitLazySelectedStates();

            MonahrqConfiguration.ConfigDebugLog("HospitalRegion.SyncDefaultStatesFromConfig", String.Format("States: {0}", DefaultStateCount));
        }

        /// <summary>
        /// Synchronizes the default states to configuration.
        /// </summary>
        internal void SyncDefaultStatesToConfig()
        {
            MonahrqConfiguration.ConfigDebugLog("HospitalRegion.SyncDefaultStatesToConfig", String.Format("States: {0}", DefaultStateCount));

            if (_defaultStates == null)
                return;

            DefaultStatesProxy = new StatesCollectionElement();
            foreach (var state in _defaultStates)
            {
                DefaultStatesProxy.Add(new StateElement { StateName = state });
            }
        }
        #region Selected Region Type.
        /// <summary>
        /// Gets or sets the type of the selected region.
        /// </summary>
        /// <value>
        /// The type of the selected region.
        /// </value>
        public Type SelectedRegionType
        {
            get
            {
                return string.IsNullOrWhiteSpace(DefaultRegionTypeName)
                    ? typeof(object)
                    : Type.GetType(DefaultRegionTypeName);
            }
            set
            {
                DefaultRegionTypeName = value.AssemblyQualifiedName;
            }
        }
        #endregion

        #region Selected States

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        protected ILogWriter Logger { get; set; }

        /// <summary>
        /// Adds the states.
        /// </summary>
        /// <param name="states">The states.</param>
        /// <param name="resetCollection">if set to <c>true</c> [reset collection].</param>
        public void AddStates(IEnumerable<string> states, bool resetCollection = false)
        {
            if (states == null || !states.Any()) return;

            if (_defaultStates == null || resetCollection) _defaultStates = new StringCollection();

            foreach (var state in states)
            {
                if (!_defaultStates.Contains(state))
                    _defaultStates.Add(state);
            }

            SyncDefaultStatesToConfig();
            //InitLazySelectedStates(true);

            //# TODO: Finish
            //this.PropertyChanged += (o, e) =>
            //{
            //	if (string.Equals(e.PropertyName, "DefaultStates", StringComparison.OrdinalIgnoreCase))
            //	{
            //		InitLazySelectedStates();
            //	}
            //};
        }

        /// <summary>
        /// Adds the state.
        /// </summary>
        /// <param name="state">The state.</param>
        public void AddState(State state)
        {
            if (state == null) return;

            if (_defaultStates == null) _defaultStates = new StringCollection();

            if (!_defaultStates.Contains(state.Abbreviation))
                _defaultStates.Add(state.Abbreviation);

            SyncDefaultStatesToConfig();
            //InitLazySelectedStates();
        }

        /// <summary>
        /// Adds the state.
        /// </summary>
        /// <param name="state">The state.</param>
        public void AddState(string state)
        {
            if (string.IsNullOrEmpty(state)) return;

            if (_defaultStates == null) _defaultStates = new StringCollection();

            if (!_defaultStates.Contains(state))
                _defaultStates.Add(state);

            SyncDefaultStatesToConfig();
            //InitLazySelectedStates();
        }
        #endregion


        /// <summary>
        /// Ares the congruent.
        /// </summary>
        /// <param name="thisOne">The this one.</param>
        /// <param name="thatOne">The that one.</param>
        /// <returns></returns>
        public static bool AreCongruent(HospitalRegionElement thisOne, HospitalRegionElement thatOne)
        {
            if (thisOne == null || thatOne == null) return false;
            if (thisOne.SelectedRegionType != thatOne.SelectedRegionType) return false;
            if (thisOne.DefaultStates.Count != thatOne.DefaultStates.Count) return false;

            var notInThis = thisOne.DefaultStates.OfType<string>().Any(state => !thatOne.DefaultStates.OfType<string>().Contains(state));
            return !notInThis && thatOne.DefaultStates.OfType<string>().Any(state => !thisOne.DefaultStates.OfType<string>().Contains(state));
        }
    }


    /// <summary>
    /// The custom timespan from strring converter.
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationConverterBase" />
    public partial class TimespanFromStringConverter
    {
        /// <summary>
        /// Converts from string to time span.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private TimeSpan ConvertFromStringToTimeSpan(ITypeDescriptorContext context,
              CultureInfo culture, string value)
        {
            return TimeSpan.Parse(value, culture);
        }

        /// <summary>
        /// Converts to time span from string.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private string ConvertToTimeSpanFromString(ITypeDescriptorContext context,
            CultureInfo culture, TimeSpan value, Type type)
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// The state to string converter class.
    /// </summary>
    public partial class StateToStringConverter
    {
        /// <summary>
        /// Converts the state of from string to.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private State ConvertFromStringToState(ITypeDescriptorContext context,
              CultureInfo culture, string value)
        {
            return new State() { Abbreviation = value };
        }

        /// <summary>
        /// Converts to state from string.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private string ConvertToStateFromString(ITypeDescriptorContext context,
            CultureInfo culture, State value, Type type)
        {
            return value.Abbreviation;
        }

    }
}
