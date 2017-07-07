using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class ZipCodeToPopulation : Entity<int>
    {
        public virtual string ZipCode { get; set; }
        public virtual int Sex { get; set; }
        public virtual int AgeGroup { get; set; }
        public virtual int Race { get; set; }
        public virtual int Year { get; set; }
        public virtual int Population { get; set; }
    }
}

