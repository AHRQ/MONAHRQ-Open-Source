using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.DataSets.ViewModels.Import
{
    /// <summary>
    /// the dataset import result view model.
    /// </summary>
    public class ImportResultViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportResultViewModel"/> class.
        /// </summary>
        /// <param name="recordNumber">The record number.</param>
        /// <param name="ex">The ex.</param>
        public ImportResultViewModel(int recordNumber, Exception ex)
        {
            RecordNumber = recordNumber;
            Error = ex;
        }

        /// <summary>
        /// Gets the record number.
        /// </summary>
        /// <value>
        /// The record number.
        /// </value>
        public int RecordNumber { get; private set; }
        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public Exception Error { get; private set; }
    }
}
