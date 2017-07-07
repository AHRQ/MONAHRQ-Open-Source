using System.Linq;
using System.Xml;
using Monahrq.Infrastructure.Utility;
using System.Dynamic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Data;

namespace Monahrq.Infrastructure.Extensions
{
	public static class DataRowExtensions
	{
		public static T SafeField<T>(this DataRow row, string columnName,T defaultValue=default(T))
		{
			return row.IsNull(columnName) ? defaultValue : row.Field<T>(columnName);
		}
	}
}

