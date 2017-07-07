using FluentNHibernate.Cfg.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Monahrq.Infrastructure.Data
{
    public interface IDataServicesProvider
    {
        NHibernate.Cfg.Configuration BuildConfiguration();
        IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase);
        string GetDBSchemaVersion(SqlConnection con);
        bool UpgradeDatabase(SqlConnection con);
    }
}
