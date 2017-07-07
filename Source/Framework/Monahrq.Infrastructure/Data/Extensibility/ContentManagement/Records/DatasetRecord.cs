using System.Data;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records
{
    /// <summary>
    /// Base dataset record implementation. Includes a <see cref="Records.Dataset"/> reference to permit multiple datasets of the same type to be loaded.
    /// </summary>
    /// <seealso cref="Entities.Domain.Entity{Int32}" />
    public abstract class DatasetRecord : Entity<int>
    {
        /// <summary>
        /// Gets a reference to the <see cref="Records.Dataset"/> that this <see cref="DatasetRecord"/> belongs to
        /// </summary>
        [CascadeAllDeleteOrphan]
        public virtual Dataset Dataset { get; set; }

        /// <summary>
        /// Creates the bulk insert mapper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable">The data table.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public override IBulkMapper CreateBulkInsertMapper<T>(DataTable dataTable, T instance = default(T), Target target = null)
        {
            return new DatasetRecordBulkInsertMapper<T>(dataTable);
        }
    }
}
