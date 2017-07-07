using Monahrq.Infrastructure.Data.Conventions;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged, EntityTableName("Base_Sexes")]
    public class Sex : EnumLookupEntity<int>
    {
    }
}
