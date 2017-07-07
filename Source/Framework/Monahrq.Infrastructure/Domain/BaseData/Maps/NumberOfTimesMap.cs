using Monahrq.Infrastructure.Domain.BaseData;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [Infrastructure.Domain.Data.MappingProviderExport]
    public class NumberOfTimesMap : EnumLookupEntityMap<NumberOfTimes>
    { }

    [Infrastructure.Domain.Data.MappingProviderExport]
    public class NumberOfTimes2Map : EnumLookupEntityMap<NumberOfTimes2>
    { }
}