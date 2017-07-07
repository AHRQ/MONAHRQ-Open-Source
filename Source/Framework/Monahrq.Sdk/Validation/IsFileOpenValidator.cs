using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;

using Monahrq.Infrastructure.Validation;

namespace Monahrq.Sdk.Validation
{
    /// <summary>
    /// Validates if a file is already open and locked. If so, then throws a validation message.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Validation.ValidationRuleEx" />
    public class IsFileOpenValidator : ValidationRuleEx
    {
        /// <summary>
        /// The is valid result
        /// </summary>
        private readonly ValidationResult _isValidResult = new ValidationResult(true, null);
        /// <summary>
        /// The validation message format
        /// </summary>
        private const string VALIDATION_MESSAGE_FORMAT = "{0} is already open in another program. Please close file first before importing.";

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; set; }

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
            try
            {
                if (value == null || value.ToString() == "") return _isValidResult;

                var file = new FileInfo(value.ToString());

                if (file.Exists && IsFileLocked(file))
                {
                    return new ValidationResult(false, string.Format(VALIDATION_MESSAGE_FORMAT, file.Name));
                }
                else
                    return _isValidResult;
            }
            catch (Exception)
            {

                return _isValidResult;
            }
        }

        /// <summary>
        /// Determines whether [is file locked] [the specified file].
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>
        ///   <c>true</c> if [is file locked] [the specified file]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    //stream.Close();
                    stream.Dispose();
                }
                    

            }

            //file is not locked
            return false;
        }
    }
}
