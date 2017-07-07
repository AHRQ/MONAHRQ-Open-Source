using Microsoft.Practices.Prism.Events;
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
    [Export]
    public class ShellExtender
    {

        [ImportMany(MigrationContractNames.Target, typeof(ITargetMigration))]
        public IEnumerable<ITargetMigration> Migrations { get; set; }

        [Import]
        IEventAggregator Events { get; set; }

        [Import]
        IDataMigrationInterpreter Interpreter { get; set; }

        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        IMonahrqShell Shell { get; set; }

        public void ExtendShell()
        {
            ExecuteMigrations();
        }

        private void ExecuteMigrations()
        {
            foreach (var migraton in Migrations)
            {
                migraton.SchemaBuilder = new SchemaBuilder(Interpreter);
                migraton.Create();
            }

            Shell.SessionFactoryHolder.Reinitialize();
        }

    }
}
