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

namespace Monahrq.Infrastructure.Extensions
{
	public static class ArrayExtensions
	{
		public static List<T> ToList<T>(this T[] array)
		{
			return new List<T>(array);
		}
	}
}

