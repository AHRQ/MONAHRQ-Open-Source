using System.Globalization;
using System.Windows.Data;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// Converts percent and total to a percentage of total.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class PercentOfConverter : IValueConverter
	{
		#region Constructor Method.
		/// <summary>
		/// Initializes a new instance of the <see cref="PercentOfConverter"/> class.
		/// </summary>
		public PercentOfConverter()
		{
			this.percent = null;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="PercentOfConverter"/> class.
		/// </summary>
		/// <param name="total">The total.</param>
		public PercentOfConverter(double? total)
		{
			this.percent = total;
		}
		#endregion

		#region IValueConverter Members
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
		public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
		{
			double total = (double)value;
			double percent = (double?)parameter ?? this.percent ?? 0;

			if (total == null) return 0;
			if (percent == null) return 0;

			return System.Convert.ToDouble(percent) *
				   System.Convert.ToDouble(total);
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
		/// <exception cref="System.NotImplementedException"></exception>
		public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

		#endregion

		#region Variables.
		/// <summary>
		/// The percent
		/// </summary>
		private double? percent;
		#endregion
	}
}