using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class ICD9toPRCCSCrosswalk : Entity<int>
    {
        public virtual string ICD9ID { get; set; }
        public virtual int PRCCSID { get; set; }
    }
}
