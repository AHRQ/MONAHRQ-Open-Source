using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Data.CustomTypes;

namespace Monahrq.Infrastructure.Entities.Domain.Reports.Map
{
    public class ReportTemplateAsString : CustomType
    {
        protected override DbType UnderlyingType
        {
            get { return DbType.String; }
        }

        public override object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            names = names ?? new string[0];
            if (names.Length == 0) return null;
            int ordinal = rs.GetOrdinal(names[0]);
            if (rs.IsDBNull(ordinal))
            {
                return null;
            }
            else
            {
                return ReportManifest.Deserialize(rs[ordinal].ToString()); 
            }
        }

        public override void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            var temp = value as ReportManifest;
            ((IDbDataParameter)cmd.Parameters[index]).Value = temp == null ? (object)DBNull.Value : (object) temp.ToString();
        }
    }
}
