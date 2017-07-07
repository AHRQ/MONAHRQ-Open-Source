using System;
using System.Windows.Input;
namespace Monahrq.DataSets.ViewModels.Validation
{
    /// <summary>
    /// the validation results summary.
    /// </summary>
    public interface IValidationResultsSummary
    {
        /// <summary>
        /// Gets the count invalid.
        /// </summary>
        /// <value>
        /// The count invalid.
        /// </value>
        int CountInvalid { get; }
        /// <summary>
        /// Gets the count valid.
        /// </summary>
        /// <value>
        /// The count valid.
        /// </value>
        int CountValid { get; }
        /// <summary>
        /// Gets the total.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        int Total { get; }
        /// <summary>
        /// Occurs when [export mappings requested].
        /// </summary>
        event EventHandler ExportMappingsRequested;
        /// <summary>
        /// Gets the export mappings.
        /// </summary>
        /// <value>
        /// The export mappings.
        /// </value>
        ICommand ExportMappings { get; }
    }
}
