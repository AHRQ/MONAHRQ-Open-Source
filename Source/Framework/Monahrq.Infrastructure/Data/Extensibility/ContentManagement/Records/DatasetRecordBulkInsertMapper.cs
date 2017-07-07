using Monahrq.Infrastructure.Entities.Domain;
using System.Reflection; 
using System.Data;
using Monahrq.Infrastructure.Core.Attributes;

namespace Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records
{
    /// <summary>
    /// The data set bulk insert mapper. Handles the mapping from instantiated dataset objects to a <see cref="DataTable"/> for bulk insert
    /// into the corresponding dataset target database table.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Domain.BulkMapper{T}" />
    public class DatasetRecordBulkInsertMapper<T> : BulkMapper<T> 
        where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetRecordBulkInsertMapper{T}"/> class.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="target">The target.</param>
        public DatasetRecordBulkInsertMapper(DataTable dt, T instance = default(T), Entities.Domain.Wings.Target target = null) 
            : base(dt, instance, target)
        {}

        /// <summary>
        /// Applies the type specific column name lookup.
        /// </summary>
        protected override void ApplyTypeSpecificColumnNameLookup()
        {
            Lookup["Dataset_id"] =
                t => (t as DatasetRecord).Dataset == null
                    ? null
                    : (object) (t as DatasetRecord).Dataset.Id;
        }

        /// <summary>
        /// Lookups the name of the property.
        /// </summary>
        /// <param name="pi">The pi.</param>
        /// <returns></returns>
        protected override string LookupPropertyName(PropertyInfo pi)
        {
            var temp = pi.GetCustomAttribute<WingTargetElementAttribute>();
            return temp == null ? pi.Name : temp.Name;
        }
    }
}
