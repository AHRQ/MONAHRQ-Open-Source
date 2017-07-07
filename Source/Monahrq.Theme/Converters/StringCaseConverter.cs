using System;
using System.Windows.Data;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// Changes case of a string.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class StringCaseConverter : IValueConverter
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
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      //  Cast the data.
      if (value is string == false || parameter == null)
        return null;

      var str = value as string;

      switch (parameter.ToString())
      {
          case "Upper":
              return str.ToUpper();
          case "Lower":
              return str.ToLower();
          default:
              return str;
      }
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
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
