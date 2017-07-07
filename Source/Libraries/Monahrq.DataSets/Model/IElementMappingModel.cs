using System;
using Monahrq.Infrastructure.Entities.Events;
namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The element mapping model interface.
    /// </summary>
    public interface IElementMappingModel
    {
        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        Monahrq.Infrastructure.Entities.Domain.Wings.Element Element { get; }
        /// <summary>
        /// Occurs when [value invalid].
        /// </summary>
        event EventHandler<ExtendedEventArgs<ElementMappingValueException>> ValueInvalid;
        /// <summary>
        /// Occurs when [value evaluated].
        /// </summary>
        event EventHandler<ExtendedEventArgs<ElementMappingValueEvaluated>> ValueEvaluated;
        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        System.Reflection.PropertyInfo Property { get; }
    }
}
