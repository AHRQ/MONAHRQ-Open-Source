using System.ComponentModel.DataAnnotations;

namespace Monahrq.Infrastructure.Validation
{
    /// <summary>
    /// Indicates that the member is required, but validation does not fail with an error if a member is null. Implementation is provided by <see cref="RequiredAttribute"/>.
    /// </summary>
    /// <seealso cref="RequiredAttribute"/>
    public class RequiredWarningAttribute : WarningValidationAttribute
    {
        public bool AllowEmptyStrings { get; set; }
        protected override ValidationAttribute CreateSourceAttribute()
        {
            return new RequiredAttribute() { AllowEmptyStrings = this.AllowEmptyStrings };
        }
    }
}