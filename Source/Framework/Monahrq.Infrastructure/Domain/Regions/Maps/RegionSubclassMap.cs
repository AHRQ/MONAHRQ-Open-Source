using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Maps;

namespace Monahrq.Infrastructure.Domain.Regions.Maps
{
    public abstract class RegionSubclassMap<T> : OwnedEntitySubclassMap<T, int, HospitalRegistry, int>
        where T : Region
    {
        protected RegionSubclassMap()
        {
            DiscriminatorValue(typeof(T).Name);
        }
    }
}