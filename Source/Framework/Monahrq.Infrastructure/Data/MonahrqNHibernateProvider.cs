using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Listeners;
using Monahrq.Infrastructure.Entities.Domain;
using NHibernate;
using NHibernate.Event;
using NHibernate.Tool.hbm2ddl;
using Monahrq.Infrastructure.Data.Interceptors;
using Monahrq.Infrastructure.Extensions;
using NHibernate.Cache;

//using NHibernate.Caches.SysCache3;

namespace Monahrq.Infrastructure.Data
{
    [DebuggerStepThrough]
    public static class MonahrqNHibernateProvider
    {
        private static readonly IConfigurationService _configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
        static MonahrqNHibernateProvider()
        { }

        public static ISessionFactory SessionFactory { get; set; }
        public static NHibernate.Cfg.Configuration Configuration { get; set; }

        public static string CurrentConnectionString
        {
            get
            {
                return Configuration != null ? Configuration.GetProperty("connection.connection_string") : null;
            }
        }
        private static readonly object _nibernateLock = new object();

        //private static IConfigurationService _configurationService;
        //private static readonly IUserFolder _userFolder;

        [DebuggerStepThrough]
        public static void BuildConfiguration()
        {
            lock (_nibernateLock)
            {
                ConfigurationManager.RefreshSection("MonahrqConfigurationSectionGroup");

                var database = GetPersistenceConfigurer(_configService.MonahrqSettings.RebuildDatabase);

                var moduleAssemblies = GetModuleAssemblies().ToList();

                if (Configuration == null)
                {
                    Configuration = Fluently.Configure()
                                            .Database(database)
                                            .Mappings(m => moduleAssemblies.ForEach(mappingAssembly => m.FluentMappings.AddFromAssembly(mappingAssembly)))
                                            .Mappings(m => moduleAssemblies.ForEach(mappingAssembly => m.HbmMappings.AddFromAssembly(mappingAssembly)))
                                            //.Cache(c => c.ProviderClass<SysCacheProvider>().UseSecondLevelCache().UseQueryCache())
                                            .Cache(c => c.ProviderClass<HashtableCacheProvider>().UseSecondLevelCache().UseQueryCache())
                                            .ExposeConfiguration(cfg =>
                                                {
                                                    cfg.SetProperty("current_session_context_class", "thread_static")
                                                       .SetProperty("use_proxy_validator", "false")
                                                       .SetProperty("connection.isolation", "ReadCommitted")
                                                       .SetProperty("adonet.batch_size", _configService.MonahrqSettings.BatchSize.ToString(CultureInfo.InvariantCulture))
                                                       .SetProperty("command_timeout", "5000");

                                                    //cfg.EventListeners.PostDeleteEventListeners = new IPostDeleteEventListener[] { new AuditEventListener() };
                                                    //cfg.EventListeners.PostUpdateEventListeners = new IPostUpdateEventListener[] { new AuditEventListener() };
                                                    //cfg.EventListeners.PostInsertEventListeners = new IPostInsertEventListener[] { new AuditEventListener() };
                                                    cfg.EventListeners.PreDeleteEventListeners = new IPreDeleteEventListener[] { new AuditPreUpdateEventListener() };
                                                    cfg.EventListeners.PreUpdateEventListeners = new IPreUpdateEventListener[] { new AuditPreUpdateEventListener() };
                                                    cfg.EventListeners.PreInsertEventListeners = new IPreInsertEventListener[] { new AuditPreUpdateEventListener() };
                                                    cfg.SetInterceptor(new SqlCaseSensitivityInterceptor());
                                                    //cfg.EventListeners.LoadEventListeners = new ILoadEventListener[] { new OrchardLoadEventListener() };
                                                    // cfg.SetInterceptor(new DataBindingInterceptor());

                                                    if (MonahrqContext.ForceDbUpGrade) return;

                                                    SchemaMetadataUpdater.QuoteTableAndColumns(cfg);

                                                    //if (_configService.MonahrqSettings.RebuildDatabase)
                                                    //{
                                                    //    var schemaExport = new SchemaExport(cfg);
                                                    //    schemaExport.Drop(false, true);
                                                    //    schemaExport.Execute(true, false, false);
                                                    //}
                                                    //else
                                                    //{
                                                        try
                                                        {
                                                            var schemaUpdate = new SchemaUpdate(cfg);

                                                            schemaUpdate.Execute(false, true);
                                                        }
                                                        catch (Exception ex)
                                                        {
															ex.GetType();
                                                        }
                                                    //}
                                                })
                                            .BuildConfiguration();
                }
            }
        }

        [DebuggerStepThrough]
        public static bool RebuildDatabase(bool rebuildCompletely = false)
        {
            try
            {
                if (rebuildCompletely)
                {
                    ForceRefresh();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void ForceRefresh()
        {
            _configService.ForceRefresh();

            if (_configService.ConnectionSettings == null || string.IsNullOrEmpty(_configService.ConnectionSettings.ConnectionString)) return;

            Configuration = null;
            SessionFactory = null;
            BuildConfiguration();

            lock (_nibernateLock)
            {
                if (Configuration != null)
                    SessionFactory = Configuration.BuildSessionFactory();
            }

            SessionFactory.ClearAllNhibernateCaches(); // clear to refresh nhibernate caches.
        }

        //public static void GetMapping<T>(ISession session) where T : class, IEntity<int>
        //{
        //    if (Configuration == null) return;

        //    var dialect = Dialect.GetDialect(Configuration.Properties);
        //    var connection = session.Connection;
        //    var meta = new DatabaseMetadata(connection as DbConnection, dialect);
        //    var createSQL = Configuration.GenerateSchemaUpdateScript(dialect, meta);
        //}

        [DebuggerStepThrough]
        private static IEnumerable<Assembly> GetModuleAssemblies()
        {
            var assemblies = new List<Assembly>();
            var modulesDirectory = new DirectoryInfo(Path.GetFullPath("Modules"));

            assemblies.Add(typeof(IEntity).Assembly);

            if (!modulesDirectory.Exists)
                return assemblies;

            var moduleAssemblies = modulesDirectory.GetFiles("*.dll");

            if (!moduleAssemblies.Any())
                return assemblies;

            foreach (var module in moduleAssemblies)
            {
                var assembly = Assembly.LoadFile(module.FullName);

                if (assembly == null || !assembly.ExportedTypes.Any(t => t.Name.EndsWith("Map")))
                    continue;

                if (!assemblies.Any(a => a.FullName.Equals(assembly.FullName, StringComparison.OrdinalIgnoreCase)))
                    assemblies.Add(assembly);
            }

            return assemblies;
        }

        [DebuggerStepThrough]
        public static IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase)
        {
            var confService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            if (string.IsNullOrEmpty(confService.ConnectionSettings.ConnectionString))
            {
                throw new ArgumentException("The connection string is empty");
            }

            var configurer = MsSqlConfiguration.MsSql2008
                                        .ConnectionString(confService.ConnectionSettings.ConnectionString)
                                        .DefaultSchema("dbo")
                                        .AdoNetBatchSize(confService.MonahrqSettings.BatchSize)
                                        .UseReflectionOptimizer()
                                     .UseOuterJoin();

            configurer = confService.MonahrqSettings.DebugSql
                                ? configurer.ShowSql()
                                : configurer;

            return configurer;
        }
    }

    //[Serializable]
    //class OrchardLoadEventListener : DefaultLoadEventListener, ILoadEventListener
    //{
    //    public new void OnLoad(LoadEvent @event, LoadType loadType)
    //    {
    //        var source = (ISessionImplementor)@event.Session;
    //        IEntityPersister entityPersister;
    //        if (@event.InstanceToLoad != null)
    //        {
    //            entityPersister = source.GetEntityPersister(null, @event.InstanceToLoad);
    //            @event.EntityClassName = @event.InstanceToLoad.GetType().FullName;
    //        }
    //        else
    //            entityPersister = source.Factory.GetEntityPersister(@event.EntityClassName);
    //        if (entityPersister == null)
    //            throw new HibernateException("Unable to locate persister: " + @event.EntityClassName);

    //        var keyToLoad = new EntityKey(@event.EntityId, entityPersister, source.EntityMode);

    //        if (loadType.IsNakedEntityReturned)
    //        {
    //            @event.Result = Load(@event, entityPersister, keyToLoad, loadType);
    //        }
    //        else if (@event.LockMode == LockMode.None)
    //        {
    //            @event.Result = ProxyOrLoad(@event, entityPersister, keyToLoad, loadType);
    //        }
    //        else
    //        {
    //            @event.Result = LockAndLoad(@event, entityPersister, keyToLoad, loadType, source);
    //        }
    //    }
    //}
}