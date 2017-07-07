using System;
using System.Collections.Generic;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain.BaseData;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals.Mapping
{
    public class HospitalMappingReference
    {
        //private readonly IList<Tuple<int, string, string>> _lazyCmsLookup; 
 
        /// <summary>
        /// The region type for the session
        /// </summary>
        public Type RegionType { get; set; }

        /// <summary>
        /// List of states used to initialize the session
        /// </summary>
        public virtual IList<State> States { get; private set; }

        /// <summary>
        /// Set of Custom Regions the user has defined in the candidate states
        /// </summary>
        public virtual IList<Region> Regions { get; private set; } 

        /// <summary>
        /// The hospitals loated in the candidate states
        /// </summary>
        public virtual IList<Hospital> Hospitals { get; private set; }

        /// <summary>
        /// Gets the CMS lookup.
        /// </summary>
        /// <value>
        /// The CMS lookup.
        /// </value>
        public IList<Tuple<int, string, string>> CmsLookup { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalMappingReference"/> class.
        /// </summary>
        /// <param name="regionType">Type of the region.</param>
        public HospitalMappingReference(Type regionType)
        {
            RegionType = regionType;
            States = new List<State>();
            Regions = new List<Region>();
            Hospitals = new List<Hospital>();
            CmsLookup = new List<Tuple<int, string, string>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalMappingReference" /> class.
        /// </summary>
        /// <param name="regionType">Type of the region.</param>
        /// <param name="states">The states.</param>
        /// <param name="hospitals">The hospitals.</param>
        /// <param name="regions">The regions.</param>
        /// <param name="cmsLookups">The CMS lookups.</param>
        public HospitalMappingReference(Type regionType, List<State> states, List<Hospital> hospitals,
                                        List<Region> regions, List<Tuple<int, string, string>> cmsLookups)
            : this(regionType)
        {
            states.ForEach(x => States.Add(x));
            regions.ForEach(x => Regions.Add(x));
            hospitals.ForEach(x => Hospitals.Add(x));
            cmsLookups.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.Item2))
                        CmsLookup.Add(x);
                }); 
        }
    }
}
