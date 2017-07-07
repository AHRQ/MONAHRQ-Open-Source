using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Websites.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Monahrq.Websites.Converters
{
    public class WebsiteMeasuresDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Measure measure = value as Measure;
            if (measure == null)
                return "";
            var WebsiteDataService = ServiceLocator.Current.GetInstance<IWebsiteDataService>();
            var websiteNames=WebsiteDataService.GetWebsiteNamesForMeasure(measure.Id);

            return string.Join(", ", websiteNames);
             
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
