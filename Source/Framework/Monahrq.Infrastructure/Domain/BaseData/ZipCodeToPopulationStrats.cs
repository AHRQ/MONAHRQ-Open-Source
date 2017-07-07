using Monahrq.Infrastructure.BaseDataLoader;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    //[BaseDataImport()]
    public class ZipCodeToPopulationStrats : Entity<int>
    {
        public virtual string ZipCode { get; set; }
        public virtual int CatID { get; set; }
        public virtual int CatVal { get; set; }
        public virtual int Year { get; set; }
        public virtual int Population { get; set; }
    }
}
