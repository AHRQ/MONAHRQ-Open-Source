using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Monahrq.Theme.Converters
{
    
    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = value as IEnumerable<string>;
           
            return list == null ? "N/A" : string.Join(", ", list);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
