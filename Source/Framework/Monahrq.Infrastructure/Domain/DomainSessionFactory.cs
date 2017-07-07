using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Extensions;
using NHibernate;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
//using Monahrq.Infrastructure.Data.Interceptors;
using FluentNHibernate;
using FluentNHibernate.Conventions;
using FluentNHibernate.Mapping.Providers;
using Monahrq.Infrastructure.Data.Extensibility;

namespace Monahrq.Infrastructure.Entities.Domain
{
    [Export(typeof(IDomainSessionFactoryProvider))]
    //[PartCreationPolicy(CreationPolicy.NonShared)]
    public class DomainSessionFactoryProvider : IDomainSessionFactoryProvider //<DomainSessionFactoryProvider>
    {
        protected IConfigurationService ConfigurationService { get; private set; }

        private ISessionFactory _lazySessionFactory;

        private ITypeProvider ExtensibilityTypeProvider { get; set; }
        public DateTime WhenBuilt { get; private set; }

        [ImportingConstructor]
        public DomainSessionFactoryProvider(
            IConfigurationService configService,
            [ImportMany] IEnumerable<IMappingProvider> mappingProviders,
            [ImportMany] IEnumerable<IIndeterminateSubclassMappingProvider> subclassMappingProviders,
            [ImportMany] IEnumerable<IConvention> conventions)
            : this(
                configService,
                new MappingProviderTypeProvider(mappingProviders, subclassMappingProviders, conventions))
        {
            WhenBuilt = DateTime.Now;
        }


        public DomainSessionFactoryProvider(
            IConfigurationService configService,
            ITypeProvider extensibilityTypeProvider)
        {

            ConfigurationService = configService;
            ExtensibilityTypeProvider = extensibilityTypeProvider;


            //if (MonahrqNHibernateProvider.SessionFactory == null)
            //{
            //    MonahrqNHibernateProvider.ForceRefresh();
            //}

            _lazySessionFactory = MonahrqNHibernateProvider.SessionFactory;
        }

        public ISessionFactory SessionFactory
        {
            get
            {
                if (_lazySessionFactory != null)
                {
                    lock (_syncLock)
                    {
                        using (var sf = _lazySessionFactory.OpenSession())
                        {
                            if (!sf.Connection.ConnectionString.EqualsIgnoreCase(ConfigurationService.ConnectionSettings.ConnectionString))
                            {
                                MonahrqNHibernateProvider.ForceRefresh();
                                _lazySessionFactory = MonahrqNHibernateProvider.SessionFactory;
                            }
                        }
                    }
                }
                else
                {
                    MonahrqNHibernateProvider.ForceRefresh();
                    _lazySessionFactory = MonahrqNHibernateProvider.SessionFactory;
                }

                return _lazySessionFactory;
            }
        }

        object[] _syncLock = new object[] { };
        private ISessionFactory InitializeSessionFactory()
        {
            if (MonahrqNHibernateProvider.SessionFactory != null)
                return MonahrqNHibernateProvider.SessionFactory;

            if (MonahrqNHibernateProvider.Configuration == null)
            {
                lock (_syncLock)
                {
                    //var configurer = MsSqlConfiguration.MsSql2008
                    //                                   .ConnectionString(
                    //                                       ConfigurationService.ConnectionSettings.ConnectionString)
                    //                                   .AdoNetBatchSize(ConfigurationService.MonahrqSettings.BatchSize)
                    //                                   .UseReflectionOptimizer().FormatSql();
                    //configurer = ConfigurationService.MonahrqSettings.DebugSql
                    //                 ? configurer.ShowSql()
                    //                 : configurer;

                    MonahrqNHibernateProvider.BuildConfiguration();


                    //MonahrqNHibernateProvider.Configuration = Fluently.Configure()
                    //                                                  .Database(configurer)
                    //                                                  .Mappings(
                    //                                                      m =>
                    //                                                          {
                    //                                                              var providers =ExtensibilityTypeProvider.ProviderTypes;
                    //                                                              var assy = providers.Select(i => i.Assembly);
                    //                                                              var distinct = assy.Distinct();
                    //                                                              distinct.ToList().ForEach(a =>m.FluentMappings.AddFromAssembly(a));
                    //                                                              assy = ExtensibilityTypeProvider.ConventionTypes.Select(i => i.Assembly).Distinct();
                    //                                                              assy.ToList().ForEach(a =>m.FluentMappings.Conventions.AddAssembly(a));
                    //                                                          }
                    //    )
                    //                                                  .ExposeConfiguration((config) =>
                    //                                                      {
                    //                                                          config.Properties.Add("use_proxy_validator","false");
                    //                                                          var schemaExport = new SchemaExport(config);
                    //                                                          if (ConfigurationService.RebuildDatabase)
                    //                                                          {
                    //                                                              schemaExport.Drop(false, true);
                    //                                                              schemaExport.Execute(true, false, false);
                    //                                                          }
                    //                                                          else
                    //                                                          {
                    //                                                              var schemaUpdate = new SchemaUpdate(config);
                    //                                                              schemaUpdate.Execute(true, true);
                    //                                                          }


                    //                                                          // See http://msdn.microsoft.com/en-us/magazine/ee819139.aspx
                    //                                                          // config.SetInterceptor(dataBindingIntercepter);

                    //                                                      }).BuildConfiguration();


                    MonahrqNHibernateProvider.SessionFactory = MonahrqNHibernateProvider.Configuration.BuildSessionFactory();
                }

            }
            //
            //.ExposeSessionFactory((factory) =>
            //    {
            //        dataBindingIntercepter.SessionFactory = factory;
            //    });

            return MonahrqNHibernateProvider.SessionFactory;
        }

    }


}
