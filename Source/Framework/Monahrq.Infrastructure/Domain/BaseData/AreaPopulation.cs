using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class AreaPopulation : Entity<int>
    {
        public virtual int StateCountyFIPS { get; set; }
        public virtual int Sex { get; set; }
        public virtual int AgeGroup { get; set; }
        public virtual int Race { get; set; }
        public virtual int Year { get; set; }
        public virtual int Population { get; set; }
    }
}

