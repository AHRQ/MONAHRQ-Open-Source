using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class CostToChargeToDXCCS : Entity<int>
    {
        public virtual int DXCCSID { get; set; }
        public virtual float Ratio { get; set; }
        public virtual string Year { get; set; }
    }
}
