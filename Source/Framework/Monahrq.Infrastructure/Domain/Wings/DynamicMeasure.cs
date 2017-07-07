using System;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Infrastructure.Domain.Wings
{
    [Serializable]
    public class DynamicMeasure : Measure
    {
        public DynamicMeasure()
        { }

        public DynamicMeasure(Target target, string measureCode)
            : base(target, measureCode)
        { }
    }
}