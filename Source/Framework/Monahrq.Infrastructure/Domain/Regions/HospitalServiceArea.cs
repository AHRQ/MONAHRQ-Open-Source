using System;
using System.ComponentModel;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Regions
{
    [Description("HSA- Health Service Area")]
    [DisplayName(@"Health Service Area")]
    [Serializable, ImplementPropertyChanged]
    public class HospitalServiceArea : RegionWithCity 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalServiceArea"/> class.
        /// </summary>
        public HospitalServiceArea()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalServiceArea"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        public HospitalServiceArea(HospitalRegistry registry)
            : base(registry)
        {
            registry.HospitalServiceAreas.Add(this);
        }

        /// <summary>
        /// Gets the region type for display.
        /// </summary>
        /// <value>
        /// The region type for display.
        /// </value>
        public override string RegionTypeForDisplay { get { return "HSA"; } }
    }
}
