using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Services;

namespace Monahrq.Measures.Service
{
    /// <summary>
    /// Topic service class
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Services.EntityService{Monahrq.Infrastructure.Entities.Domain.Measures.Topic, System.Int32}" />
    [Export]
    public class TopicService: EntityService<Topic, int>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicService"/> class.
        /// </summary>
        /// <param name="factoryProvider">The factory provider.</param>
        [ImportingConstructor]
        public TopicService(IDomainSessionFactoryProvider factoryProvider):
            base(factoryProvider)
        {

        }


    }
}
