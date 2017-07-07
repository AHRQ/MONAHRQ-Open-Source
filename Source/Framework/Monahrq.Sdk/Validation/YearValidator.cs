using System.Globalization;
using System.Windows.Controls;

namespace Monahrq.Sdk.Validation
{
    /// <summary>
    /// Validates field value to ensure that it's a valided year. If false, then returns a failed <see cref="ValidationResult"/>.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.ValidationRule" />
    public class YearValidator : ValidationRule
    {
        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public string FieldName { get; set; }

        /// <summary>
        /// When overridden in a derived class, performs validation checks on a value.
        /// </summary>
        /// <param name="value">The value from the binding target to check.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>
        /// A <see cref="T:System.Windows.Controls.ValidationResult" /> object.
        /// </returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int year;

            if (value == null || object.Equals(value, string.Empty))
                return new ValidationResult(false, string.Format("{0} cannot be empty.", FieldName));

            int.TryParse(value.ToString(), out year);

            if (year < 1 || year > 10000)
            {
                return new ValidationResult(false, string.Format("{0} must be a valid year.", FieldName));
            }
            return new ValidationResult(true, "valid");
        }
    }
}
