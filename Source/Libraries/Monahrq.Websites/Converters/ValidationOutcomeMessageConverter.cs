using System;
using System.Windows;
using System.Windows.Data;
using Monahrq.Sdk.Regions;
using Monahrq.Websites.ViewModels.Publish;

namespace Monahrq.Websites.Converters
{
    public class ValidationOutcomeMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is ValidationOutcome)) return "UNKNOWN ERROR";
            ValidationOutcome outcome = (ValidationOutcome)value;
            return outcome == ValidationOutcome.StaleBaseData
            ? "CONTACT MONAHRQ"
               : "Click here for more information";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }




}
