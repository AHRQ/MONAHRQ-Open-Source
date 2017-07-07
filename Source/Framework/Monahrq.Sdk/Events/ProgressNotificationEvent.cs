using Microsoft.Practices.Prism.Events;

namespace Monahrq.Sdk.Events
{
    /// <summary>
    /// The Progress Notification Event
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.Int32}" />
    public class ProgressNotificationEvent : CompositePresentationEvent<int> { }
}
