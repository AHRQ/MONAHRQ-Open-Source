using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Wing.Dynamic.Models;
using Monahrq.Wing.Dynamic.Views;
using NHibernate.Linq;

namespace Monahrq.Wing.Dynamic.ViewModels
{
    /// <summary>
    /// Import process class
    /// </summary>
    /// <seealso cref="Monahrq.Wing.Dynamic.ViewModels.SimpleProcessFileViewModel" />
    /// <seealso cref="Monahrq.Wing.Dynamic.Views.IDataImporter" />
    public class SimpleImportProcessor : SimpleProcessFileViewModel, IDataImporter
    {
        /// <summary>
        /// The header line
        /// </summary>
        public string HeaderLine = "";
        IList<Element> _dynamicElements = new List<Element>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleImportProcessor"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public SimpleImportProcessor(WizardContext context)
            : base(context)
        {
            context.InitContext();
            SetHeaderLine(context);
        }

        /// <summary>
        /// Sets the header line.
        /// </summary>
        /// <param name="context">The context.</param>
        private void SetHeaderLine(WizardContext context)
        {
            var columnQuery = string.Format(@"SELECT STUFF(
         (SELECT ',' + CONVERT(VARCHAR(50), C.COLUMN_NAME, 120)
		 FROM INFORMATION_SCHEMA.COLUMNS C
WHERE C.TABLE_NAME = '{0}' AND C.TABLE_SCHEMA='DBO' AND C.COLUMN_NAME NOT IN ('Id','Dataset_id') 
ORDER BY C.ORDINAL_POSITION
          FOR XML PATH (''))
          , 1, 1, '') AS COLUMNNAMES", context.CustomTarget.DbSchemaName);

            using (var session = context.Provider.SessionFactory.OpenSession())
            {
                HeaderLine = session.CreateSQLQuery(columnQuery)
                                    .UniqueResult<string>();

                _dynamicElements = session.Query<Target>()
                    .Where(t => t.Id == context.CustomTarget.Id)
                    .SelectMany(t => t.Elements)
                    .ToList();
            }
        }

        /// <summary>
        /// This processes a single line processing for dynamic target files
        /// </summary>
        /// <param name="inputLine">The input line.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Empty line !</exception>
        public bool LineFunction(string inputLine)
        {
            if (string.IsNullOrEmpty(inputLine)) throw new Exception("Empty line !");
            // In the main import of the rows.
            var cols = inputLine.Split(new[] {","}, StringSplitOptions.None).ToList();

            var dataRow = DynamicWingTargetDataTable.NewRow();

            
            //using (var session = DataContextObject.Provider.SessionFactory.OpenStatelessSession())
            //{
            //    session.Refresh(DataContextObject.CustomTarget);

                //foreach (var element in  DataContextObject.CustomTarget.Elements.ToList())
                foreach (var element in _dynamicElements.ToList())
                {
                    var columnIndex = element.Ordinal - 1;
                    var item = cols[columnIndex];
                    var columnValue = !element.IsRequired && string.IsNullOrEmpty(item)
                        ? DBNull.Value
                        : ConvertValues(item, element);

                    dataRow[element.Ordinal] = columnValue;

                    if (DataContextObject.DatasetItem != null)
                        dataRow["Dataset_Id"] = DataContextObject.DatasetItem.Id;
                }
            //}

            DynamicWingTargetDataTable.Rows.Add(dataRow);
            return true;
        }

        /// <summary>
        /// Converts the values.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        private static object ConvertValues(string s, Element element)
        {
            if (string.IsNullOrEmpty(s)) return DBNull.Value;

            var dbType = (DbType) Enum.Parse(typeof(DbType), element.Type.ToString(), true);

            switch (dbType)
            {
                case DbType.Boolean:
                    return Convert.ToBoolean(s);
                    case DbType.Date:
                    return Convert.ToDateTime(s).Date;
                    case DbType.DateTime:
                    return Convert.ToDateTime(s);
                    case DbType.Decimal:
                    return Convert.ToDecimal(s);
                    case DbType.Double:
                    return Convert.ToDouble(s);
                    case DbType.Guid:
                    return new Guid(s);
                    case DbType.Int16:
                    return Convert.ToInt16(s);
                    case DbType.Int32:
                    return Convert.ToInt32(s);
                    case DbType.Int64:
                    return Convert.ToInt64(s);
                    case DbType.Object:
                    return s;
                    case DbType.SByte:
                    return Convert.ToSByte(s);
                    case DbType.Single:
                    return Convert.ToSingle(s);
                default: // string
                    return s;

            }
        }
    }
}
