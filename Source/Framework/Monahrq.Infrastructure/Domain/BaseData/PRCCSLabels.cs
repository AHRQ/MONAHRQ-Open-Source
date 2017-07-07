using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class PRCCSLabels : Entity<int>
    {
        public virtual int CategoryID { get; set; }
        public virtual int PRCCSID { get; set; }
        public virtual string Description { get; set; }
    }
}
