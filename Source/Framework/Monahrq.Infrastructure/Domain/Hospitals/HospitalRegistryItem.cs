using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals
{
    [Serializable]
    public abstract class HospitalRegistryItem : OwnedEntity<HospitalRegistry, int, int>
    {
        protected HospitalRegistryItem()
        {
        }

        protected HospitalRegistryItem(HospitalRegistry registry)
            : base(registry) 
        {
            Registry = registry;
        }

        public virtual HospitalRegistry Registry { get; private set; }
        public virtual decimal? Version { get; set; }
        public virtual bool IsSourcedFromBaseData { get; set; }
    }
}
