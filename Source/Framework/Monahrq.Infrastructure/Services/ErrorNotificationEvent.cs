using System;
using Microsoft.Practices.Prism.Events;

namespace Monahrq.Infrastructure.Services
{
    /// <summary>
    /// The custom error notification event.
    /// </summary>
    /// <seealso cref="CompositePresentationEvent{Exception}" />
    [Obsolete("Use better exception handling or modify this event to contain some useful contextual information")]
    public class ErrorNotificationEvent : CompositePresentationEvent<Exception> { }
}