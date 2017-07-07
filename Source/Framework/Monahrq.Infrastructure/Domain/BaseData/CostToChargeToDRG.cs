using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class CostToChargeToDRG : Entity<int>
    {
        public virtual int DRGID { get; set; }
        public virtual float Ratio { get; set; }
        public virtual string Year { get; set; }
    }
}
