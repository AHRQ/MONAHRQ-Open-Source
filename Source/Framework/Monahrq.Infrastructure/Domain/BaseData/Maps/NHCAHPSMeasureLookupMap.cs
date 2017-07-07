using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Domain.BaseData.Maps;
using NHibernate.Type;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
	[Monahrq.Infrastructure.Domain.Data.MappingProviderExport]
	public class NHCAHPSMeasureLookupMap : GeneratedKeyLookupMap<NHCAHPSMeasureLookup>
	{
		public NHCAHPSMeasureLookupMap()
		{
			Map(x => x.MeasureId)
				.Not.Nullable();
			Map(x => x.MeasureType)
				.Not.Nullable();
			Map(x => x.CAHPSQuestionType)
				.Nullable();
		}
		protected override FluentNHibernate.Mapping.PropertyPart NameMap()
		{
			return null;
		}
	}

}
