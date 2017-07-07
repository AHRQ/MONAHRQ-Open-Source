using System.Collections.Generic;
using Microsoft.Practices.Prism.Events;

namespace Monahrq.Sdk.Events
{
    /// <summary>
    /// The simple Import Completed Payload interface to be utilized in File import Process results.
    /// </summary>
    public interface ISimpleImportCompletedPayload
    {
        /// <summary>
        /// Gets the count inserted.
        /// </summary>
        /// <value>
        /// The count inserted.
        /// </value>
        int CountInserted { get; }
        /// <summary>
        /// Gets the number of errors.
        /// </summary>
        /// <value>
        /// The number of errors.
        /// </value>
        int NumberOfErrors { get; }
        /// <summary>
        /// Gets the error file.
        /// </summary>
        /// <value>
        /// The error file.
        /// </value>
        string ErrorFile { get; }
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        string Description { get; }
        /// <summary>
        /// Gets the inserted.
        /// </summary>
        /// <value>
        /// The inserted.
        /// </value>
        List<object> Inserted { get; }
    }

    /// <summary>
    /// The Simple Import Completed Event
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Sdk.Events.ISimpleImportCompletedPayload}" />
    public class SimpleImportCompletedEvent : CompositePresentationEvent<ISimpleImportCompletedPayload> { }
}
