using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Validation;

namespace Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records
{
    /// <summary>
    /// The abstract/ base class for all first party ICD Coded Entities that follow the ICD 9 and/or ICD 10 guidelines.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecord" />
    public abstract class ICDCodedTarget : DatasetRecord
    {
        /// <summary>
        /// Gets or sets the type of the icd code.
        /// </summary>
        /// <value>
        /// The type of the icd code.
        /// </value>
        public virtual ICDCodeTypeEnum? ICDCodeType { get; set; }

        #region ICD Code Functionality

        /// <summary>
        /// Gets the property name matchs.
        /// </summary>
        /// <value>
        /// The property name matchs.
        /// </value>
        protected abstract List<string> PropertyNameMatchs { get; }
        /// <summary>
        /// Gets a value indicating whether [perform procedure check].
        /// </summary>
        /// <value>
        /// <c>true</c> if [perform procedure check]; otherwise, <c>false</c>.
        /// </value>
        protected abstract bool PerformProcedureCheck { get; }
        /// <summary>
        /// The properties
        /// </summary>
        readonly List<PropertyInfo> _properties = new List<PropertyInfo>();

        #endregion     

        /// <summary>
        /// Initializes a new instance of the <see cref="ICDCodedTarget"/> class.
        /// </summary>
        protected ICDCodedTarget()
        {
            _properties.AddRange(GetTypeProperties());
        }

        /// <summary>
        /// Gets the type properties.
        /// </summary>
        /// <returns></returns>
        private IList<PropertyInfo> GetTypeProperties()
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            foreach (var nameMatch in PropertyNameMatchs)
            {
                properties.AddRange(GetType().GetProperties()
                                             .Where(propertyInfo => (propertyInfo.Name.Equals("Principal" + nameMatch, StringComparison.InvariantCultureIgnoreCase) ||
                                                                     propertyInfo.Name.Equals("Primary" + nameMatch, StringComparison.InvariantCultureIgnoreCase) ||
                                                                     propertyInfo.Name.StartsWith(nameMatch, StringComparison.InvariantCultureIgnoreCase)))
                                             .Distinct()
                                             .ToList());
            }

            return properties;
        }

        /// <summary>
        /// Cleans the before save.
        /// </summary>
        public override void CleanBeforeSave()
        {
            var diagnosisValues = _properties.Where(propertyInfo => propertyInfo.Name.ContainsIgnoreCase("Diagnosis") && propertyInfo.GetValue(this, null) != null)
                                             .Select(propertyInfo => new KeyValuePair<string, object>(propertyInfo.Name, propertyInfo.GetValue(this, null))).ToList();

            var isIcd9DiagnosisCodesValid = diagnosisValues.All(code => SharedRegularExpressions.ICD9Regex.IsMatch(code.ToString()));
            var isIcd10DiagnosisCodesValid = !isIcd9DiagnosisCodesValid && diagnosisValues.All(code => SharedRegularExpressions.ICD10Regex.IsMatch(code.Value.ToString()));

            bool isIcd10ProcedureCodeValid = false, isIcd9ProcedureCodeValid = false;
            if (PerformProcedureCheck)
            {
                var procedureValues = _properties.Where(propertyInfo => propertyInfo.Name.ContainsIgnoreCase("Procedure") && propertyInfo.GetValue(this, null) != null)
                                                 .Select(propertyInfo => new KeyValuePair<string, object>(propertyInfo.Name, propertyInfo.GetValue(this, null))).ToList();

                isIcd9ProcedureCodeValid = !procedureValues.Any() || procedureValues.All(code => SharedRegularExpressions.ICD9Regex.IsMatch(code.Value.ToString()));
                isIcd10ProcedureCodeValid = !procedureValues.Any() || (!isIcd9ProcedureCodeValid && procedureValues.All(code => SharedRegularExpressions.ICD10ProcedureRegex.IsMatch(code.Value.ToString())));
            }

            if (PerformProcedureCheck)
            {
                if (isIcd9DiagnosisCodesValid && isIcd9ProcedureCodeValid)
                    ICDCodeType = ICDCodeTypeEnum.ICD9;

                if (isIcd10DiagnosisCodesValid && isIcd10ProcedureCodeValid)
                    ICDCodeType = ICDCodeTypeEnum.ICD10;
            }
            else
            {
                if (isIcd9DiagnosisCodesValid)
                    ICDCodeType = ICDCodeTypeEnum.ICD9;

                if (isIcd10DiagnosisCodesValid)
                    ICDCodeType = ICDCodeTypeEnum.ICD10;
            }

            base.CleanBeforeSave();
        }
    }
}
