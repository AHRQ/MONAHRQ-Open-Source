using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Domain.Hospitals;

namespace Monahrq.Wing.BaseData.HRRBaseData
{
    [Export]
    public class HealthReferralRegionStrategy:
            IEntityDataReaderStrategy<HealthReferralRegion,  int>
    {
        public HealthReferralRegion LoadFromReader(System.Data.IDataReader reader)
        {
            var result = new HealthReferralRegion()
            {
                ImportRegionId = reader["HRRNumber"].ToString(),
                State = reader["HRRState"].ToString(),
                City = reader["HRRCity"].ToString()
            };
            return result;
        }
    }
}
 