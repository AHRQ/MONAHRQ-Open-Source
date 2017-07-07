using System;
using Monahrq.Infrastructure.Data.Conventions;
using PropertyChanged;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Domain.Data;
using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using NHibernate.Type;

namespace Monahrq.Infrastructure.Entities.Domain.WebsiteContentManagement.Maps
{
	[MappingProviderExport]
	public class WebsitePageMap : EntityMap<WebsitePage, int, IdentityGeneratedKeyStrategy>
//	public class WebsitePageMap : OwnedEntityMap<WebsitePage, int, Website, int, IdentityGeneratedKeyStrategy>
	{

		public WebsitePageMap()
		{
			//References(x => x.Website,"Website_Id");
			Map(x => x.ReportName);
			Map(x => x.PageType)
				.CustomType<NHibernate.Type.EnumStringType<WebsitePageTypeEnum>>()
				.Not.Nullable();
			Map(x => x.Audience).CustomType<EnumStringType<Reports.Audience>>()
				.Not.Nullable();
			Map(x => x.IsEditable)
				.Not.Nullable();
			HasMany(x => x.Zones)
				.KeyColumn("WebsitePage_Id")
				.Not.Inverse()
				.Not.LazyLoad()
				.Cascade.AllDeleteOrphan();
		}
	}
}