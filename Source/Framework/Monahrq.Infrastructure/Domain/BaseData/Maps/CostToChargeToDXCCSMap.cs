using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class CostToChargeToDXCCSMap : GeneratedKeyLookupMap<CostToChargeToDXCCS>
    {
        public CostToChargeToDXCCSMap()
        {
            Map(x => x.DXCCSID).Length(12).Index("IDX_COST_TO_CHARGE_TO_DXCCS_DXCCSID");
            Map(x => x.Ratio);
            Map(x => x.Year).Length(4).Index("IDX_COST_TO_CHARGE_TO_DXCCS_YEAR");
        }
        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
