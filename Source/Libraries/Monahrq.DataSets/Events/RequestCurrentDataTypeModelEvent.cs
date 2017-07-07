using Microsoft.Practices.Prism.Events;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Events;

namespace Monahrq.DataSets.Events
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Events.ExtendedEventArgs{Monahrq.DataSets.Model.DataTypeModel, System.Int32?}" />
    public class RequestCurrentDataTypeModelEventArgs : ExtendedEventArgs<DataTypeModel, int?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestCurrentDataTypeModelEventArgs"/> class.
        /// </summary>
        public RequestCurrentDataTypeModelEventArgs() : base(null, null) { }
        /// <summary>
        /// Gets or sets a value indicating whether [show additional alerts].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show additional alerts]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowAdditionalAlerts { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.DataSets.Events.RequestCurrentDataTypeModelEventArgs}" />
    public class RequestCurrentDataTypeModelEvent : CompositePresentationEvent<RequestCurrentDataTypeModelEventArgs>
    {}

    /// <summary>
    /// 
    /// </summary>
    public class DeleteEntryEventArg
    {
        /// <summary>
        /// Gets or sets the dataset.
        /// </summary>
        /// <value>
        /// The dataset.
        /// </value>
        public Dataset Dataset { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [show user prompt].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show user prompt]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowUserPrompt { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.DataSets.Events.DeleteEntryEventArg}" />
    public class DeleteEntryEvent : CompositePresentationEvent<DeleteEntryEventArg> { }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.Dataset}" />
    public class UpdateEntryEvent : CompositePresentationEvent<Dataset> { }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.String}" />
    public class UpdateDrgMdsStatusEvent : CompositePresentationEvent<string> { }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.Dataset}" />
    public class ProcessDrgMdsDatasetInfoEvent : CompositePresentationEvent<Dataset> { }
}
