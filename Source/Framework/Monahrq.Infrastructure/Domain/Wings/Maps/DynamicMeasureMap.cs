using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Domain.Wings.Maps
{
    [SubclassMappingProviderExport]
    public class DynamicMeasureMap : SubclassMap<DynamicMeasure>
    {
        public DynamicMeasureMap()
        {
            DiscriminatorValue("DYNAMIC");
        }
    }
}
