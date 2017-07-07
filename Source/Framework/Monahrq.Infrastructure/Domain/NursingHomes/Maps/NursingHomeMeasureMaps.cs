using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Infrastructure.Domain.NursingHomes.Maps
{
    [SubclassMappingProviderExport]
    public class NursingHomeMeasureMap : SubclassMap<NursingHomeMeasure>
    {
        public NursingHomeMeasureMap()
        {
            DiscriminatorValue("NURSINGHOME");

            var indexName = string.Format("IDX_{0}", typeof (Measure).EntityTableName());

           
        }
    }
}