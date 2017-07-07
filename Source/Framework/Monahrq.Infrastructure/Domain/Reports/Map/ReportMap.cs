using FluentNHibernate;
using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Data.CustomTypes;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using System;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Utility;
using NHibernate.Type;

namespace Monahrq.Infrastructure.Entities.Domain.Reports.Map
{
    [Infrastructure.Domain.Data.MappingProviderExport]
    public class ReportColumnMap : EntityMap<ReportColumn, int, IdentityGeneratedKeyStrategy>
    {
        public ReportColumnMap()
        {
            var idxName = string.Format("IDX_{0}", EntityTableName);
            Map(x => x.IsMeasure).Not.Nullable().Default("0").Index(idxName);
            Map(x => x.MeasureCode).Length(20).Index(idxName);
            Map(x => x.IsIncluded)
                .Not.Nullable().Default("1")
                .Index(idxName);

            Cache.ReadWrite().Region("ReportColumns");
        }
    }


    [Infrastructure.Domain.Data.MappingProviderExport]
    public class ReportWebsitePageMap : EntityMap<ReportWebsitePage, int, IdentityGeneratedKeyStrategy>
    {
        public ReportWebsitePageMap()
        {
            var idxName = string.Format("IDX_{0}", EntityTableName);
            Map(x => x.Name).Length(256).Index(idxName);
            Map(x => x.Audience).CustomType<EnumStringType<Reports.Audience>>()
                .Not.Nullable();
            Map(x => x.Path).Length(256).Index(idxName);
            Map(x => x.Url).Length(256).Index(idxName);
            Map(x => x.IsEditable).Index(idxName);
            HasMany(x => x.WebsitePageZones)
                .KeyColumn("ReportWebsitePage_Id")
                //	.AsList(x => x.Column("[Index]"))
                .Not.Inverse()
                .Not.LazyLoad()
                .Cascade.AllDeleteOrphan();

            Cache.ReadWrite().Region("ReportWebsitePages");
        }

        protected override PropertyPart NameMap()
        {
            return null;
        }
    }

    [Infrastructure.Domain.Data.MappingProviderExport]
    public class ReportWebsitePageZoneMap : EntityMap<ReportWebsitePageZone, int, IdentityGeneratedKeyStrategy>
    {
        public ReportWebsitePageZoneMap()
        {
            var idxName = string.Format("IDX_{0}", EntityTableName);
            Map(x => x.Name).Length(256).Index(idxName);
            Map(x => x.CodePath).Length(256).Index(idxName);
            Cache.ReadWrite().Region("ReportWebsitePageZones");
        }

        protected override PropertyPart NameMap()
        {
            return null;
        }
    }


    [Infrastructure.Domain.Data.MappingProviderExport]
    public class ReportMap : EntityMap<Report, int, IdentityGeneratedKeyStrategy>
    {
        public ReportMap()
        {
            Map(x => x.ReportProfile);
            Map(x => x.Category);
            Map(x => x.IsTrending);

            Map(x => x.Audiences).CustomType<EnumListToStringType<Audience>>()
                                 .Not.Nullable();

            Map(x => x.Datasets).CustomType<StringListToStringType>()
                                .Not.Nullable();

            Map(x => x.RequiresCostToChargeRatio);
            Map(x => x.RequiresCmsProviderId);
            Map(x => x.ReportAttributes);

            // HasMany(x => x.Filters).KeyColumn("Report_Id")
            //    .AsBag()
            //    .Not.Inverse()
            //    .Cascade.AllDeleteOrphan()
            //    .Not.LazyLoad();

            HasMany<RptFilter>(Reveal.Member<Report>("_rptfilters"))
                .KeyColumn("Report_Id")
                .AsList(x => x.Column("[Index]"))
                .Not.Inverse()
                .Cascade.AllDeleteOrphan()
                .Not.LazyLoad()
                .Cache.Region("Report_Filters").NonStrictReadWrite();

            HasMany(x => x.Columns)
                .KeyColumn("Report_Id")
                .ForeignKeyConstraintName("FK_Reports_Owns_ReportColumns")
                //.AsList(x => x.Column("[Index]"))
                .AsBag()
                .NotFound.Ignore()
                .Not.Inverse()
                .Cascade.AllDeleteOrphan()
                .Not.LazyLoad()
                .Cache.Region("Report_Columns").NonStrictReadWrite();

            HasMany(x => x.WebsitePages)
                .KeyColumn("Report_Id")
                .ForeignKeyConstraintName("FK_Reports_Owns_WebsitePages")
                .AsBag()
                .NotFound.Ignore()
                .Not.Inverse()
                .Cascade.AllDeleteOrphan()
                .Not.LazyLoad()
                .Cache.Region("Report_WebsitePages").NonStrictReadWrite();

            Map(x => x.Description).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.Footnote).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.ComparisonKeyIconSetName);
            Map(x => x.ReportType).Length(255).Not.Nullable();

            HasMany(x => x.ComparisonKeyIcons)
                .KeyColumn("Report_Id")
                .ForeignKeyConstraintName("FK_Reports_Owns_ComparisonKeyIcons")
                .AsList(x => x.Column("[Index]"))
                .NotFound.Ignore()
                .Not.Inverse()
                .Cascade.AllDeleteOrphan()
                .Not.LazyLoad()
                .Cache.Region("Report_ComparisonKeyIcons").NonStrictReadWrite();

            Map(Reveal.Member<Report>("_reportManifestXml"), "SourceTemplateXml")
                      .CustomSqlType("xml").CustomType<XmlDocType>();
            Map(x => x.IsDefaultReport).CustomSqlType("bit").Default("0").Not.Nullable();
            Map(x => x.IsCustom).CustomSqlType("bit").Default("0").Not.Nullable();
            Map(x => x.Filter);

            Map(Reveal.Member<Report>("_filterItemsXml"), "FilterItemsXml")
                      .CustomSqlType("xml").CustomType<XmlDocType>();
            Map(x => x.InterpretationText).CustomType("StringClob").CustomSqlType("nvarchar(max)");

            Map(x => x.ShowInterpretationText).CustomSqlType("BIT").Default("0").Not.Nullable();
            Map(x => x.ReportOutputSql, "ReportSql").CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.OutputFileName).Length(255);
            Map(x => x.OutputJsNamespace).Length(255);
            Map(x => x.DateCreated).CustomType<NullableDateTimeType>().Nullable().Default("GetDate()");
            //      Map(x => x.LastReportManifestUpdate).CustomType<NullableDateTimeType>().Default("1/1/2000");//.Default("GetDate()");
            Map(x => x.LastReportManifestUpdate).CustomType<NullableDateTimeType>().Default(new DateTime(2016, 01, 01).ToString("MM/dd/yyyy"));

            Cache.ReadWrite().Region("Reports");
        }
    }

    [Infrastructure.Domain.Data.MappingProviderExport]
    public class ComparisonKeysMap : OwnedEntityMap<ComparisonKeyIconSet, int, Report, int, IdentityGeneratedKeyStrategy>
    {
        public ComparisonKeysMap()
        {
            Map(x => x.BestImage).CustomType<UriType>();
            Map(x => x.BelowImage).CustomType<UriType>();
            Map(x => x.BetterImage).CustomType<UriType>();
            Map(x => x.NotEnoughDataImage).CustomType<UriType>();
            Map(x => x.AverageImage).CustomType<UriType>();
            Map(x => x.IsIncluded)
                .Not.Nullable().Default("1");
            Cache.ReadWrite().Region("ComparisonKeys");
        }
    }

    [Infrastructure.Domain.Data.MappingProviderExport]
    public class RptFilterMap : OwnedEntityMap<RptFilter, int, Report, int, IdentityGeneratedKeyStrategy>
    {
        readonly string _indexName = string.Format("IDX_" + typeof(RptFilter).EntityTableName());

        public RptFilterMap()
        {
            Map(x => x.FilterType).Length(30).Index(_indexName);
            Map(x => x.Value).Not.Nullable().Default("0");
            Map(x => x.IsRadioButton).Not.Nullable().Default("0").Index(_indexName);
            Map(x => x.RadioGroupName).Length(150).Index(_indexName);

            Cache.ReadWrite().Region("RptFilters");
        }

        protected override PropertyPart NameMap()
        {
            return Map(i => i.Name)
               .Length(256).Not.Nullable()
               .Index(_indexName);
        }
    }
}