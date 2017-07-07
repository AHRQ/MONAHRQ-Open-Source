using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals.Mapping
{
    internal static class HospitalExtensions
    {
        public static KeyValuePair<int, string> HospitalCms(this Hospital source)
        {
            return new KeyValuePair<int, string>(source.Id, (source.CmsProviderID ?? null).Trim());
        }
    }
}
