using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using System.ComponentModel.DataAnnotations;
using Monahrq.Infrastructure.Core.Attributes;

namespace Monahrq.Wing.HospitalSpendingByClaimType
{
    /// <summary>
    /// Provide fluent NHibernate mappings for the <see cref="HospitalSpendingByClaimTypeTarget"/> type
    /// </summary>
    public class HospitalSpendingByClaimTypeTargetMap : DatasetRecordMap<HospitalSpendingByClaimTypeTarget>
    {
        public HospitalSpendingByClaimTypeTargetMap()
        {
            this.Map(m => m.CmsProviderId).Not.Nullable().Length(12);
            this.Map(m => m.ClaimType).CustomType<ClaimType>();
            this.Map(m => m.Period).CustomType<ClaimType>();
            this.Map(m => m.AverageSpendingPerEpisodeHospital);
            this.Map(m => m.AverageSpendingPerEpisodeNational);
            this.Map(m => m.AverageSpendingPerEpisodeState);
            this.Map(m => m.PercentSpendingPerEpisodeHospital);
            this.Map(m => m.PercentSpendingPerEpisodeNational);
            this.Map(m => m.PercentSpendingPerEpisodeState);

            this.CompositeId()
                .KeyProperty(p => p.CmsProviderId)
                .KeyProperty(p => p.ClaimType)
                .KeyProperty(p => p.Period);
        }
    }
}
