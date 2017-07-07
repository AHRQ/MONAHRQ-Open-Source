using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using System.ComponentModel.DataAnnotations;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Reports.Converters
{
    /// <summary>
    /// Custom converter class for Filter type display name.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class FilterTypeDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            var enumValue = Enum.Parse(typeof(ReportFilterTypeEnum), value.ToString(), true) as Enum;

            if (enumValue != null) return enumValue.GetDescription();

            return string.Empty;
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
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
