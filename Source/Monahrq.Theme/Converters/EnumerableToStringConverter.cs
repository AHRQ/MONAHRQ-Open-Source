using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// Converts enum to a string value.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class EnumerableToStringConverter : IValueConverter
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
            //Edited this to be generic so that it will take any enumerable string. 
            var list = value as IEnumerable<string>;
            return list == null ? null : String.Join(", ", list);
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
