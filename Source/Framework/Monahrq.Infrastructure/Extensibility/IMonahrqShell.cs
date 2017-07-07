using System;
using System.Collections.Generic;
using Monahrq.Sdk.Extensibility.Builders.Models;
using Monahrq.Sdk.Extensibility.Data;
using Monahrq.Sdk.Extensibility.Data.Migration;
using Monahrq.Sdk.Extensibility.Data.Migration.Interpreters;
using Monahrq.Sdk.Extensibility.Data.Migration.Schema;
using Monahrq.Sdk.Extensibility.Data.Providers;
using Monahrq.Infrastructure;
using System.Configuration;
using Monahrq.Infrastructure.Data;

namespace Monahrq.Sdk.Extensibility
{
    
    public interface IMonahrqShell
    {
        ConnectionStringSettings Connection { get; }
        ISessionLocator SessionLocator { get; }
        IEnumerable<ICommandInterpreter> CommandInterpreters { get; }
        ISessionFactoryHolder SessionFactoryHolder { get; }
        ILogWriter Logger { get; }
    }

    public interface IMigrationFactory<T>
        where T : IDataMigration, new()
    {
        IDataMigration CreateDataMigration();
        IMonahrqShell ShellContext { get; }
    }

}
