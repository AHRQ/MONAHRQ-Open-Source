using System;
using Monahrq.Infrastructure.Domain.Categories;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.NursingHomes
{
    [Serializable,
     ImplementPropertyChanged]
    public class NursingHomeCategory : Category
    {
        protected NursingHomeCategory()
        {
        }

        //public virtual int NursingHomeCount { get; set; }
    }
}