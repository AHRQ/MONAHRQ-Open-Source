using System;
using Monahrq.Infrastructure.Data.Conventions;

namespace Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records
{
    /// <summary>
    /// The datset transaction record.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecord" />
    [Serializable, EntityTableName("Wings_DatasetTransactionRecords")]
    public class DatasetTransactionRecord: DatasetRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetTransactionRecord"/> class.
        /// </summary>
        public DatasetTransactionRecord()
        {
                
        }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public virtual int Code { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public virtual string Message { get; set; }
        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>
        /// The extension.
        /// </value>
        public virtual int Extension { get; set; }
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [StringLengthMax]
        public virtual string Data { get; set; }
    }
}
