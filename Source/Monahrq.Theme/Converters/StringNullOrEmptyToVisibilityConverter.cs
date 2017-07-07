using System;
using System.Windows.Data;
using System.Windows;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// Returns visibility based on a string being null or empty.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class StringNullOrEmptyToVisibilityConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      //  Cast the data.
      if (value is string == false)
        return Visibility.Collapsed;

      string str = value as string;

      return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
