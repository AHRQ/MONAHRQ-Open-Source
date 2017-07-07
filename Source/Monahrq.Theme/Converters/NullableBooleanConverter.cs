using System;
using System.Globalization;
using System.Windows.Data;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// Converts nullable boolean to visibility.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	[ValueConversion(typeof(bool?), typeof(string))]
    public class NullableBooleanConverter : IValueConverter
    {
		/// <summary>
		/// The null string
		/// </summary>
		private const string NULL_STRING = "[[NullString]]";

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
            if (value == null)
            {
                return NULL_STRING;
            }

            bool parsedValue;
            return bool.TryParse(value.ToString(), out parsedValue)
                       ? (parsedValue ? bool.TrueString : bool.FalseString)
                       : NULL_STRING;
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
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            bool parsedValue;
            return bool.TryParse(value.ToString(), out parsedValue) ? (object) parsedValue : null;
        }
    }

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	[ValueConversion(typeof(int?), typeof(int))]
    public class NullableIntToNegativeOneConverter : IValueConverter
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
            return value ?? -1;
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
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int i;

            if (value == null)
            {
                return null;
            }

            if (int.TryParse(value.ToString(), out i))
            {
                return i == -1 ? null : value;
            }

            return value;
        }

    }
}