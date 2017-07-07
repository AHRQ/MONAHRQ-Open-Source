using System;
using System.Collections.Generic;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.FieldStorage;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Domain.Entity{System.Int32}" />
    [Serializable, EntityTableName("Wings_Datasets")]
    public class Dataset : Entity<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Dataset"/> class.
        /// </summary>
        public Dataset()
        {
            Versions = new List<DatasetVersionRecord>();
            Infoset = new Infoset();
        }

        #region Properties
        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        /// <value>
        /// The type of the content.
        /// </value>
        public virtual Target ContentType { get; set; }
        /// <summary>
        /// Gets or sets the versions.
        /// </summary>
        /// <value>
        /// The versions.
        /// </value>
        public virtual IList<DatasetVersionRecord> Versions { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [StringLengthMax]
        public virtual string SummaryData { get { return Infoset.Data; } set { Infoset.Data = value; } }
        /// <summary>
        /// Gets or sets the infoset.
        /// </summary>
        /// <value>
        /// The infoset.
        /// </value>
        public virtual Infoset Infoset { get; protected set; }
        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        public virtual string File { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public virtual string Description { get; set; }
        /// <summary>
        /// Gets or sets the date imported.
        /// </summary>
        /// <value>
        /// The date imported.
        /// </value>
        public virtual DateTime? DateImported { get; set; }
        /// <summary>
        /// Gets or sets the reporting quarter.
        /// </summary>
        /// <value>
        /// The reporting quarter.
        /// </value>
        public virtual string ReportingQuarter { get; set; }
        /// <summary>
        /// Gets or sets the reporting year.
        /// </summary>
        /// <value>
        /// The reporting year.
        /// </value>
        public virtual string ReportingYear { get; set; }
        /// <summary>
        /// Gets or sets the Version month.
        /// </summary>
        /// <value>
        /// The DRGMDC mapping status message.
        /// </value>
        public virtual string VersionMonth { get; set; }
        /// <summary>
        /// Gets or sets the Version year.
        /// </summary>
        /// <value>
        /// The DRGMDC mapping status message.
        /// </value>
        public virtual string VersionYear { get; set; }

        /// <summary>
        /// The Version of the reporting process required by the dataset.
        /// </summary>
        /// <value>
        /// The report process version.
        /// </value>
        public virtual string ReportProcessVersion { get; set; }

        /// <summary>
        /// Gets or sets the DRGMDC mapping status message.
        /// </summary>
        /// <value>
        /// The DRGMDC mapping status message.
        /// </value>
        public virtual string DRGMDCMappingStatusMessage { get; set; }
        /// <summary>
        /// Gets or sets the DRGMDC mapping status.
        /// </summary>
        /// <value>
        /// The DRGMDC mapping status.
        /// </value>
        public virtual DrgMdcMappingStatusEnum DRGMDCMappingStatus { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [is finished].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is finished]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsFinished { get; set; }

        /// <summary>
        /// Gets or sets the total rows.
        /// </summary>
        /// <value>
        /// The total rows.
        /// </value>
        public virtual long? TotalRows { get; set; }

        /// <summary>
        /// Gets or sets the rows imported.
        /// </summary>
        /// <value>
        /// The rows imported.
        /// </value>
        public virtual long? RowsImported { get; set; }

        /// <summary>
        /// Gets or sets the number of rows imported as string.
        /// </summary>
        /// <value>
        /// The number of rows imported as string.
        /// </value>
        public string NumberOfRowsImportedAsString
        {
            get
            {
                return /*TotalRows.HasValue &&*/ RowsImported.HasValue && RowsImported > 0 ? /*string.Format("{0} of {1}", RowsImported, TotalRows)*/ RowsImported.Value.ToString() : null;
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name
        {
            get { return File; }
            set { File = value; }
        }

        /// <summary>
        /// Gets or sets wheather to Use Real time provider via generated website or non-real time provider data imported in MONAHRQ.
        /// </summary>
        /// <value>
        /// The UseRealtime flag for providers (physicians).
        /// </value>
        public bool UseRealtimeData { get; set; }
        /// <summary>
        /// Gets or sets the selected provider states to retrieve the physicians from the data.medicare.gov api.
        /// </summary>
        /// <value>
        /// The selected provider states.
        /// </value>
        public string ProviderStates { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is re import.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is re import; otherwise, <c>false</c>.
        /// </value>
        public bool IsReImport { get { return IsFinished && IsPersisted;} }


        #endregion

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="options">The options.</param>
        public void Accept(ITargetDeletionVisitor visitor, VisitorOptions options)
        {
            if (!string.IsNullOrEmpty(visitor.TargetType) && !visitor.TargetType.EqualsIgnoreCase(this.ContentType.Name))
                return;

            visitor.Visit(this, options);
        }
    }
}