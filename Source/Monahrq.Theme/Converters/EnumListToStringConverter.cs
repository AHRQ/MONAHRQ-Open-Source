using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// Converts a list of enums to a string list.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class EnumListToStringConverter : IValueConverter 
    {
		/// <summary>
		/// Converts a value.
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var list = value as IList;

            if (list == null)
            {
                return "N/A";
            }

            var enumStringList = new List<string>();
            foreach (var item in list)
            {
                if (item.GetType().IsEnum)
                {
                    var enumValue = Enum.Parse(item.GetType(), item.ToString());
                    enumStringList.Add(EnumExtensions.GetEnumFieldDescription(enumValue, enumValue.ToString()));
                }
            }

            return !enumStringList.Any() ? "N/A" : string.Join(", ", enumStringList);
        }

		/// <summary>
		/// Converts a value.
		/// </summary>
		/// <param name="value">The value that is produced by the binding target.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}