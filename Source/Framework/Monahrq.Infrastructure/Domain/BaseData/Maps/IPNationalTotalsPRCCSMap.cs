using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class IPNationalTotalsPRCCSMap : GeneratedKeyLookupMap<IPNationalTotalsPRCCS>
    {
        public IPNationalTotalsPRCCSMap()
            : base()
        {
            Map(x => x.PRCCSID).Index("IDX_IPNATIONALTOTALSPRCCS_PRCCSID");
            Map(x => x.Region).Index("IDX_IPNATIONALTOTALSPRCCS_REGION");
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
