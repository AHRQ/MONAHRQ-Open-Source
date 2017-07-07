using System.Data;
using System.Linq;
using System.Reflection;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Wing.Dynamic.Models
{
    /// <summary>
    /// Class to create dynamic bulk insert mapper using source datatable and target
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Domain.BulkMapper{T}" />
    public class DynamicDatasetRecordBulkInsertMapper<T> : BulkMapper<T> 
        where T : class
    {
        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public Target Target { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDatasetRecordBulkInsertMapper{T}"/> class.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="target">The target.</param>
        public DynamicDatasetRecordBulkInsertMapper(DataTable dt, Target target = null) : base(dt)
        {
            this.Target = this.Target;
        }

        /// <summary>
        /// Applies the type specific column name lookup.
        /// </summary>
        protected override void ApplyTypeSpecificColumnNameLookup()
        {
            this.Lookup["Dataset_id"] =
                    t => (t as DatasetRecord).Dataset == null
                        ? null
                        : (object)(t as DatasetRecord).Dataset.Id;
        }

        /// <summary>
        /// Look up the name of the property.
        /// </summary>
        /// <param name="pi">The property info.</param>
        /// <returns></returns>
        protected override string LookupPropertyName(PropertyInfo pi)
        {
            //var temp = pi.GetCustomAttribute<WingTargetElementAttribute>();
            //return temp == null ? pi.Name : temp.Name;

            if (this.Target == null || !this.Target.Elements.Any()) return pi.Name;

            var element = this.Target.Elements.FirstOrDefault(t => t.Name.EqualsIgnoreCase(pi.Name));

            return (element != null) ? element.Name : pi.Name;

        }
    }
}