using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Monahrq.Infrastructure.Domain.Regions;

namespace Monahrq.Theme.Converters
{

	/// <summary>
	/// Converts an enum into its description (defined in the Enum's Description Attribute)
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class DescriptionAttributeConverter : IValueConverter
    {
		/// <summary>
		/// Gets the enum description.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		private string GetEnumDescription(Type type)
        {

            var attribArray = type.GetCustomAttributes(typeof (DescriptionAttribute), true);

            if (!attribArray.Any())
            {
                return @"SELECT";
            }

            var attrib = attribArray[0] as DescriptionAttribute;
            return attrib != null ? attrib.Description : @"N/A";
        }

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
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            var type = value as Type ?? typeof(object);
            return GetEnumDescription(type);
            
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
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var description = value.ToString();
            //TODO: REMOVE HARDODED TYPES AND READ ASSEMBLY FOR MATCHING TYPES
            if (description.Contains("HRR"))
                return typeof(HealthReferralRegion);
            if (description.Contains("HSA"))
                return typeof(HospitalServiceArea);
            if (description.Contains("Custom"))
                return typeof(CustomRegion);

            return new object();
        }
    }
}
