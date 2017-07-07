using Microsoft.Practices.Prism.Commands;
using Monahrq.DataSets.Model;
using System;
using System.Windows.Input;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using PropertyChanged;

namespace Monahrq.DataSets.ViewModels.Validation
{
    /// <summary>
    /// The record state enumeration.
    /// </summary>
    public enum RecordState
    {
        /// <summary>
        /// The success
        /// </summary>
        Success,
        /// <summary>
        /// The critical error
        /// </summary>
        CriticalError,
        /// <summary>
        /// The validation error
        /// </summary>
        ValidationError,
        /// <summary>
        /// The warning
        /// </summary>
        Warning,
        /// <summary>
        /// The excluded by crosswalk
        /// </summary>
        ExcludedByCrosswalk
    }

    /// <summary>
    /// The dataset import validation result viewmodel.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.ViewModels.Validation.IValidationResultViewModel" />
    [Serializable]
    [ImplementPropertyChanged]
    public class ValidationResultViewModel : IValidationResultViewModel
    {
        /// <summary>
        /// Gets the transaction record.
        /// </summary>
        /// <value>
        /// The transaction record.
        /// </value>
        public DatasetTransactionRecord TransactionRecord { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResultViewModel"/> class.
        /// </summary>
        /// <param name="importRecord">The import record.</param>
        public ValidationResultViewModel(Dataset importRecord)
        {
            TransactionRecord = new DatasetTransactionRecord { Dataset = importRecord };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResultViewModel"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public ValidationResultViewModel(DatasetTransactionRecord source)
        {
            TransactionRecord = source;
        }

        /// <summary>
        /// Gets or sets the record number.
        /// </summary>
        /// <value>
        /// The record number.
        /// </value>
        public int RecordNumber
        {
            get
            {
                return TransactionRecord.Extension;
            }

            set
            {
                TransactionRecord.Extension = value;
            }

        }
        /// <summary>
        /// Gets or sets the name of the element.
        /// </summary>
        /// <value>
        /// The name of the element.
        /// </value>
        public String ElementName
        {
            get
            {
                return TransactionRecord.Data;
            }

            set
            {
                TransactionRecord.Data = value;
            }

        }
        /// <summary>
        /// Gets or sets the type of the result.
        /// </summary>
        /// <value>
        /// The type of the result.
        /// </value>
        public RecordState ResultType
        {
            get
            {
                return (RecordState)TransactionRecord.Code;
            }

            set
            {
                TransactionRecord.Code = (int)value;
            }

        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message
        {
            get
            {
                return TransactionRecord.Message;
            }

            set
            {
                TransactionRecord.Message = value;
            }
        }
    }

    /// <summary>
    /// The dataset import wizard validation result summary viewmodel.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.ViewModels.Validation.IValidationResultsSummary" />
    public class ValidationResultsSummary : IValidationResultsSummary
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="ValidationResultsSummary"/> class from being created.
        /// </summary>
        private ValidationResultsSummary()
        {
            ExportMappings = new DelegateCommand(() => ExportMappingsRequested(this, EventArgs.Empty));
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResultsSummary"/> class.
        /// </summary>
        /// <param name="numberInserted">The number inserted.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <param name="context">The context.</param>
        public ValidationResultsSummary(int numberInserted, int totalRecords, DatasetContext context) : this()
        {
            if (context != null)
            {
                context.ValidationSummary = this;
            }
            CountValid = numberInserted;
            Total = totalRecords;
        }

        /// <summary>
        /// Gets the count valid.
        /// </summary>
        /// <value>
        /// The count valid.
        /// </value>
        public int CountValid
        {
            get;
            private set;
        }

        /// <summary>
        /// The count invalid
        /// </summary>
        private int _countInvalid;
        /// <summary>
        /// Gets or sets the count invalid.
        /// </summary>
        /// <value>
        /// The count invalid.
        /// </value>
        public int CountInvalid
        {
            get { return Total - CountValid; }
            set { _countInvalid = value; }
        }

        /// <summary>
        /// Gets the total.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        public int Total
        {
            get;
            private set;
        }

        /// <summary>
        /// Occurs when [export mappings requested].
        /// </summary>
        public event EventHandler ExportMappingsRequested = delegate { };

        /// <summary>
        /// Gets the export mappings.
        /// </summary>
        /// <value>
        /// The export mappings.
        /// </value>
        public ICommand ExportMappings { get; private set; }
    }
}
