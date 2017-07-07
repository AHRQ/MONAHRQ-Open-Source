using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals
{
    [ImplementPropertyChanged]
    [Serializable]
    public class HospitalPropertyItem : HospitalRegistryItem
    {
        protected HospitalPropertyItem()
            : base()
        {
        }

        public HospitalPropertyItem(HospitalRegistry registry)
            : base(registry)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            Hospitals = new List<Hospital>();
        }

        public IList<Hospital> Hospitals { get; private set; }
    }

}
