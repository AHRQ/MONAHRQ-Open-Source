using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class ConsumerPriceIndex : Entity<int>
    {
        public virtual int Year { get; set; }
        public virtual float Value { get; set; }
    }
}
