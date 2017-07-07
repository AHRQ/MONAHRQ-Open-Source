using System;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Infrastructure.Domain.Hospitals
{
    [Serializable]
    public class HospitalMeasure : Measure
    {
        public HospitalMeasure()
        {}

        public HospitalMeasure(Target target, string measureCode)
            : base(target, measureCode)
        {}
    }
}