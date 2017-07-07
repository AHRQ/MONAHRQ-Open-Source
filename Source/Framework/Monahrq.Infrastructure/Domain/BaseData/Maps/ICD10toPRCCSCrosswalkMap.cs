using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class ICD10toPRCCSCrosswalkMap : GeneratedKeyLookupMap<ICD10toPRCCSCrosswalk>
    {
        public ICD10toPRCCSCrosswalkMap()
        {
            Map(x => x.ICDID).Length(10).Index("IDX_ICD10TOPRCCSCROSSWALKMAP_ICDID");
            Map(x => x.PRCCSID).Index("IDX_ICD10TOPRCCSCROSSWALKMAP_PRCCSID");
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}