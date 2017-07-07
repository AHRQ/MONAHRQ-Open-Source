using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Validation
{
    /// <summary>
    /// Wraps another <see cref="ValidationAttribute"/> to receive special handling by <see cref="InstanceValidator"/>
    /// </summary>
    public abstract class WarningValidationAttribute : ValidationAttribute
    {
        protected abstract ValidationAttribute CreateSourceAttribute();

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var attr = CreateSourceAttribute();
            var validationResults = new List<ValidationResult>();
            if (Validator.TryValidateValue(value, validationContext, validationResults, new[] { attr }))
            {
                return ValidationResult.Success;
            }
            if (validationResults.Any())
            {
                return validationResults.FirstOrDefault();
            }

            var ctxt = new ValidationContext(validationContext.ObjectInstance);
            ctxt.DisplayName = ctxt.ObjectInstance == null ? "unknown" : ctxt.ObjectInstance.GetType().Name;
            return new ValidationResult(string.Format("Validation failed on {0}", ctxt.DisplayName));
        }
    }
}
