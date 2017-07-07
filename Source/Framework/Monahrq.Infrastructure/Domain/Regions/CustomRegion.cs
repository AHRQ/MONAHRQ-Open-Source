using System;
using System.ComponentModel;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Regions
{
    [Description(@"Custom Region")]
    [DisplayName(@"Custom Region")]
    [Serializable, ImplementPropertyChanged]
    public class CustomRegion : Region 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRegion"/> class.
        /// </summary>
        public CustomRegion()
        { }

        /// <summary>
        /// Gets or sets the created.
        /// </summary>
        /// <value>
        /// The created.
        /// </value>
        public virtual DateTime Created { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRegion"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        public CustomRegion(HospitalRegistry registry)
            : base(registry)
        {
            registry.CustomRegions.Add(this);
        }

        /// <summary>
        /// Gets the region type for display.
        /// </summary>
        /// <value>
        /// The region type for display.
        /// </value>
        public override string RegionTypeForDisplay { get { return "Custom Region"; } }
    }
}
