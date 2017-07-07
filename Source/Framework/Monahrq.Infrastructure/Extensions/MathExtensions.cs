using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Extensions
{
	static public class MathExtensions
	{
		public static bool IsBetween<T>(this T item, T start, T end, bool isEndInclusive = false)
		{
			return item != null &&
				Comparer<T>.Default.Compare(item, start) >= 0 &&
				 ((isEndInclusive && Comparer<T>.Default.Compare(item, end) <= 0) ||
					(!isEndInclusive && Comparer<T>.Default.Compare(item, end) < 0));
		}

		public static T Clamp<T>(this T num, T lowerBound, T upperBound)
		{
			if (Comparer<T>.Default.Compare(num, lowerBound) <= 0) return lowerBound;
			if (Comparer<T>.Default.Compare(num, upperBound) >= 0) return upperBound;
			return num;
		}


		#region Parsing Methods.
		//	public static bool TryParseNullable(this int _, String str, out int? result)
		//	{
		//		int temp;
		//		result = int.TryParse(str, out temp) ? (int?)temp : null;
		//		return result != null;
		//	}
		//	public static bool TryParseNullable(this double _, String str, out double? result)
		//	{
		//		double temp;
		//		result = double.TryParse(str, out temp) ? (double?)temp : null;
		//		return result != null;
		//	}
		//	
		//	public static int? TryParseNullable(this int _, String str)
		//	{
		//		int temp;
		//		return int.TryParse(str, out temp) ? (int?)temp : null;
		//	}
		//	public static double? TryParseNullable(this double _, String str)
		//	{
		//		double temp;
		//		return double.TryParse(str, out temp) ? (double?)temp : null;
		//	}

		//}
		//static public class DoubleExtensions
		//{

		public static bool TryParseNullable(String str, out int? result)
		{
			int temp;
			result = int.TryParse(str, out temp) ? (int?)temp : null;
			return result != null;
		}
		public static bool TryParseNullable(String str, out double? result)
		{
			double temp;
			result = double.TryParse(str, out temp) ? (double?)temp : null;
			return result != null;
		}

		public static int? ParseNullableInt(String str)
		{
			int temp;
			return int.TryParse(str, out temp) ? (int?)temp : null;
		}
		public static double? ParseNullableDouble(String str)
		{
			double temp;
			return double.TryParse(str, out temp) ? (double?)temp : null;
		}
		#endregion
	}
}
