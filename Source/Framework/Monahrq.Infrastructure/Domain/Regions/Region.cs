using System;
using System.Data;
using System.Reflection;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Regions
{
    [Serializable, 
     EntityTableName("Regions"),
     ImplementPropertyChanged]
    public class Region : HospitalRegistryItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Region"/> class.
        /// </summary>
        public Region()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Region"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        protected Region(HospitalRegistry registry)
            : base(registry)
        {
        }

        #region Properties
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public string Code { get; set; }
        public virtual string RegionType { get; set; }

        /// <summary>
        /// Gets or sets the import region identifier.
        /// </summary>
        /// <value>
        /// The import region identifier.
        /// </value>
        public virtual int? ImportRegionId { get; set; }

        /// <summary>
        /// Gets the region type for display.
        /// </summary>
        /// <value>
        /// The region type for display.
        /// </value>
        public virtual string RegionTypeForDisplay { get { return "N/A"; } }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public virtual string DisplayName
        {
            get
            {
                var displayName = Name;

                return !string.IsNullOrEmpty(State ?? string.Empty) ? string.Format("{0} ({1})", displayName, State) : Name;
            }
        }

        public override string Name { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public virtual string State { get; set; }                                                  // or this could be normalized to be a fkey to a State reference table

        /// <summary>
        /// Gets or sets the is Editing Field. 
        /// </summary>
        public bool IsEditing { get; set; }

        /// <summary>
        /// Gets or sets the hospital count.
        /// </summary>
        /// <value>
        /// The hospital count.
        /// </value>
        public virtual int HospitalCount { get; set; }
        #endregion

        public override IBulkMapper CreateBulkInsertMapper<T>(DataTable dataTable, T instance = default(T), Target target = null)
        {
            return new RegionBulkInsertMapper<T>(dataTable);
        }
    }

    public class RegionBulkInsertMapper<T> : BulkMapper<T> where T : class
    {
        public RegionBulkInsertMapper(DataTable dt) : base(dt) { }

        protected override void ApplyTypeSpecificColumnNameLookup()
        {
            Lookup["RegionType"] = t => t.GetType().Name;
        }

        protected override string LookupPropertyName(PropertyInfo pi)
        {
            return pi.Name;
        }
    }
}
