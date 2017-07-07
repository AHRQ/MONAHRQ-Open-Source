using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;

namespace Monahrq.Infrastructure.Domain.Websites.Maps
{
    [MappingProviderExport]
    public class ActivityLogEntryMap : EntityMap<ActivityLogEntry, int, IdentityGeneratedKeyStrategy>
    {
        public ActivityLogEntryMap()
            : base()
        {
            Map(x => x.Description, "[Description]");
            Map(x => x.Date, "[Date]");
            //Map(x => x.Index, "[Index]").Not.Nullable().Default("0"); 
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
