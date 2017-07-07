using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Monahrq.Websites.Converters
{
    public class ZeroFilterVisibilityConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = Int32.Parse(value.ToString());
            return count < 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
