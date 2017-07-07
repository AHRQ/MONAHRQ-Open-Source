using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class ZipCodeToHRRAndHSAMap : GeneratedKeyLookupMap<ZipCodeToHRRAndHSA>
    {
        public ZipCodeToHRRAndHSAMap()
        {
            var indexName = string.Format("IDX_{0}", typeof(ZipCodeToHRRAndHSA).EntityTableName());
            Map(x => x.Zip).Length(5).Index(indexName);
            Map(x => x.HRRNumber).Index(indexName);
            Map(x => x.HSANumber).Index(indexName);
            Map(x => x.State).Length(5).Not.Nullable().Index(indexName);
            //References(x => x.State, "State_Id")
            //    //.Not.Nullable()
            //    .ForeignKey("FK_ZIPCODETOHRRANDHSA_STATES")
            //    .Not.LazyLoad()
            //    .Cascade.SaveUpdate();
            Map(x => x.StateFIPS).Index(indexName);
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
