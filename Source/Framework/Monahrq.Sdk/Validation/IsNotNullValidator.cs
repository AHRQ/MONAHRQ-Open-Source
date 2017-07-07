using System;
using System.Globalization;
using System.Windows.Controls;

namespace Monahrq.Sdk.Validation
{
    /// <summary>
    /// Checks to ensure that a field is not null. If so, then returns a false validation result.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.ValidationRule" />
    public class IsNotNullValidator : ValidationRule
    {
        /// <summary>
        /// The field name
        /// </summary>
        private String _fieldName;

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public String FieldName
        {
            get { return _fieldName; }
            set { _fieldName = value; }
        }

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
            if (value == null)
                return new ValidationResult(false, string.Format("{0} cannot be empty.", FieldName));
            return new ValidationResult(true, "valid");
        }
    }
}
