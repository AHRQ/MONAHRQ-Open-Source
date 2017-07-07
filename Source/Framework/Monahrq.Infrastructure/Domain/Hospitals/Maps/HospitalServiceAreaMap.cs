using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Domain.Regions.Maps;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals.Maps
{
    [SubclassMappingProviderExport]
    public class HospitalServiceAreaMap : RegionSubclassMap<HospitalServiceArea>
    {
        public HospitalServiceAreaMap()
        {
            Map(x => x.City);
        }
    }

}
