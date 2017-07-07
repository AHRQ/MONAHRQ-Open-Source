using Monahrq.Infrastructure.Domain.BaseData;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class EDNationalTotalsMap : GeneratedKeyLookupMap<EDNationalTotals>
    {
        public EDNationalTotalsMap()
        {
            Map(x => x.CCSID).Index("IDX_EDNATIONALTOTALS_CCSID");
            Map(x => x.NumEdVisits);
            Map(x => x.NumEdVisitsStdErr);
            Map(x => x.NumAdmitHosp);
            Map(x => x.NumAdmitHospStdErr);
            Map(x => x.DiedEd);
            Map(x => x.DiedEdStdErr);
            Map(x => x.DiedHosp);
            Map(x => x.DiedHospStdErr);
        }

        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}
