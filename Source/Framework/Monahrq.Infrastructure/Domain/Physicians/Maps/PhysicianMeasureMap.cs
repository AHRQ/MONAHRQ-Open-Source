using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Domain.Physicians.Maps
{
    public class PhysicianMeasureMap : SubclassMap<PhysicianMeasure>
    {

        public PhysicianMeasureMap()
        {
            DiscriminatorValue("PHYSICIAN");

            var indexName = string.Format("IDX_{0}", typeof(Measure).EntityTableName());


        }
    }
}
