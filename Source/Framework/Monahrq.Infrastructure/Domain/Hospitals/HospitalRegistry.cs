using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Monahrq.Infrastructure.Domain.Categories;
using Monahrq.Infrastructure.Domain.Regions;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals
{
    [Serializable]
    public class HospitalRegistry : Entity<int>
    {
        public HospitalRegistry()
        {
            Hospitals = new List<Hospital>();
            HealthReferralRegions = new ObservableCollection<HealthReferralRegion>();
            HospitalServiceAreas = new ObservableCollection<HospitalServiceArea>();
            CustomRegions = new ObservableCollection<CustomRegion>();
            HospitalCategories = new ObservableCollection<HospitalCategory>();
            ImportDate = DateTime.Now;
        }

        public HospitalRegistry(decimal version)
            : this()
        {
            DataVersion = version;
        }
        
        public virtual DateTime ImportDate { get; private set; }
        public virtual decimal DataVersion {get; private set;}
        public virtual IList<Hospital> Hospitals { get; private set; }
        public virtual IList<HospitalCategory> HospitalCategories { get; private set; }
        public virtual IList<HealthReferralRegion> HealthReferralRegions { get; private set; }
        public virtual IList<HospitalServiceArea> HospitalServiceAreas { get; private set; }
        public virtual IList<CustomRegion> CustomRegions { get; private set; }

     
    }

}
