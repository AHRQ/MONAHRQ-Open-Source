using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using Monahrq.Infrastructure.Data.Transformers;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensibility.Exports;
using NHibernate.Util;

namespace Monahrq.Infrastructure.Services.Dynamic
{
    /// <summary>
    /// The dynamic open source csv template strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Extensibility.Exports.BaseCsvTemplateExportStrategy" />
    [Export(typeof(DynamicCsvTemplateStrategy))]
    public sealed class DynamicCsvTemplateStrategy : BaseCsvTemplateExportStrategy
    {
        /// <summary>
        /// The data columns
        /// </summary>
        private IList<DataColumn> _dataColumns;

        /// <summary>
        /// Gets or sets the dynamic target.
        /// </summary>
        /// <value>
        /// The dynamic target.
        /// </value>
        public Target DynamicTarget { get; set; }

        /// <summary>
        /// Initializes the Template export strategy.
        /// </summary>
        /// <exception cref="InvalidOperationException">Please make sure to set the DynamicTarget property so that the code can execute.</exception>
        protected override void Initialize()
        {
            if(DynamicTarget == null) 
                throw new InvalidOperationException("Please make sure to set the DynamicTarget property so that the code can execute.");


            DataTable dt = new DataTable(DynamicTarget.DbSchemaName);
            using (var session = base.DataProvider.SessionFactory.OpenStatelessSession())
            {
                dt = session.CreateSQLQuery(string.Format("select top 0 * from {0}", DynamicTarget.DbSchemaName))
                                      .SetResultTransformer(new DataTableResultTransformer())
                                      .UniqueResult<DataTable>();
            }

            if (dt != null && dt.Columns.Any())
                _dataColumns = dt.Columns.OfType<DataColumn>().ToList();
        }

        /// <summary>
        /// Gets the column definitions.
        /// </summary>
        /// <returns></returns>
        protected override IList<TemplateColumnDefinition> GetColumnDefinitions()
        {
            return _dataColumns.Select(dc => new TemplateColumnDefinition
                                                    {
                                                        Name = dc.ColumnName,
                                                        DataType = dc.DataType,
                                                        IsReguired = !dc.AllowDBNull
                                                    }).ToList();
        }

        /// <summary>
        /// Gets the template directory path.
        /// </summary>
        /// <value>
        /// The template directory path.
        /// </value>
        protected override string TemplateDirectoryPath
        {
            get { return Path.Combine(MonahrqContext.MyDocumentsApplicationDirPath, DynamicTarget.WingTargetXmlFilePath); }
        }

        /// <summary>
        /// Gets the template file prefix.
        /// </summary>
        /// <value>
        /// The template file prefix.
        /// </value>
        protected override string TemplateFilePrefix
        {
            get { return string.Format("{0} v{1}", DynamicTarget.Name, DynamicTarget.Version.Number.Replace(".","_")); }
        }

        /// <summary>
        /// Generates the sample data.
        /// </summary>
        /// <returns></returns>
        protected override IList<TemplateRow> GenerateSampleData()
        {
            var rowData = new List<TemplateRow>();

            // TODO: Add functionality to dynamically populate row data.

            return rowData;
        }
    }
}
