using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Domain.ClinicalDimensions.Maps
{
    [MappingProviderExport]
    public class MDCMap : ClinicalDimensionMap<MDC>
    {
        public MDCMap()
        {
            Map(x => x.MDCID).Index(IndexName("MDCID")); 
        }
    }
}
