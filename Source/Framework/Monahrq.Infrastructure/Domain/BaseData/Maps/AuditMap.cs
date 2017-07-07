//using Monahrq.Infrastructure.Domain.Data;

//namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
//{
//    [MappingProviderExport]
//    public class AuditMap : GeneratedKeyLookupMap<Audit>
//    {
//        public AuditMap() : base()
//        {
//            Map(x => x.DataVersion).Index("IDX_BASE_DATA_VERSION");
//            Map(x => x.Dataset).Length(1000).Index("IDX_BASE_DATA_DATASET");
//            Map(x => x.Contract).Length(1000).Index("IDX_BASE_DATA_CONTRACT");
//        }

//        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
//        {
//            return null;
//        }
//    }
//}
