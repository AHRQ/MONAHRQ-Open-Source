using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.BaseData
{
    [ImplementPropertyChanged]
    [EntityTableName("Base_NumberOfTimes")]
    public class NumberOfTimes : EnumLookupEntity<int>
    {}

    [ImplementPropertyChanged]
    [EntityTableName("Base_NumberOfTimes2")]
    public class NumberOfTimes2 : EnumLookupEntity<int>
    { }
}