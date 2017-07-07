using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Domain.Hospitals;

namespace Monahrq.Wing.BaseData.HSABaseData
{
    [Export]
    public class HospitalServiceAreaRegionStrategy:
            IEntityDataReaderStrategy<HospitalServiceArea, int>
    {
        public HospitalServiceArea LoadFromReader(System.Data.IDataReader reader)
        {
            var result = new HospitalServiceArea()
            {
                ImportRegionId = reader["HSANUM"].ToString(),
                State = reader["HSASTATE"].ToString(),
                City = reader["HSACITY"].ToString()
            };
            return result;
        }
    }
}
 