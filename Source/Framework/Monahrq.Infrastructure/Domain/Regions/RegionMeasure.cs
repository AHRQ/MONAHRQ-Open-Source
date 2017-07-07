using System;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Infrastructure.Domain.Regions
{
    [Serializable]
    public class RegionMeasure : Measure
    {
        public RegionMeasure()
        { }

        public RegionMeasure(Target target, string measureCode)
            : base(target, measureCode)
        { }
    }
}
