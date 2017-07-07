using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Domain.Hospitals.Maps;

namespace Monahrq.Infrastructure.Domain.Regions.Maps
{
    [MappingProviderExport]
    public class RegionMap  : HospitalRegistryItemMap<Region>
    {
        public RegionMap()
            : base()
        {
            Map(x => x.Code).Length(15)
                .Not.Nullable()
                .Unique().UniqueKey("UDX_" + EntityTableName);
            Map(x => x.ImportRegionId).Index("IDX_" + EntityTableName); // + "_IMPORTREGIONID"
            Map(x => x.State).Length(3).Index("IDX_" + EntityTableName); //  + "_STATE"
            Map(x => x.RegionType).ReadOnly().Not.Insert().Not.Update();
           
            DiscriminateSubClassesOnColumn("RegionType")
                .AlwaysSelectWithValue()
                .Index("IDX_" + EntityTableName + "_REGIONTYPES");
        }
    }
}
