using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class PRCCSLabelsMap : GeneratedKeyLookupMap<PRCCSLabels>
    {
        public PRCCSLabelsMap()
        {
            Map(x => x.CategoryID).Index("IDX_PRCCSLABELS_CATEGORYID");
            Map(x => x.PRCCSID).Index("IDX_PRCCSLABELS_PRCCSID");
            Map(x => x.Description).Length(100);
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
