using Monahrq.Sdk.DataProvider;
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

namespace Monahrq.Default.DataProvider.MsExcel
{
    /// <summary>
    /// class for excel data source
    /// </summary>
    [Export]
    [ImplementPropertyChanged]
    public class ExcelConnectionStringViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance has header.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has header; otherwise, <c>false</c>.
        /// </value>
        public bool HasHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the tables(sheets) in the excel.
        /// </summary>
        /// <value>
        /// The tables.
        /// </value>
        public ListCollectionView Tables { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelConnectionStringViewModel"/> class.
        /// </summary>
        public ExcelConnectionStringViewModel()
        {
            Tables = new ListCollectionView(new List<string>());
        }

        /// <summary>
        ///Loads the excel table(sheet) names
        /// </summary>
        /// <param name="builder">The builder.</param>
        internal void Load(DbConnectionStringBuilder builder)
        {
            using(var con = new OleDbConnection(builder.ConnectionString))
            {
                con.Open();
                var tables = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                Tables = new ListCollectionView(tables.Rows.OfType<DataRow>().Select(row=>row["TABLE_NAME"].ToString()).ToList());
            }
        }
    }
}
