using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class IPNationalTotalsDXCCSMap : GeneratedKeyLookupMap<IPNationalTotalsDXCCS>
    {
        public IPNationalTotalsDXCCSMap()
            : base()
        {
            Map(x => x.DXCCSID).Index("IDX_IPNATIONALTOTALSDXCCS_DXCCSID");
            Map(x => x.Region).Index("IDX_IPNATIONALTOTALSDXCCS_REGION");
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
