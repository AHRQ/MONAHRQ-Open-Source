using Monahrq.Infrastructure.Domain.Categories;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals.Maps
{
    [Infrastructure.Domain.Data.MappingProviderExport]
    public class HospitalRegistryMap : EntityMap<HospitalRegistry, int, IdentityGeneratedKeyStrategy>
    {
        public HospitalRegistryMap()
            : base()
        {
            Map(x => x.DataVersion).Unique().Not.Nullable();
            Map(x => x.ImportDate).Not.Nullable();
            HasMany<Hospital>(x => x.Hospitals).KeyColumn("Registry_Id")
                .Not.LazyLoad()
                .Inverse()
                .Cascade.AllDeleteOrphan();
            HasMany<HospitalServiceArea>(x => x.HospitalServiceAreas).Not.LazyLoad().Inverse().Cascade.AllDeleteOrphan();
            HasMany<HealthReferralRegion>(x => x.HealthReferralRegions).Not.LazyLoad().Inverse().Cascade.AllDeleteOrphan();
            HasMany<CustomRegion>(x => x.CustomRegions).Not.LazyLoad().Inverse().Cascade.AllDeleteOrphan();
            HasMany<HospitalCategory>(x => x.HospitalCategories)
                .KeyColumn("Registry_Id")
                .Not.LazyLoad()
                .Inverse()
                .Cascade.AllDeleteOrphan();
        }

        public override string EntityTableName
        {
            get
            {
                return "Hospitals_" + Inflector.Pluralize(typeof(HospitalRegistry).Name);
            }
        }
    }

     
}
