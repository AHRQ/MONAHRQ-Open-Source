using System;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Infrastructure.Domain.NursingHomes
{
    [Serializable]
    public class NursingHomeMeasure : Measure
    {
        public NursingHomeMeasure()
        { }

        public NursingHomeMeasure(Target target, string measureCode)
            : base(target, measureCode)
        { }
        
        
    }
}