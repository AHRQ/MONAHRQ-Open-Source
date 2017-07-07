using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.Services
{
    /// <summary>
    /// The generic entity domain service.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public class EntityService<T, TKey>
        where T : IEntity<TKey>
    {
        /// <summary>
        /// Gets or sets the factory provider.
        /// </summary>
        /// <value>
        /// The factory provider.
        /// </value>
        protected IDomainSessionFactoryProvider FactoryProvider { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityService{T, TKey}"/> class.
        /// </summary>
        /// <param name="factoryProvider">The factory provider.</param>
        public EntityService(IDomainSessionFactoryProvider factoryProvider)
        {
            FactoryProvider = factoryProvider;
        }
    }
}
