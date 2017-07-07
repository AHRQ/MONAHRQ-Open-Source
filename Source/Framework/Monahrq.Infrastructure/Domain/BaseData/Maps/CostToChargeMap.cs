using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class CostToChargeMap : GeneratedKeyLookupMap<CostToCharge>
    {
        public CostToChargeMap()
        {
            Map(x => x.ProviderID).Length(13).Index("IDX_COST_TO_CHARGE_PROVIDER_ID");
            Map(x => x.Ratio);
            Map(x => x.Year).Length(5).Index("IDX_COST_TO_CHARGE_YEAR");
        }
        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
