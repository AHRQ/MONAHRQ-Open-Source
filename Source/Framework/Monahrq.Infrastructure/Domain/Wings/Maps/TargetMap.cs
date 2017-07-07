using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Infrastructure.Utility;
using NHibernate.Type;

namespace Monahrq.Infrastructure.Entities.Domain.Wings.Maps
{
    [MappingProviderExport]
    public class TargetMap : OwnedEntityMap<Target, int, Wing, int, IdentityGeneratedKeyStrategy>
    {
        public TargetMap()
        {
            var idxName = string.Format("IDX_{0}", typeof(Target).EntityTableName());
            Map(x => x.Guid).Not.Nullable();

            Component(x => x.Version, i => i.Map(x => x.Number, "VersionNumber").Length(50).Index(idxName + "_2"));
            Map(x => x.ClrType).Nullable();
            Map(x => x.DbSchemaName).Length(150);
            Map(x => x.WingTargetXmlFilePath, "FilePath").Length(256);
            Map(x => x.TemplateFileName, "TemplateFileName").Length(256);

            Map(x => x.Publisher).Length(255).Not.Nullable();
            Map(x => x.PublisherEmail).Length(255).Nullable();
            Map(x => x.PublisherWebsite).Length(255).Nullable();
            Map(x => x.Description).CustomType("StringClob").CustomSqlType("nvarchar(max)");

            Map(x => x.IsDisabled).Not.Nullable().Default("0").Index(idxName);
            Map(x => x.IsCustom).Not.Nullable().Default("0").Index(idxName);
            Map(x => x.IsReferenceTarget).Not.Nullable().Default("0");
            Map(x => x.IsTrendingEnabled).Not.Nullable().Default("0").Index(idxName);
            Map(x => x.AllowMultipleImports).Not.Nullable().Default("0").Index(idxName);
            Map(x => x.ImportType).CustomType<EnumStringType<DynamicStepTypeEnum>>()
                    .Length(50).Nullable();

            Map(x => x.DisplayOrder).Not.Nullable().Default("0");
            
            //Map(x => x.CreateSqlScript).CustomSqlType("nvarchar(max)");
            //Map(x => x.ImportSQLScript).CustomSqlType("nvarchar(max)");
            //Map(x => x.AddMeausersSqlScript).CustomSqlType("nvarchar(max)");
            //Map(x => x.AddReportsSqlScript).CustomSqlType("nvarchar(max)");
            


            HasMany(x => x.Elements)
                .Inverse()
                .Not.LazyLoad()
                .Cascade.All();
            HasMany(x => x.Measures)
                .Inverse()
                .Not.LazyLoad()
                .Cascade.All();
            HasMany(x => x.Scopes)
                .Inverse()
                .Not.LazyLoad()
                .Cascade.All();

            Cache.ReadWrite().Region("Targets");
        }

        //protected override string OwnerName
        //{
        //    get
        //    {
        //        return "Wing_Id";
        //    }
        //}

    }
}