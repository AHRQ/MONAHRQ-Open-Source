using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using SysComp = System.ComponentModel;

namespace Monahrq.Theme.Converters {
	/// <summary>
	/// Converts 'back' string to int.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class PagingPageSizeConverter : IValueConverter {
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
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            return value;
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
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            //Just return value if value object is of type int (as determined by IPagingArguments)
            if (value.GetType() == typeof(int)) return value;
            //..otherwise try to use the ToString() value instead..
            int intValue;
            if (int.TryParse(value.ToString(), out intValue)) return intValue;
            //..otherwise returns a value of zero as PageSize
            return 0;
        }
    }
}
