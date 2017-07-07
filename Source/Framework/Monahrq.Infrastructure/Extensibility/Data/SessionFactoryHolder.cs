using System;
using Monahrq.Sdk.Extensibility.Builders.Models;
using Monahrq.Sdk.Extensibility.Data.Providers;
using NHibernate;
using Monahrq.Infrastructure;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Configuration;

using Cfg = NHibernate.Cfg.Configuration;
using Monahrq.Infrastructure.FileSystem;

    
using Monahrq.Infrastructure.Data;

namespace Monahrq.Sdk.Extensibility.Data
{
    public interface ISessionFactoryHolder  
    {
        ISessionFactory GetSessionFactory();
        Cfg GetConfiguration();
        SessionFactoryParameters GetSessionFactoryParameters();
        void Reinitialize();
    }

    [Export(typeof(ISessionFactoryHolder))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SessionFactoryHolder : ISessionFactoryHolder, IDisposable
    {
        private ShellBlueprint ShellBlueprint {get;set;}
        private readonly IDataServicesProviderFactory _dataServicesProviderFactory;
        private readonly IUserFolder _userFolder;
        private readonly ISessionConfigurationCache _sessionConfigurationCache;

        private ISessionFactory _sessionFactory;
        private Cfg _configuration;

        [ImportingConstructor]
        public SessionFactoryHolder(
            IConfigurationService configService,
            [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
            IShellBlueprintFactory shellBlueprintFactory,
            IDataServicesProviderFactory dataServicesProviderFactory,
                    IUserFolder userFolder,
            [Import(RequiredCreationPolicy = CreationPolicy.Shared)] 
            ISessionConfigurationCache sessionConfigurationCache)
        {
            ConfigService = configService;
            ShellBlueprint = shellBlueprintFactory.CreateBlueprint();
            BluePrintFactory = () =>
                {
                    shellBlueprintFactory.Reset();
                    return shellBlueprintFactory.CreateBlueprint();
                };
            _dataServicesProviderFactory = dataServicesProviderFactory;
            _userFolder = userFolder;
            _sessionConfigurationCache = sessionConfigurationCache;
            Logger = NullLogger.Instance;
        }

        IConfigurationService ConfigService { get; set; }

        Func<ShellBlueprint> BluePrintFactory
        {
            get;
            set;
        }

        public ILogWriter Logger { get; set; }

        public void Dispose()
        {
            if (_sessionFactory != null)
            {
                _sessionFactory.Dispose();
                _sessionFactory = null;
            }
        }

        //Lazy<ISessionFactory> LazySessionFactory { get; set; }
        ISessionFactory LazySessionFactory { get; set; }
        public void Reinitialize()
        {
            lock (_factorySync)
            {
                _configuration = null;
                _sessionConfigurationCache.Reset();
                ShellBlueprint = BluePrintFactory();
                //LazySessionFactory = new Lazy<ISessionFactory>(() => BuildSessionFactory(), true);
                LazySessionFactory = BuildSessionFactory();
            }
        }

        static readonly object _factorySync = new object();
        public ISessionFactory GetSessionFactory()
        {
            if (LazySessionFactory == null)
            {
                lock (_factorySync)
                {
                    if (LazySessionFactory == null)
                    {
                        Reinitialize();
                        //return LazySessionFactory.Value;
                        return LazySessionFactory;
                    }
                }
            }
            //return LazySessionFactory.Value;
            return LazySessionFactory;
        }

        public Cfg GetConfiguration()
        {
            lock (_factorySync)
            {
                if (_configuration == null)
                {
                    _configuration = BuildConfiguration();
                }
            }
            return _configuration;
        }

        private ISessionFactory BuildSessionFactory()
        {
            Logger.Debug("Building session factory");

            if (MonahrqNHibernateProvider.SessionFactory != null)
                return MonahrqNHibernateProvider.SessionFactory;

            if (MonahrqNHibernateProvider.Configuration == null)
                MonahrqNHibernateProvider.Configuration = GetConfiguration();

            MonahrqNHibernateProvider.SessionFactory = MonahrqNHibernateProvider.Configuration.BuildSessionFactory();

            Logger.Debug("Done building session factory");
            return MonahrqNHibernateProvider.SessionFactory;
        }


        private Cfg BuildConfiguration()
        {
            Logger.Debug("Building configuration");
            var parameters = GetSessionFactoryParameters();

            var config = _sessionConfigurationCache.GetConfiguration(() =>
                _dataServicesProviderFactory
                    .CreateProvider(parameters)
                    .BuildConfiguration(parameters));
            #region NH specific optimization
            // cannot be done in fluent config
            // the IsSelectable = false prevents unused ContentPartRecord proxies from being created 
            // for each Dataset or DatasetVersionRecord.
            // done for perf reasons - has no other side-effect

            foreach (var persistentClass in config.ClassMappings)
            {
                if (persistentClass.EntityName.StartsWith("Monahrq.Sdk.Extensibility.ContentManagement.Records."))
                {
                    foreach (var property in persistentClass.PropertyIterator)
                    {
                        if (property.Name.EndsWith("Record") && !property.IsBasicPropertyAccessor)
                        {
                            property.IsSelectable = false;
                        }
                    }
                }
            }
            #endregion
            Logger.Debug("Done Building configuration");
            return config;
        }

        public SessionFactoryParameters GetSessionFactoryParameters()
        {
            var shellPath = _userFolder.CombineToPhysicalPath(ConfigService.ConnectionSettings.Name);
            _userFolder.CreateDirectory(shellPath);
            lock (_factorySync)
            {
                return new SessionFactoryParameters
                {
                    Provider =  ConfigService.ConnectionSettings.ProviderName,
                    DataFolder = _userFolder.MapPath(_userFolder.DataModelFile),
                    ConnectionString = ConfigService.ConnectionSettings.ConnectionString,
                    RecordDescriptors = ShellBlueprint.Records,
                };
            }
        }
    }


}
