using Microsoft.Practices.Prism.Events;
using System.Threading;

namespace Monahrq.Infrastructure
{
    /// <summary>
    /// The disable navigation event.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Infrastructure.DisableNavigationEvent}" />
    public class DisableNavigationEvent : CompositePresentationEvent<DisableNavigationEvent>
    {

        /// <summary>
        /// Gets or sets a value indicating whether [disable UI elements].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [disable UI elements]; otherwise, <c>false</c>.
        /// </value>
        public bool DisableUIElements { get; set; }
    }

    /// <summary>
    /// The starting website publishing event.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Infrastructure.StartingWebsitePublishingEvent}" />
    public class StartingWebsitePublishingEvent : CompositePresentationEvent<StartingWebsitePublishingEvent>
    {
        /// <summary>
        /// Gets or sets the thread.
        /// </summary>
        /// <value>
        /// The thread.
        /// </value>
        public Thread Thread { get; set; }
    }

    /// <summary>
    /// The cancelling the website publishing event.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Infrastructure.CancellingWebsitePublishingEvent}" />
    public class CancellingWebsitePublishingEvent : CompositePresentationEvent<CancellingWebsitePublishingEvent>
    {
    }

    /// <summary>
    /// The message update event.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Infrastructure.MessageUpdateEvent}" />
    public class MessageUpdateEvent : CompositePresentationEvent<MessageUpdateEvent>
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
    }

    /// <summary>
    /// The UI message update event.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.MessageUpdateEvent" />
    public class UiMessageUpdateEventForeGround : MessageUpdateEvent { }

}
