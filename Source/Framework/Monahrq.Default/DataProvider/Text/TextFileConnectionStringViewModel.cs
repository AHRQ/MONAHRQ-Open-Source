using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Monahrq.Default.DataProvider.Text
{
    /// <summary>
    /// Class for text file
    /// </summary>
    [Export]
    [ImplementPropertyChanged]
    public class TextFileConnectionStringViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance has header.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has header; otherwise, <c>false</c>.
        /// </value>
        public bool HasHeader { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance has double quotes.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has double quotes; otherwise, <c>false</c>.
        /// </value>
        public bool HasDoubleQuotes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFileConnectionStringViewModel"/> class.
        /// </summary>
        public TextFileConnectionStringViewModel()
        {
            // both options are probably on for most files, so defaults = true
            HasHeader = true;
            HasDoubleQuotes = true;
        }

        /// <summary>
        /// Loads the specified builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        internal void Load(DbConnectionStringBuilder builder)
        {
            // There are no table names inside csv files, the "tables" are the files in the folder.
        }
    }
}
