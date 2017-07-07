using System;
using System.ComponentModel.DataAnnotations;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Wings;

namespace Monahrq.Wing.NursingHomeCompare.NHCAHPS
{
    [Serializable, EntityTableName("Targets_NHCAHPSSurveyTargets")]
    [WingTarget("NH-CAHPS Survey Results Data", Constants.WING_GUID, "Mapping target for NH-CAHPS Survey Results Data", false, false, 9,
        PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
        PublisherEmail = "moanhrq@ahrq.gov",
        PublisherWebsite = "http://monahrq.ahrq.gov/")]
    public class NHCAHPSSurveyTarget : DatasetRecord
    {
        #region Properties
        // Required

        /// <summary>
        /// Host User Assigned Respondent ID
        /// </summary>
        /// <value>
        /// The nhid.
        /// </value>
        [Required, 
         WingTargetElement("NHId", "NHId", true, 1, "Host User Assigned Respondent ID.")]
        public string NHId { get; set; }

        /// <summary>
        /// Nursing Home Provider ID. This should match the provider  ID’s for the Medical Practices contained in the Nursing Home Compare Database.
        /// </summary>
        /// <value>
        /// The provider identifier.
        /// </value>
        [Required,
         WingTargetElement("ProviderId", "ProviderId", true, 0, "Nursing Home CMS Provider Id. This should match the CMS provider Id’s for the Medical Practices contained in the Nursing Home Compare Database.")]
        public string ProviderId { get; set; }
        
        /// <summary>
        /// Practice Sample Size
        /// </summary>
        /// <value>
        /// The size of the practice sample.
        /// </value>
        [Required,
         WingTargetElement("PracticeSampleSize", "PracticeSampleSize", true, 2, "The practice sample size.")]
        public int PracticeSampleSize { get; set; }

        // Non Required
        
        /// <summary>
        /// Survey Completion Date
        /// </summary>
        /// <value>
        /// The survey completion date.
        /// </value>
        [DataType(DataType.Date)]
        [WingTargetElement("SurveyCompletionDate", "SurveyCompletionDate", false, 3, "The survey completion date e.g. 12/21/2014 (December 21, 2014), 03/21/2014 (March 21, 2014).")]
        public DateTime? SurveyCompletionDate { get; set; }

        /// <summary>
        /// Date of Last Visit (Month & Year) e.g. 122014 (December, 2014), 32014 (March, 2014)
        /// </summary>
        /// <value>
        /// The date of last visit.
        /// </value>
        [WingTargetElement("DateOfLastVisit", "DateOfLastVisit", false, 4, "Date of Last Visit e.g. 12/21/2014 (December 21, 2014), 03/21/2014 (March 21, 2014).")]
        public DateTime? DateOfLastVisit { get; set; }

        /// <summary>
        /// Wait too long for help with eating
        /// </summary>
        /// <value>
        /// The Q17.
        /// </value>
        [WingTargetElement("Q17", "Q17", false, 5, "Wait too long for help with eating.")]
        public YesNoEnum? Q17 { get; set; }

        /// <summary>
        /// Wait too long for help with drinking
        /// </summary>
        /// <value>
        /// The Q19.
        /// </value>
        [WingTargetElement("Q19", "Q19", false, 6, "Wait too long for help with drinking.")]
        public YesNoEnum? Q19 { get; set; }

        /// <summary>
        /// Wait too long for help with toileting
        /// </summary>
        /// <value>
        /// The Q21.
        /// </value>
        [WingTargetElement("Q21", "Q21", false, 7, "Wait too long for help with toileting.")]
        public YesNoEnum? Q21 { get; set; }

        /// <summary>
        /// Nurses/Aides treat resident with courtesy and respect
        /// </summary>
        /// <value>
        /// The Q12.
        /// </value>
        [WingTargetElement("Q12", "Q12", false, 8, "Nurses/Aides treat resident with courtesy and respect.")]
        public HowOftenEnum? Q12 { get; set; }

        /// <summary>
        /// Nurses/Aides treat resident with kindness
        /// </summary>
        /// <value>
        /// The Q13.
        /// </value>
        [WingTargetElement("Q13", "Q13", false, 9, "Nurses/Aides treat resident with kindness.")]
        public HowOftenEnum? Q13 { get; set; }

        /// <summary>
        /// Nurses/Aides really cared about resident
        /// </summary>
        /// <value>
        /// The Q14.
        /// </value>
        [WingTargetElement("Q14", "Q14", false, 10, "Nurses/Aides really cared about resident.")]
        public HowOftenEnum? Q14 { get; set; }

        /// <summary>
        /// Nurses/Aides appropriate with resident displaying behavioral problems
        /// </summary>
        /// <value>
        /// The Q24.
        /// </value>
        [WingTargetElement("Q24", "Q24", false, 11, "Nurses/Aides appropriate with resident displaying behavioral problems.")]
        public HowOftenEnum? Q24 { get; set; }

        /// <summary>
        /// Nurses/Aides rude to resident
        /// </summary>
        /// <value>
        /// The Q15.
        /// </value>
        [WingTargetElement("Q15", "Q15", false, 12, "Nurses/Aides rude to resident.")]
        public HowOftenEnum? Q15 { get; set; }

        /// <summary>
        /// Nurses/Aides give respondent timely information about resident
        /// </summary>
        /// <value>
        /// The Q26.
        /// </value>
        [WingTargetElement("Q26", "Q26", false, 13, "Nurses/Aides give respondent timely information about resident.")]
        public HowOftenEnum? Q26 { get; set; }

        /// <summary>
        /// Nurses/Aides explain things to respondent
        /// </summary>
        /// <value>
        /// The Q27.
        /// </value>
        [WingTargetElement("Q27", "Q27", false, 14, "Nurses/Aides explain things to respondent.")]
        public HowOftenEnum? Q27 { get; set; }

        /// <summary>
        /// Respondent involved in decisions about care
        /// </summary>
        /// <value>
        /// The Q37.
        /// </value>
        [WingTargetElement("Q37", "Q37", false, 15, "Respondent involved in decisions about care.")]
        public HowOftenEnum? Q37 { get; set; }

        /// <summary>
        /// Respondent given information about payments/expenses
        /// </summary>
        /// <value>
        /// The Q42.
        /// </value>
        [WingTargetElement("Q42", "Q42", false, 16, "Respondent given information about payments/expenses.")]
        public HowOftenEnum? Q42 { get; set; }

        /// <summary>
        /// Nurses/Aides discourage respondents questions
        /// </summary>
        /// <value>
        /// The Q28.
        /// </value>
        [WingTargetElement("Q28", "Q28", false, 17, "Nurses/Aides discourage respondents questions.")]
        public YesNoEnum? Q28 { get; set; }

        /// <summary>
        /// Respondent stops self from complaining
        /// </summary>
        /// <value>
        /// The Q35.
        /// </value>
        [WingTargetElement("Q35", "Q35", false, 18, "Respondent stops self from complaining.")]
        public YesNoEnum? Q35 { get; set; }

        /// <summary>
        /// Can find a nurse or aide
        /// </summary>
        /// <value>
        /// The Q11.
        /// </value>
        [WingTargetElement("Q11", "Q11", false, 19, "Can find a nurse or aide.")]
        public HowOftenEnum? Q11 { get; set; }

        /// <summary>
        /// Enough nurses/aides
        /// </summary>
        /// <value>
        /// The Q40.
        /// </value>
        [WingTargetElement("Q40", "Q40", false, 20, "Enough nurses/aides.")]
        public HowOftenEnum? Q40 { get; set; }

        /// <summary>
        /// Room looks/smells clean
        /// </summary>
        /// <value>
        /// The Q29.
        /// </value>
        [WingTargetElement("Q29", "Q29", false, 21, "Room looks/smells clean.")]
        public HowOftenEnum? Q29 { get; set; }

        /// <summary>
        /// Resident looks/smells clean
        /// </summary>
        /// <value>
        /// The Q22.
        /// </value>
        [WingTargetElement("Q22", "Q22", false, 22, "Resident looks/smells clean.")]
        public HowOftenEnum? Q22 { get; set; }

        /// <summary>
        /// Public areas look/smell clean
        /// </summary>
        /// <value>
        /// The Q30.
        /// </value>
        [WingTargetElement("Q30", "Q30", false, 23, "Public areas look/smell clean.")]
        public HowOftenEnum? Q30 { get; set; }

        /// <summary>
        /// Family member’s personal medical belongings lost
        /// </summary>
        /// <value>
        /// The Q31.
        /// </value>
        [WingTargetElement("Q31", "Q31", false, 24, "Family member’s personal medical belongings lost.")]
        public NumberOfTimesEnum? Q31 { get; set; }

        /// <summary>
        /// Family member’s clothes damaged or lost
        /// </summary>
        /// <value>
        /// The Q33.
        /// </value>
        [WingTargetElement("Q33", "Q33", false, 25, "Family member’s clothes damaged or lost.")]
        public NumberOfTimes2Enum? Q33 { get; set; }

        /// <summary>
        /// Overall Rating of Care at Nursing Home
        /// </summary>
        /// <value>
        /// The Q38.
        /// </value>
        [WingTargetElement("Q38", "Q38", false, 26, "Overall Rating of Care at Nursing Home.")]
        public Ratings2Enum? Q38 { get; set; }

        #endregion
    }

    public class NHCAHPSSurveyTargetMap : DatasetRecordMap<NHCAHPSSurveyTarget>
    {
        public NHCAHPSSurveyTargetMap()
        {
            var indexName = string.Format("IDX_{0}", EntityTableName);

            Map(x => x.ProviderId).Length(12).Not.Nullable().Index(indexName);
            Map(x => x.NHId).Length(10).Not.Nullable().Index(indexName);
            Map(x => x.PracticeSampleSize).Not.Nullable();
            Map(x => x.SurveyCompletionDate).Nullable();
            Map(x => x.DateOfLastVisit).Nullable();

            Map(x => x.Q11).CustomType<HowOftenEnum>().Nullable();
            Map(x => x.Q12).CustomType<HowOftenEnum>().Nullable();
            Map(x => x.Q13).CustomType<HowOftenEnum>().Nullable();
            Map(x => x.Q14).CustomType<HowOftenEnum>().Nullable();
            Map(x => x.Q15).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Q17).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Q19).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Q21).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Q22).CustomType<HowOftenEnum>().Nullable();
            Map(x => x.Q24).CustomType<HowOftenEnum>().Nullable();
            Map(x => x.Q26).CustomType<HowOftenEnum>().Nullable();
            Map(x => x.Q27).CustomType<HowOftenEnum>().Nullable();
            Map(x => x.Q28).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Q29).CustomType<HowOftenEnum>().Nullable();
            Map(x => x.Q30).CustomType<HowOftenEnum>().Nullable();
            Map(x => x.Q31).CustomType<NumberOfTimesEnum>().Nullable();
            Map(x => x.Q33).CustomType<NumberOfTimes2Enum>().Nullable();
            Map(x => x.Q35).CustomType<YesNoEnum>().Nullable();
            Map(x => x.Q37).CustomType<HowOftenEnum>().Nullable();
            Map(x => x.Q38).CustomType<Ratings2Enum>().Nullable();
            Map(x => x.Q40).CustomType<HowOftenEnum>().Nullable();
            Map(x => x.Q42).CustomType<HowOftenEnum>().Nullable();
        }
    }
}