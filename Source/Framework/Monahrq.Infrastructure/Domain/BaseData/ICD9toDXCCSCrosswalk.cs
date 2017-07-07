using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class ICD9toDXCCSCrosswalk : Entity<int>
    {
        public virtual string ICD9ID { get; set; }
        public virtual int DXCCSID { get; set; }
    }
}
