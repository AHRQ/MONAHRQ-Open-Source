using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Monahrq.Theme.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        private static readonly BoolToVisibilityConverter defaultInstance = new BoolToVisibilityConverter();

        public static BoolToVisibilityConverter Default { get { return defaultInstance; } }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var isVisible = value is bool && (bool) value;
            bool isInverse=false;

            if (parameter != null)
            {
               isInverse = (parameter as string).ToUpper().Equals("INVERSE"); /*INVERSE MEANS IF ITS TRUE RETURN COLLAPESED, IF FALSE RETURN VISIBLE*/
            }
         

            isVisible = isVisible && !isInverse;

            var result=isVisible ? Visibility.Visible : Visibility.Collapsed;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
