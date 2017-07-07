using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Domain.Physicians
{
    [Serializable]
    public class PhysicianMeasure : Measure
    {
        public PhysicianMeasure()
        { }

        public PhysicianMeasure(Target target, string measureCode)
            : base(target, measureCode)
        { }

    }
}
