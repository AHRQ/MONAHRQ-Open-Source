using System;
using System.ComponentModel.DataAnnotations;

namespace Monahrq.Infrastructure.Validation
{
    /// <summary>
    /// Verifies that the given value can be parsed as a double
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NumericAttribute : DataTypeAttribute
    {
        public NumericAttribute()
            : base("numeric")
        {
        }

        public override string FormatErrorMessage(string name)
        {
            if (this.ErrorMessage == null && this.ErrorMessageResourceName == null)
            {
                this.ErrorMessage = string.Format("{0} needs to be a valid numeric value.", name);
            }

            return base.FormatErrorMessage(name);
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;

            double retNum;

            return double.TryParse(Convert.ToString(value), out retNum);
        }
    }
}