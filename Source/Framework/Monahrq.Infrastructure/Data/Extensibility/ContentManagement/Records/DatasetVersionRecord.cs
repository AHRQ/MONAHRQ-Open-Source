using System;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records
{
    /// <summary>
    /// The dataset version record.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Domain.Entity{System.Int32}" />
    [Serializable, EntityTableName("Wings_DatasetVersions")]
    public class DatasetVersionRecord : Entity<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetVersionRecord"/> class.
        /// </summary>
        public DatasetVersionRecord()
        {
        }

        /// <summary>
        /// Gets or sets the dataset.
        /// </summary>
        /// <value>
        /// The dataset.
        /// </value>
        public virtual Dataset Dataset { get; set; }
        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        /// <value>
        /// The number.
        /// </value>
        public virtual int Number { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DatasetVersionRecord"/> is published.
        /// </summary>
        /// <value>
        ///   <c>true</c> if published; otherwise, <c>false</c>.
        /// </value>
        public virtual bool Published { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DatasetVersionRecord"/> is latest.
        /// </summary>
        /// <value>
        ///   <c>true</c> if latest; otherwise, <c>false</c>.
        /// </value>
        public virtual bool Latest { get; set; }
    }
}