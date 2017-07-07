using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Infrastructure.Utility;
using NHibernate.Type;

namespace Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records
{
    /// <summary>
    /// The dataset FluentNHibernate mapping 
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Domain.Maps.EntityMap{Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.Dataset, System.Int32, Monahrq.Infrastructure.Entities.Data.Strategies.IdentityGeneratedKeyStrategy}" />
    public class DatasetMap : EntityMap<Dataset, int, IdentityGeneratedKeyStrategy>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetMap"/> class.
        /// </summary>
        public DatasetMap()
        {
            var indexName = string.Format("IDX_" + typeof(Dataset).EntityTableName());
            Map(m => m.SummaryData).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(m => m.File, "[File]").CustomSqlType("nvarchar(255)");
            Map(m => m.Description).CustomSqlType("nvarchar(255)");
            Map(m => m.DateImported).Nullable().Index(indexName);
            Map(m => m.TotalRows).CustomType<long?>().Nullable();
            Map(m => m.RowsImported).CustomType<long?>().Nullable();
            Map(m => m.ReportingQuarter).CustomSqlType("nvarchar(20)");
			Map(m => m.ReportingYear).CustomSqlType("nvarchar(20)");
            Map(m => m.VersionMonth).CustomSqlType("nvarchar(20)");
			Map(m => m.VersionYear).CustomSqlType("nvarchar(20)");
			Map(m => m.ReportProcessVersion).CustomSqlType("nvarchar(50)");
            Map(m => m.ProviderStates).CustomSqlType("nvarchar(200)").Index(indexName);
            Map(m => m.UseRealtimeData, "ProviderUseRealtime").CustomSqlType("bit").Default("0").Index(indexName);
            Map(m => m.DRGMDCMappingStatus).CustomType<EnumStringType<DrgMdcMappingStatusEnum>>().Index(indexName);//"IX_DATASET_DRGMDCMAPPINGSTATUS"
            Map(m => m.DRGMDCMappingStatusMessage).CustomSqlType("nvarchar(500)");
            Map(m => m.IsFinished).CustomSqlType("bit").Default("0").Index(indexName);//.Index("IX_DATASET_ISFINISHED");
            
            References(x => x.ContentType, "ContentType_Id")
                 .Cascade.None()
                 .Not.LazyLoad();
            //References(x => x.Summary, "Summary_Id").Cascade.All().Not.LazyLoad();
            HasMany(m => m.Versions)
                .Not.Inverse()
                .Cascade.AllDeleteOrphan().LazyLoad();

            Cache.Region("Datasets").NonStrictReadWrite();
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