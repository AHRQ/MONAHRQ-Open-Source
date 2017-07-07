using System;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media ;
namespace Monahrq.Websites.Converters
{
    class TextToFontFamilyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string fonts = value as string;
            if (null == fonts)
                return "Courier New";
            return fonts.Split(new[] {','}).First();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
