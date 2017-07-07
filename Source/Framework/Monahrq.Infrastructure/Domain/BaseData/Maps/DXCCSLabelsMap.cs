using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class DXCCSLabelsMap : GeneratedKeyLookupMap<DXCCSLabels>
    {
        public DXCCSLabelsMap()
        {
            Map(x => x.CategoryID).Index("IDX_DXCCSLABELS_CATEGORYID");
            Map(x => x.DXCCSID).Index("IDX_DXCCSLABELS_DXCCSID");
            Map(x => x.Description).Length(100);
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
