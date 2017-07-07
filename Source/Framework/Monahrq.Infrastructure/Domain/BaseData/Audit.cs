using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class Audit : Entity<int>
    {
        public virtual decimal DataVersion { get; set; }
        public virtual string Dataset { get; set; }
        public virtual string Contract { get; set; }
    }
}