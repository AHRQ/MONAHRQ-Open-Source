using System;
using System.ComponentModel.DataAnnotations;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Wings;
// ReSharper disable InconsistentNaming

namespace Monahrq.Wing.Physician.CGCAHPS
{
	/// <summary>
	/// Data model for CGCAHPS.
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecord" />
	[Serializable]
    [EntityTableName("Targets_CGCAHPSSurveyTargets")]
    [WingTarget("CG-CAHPS Survey Results Data", CGCAHPSConstants.WING_GUID,
                "Mapping target for CG-CAHPS Survey Results Data", false, false, 9,
        PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
        PublisherEmail = "moanhrq@ahrq.gov",
        PublisherWebsite = "http://monahrq.ahrq.gov/")]
    public class CGCAHPSSurveyTarget : DatasetRecord
    {
		#region Properties

		// Required
		/// <summary>
		/// Host User Assigned Unique Respondent ID
		/// </summary>
		/// <value>
		/// The cg identifier.
		/// </value>
		[Required,
         WingTargetElement("CGId", "CGId", true, 1, "Host User Assigned Respondent ID.")]
        public string CGId { get; set; }
		/// <summary>
		/// Host User Assigned Unique Practice Site ID. This should match the practice ID’s for the Medical Practices contained in the
		/// Physician Compare Database.
		/// </summary>
		/// <value>
		/// The cg practice identifier.
		/// </value>
		[Required,
         WingTargetElement("CGPracticeId", "CGPracticeId", true, 0, "Host User Assigned Practice Site ID. This should match the practice ID’s for the Medical Practices contained in the Physician Compare Database.")]
        public string CGPracticeId { get; set; }
		/// <summary>
		/// Adult Practice Sample Size
		/// </summary>
		/// <value>
		/// The size of the practice sample.
		/// </value>
		[Required,
         WingTargetElement("AdultPracticeSampleSize", "AdultPracticeSampleSize", true, 3, "The Adult Practice Sample Size.")]
        public int? AdultPracticeSampleSize { get; set; }

		/// <summary>
		/// Adult Practice Sample Size
		/// </summary>
		/// <value>
		/// The size of the practice sample.
		/// </value>
		[Required,
         WingTargetElement("ChildPracticeSampleSize", "ChildPracticeSampleSize", true, 4, "The Child Practice Sample Size.")]
        public int? ChildPracticeSampleSize { get; set; }

		// Non Required
		/// <summary>
		/// Gets or sets the survey completion date.
		/// </summary>
		/// <value>
		/// The survey completion date.
		/// </value>
		[WingTargetElement("SurveyCompletionDate", "SurveyCompletionDate", false, 5, "The Date the Survey was Completed e.g. 12/21/2014 (December 21, 2014), 03/21/2014 (March 21, 2014).")]
        public DateTime? SurveyCompletionDate { get; set; }
		/// <summary>
		/// The Physician's NPR ID
		/// </summary>
		/// <value>
		/// The NPR identifier.
		/// </value>
		[WingTargetElement("NPRId", "NPRId", false, 2, "The NPR ID of the Physician.")]
        public string NPRId { get; set; }
		/// <summary>
		/// Gets or sets the date of last visit.
		/// </summary>
		/// <value>
		/// The date of last visit.
		/// </value>
		[WingTargetElement("DateOfLastVisit", "DateOfLastVisit", false, 7, "The Date of Last Visit e.g. 12/21/2014 (December 21, 2014), 03/21/2014 (March 21, 2014).")]
        public DateTime? DateOfLastVisit { get; set; }
		/// <summary>
		/// Physician Gender (1: Male, 2: Female)
		/// </summary>
		/// <value>
		/// The physician gender.
		/// </value>
		[WingTargetElement("PhysicianGender", "PhysicianGender", false, 6, "The Physician Gender (1: Male, 2: Female).")]
        public GenderEnum? PhysicianGender { get; set; }
		/// <summary>
		/// Got appointment for urgent care as soon as needed
		/// </summary>
		/// <value>
		/// The Q17.
		/// </value>
		[WingTargetElement("AV_06", "AV_06", false,8, "Got appointment for urgent care as soon as needed.")]
        public HowOftenEnum? AV_06 { get; set; }
		/// <summary>
		/// Got appointment for check-up or routine care as soon as needed
		/// </summary>
		/// <value>
		/// The Q19.
		/// </value>
		[WingTargetElement("AV_08", "AV_08", false, 9, "Got appointment for check-up or routine care as soon as needed.")]
        public HowOftenEnum? AV_08 { get; set; }
		/// <summary>
		/// Got answer to phone question during regular office hours on same day
		/// </summary>
		/// <value>
		/// The Q21.
		/// </value>
		[WingTargetElement("AV_10", "AV_10", false, 10, "Got answer to phone question during regular office hours on same day.")]
        public HowOftenEnum? AV_10 { get; set; }
		/// <summary>
		/// Got answer to phone question after hours as soon as needed
		/// </summary>
		/// <value>
		/// The Q12.
		/// </value>
		[WingTargetElement("AV_12", "AV_12", false, 11, "Got answer to phone question after hours as soon as needed.")]
        public HowOftenEnum? AV_12 { get; set; }
		/// <summary>
		/// Saw provider within 15 minutes of appointment time
		/// </summary>
		/// <value>
		/// The Q13.
		/// </value>
		[WingTargetElement("AV_13", "AV_13", false, 12, "Saw provider within 15 minutes of appointment time.")]
        public HowOftenEnum? AV_13 { get; set; }
		/// <summary>
		/// Provider explained things clearly
		/// </summary>
		/// <value>
		/// The Q14.
		/// </value>
		[WingTargetElement("AV_16", "AV_16", false, 13, "Provider explained things clearly.")]
        public DefiniteEnum? AV_16 { get; set; }
		/// <summary>
		/// Provider listened carefully
		/// </summary>
		/// <value>
		/// The Q24.
		/// </value>
		[WingTargetElement("AV_17", "AV_17", false, 14, "Provider listened carefully.")]
        public DefiniteEnum? AV_17 { get; set; }
		/// <summary>
		/// Provider gave easy to understand information on health questions or concerns
		/// </summary>
		/// <value>
		/// The Q15.
		/// </value>
		[WingTargetElement("AV_19", "AV_19", false, 15, "Provider gave easy to understand information on health questions or concerns.")]
        public DefiniteEnum? AV_19 { get; set; }
		/// <summary>
		/// Provider knew important information about your medical history
		/// </summary>
		/// <value>
		/// The Q26.
		/// </value>
		[WingTargetElement("AV_20", "AV_20", false, 16, "Provider knew important information about your medical history.")]
        public DefiniteEnum? AV_20 { get; set; }
		/// <summary>
		/// Provider showed respect
		/// </summary>
		/// <value>
		/// The Q27.
		/// </value>
		[WingTargetElement("AV_21", "AV_21", false, 17, "Provider showed respect.")]
        public DefiniteEnum? AV_21 { get; set; }
		/// <summary>
		/// Provider spent enough time
		/// </summary>
		/// <value>
		/// The Q37.
		/// </value>
		[WingTargetElement("AV_22", "AV_22", false, 18, "Provider spent enough time.")]
        public DefiniteEnum? AV_22 { get; set; }
		/// <summary>
		/// Provider’s office followed up with test results
		/// </summary>
		/// <value>
		/// The Q42.
		/// </value>
		[WingTargetElement("AV_24", "AV_24", false, 19, "Provider’s office followed up with test results.")]
        public YesNoEnum? AV_24 { get; set; }
		/// <summary>
		/// Rating of provider
		/// </summary>
		/// <value>
		/// The Q28.
		/// </value>
		[WingTargetElement("AV_25", "AV_25", false, 20, "Rating of provider.")]
        public RateProviderEnum? AV_25 { get; set; }
		/// <summary>
		/// Would you recommend this provider's office
		/// </summary>
		/// <value>
		/// The Q35.
		/// </value>
		[WingTargetElement("AV_26", "AV_26", false, 21, "Would you recommend this provider's office.")]
        public DefiniteEnum? AV_26 { get; set; }
		/// <summary>
		/// Office staff was helpful
		/// </summary>
		/// <value>
		/// The Q11.
		/// </value>
		[WingTargetElement("AV_27", "AV_27", false, 22, "Office staff was helpful.")]
        public DefiniteEnum? AV_27 { get; set; }
		/// <summary>
		/// Office staff courteous and respectful
		/// </summary>
		/// <value>
		/// The Q40.
		/// </value>
		[WingTargetElement("AV_28", "AV_28", false, 23, "Office staff courteous and respectful.")]
        public DefiniteEnum? AV_28 { get; set; }
		/// <summary>
		/// Spoke with provider's office about child's learning ability
		/// </summary>
		/// <value>
		/// The Q29.
		/// </value>
		[WingTargetElement("Cd_38", "Cd_38", false, 24, "Spoke with provider's office about child's learning ability.")]
        public YesNoEnum? Cd_38 { get; set; }
		/// <summary>
		/// Spoke with provider's office about normal behaviors
		/// </summary>
		/// <value>
		/// The Q22.
		/// </value>
		[WingTargetElement("Cd_39", "Cd_39", false, 25, "Spoke with provider's office about normal behaviors.")]
        public YesNoEnum? Cd_39 { get; set; }
		/// <summary>
		/// Spoke with provider's office about child's growth
		/// </summary>
		/// <value>
		/// The Q30.
		/// </value>
		[WingTargetElement("Cd_40", "Cd_40", false, 26, "Spoke with provider's office about child's growth.")]
        public YesNoEnum? Cd_40 { get; set; }
		/// <summary>
		/// Spoke with provider's office about child's moods and emotions
		/// </summary>
		/// <value>
		/// The Q31.
		/// </value>
		[WingTargetElement("Cd_41", "Cd_41", false, 27, "Spoke with provider's office about child's moods and emotions.")]
        public YesNoEnum? Cd_41 { get; set; }
		/// <summary>
		/// Spoke with provider's office about ways to prevent injuries
		/// </summary>
		/// <value>
		/// The CD_42.
		/// </value>
		[WingTargetElement("Cd_42", "Cd_42", false, 28, "Spoke with provider's office about ways to prevent injuries.")]
        public YesNoEnum? Cd_42 { get; set; }
		/// <summary>
		/// Received information about preventing injuries
		/// </summary>
		/// <value>
		/// The CD_43.
		/// </value>
		[WingTargetElement("Cd_43", "Cd_43", false, 28, "Received information about preventing injuries.")]
        public YesNoEnum? Cd_43 { get; set; }
		/// <summary>
		/// Spoke with provider's office about time spent on the computer or watching TV
		/// </summary>
		/// <value>
		/// The Q33.
		/// </value>
		[WingTargetElement("Cd_44", "Cd_44", false, 30, "Spoke with provider's office about time spent on the computer or watching TV.")]
        public YesNoEnum? Cd_44 { get; set; }
		/// <summary>
		/// Spoke with provider's office about child's diet
		/// </summary>
		/// <value>
		/// The CD_45.
		/// </value>
		[WingTargetElement("Cd_45", "Cd_45", false, 31, "Spoke with provider's office about child's diet.")]
        public YesNoEnum? Cd_45 { get; set; }
		/// <summary>
		/// Spoke with provider's office about child's exercise
		/// </summary>
		/// <value>
		/// The CD_46.
		/// </value>
		[WingTargetElement("Cd_46", "Cd_46", false, 32, "Spoke with provider's office about child's exercise.")]
        public YesNoEnum? Cd_46 { get; set; }
		/// <summary>
		/// Spoke with provider's office about child's ability to get along with others
		/// </summary>
		/// <value>
		/// The Q38.
		/// </value>
		[WingTargetElement("Cd_47", "Cd_47", false, 33, "Spoke with provider's office about child's ability to get along with others.")]
        public YesNoEnum? Cd_47 { get; set; }
		/// <summary>
		/// Spoke with provider's office about household problems affecting the child
		/// </summary>
		/// <value>
		/// The CD_48.
		/// </value>
		[WingTargetElement("Cd_48", "Cd_48", false, 34, "Spoke with provider's office about household problems affecting the child.")]
        public YesNoEnum? Cd_48 { get; set; }

        #endregion
    }

	/// <summary>
	/// NHibername mapping files for CGCAHPS.
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecordMap{CGCAHPSSurveyTarget}" />
	public class CGCAHPSSurveyTargetMap : DatasetRecordMap<CGCAHPSSurveyTarget>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="CGCAHPSSurveyTargetMap"/> class.
		/// </summary>
		public CGCAHPSSurveyTargetMap()
        {
            var indexName = string.Format("IDX_{0}", EntityTableName);

            Map(x => x.CGPracticeId).Length(12).Not.Nullable().Index(indexName);
            Map(x => x.CGId, "CGId").Length(10).Not.Nullable().Index(indexName);
            Map(x => x.NPRId).Length(12).Nullable().Index(indexName);
            Map(x => x.AdultPracticeSampleSize).Not.Nullable();
            Map(x => x.ChildPracticeSampleSize).Not.Nullable();
            Map(x => x.PhysicianGender).CustomType<GenderEnum>().Nullable().Index(indexName);
            Map(x => x.SurveyCompletionDate).Nullable();
            Map(x => x.DateOfLastVisit).Nullable();

            Map(x => x.AV_06).CustomType<DefiniteEnum>().Nullable();
            Map(x => x.AV_08).CustomType<DefiniteEnum>().Nullable();
            Map(x => x.AV_10).CustomType<DefiniteEnum>().Nullable();
            Map(x => x.AV_12).CustomType<DefiniteEnum>().Nullable();
            Map(x => x.AV_13).CustomType<DefiniteEnum>().Nullable();
            Map(x => x.AV_16).CustomType<DefiniteEnum>().Nullable();
            Map(x => x.AV_17).CustomType<DefiniteEnum>().Nullable();
            Map(x => x.AV_19).CustomType<DefiniteEnum>().Nullable();
            Map(x => x.AV_20).CustomType<DefiniteEnum>().Nullable();
            Map(x => x.AV_21).CustomType<DefiniteEnum>().Nullable();
            Map(x => x.AV_22).CustomType<DefiniteEnum>().Nullable();
            Map(x => x.AV_24).CustomType<YesNoEnum>().Nullable();
            Map(x => x.AV_25).CustomType<RateProviderEnum>().Nullable();
            Map(x => x.AV_26).CustomType<DefiniteEnum>().Nullable();
            Map(x => x.AV_27).CustomType<DefiniteEnum>().Nullable();
            Map(x => x.AV_28).CustomType<DefiniteEnum>().Nullable();

            Map(x => x.Cd_38).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Cd_39).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Cd_40).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Cd_41).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Cd_42).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Cd_43).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Cd_44).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Cd_45).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Cd_46).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Cd_47).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Cd_48).CustomType<YesNoEnum>().Nullable();
        }
    }
}