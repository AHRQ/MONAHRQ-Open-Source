using System;
using System.ComponentModel.DataAnnotations;

namespace Monahrq.Sdk.Validation
{
    /// <summary>
    /// Validates that a value falls between a year range from a specified year to current year
    /// </summary>
    /// <remarks>
    /// This is effectively a wrapper around <see cref="RangeAttribute"/> that optioanlly incorporates the logic of <see cref="RequiredAttribute"/>
    /// </remarks>
    /// <seealso cref="System.ComponentModel.DataAnnotations.RangeAttribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class RangeToCurrentYearAttribute : RangeAttribute
    {
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
            if(value == null & !AllowNull) return new ValidationResult(ErrorMessage);
            if (value == null & AllowNull) return ValidationResult.Success;
            return base.IsValid(value, validationContext);
        }

        /// <summary>
        /// Indicates whether null values will pass this validation test
        /// </summary>
        private bool AllowNull { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeToCurrentYearAttribute"/> class.
        /// </summary>
        /// <param name="MinYear">The minimum year.</param>
        /// <param name="allowNull">if set to <c>true</c> [allow null].</param>
        public RangeToCurrentYearAttribute(int MinYear, bool allowNull)
            : base(MinYear, DateTime.Now.Year)
        {
            AllowNull = allowNull;
        }
    }
}
