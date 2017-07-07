using System;
using System.Windows.Data;
using System.Windows;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// Compares 2 enums.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class EnumToBooleanConverter : IValueConverter
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
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return value.Equals(parameter);
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
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        //return value.Equals(false) ? DependencyProperty.UnsetValue : parameter;
        return value.Equals(true) ? parameter : Binding.DoNothing;
    }
  }
}
