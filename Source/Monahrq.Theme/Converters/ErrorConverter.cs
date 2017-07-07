using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// Converts ValidationErrors to strings.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class ErrorConverter : IValueConverter
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
            IList<ValidationError> errors = value as IList<ValidationError>;

            if (errors == null || errors.Count == 0)
                return string.Empty;

            Exception exception = errors[0].Exception;
            if (exception != null)
            {
                if (exception is TargetInvocationException)
                {
                    // It's an exception in the the model's Property setter. Get the inner exception
                    exception = exception.InnerException;
                }
                
                return exception.Message;
            }

            return errors[0].ErrorContent;
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
