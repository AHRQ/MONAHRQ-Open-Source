using System.Windows.Data;
using Monahrq.Infrastructure.Domain.Websites;

namespace Monahrq.Websites.Converters
{
    public class TrendingYearConverter : IValueConverter
    {

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var item = value as TrendingYear;

            return item != null ? item.IsDefault ? item.Year + " (Default)" : item.Year : string.Empty;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}

