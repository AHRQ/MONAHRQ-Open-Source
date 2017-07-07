using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class ZipCodeToLatLongMap : GeneratedKeyLookupMap<ZipCodeToLatLong>
    {
        public ZipCodeToLatLongMap()
        {
            Map(x => x.Zip).Length(12).Index("IDX_ZIPCODETOLATLONG_ZIPCODE");
            Map(x => x.Latitude).Index("IDX_ZIPCODETOLATLONG_LATITITUDE");
            Map(x => x.Longitude).Index("IDX_ZIPCODETOLATLONG_LONGITUDE");
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
