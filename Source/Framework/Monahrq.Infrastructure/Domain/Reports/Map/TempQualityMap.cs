using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Infrastructure.Entities.Data.Strategies;

namespace Monahrq.Infrastructure.Entities.Domain.Reports.Map
{
    [Infrastructure.Domain.Data.MappingProviderExport]
	public class TempQualityMap : EntityMap<TempQuality, int, IdentityGeneratedKeyStrategy>	// ClassMap<TempQuality>
	{
        
        public TempQualityMap() {
            Table("Temp_Quality");
            LazyLoad();
            Map(x => x.ReportId).Column("ReportID").Not.Nullable();
            Map(x => x.MeasureId).Column("MeasureID").Not.Nullable();
            Map(x => x.HospitalId).Column("HospitalID");
            Map(x => x.CountyId).Column("CountyID");
            Map(x => x.RegionId).Column("RegionID");
            Map(x => x.ZipCode).Column("ZipCode");
            Map(x => x.HospitalType).Column("HospitalType");
            Map(x => x.RateAndCI).Column("RateAndCI");
            Map(x => x.NatRating).Column("NatRating");
            Map(x => x.NatFilled).Column("NatFilled");
            Map(x => x.PeerRating).Column("PeerRating");
            Map(x => x.PeerFilled).Column("PeerFilled");
            Map(x => x.Col1).Column("Col1");
            Map(x => x.Col2).Column("Col2");
            Map(x => x.Col3).Column("Col3");
            Map(x => x.Col4).Column("Col4");
            Map(x => x.Col5).Column("Col5");
            Map(x => x.Col6).Column("Col6");
            Map(x => x.Col7).Column("Col7");
            Map(x => x.Col8).Column("Col8");
            Map(x => x.Col9).Column("Col9");
            Map(x => x.Col10).Column("Col10");
        }
    }
}

