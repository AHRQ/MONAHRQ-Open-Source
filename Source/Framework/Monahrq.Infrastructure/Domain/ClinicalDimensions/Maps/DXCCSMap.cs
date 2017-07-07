using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Domain.ClinicalDimensions.Maps
{
    [MappingProviderExport]
    public class DXCCSMap : ClinicalDimensionMap<DXCCS>
    {
        public DXCCSMap()
        {
            Map(x => x.DXCCSID).Index(IndexName("DXCCSID"));
        }
    }
}
