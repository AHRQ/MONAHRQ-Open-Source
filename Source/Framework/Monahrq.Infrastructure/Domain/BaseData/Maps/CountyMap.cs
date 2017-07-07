using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class CountyMap : GeneratedKeyLookupMap<County>
    {
        public CountyMap()
        {
            Map(x => x.CountyFIPS).Length(5).Index("IDX_" + EntityTableName); // + "_COUNTYFIPS"
            Map(x => x.CountySSA).Length(3).Index("IDX_" + EntityTableName);
            Map(x => x.State).Length(5).Index("IDX_" + EntityTableName); //+ "_STATE"
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return Map(x => x.Name).Length(250).Index("IDX_" + EntityTableName + "_COUNTYNAME");
        }
    }
}
