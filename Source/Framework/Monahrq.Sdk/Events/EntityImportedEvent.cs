using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Sdk.Events
{
    /// <summary>
    /// The entity imported event
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{T}" />
    public class EntityImportedEvent<T> :  CompositePresentationEvent<T> 
        where T: IEntity 
    {
    }
}
