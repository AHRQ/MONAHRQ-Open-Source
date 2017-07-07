using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Regions
{
    [Description(@"HRR - Health Referral Region")]
    [DisplayName(@"Health Referral Region")]
    [Serializable, ImplementPropertyChanged]
    public class HealthReferralRegion : RegionWithCity 
    {
        public HealthReferralRegion()
        {} 

        public HealthReferralRegion(HospitalRegistry registry)
            : base(registry)
        {
            registry.HealthReferralRegions.Add(this);
        } 

        public override string RegionTypeForDisplay { get { return "HRR"; } }
    }
}
