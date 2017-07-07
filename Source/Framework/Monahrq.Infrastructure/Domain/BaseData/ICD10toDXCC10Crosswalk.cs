using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class ICD10toDXCCSCrosswalk : Entity<int>
    {
        public virtual string ICDID { get; set; }
        public virtual int DXCCSID { get; set; }
    }
}