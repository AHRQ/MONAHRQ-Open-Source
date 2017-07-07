using System;
using Microsoft.Practices.Prism.Events;

namespace Monahrq.Infrastructure.Services
{
    /// <summary>
    /// The custom error notification event.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.Exception}" />
    public class ErrorNotificationEvent : CompositePresentationEvent<Exception> { }
}