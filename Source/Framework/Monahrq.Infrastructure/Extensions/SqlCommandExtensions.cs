using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Extensions
{
	public static class SqlCommandExtensions
	{


		public static String DeclareValueForSQL(this SqlParameter sp,int index=0)
		{
			String retval = "";


			switch (sp.SqlDbType)
			{
				case SqlDbType.Structured:
					retval = ConvertDataTableToString(
						sp.Value as DataTable,
						String.Format("dt{0}", index.ToString()));
					break;
				default:
					if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
					{
						retval += ("\n\ndeclare " + sp.ParameterName + "\t" + sp.SqlDbType.ToString() + "\t= ");
						retval += (((sp.Direction == ParameterDirection.Output) ? "null" : sp.ParameterValueForSQL()) + ";");
					}
					break;
			}
			return retval;

		}
		public static String ParameterValueForSQL(this SqlParameter sp,int index=0)
		{
			String retval = "";

			switch (sp.SqlDbType)
			{
				case SqlDbType.Char:
				case SqlDbType.NChar:
				case SqlDbType.NText:
				case SqlDbType.NVarChar:
				case SqlDbType.Text:
				case SqlDbType.Time:
				case SqlDbType.VarChar:
				case SqlDbType.Xml:
				case SqlDbType.Date:
				case SqlDbType.DateTime:
				case SqlDbType.DateTime2:
				case SqlDbType.DateTimeOffset:
					retval = "'" + sp.Value.ToString().Replace("'", "''") + "'";
					break;

				case SqlDbType.Bit:
					retval = (sp.Value.ToBooleanOrDefault(false)) ? "1" : "0";
					break;

				case SqlDbType.Structured:	// ??????
					retval = String.Format("dt{0}", index.ToString());
					break;
				default:
					retval = sp.Value.ToString().Replace("'", "''");
					break;
			}

			return retval;
		}

		public static String CommandAsSql(this SqlCommand sc)
		{
			try
			{
				StringBuilder sql = new StringBuilder();
				Boolean FirstParam = true;

				sql.AppendLine("use " + sc.Connection.Database + ";");
				switch (sc.CommandType)
				{
					case CommandType.StoredProcedure:
						sql.AppendLine("declare @return_value int;");

						int paramIndex = 0;
						foreach (SqlParameter sp in sc.Parameters)
						{
							sql.Append(sp.DeclareValueForSQL(paramIndex));
							paramIndex++;
						}

						sql.AppendLine("\n\nexec [" + sc.CommandText + "]");

						paramIndex = 0;
						foreach (SqlParameter sp in sc.Parameters)
						{
							if (sp.Direction != ParameterDirection.ReturnValue)
							{
								sql.Append((FirstParam) ? "\t" : "\t, ");

								if (FirstParam) FirstParam = false;

								if (sp.Direction == ParameterDirection.Input)
									sql.AppendLine(sp.ParameterName + " = " + sp.ParameterValueForSQL(paramIndex));
								else

									sql.AppendLine(sp.ParameterName + " = " + sp.ParameterName + " output");
							}
							paramIndex++;
						}
						sql.AppendLine(";");

						sql.AppendLine("select 'Return Value' = convert(varchar, @return_value);");

						foreach (SqlParameter sp in sc.Parameters)
						{
							if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
							{
								sql.AppendLine("select '" + sp.ParameterName + "' = convert(varchar, " + sp.ParameterName + ");");
							}
						}
						break;
					case CommandType.Text:
						sql.AppendLine(sc.CommandText);
						break;
				}

				return sql.ToString();
			}
			catch (Exception ex)
			{
				return "";
			}
		}


		private static string ConvertDataTableToString(DataTable dataTable, String tableName)
		{
			var output = new StringBuilder();

			var columnsWidths = new int[dataTable.Columns.Count];

			// Get Max Column widths from Data.
			foreach (DataRow row in dataTable.Rows)
			{
				for (int i = 0; i < dataTable.Columns.Count; i++)
				{
					var length = row[i].ToString().Length;
					if (columnsWidths[i] < length)
						columnsWidths[i] = length;
				}
			}

			// Get Max Column widths from Headers.
			for (int i = 0; i < dataTable.Columns.Count; i++)
			{
				var length = dataTable.Columns[i].ColumnName.Length;
				if (columnsWidths[i] < length)
					columnsWidths[i] = length;
			}

			//	Create Table.
			output.Append( String.Format("\ndeclare @{0} {1};",tableName,dataTable.TableName) );

			//	Create Insert (Header) Section.
			String insertHeader = String.Format("insert @{0}(",tableName);
			for (int index = 0; index < dataTable.Columns.Count; index++)
			{
				if (index < dataTable.Columns.Count-1)
					insertHeader += dataTable.Columns[index].ColumnName + ",";
				else
					insertHeader += dataTable.Columns[index].ColumnName + ")";
			}

			//	Create Insert (Value) Section.
			foreach (DataRow row in dataTable.Rows)
			{
				output.Append("\n" + insertHeader + " values(");

				{
					for (int index = 0; index < dataTable.Columns.Count; index++)
					{
						if (index < dataTable.Columns.Count - 1)
							output.Append(PadCenter(row[index].ToString(), columnsWidths[index] + 2) + ",");
						else
							output.Append(PadCenter(row[index].ToString(), columnsWidths[index] + 2) + ")");
					}
				}
			}
			return output.ToString();
		}
		private static string PadCenter(string text, int maxLength)
		{
			int diff = maxLength - text.Length;
			return new string(' ', diff / 2) + text + new string(' ', (int)(diff / 2.0 + 0.5));

		}

		public static Boolean ToBooleanOrDefault(this String s, Boolean Default)
		{
			return ToBooleanOrDefault((Object)s, Default);
		}
		public static Boolean ToBooleanOrDefault(this Object o, Boolean Default)
		{
			Boolean ReturnVal = Default;
			try
			{
				if (o != null)
				{
					switch (o.ToString().ToLower())
					{
						case "yes":
						case "true":
						case "ok":
						case "y":
							ReturnVal = true;
							break;
						case "no":
						case "false":
						case "n":
							ReturnVal = false;
							break;
						default:
							ReturnVal = Boolean.Parse(o.ToString());
							break;
					}
				}
			}
			catch
			{
			}
			return ReturnVal;
		}
	}
}
