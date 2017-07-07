using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class CostToCharge : Entity<int>
    {
        public virtual string ProviderID { get; set; }
        public virtual double Ratio { get; set; }
        public virtual string Year { get; set; }
    }
}
