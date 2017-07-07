using System;
using Monahrq.Infrastructure.Data.Conventions;
using PropertyChanged;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Domain.Data;
using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Data.CustomTypes;

namespace Monahrq.Infrastructure.Entities.Domain.WebsiteContentManagement.Maps
{
	[MappingProviderExport]
	public class WebsitePageZoneMap : EntityMap<WebsitePageZone, int, IdentityGeneratedKeyStrategy>
	{

		public WebsitePageZoneMap()
		{
			Map(x => x.Contents).CustomType("StringClob").CustomSqlType("nvarchar(max)");
			Map(x => x.Audience)
				.Not.Nullable();
			Map(x => x.ZoneType)
				.CustomType<NHibernate.Type.EnumStringType<BaseData.WebsitePageZoneTypeEnum>>()
				.Not.Nullable();
		}
	}
}