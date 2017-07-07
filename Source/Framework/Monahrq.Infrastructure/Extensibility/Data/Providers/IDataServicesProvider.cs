using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monahrq.Sdk.Extensibility.Data.Providers
{
    using FluentNHibernate.Cfg.Db;
    using NHibernate.Cfg;

    public interface IDataServicesProvider  
    {
        Configuration BuildConfiguration(SessionFactoryParameters sessionFactoryParameters);
        IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase);
    }
}
