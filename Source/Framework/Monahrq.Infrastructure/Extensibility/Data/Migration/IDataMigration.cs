using Monahrq.Sdk.Extensibility.Data.Migration.Schema;
using System.Collections.Generic;

namespace Monahrq.Sdk.Extensibility.Data.Migration
{
    public interface IDataMigration 
    {
        object Target { get; set; }
        SchemaBuilder SchemaBuilder { get; set; }
        int Create();
        int Update();
    }

    public interface ITargetMigration : IDataMigration
    {
        IEnumerable<string> TargetNames { get; }
    }
}
