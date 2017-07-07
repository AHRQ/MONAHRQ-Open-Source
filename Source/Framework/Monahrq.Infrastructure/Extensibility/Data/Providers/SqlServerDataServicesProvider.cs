using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Cfg.Db;
using NHibernate.Tool.hbm2ddl;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Configuration;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.FileSystem;

namespace Monahrq.Sdk.Extensibility.Data.Providers
{
    public class SqlServerDataServicesProvider : AbstractDataServicesProvider
    {


        private readonly string _dataFolder;
        private readonly string _connectionString;

        public SqlServerDataServicesProvider(string dataFolder, string connectionString, IUserFolder userFolder)
            : base(userFolder)
        {
            _dataFolder = dataFolder;
            _connectionString = connectionString;
        }

        public static string ProviderName
        {
            get { return "SqlServer"; }
        }

        public override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase)
        {
            var confService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentException("The connection string is empty");
            }
            return MsSqlConfiguration.MsSql2008
                .ConnectionString(_connectionString)
                .AdoNetBatchSize(confService.MonahrqSettings.BatchSize);
        }
    }
}
