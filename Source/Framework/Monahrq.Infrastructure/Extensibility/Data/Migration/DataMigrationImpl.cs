using Monahrq.Sdk.Extensibility.Data.Migration.Schema;
using Monahrq.Sdk.Logging;

namespace Monahrq.Sdk.Extensibility.Data.Migration
{
    /// <summary>
    /// Data Migration classes can inherit from this class to get a SchemaBuilder instance configured with the current tenant database prefix
    /// </summary>
    public abstract class DataMigrationImpl : IDataMigration
    {
        protected readonly SessionLogger SessionLogger = new SessionLogger(new CallbackLogger());

        public SchemaBuilder SchemaBuilder { get; set; }
        public abstract int Create();
        public virtual int Update() { return 1; }
        public object Target { get; set; }
    }

    public class MigrationContractNames
    {
        public const string Target = "TARGET MIGRATION";
    }
}