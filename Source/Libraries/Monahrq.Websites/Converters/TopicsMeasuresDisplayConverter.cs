using Monahrq.Infrastructure.Entities.Domain.Measures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Monahrq.Websites.Converters
{
   public class TopicsMeasuresDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Measure measure = value as Measure;
            if (measure == null)
                return "";
            var topicsNames=measure.Topics.Select(t => t.Owner + " > " + t.Name);
            return string.Join(", ", topicsNames);

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
