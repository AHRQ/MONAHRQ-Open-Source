using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Domain.Measures;

namespace Monahrq.Infrastructure.Domain.Counties.Maps
{
    [SubclassMappingProviderExport]
    public class CountyMeasureMap : SubclassMap<CountyMeasure>
    {
        public CountyMeasureMap()
        {
            DiscriminatorValue("COUNTY");
        }
    }
}
