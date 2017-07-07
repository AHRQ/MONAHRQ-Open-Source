using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Regions
{
    [ImplementPropertyChanged, EntityTableName("RegionPopulationStrats")]
    public class RegionPopulationStrats : Entity<int>
    {
        public virtual int RegionType { get; set; }
        public virtual int RegionID { get; set; }
        public virtual int CatID { get; set; }
        public virtual int CatVal { get; set; }
        public virtual int Year { get; set; }
        public virtual int Population { get; set; }
    }

    public enum RegionTypeEnum
    {
        Custom = 0,
        HRR = 1,
        HSA = 2
    }
}
