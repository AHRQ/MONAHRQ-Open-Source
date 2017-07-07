using System;
using System.ComponentModel;
using Monahrq.Default.DataProvider.Administration.File;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Theme.Controls.Wizard.Models.Data;


namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The Wizard step base data context.
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.Data.ModelBase" />
    public abstract class BaseDataContext:ModelBase
   {
        /// <summary>
        /// Gets or sets the quarter.
        /// </summary>
        /// <value>
        /// The quarter.
        /// </value>
        public int? Quarter { get; set; }
        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        public string Year { get; set; }
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the type of the mapping.
        /// </summary>
        /// <value>
        /// The type of the mapping.
        /// </value>
        public MappingType MappingType { get; set; }
        /// <summary>
        /// Gets or sets the selected mapping dataset identifier.
        /// </summary>
        /// <value>
        /// The selected mapping dataset identifier.
        /// </value>
        public int SelectedMappingDatasetId { get; set; }
        /// <summary>
        /// Gets or sets the select data mapping file.
        /// </summary>
        /// <value>
        /// The select data mapping file.
        /// </value>
        public string SelectDataMappingFile { get; set; }
   }

    /// <summary>
    /// The mapping type enumeration.
    /// </summary>
    [Flags]
    public enum MappingType
    {
        /// <summary>
        /// The existing data set
        /// </summary>
        [Description("Using Saved Datasets")]
        ExistingDataSet,
        /// <summary>
        /// The existing file
        /// </summary>
        [Description("Using an Exported File")]
        ExistingFile,
    }
}
