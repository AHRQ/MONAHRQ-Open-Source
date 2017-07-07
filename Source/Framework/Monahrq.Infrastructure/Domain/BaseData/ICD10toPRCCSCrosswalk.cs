using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class ICD10toPRCCSCrosswalk : Entity<int>
    {
        public virtual string ICDID { get; set; }
        public virtual int PRCCSID { get; set; }
    }
}