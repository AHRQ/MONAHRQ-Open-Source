using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NHibernate.Cfg;

//using Orchard.Environment;
//using Orchard.Environment.Configuration;
//using Orchard.Environment.ShellBuilders.Models;
//using Orchard.FileSystems.AppData;
//using Orchard.Logging;
//using Orchard.Utility;

namespace Monahrq.Sdk.Extensibility.Data
{
    using Monahrq.Sdk.Extensibility.Builders.Models;
    using Monahrq.Infrastructure.Entities.Domain.Wings;
    using Monahrq.Sdk.Extensibility.Utility;
    using NHibernate.Cfg;
    using Monahrq.Infrastructure;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Threading;
    using Monahrq.Infrastructure.Configuration;
    using System.Configuration;
    using Cfg = NHibernate.Cfg.Configuration;
    using Monahrq.Infrastructure.FileSystem;

    [Export(typeof(ISessionConfigurationCache))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SessionConfigurationCache : ISessionConfigurationCache
    {
        private readonly ShellBlueprint _shellBlueprint;
        private readonly IUserFolder UserFolder;
        private ConfigurationCache _currentConfig;

        static bool HasBeenInitialized;

        static object syncRoot = new object();

        public SessionConfigurationCache(IConfigurationService configurationService,
            ShellBlueprint shellBlueprint,
            IUserFolder userFolder)
        {
            ConfigurationService = configurationService;
            UserFolder = userFolder;
            _shellBlueprint = shellBlueprint;
            _currentConfig = null;
            Logger = NullLogger.Instance;

            lock (syncRoot)
            {
                if (!HasBeenInitialized)
                {
                    DeleteCache();
                    HasBeenInitialized = true;
                }
            }

        }

        private void DeleteCache()
        {
            if (UserFolder.FileExists(UserFolder.DataModelFile))
            {
                UserFolder.DeleteFile(UserFolder.DataModelFile);
            }
        }

        public void Reset()
        {
            HasBeenInitialized = false;
            DeleteCache();
            _currentConfig = null;
        }
        private IConfigurationService ConfigurationService
        {
            get;
            set;
        }

        [ImportingConstructor]
        public SessionConfigurationCache(IConfigurationService configurationService,
            IShellBlueprintFactory blueprintFactory,
            IUserFolder userfolder,
            [Import(LogNames.Operations)] ILogWriter logger)
            : this(configurationService, blueprintFactory.CreateBlueprint(), userfolder)
        {
            Logger = logger;
        }

        public ILogWriter Logger { get; set; }

        public Cfg GetConfiguration(Func<Cfg> builder)
        {
            //var hash = ComputeHash().Value;

            //// if the current configuration is unchanged, return it
            //if (_currentConfig != null && _currentConfig.Hash == hash)
            //{
            //    return _currentConfig.Configuration;
            //}

            //// Return previous configuration if it exists and has the same hash as
            //// the current blueprint.
            //var previousConfig = ReadConfiguration(hash);
            //if (previousConfig != null)
            //{
            //    _currentConfig = previousConfig;
            //    return previousConfig.Configuration;
            //}

            //// Create cache and persist it
            //_currentConfig = new ConfigurationCache
            //{
            //    Hash = hash,
            //    Configuration = builder()
            //};

            //StoreConfiguration(_currentConfig);
            //return _currentConfig.Configuration;

            return builder();
        }

        private class ConfigurationCache
        {
            public string Hash { get; set; }
            public Cfg Configuration { get; set; }
        }

        private void StoreConfiguration(ConfigurationCache cache)
        {

            try
            {
                var formatter = new BinaryFormatter();
                using (var stream = UserFolder.CreateFile(UserFolder.DataModelFile))
                {
                    formatter.Serialize(stream, cache.Hash);
                    formatter.Serialize(stream, cache.Configuration);
                }
            }
            catch (SerializationException e)
            {
                //Note: This can happen when multiple processes/AppDomains try to save
                //      the cached configuration at the same time. Only one concurrent
                //      writer will win, and it's harmless for the other ones to fail.
                for (Exception scan = e; scan != null; scan = scan.InnerException)
                    Logger.Warning("Error storing new NHibernate cache configuration: {0}", scan.Message);
            }
        }

        private ConfigurationCache ReadConfiguration(string hash)
        {
            if (!UserFolder.FileExists(UserFolder.DataModelFile))
                return null;

            try
            {
                var formatter = new BinaryFormatter();
                using (var stream = UserFolder.OpenFile(UserFolder.DataModelFile))
                {

                    // if the stream is empty, stop here
                    if (stream.Length == 0)
                    {
                        return null;
                    }

                    var oldHash = (string)formatter.Deserialize(stream);
                    if (hash != oldHash)
                    {
                        Logger.Information("The cached NHibernate configuration is out of date. A new one will be re-generated.");
                        return null;
                    }

                    var oldConfig = (Cfg)formatter.Deserialize(stream);

                    return new ConfigurationCache
                    {
                        Hash = oldHash,
                        Configuration = oldConfig
                    };
                }
            }
            catch (Exception e)
            {
                for (var scan = e; scan != null; scan = scan.InnerException)
                    Logger.Warning("Error reading the cached NHibernate configuration: {0}", scan.Message);
                Logger.Information("A new one will be re-generated.");
                return null;
            }
        }

        private Hash ComputeHash()
        {
            var hash = new Hash();

            // Shell settings physical location
            //   The nhibernate configuration stores the physical path to the SqlCe database
            //   so we need to include the physical location as part of the hash key, so that
            //   xcopy migrations work as expected.

            hash.AddString(UserFolder.DataModelFile.ToLowerInvariant());

            // Shell settings data
            hash.AddString(ConfigurationService.ConnectionSettings.ProviderName);
            hash.AddString(ConfigurationService.ConnectionSettings.ConnectionString);
            hash.AddString(ConfigurationService.ConnectionSettings.Name);

            // Assembly names, record names and property names
            foreach (var tableName in _shellBlueprint.Records.Select(x => x.TableName))
            {
                hash.AddString(tableName);
            }

            foreach (var recordType in _shellBlueprint.Records.Select(x => x.Type))
            {
                hash.AddTypeReference(recordType);

                if (recordType.BaseType != null)
                    hash.AddTypeReference(recordType.BaseType);

                foreach (var property in recordType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public))
                {
                    hash.AddString(property.Name);
                    hash.AddTypeReference(property.PropertyType);

                    foreach (var attr in property.GetCustomAttributesData())
                    {
                        hash.AddTypeReference(attr.Constructor.DeclaringType);
                    }
                }
            }

            return hash;
        }


    }
}
