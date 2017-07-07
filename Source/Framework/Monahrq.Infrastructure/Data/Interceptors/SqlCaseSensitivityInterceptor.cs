using NHibernate;
using NHibernate.SqlCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Data.Interceptors
{
    public class SqlCaseSensitivityInterceptor : EmptyInterceptor, IInterceptor
    {
        SqlString IInterceptor.OnPrepareStatement(SqlString sql)
        {
            //if(sql != null && sql.Length > 0) 
            //{
            //    var tempSQL = sql.ToString().ToLowerInvariant();
            //    tempSQL = tempSQL.Replace("true", "True")
            //                     .Replace("false", "False");

            //    sql = new SqlString();
            //}
            return sql;
        }
    }
}
