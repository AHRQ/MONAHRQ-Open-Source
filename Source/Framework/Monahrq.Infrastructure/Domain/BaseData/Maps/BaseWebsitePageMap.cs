using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Domain.BaseData.Maps;
using NHibernate.Type;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
	[MappingProviderExport]
	public class BaseWebsitePageMap : GeneratedKeyLookupMap<BaseWebsitePage>
	{
		public BaseWebsitePageMap()
		{
			Map(x => x.TemplateRelativePath)
				.Not.Nullable();
			Map(x => x.PublishRelativePath)
				.Not.Nullable();
			Map(x => x.PageType)
				.CustomType<NHibernate.Type.EnumStringType<WebsitePageTypeEnum>>()
				.Not.Nullable();
			Map(x => x.ReportName)
				.Nullable();
			Map(x => x.Audience).CustomType<EnumStringType<Reports.Audience>>()
				.Not.Nullable();
			Map(x => x.Url)
				.Not.Nullable();
			Map(x => x.IsEditable)
				.Not.Nullable();
		}
	}
}
