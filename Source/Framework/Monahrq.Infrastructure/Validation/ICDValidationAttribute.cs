using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Monahrq.Infrastructure.Extensions;
using System.Diagnostics;

namespace Monahrq.Infrastructure.Validation
{
    /// <summary>
    /// Validates ICD9 and ICD10 codes in any property matching the name patterns "Primary[Diagnosis|Procedure]", "Principal[Diagnosis|Procedure]", and "[Diagnosis|Procedure]*"
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ICDValidationAttribute : ValidationAttribute
    {
        protected List<string> PropertyNameMatchs { get; set; }
        protected bool PerformProcedureCheck { get; set; }
        List<PropertyInfo> Properties { get; set; }
        //public Regex ICDDiagnosisRegExPattern { get; set; }
        //public Regex ICDProcedureRegExPattern { get; set; }

        //private Regex _icd9RegEx;
        //private Regex _icd10RegEx;
        //private Regex _icd10ProcedureRegEx;

        /// <summary>
        /// Initializes a new instance of the <see cref="ICDValidationAttribute"/> class.
        /// </summary>
        /// <param name="typetoValidate">The type to validate.</param>
        /// <param name="performProcedureCheck">if set to <c>true</c> validate procedure properties as well.</param>
        public ICDValidationAttribute(Type typetoValidate, bool performProcedureCheck)
        {
            Properties = new List<PropertyInfo>();
            PropertyNameMatchs = new List<string> { "Diagnosis", "Procedure" };
            PerformProcedureCheck = performProcedureCheck;
            ValidationHelper.InstanceType = typetoValidate;

            Properties.AddRange(GetTypeProperties());

            //_icd10RegEx = new Regex(DatasetRecord.ICD10__DIAGNOSTICCODE_REGEX, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            //_icd9RegEx = new Regex(DatasetRecord.ICD9_CODE_REGEX, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            //_icd10ProcedureRegEx = new Regex(DatasetRecord.ICD10_PROCEDURECODE_REGEX, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

            // ICDDiagnosisRegExPattern = new Regex(icdDiagnosisRegExPattern);
            // ICDProcedureRegExPattern = !string.IsNullOrEmpty(icdProcedureRegExPattern) ? new Regex(icdProcedureRegExPattern) : null;
        }

        public override bool RequiresValidationContext
        {
            get
            {
                return true;
            }
        }

        private IList<PropertyInfo> GetTypeProperties()
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            foreach (var nameMatch in PropertyNameMatchs)
            {
                properties.AddRange(ValidationHelper.InstanceType.GetProperties()
                    .Where(
                        propertyInfo =>
                            (propertyInfo.Name.Equals("Principal" + nameMatch,
                                 StringComparison.InvariantCultureIgnoreCase) ||
                             propertyInfo.Name.Equals("Primary" + nameMatch, StringComparison.InvariantCultureIgnoreCase) ||
                             propertyInfo.Name.StartsWith(nameMatch, StringComparison.InvariantCultureIgnoreCase)))
                    .Distinct()
                    .ToList());
            }

            return properties;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // try
            // {
            var invalidProperties = new List<string>();
            var instance = value ?? validationContext.ObjectInstance;
#if DEBUG
            var timer = new Stopwatch();
            timer.Start();
#endif
            if (!Properties.Any())
            {
                Properties.AddRange(GetTypeProperties());
            }
#if DEBUG
            timer.Stop();
            var message = string.Format("IP/ED dataset imports - Time of Execution get all diagnosis and procedure codes for dataset row: {0}:{1}",
                                        timer.Elapsed.Seconds, timer.Elapsed.Milliseconds);

            Trace.WriteLine(message);
            timer.Reset();
#endif

            if (!Properties.Any())
            {
                return ValidationResult.Success;
            }
#if DEBUG
            timer.Start();
#endif
            var diagnosisValues = Properties.Where(propertyInfo => propertyInfo.Name.ContainsIgnoreCase("Diagnosis") && propertyInfo.GetValue(instance, null) != null)
                                                   .Select(propertyInfo => new KeyValuePair<string, object>(propertyInfo.Name, propertyInfo.GetValue(instance, null))).ToList();

            var isIcd9DiagnosisCodesValid = diagnosisValues.All(code =>
            {
                if (ICD9DiagnosticCheck(code.Value.ToString())) return true;
                // if (SharedRegularExpressions.ICD9Regex.IsMatch(code.Value.ToString())) return true;

                invalidProperties.Add(code.Key);
                return false;
            });
            var isIcd10DiagnosisCodesValid = !isIcd9DiagnosisCodesValid && diagnosisValues.All(code =>
            {
                if (ICD10DiagnosticCheck(code.Value.ToString())) return true;
                // if (SharedRegularExpressions.ICD10Regex.IsMatch(code.Value.ToString())) return true;
                invalidProperties.Add(code.Key);
                return false;
            });
#if DEBUG
            timer.Stop();
            message = string.Format("IP/ED dataset imports - Time of Execution process all diagnosis codes: {0}:{1}",
                                    timer.Elapsed.Seconds, timer.Elapsed.Milliseconds);

            Trace.WriteLine(message);
            timer.Reset();
#endif
            bool isIcd10ProcedureCodeValid = false, isIcd9ProcedureCodeValid = false;
            if (PerformProcedureCheck)
            {
#if DEBUG
                timer.Start();
#endif
                var procedureValues = Properties
                    .Where(propertyInfo => propertyInfo.Name.ContainsIgnoreCase("Procedure") && propertyInfo.GetValue(instance, null) != null)
                    .Select(propertyInfo => new KeyValuePair<string, object>(propertyInfo.Name, propertyInfo.GetValue(instance, null)))
                    .ToList();

                isIcd9ProcedureCodeValid = !procedureValues.Any() || procedureValues.All(code =>
                {
                    if (ICD9ProcedureCheck(code.Value.ToString())) return true;
                    // if (SharedRegularExpressions.ICD9ProcedureRegex.IsMatch(code.Value.ToString())) return true;

                    invalidProperties.Add(code.Key);
                    return false;
                });
                isIcd10ProcedureCodeValid = !procedureValues.Any() || (!isIcd9ProcedureCodeValid && procedureValues.All(
                    code =>
                    {
                        if (ICD10ProcedureCheck(code.Value.ToString())) return true;
                        // if (SharedRegularExpressions.ICD10ProcedureRegex.IsMatch(code.Value.ToString())) return true;

                        invalidProperties.Add(code.Key);
                        return false;
                    }));
#if DEBUG
                timer.Stop();
                message = string.Format("IP/ED dataset imports - Time of Execution process all procedure codes: {0}:{1}",
                                        timer.Elapsed.Seconds, timer.Elapsed.Milliseconds);

                Trace.WriteLine(message);
                timer.Reset();
#endif
            }

            if (PerformProcedureCheck)
            {
                if (isIcd9DiagnosisCodesValid && isIcd9ProcedureCodeValid)
                    return ValidationResult.Success;

                if (isIcd10DiagnosisCodesValid && isIcd10ProcedureCodeValid)
                    return ValidationResult.Success;
            }
            else
            {
                if (isIcd9DiagnosisCodesValid)
                    return ValidationResult.Success;

                if (isIcd10DiagnosisCodesValid)
                    return ValidationResult.Success;
            }

            //string errorMessage = GetFormattedMessage(PerformProcedureCheck, this._neededProperties, validationContext)
            return new ValidationResult("Record deleted because it either contains a mix of ICD-9 and ICD-10 coded data or invalid codes.", /*invalidProperties.Any() ? invalidProperties.Distinct().Select(p => p).ToList() :*/ new List<string>()); //ValidationHelper.IcdCheckProperties.Distinct().Select(p => p.Name).ToList()
            // }
            // finally
            // {

            // }
        }

        private bool ICD9DiagnosticCheck(string stringToCheck)
        {
            if (string.IsNullOrEmpty(stringToCheck)) return true;

            stringToCheck = stringToCheck.Trim();

            if (!SharedRegularExpressions.NoSpecialCharactersRegex.IsMatch(stringToCheck)) return false;

            if (stringToCheck.Length < 3) return false;
            if (stringToCheck.Length > 5) return false;

            var firstMember = stringToCheck.Substring(0,1);

            if (!firstMember.IsNumeric())
            {
                if (!firstMember.ContainsAny("e", "E", "v", "V"))
                    return false;
            } 

            var remainingMembers = stringToCheck.Substring(1, stringToCheck.Length - 1);
            if (!remainingMembers.IsNumeric()) return false;

            return true;
        }

        private bool ICD9ProcedureCheck(string stringToCheck)
        {
            if (string.IsNullOrEmpty(stringToCheck)) return true;

            stringToCheck = stringToCheck.Trim();

            if (!SharedRegularExpressions.NoSpecialCharactersRegex.IsMatch(stringToCheck)) return false;

            if (stringToCheck.Length < 3) return false;
            if (stringToCheck.Length > 4) return false;

            if (!stringToCheck.IsNumeric()) return false;

            return true;
        }

        private bool ICD10DiagnosticCheck(string stringToCheck)
        {
            if (string.IsNullOrEmpty(stringToCheck)) return true;

            stringToCheck = stringToCheck.Trim();

            if (!SharedRegularExpressions.NoSpecialCharactersRegex.IsMatch(stringToCheck)) return false;

            if (stringToCheck.Length < 3) return false;
            if (stringToCheck.Length > 7) return false;

            var firstMember = stringToCheck.Substring(0, 1);
            if (firstMember.IsNumeric()) return false;

            var secondMember = stringToCheck.Substring(1, 1);
            if (!secondMember.IsNumeric()) return false;

            return true;
        }

        private bool ICD10ProcedureCheck(string stringToCheck)
        {
            if (string.IsNullOrEmpty(stringToCheck)) return true;

            stringToCheck = stringToCheck.Trim();

            if (!SharedRegularExpressions.NoSpecialCharactersRegex.IsMatch(stringToCheck)) return false;

            return stringToCheck.Length == 7;
        }
    }

    public static class ValidationHelper
    {
        public static List<PropertyInfo> IcdCheckProperties = new List<PropertyInfo>();
        public static Type InstanceType { get; set; }
    }
}
