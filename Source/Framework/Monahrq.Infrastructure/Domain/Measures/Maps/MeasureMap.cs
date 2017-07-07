using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Domain.Hospitals;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Utilities;
using NHibernate.Type;

namespace Monahrq.Infrastructure.Entities.Domain.Measures.Maps
{
    [MappingProviderExport]
    public class MeasureMap : OwnedEntityMap<Measure, int, Target, int, IdentityGeneratedKeyStrategy>
    {
        public MeasureMap()
        {
            var indexName = string.Format("IDX_{0}", typeof(Measure).EntityTableName());
            Map(m => m.MeasureType).Length(100).Index(indexName);

            Component(x => x.MeasureTitle, i =>
            {
                i.Map(x => x.Clinical, "ClinicalTitle").CustomSqlType("nvarchar(max)");
                i.Map(x => x.Plain, "PlainTitle").CustomSqlType("nvarchar(max)");
                i.Map(x => x.Policy, "PolicyTitle").CustomSqlType("nvarchar(max)");
                i.Map(x => x.Selected, "SelectedTitle").CustomType<EnumStringType<SelectedMeasuretitleEnum>>();
            });

            Map(x => x.Source).Length(20);
            Map(x => x.Description).CustomSqlType("nvarchar(max)");
            Map(x => x.MoreInformation).CustomSqlType("nvarchar(max)");
            Map(x => x.Footnotes).CustomSqlType("nvarchar(max)");

            Map(x => x.NationalBenchmark).Scale(7);

            Component(x => x.StatePeerBenchmark, i =>
            {
                i.Map(x => x.ProvidedBenchmark).Nullable().Scale(7);
                i.Map(x => x.CalculationMethod).CustomType<EnumStringType<StatePeerBenchmarkCalculationMethod>>();
            });

            Map(x => x.HigherScoresAreBetter).Default("0");
            Map(x => x.ScaleBy).Nullable().Scale(0);
            Map(x => x.ScaleTarget).Length(20);
            Map(x => x.RiskAdjustedMethod).Length(20);
            Map(x => x.RateLabel).Length(255);
            Map(x => x.NQFEndorsed).Default("0");
            Map(x => x.NQFID).Length(20);

            Map(x => x.SuppressionDenominator).Nullable().Scale(0);
            Map(x => x.SuppressionNumerator).Nullable().Scale(0);
            Map(x => x.PerformMarginSuppression).Default("0");
            Map(x => x.UpperBound).Nullable().Scale(0);
            Map(x => x.LowerBound).Nullable().Scale(0);
            Map(x => x.Url).Length(500);
            Map(x => x.UrlTitle).Length(550);
            Map(m => m.IsOverride).Default("0").Index(indexName);
            Map(m => m.IsLibEdit).Default("0").Index(indexName);
            Map(m => m.UsedInCalculations).Default("0").Index(indexName);
            Map(m => m.SupportsCost).Default("0").Index(indexName);
            Map(m => m.ConsumerDescription).CustomSqlType("nvarchar(max)");
            Map(m => m.ConsumerPlainTitle).CustomSqlType("nvarchar(max)");

			//HasManyToMany(x => x.Topics)
			//    .Table(Inflector.Pluralize(typeof(Measure).Name) + "_MeasureTopics")
			//    .ParentKeyColumn("Measure_Id")
			//    .ChildKeyColumn("Topic_Id")
			//    .AsBag()
			//    .Not.LazyLoad()
			//    .Cascade.All();
			HasMany(x => x.MeasureTopics)
				.KeyColumn(typeof(Measure).Name + "_Id")
				.AsBag()
				.Not.Inverse()
				.Cascade.AllDeleteOrphan()
				.Not.LazyLoad()
				.Cache.Region("Measures_MeasureTopics").NonStrictReadWrite();

			DiscriminateSubClassesOnColumn("ClassType").Length(20).Not.Nullable().Index(indexName + "_ClassType");
        }
    }
}