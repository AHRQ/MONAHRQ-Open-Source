using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Data
{

    using NHibernate;
    using NHibernate.Cfg;

    public interface ISessionFactoryProvider
    {
        ISessionFactory GetSessionFactory();
        Configuration GetConfiguration();
    }
}
