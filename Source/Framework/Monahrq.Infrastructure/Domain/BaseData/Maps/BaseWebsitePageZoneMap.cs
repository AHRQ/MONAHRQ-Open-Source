using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Data.CustomTypes;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Domain.BaseData.Maps;
using NHibernate.Type;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class BaseWebsitePageZoneMap : GeneratedKeyLookupMap<BaseWebsitePageZone>
	{
		public BaseWebsitePageZoneMap()
		{
			Map(x => x.CodePath)
				.Not.Nullable();
			Map(x => x.ZoneType)
				.CustomType<NHibernate.Type.EnumStringType<WebsitePageZoneTypeEnum>>()
				.Not.Nullable();
			Map(x => x.WebsitePageName)
				.Not.Nullable();
			Map(x => x.Audience).CustomType<EnumStringType<Reports.Audience>>()
				.Not.Nullable();
		}


		protected override PropertyPart NameMap()
		{
			return Map(i => i.Name)
				.Length(255).Not.Nullable()
				;//.Index(NameIndexName);
		}
	}
}