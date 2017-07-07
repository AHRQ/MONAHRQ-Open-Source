using Microsoft.Practices.Prism.Events;

namespace Monahrq.Sdk.Events
{
    /// <summary>
    /// The Navigate Datasets Event
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Sdk.Events.Empty}" />
    public class NavigateDatasetsEvent : CompositePresentationEvent<Empty> { }
    /// <summary>
    /// The Navigate Hospitals and Regions Event
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Sdk.Events.Empty}" />
    public class NavigateHospitalsAndRegionsEvent : CompositePresentationEvent<Empty> { }    
}
