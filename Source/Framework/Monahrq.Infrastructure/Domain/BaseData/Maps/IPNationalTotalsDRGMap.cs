using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class IPNationalTotalsDRGMap : GeneratedKeyLookupMap<IPNationalTotalsDRG>
    {
        public IPNationalTotalsDRGMap()
            : base()
        {
            Map(x => x.DRGID).Index("IDX_IPNATIONALTOTALSDRG_DRGID");
            Map(x => x.Region).Index("IDX_IPNATIONALTOTALSDRG_REGION");
            Map(x => x.Discharges);
            Map(x => x.DischargesStdErr);
            Map(x => x.MeanCharges);
            Map(x => x.MeanChargesStdErr);
            Map(x => x.MeanCost);
            Map(x => x.MeanCostStdErr);
            Map(x => x.MeanLOS);
            Map(x => x.MeanLOSStdErr);
            Map(x => x.MedianCharges);
            Map(x => x.MedianCost);
            Map(x => x.MedianLOS);
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
