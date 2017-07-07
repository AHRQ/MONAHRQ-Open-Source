using System;
using System.Globalization;
using System.Windows.Data;

namespace Monahrq.Websites.Converters
{
    // This class is for the ExpandAll checkbox which governs a group of child expander controls.
    // If the checkbox is checked (unchecked), all should be expanded (collapsed).
    // But also, if the checkbox is checked, and then the user collapses ANY of the child controls, the checkbox should switch to being unchecked.
    // This class requires 2-way Multibinding, like this:  <MultiBinding Mode="TwoWay" Converter="{StaticResource ExpandAllMultiConverter}">
    public class ExpandAllMultiConverter : IMultiValueConverter
    {
        // return true if ALL of the child items are expanded
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