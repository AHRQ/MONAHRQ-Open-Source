using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Websites.ViewModels;

namespace Monahrq.Websites.Converters
{
    public class WebsiteMeasureTopicSelectabilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var topic = value as TopicViewModel;

            if (topic == null || topic.TopicCategory.TopicType == TopicTypeEnum.NursingHome) return false;

            return true;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}