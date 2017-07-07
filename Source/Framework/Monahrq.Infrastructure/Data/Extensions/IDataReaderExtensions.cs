using System;
using System.Data;

namespace Monahrq.Infrastructure.Data
{
    public static class IDataReaderExtensions
    {
        /// <summary>
        /// Guards the specified RDR.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rdr">The RDR.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static T Guard<T>(this IDataReader rdr, string field)
        {
            var ord = rdr.GetOrdinal(field.Trim());

            try
            {
                return rdr.IsDBNull(ord)
                           ? default(T)
                           : (T) Convert.ChangeType(rdr.GetValue(ord), typeof (T));
            }
            catch
            {
                return default(T);
            }

        }

        public static T Guard<T>(this IDataReader rdr, int columnIndex) 
        {
            //var ord = rdr.GetOrdinal());

            try
            {
                return rdr.IsDBNull(columnIndex)
                           ? default(T)
                           : (T)Convert.ChangeType(rdr.GetValue(columnIndex), typeof(T));
            }
            catch
            {
                return default(T);
            }

        }

        /// <summary>
        /// Columns the exists.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static bool ColumnExists(this IDataReader reader, string columnName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i) == columnName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// To the data table with one row to a DataRow.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="dataTable">The data table.</param>
        /// <returns></returns>
        public static DataRow ToDataTableRow(this IDataReader reader, DataTable dataTable)
        {
            var row = dataTable.NewRow();
            //row.BeginEdit();

            foreach (DataColumn column in dataTable.Columns)
            {
                if (reader.ColumnExists(column.ColumnName))
                    row[column.ColumnName] = reader[column.ColumnName];
            }

            //row.EndEdit();
            //row.AcceptChanges();
            
            return row;
        }
    }
}
