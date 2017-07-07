// These aliases permit nullable properties below
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Validation;
using Monahrq.Sdk.Validation;
using Monahrq.Wing.Discharge.Validators;
using admissionSource = Monahrq.Infrastructure.Domain.Wings.AdmissionSource;
using admissionType = Monahrq.Infrastructure.Domain.Wings.AdmissionType;
using dischargeDisposition = Monahrq.Infrastructure.Domain.Wings.DischargeDisposition;
using edServices = Monahrq.Infrastructure.Domain.Wings.EDServices;
using race = Monahrq.Infrastructure.Domain.Wings.Race;
using primaryPayer = Monahrq.Infrastructure.Domain.Wings.PrimaryPayer;
using pointOfOrigin = Monahrq.Infrastructure.Domain.Wings.PointOfOrigin;
using sex = Monahrq.Infrastructure.Domain.Wings.Sex;


namespace Monahrq.Wing.Discharge.Inpatient
{

	/// <summary>
	/// The Inpatient Data Model.
	/// ALL numeric and enum properties below must be nullable (e.g. int?, DateTime?). String properties are inherently nullable. 
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.ICDCodedTarget" />
	[WingTarget("Inpatient Discharge", "0f330964-62d9-4f6b-ab70-beecb8acc0a5",
        "Mapping target for importing Inpatient Discharge Data", false, true,
	    PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
	    PublisherEmail = "moanhrq@ahrq.gov",
	    PublisherWebsite = "http://monahrq.ahrq.gov/")]
    [RejectIfAnyPropertyHasValue(typeof(InpatientTarget), sex.Exclude, admissionSource.Exclude,
        admissionType.Exclude, dischargeDisposition.Exclude,
        edServices.Exclude, race.Exclude, primaryPayer.Exclude, pointOfOrigin.Exclude)]
    [ICDValidation(typeof(InpatientTarget), true)]
    [EntityTableName("Targets_InpatientTargets")]
    public class InpatientTarget : ICDCodedTarget
    {
		// In this regex "^[^.]{3,7}$"...
		// ^ means start of line (so the entire input must match or fail the regex expression)
		// [^.] means no dot.
		// {3,7} is min/max length.
		// $ means end of line.

		/// <summary>
		/// The dx code regex
		/// </summary>
		const string DX_CODE_REGEX = @"^[^.]{3,7}$";
		/// <summary>
		/// The procedure code regex
		/// </summary>
		const string PROCEDURE_CODE_REGEX = @"^[^.]{3,7}$";

		/// <summary>
		/// Initializes a new instance of the <see cref="InpatientTarget"/> class.
		/// </summary>
		public InpatientTarget() : base() { }

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>
		/// The key.
		/// </value>
		[StringLength(20), WingTargetElement("Key", "Key", false, 1, "Sequence Number. Unique case identifier")]
        public virtual string Key { get; set; }

		/// <summary>
		/// Gets or sets the age.
		/// </summary>
		/// <value>
		/// The age.
		/// </value>
		[Range(0, 120), Required, WingTargetElement("Age", "Age", true, 2, "Age in Years at Admission")]
        public virtual int? Age { get; set; }

		/// <summary>
		/// Gets or sets the age in days.
		/// </summary>
		/// <value>
		/// The age in days.
		/// </value>
		[Range(0, 364), /*AgeInDaysValidation,*/ WingTargetElement("AgeInDays", "Age in Days", false, 3, "Age in Days (coded only when the age in years is zero)")]
        public virtual int? AgeInDays { get; set; }

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
		[Required]
        [WingTargetElement("Sex", "Sex", true, 5, "Sex of Patient")]
        public virtual sex? Sex { get; set; }

		/// <summary>
		/// Gets or sets the primary payer.
		/// </summary>
		/// <value>
		/// The primary payer.
		/// </value>
		[WingTargetElement("PrimaryPayer", "Primary Payer", false, 6, "Expected Primary Payer")]
        public virtual primaryPayer? PrimaryPayer { get; set; }

		/// <summary>
		/// Gets or sets the patient state county code.
		/// </summary>
		/// <value>
		/// The patient state county code.
		/// </value>
		[StringLength(5, MinimumLength = 5)]
        [WingTargetElement("PatientStateCountyCode", "Patient StateCounty Code", false, 7, "County patient residence.  SSCCC where SSCCC is the FIPS state and county code.")]
        public virtual string PatientStateCountyCode { get; set; }

		/// <summary>
		/// Gets or sets the local hospital identifier.
		/// </summary>
		/// <value>
		/// The local hospital identifier.
		/// </value>
		[StringLength(12)]
        [Required]
        [WingTargetElement("LocalHospitalID", "Local Hospital ID", true, 8, "Data Source Hospital Number")]
        public virtual string LocalHospitalID { get; set; }

		/// <summary>
		/// Gets or sets the discharge disposition.
		/// </summary>
		/// <value>
		/// The discharge disposition.
		/// </value>
		[WingTargetElement("DischargeDisposition", "Discharge Disposition", false, 9, "Disposition of Patient")]
        public virtual dischargeDisposition? DischargeDisposition { get; set; }

		/// <summary>
		/// Gets or sets the type of the admission.
		/// </summary>
		/// <value>
		/// The type of the admission.
		/// </value>
		[WingTargetElement("AdmissionType", "Admission Type", false, 10, "Admission Type")]
        public virtual admissionType? AdmissionType { get; set; }

		/// <summary>
		/// Gets or sets the admission source.
		/// </summary>
		/// <value>
		/// The admission source.
		/// </value>
		[WingTargetElement("AdmissionSource", "Admission Source", false, 11, "Admission Source")]
        public virtual admissionSource? AdmissionSource { get; set; }

		/// <summary>
		/// Gets or sets the point of origin.
		/// </summary>
		/// <value>
		/// The point of origin.
		/// </value>
		[WingTargetElement("PointOfOrigin", "Point of Origin", false, 12, "Point of Origin")]
        public virtual pointOfOrigin? PointOfOrigin { get; set; }

		//[Range(0, 99999)]
		//[Numeric] //Needs numeric validation
		/// <summary>
		/// Gets or sets the length of stay.
		/// </summary>
		/// <value>
		/// The length of stay.
		/// </value>
		[Numeric, WingTargetElement("LengthOfStay", "Length of Stay", false, 13, "Length of Stay")]
        public virtual int? LengthOfStay { get; set; }

		/// <summary>
		/// Gets or sets the discharge date.
		/// </summary>
		/// <value>
		/// The discharge date.
		/// </value>
		[WingTargetElement("DischargeDate", "Discharge Date", false, 165, "Date of Discharge (MM/DD/YYYY). For patient identification with external analysis. (Do not map this field if not required)")]
        public virtual DateTime? DischargeDate { get; set; }

		/// <summary>
		/// Gets or sets the discharge year.
		/// </summary>
		/// <value>
		/// The discharge year.
		/// </value>
		[Numeric, RangeToCurrentYear(1997, false)]
        [WingTargetElement("DischargeYear", "Discharge Year", true, 13, "Year of Discharge")]
        public virtual int? DischargeYear { get; set; }

		/// <summary>
		/// Gets or sets the discharge quarter.
		/// </summary>
		/// <value>
		/// The discharge quarter.
		/// </value>
		[Range(1, 4, ErrorMessage = @"Value must be numeric (1, 2, 3, 4) with no leading alpha characters.")] //Numeric, 
        [DischargeQuarterValidation]
        [WingTargetElement("DischargeQuarter", "Discharge Quarter", true, 14, "Quarter of Discharge (1 = Jan-Mar, 2=Apr-Jun, 3=Jul-Sep, 4=Oct-Dec) Discharge Date must be set if Discharge Quarter is not.")]
        public virtual object DischargeQuarter { get; set; }

		//[Range(0, 10000)]
		/// <summary>
		/// Gets or sets the days on mech ventilator.
		/// </summary>
		/// <value>
		/// The days on mech ventilator.
		/// </value>
		[WingTargetElement("DaysOnMechVentilator", "Days on Mech Ventilator", false, 15, "Number of days on a mechanical ventilator.  Optional for the 3M APR-DRG Grouper.")]
        public virtual int? DaysOnMechVentilator { get; set; }

		//[Range(200, 7000)]
		/// <summary>
		/// Gets or sets the birth weight grams.
		/// </summary>
		/// <value>
		/// The birth weight grams.
		/// </value>
		[WingTargetElement("BirthWeightGrams", "Birth Weight Grams", false, 16, "Birth Weight in Grams.  (200g-7000g)  Optional for the 3M APR-DRG Grouper.")]
        public virtual int? BirthWeightGrams { get; set; }

		/// <summary>
		/// Gets or sets the total charge.
		/// </summary>
		/// <value>
		/// The total charge.
		/// </value>
		[Range(0, int.MaxValue)]
        [WingTargetElement("TotalCharge", "Total Charge", false, 17, "Use to calculate cost-to-charge ratio used in preventive hospitalization cost mapping.")]
        public virtual int? TotalCharge { get; set; }

		/// <summary>
		/// Gets or sets the DRG.
		/// </summary>
		/// <value>
		/// The DRG.
		/// </value>
		[Range(0, 999), DRG_MDC_Validation, WingTargetElement("DRG", "DRG", false, 27, "Imported DRG CMS Value")]
        public virtual int? DRG { get; set; }

		/// <summary>
		/// Gets or sets the MDC.
		/// </summary>
		/// <value>
		/// The MDC.
		/// </value>
		[Range(0, 999), DRG_MDC_Validation, WingTargetElement("MDC", "MDC", false, 28, "Imported MDC CMS Value")]
        public virtual int? MDC { get; set; }

		/// <summary>
		/// Gets or sets the principal diagnosis.
		/// </summary>
		/// <value>
		/// The principal diagnosis.
		/// </value>
		[Required]
        [RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("PrincipalDiagnosis", "Principal Diagnosis", true, 29, "ICD-9/10-CM Diagnosis Code. (Principal diagnosis)")]
        public virtual string PrincipalDiagnosis { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code2.
		/// </summary>
		/// <value>
		/// The diagnosis code2.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode2", "Diagnosis Code 2", false, 30, "ICD-9/10-CM Diagnosis Code 2. (Secondary diagnoses)")]
        public virtual string DiagnosisCode2 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code3.
		/// </summary>
		/// <value>
		/// The diagnosis code3.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode3", "Diagnosis Code 3", false, 31, "ICD-9/10-CM Diagnosis Code 3. (Secondary diagnoses)")]
        public virtual string DiagnosisCode3 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code4.
		/// </summary>
		/// <value>
		/// The diagnosis code4.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode4", "Diagnosis Code 4", false, 32, "ICD-9/10-CM Diagnosis Code 4. (Secondary diagnoses)")]
        public virtual string DiagnosisCode4 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code5.
		/// </summary>
		/// <value>
		/// The diagnosis code5.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode5", "Diagnosis Code 5", false, 33, "ICD-9/10-CM Diagnosis Code 5. (Secondary diagnoses)")]
        public virtual string DiagnosisCode5 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code6.
		/// </summary>
		/// <value>
		/// The diagnosis code6.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode6", "Diagnosis Code 6", false, 34, "ICD-9/10-CM Diagnosis Code 6. (Secondary diagnoses)")]
        public virtual string DiagnosisCode6 { get; set; }
		/// <summary>
		/// Gets or sets the diagnosis code7.
		/// </summary>
		/// <value>
		/// The diagnosis code7.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode7", "Diagnosis Code 7", false, 35, "ICD-9/10-CM Diagnosis Code 7. (Secondary diagnoses)")]
        public virtual string DiagnosisCode7 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code8.
		/// </summary>
		/// <value>
		/// The diagnosis code8.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode8", "Diagnosis Code 8", false, 36, "ICD-9/10-CM Diagnosis Code 8. (Secondary diagnoses)")]
        public virtual string DiagnosisCode8 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code9.
		/// </summary>
		/// <value>
		/// The diagnosis code9.
		/// </value>
		[RegexWarning(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode9", "Diagnosis Code 9", false, 37, "ICD-9/10-CM Diagnosis Code 9. (Secondary diagnoses)")]
        public virtual string DiagnosisCode9 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code10.
		/// </summary>
		/// <value>
		/// The diagnosis code10.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode10", "Diagnosis Code 10", false, 38, "ICD-9/10-CM Diagnosis Code 10. (Secondary diagnoses)")]
        public virtual string DiagnosisCode10 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code11.
		/// </summary>
		/// <value>
		/// The diagnosis code11.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode11", "Diagnosis Code 11", false, 39, "ICD-9/10-CM Diagnosis Code 11. (Secondary diagnoses)")]
        public virtual string DiagnosisCode11 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code12.
		/// </summary>
		/// <value>
		/// The diagnosis code12.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode12", "Diagnosis Code 12", false, 40, "ICD-9/10-CM Diagnosis Code 12. (Secondary diagnoses)")]
        public virtual string DiagnosisCode12 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code13.
		/// </summary>
		/// <value>
		/// The diagnosis code13.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode13", "Diagnosis Code 13", false, 41, "ICD-9/10-CM Diagnosis Code 13. (Secondary diagnoses)")]
        public virtual string DiagnosisCode13 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code14.
		/// </summary>
		/// <value>
		/// The diagnosis code14.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode14", "Diagnosis Code 14", false, 42, "ICD-9/10-CM Diagnosis Code 14. (Secondary diagnoses)")]
        public virtual string DiagnosisCode14 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code15.
		/// </summary>
		/// <value>
		/// The diagnosis code15.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode15", "Diagnosis Code 15", false, 43, "ICD-9/10-CM Diagnosis Code 15. (Secondary diagnoses)")]
        public virtual string DiagnosisCode15 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code16.
		/// </summary>
		/// <value>
		/// The diagnosis code16.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode16", "Diagnosis Code 16", false, 44, "ICD-9/10-CM Diagnosis Code 16. (Secondary diagnoses)")]
        public virtual string DiagnosisCode16 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code17.
		/// </summary>
		/// <value>
		/// The diagnosis code17.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode17", "Diagnosis Code 17", false, 45, "ICD-9/10-CM Diagnosis Code 17. (Secondary diagnoses)")]
        public virtual string DiagnosisCode17 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code18.
		/// </summary>
		/// <value>
		/// The diagnosis code18.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode18", "Diagnosis Code 18", false, 46, "ICD-9/10-CM Diagnosis Code 18. (Secondary diagnoses)")]
        public virtual string DiagnosisCode18 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code19.
		/// </summary>
		/// <value>
		/// The diagnosis code19.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode19", "Diagnosis Code 19", false, 47, "ICD-9/10-CM Diagnosis Code 19. (Secondary diagnoses)")]
        public virtual string DiagnosisCode19 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code20.
		/// </summary>
		/// <value>
		/// The diagnosis code20.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode20", "Diagnosis Code 20", false, 48, "ICD-9/10-CM Diagnosis Code 20. (Secondary diagnoses)")]
        public virtual string DiagnosisCode20 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code21.
		/// </summary>
		/// <value>
		/// The diagnosis code21.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode21", "Diagnosis Code 21", false, 49, "ICD-9/10-CM Diagnosis Code 21. (Secondary diagnoses)")]
        public virtual string DiagnosisCode21 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code22.
		/// </summary>
		/// <value>
		/// The diagnosis code22.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode22", "Diagnosis Code 22", false, 50, "ICD-9/10-CM Diagnosis Code 22. (Secondary diagnoses)")]
        public virtual string DiagnosisCode22 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code23.
		/// </summary>
		/// <value>
		/// The diagnosis code23.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode23", "Diagnosis Code 23", false, 51, "ICD-9/10-CM Diagnosis Code 23. (Secondary diagnoses)")]
        public virtual string DiagnosisCode23 { get; set; }

		/// <summary>
		/// Gets or sets the diagnosis code24.
		/// </summary>
		/// <value>
		/// The diagnosis code24.
		/// </value>
		[RegularExpression(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("DiagnosisCode24", "Diagnosis Code 24", false, 52, "ICD-9/10-CM Diagnosis Code 24. (Secondary diagnoses)")]
        public virtual string DiagnosisCode24 { get; set; }

		//[Required]
		/// <summary>
		/// Gets or sets the principal procedure.
		/// </summary>
		/// <value>
		/// The principal procedure.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("PrincipalProcedure", "Principal Procedure", false, 64, "ICD-9/10-CM Procedure Code. (Principal procedure)")]
        public virtual string PrincipalProcedure { get; set; }

		/// <summary>
		/// Gets or sets the procedure code2.
		/// </summary>
		/// <value>
		/// The procedure code2.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode2", "Procedure Code 2", false, 65, "ICD-9/10-CM Procedure Code 2. (secondary procedures)")]
        public virtual string ProcedureCode2 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code3.
		/// </summary>
		/// <value>
		/// The procedure code3.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode3", "Procedure Code 3", false, 66, "ICD-9/10-CM Procedure Code 3. (secondary procedures)")]
        public virtual string ProcedureCode3 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code4.
		/// </summary>
		/// <value>
		/// The procedure code4.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode4", "Procedure Code 4", false, 67, "ICD-9/10-CM Procedure Code 4. (secondary procedures)")]
        public virtual string ProcedureCode4 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code5.
		/// </summary>
		/// <value>
		/// The procedure code5.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode5", "Procedure Code 5", false, 68, "ICD-9/10-CM Procedure Code 5. (secondary procedures)")]
        public virtual string ProcedureCode5 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code6.
		/// </summary>
		/// <value>
		/// The procedure code6.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode6", "Procedure Code 6", false, 69, "ICD-9/10-CM Procedure Code 6. (secondary procedures)")]
        public virtual string ProcedureCode6 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code7.
		/// </summary>
		/// <value>
		/// The procedure code7.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode7", "Procedure Code 7", false, 70, "ICD-9/10-CM Procedure Code 7. (secondary procedures)")]
        public virtual string ProcedureCode7 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code8.
		/// </summary>
		/// <value>
		/// The procedure code8.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode8", "Procedure Code 8", false, 71, "ICD-9/10-CM Procedure Code 8. (secondary procedures)")]
        public virtual string ProcedureCode8 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code9.
		/// </summary>
		/// <value>
		/// The procedure code9.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode9", "Procedure Code 9", false, 72, "ICD-9/10-CM Procedure Code 9. (secondary procedures)")]
        public virtual string ProcedureCode9 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code10.
		/// </summary>
		/// <value>
		/// The procedure code10.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode10", "Procedure Code 10", false, 73, "ICD-9/10-CM Procedure Code 10. (secondary procedures)")]
        public virtual string ProcedureCode10 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code11.
		/// </summary>
		/// <value>
		/// The procedure code11.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode11", "Procedure Code 11", false, 74, "ICD-9/10-CM Procedure Code 11. (secondary procedures)")]
        public virtual string ProcedureCode11 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code12.
		/// </summary>
		/// <value>
		/// The procedure code12.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode12", "Procedure Code 12", false, 75, "ICD-9/10-CM Procedure Code 12. (secondary procedures)")]
        public virtual string ProcedureCode12 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code13.
		/// </summary>
		/// <value>
		/// The procedure code13.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode13", "Procedure Code 13", false, 76, "ICD-9/10-CM Procedure Code 13. (secondary procedures)")]
        public virtual string ProcedureCode13 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code14.
		/// </summary>
		/// <value>
		/// The procedure code14.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode14", "Procedure Code 14", false, 77, "ICD-9/10-CM Procedure Code 14. (secondary procedures)")]
        public virtual string ProcedureCode14 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code15.
		/// </summary>
		/// <value>
		/// The procedure code15.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode15", "Procedure Code 15", false, 78, "ICD-9/10-CM Procedure Code 15. (secondary procedures)")]
        public virtual string ProcedureCode15 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code16.
		/// </summary>
		/// <value>
		/// The procedure code16.
		/// </value>
		[RegexWarning(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode16", "Procedure Code 16", false, 79, "ICD-9/10-CM Procedure Code 16. (secondary procedures)")]
        public virtual string ProcedureCode16 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code17.
		/// </summary>
		/// <value>
		/// The procedure code17.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode17", "Procedure Code 17", false, 80, "ICD-9/10-CM Procedure Code 17. (secondary procedures)")]
        public virtual string ProcedureCode17 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code18.
		/// </summary>
		/// <value>
		/// The procedure code18.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode18", "Procedure Code 18", false, 81, "ICD-9/10-CM Procedure Code 18. (secondary procedures)")]
        public virtual string ProcedureCode18 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code19.
		/// </summary>
		/// <value>
		/// The procedure code19.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode19", "Procedure Code 19", false, 82, "ICD-9/10-CM Procedure Code 19. (secondary procedures)")]
        public virtual string ProcedureCode19 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code20.
		/// </summary>
		/// <value>
		/// The procedure code20.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode20", "Procedure Code 20", false, 83, "ICD-9/10-CM Procedure Code 20. (secondary procedures)")]
        public virtual string ProcedureCode20 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code21.
		/// </summary>
		/// <value>
		/// The procedure code21.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode21", "Procedure Code 21", false, 84, "ICD-9/10-CM Procedure Code 21. (secondary procedures)")]
        public virtual string ProcedureCode21 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code22.
		/// </summary>
		/// <value>
		/// The procedure code22.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode22", "Procedure Code 22", false, 85, "ICD-9/10-CM Procedure Code 22. (secondary procedures)")]
        public virtual string ProcedureCode22 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code23.
		/// </summary>
		/// <value>
		/// The procedure code23.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode23", "Procedure Code 23", false, 86, "ICD-9/10-CM Procedure Code 23. (secondary procedures)")]
        public virtual string ProcedureCode23 { get; set; }

		/// <summary>
		/// Gets or sets the procedure code24.
		/// </summary>
		/// <value>
		/// The procedure code24.
		/// </value>
		[RegularExpression(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 3-7 characters and digits with no decimal point.")]
        [WingTargetElement("ProcedureCode24", "Procedure Code 24", false, 87, "ICD-9/10-CM Procedure Code 24. (secondary procedures)")]
        public virtual string ProcedureCode24 { get; set; }

		#region Comment out but may be reused later
		//[Range(1, 579)]
		//[WingTargetElement("DiagnosisRelatedGroup", "Diagnosis Related Group", false, 19, "Diagnosis Related Group from CMS DRG Grouper")]
		//public virtual int? DiagnosisRelatedGroup { get; set; }

		//[Range(1, 999)]
		//[WingTargetElement("MSDRG", "MS DRG", false, 20, "MS Diagnosis Related Group")]
		//public virtual int? MSDRG { get; set; }

		//[Range(1, 579)]
		//[WingTargetElement("RATESDRG", "RATES DRG", false, 21, "CMS Diagnosis Related Group")]
		//public virtual int? RATESDRG { get; set; }

		//[Range(0, 99)]
		//[WingTargetElement("MajorDiagnosticCategory", "Major Diagnostic Category", false, 24, "Based upon principal diagnosis")]
		//public virtual int? MajorDiagnosticCategory { get; set; }

		//[Range(1, 99)]
		//[WingTargetElement("ICDVER", "ICD VER", false, 18, "ICD9CM Version")]
		//public virtual int? ICDVER { get; set; }

		//[Range(20, 30)]
		//[WingTargetElement("DRGVersion", "DRG Version", false, 26, "DRG Version of 25 or greater sets MS_DRG value, leser sets CMS_DRG value")]
		//public virtual int? DRGVersion { get; set; }

		//[RegexWarning(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-5 characters and digits with no decimal point.")]
		//[WingTargetElement("DiagnosisCode25", "Diagnosis Code 25", false, 53, "ICD-9/10-CM Diagnosis Code 25. (Secondary diagnoses)")]
		//public virtual string DiagnosisCode25 { get; set; }

		//[RegexWarning(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-5 characters and digits with no decimal point.")]
		//[WingTargetElement("DiagnosisCode26", "Diagnosis Code 26", false, 54, "ICD-9/10-CM Diagnosis Code 26. (Secondary diagnoses)")]
		//public virtual string DiagnosisCode26 { get; set; }

		//[RegexWarning(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-5 characters and digits with no decimal point.")]
		//[WingTargetElement("DiagnosisCode27", "Diagnosis Code 27", false, 55, "ICD-9/10-CM Diagnosis Code 27. (Secondary diagnoses)")]
		//public virtual string DiagnosisCode27 { get; set; }

		//[RegexWarning(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-5 characters and digits with no decimal point.")]
		//[WingTargetElement("DiagnosisCode28", "Diagnosis Code 28", false, 56, "ICD-9/10-CM Diagnosis Code 28. (Secondary diagnoses)")]
		//public virtual string DiagnosisCode28 { get; set; }

		//[RegexWarning(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-5 characters and digits with no decimal point.")]
		//[WingTargetElement("DiagnosisCode29", "Diagnosis Code 29", false, 57, "ICD-9/10-CM Diagnosis Code 29. (Secondary diagnoses)")]
		//public virtual string DiagnosisCode29 { get; set; }

		//[RegexWarning(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-5 characters and digits with no decimal point.")]
		//[WingTargetElement("DiagnosisCode30", "Diagnosis Code 30", false, 58, "ICD-9/10-CM Diagnosis Code 30. (Secondary diagnoses)")]
		//public virtual string DiagnosisCode30 { get; set; }

		//[RegexWarning(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-5 characters and digits with no decimal point.")]
		//[WingTargetElement("DiagnosisCode31", "Diagnosis Code 31", false, 59, "ICD-9/10-CM Diagnosis Code 31. (Secondary diagnoses or E-code)")]
		//public virtual string DiagnosisCode31 { get; set; }

		//[RegexWarning(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-5 characters and digits with no decimal point.")]
		//[WingTargetElement("DiagnosisCode32", "Diagnosis Code 32", false, 60, "ICD-9/10-CM Diagnosis Code 32. (Secondary diagnoses or E-code)")]
		//public virtual string DiagnosisCode32 { get; set; }

		//[RegexWarning(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-5 characters and digits with no decimal point.")]
		//[WingTargetElement("DiagnosisCode33", "Diagnosis Code 33", false, 61, "ICD-9/10-CM Diagnosis Code 33. (Secondary diagnoses or E-code)")]
		//public virtual string DiagnosisCode33 { get; set; }

		//[RegexWarning(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-5 characters and digits with no decimal point.")]
		//[WingTargetElement("DiagnosisCode34", "Diagnosis Code 34", false, 62, "ICD-9/10-CM Diagnosis Code 34. (Secondary diagnoses or E-code)")]
		//public virtual string DiagnosisCode34 { get; set; }

		//[RegexWarning(DX_CODE_REGEX, ErrorMessage = @"Must consist of 3-5 characters and digits with no decimal point.")]
		//[WingTargetElement("DiagnosisCode35", "Diagnosis Code 35", false, 63, "ICD-9/10-CM Diagnosis Code 35. (Secondary diagnoses or E-code)")]
		//public virtual string DiagnosisCode35 { get; set; }

		//[RegexWarning(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 2-4 characters and digits with no decimal point.")]
		//[WingTargetElement("ProcedureCode25", "Procedure Code 25", false, 88, "ICD-9/10-CM Procedure Code 25. (secondary procedures)")]
		//public virtual string ProcedureCode25 { get; set; }

		//[RegexWarning(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 2-4 characters and digits with no decimal point.")]
		//[WingTargetElement("ProcedureCode26", "Procedure Code 26", false, 89, "ICD-9/10-CM Procedure Code 26. (secondary procedures)")]
		//public virtual string ProcedureCode26 { get; set; }

		//[RegexWarning(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 2-4 characters and digits with no decimal point.")]
		//[WingTargetElement("ProcedureCode27", "Procedure Code 27", false, 90, "ICD-9/10-CM Procedure Code 27. (secondary procedures)")]
		//public virtual string ProcedureCode27 { get; set; }

		//[RegexWarning(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 2-4 characters and digits with no decimal point.")]
		//[WingTargetElement("ProcedureCode28", "Procedure Code 28", false, 91, "ICD-9/10-CM Procedure Code 28. (secondary procedures)")]
		//public virtual string ProcedureCode28 { get; set; }

		//[RegexWarning(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 2-4 characters and digits with no decimal point.")]
		//[WingTargetElement("ProcedureCode29", "Procedure Code 29", false, 92, "ICD-9/10-CM Procedure Code 29. (secondary procedures)")]
		//public virtual string ProcedureCode29 { get; set; }

		//[RegexWarning(PROCEDURE_CODE_REGEX, ErrorMessage = @"Must consist of 2-4 characters and digits with no decimal point.")]
		//[WingTargetElement("ProcedureCode30", "Procedure Code 30", false, 93, "ICD-9/10-CM Procedure Code 30. (secondary procedures)")]
		//public virtual string ProcedureCode30 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure1", "Days to Procedure 1", false, 94, "Days from Admission to Procedure.  (Principal procedure) ")]
		// public virtual int? DaysToProcedure1 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure2", "Days to Procedure 2", false, 95, "Days from Admission to Procedure 2.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure2 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure3", "Days to Procedure 3", false, 96, "Days from Admission to Procedure 3.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure3 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure4", "Days to Procedure 4", false, 97, "Days from Admission to Procedure 4.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure4 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure5", "Days to Procedure 5", false, 98, "Days from Admission to Procedure 5.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure5 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure6", "Days to Procedure 6", false, 99, "Days from Admission to Procedure 6.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure6 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure7", "Days to Procedure 7", false, 100, "Days from Admission to Procedure 7.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure7 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure8", "Days to Procedure 8", false, 101, "Days from Admission to Procedure 8.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure8 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure9", "Days to Procedure 9", false, 102, "Days from Admission to Procedure 9.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure9 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure10", "Days to Procedure 10", false, 103, "Days from Admission to Procedure 10.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure10 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure11", "Days to Procedure 11", false, 104, "Days from Admission to Procedure 11.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure11 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure12", "Days to Procedure 12", false, 105, "Days from Admission to Procedure 12.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure12 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure13", "Days to Procedure 13", false, 106, "Days from Admission to Procedure 13.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure13 { get; set; }

		//// [Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure14", "Days to Procedure 14", false, 107, "Days from Admission to Procedure 14.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure14 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure15", "Days to Procedure 15", false, 108, "Days from Admission to Procedure 15.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure15 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure16", "Days to Procedure 16", false, 109, "Days from Admission to Procedure 16.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure16 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure17", "Days to Procedure 17", false, 110, "Days from Admission to Procedure 17.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure17 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure18", "Days to Procedure 18", false, 111, "Days from Admission to Procedure 18.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure18 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure19", "Days to Procedure 19", false, 112, "Days from Admission to Procedure 19.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure19 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure20", "Days to Procedure 20", false, 113, "Days from Admission to Procedure 20.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure20 { get; set; }

		// //[Range(-32000, 32000)]
		// [WingTargetElement("DaysToProcedure21", "Days to Procedure 21", false, 114, "Days from Admission to Procedure 21.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure21 { get; set; }

		// [/*Range(-32000, 32000),*/ WingTargetElement("DaysToProcedure22", "Days to Procedure 22", false, 115, "Days from Admission to Procedure 22.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure22 { get; set; }

		// [/*Range(-32000, 32000),*/ WingTargetElement("DaysToProcedure23", "Days to Procedure 23", false, 116, "Days from Admission to Procedure 23.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure23 { get; set; }

		// [/*Range(-32000, 32000),*/ WingTargetElement("DaysToProcedure24", "Days to Procedure 24", false, 117, "Days from Admission to Procedure 24.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure24 { get; set; }

		// [/*Range(-32000, 32000),*/ WingTargetElement("DaysToProcedure25", "Days to Procedure 25", false, 118, "Days from Admission to Procedure 25.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure25 { get; set; }

		// [/*Range(-32000, 32000),*/ WingTargetElement("DaysToProcedure26", "Days to Procedure 26", false, 119, "Days from Admission to Procedure 26.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure26 { get; set; }

		// [/*Range(-32000, 32000),*/ WingTargetElement("DaysToProcedure27", "Days to Procedure 27", false, 120, "Days from Admission to Procedure 27.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure27 { get; set; }

		// [/*Range(-32000, 32000),*/ WingTargetElement("DaysToProcedure28", "Days to Procedure 28", false, 121, "Days from Admission to Procedure 28.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure28 { get; set; }

		// [/*Range(-32000, 32000),*/WingTargetElement("DaysToProcedure29", "Days to Procedure 29", false, 122, "Days from Admission to Procedure 29.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure29 { get; set; }

		// [/*Range(-32000, 32000),*/ WingTargetElement("DaysToProcedure30", "Days to Procedure 30", false, 123, "Days from Admission to Procedure 30.  (Secondary procedures)")]
		// public virtual int? DaysToProcedure30 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission1", "Present on Admission 1", false, 127, "Principal Diagnosis is present on admission")]
		//public virtual string PresentOnAdmission1 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission2", "Present on Admission 2", false, 128, "Diagnosis Code 2 is present on admission")]
		//public virtual string PresentOnAdmission2 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission3", "Present on Admission 3", false, 129, "Diagnosis Code 3 is present on admission")]
		//public virtual string PresentOnAdmission3 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission4", "Present on Admission 4", false, 130, "Diagnosis Code 4 is present on admission")]
		//public virtual string PresentOnAdmission4 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission5", "Present on Admission 5", false, 131, "Diagnosis Code 5 is present on admission")]
		//public virtual string PresentOnAdmission5 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission6", "Present on Admission 6", false, 132, "Diagnosis Code 6 is present on admission")]
		//public virtual string PresentOnAdmission6 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission7", "Present on Admission 7", false, 133, "Diagnosis Code 7 is present on admission")]
		//public virtual string PresentOnAdmission7 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission8", "Present on Admission 8", false, 134, "Diagnosis Code 8 is present on admission")]
		//public virtual string PresentOnAdmission8 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission9", "Present on Admission 9", false, 135, "Diagnosis Code 9 is present on admission")]
		//public virtual string PresentOnAdmission9 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission10", "Present on Admission 10", false, 136, "Diagnosis Code 10 is present on admission")]
		//public virtual string PresentOnAdmission10 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission11", "Present on Admission 11", false, 137, "Diagnosis Code 11 is present on admission")]
		//public virtual string PresentOnAdmission11 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission12", "Present on Admission 12", false, 138, "Diagnosis Code 12 is present on admission")]
		//public virtual string PresentOnAdmission12 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission13", "Present on Admission 13", false, 139, "Diagnosis Code 13 is present on admission")]
		//public virtual string PresentOnAdmission13 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission14", "Present on Admission 14", false, 140, "Diagnosis Code 14 is present on admission")]
		//public virtual string PresentOnAdmission14 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission15", "Present on Admission 15", false, 141, "Diagnosis Code 15 is present on admission")]
		//public virtual string PresentOnAdmission15 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission16", "Present on Admission 16", false, 142, "Diagnosis Code 16 is present on admission")]
		//public virtual string PresentOnAdmission16 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission17", "Present on Admission 17", false, 143, "Diagnosis Code 17 is present on admission")]
		//public virtual string PresentOnAdmission17 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission18", "Present on Admission 18", false, 144, "Diagnosis Code 18 is present on admission")]
		//public virtual string PresentOnAdmission18 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission19", "Present on Admission 19", false, 145, "Diagnosis Code 19 is present on admission")]
		//public virtual string PresentOnAdmission19 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission20", "Present on Admission 20", false, 146, "Diagnosis Code 20 is present on admission")]
		//public virtual string PresentOnAdmission20 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission21", "Present on Admission 21", false, 147, "Diagnosis Code 21 is present on admission")]
		//public virtual string PresentOnAdmission21 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission22", "Present on Admission 22", false, 148, "Diagnosis Code 22 is present on admission")]
		//public virtual string PresentOnAdmission22 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission23", "Present on Admission 23", false, 149, "Diagnosis Code 23 is present on admission")]
		//public virtual string PresentOnAdmission23 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission24", "Present on Admission 24", false, 150, "Diagnosis Code 24 is present on admission")]
		//public virtual string PresentOnAdmission24 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission25", "Present on Admission 25", false, 151, "Diagnosis Code 25 is present on admission")]
		//public virtual string PresentOnAdmission25 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission26", "Present on Admission 26", false, 152, "Diagnosis Code 26 is present on admission")]
		//public virtual string PresentOnAdmission26 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission27", "Present on Admission 27", false, 153, "Diagnosis Code 27 is present on admission")]
		//public virtual string PresentOnAdmission27 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission28", "Present on Admission 28", false, 154, "Diagnosis Code 28 is present on admission")]
		//public virtual string PresentOnAdmission28 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission29", "Present on Admission 29", false, 155, "Diagnosis Code 29 is present on admission")]
		//public virtual string PresentOnAdmission29 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission30", "Present on Admission 30", false, 156, "Diagnosis Code 30 is present on admission")]
		//public virtual string PresentOnAdmission30 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission31", "Present on Admission 31", false, 157, "Diagnosis Code 31 is present on admission")]
		//public virtual string PresentOnAdmission31 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission32", "Present on Admission 32", false, 158, "Diagnosis Code 32 is present on admission")]
		//public virtual string PresentOnAdmission32 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission33", "Present on Admission 33", false, 159, "Diagnosis Code 33 is present on admission")]
		//public virtual string PresentOnAdmission33 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission34", "Present on Admission 34", false, 160, "Diagnosis Code 34 is present on admission")]
		//public virtual string PresentOnAdmission34 { get; set; }

		//[StringLength(1), WingTargetElement("PresentOnAdmission35", "Present on Admission 35", false, 161, "Diagnosis Code 35 is present on admission")]
		//public virtual string PresentOnAdmission35 { get; set; }
		#endregion

		/// <summary>
		/// Gets or sets the custom stratifier1.
		/// </summary>
		/// <value>
		/// The custom stratifier1.
		/// </value>
		[StringLength(20), WingTargetElement("CustomStratifier1", "Custom Stratifier 1", false, 124, "Custom Stratifier 1")]
        public virtual string CustomStratifier1 { get; set; }

		/// <summary>
		/// Gets or sets the custom stratifier2.
		/// </summary>
		/// <value>
		/// The custom stratifier2.
		/// </value>
		[StringLength(20), WingTargetElement("CustomStratifier2", "Custom Stratifier 2", false, 125, "Custom Stratifier 2")]
        public virtual string CustomStratifier2 { get; set; }

		/// <summary>
		/// Gets or sets the custom stratifier3.
		/// </summary>
		/// <value>
		/// The custom stratifier3.
		/// </value>
		[StringLength(20), WingTargetElement("CustomStratifier3", "Custom Stratifier 3", false, 126, "Custom Stratifier 3")]
        public virtual string CustomStratifier3 { get; set; }

		/// <summary>
		/// Gets or sets the patient identifier.
		/// </summary>
		/// <value>
		/// The patient identifier.
		/// </value>
		[StringLength(20), WingTargetElement("PatientID", "Patient ID", false, 162, "Patient Identifier.  For use with external analysis.  (Do not map this field if not required)")]
        public virtual string PatientID { get; set; }

		/// <summary>
		/// Gets or sets the birth date.
		/// </summary>
		/// <value>
		/// The birth date.
		/// </value>
		[WingTargetElement("BirthDate", "Birth Date", false, 163, "Date of Birth (MM/DD/YYYY).  For patient identification with external analysis.  (Do not map this field if not required)")]
        public virtual DateTime? BirthDate { get; set; }

		/// <summary>
		/// Gets or sets the admission date.
		/// </summary>
		/// <value>
		/// The admission date.
		/// </value>
		[WingTargetElement("AdmissionDate", "Admission Date", false, 164, "Admission Date (MM/DD/YYYY).  For patient identification with external analysis.  (Do not map this field if not required)")]
        public virtual DateTime? AdmissionDate { get; set; }

		/// <summary>
		/// Gets or sets the ed services.
		/// </summary>
		/// <value>
		/// The ed services.
		/// </value>
		[WingTargetElement("EDServices", "ED Services", false, 165, "ED Services")]
        public virtual edServices? EDServices { get; set; }

		/// <summary>
		/// Gets or sets the HRR region identifier.
		/// </summary>
		/// <value>
		/// The HRR region identifier.
		/// </value>
		[WingTargetElement("HRRRegionID", "HRR Region ID", false, 166, "Data Source for HRR Region ID")]
        public virtual int? HRRRegionID { get; set; }

		/// <summary>
		/// Gets or sets the hsa region identifier.
		/// </summary>
		/// <value>
		/// The hsa region identifier.
		/// </value>
		[WingTargetElement("HSARegionID", "HSA Region ID", false, 167, "Data Source for HSA Region ID")]
        public virtual int? HSARegionID { get; set; }

		/// <summary>
		/// Gets or sets the custom region identifier.
		/// </summary>
		/// <value>
		/// The custom region identifier.
		/// </value>
		[WingTargetElement("CustomRegionID", "Custom Region ID", false, 168, "Data Source for Custom Region ID")]
        public virtual int? CustomRegionID { get; set; }

		/// <summary>
		/// Gets or sets the patient zip code.
		/// </summary>
		/// <value>
		/// The patient zip code.
		/// </value>
		[StringLength(5, MinimumLength = 5)]
        [WingTargetElement("PatientZipCode", "Patient Zip Code", false, 169, "Data Source for Patient Zip Code")]
        public virtual string PatientZipCode { get; set; }

		/// <summary>
		/// Creates the bulk insert mapper.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dataTable">The data table.</param>
		/// <param name="instance">The instance.</param>
		/// <param name="target">The target.</param>
		/// <returns></returns>
		public override IBulkMapper CreateBulkInsertMapper<T>(DataTable dataTable, T instance = default(T), Target target = null)
        {
            return new DatasetRecordBulkInsertMapper<T>(dataTable, instance, Dataset.ContentType);
        }

		/// <summary>
		/// Gets the property name matchs.
		/// </summary>
		/// <value>
		/// The property name matchs.
		/// </value>
		protected override List<string> PropertyNameMatchs { get { return new List<string> {"Diagnosis", "Procedure"}; } }
		/// <summary>
		/// Gets a value indicating whether [perform procedure check].
		/// </summary>
		/// <value>
		/// <c>true</c> if [perform procedure check]; otherwise, <c>false</c>.
		/// </value>
		protected override bool PerformProcedureCheck { get { return true; } }
    }

	/// <summary>
	/// The NHibernate mapping for the Inpatient Data Model.
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecordMap{Monahrq.Wing.Discharge.Inpatient.InpatientTarget}" />
	public class InpatientTargetMap : DatasetRecordMap<InpatientTarget>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="InpatientTargetMap"/> class.
		/// </summary>
		public InpatientTargetMap()
        {
            var indexPrefix = string.Format("IDX_{0}", typeof(InpatientTarget).Name);
            Map(m => m.Key).Length(20);
            Map(m => m.Age).Nullable().Index(indexPrefix); //+ "_AGE"
            Map(m => m.AgeInDays).Nullable();
            Map(m => m.Race).CustomType<race>().Nullable().Index(indexPrefix); //+ "_RACE"
            Map(m => m.Sex).CustomType<sex>().Nullable().Index(indexPrefix); // + "_SEX"
            Map(m => m.PrimaryPayer).CustomType<int>().Nullable();
            Map(m => m.PatientStateCountyCode).Length(5);
            Map(m => m.LocalHospitalID).Length(12).Index(indexPrefix); //+ "_LOCAL_HOSPITAL_ID"
            Map(m => m.DischargeDisposition).CustomType<dischargeDisposition>();
            Map(m => m.AdmissionType).CustomType<admissionType>();
            Map(m => m.AdmissionSource).CustomType<admissionSource>();
            Map(m => m.PointOfOrigin).CustomType<pointOfOrigin>();
            Map(m => m.LengthOfStay).Nullable();

            Map(m => m.DischargeDate).Nullable();
            Map(m => m.DischargeYear).Nullable();
            Map(m => m.DischargeQuarter).CustomType<int>().Nullable();

            Map(m => m.DaysOnMechVentilator).Nullable();
            Map(m => m.BirthWeightGrams).Nullable();
            Map(m => m.TotalCharge).Nullable();

            Map(m => m.DRG).Nullable().Index(indexPrefix + "_DRG");
            Map(m => m.MDC).Nullable().Index(indexPrefix + "_MDC");

            Map(m => m.PrincipalDiagnosis).Length(10);

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
            Map(m => m.DiagnosisCode21).Length(10);
            Map(m => m.DiagnosisCode22).Length(10);
            Map(m => m.DiagnosisCode23).Length(10);
            Map(m => m.DiagnosisCode24).Length(10);

            Map(m => m.PrincipalProcedure).Length(10);

            Map(m => m.ProcedureCode2).Length(10);
            Map(m => m.ProcedureCode3).Length(10);
            Map(m => m.ProcedureCode4).Length(10);
            Map(m => m.ProcedureCode5).Length(10);
            Map(m => m.ProcedureCode6).Length(10);
            Map(m => m.ProcedureCode7).Length(10);
            Map(m => m.ProcedureCode8).Length(10);
            Map(m => m.ProcedureCode9).Length(10);
            Map(m => m.ProcedureCode10).Length(10);
            Map(m => m.ProcedureCode11).Length(10);
            Map(m => m.ProcedureCode12).Length(10);
            Map(m => m.ProcedureCode13).Length(10);
            Map(m => m.ProcedureCode14).Length(10);
            Map(m => m.ProcedureCode15).Length(10);
            Map(m => m.ProcedureCode16).Length(10);
            Map(m => m.ProcedureCode17).Length(10);
            Map(m => m.ProcedureCode18).Length(10);
            Map(m => m.ProcedureCode19).Length(10);
            Map(m => m.ProcedureCode20).Length(10);
            Map(m => m.ProcedureCode21).Length(10);
            Map(m => m.ProcedureCode22).Length(10);
            Map(m => m.ProcedureCode23).Length(10);
            Map(m => m.ProcedureCode24).Length(10);

            Map(m => m.CustomStratifier1).Length(20);
            Map(m => m.CustomStratifier2).Length(20);
            Map(m => m.CustomStratifier3).Length(20);

            Map(m => m.PatientID).Length(20).Index(indexPrefix + "_PATIENT_ID");
            Map(m => m.BirthDate).Nullable();
            Map(m => m.AdmissionDate).Nullable();

            Map(m => m.EDServices).CustomType<int>().Nullable().Index(indexPrefix + "_ED_SERVICES");

            Map(m => m.HRRRegionID).Nullable().Index(indexPrefix + "_HRR_REGION_ID");
            Map(m => m.HSARegionID).Nullable().Index(indexPrefix + "_HSA_REGION_ID");
            Map(m => m.CustomRegionID).Nullable().Index(indexPrefix + "_CUSTOM_REGION_ID");
            Map(m => m.PatientZipCode).Length(5).Index(indexPrefix + "_PATIENT_ZIP_CODE");

            Map(m => m.ICDCodeType)
                .CustomType<ICDCodeTypeEnum>()
                .Nullable()
                .Index(indexPrefix + "_ICDCODETYPE");
        }
    }
}
