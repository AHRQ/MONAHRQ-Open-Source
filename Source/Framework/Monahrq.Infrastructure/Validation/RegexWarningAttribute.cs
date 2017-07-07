using System.ComponentModel.DataAnnotations;

namespace Monahrq.Infrastructure.Validation
{
    /// <summary>
    /// Verifies that the member's value matches the given <see cref="Pattern"/>, but validation does not fail with an error if the match fails.
    /// Implementation is provided by <see cref="RegularExpressionAttribute"/>.
    /// </summary>
    public class RegexWarningAttribute : WarningValidationAttribute
    {
        public string Pattern { get; private set; }

        public RegexWarningAttribute(string pattern)
        {
            this.Pattern = pattern;
        }

        protected override ValidationAttribute CreateSourceAttribute()
        {
            return new RegularExpressionAttribute(this.Pattern);
        }
    }
}