using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.Extensibility.Data;
using Monahrq.Sdk.Extensibility.Data.Migration;
using Monahrq.Sdk.Extensibility.Data.Migration.Interpreters;
using Monahrq.Sdk.Extensibility.Data.Migration.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Sdk.Extensibility
{
    public class MigrationFactory<T> : IMigrationFactory<T>
            where T : IDataMigration, new()
    {

        public MigrationFactory(IMonahrqShell shellContext)
        {
            ShellContext = shellContext;
        }

        public IMonahrqShell ShellContext
        {
            get;
            private set;
        }

        private IDataMigrationInterpreter CreateMigrationInterpreter()
        {
            var cfgSvc = ServiceLocator.Current.GetInstance<IConfigurationService>();
            return new DefaultDataMigrationInterpreter(cfgSvc.ConnectionSettings, ShellContext.SessionLocator,
                   ShellContext.CommandInterpreters, ShellContext.SessionFactoryHolder, 
                   ShellContext.Logger, typeof(T));
        }

        private IDataMigrationInterpreter UpdateMigrationInterpreter()
        {
            var cfgSvc = ServiceLocator.Current.GetInstance<IConfigurationService>();
            return new DefaultDataMigrationInterpreter(cfgSvc.ConnectionSettings, ShellContext.SessionLocator,
                   ShellContext.CommandInterpreters, ShellContext.SessionFactoryHolder,
                   ShellContext.Logger, typeof(T));
        }

        private SchemaBuilder CreateSchemaBuilder()
        {
            return new SchemaBuilder(CreateMigrationInterpreter());
        }

        private SchemaBuilder UpdateSchemaBuilder()
        {
            return new SchemaBuilder(UpdateMigrationInterpreter());
        }

        public IDataMigration CreateDataMigration()
        {
            return new T { SchemaBuilder = CreateSchemaBuilder() };
        }

        public IDataMigration UpdateDataMigration()
        {
            return new T { SchemaBuilder = UpdateSchemaBuilder() };
        }
    }
}
