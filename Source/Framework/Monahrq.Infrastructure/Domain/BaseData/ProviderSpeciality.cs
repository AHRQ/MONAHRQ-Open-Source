using System;
using Monahrq.Infrastructure.Data.Conventions;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [Serializable, ImplementPropertyChanged,
     EntityTableName("Base_ProviderSpecialities")]
    public class ProviderSpeciality : Entity<int>
    {
        public string ProviderTaxonomy { get; set; }
    }
}
