using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Domain.Regions.Maps
{
    [SubclassMappingProviderExport]
    public class CustomRegionMap : RegionSubclassMap<CustomRegion>
    {
          public CustomRegionMap()
          {}
    }
}
