using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Default.ViewModels
{
    /// <summary>
    /// class for even payload
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventPayloadBaseViewModel<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventPayloadBaseViewModel{T}"/> class.
        /// </summary>
        /// <param name="payload">The payload.</param>
        public EventPayloadBaseViewModel(T payload)
        {
            Payload = payload;
        }
        /// <summary>
        /// Gets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        public T Payload { get; private set; }
    }
}
