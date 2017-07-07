using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Reports.Model;

namespace Monahrq.Reports.Converters
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class AudienceToListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = value as ObservableCollection<AudienceModel>;
            return list == null ? null : string.Join(", ", list.Select(a => a.Name).ToList());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            return null;
        }
    }

    //public class ListToStringConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var list = value as IEnumerable<string>;
    //        return list == null ? null : string.Join(", ", list);
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        //throw new NotImplementedException();
    //        return null;
    //    }
    //}

    /// <summary>
    /// To get the comma delimited names.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ReportDatasetsListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = value as IEnumerable<ReportDataset>;
            //return list == null ? null : string.Join(", ", list);

            if (list == null) return null;

            var result = list.Aggregate(string.Empty, (current, reportDataset) => current + string.Format("{0},", reportDataset.Name));
            if (!string.IsNullOrEmpty(result) && result.EndsWith(","))
                result = result.SubStrBeforeLast(",");

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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // throw new NotImplementedException();
            return null;
        }
    }

    /// <summary>
    /// Customer converter class for Delete button visibility.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class DeleteReportButtonVisibility : IValueConverter
    {
        /// <summary>
        /// Returns the Visibilty enum for the Delete button.
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

            var report = value as Report;
            if (report == null) return Visibility.Visible;

            return report.IsDefaultReport
                       //||
                       //   (report.WebsitesForReportDisplay != null && report.WebsitesForReportDisplay.Count > 0)
                       ? Visibility.Collapsed
                       : Visibility.Visible;
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            return null;
        }
    }
}
