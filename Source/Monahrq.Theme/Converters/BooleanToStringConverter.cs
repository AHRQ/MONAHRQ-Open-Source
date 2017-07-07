using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Monahrq.Theme.Converters
{
    public class BooleanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null) return null;
            if (parameter.ToString().IndexOf("|") < 0) return null;

            //if parameter is not configured property with pipeline delimiter(s) than return null
            var returnValues = parameter.ToString().Split('|');
            if (returnValues.Count() <= 0) return null;

            //if input value is null.. 
            if (value == null || value.ToString() == string.Empty)
            {
                //..and converter parameter is configured for null returns return the null replacment..
                if (returnValues.Count() == 3) return returnValues[2];

                //..otherwise return null
                else return null;
            }

            bool boolValue = false;
            try 
            { 
                boolValue = System.Convert.ToBoolean(value.ToString()); 
            }
            catch (Exception) 
            { 
                boolValue = false; 
            }

            //if input value is not null...
            //..use the first pipeline delimiter parameter for true..
            if (boolValue == true) return returnValues[0];

            //..and the second for false
            else return returnValues[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            return Binding.DoNothing;
        }


        public object ConversionParameter { get; set; }
    }
}