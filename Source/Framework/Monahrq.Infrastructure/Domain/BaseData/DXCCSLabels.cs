using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class DXCCSLabels : Entity<int>
    {
        public virtual int CategoryID { get; set; }
        public virtual int DXCCSID { get; set; }
        public virtual string Description { get; set; }
    }
}
