using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Monahrq.Websites.ViewModels.Publish;

namespace Monahrq.Websites.Converters
{

    [ValueConversion(typeof(ValidationLevel), typeof(Image))]
    public class ValidationLevelImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ValidationLevel rate = (ValidationLevel)value;
            string imagePath = rate == ValidationLevel.Success ? string.Empty :
                rate == ValidationLevel.Error ? "../Resources/CriticalError.png"
                : "../Resources/Warning.png";
            return string.IsNullOrEmpty(imagePath) ? null : new BitmapImage(new Uri(imagePath, UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
