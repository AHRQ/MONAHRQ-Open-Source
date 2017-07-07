using Microsoft.Practices.Prism.Events;

namespace Monahrq.Events
{
    public class DisableNavigationEvent : CompositePresentationEvent<DisableNavigationEvent>
    {

        public bool DisableUIElements { get; set; }
    }
}
