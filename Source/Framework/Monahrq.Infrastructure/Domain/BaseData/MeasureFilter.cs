using System;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.BaseData
{

    [Serializable, ImplementPropertyChanged]
    [EntityTableName("Base_MeasureFilters")]
    public class MeasureFilter: Entity<int>
    {
        public MeasureFilterValue Value
        {
            get
            {
                return (MeasureFilterValue)Id;
            }
        }
    }

    public enum MeasureFilterValue
    {
        None = 0,
        Website,
        Topic,
        Subtopic,
        Source,
        ClinicalTitle,
        MeasureType,
        NQFID
    }

    namespace ViewModel
    {

        [ImplementPropertyChanged]
        public class MeasureFilterViewModel : Entity<int>
        {
        }

    }
}
