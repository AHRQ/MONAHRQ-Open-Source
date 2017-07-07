using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;

namespace Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records
{
    /// <summary>
    /// The dataset transaction record FluentNHibernate mapping.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Domain.Maps.EntityMap{Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetTransactionRecord, System.Int32, Monahrq.Infrastructure.Entities.Data.Strategies.IdentityGeneratedKeyStrategy}" />
    public class DatasetTransactionRecordMap : EntityMap<DatasetTransactionRecord, int, IdentityGeneratedKeyStrategy>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetTransactionRecordMap"/> class.
        /// </summary>
        public DatasetTransactionRecordMap()
        {
            References(x => x.Dataset, "Dataset_Id").Not.LazyLoad().Cascade.All();
            Map(m => m.Code);
            Map(m => m.Message).CustomSqlType("nvarchar(255)");
            Map(m => m.Extension);
            Map(m => m.Data).CustomSqlType("nvarchar(max)");
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