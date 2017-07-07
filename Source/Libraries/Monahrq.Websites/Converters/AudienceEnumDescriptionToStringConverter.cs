using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Entities.Domain.Reports;

namespace Monahrq.Websites.Converters
{
    public class AudienceEnumDescriptionToStringConverter : IValueConverter
    {
        // The Reports_Report.Audiences column in SQL in nvarchar, but it holds the names of the numeric fields so they can be converted automatically
        // to enum fields when queried. What we need to display in the UI datagrid is the DescriptionAttribute of the enum fields.
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string result = "";

            var audiences = value as IList<Audience>;

            if (audiences == null) return result;

            result = audiences.OrderBy(a => a.ToString()).ToList()
                                                         .Aggregate(result, (current, audience) => current + (EnumExtensions.GetEnumFieldDescription(audience, audience.ToString()) + ", "));

            if (result.EndsWith(", "))
                result = result.SubStrBeforeLast(", ");

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
