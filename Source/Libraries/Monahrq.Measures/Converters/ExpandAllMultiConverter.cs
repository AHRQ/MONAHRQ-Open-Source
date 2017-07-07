using System;
using System.Globalization;
using System.Windows.Data;

namespace Monahrq.Measures.Converters
{
    // This class is for the ExpandAll checkbox which governs a group of child expander controls.
    // If the checkbox is checked (unchecked), all should be expanded (collapsed).
    // But also, if the checkbox is checked, and then the user collapses ANY of the child controls, the checkbox should switch to being unchecked.
    // This class requires 2-way Multibinding, like this:  <MultiBinding Mode="TwoWay" Converter="{StaticResource ExpandAllMultiConverter}">    
    /// <summary>
    /// This custom converter class is for the ExpandAll checkbox which governs a group of child expander controls.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class ExpandAllMultiConverter : IMultiValueConverter
    {
        // return true if ALL of the child items are expanded
        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding" /> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty" />.<see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding" />.<see cref="F:System.Windows.Data.Binding.DoNothing" /> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool allExpanded = true;
            foreach (var item in values)
            {
                if (!(bool)item)
                {
                    allExpanded = false;
                    break;
                }
            }
            return allExpanded;
        }

        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // convert the single 'IsChecked' value from the Checkbox into 4 'IsExpanded' values
            var allValues = new object[targetTypes.Length];
            for (int i = 0; i < allValues.Length; i++)
            {
                allValues[i] = value;
            }

            return allValues;
        }
    }
}