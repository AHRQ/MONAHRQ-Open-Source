using System.Globalization;
using System.Windows.Data;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// Converts a string to a 'money' string.  Uses string formatting :C0.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class CurrencyConverter : IValueConverter
    {
		#region IValueConverter Members

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
		public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            var result = value as decimal? ?? 0;

            return System.String.Format(CultureInfo.CurrentUICulture, "{0:C}", result);
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
		/// <exception cref="System.NotImplementedException"></exception>
		public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}