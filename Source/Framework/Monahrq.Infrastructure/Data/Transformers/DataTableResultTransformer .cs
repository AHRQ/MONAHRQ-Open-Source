using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NHibernate.Transform;

namespace Monahrq.Infrastructure.Data.Transformers
{
    public class DataTableResultTransformer : IResultTransformer
    {
        private DataTable dataTable;

        public IList TransformList(IList collection)
        {
            var rows = collection.Cast<DataRow>().ToList();
            rows.ForEach(dataRow => dataTable.Rows.Add(dataRow));
            return new List<DataTable> { dataTable };
        }

        public object TransformTuple(object[] tuple, string[] aliases)
        {
            //Create the table schema based on aliases if its not already done
            CreateDataTable(aliases);

            //Create and Fill DataRow
            return FillDataRow(tuple, aliases);
        }

        private DataRow FillDataRow(object[] tuple, string[] aliases)
        {
            DataRow dataRow = dataTable.NewRow();
            aliases.ToList().ForEach(alias =>
            {
                dataRow[alias] = tuple[Array.FindIndex(aliases, colName => colName == alias)];
            });
            return dataRow;
        }

        private void CreateDataTable(IEnumerable<string> aliases)
        {
            if (dataTable == null)
            {
                dataTable = new DataTable();
                aliases.ToList().ForEach(alias => dataTable.Columns.Add(alias));
            }
        }
    }
}
