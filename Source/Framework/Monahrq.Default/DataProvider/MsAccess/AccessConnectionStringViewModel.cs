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

namespace Monahrq.Default.DataProvider.MsAccess
{
    /// <summary>
    /// Class to load the tables from a ms-access datasource
    /// </summary>
    [Export]
    [ImplementPropertyChanged]
    public class AccessConnectionStringViewModel
    {
        /// <summary>
        /// Gets or sets the tables.
        /// </summary>
        /// <value>
        /// The tables.
        /// </value>
        public ListCollectionView Tables { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessConnectionStringViewModel"/> class.
        /// </summary>
        public AccessConnectionStringViewModel()
        {
            Tables = new ListCollectionView(new List<string>());
        }

        /// <summary>
        /// Loads the tables from a data source.
        /// </summary>
        /// <param name="builder">The builder.</param>
        internal void Load(DbConnectionStringBuilder builder)
        {
            using (var con = new OleDbConnection(builder.ConnectionString))
            {
                con.Open();
                var tables = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                
                // filter out Microsoft system tables and temporary objects
                Tables = new ListCollectionView(tables.Rows.OfType<DataRow>()
                    .Select(row => row["TABLE_NAME"].ToString())
                    .Where(tbl => !tbl.StartsWith("MSys") && !tbl.StartsWith("~"))
                    .ToList());
            }
        }
    }
}
