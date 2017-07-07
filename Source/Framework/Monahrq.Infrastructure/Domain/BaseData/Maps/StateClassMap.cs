using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData.Maps
{
    [MappingProviderExport]
    public class StateClassMap : GeneratedKeyLookupMap<State>
    {
        public StateClassMap()
        {

            Map(x => x.FIPSState).Index("IDX_STATE_FIPS"); ;
            Map(x => x.Abbreviation).Index("IDX_STATE_ABBR");
            Map(x => x.MinX);
            Map(x => x.MinY);
            Map(x => x.MaxY);
            Map(x => x.MaxX);
            Map(x => x.X0);
            Map(x => x.Y0);

            // See Jason: why are we referencing regions and hospitals from states?
            //HasMany<HealthReferralRegion>(x => x.HealthReferralRegions)
            //    .Cascade.SaveUpdate()
            //    .Not.LazyLoad();
            //HasMany<HospitalServiceArea>(x => x.HospitalServiceAreas)
            //    .Cascade.SaveUpdate()
            //    .Not.LazyLoad();
            //HasMany<CustomRegion>(x => x.CustomRegions)
            //    .Cascade.SaveUpdate()
            //    .Not.LazyLoad();

            // see Jason: commented out to resolve exception:
            //          "Initializing[Monahrq.Infrastructure.Entities.Domain.BaseData.State#5]-Could not initialize proxy - no Session."
            // HasMany<Hospital>(x => x.Hospitals).LazyLoad();

            // if count is needed, use this...
            //  .Formula().Not.Insert().Not.Update()
        }
    }
}
