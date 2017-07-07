using Monahrq.Infrastructure.Domain.Data;
using FluentNHibernate;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class ICD9toDXCCSCrosswalkMap : GeneratedKeyLookupMap<ICD9toDXCCSCrosswalk>
    {
        public ICD9toDXCCSCrosswalkMap()
        {
            Map(x => x.ICD9ID).Length(10).Index("IDX_ICD9TODXCCSCROSSWALK_ICD9ID");
            Map(x => x.DXCCSID).Index("IDX_ICD9TODXCCSCROSSWALK_CCSID");
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
