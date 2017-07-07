using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Reports.Converters
{

    public class AudienceEnumDescriptionToStringConverter : IValueConverter
    {
        // The Reports_Report.Audiences column in SQL in nvarchar, but it holds the names of the numeric fields so they can be converted automatically
        // to enum fields when queried. What we need to display in the UI datagrid is the DescriptionAttribute of the enum fields.

        /// <summary>
        ///To get the description of the audiences.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var audience = (uint)value;
            string result = "";

            var audiences = value as IList<Audience>;

            if (audiences == null) return result;

            result = audiences.OrderBy(a => a.ToString()).ToList()
                                                         .Aggregate(result, (current, audience) => current + (EnumExtensions.GetEnumFieldDescription(audience, audience.ToString()) + ", "));

            if (result.EndsWith(", ")) 
                result = result.SubStrBeforeLast(", ");

            return result;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }




    /// <summary>
    /// Custom converter class.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ReportOriginConverter : IValueConverter
    {
        public readonly string Custom = @"Custom";
        public readonly string Template = @"Default template";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isDefault = value is bool && (bool) value;
            return string.Format("{0}", isDefault ? Template : Custom);
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
