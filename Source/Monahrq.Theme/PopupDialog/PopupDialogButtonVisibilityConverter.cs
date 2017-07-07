using System;
using System.Windows;
using System.Windows.Data;

namespace Monahrq.Theme.PopupDialog
{
    public class PopupDialogButtonVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var buttons = (PopupDialogButtons)value;
            var button = parameter == null ? PopupDialogButtons.None : buttons & (PopupDialogButtons)parameter;
            return button == PopupDialogButtons.None ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
