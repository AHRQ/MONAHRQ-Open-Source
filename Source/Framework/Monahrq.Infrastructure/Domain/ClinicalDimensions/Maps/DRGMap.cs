using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;


namespace Monahrq.Infrastructure.Domain.ClinicalDimensions.Maps
{
    [Monahrq.Infrastructure.Domain.Data.MappingProviderExport]
    public class DRGMap : ClinicalDimensionMap<DRG>
    {
        public DRGMap()
        {
            Map(x => x.DRGID).Index(IndexName("DRGID"));
            Map(x => x.MDCID).Index(IndexName("MDCID")); ;
        }
    }
}
