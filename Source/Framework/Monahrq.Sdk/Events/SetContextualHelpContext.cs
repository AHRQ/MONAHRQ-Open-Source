using Microsoft.Practices.Prism.Events;

namespace Monahrq.Sdk.Events
{
    /// <summary>
    /// The Set Contextual Help Context Event
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.String}" />
    public class SetContextualHelpContextEvent : CompositePresentationEvent<string> { }
    /// <summary>
    /// The Open Contextual Help Content Event
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.String}" />
    public class OpenContextualHelpContextEvent : CompositePresentationEvent<string> { }
}