using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// returns the string Yes if the report is not a default report, otherwise No for standard reports
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class CustomReportToStringConverter : IValueConverter
    {
		/// <summary>
		/// The no
		/// </summary>
		public readonly string No = "No";
		/// <summary>
		/// The yes
		/// </summary>
		public readonly string Yes = "Yes";

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
            var isDefault = value is bool && (bool)value;
            return isDefault ? No : Yes;
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
