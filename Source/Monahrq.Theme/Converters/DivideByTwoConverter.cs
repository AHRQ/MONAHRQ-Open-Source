using System;
using System.Windows.Data;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// Converts a number x to half x
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class DivideByTwoConverter : IValueConverter
  {
		#region IValueConverter Members

		/// <summary>
		/// Converts the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="targetType">Type of the target.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="culture">The culture.</param>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value != null && value is double)
      {
          return (parameter != null && parameter is string && (string) parameter == "Negative")
                     ? -((double) value)/2
                     : ((double) value)/2;
      }
      return 0.0;
    }

		/// <summary>
		/// Converts the back.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="targetType">Type of the target.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="culture">The culture.</param>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
