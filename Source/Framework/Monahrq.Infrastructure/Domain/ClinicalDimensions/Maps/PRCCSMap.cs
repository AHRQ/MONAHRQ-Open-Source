using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Domain.ClinicalDimensions.Maps
{
    [MappingProviderExport]
    public class PRCCSMap : ClinicalDimensionMap<PRCCS>
    {
        public PRCCSMap()
        {
            Map(x => x.PRCCSID).Index(IndexName("PRCCSID")); 
        }
    }
}
