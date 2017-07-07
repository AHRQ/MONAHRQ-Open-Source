using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [Monahrq.Infrastructure.Domain.Data.MappingProviderExport]
    public class ConsumerPriceIndexMap : GeneratedKeyLookupMap<ConsumerPriceIndex>
    {
        public ConsumerPriceIndexMap()
        {
            Map(x => x.Year);
            Map(x => x.Value);
        }
        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
