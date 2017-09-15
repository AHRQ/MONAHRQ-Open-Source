using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Validation;
using Monahrq.Wing.Discharge.Inpatient;
using Monahrq.Wing.Discharge.TreatAndRelease;

namespace Monahrq.Wing.Discharge
{
	/// <summary>
	/// Does a bulk dump of Discharge data into DB.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecordBulkInsertMapper{T}" />
	public class DischargeTargetBulkInsertMapper<T> : DatasetRecordBulkInsertMapper<T> where T : class
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="DischargeTargetBulkInsertMapper{T}"/> class.
		/// </summary>
		/// <param name="dt">The dt.</param>
		/// <param name="instance">The instance.</param>
		/// <param name="target">The target.</param>
		public DischargeTargetBulkInsertMapper(DataTable dt, T instance = null, Target target = null)
            : base(dt, instance, target)
        {
           
        }

		/// <summary>
		/// Applies the type specific column name lookup.
		/// </summary>
		protected override void ApplyTypeSpecificColumnNameLookup()
        {
            if (typeof (T) == typeof (InpatientTarget) || typeof (T) == typeof (TreatAndReleaseTarget))
            {
                var diagnosticKeys = Lookup.Keys.Where(key => key.EqualsIgnoreCase("PrincipalDiagnosis") || key.EqualsIgnoreCase("PrimaryDiagnosis") || key.StartsWith("Diagnosis")).ToList();

                var diagnosticQuery = diagnosticKeys.Where(key => Lookup.ContainsKey(key) && Lookup[key] != null).Where(key =>
                {
                    var propertyFunc = Lookup[key];
                    return propertyFunc != null && propertyFunc(Instance) != null;
                });

                // var icd10RegEx = new Regex(DatasetRecord.ICD10__DIAGNOSTICCODE_REGEX);
                // var icd9RegEx = new Regex(DatasetRecord.ICD9_CODE_REGEX);
                string primaryDiagnosticCode = "";
                var diagnosticCodesToMatch = diagnosticQuery.SelectMany(key =>
                {
                    var returnObjs = new List<string>(); 
                    var propertyFunc = Lookup[key];

                    if (propertyFunc != null)
                    {
                        var value = propertyFunc(Instance);

                        if(value != null)
                        {
                            returnObjs.Add(value.ToString());

                            if (key.EqualsIgnoreCase("PrincipalDiagnosis") || key.EqualsIgnoreCase("PrimaryDiagnosis"))
                                primaryDiagnosticCode = value.ToString();
                        }
                    }

                    return returnObjs;

                }).ToList();

                var specialDX1CodeMatch = SharedRegularExpressions.EVSpecialCodeRegex.IsMatch(primaryDiagnosticCode);
                var doAllICD9DiagnosticMatch = diagnosticCodesToMatch.All(d => SharedRegularExpressions.ICD9Regex.IsMatch(d));
                var doAllICD10DiagnosticMatch = diagnosticCodesToMatch.All(d => SharedRegularExpressions.ICD10Regex.IsMatch(d));

                doAllICD9DiagnosticMatch = specialDX1CodeMatch && doAllICD9DiagnosticMatch ? false : true;


                bool doAllICD10ProcedureMatch = false;
                bool doAllICD9ProcedureMatch = false;

                if (typeof (T) == typeof (InpatientTarget))
                {
                    var procedureKeys = Lookup.Keys.Where(key => key.EqualsIgnoreCase("PrincipalProcedure") || key.StartsWith("Procedure"))
                                                   .ToList();
                    var procedureQuery = procedureKeys.Where(key => Lookup.ContainsKey(key) && Lookup[key] != null).Where(key =>
                    {
                        var propertyFunc = Lookup[key];
                        return propertyFunc != null && propertyFunc(Instance) != null;
                    });

                    var procedureCodesToMatch = procedureQuery.SelectMany(key =>
                    {
                        var returnObjs = new List<string>();
                        var propertyFunc = Lookup[key];

                        if (propertyFunc == null) return returnObjs;

                        var value = propertyFunc(Instance);

                        if (value != null)
                            returnObjs.Add(value.ToString());

                        return returnObjs;

                    }).ToList();
                    doAllICD9ProcedureMatch = procedureCodesToMatch.All(d => SharedRegularExpressions.ICD9ProcedureRegex.IsMatch(d));
                    doAllICD10ProcedureMatch = procedureCodesToMatch.All(d => SharedRegularExpressions.ICD10ProcedureRegex.IsMatch(d));
                }

                if (typeof (T) == typeof (InpatientTarget))
                {
                    if (doAllICD9DiagnosticMatch && doAllICD9ProcedureMatch)
                        Lookup["ICDCodeType"] = t => ICDCodeTypeEnum.ICD9;
                    else if (doAllICD10DiagnosticMatch && doAllICD10ProcedureMatch)
                        Lookup["ICDCodeType"] = t => ICDCodeTypeEnum.ICD10;
                    else
                        Lookup["ICDCodeType"] = t => null;
                }

                if (typeof(T) == typeof(TreatAndReleaseTarget))
                {
                    if (doAllICD9DiagnosticMatch )
                        Lookup["ICDCodeType"] = t => ICDCodeTypeEnum.ICD9;
                    else if (doAllICD10DiagnosticMatch)
                        Lookup["ICDCodeType"] = t => ICDCodeTypeEnum.ICD10;
                    else
                        Lookup["ICDCodeType"] = t => null;
                }
            }

            base.ApplyTypeSpecificColumnNameLookup();
        }

		/// <summary>
		/// Lookups the name of the property.
		/// </summary>
		/// <param name="pi">The pi.</param>
		/// <returns></returns>
		protected override string LookupPropertyName(PropertyInfo pi)
        {
            var temp = pi.GetCustomAttribute<WingTargetElementAttribute>();
            return temp == null ? pi.Name : temp.Name;
        }
    }
}
