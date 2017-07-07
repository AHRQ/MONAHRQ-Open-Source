using System;
using NHibernate;

namespace Monahrq.Infrastructure.Entities.Domain
{
    /// <summary>
    /// Provides a <see cref="ISessionFactory"/> instance
    /// </summary>
    public interface IDomainSessionFactoryProvider 
    {
        /// <summary>
        /// Indicates when this <see cref="IDomainSessionFactoryProvider"/> was constructed
        /// </summary>
        DateTime WhenBuilt { get; }

        ISessionFactory SessionFactory { get; }
    }
}
