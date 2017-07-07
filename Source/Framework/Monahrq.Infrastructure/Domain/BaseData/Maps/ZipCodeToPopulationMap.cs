using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class ZipCodeToPopulationMap : GeneratedKeyLookupMap<ZipCodeToPopulation>
    {
        public ZipCodeToPopulationMap()
            : base()
        {
            Map(x => x.ZipCode).Index("IDX_ZIPCODETOPOPULATION_ZIPCODE");
            Map(x => x.Sex).Index("IDX_ZIPCODETOPOPULATION_SEX");
            Map(x => x.AgeGroup).Index("IDX_ZIPCODETOPOPULATION_AGEGROUP");
            Map(x => x.Race).Index("IDX_ZIPCODETOPOPULATION_RACE");
            Map(x => x.Year).Index("IDX_ZIPCODETOPOPULATION_YEAR");
            Map(x => x.Population).Index("IDX_ZIPCODETOPOPULATION_POPULATION");
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
