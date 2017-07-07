using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Domain.ClinicalDimensions.Maps
{
    [MappingProviderExport]
    public class MSDRGMap : ClinicalDimensionMap<MSDRG>
    {
        public MSDRGMap()
        {
            Map(x => x.MDCID).Index(IndexName("MDCID"));
            Map(x => x.MSDRGID).Index(IndexName("MSDRGID")); 
        }
    }
}
