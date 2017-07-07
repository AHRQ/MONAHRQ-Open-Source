using System.Data;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Wing.Dynamic.Models
{
    /// <summary>
    /// Class to create dynamic dataset record
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecord" />
    public abstract class DynamicDatasetRecord : DatasetRecord
    {
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
            return new DynamicDatasetRecordBulkInsertMapper<T>(dataTable, target);
        }
    }
}
