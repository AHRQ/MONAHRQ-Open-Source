using System.Windows;
using System.Windows.Data;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// Converts MeasureTypes  to visibility. Nursing Housing Measure used as litmus.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class MeasureTypeVisisbility : IValueConverter
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
		public object Convert(object value, System.Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var measure = value as Measure;

            if (measure == null || (measure.GetType() != typeof(NursingHomeMeasure))) return Visibility.Visible;

            return Visibility.Collapsed;
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
		public object ConvertBack(object value, System.Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class BoundsIPMeasureTypeVisisbility : IValueConverter
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
		public object Convert(object value, System.Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value == null) return Visibility.Visible;

            if(value is Measure)
            {
                var measure = value as Measure;
                if (measure == null || !measure.Owner.Name.EqualsIgnoreCase("Inpatient Discharge")) return Visibility.Visible;

                return measure.Name.StartsWith("IP") ? Visibility.Collapsed : Visibility.Visible;
            }
            
            return Visibility.Visible;
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
		public object ConvertBack(object value, System.Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
