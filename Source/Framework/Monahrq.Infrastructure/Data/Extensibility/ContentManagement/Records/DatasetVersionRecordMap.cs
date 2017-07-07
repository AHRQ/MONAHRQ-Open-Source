using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;

namespace Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records
{
    /// <summary>
    /// The <see cref="DatasetVersionRecord"/> FluentNHibernate map.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Domain.Maps.EntityMap{Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetVersionRecord, System.Int32, Monahrq.Infrastructure.Entities.Data.Strategies.IdentityGeneratedKeyStrategy}" />
    public class DatasetVersionRecordMap : EntityMap<DatasetVersionRecord, int, IdentityGeneratedKeyStrategy>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetVersionRecordMap"/> class.
        /// </summary>
        public DatasetVersionRecordMap()
        {
            References(x => x.Dataset, "Dataset_Id")
                .Not.LazyLoad().Cascade.All();
            Map(m => m.Number);
            Map(m => m.Published).CustomSqlType("bit");
            Map(m => m.Latest).CustomSqlType("bit");

            Cache.ReadWrite().Region("DatasetVersionRecords");
        }

        /// <summary>
        /// Names the map.
        /// </summary>
        /// <returns></returns>
        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        } 
    }
}
