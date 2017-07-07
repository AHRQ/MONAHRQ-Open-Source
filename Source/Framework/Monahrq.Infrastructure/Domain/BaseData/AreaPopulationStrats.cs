using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class AreaPopulationStrats : Entity<int>
    {
        public virtual int StateCountyFIPS { get; set; }
        public virtual int CatID { get; set; }
        public virtual int CatVal { get; set; }
        public virtual int Year { get; set; }
        public virtual int Population { get; set; }
    }
}

