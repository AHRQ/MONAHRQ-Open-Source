using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Infrastructure.Data.CustomTypes;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Reports;

namespace Monahrq.Infrastructure.Domain.Physicians.Maps
{

	[MappingProviderExport]
	public class MeasureTopicMap : EntityMap<MeasureTopic, int, IdentityGeneratedKeyStrategy>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MeasureTopicMap"/> class.
		/// </summary>
		public MeasureTopicMap()
	    {
	        var indexName = string.Format("IDX_{0}", EntityTableName);

			References(x => x.Measure, "Measure_Id")
                             .ForeignKey("FK_MMT_Measures")
							 .Cascade.None()
							 .Nullable()
							 .Not.LazyLoad();

	        References(x => x.Topic, "Topic_Id")
                             .ForeignKey("FK_MMT_Topics")
                             .Cascade.None()
	                         .Nullable()
	                         .Not.LazyLoad();

			Map(x => x.UsedForInfographic).Not.Nullable().Default("0");
		//	Map(x => x.Audiences).CustomType<EnumListToStringType<Audience>>();
		}

		public override string EntityTableName
	    {
	        get
	        {
	            return "Measures_MeasureTopics";
	        }
	    }

	    protected override PropertyPart NameMap()
	    {
	        return null;
	    }
	}
}
