using System;
using System.Windows.Data;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// Converts an enum into its description (defined in the Enum's Description Attribute)
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class EnumToDescriptionConverter : IValueConverter
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
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
			if (value == null || !(value is Enum))
				return "";
			else
				return ((Enum)value).GetDescription();
			
            //if (targetType.IsEnum)
            //{
            //    var enumValue = Enum.Parse(targetType, value.ToString());
            //    return EnumExtensions.GetEnumFieldDescription(enumValue, value.ToString());
            //}
            //return value.ToString();
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
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
			return value;
            //return value.Equals(false) ? DependencyProperty.UnsetValue : parameter;
            //return value.ToString().GetValueFromDescription();
            //throw new NotImplementedException();
        }
    }



	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class EnumToDescriptionConverterX : IValueConverter
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
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) return "";
			return ((Enum)value).GetDescription();
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
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value;
		}
	}
}