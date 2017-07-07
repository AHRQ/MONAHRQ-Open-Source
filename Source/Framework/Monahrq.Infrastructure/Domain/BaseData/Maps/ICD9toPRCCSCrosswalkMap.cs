using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [Monahrq.Infrastructure.Domain.Data.MappingProviderExport]
    public class ICD9toPRCCSCrosswalkMap : GeneratedKeyLookupMap<ICD9toPRCCSCrosswalk>
    {
        public ICD9toPRCCSCrosswalkMap()
        {
            Map(x => x.ICD9ID).Length(10).Index("IDX_ICD9TOPRCCSCROSSWALK_ICD9ID");
            Map(x => x.PRCCSID).Index("IDX_ICD9TOPRCCSCROSSWALK_CCSID");
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
