using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class ICD10toDXCCSCrosswalkMap : GeneratedKeyLookupMap<ICD10toDXCCSCrosswalk>
    {
        public ICD10toDXCCSCrosswalkMap()
        {
            Map(x => x.ICDID).Length(10).Index("IDX_ICD10TODXCCS10CROSSWALK_ICDID");
            Map(x => x.DXCCSID).Index("IDX_ICD10TODXCCS10CROSSWALK_DXCCSID");
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}