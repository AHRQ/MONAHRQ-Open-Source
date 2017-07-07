using System.Collections.Generic;
using System.Linq;
using Monahrq.Sdk.Extensibility.Data;
using Monahrq.Infrastructure;
using System.ComponentModel.Composition;
using Monahrq.Sdk.Extensibility.Data.Migration.Interpreters;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data;

namespace Monahrq.Sdk.Extensibility
{
    [Export(typeof(IMonahrqShell))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MonahrqShell : IMonahrqShell
    {
        public ISessionLocator SessionLocator { get; private set; }
        public IEnumerable<ICommandInterpreter> CommandInterpreters { get; private set; }
        public ISessionFactoryHolder SessionFactoryHolder { get; private set; }
        private IConfigurationService ConfigurationService { get; set; }

        [ImportingConstructor]
        public MonahrqShell(
            ISessionLocator sessionLocator,
            IConfigurationService configService,
            ISessionFactoryHolder sessionFactoryHolder,
            [Import(LogNames.Operations)] ILogWriter logger,
            [ImportMany(typeof(ICommandInterpreter))] IEnumerable<ICommandInterpreter> commandInterpreters)
        {
            SessionLocator = sessionLocator;
            ConfigurationService = configService;
            SessionFactoryHolder = sessionFactoryHolder;
            Logger = logger;

            if (commandInterpreters != null)
                CommandInterpreters = commandInterpreters;
        }

        public ILogWriter Logger
        {
            get;
            private set;
        }


        public System.Configuration.ConnectionStringSettings Connection
        {
            get
            {
                return ConfigurationService.ConnectionSettings;
            }
        }
    }
}
