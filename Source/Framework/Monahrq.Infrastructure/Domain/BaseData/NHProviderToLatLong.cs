using Monahrq.Infrastructure.BaseDataLoader;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class NHProviderToLatLong : Entity<int>
    {
        public virtual string ProviderId { get; set; }
        public virtual double Latitude { get; set; }
        public virtual double Longitude { get; set; }
    }
}
