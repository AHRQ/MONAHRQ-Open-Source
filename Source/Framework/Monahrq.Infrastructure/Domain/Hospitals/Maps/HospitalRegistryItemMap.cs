using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Infrastructure.Entities.Data.Strategies;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals.Maps
{
    public abstract class HospitalRegistryItemMap<T> : EntityMap<T, int, IdentityGeneratedKeyStrategy>
          where T : HospitalRegistryItem
    {
        protected HospitalRegistryItemMap()
            : base()
        {
            //References<HospitalRegistry>(x => x.Registry, "Registry_Id").LazyLoad();
            Map(x => x.IsSourcedFromBaseData).Not.Nullable().Default("0");
            Map(x => x.Version);
        }
    }
}
