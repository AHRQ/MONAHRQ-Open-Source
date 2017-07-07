using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Monahrq.DataSets.Services;

namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// Given a NamedConnectionElement (e.g. any Oledb source), builds and maintains a data stage of stagesize rows.
    /// </summary>
    public class ImportStageModel
    {
        const int DefaultStageSize = 5;

        /// <summary>
        /// Gets or sets the provider factory.
        /// </summary>
        /// <value>
        /// The provider factory.
        /// </value>
        DataproviderFactory ProviderFactory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportStageModel"/> class.
        /// </summary>
        /// <param name="providerFactory">The provider factory.</param>
        /// <param name="stageSize">Size of the stage.</param>
        public ImportStageModel(DataproviderFactory providerFactory, int stageSize = DefaultStageSize)
        {
            this.ProviderFactory = providerFactory;
            this.StageSize = stageSize;
            DataTable = new Lazy<DataTable>(CreateTable);
        }

        /// <summary>
        /// Gets the column values.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="displayDashes">if set to <c>true</c> [display dashes].</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Column name not found.;columnName</exception>
        public List<string> GetColumnValues(string columnName, bool displayDashes = false)
        {
            var col = DataTable.Value.Columns[columnName];
            if (col == null)
            {
                throw new ArgumentException("Column name not found.", "columnName");
            }

            var result = new List<string>();

            var cnt = Math.Min(StageSize, DataTable.Value.Rows.Count);

            for (int i = 0; i < cnt; i++)
            {
                var row = DataTable.Value.Rows[i];

                var rowValue = row[col].ToString();

                if (displayDashes && (string.IsNullOrEmpty((rowValue ?? string.Empty).Trim())))
                {
                    rowValue = "  -  ";
                }
                
                result.Add(rowValue);
            }

            // fill up the remaining slots requested
            cnt = StageSize - result.Count;
            for (int i = 0; i < cnt; i++)
            {
                result.Add(!displayDashes ? string.Empty : "  -  ");
            }

            return result;
        }

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <returns></returns>
        DataTable CreateTable()
        {
            var services = ProviderFactory.Services;
            var connection = services.ConnectionFactory();
            var controller = ProviderFactory.Services.Controller;
            return controller.SelectTable(connection.ConnectionString, services.Configuration.SelectFrom, StageSize);
        }

        /// <summary>
        /// Gets or sets the data table.
        /// </summary>
        /// <value>
        /// The data table.
        /// </value>
        Lazy<DataTable> DataTable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the source fields.
        /// </summary>
        /// <value>
        /// The source fields.
        /// </value>
        public IEnumerable<SourceField> SourceFields
        {
            get
            {
                return DataTable.Value.Columns.OfType<DataColumn>().Select(col => new SourceField { Name = col.ColumnName, Order = col.Ordinal});
            }
        }

        /// <summary>
        /// Gets the size of the stage.
        /// </summary>
        /// <value>
        /// The size of the stage.
        /// </value>
        public int StageSize 
        { 
            get; 
            private set;
        }

        /// <summary>
        /// Gets or sets the select from.
        /// </summary>
        /// <value>
        /// The select from.
        /// </value>
        public string SelectFrom { get; set; }
    }

    /// <summary>
    /// The source field entity.
    /// </summary>
    public class SourceField
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }
    }
}
