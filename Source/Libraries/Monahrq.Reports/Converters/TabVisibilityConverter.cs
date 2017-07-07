using System.Diagnostics;
using System.Windows;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using System;
using System.Windows.Data;
using Monahrq.Infrastructure.Validation;

namespace Monahrq.Reports.Converters
{
    /// <summary>
    /// Custom converter class for the Tab visibility.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class TabVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// To determine whether Tab needs to Visible or collapsed, based upon the count
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var count = (int)value;
            return count == 0 ? Visibility.Collapsed : Visibility.Visible;
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
