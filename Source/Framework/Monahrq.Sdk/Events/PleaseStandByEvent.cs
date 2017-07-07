using System;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Services;

namespace Monahrq.Sdk.Events
{
    /// <summary>
    /// The Please Stand By Event Payload object
    /// </summary>
    public class PleaseStandByEventPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PleaseStandByEventPayload"/> class.
        /// </summary>
        public PleaseStandByEventPayload() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PleaseStandByEventPayload"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public PleaseStandByEventPayload(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
    }

    /// <summary>
    /// The Please Stand By Event
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Sdk.Events.PleaseStandByEventPayload}" />
    public class PleaseStandByEvent : CompositePresentationEvent<PleaseStandByEventPayload> { }

    /// <summary>
    /// The Please Stand By with Custom Message Event
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.String}" />
    public class PleaseStandByMessageUpdateEvent : CompositePresentationEvent<string> { }

    /// <summary>
    /// The Resume Normal Processing Event
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Sdk.Events.Empty}" />
    public class ResumeNormalProcessingEvent : CompositePresentationEvent<Empty> { }

    /// <summary>
    /// Please Stand By Wrapper class.
    /// </summary>
    public static class PleaseStandByWrapper
    {
        /// <summary>
        /// Executes the a specified action and then trigers the please stand by event with a custom message if provided.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="action">The action.</param>
        public static void Execute(string message, Action action)
        {
            var events = ServiceLocator.Current.GetInstance<IEventAggregator>();

            events.GetEvent<PleaseStandByEvent>().Publish(new PleaseStandByEventPayload(message));
            try
            {
                action();
            }
            catch (Exception ex)
            {
                events.GetEvent<ErrorNotificationEvent>().Publish(ex);
            }
            finally
            {
                events.GetEvent<ResumeNormalProcessingEvent>().Publish(Empty.Value);
            }
        }
    }
}