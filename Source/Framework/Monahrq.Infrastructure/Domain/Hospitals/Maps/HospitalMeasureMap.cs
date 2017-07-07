using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Domain.Hospitals.Maps
{
    [SubclassMappingProviderExport]
    public class HospitalMeasureMap : SubclassMap<HospitalMeasure>
    {
        public HospitalMeasureMap()
        {
            DiscriminatorValue("HOSPITAL");
        }
    }
}
