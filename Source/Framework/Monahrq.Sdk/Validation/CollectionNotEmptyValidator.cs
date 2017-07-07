using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Monahrq.Sdk.Validation
{
    /// <summary>
    /// Validated if a collection is empty or null. 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.ValidationRule" />
    public class CollectionNotEmptyValidator : ValidationRule
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// When overridden in a derived class, performs validation checks on a value.
        /// </summary>
        /// <param name="value">The value from the binding target to check.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>
        /// A <see cref="T:System.Windows.Controls.ValidationResult" /> object.
        /// </returns>
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            ValidationResult lResult = null;

            IEnumerable<object> lCollection = (IEnumerable<object>)value;
            if (lCollection == null || lCollection.Count() == 0)
            {
                lResult = new ValidationResult(false, ErrorMessage);
            }
            else
            {
                lResult = new ValidationResult(true, null);
            }

            return lResult;
        }
    }
}
