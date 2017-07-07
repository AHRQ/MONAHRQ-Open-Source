using Microsoft.Practices.Prism.Events;

namespace Monahrq.Sdk.Events
{

    /// <summary>
    /// The Notification Type Enum.
    /// </summary>
    public enum ENotificationType
	{
        /// <summary>
        /// The information
        /// </summary>
        Info,
        /// <summary>
        /// The error
        /// </summary>
        Error,
        /// <summary>
        /// The warning
        /// </summary>
        Warning
    }

    /// <summary>
    /// The Notification Message object used with the <see cref="GenericNotificationExEvent"/>
    /// </summary>
    public class NotificationMessage
	{
        /// <summary>
        /// Gets or sets the notification message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
        /// <summary>
        /// Gets or sets the type of the notification.
        /// </summary>
        /// <value>
        /// The type of the notification.
        /// </value>
        public ENotificationType NotificationType { get; set; }
	}

    /// <summary>
    /// The Generic Notification Event
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.String}" />
    public class GenericNotificationEvent : CompositePresentationEvent<string> { }
    /// <summary>
    /// The Generic Notification Ex Event
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Sdk.Events.NotificationMessage}" />
    public class GenericNotificationExEvent : CompositePresentationEvent<NotificationMessage> { }
}
