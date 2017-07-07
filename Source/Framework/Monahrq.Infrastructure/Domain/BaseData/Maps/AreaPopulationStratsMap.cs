using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class AreaPopulationStratsMap : GeneratedKeyLookupMap<AreaPopulationStrats>
    {
        public AreaPopulationStratsMap(): base()
        {
            Map(x => x.StateCountyFIPS).Index("IDX_AREAPOPULATIONSTRATS_STATECOUNTYFIPS");
            Map(x => x.CatID).Index("IDX_AREAPOPULATIONSTRATS_CATID");
            Map(x => x.CatVal).Index("IDX_AREAPOPULATIONSTRATS_CATVAL");
            Map(x => x.Year).Index("IDX_AREAPOPULATIONSTRATS_YEAR");
            Map(x => x.Population).Index("IDX_AREAPOPULATIONSTRATS_POPULATION");
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
