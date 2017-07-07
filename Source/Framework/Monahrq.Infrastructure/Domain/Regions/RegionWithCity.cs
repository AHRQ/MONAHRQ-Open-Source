using System;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Regions
{
    [ImplementPropertyChanged]
    [Serializable]
    public abstract class RegionWithCity : Region
    {
         protected RegionWithCity()
            : base()
        {
        
        }

        protected RegionWithCity(HospitalRegistry registry)
            : base(registry)
        {
            
        }

         private string _city;

         public string City
         {
             get
             {
                 return _city;
             }
             set
             {
                 if (string.IsNullOrWhiteSpace(Name))
                 {
                     Name = value;
                 }
                 _city = value;
             }
         }
    }
}
