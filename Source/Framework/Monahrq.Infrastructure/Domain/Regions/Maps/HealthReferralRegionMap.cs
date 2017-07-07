namespace Monahrq.Infrastructure.Domain.Regions.Maps
{
    [Data.SubclassMappingProviderExportAttribute]
    public class HealthReferralRegionMap : RegionSubclassMap<HealthReferralRegion>
    {
        public HealthReferralRegionMap()
        {
            Map(x => x.City);
        }
    }
}
