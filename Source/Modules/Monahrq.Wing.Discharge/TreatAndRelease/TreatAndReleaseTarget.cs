// There are 3 aspects to "required fields"...
// 1. Required Validation Attribute... for validation reporting.
// 2. Required arg in the WingElementAttribute for enforcing the user to map the target field element 
// 3. nullability to control value assignment

using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Sdk.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Infrastructure.Validation;
using Monahrq.Sdk.Extensibility.ContentManagement.Records;
using Monahrq.Wing.Discharge.Validators;
using Monahrq.Sdk.Validation;

using sex = Monahrq.Infrastructure.Domain.Wings.Sex;
using race = Monahrq.Infrastructure.Domain.Wings.Race;
using primarypayer = Monahrq.Infrastructure.Domain.Wings.PrimaryPayer;
using hospitaltraumalevel = Monahrq.Infrastructure.Domain.Wings.HospitalTraumaLevel;
using dischargedisposition = Monahrq.Infrastructure.Domain.Wings.DischargeDisposition;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Wing.Discharge.TreatAndRelease
{
	/// <summary>
	/// The Treat and Release Data Model.
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.ICDCodedTarget" />
	[Serializable]
    [WingTarget("ED Treat And Release", "5ae6b96c-79f7-477f-bad3-31cae65383d2",
        "Mapping target for importing Emergency Treat And Release Data", false, true, 1,
        PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
        PublisherEmail = "moanhrq@ahrq.gov",
        PublisherWebsite = "http://monahrq.ahrq.gov/")]
    [RejectIfAnyPropertyHasValue(typeof(TreatAndReleaseTarget),
        sex.Exclude,
        race.Exclude,
        primarypayer.Exclude,
        hospitaltraumalevel.Exclude,
        dischargedisposition.Exclude)]
    [ICDValidation(typeof(TreatAndReleaseTarget), false)]
    [EntityTableName("Targets_TreatAndReleaseTargets")]
    public class TreatAndReleaseTarget : ICDCodedTarget
    {
		// In this regex "^[^.]{3,5}$"...
		// ^ means start of line (so the entire input must match or fail the regex expression)
		// [^.] means no dot.
		// {3,5} is min/max length.
		// $ means end of line.
		/// <summary>
		/// The dx code regex
		/// </summary>
		const string DxCodeRegex = @"^[^.]{3,7}$";
		//const string ProcedureCodeRegex    = @"^[^.]{2,4}$";

		/// <summary>
		/// Initializes a new instance of the <see cref="TreatAndReleaseTarget"/> class.
		/// </summary>
		public TreatAndReleaseTarget() : base() { }

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>
		/// The key.
		/// </value>
		[StringLength(20, MinimumLength = 0), WingTargetElement("Key", "Key", false, 1, "Sequence Number. Unique case identifier")]
        public virtual string Key { get; set; }

		/// <summary>
		/// Gets or sets the age.
		/// </summary>
		/// <value>
		/// The age.
		/// </value>
		[Range(0, 124), Required(), WingTargetElement("Age", "Age", true, 2, "Age in Years at Admission")]
        public virtual int? Age { get; set; }

		/// <summary>
		/// Gets or sets the race.
		/// </summary>
		/// <value>
		/// The race.
		/// </value>
		[WingTargetElement("Race", "Race", false, 4, "Race of Patient")]
        public virtual race? Race { get; set; }

		/// <summary>
		/// Gets or sets the sex.
		/// </summary>
		/// <value>
		/// The sex.
		/// </value>
		[Required(), WingTargetElement("Sex", "Sex", true, 5, "Sex of Patient")]
        public virtual sex? Sex { get; set; }

		/// <summary>
		/// Gets or sets the primary payer.
		/// </summary>
		/// <value>
		/// The primary payer.
		/// </value>
		[WingTargetElement("PrimaryPayer", "Primary Payer", false, 6, "Expected Primary Payer")]
        public virtual primarypayer? PrimaryPayer { get; set; }

		/// <summary>
		/// Gets or sets the patient state county code.
		/// </summary>
		/// <value>
		/// The patient state county code.
		/// </value>
		[StringLength(5, MinimumLength = 5), WingTargetElement("PatientStateCountyCode", "Patient StateCounty Code", false, 7, "County patient residence.  SSCCC where SSCCC is the FIPS state and county code.")]
        public virtual string PatientStateCountyCode { get; set; }

		/// <summary>
		/// Gets or sets the local hospital identifier.
		/// </summary>
		/// <value>
		/// The local hospital identifier.
		/// </value>
		[StringLength(12, MinimumLength = 0), Required(), WingTargetElement("LocalHospitalID", "Local Hospital ID", true, 8, "Data Source Hospital Number")]
        public virtual string LocalHospitalID { get; set; }

		/// <summary>
		/// Gets or sets the discharge disposition.
		/// </summary>
		/// <value>
		/// The discharge disposition.
		/// </value>
		[WingTargetElement("DischargeDisposition", "Discharge Disposition", true, 9, "Disposition of Patient")]
        public virtual dischargedisposition? DischargeDisposition { get; set; }

		/// <summary>
		/// Gets or sets the discharge year.
		/// </summary>
		/// <value>
		/// The discharge year.
		/// </value>
		[RangeToCurrentYear(1997, false)]
        [Required()]
        [WingTargetElement("DischargeYear", "Discharge Year", true, 22, "Year of Discharge")]
        public virtual int? DischargeYear { get; set; }

		/// <summary>
		/// Gets or sets the primary diagnosis.
		/// </summary>
		/// <value>
		/// The primary diagnosis.
		/// </value>
		[Required()]
        [RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("PrimaryDiagnosis", "Primary Diagnosis", true, 29, "ICD-9-CM Diagnosis Code. (Primary diagnosis)")]
        public virtual string PrimaryDiagnosis { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code2.
		/// </summary>
		/// <value>
		/// The diagnosis code2.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode2", "Diagnosis Code 2", false, 30, "ICD-9-CM Diagnosis Code 2. (Secondary diagnoses)")]
        public virtual string DiagnosisCode2 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code3.
		/// </summary>
		/// <value>
		/// The diagnosis code3.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode3", "Diagnosis Code 3", false, 31, "ICD-9-CM Diagnosis Code 3. (Secondary diagnoses)")]
        public virtual string DiagnosisCode3 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code4.
		/// </summary>
		/// <value>
		/// The diagnosis code4.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode4", "Diagnosis Code 4", false, 32, "ICD-9-CM Diagnosis Code 4. (Secondary diagnoses)")]
        public virtual string DiagnosisCode4 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code5.
		/// </summary>
		/// <value>
		/// The diagnosis code5.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode5", "Diagnosis Code 5", false, 33, "ICD-9-CM Diagnosis Code 5. (Secondary diagnoses)")]
        public virtual string DiagnosisCode5 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code6.
		/// </summary>
		/// <value>
		/// The diagnosis code6.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode6", "Diagnosis Code 6", false, 34, "ICD-9-CM Diagnosis Code 6. (Secondary diagnoses)")]
        public virtual string DiagnosisCode6 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code7.
		/// </summary>
		/// <value>
		/// The diagnosis code7.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode7", "Diagnosis Code 7", false, 35, "ICD-9-CM Diagnosis Code 7. (Secondary diagnoses)")]
        public virtual string DiagnosisCode7 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code8.
		/// </summary>
		/// <value>
		/// The diagnosis code8.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode8", "Diagnosis Code 8", false, 36, "ICD-9-CM Diagnosis Code 8. (Secondary diagnoses)")]
        public virtual string DiagnosisCode8 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code9.
		/// </summary>
		/// <value>
		/// The diagnosis code9.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode9", "Diagnosis Code 9", false, 37, "ICD-9-CM Diagnosis Code 9. (Secondary diagnoses)")]
        public virtual string DiagnosisCode9 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code10.
		/// </summary>
		/// <value>
		/// The diagnosis code10.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode10", "Diagnosis Code 10", false, 38, "ICD-9-CM Diagnosis Code 10. (Secondary diagnoses)")]
        public virtual string DiagnosisCode10 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code11.
		/// </summary>
		/// <value>
		/// The diagnosis code11.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode11", "Diagnosis Code 11", false, 39, "ICD-9-CM Diagnosis Code 11. (Secondary diagnoses)")]
        public virtual string DiagnosisCode11 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code12.
		/// </summary>
		/// <value>
		/// The diagnosis code12.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode12", "Diagnosis Code 12", false, 40, "ICD-9-CM Diagnosis Code 12. (Secondary diagnoses)")]
        public virtual string DiagnosisCode12 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code13.
		/// </summary>
		/// <value>
		/// The diagnosis code13.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode13", "Diagnosis Code 13", false, 41, "ICD-9-CM Diagnosis Code 13. (Secondary diagnoses)")]
        public virtual string DiagnosisCode13 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code14.
		/// </summary>
		/// <value>
		/// The diagnosis code14.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode14", "Diagnosis Code 14", false, 42, "ICD-9-CM Diagnosis Code 14. (Secondary diagnoses)")]
        public virtual string DiagnosisCode14 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code15.
		/// </summary>
		/// <value>
		/// The diagnosis code15.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode15", "Diagnosis Code 15", false, 43, "ICD-9-CM Diagnosis Code 15. (Secondary diagnoses)")]
        public virtual string DiagnosisCode15 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code16.
		/// </summary>
		/// <value>
		/// The diagnosis code16.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode16", "Diagnosis Code 16", false, 44, "ICD-9-CM Diagnosis Code 16. (Secondary diagnoses)")]
        public virtual string DiagnosisCode16 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code17.
		/// </summary>
		/// <value>
		/// The diagnosis code17.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode17", "Diagnosis Code 17", false, 45, "ICD-9-CM Diagnosis Code 17. (Secondary diagnoses)")]
        public virtual string DiagnosisCode17 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code18.
		/// </summary>
		/// <value>
		/// The diagnosis code18.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode18", "Diagnosis Code 18", false, 46, "ICD-9-CM Diagnosis Code 18. (Secondary diagnoses)")]
        public virtual string DiagnosisCode18 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code19.
		/// </summary>
		/// <value>
		/// The diagnosis code19.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode19", "Diagnosis Code 19", false, 47, "ICD-9-CM Diagnosis Code 19. (Secondary diagnoses)")]
        public virtual string DiagnosisCode19 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code20.
		/// </summary>
		/// <value>
		/// The diagnosis code20.
		/// </value>
		[RegularExpression(DxCodeRegex, ErrorMessage = "Must consist of 3-7 characters and digits with no decimal point."), WingTargetElement("DiagnosisCode20", "Diagnosis Code 20", false, 48, "ICD-9-CM Diagnosis Code 20. (Secondary diagnoses)")]
        public virtual string DiagnosisCode20 { get; set; }

		/// <summary>
		/// Gets or sets the hospital trauma level.
		/// </summary>
		/// <value>
		/// The hospital trauma level.
		/// </value>
		[WingTargetElement("HospitalTraumaLevel", "Hospital Trauma Level", false, 49, "Indicates the trauma level of the hospital and is based on information from the Trauma Information Exchange Program database")]
        public virtual hospitaltraumalevel? HospitalTraumaLevel { get; set; }

		/// <summary>
		/// Gets or sets the numberof diagnoses.
		/// </summary>
		/// <value>
		/// The numberof diagnoses.
		/// </value>
		[Range(0, 35), WingTargetElement("NumberofDiagnoses", "Number of Diagnoses ", false, 50, "Number of Diagnoses")]
        public virtual int? NumberofDiagnoses { get; set; }

		/// <summary>
		/// Gets or sets the discharge quarter.
		/// </summary>
		/// <value>
		/// The discharge quarter.
		/// </value>
		[Range(1, 4, ErrorMessage = @"Value must be numeric (1, 2, 3, 4) with no leading alpha characters.")] //Numeric, 
        [DischargeQuarterValidation]
        [WingTargetElement("DischargeQuarter", "Discharge Quarter", false, 14, "Quarter of Discharge (1 = Jan-Mar, 2=Apr-Jun, 3=Jul-Sep, 4=Oct-Dec) Discharge Date must be set if Discharge Quarter is not.")]
        public virtual object DischargeQuarter { get; set; }

		//public virtual ICDCodeTypeEnum? ICDCodeType { get; set; }

		/// <summary>
		/// Creates the bulk insert mapper.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dataTable">The data table.</param>
		/// <param name="instance">The instance.</param>
		/// <param name="target">The target.</param>
		/// <returns></returns>
		public override IBulkMapper CreateBulkInsertMapper<T>(DataTable dataTable, T instance = default(T), Infrastructure.Entities.Domain.Wings.Target target = null)
        {
            return new DatasetRecordBulkInsertMapper<T>(dataTable, instance, Dataset.ContentType);
        }

		/// <summary>
		/// Gets the property name matchs.
		/// </summary>
		/// <value>
		/// The property name matchs.
		/// </value>
		protected override List<string> PropertyNameMatchs { get { return new List<string> { "Diagnosis" }; } }
		/// <summary>
		/// Gets a value indicating whether [perform procedure check].
		/// </summary>
		/// <value>
		/// <c>true</c> if [perform procedure check]; otherwise, <c>false</c>.
		/// </value>
		protected override bool PerformProcedureCheck { get { return false; } }
    }

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecordMap{Monahrq.Wing.Discharge.TreatAndRelease.TreatAndReleaseTarget}" />
	public class TreatAndReleaseTargetMap : DatasetRecordMap<TreatAndReleaseTarget>
    {
		/// <summary>
		/// The NHibernate mapping The Treat and Release Data Model.
		/// Initializes a new instance of the <see cref="TreatAndReleaseTargetMap"/> class.
		/// </summary>
		public TreatAndReleaseTargetMap()
        {
            var indexPrefix = string.Format("IDX_{0}", typeof(TreatAndReleaseTarget).Name);
            Map(m => m.Key).Length(20);
            Map(m => m.Age).Index(indexPrefix + "_AGE");
            Map(m => m.Race).CustomType<race>().Nullable().Index(indexPrefix + "_RACE");
            Map(m => m.Sex).CustomType<sex>().Nullable().Index(indexPrefix + "_SEX");
            Map(m => m.PrimaryPayer).CustomType<primarypayer>().Nullable().Index(indexPrefix + "_PRIMARY_PAYER");
            Map(m => m.PatientStateCountyCode).Length(5).Index(indexPrefix + "_PATIENT_STATE_COUNTY_CODE");
            Map(m => m.LocalHospitalID).Length(12).Index(indexPrefix + "_LOCAL_HOSPITAL_ID");
            Map(m => m.DischargeDisposition).CustomType<dischargedisposition>().Nullable();
            Map(m => m.DischargeYear).Nullable();
            Map(m => m.DischargeQuarter).CustomType<int>().Nullable();
            Map(m => m.PrimaryDiagnosis).Length(10);
            Map(m => m.DiagnosisCode2).Length(10);
            Map(m => m.DiagnosisCode3).Length(10);
            Map(m => m.DiagnosisCode4).Length(10);
            Map(m => m.DiagnosisCode5).Length(10);
            Map(m => m.DiagnosisCode6).Length(10);
            Map(m => m.DiagnosisCode7).Length(10);
            Map(m => m.DiagnosisCode8).Length(10);
            Map(m => m.DiagnosisCode9).Length(10);
            Map(m => m.DiagnosisCode10).Length(10);
            Map(m => m.DiagnosisCode11).Length(10);
            Map(m => m.DiagnosisCode12).Length(10);
            Map(m => m.DiagnosisCode13).Length(10);
            Map(m => m.DiagnosisCode14).Length(10);
            Map(m => m.DiagnosisCode15).Length(10);
            Map(m => m.DiagnosisCode16).Length(10);
            Map(m => m.DiagnosisCode17).Length(10);
            Map(m => m.DiagnosisCode18).Length(10);
            Map(m => m.DiagnosisCode19).Length(10);
            Map(m => m.DiagnosisCode20).Length(10);
            Map(m => m.HospitalTraumaLevel).CustomType<hospitaltraumalevel>().Nullable().Index(indexPrefix + "_HOSPITALTRAUMALEVEL");
            Map(m => m.NumberofDiagnoses);
            Map(m => m.ICDCodeType).CustomType<ICDCodeTypeEnum>().Nullable().Index(indexPrefix + "_ICDCODETYPE");
        }

        //public override string EntityTableName
        //{
        //    get
        //    {
        //        return "Targets_" + Inflector.Pluralize(typeof(TreatAndReleaseTarget).Name);
        //    }
        //}
    }
}