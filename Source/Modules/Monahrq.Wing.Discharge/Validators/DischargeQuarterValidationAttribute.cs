using System;
using System.ComponentModel.DataAnnotations;
using Monahrq.Wing.Discharge.Inpatient;

namespace Monahrq.Wing.Discharge.Validators
{
	/// <summary>
	/// Validates that the <see cref="InpatientTarget.DischargeQuarter"/> value is present. The actual value is not validated.
	/// </summary>
	/// <seealso cref="System.ComponentModel.DataAnnotations.ValidationAttribute" />
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class DischargeQuarterValidationAttribute : ValidationAttribute
    {
		/// <summary>
		/// Gets a value that indicates whether the attribute requires validation context.
		/// </summary>
		public override bool RequiresValidationContext { get { return true; } }

		/// <summary>
		/// Returns true if ... is valid.
		/// </summary>
		/// <param name="value">The value to validate.</param>
		/// <param name="validationContext">The context information about the validation operation.</param>
		/// <returns>
		/// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
		/// </returns>
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var target = validationContext.ObjectInstance as InpatientTarget;
            if(target == null) return ValidationResult.Success;

            if(target.DischargeQuarter != null || target.DischargeDate.HasValue)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(@"Discharge Quarter invalid");
        }

		/// <summary>
		/// Applies formatting to an error message, based on the data field where the error occurred.
		/// </summary>
		/// <param name="name">The name to include in the formatted message.</param>
		/// <returns>
		/// An instance of the formatted error message.
		/// </returns>
		public override string FormatErrorMessage(string name)
        {
            return string.Format(@"{0} requires values for either ""Discharge Quarter"" or ""Discharge Date"" or the value for ""Discharge Quarter"" was not numeric.", name);
        }
    }
}
