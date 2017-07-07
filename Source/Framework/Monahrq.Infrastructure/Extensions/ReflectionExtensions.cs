using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Monahrq.Infrastructure.Extensions
{
    internal static class ReflectionExtensions
    {
        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        public static T GetAttribute<T>(this MemberInfo m) where T : Attribute
        {
            return m.GetCustomAttributes(typeof(T), false).OfType<T>().First();
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetAttributes<T>(this MemberInfo m) where T : Attribute
        {
            return m.GetCustomAttributes(typeof(T), false).OfType<T>();
        }

        /// <summary>
        /// Determines whether the specified m has attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        public static bool HasAttribute<T>(this MemberInfo m) where T : Attribute
        {
            return m.GetCustomAttributes(typeof(T), false).Any();
        }

        /// <summary>
        /// Gets the attribute or null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        public static T GetAttributeOrNull<T>(this MemberInfo m) where T : Attribute
        {
            return m.GetCustomAttributes(typeof(T), false).OfType<T>().FirstOrDefault();
        }
    }

    public static class DataReaderExtensions
    {
        public static IEnumerable<DataRow> ToDataRows(IDataReader reader)
        {
            if(reader == null || reader.IsClosed) return new List<DataRow>();

            var schemaTable = reader.GetSchemaTable();

            var rows = new List<DataRow>();
            while (reader.Read())
            {
                DataRow row = schemaTable.Rows.Add();
                foreach (DataColumn col in schemaTable.Columns)
                    row[col.ColumnName] = reader[col.ColumnName];

                rows.Add(row);
            }

            return rows;
        }
    }
}
