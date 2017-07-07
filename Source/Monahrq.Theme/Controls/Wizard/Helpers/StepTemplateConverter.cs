using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Monahrq.Theme.Controls.Wizard.Models;

namespace Monahrq.Theme.Controls.Wizard.Helpers
{

	/// <summary>
	/// Used by the main wizard view (WizardView.xaml).
	/// When a step changes, Convert is called and passed the current CompleteStep object.  This then passed back the view for that step as a DataTemplate.
	/// </summary>
	/// <seealso cref="System.Windows.Markup.MarkupExtension" />
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class StepTemplateConverter : MarkupExtension, IValueConverter
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
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            var viewType = ( (IProvideViewType)value ).ViewType;
            return new DataTemplate( /*type passed in here does not have anything to do with DataContext*/ ) { VisualTree = new FrameworkElementFactory( viewType ) };
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
		/// <exception cref="InvalidOperationException"></exception>
		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new InvalidOperationException( string.Format( "{0} can only be used OneWay.", this.GetType().Name ) );
        }

		/// <summary>
		/// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension.
		/// </summary>
		/// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
		/// <returns>
		/// The object value to set on the property where the extension is applied.
		/// </returns>
		public override object ProvideValue( IServiceProvider serviceProvider )
        {
            return this;
        }
    }

}
