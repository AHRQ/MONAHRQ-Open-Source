using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Domain.Wings
{
    public interface ITargetDeletionVisitor : ISessionAwareVisitor
    {
        string TargetType { get; }

        int Order { get; }
    }

    public interface ISessionAwareVisitor
    {
        void Visit(IEntity entity, VisitorOptions options /*NHibernate.ISession session*/);
    }

    public class VisitorOptions
    {
        public IDomainSessionFactoryProvider DataProvider { get; set; }
        public ILogWriter Logger { get; set; }
    }
}
