using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Domain.Regions.Maps
{
    [SubclassMappingProviderExport]
    public class RegionMeasureMap : SubclassMap<RegionMeasure>
    {
        public RegionMeasureMap()
        {
            DiscriminatorValue("REGION");
        }
    }
}
