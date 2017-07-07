using Microsoft.Practices.Prism.Events;

namespace Monahrq.Events
{
    /// <summary>
    /// class for CloseSplashEvent.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Events.CloseSplashEvent}" />
    public class CloseSplashEvent : CompositePresentationEvent<CloseSplashEvent>
    {
    }

    /// <summary>
    /// Class for ShutdownEvent.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Events.ShutdownEvent}" />
    public class ShutdownEvent : CompositePresentationEvent<ShutdownEvent>
    {
    }
                
}
