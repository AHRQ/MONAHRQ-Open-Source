using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class CostToChargeToDRGMap : GeneratedKeyLookupMap<CostToChargeToDRG>
    {
        public CostToChargeToDRGMap()
        {
            Map(x => x.DRGID).Length(12).Index("IDX_COST_TO_CHARGE_TO_DRG_DRGID");
            Map(x => x.Ratio);
            Map(x => x.Year).Length(4).Index("IDX_COST_TO_CHARGE_TO_DRG_YEAR");
        }
        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
