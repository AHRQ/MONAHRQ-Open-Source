using System.Collections.Generic;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using PropertyChanged;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;

namespace Monahrq.Wing.Ahrq.Model
{
    /// <summary>
    /// Class for wizard steps.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.Model.DatasetContextBase" />
    [Export, 
     PartCreationPolicy(CreationPolicy.NonShared),
     ImplementPropertyChanged]
    public class WizardContext : DatasetContextBase
    {
        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        public List<FileProgress> Files { get; set; }
        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        public string Summary { get; set; }
        /// <summary>
        /// The record key
        /// </summary>
        const string RECORD_KEY = "{488402EF-E550-4C2A-B75E-15F7A1234BCB}";          // TODO: per content type
        /// <summary>
        /// Gets or sets the type of the current import.
        /// </summary>
        /// <value>
        /// The type of the current import.
        /// </value>
        internal Target CurrentImportType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WizardContext"/> class.
        /// </summary>
        public WizardContext()
        {
            if (DatasetItem == null) DatasetItem = new Dataset();

            this[RECORD_KEY] = DatasetItem;
        }

        /// <summary>
        /// Applies the summary record.
        /// </summary>
        protected override void ApplySummaryRecord(bool mappingOnly = false) 
        {
            DatasetItem.SummaryData = Summary;
        }
    }
}
