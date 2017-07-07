using Microsoft.Practices.Prism.Events;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Configuration;
using NHibernate;
using Monahrq.Infrastructure.Entities.Events;

namespace Monahrq.Sdk.Events
{
    public class ConnectionStringEventArgs: ExtendedEventArgs<DbConnectionStringBuilder>
    {
        public ConnectionStringEventArgs(DbConnectionStringBuilder builder): base(builder)
        {
        }
    }

    public class ConnectionStringFailedEventArgs : ExtendedEventArgs<DbConnectionStringBuilder>
    {
        public Exception ConnectionException { get; private set; }
 
        public ConnectionStringFailedEventArgs(DbConnectionStringBuilder builder, Exception ex)
            : base(builder)
        {
            ConnectionException = ex;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ConnectionStringSuccessEventArgs : ExtendedEventArgs<DbConnectionStringBuilder>
    {
        public string Message { get; private set; }
        public bool Success { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStringSuccessEventArgs"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="message">The message.</param>
        /// <param name="success">if set to <c>true</c> [success].</param>
        public ConnectionStringSuccessEventArgs(DbConnectionStringBuilder builder, string message, bool success = true)
            : base(builder)
        {
            Message = message;
            Success = success;
        }
    }

    public class ConnectionFailedEvent : CompositePresentationEvent<ConnectionStringFailedEventArgs> { }
    public class ConnectionSuccessEvent : CompositePresentationEvent<ConnectionStringSuccessEventArgs> { }

    public class NamedConnectionElementEvent : ExtendedEventArgs<NamedConnectionElement> 
    {
        public NamedConnectionElementEvent(NamedConnectionElement element)
            : base(element)
        {
        }
    }

    public class NamedConnectionReadyEvent : CompositePresentationEvent<NamedConnectionElementEvent> { }


    public class SessionWrapper
    {
        public ISession Session {get;set;}
        public SessionWrapper(ISession session)
        {
            Session = session;
        }
    }


}
