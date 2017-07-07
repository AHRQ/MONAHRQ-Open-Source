using System;
using System.Data;
using System.Reflection;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Categories
{

    [Serializable, 
     ImplementPropertyChanged,
     EntityTableName("Categories")]
    public abstract class Category : Entity<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Category"/> class.
        /// </summary>
        protected Category()
        { }

        #region Properties
        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        public virtual int CategoryID { get; set; }
        /// <summary>
        /// Gets or sets the owner count.
        /// </summary>
        /// <value>
        /// The owner count.
        /// </value>
        public virtual int OwnerCount { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public virtual decimal? Version { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is sourced from base data.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is sourced from base data; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsSourcedFromBaseData { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected { get; set; }
        #endregion

        public override IBulkMapper CreateBulkInsertMapper<T>(DataTable dataTable, T instance = default(T), Target target = null)
        {
            return new CategoryBulkInsertMapper<T>(dataTable);
        }

        //public override IBulkMapper CreateBulkInsertMapper<T>(System.Data.DataTable dataTable, T instance = null, Monahrq.Infrastructure.Entities.Domain.Wings.Target target = null)
        //{
        //    return new CategoryBulkInsertMapper<T>(dataTable);
        //}
    }

    public class CategoryBulkInsertMapper<T> : BulkMapper<T> 
        where T : class
    {
        public CategoryBulkInsertMapper(DataTable dt) : base(dt)
        {
        }

        protected override void ApplyTypeSpecificColumnNameLookup()
        {
            Lookup["CategoryType"] = t => t.GetType().Name;
        }

        protected override string LookupPropertyName(PropertyInfo pi)
        {
            return pi.Name;
        }
    }
}
