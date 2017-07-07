using System;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Wing.Physician.HEDIS
{
	/// <summary>
	/// Data model for HEDIS.
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecord" />
	[Serializable]
    [EntityTableName("Targets_PhysicianHEDISTargets")]
    [WingTarget("Medical Practice HEDIS Measures Data", HEDISConstants.WING_GUID, "Mapping target for Medical Practice HEDIS Measures Data", false, 9,
        PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
        PublisherEmail = "moanhrq@ahrq.gov",
        PublisherWebsite = "http://monahrq.ahrq.gov/")]
    public class HEDISTarget : DatasetRecord
    {
		#region Properties

		/// <summary>
		/// Gets or sets the practice identifier.
		/// </summary>
		/// <value>
		/// The practice identifier.
		/// </value>
		public string MedicalPracticeId { get; set; }

		// Required
		/// <summary>
		/// Gets or sets the Physician’s NPI ID.
		/// </summary>
		/// <value>
		/// The phy npi.
		/// </value>
		public long? PhyNpi { get; set; }

		// Non Required
		/// <summary>
		/// Gets or sets the Physician Rate - Diabetes: HbA1c testing.
		/// </summary>
		/// <value>
		/// The diab hb a1c test.
		/// </value>
		public double? DiabHbA1CTest { get; set; }
		/// <summary>
		/// Gets or sets the Physician Rate - Diabetes: HbA1c Control &lt;8%.
		/// </summary>
		/// <value>
		/// The diab hb a1c control.
		/// </value>
		public double? DiabHbA1CControl { get; set; }
		/// <summary>
		/// Gets or sets the Physician Rate - Diabetes: Blood Pressure Control &lt;140/90.
		/// </summary>
		/// <value>
		/// The diab bp control.
		/// </value>
		public double? DiabBPControl { get; set; }
		/// <summary>
		/// Gets or sets the Physician Rate - Asthma: Asthma medication ratio.
		/// </summary>
		/// <value>
		/// The asth medication ratio.
		/// </value>
		public double? AsthMedicationRatio { get; set; }
		///// <summary>
		///// Gets or sets the Physician Rate - Cardiovascular Conditions: LDL-C Screening.
		///// </summary>
		///// <value>
		///// The card cond LDL c screening.
		///// </value>
		//public double? CardCondLdlCScreening { get; set; }
		///// <summary>
		///// Gets or sets the Physician Rate - Cardiovascular Conditions: LDL-C Control (&lt;100 mg/dL).
		///// </summary>
		///// <value>
		///// The card conditions LDL c control.
		///// </value>
		//public double? CardConditionsLdlCControl { get; set; }
		/// <summary>
		/// Gets or sets the Physician Rate - Hypertension: Blood Pressure Control &lt;140/90 (age 18-59).
		/// </summary>
		/// <value>
		/// The hyper bp control.
		/// </value>
		public double? HyperBPControl { get; set; }
		/// <summary>
		/// Gets or sets the Physician Rate - COPD: Use of spirometry testing in the assessment and diagnosis of COPD.
		/// </summary>
		/// <value>
		/// The copd.
		/// </value>
		public double? COPD { get; set; }
		/// <summary>
		/// Gets or sets the Physician Rate - State Average Rate - Diabetes: HbA1c testing.
		/// </summary>
		/// <value>
		/// The diab hb a1c test stavg.
		/// </value>
		public double? DiabHbA1CTestSTAVG { get; set; }
		/// <summary>
		/// Gets or sets the Physician Rate - State Average Rate - Diabetes: HbA1c Control &lt;8%.
		/// </summary>
		/// <value>
		/// The diab hb a1c control stavg.
		/// </value>
		public double? DiabHbA1CControlSTAVG { get; set; }
		/// <summary>
		/// Gets or sets the State Average Rate - Diabetes: Blood Pressure Control &lt;140/90.
		/// </summary>
		/// <value>
		/// The diab bp control stavg.
		/// </value>
		public double? DiabBPControlSTAVG { get; set; }
		/// <summary>
		/// Gets or sets the Asthma: Asthma medication ratio.
		/// </summary>
		/// <value>
		/// The asth medication ratio stavg.
		/// </value>
		public double? AsthMedicationRatioSTAVG { get; set; }
		///// <summary>
		///// Gets or sets the State Average Rate - Cardiovascular Conditions: LDL-C Screening.
		///// </summary>
		///// <value>
		///// The card cond LDL c screening stavg.
		///// </value>
		//public double? CardCondLdlCScreeningSTAVG { get; set; }
		///// <summary>
		///// Gets or sets the State Average Rate - Cardiovascular Conditions: LDL-C Control (&lt;100 mg/dL).
		///// </summary>
		///// <value>
		///// The card conditions LDL c control stavg.
		///// </value>
		//public double? CardConditionsLdlCControlSTAVG { get; set; }
		/// <summary>
		/// Gets or sets the State Average Rate - Hypertension: Blood Pressure Control &lt;140/90 (age 18-59)g.
		/// </summary>
		/// <value>
		/// The hyper bp control stavg.
		/// </value>
		public double? HyperBPControlSTAVG { get; set; }
		/// <summary>
		/// Gets or sets the State Average Rate - COPD: Use of spirometry testing in the assessment and diagnosis of COPD.
		/// </summary>
		/// <value>
		/// The copdstavg.
		/// </value>
		public double? COPDSTAVG { get; set; }

        #endregion
    }

	/// <summary>
	/// NHibername mapping files for HEDIS.
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecordMap{HEDISTarget}" />
	public class PhysicianHEDISTargetMap : DatasetRecordMap<HEDISTarget>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="PhysicianHEDISTargetMap"/> class.
		/// </summary>
		public PhysicianHEDISTargetMap()
        {
            var indexName = string.Format("IDX_{0}", EntityTableName);

            Map(x => x.MedicalPracticeId).Not.Nullable().Index(indexName);
            Map(x => x.PhyNpi).Nullable(); //.Index(indexName);

            Map(x => x.DiabHbA1CTest).Nullable();
            Map(x => x.DiabHbA1CControl).Nullable();
            Map(x => x.DiabBPControl).Nullable();
            Map(x => x.AsthMedicationRatio).Nullable();
            //Map(x => x.CardCondLdlCScreening).Nullable();
            //Map(x => x.CardConditionsLdlCControl).Nullable();
            Map(x => x.HyperBPControl).Nullable();
            Map(x => x.COPD).Nullable();
            Map(x => x.DiabHbA1CTestSTAVG).Nullable();
            Map(x => x.DiabHbA1CControlSTAVG).Nullable();
            Map(x => x.DiabBPControlSTAVG).Nullable();
            Map(x => x.AsthMedicationRatioSTAVG).Nullable();
            //Map(x => x.CardCondLdlCScreeningSTAVG).Nullable();
            //Map(x => x.CardConditionsLdlCControlSTAVG).Nullable();
            Map(x => x.HyperBPControlSTAVG).Nullable();
            Map(x => x.COPDSTAVG).Nullable();
        }

		/// <summary>
		/// Gets the name of the entity table.
		/// </summary>
		/// <value>
		/// The name of the entity table.
		/// </value>
		public override string EntityTableName
        {
            get { return typeof(HEDISTarget).EntityTableName(); }
        }
    }
}