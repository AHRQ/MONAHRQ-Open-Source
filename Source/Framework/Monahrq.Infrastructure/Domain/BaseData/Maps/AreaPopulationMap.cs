using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class AreaPopulationMap : GeneratedKeyLookupMap<AreaPopulation>
    {
        public AreaPopulationMap(): base()
        {
            Map(x => x.StateCountyFIPS).Index("IDX_AREAPOPULATION_STATECOUNTYFIPS");
            Map(x => x.Sex).Index("IDX_AREAPOPULATION_SEX");
            Map(x => x.AgeGroup).Index("IDX_AREAPOPULATION_AGEGROUP");
            Map(x => x.Race).Index("IDX_AREAPOPULATION_RACE");
            Map(x => x.Year).Index("IDX_AREAPOPULATION_YEAR");
            Map(x => x.Population).Index("IDX_AREAPOPULATION_POPULATION");
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
