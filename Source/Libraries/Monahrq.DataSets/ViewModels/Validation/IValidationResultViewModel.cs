using System;
using System.Windows.Input;
namespace Monahrq.DataSets.ViewModels.Validation
{
    /// <summary>
    /// The dataset import wizard validation result summary interface.
    /// </summary>
    public interface IValidationResultViewModel
    {
        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        /// <value>
        /// The name of the element.
        /// </value>
        string ElementName { get; }
        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        string Message { get;  }
        /// <summary>
        /// Gets the record number.
        /// </summary>
        /// <value>
        /// The record number.
        /// </value>
        int RecordNumber { get; }
        /// <summary>
        /// Gets the type of the result.
        /// </summary>
        /// <value>
        /// The type of the result.
        /// </value>
        RecordState ResultType { get; }
     
    }
}
