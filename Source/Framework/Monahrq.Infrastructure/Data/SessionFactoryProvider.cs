using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Data
{

//    using Monahrq.Infrastructure.Data.Interceptors;
    using Monahrq.Infrastructure.Entities.Data;
    using Monahrq.Infrastructure.FileSystem;
    using cfg = NHibernate.Cfg.Configuration;

    [Export(typeof(ISessionFactoryProvider))]
    public class SessionFactoryProvider : ISessionFactoryProvider, IDisposable
    {
        [Import]
        IDataServicesProvider DataServicesProvider
        {
            get;
            set;
        } 

        [Import]
        IUserFolder UserFolder
        {
            get;
            set;
        }

        [Import(LogNames.Session)]
        ILogWriter Logger { get; set; }
        
        private ISessionFactory _sessionFactory;
        private cfg _configuration;

        public void Dispose()
        {
            if (_sessionFactory != null)
            {
                _sessionFactory.Dispose();
                _sessionFactory = null;
            }
        }

        Lazy<ISessionFactory> LazySessionFactory { get; set; }
        public void Reinitialize()
        {
            lock (factorySync)
            {
                //MonahrqNHibernateProvider.Configuration = null;
                LazySessionFactory = new Lazy<ISessionFactory>(() => BuildSessionFactory(), true);
            }
        }

        static object factorySync = new object();
        public ISessionFactory GetSessionFactory()
        {
            if (LazySessionFactory == null)
            {
                lock (factorySync)
                {
                    if (LazySessionFactory == null)
                    {
                        Reinitialize();
                        return LazySessionFactory.Value;
                    }
                }
            }
            return LazySessionFactory.Value;
        }

        public cfg GetConfiguration()
        {
            lock (factorySync)
            {
                if (MonahrqNHibernateProvider.Configuration == null)
                {
                    MonahrqNHibernateProvider.BuildConfiguration();
                }
            }
            return MonahrqNHibernateProvider.Configuration;
        }

        private ISessionFactory BuildSessionFactory()
        {
//            var dataBindingIntercepter = new DataBindingInterceptor();

            Logger.Debug("Building session factory");

            cfg config = GetConfiguration();
            var result = MonahrqNHibernateProvider.SessionFactory ?? MonahrqNHibernateProvider.Configuration.BuildSessionFactory();
            //  .ExposeSessionFactory(factory => dataBindingIntercepter.SessionFactory = factory);
            Logger.Debug("Done building session factory");
            return result;
        }

        private cfg BuildConfiguration()
        {
            Logger.Debug("Building configuration");

            var config = DataServicesProvider.BuildConfiguration();

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


    }
}
