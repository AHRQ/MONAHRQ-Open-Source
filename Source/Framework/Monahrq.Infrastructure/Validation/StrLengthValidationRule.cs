using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Iesi.Collections;

namespace Monahrq.Infrastructure.Validation
{
    public class StringRangeRule : ValidationRuleEx
    {
        public int? Min { get; set; }
        public int? Max { get; set; }
        public string PropertyName { get; set; }

        public string MissingMsg
        {
            get;
            set;
        }
      
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
			var strValue = (string)GetBoundValue(value) ?? string.Empty;

            if (Min.HasValue &&
                strValue.Length < Min.Value)
            {
                    if(Min.Value == 1) // special number for not required fields
                        return new ValidationResult(false, MissingMsg??string.Format("{0} Can not be empty", PropertyName));

                    return new ValidationResult(false, string.Format("Please enter '{0}' using more than {1} characters", PropertyName,Min.Value));
            }
            if (Max.HasValue &&
               strValue.Length > Max.Value)
                    return new ValidationResult(false, string.Format("Please enter '{0}' using fewer than {1} characters", PropertyName,Max.Value));
                

          return new  ValidationResult(true, null);
            
        }
    }

    public class RequiredRule : ValidationRule
    {
        //public int? Min { get; set; }
        //public int? Max { get; set; }
        public string PropertyName { get; set; }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null || value.ToString() == string.Empty)
            {
                return new ValidationResult(false, string.Format("A valid {0} is required field.", PropertyName));
            }

            return new ValidationResult(true, null);
        }
    }
}
