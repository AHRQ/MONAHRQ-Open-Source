using Monahrq.Infrastructure.Entities.Domain.Measures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using Monahrq.Measures.ViewModels;

namespace Monahrq.Measures.Converters
{
    /// <summary>
    /// Facts visibility class which determines whether to show(Visible) or hide(Collapsed) the facts
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class FactsVisbility : IValueConverter
    {
        /// <summary>
        /// Converts the specified values.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetTypes">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use..</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns></returns>
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return Visibility.Collapsed;

            var viewModel = values as TopicViewModel;
            if (viewModel != null && viewModel.IsNew && !viewModel.IsUserCreated) return Visibility.Visible;

            return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts the value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetTypes">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use..</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
