using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.Services
{
    /// <summary>
    /// The monahrq data service.
    /// </summary>
    public class MonahrqDataservice
    {
        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        protected IDomainSessionFactoryProvider Provider { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonahrqDataservice"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        [ImportingConstructor]
        public MonahrqDataservice(IDomainSessionFactoryProvider provider)
        {
            Provider = provider;
        }
    }
}
