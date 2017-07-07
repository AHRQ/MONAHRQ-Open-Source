using System;
using Monahrq.Infrastructure.Domain.Categories;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals
{
    [Serializable,
     ImplementPropertyChanged]
    public class HospitalCategory : Category
    {
        public HospitalCategory()
        {
        }

        public HospitalCategory(HospitalRegistry registry)
        {
            if (registry == null) return;
            Registry = registry;
            Registry.HospitalCategories.Add(this);
        }

        public HospitalRegistry Registry { get; set; }

        public virtual int HospitalCountForSelectedRegion { get; set; }
    }
}