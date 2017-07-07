using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Data.CustomTypes;
using System.Collections.Generic;

namespace Monahrq.Infrastructure.Entities.Domain.Measures.Maps
{
    [MappingProviderExport]
    public class TopicMap : OwnedEntityMap<Topic, int, TopicCategory, int, IdentityGeneratedKeyStrategy>
    {
        public TopicMap()
        {
            Map(x => x.LongTitle)
                //.Length(2000)
                .CustomType("StringClob").CustomSqlType("nvarchar(max)")
                .Nullable();

            Map(x => x.Description)
                //.Length(2000)
                .CustomType("StringClob").CustomSqlType("nvarchar(max)")
                .Nullable();

            Map(x => x.WingTargetName).Length(255);
            //HasManyToMany(x => x.Measures)
            //    .Table(Inflector.Pluralize(typeof(Measure).Name) + "_MeasureTopics")
            //    .ParentKeyColumn("Topic_Id")
            //    .ChildKeyColumn("Measure_Id")
            //    .Not.LazyLoad()
            //    .Inverse()
            //    .Cascade.SaveUpdate();
            Map(x => x.IsUserCreated)
               .CustomSqlType("bit")
               .Not.Nullable()
               .Default("0");

            Map(x => x.ConsumerLongTitle)
                .CustomType("StringClob")
                .CustomSqlType("nvarchar(max)")
                .Nullable();

            Cache.NonStrictReadWrite().Region("Topics");
        }

        //protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        //{
        //    return base.NameMap();
        //}
    }

    [MappingProviderExport]
    public class TopicCategoryMap : EntityMap<TopicCategory, int, IdentityGeneratedKeyStrategy>
    {
        public TopicCategoryMap()
        {
            HasMany(x => x.Topics)
                .Not.Inverse()
                .Cascade.All()
                .Not.LazyLoad();

            Map(x => x.LongTitle)
                .CustomType("StringClob").CustomSqlType("nvarchar(max)")
                .Nullable();

            Map(x => x.Description)
                .CustomType("StringClob").CustomSqlType("nvarchar(max)")
                .Nullable();

            Map(x => x.TopicType).Length(25);

            Map(x => x.WingTargetName).Length(255);

            Map(x => x.CategoryType).Length(20);

            Cache.NonStrictReadWrite()
                 .Region("TopicCategories");

            Map(x => x.IsUserCreated)
                .CustomSqlType("bit")
                .Not.Nullable()
                .Default("0");

            Map(x => x.ConsumerDescription)
                .CustomType("StringClob")
                .CustomSqlType("nvarchar(max)")
                .Nullable();

            Map(x => x.DateCreated)
                .CustomType<NullableDateTimeType>()
                .Nullable()
                .Default("GetDate()");

            Map(x => x.Facts)
                .CustomType<JsonToStringType<List<TopicFacts>>>()
                .Nullable();

            Map(x => x.TopicFacts1).Length(2000);
            Map(x => x.TopicFacts2).Length(2000);
            Map(x => x.TopicFacts3).Length(2000);
            Map(x => x.TipsChecklist).Length(4000);
            Map(x => x.TopicIcon).Length(200);

        }
    }
}
