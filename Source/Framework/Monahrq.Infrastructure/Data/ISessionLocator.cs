using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Data
{

    public interface ISessionLocator
    {
        ISession For(Type entityType);
    }
}
