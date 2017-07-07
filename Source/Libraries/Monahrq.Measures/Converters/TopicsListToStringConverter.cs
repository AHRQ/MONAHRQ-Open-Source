using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Measures.ViewModels;

namespace Monahrq.Measures.Converters
{
    /// <summary>
    /// Converter class for converting topics list to a string
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class TopicsListToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
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
            var topicsnull = "Topics are not assigned";
            var list = value as IEnumerable<Topic>;
            if (list == null) return topicsnull;

            var tmplist = list.Select(topic => topic.Owner.Name + " > " + topic.Name).ToList();

            var result= string.Join("; ", tmplist);
            
            return string.IsNullOrEmpty(result) ? topicsnull : result;
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
