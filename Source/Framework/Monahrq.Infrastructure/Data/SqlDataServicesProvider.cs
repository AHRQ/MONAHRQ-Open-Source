using System;
using System.ComponentModel.Composition;
using FluentNHibernate.Cfg.Db;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Extensibility.Configuration;

namespace Monahrq.Infrastructure.Data
{
    public class SqlServerDataServicesProvider<T> : AbstractDataServicesProvider<T>
    {
        [Import]
        IPersistenceManager PersistenceManager
        {
            get;
            set;
        }

        [Import]
        IConfigurationService ConfigurationService { get; set; }

        public override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase)
        {
            if (string.IsNullOrEmpty(ConfigurationService.ConnectionSettings.ConnectionString))
            {
                throw new ArgumentException("The connection string is empty");
            }
            PersistenceManager.AssertDb();

            return MsSqlConfiguration.MsSql2008
                .ConnectionString(ConfigurationService.ConnectionSettings.ConnectionString)
                .AdoNetBatchSize(ConfigurationService.MonahrqSettings.BatchSize);
        }

     

    }
}
