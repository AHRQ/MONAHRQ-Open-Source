using Microsoft.Practices.Prism.Events;

namespace Monahrq.Infrastructure.Services
{
    /// <summary>
    /// The custom domain service error event. This event should be thrown when an error occurs in the domain service endpoint/method.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Infrastructure.Services.ServiceErrorEventArgs}" />
    public class ServiceErrorEvent:CompositePresentationEvent<ServiceErrorEventArgs>{}
}