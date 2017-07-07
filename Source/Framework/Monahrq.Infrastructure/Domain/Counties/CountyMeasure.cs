using System;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Infrastructure.Entities.Domain.Measures
{
    [Serializable]
    public class CountyMeasure : Measure
    {
        public CountyMeasure()
        { }

        public CountyMeasure(Target target, string measureCode)
            : base(target, measureCode)
        { }
    }
}