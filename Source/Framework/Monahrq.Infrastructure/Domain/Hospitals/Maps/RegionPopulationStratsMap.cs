using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Domain.BaseData.Maps;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals.Maps
{
    [MappingProviderExport]
    public class RegionPopulationStratsMap : GeneratedKeyLookupMap<RegionPopulationStrats>
    {
        public RegionPopulationStratsMap(): base()
        {
            Map(x => x.RegionType).Index("IDX_REGIONPOPULATIONSTRATS_REGIONTYPE");
            Map(x => x.RegionID).Index("IDX_REGIONPOPULATIONSTRATS_REGIONID");
            Map(x => x.CatID).Index("IDX_REGIONPOPULATIONSTRATS_CATID");
            Map(x => x.CatVal).Index("IDX_REGIONPOPULATIONSTRATS_CATVAL");
            Map(x => x.Year).Index("IDX_REGIONPOPULATIONSTRATS_YEAR");
            Map(x => x.Population).Index("IDX_REGIONPOPULATIONSTRATS_POPULATION");
        }

        public override string EntityTableName
        {
            get
            {
                return "Hospitals_" + Inflector.Pluralize(typeof(RegionPopulationStrats).Name);
            }
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
