namespace Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records
{
    /// <summary>
    /// The dataset part version record.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecord" />
    public abstract class DatasetPartVersionRecord : DatasetRecord
    {
        /// <summary>
        /// Gets or sets the dataset version record.
        /// </summary>
        /// <value>
        /// The dataset version record.
        /// </value>
        public virtual DatasetVersionRecord DatasetVersionRecord { get; set; }
    }
}
