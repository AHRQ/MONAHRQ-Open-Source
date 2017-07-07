using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class ZipCodeToPopulationStratsMap : GeneratedKeyLookupMap<ZipCodeToPopulationStrats>
    {
        public ZipCodeToPopulationStratsMap()
            : base()
        {
            Map(x => x.ZipCode).Index("IDX_ZIPCODETOPOPULATIONSTRATS_ZIPCODE");
            Map(x => x.CatID).Index("IDX_ZIPCODETOPOPULATIONSTRATS_CATID");
            Map(x => x.CatVal).Index("IDX_ZIPCODETOPOPULATIONSTRATS_CATVAL");
            Map(x => x.Year).Index("IDX_ZIPCODETOPOPULATIONSTRATS_YEAR");
            Map(x => x.Population).Index("IDX_ZIPCODETOPOPULATIONSTRATS_POPULATION");
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
