using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class NHProviderToLatLongMap : GeneratedKeyLookupMap<NHProviderToLatLong>
    {
        public NHProviderToLatLongMap()
        {
            var indexName = string.Format("IDX_{0}", typeof(NHProviderToLatLong).EntityTableName());
            Map(x => x.ProviderId).Length(8).Index(indexName).Not.Nullable();
            Map(x => x.Latitude).Index(indexName).Default("0").Not.Nullable();
            Map(x => x.Longitude).Index(indexName).Default("0").Not.Nullable();
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
