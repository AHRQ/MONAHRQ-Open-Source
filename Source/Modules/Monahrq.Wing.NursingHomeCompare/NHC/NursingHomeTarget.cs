using System;
using System.ComponentModel.DataAnnotations;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;

namespace Monahrq.Wing.NursingHomeCompare.NHC
{
	/// <summary>
	/// DataModel for NursingHomeTarget.
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecord" />
	[Serializable, EntityTableName("Targets_NursingHomeTargets")]
    [WingTarget("Nursing Home Compare Data", "13751D1A-4DCC-451E-B4B0-8A4A8714DA76", "Mapping target for Nursing home Compare Data", false, 5,
        PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
        PublisherEmail = "moanhrq@ahrq.gov",
        PublisherWebsite = "http://monahrq.ahrq.gov/")]
    public class NursingHomeTarget : DatasetRecord
    {
        public NursingHomeTarget()
        {}

        #region Properties
        /// <summary>
        /// Gets or sets the provider id.
        /// </summary>
        /// <value>
        /// The provider number.
        /// </value>
        [StringLength(12), WingTargetElement("provnum", false, 2)]
        public virtual string ProviderId { get; set; }
        /// <summary>
        /// Gets or sets the file date version.
        /// </summary>
        /// <value>
        /// The file version.
        /// </value>
        public virtual DateTime? FileDate { get; set; }
        /// <summary>
        /// Gets or sets the type of the facility.
        /// </summary>
        /// <value>
        /// The type of the facility.
        /// </value>
        /// public virtual FacilityTypeEnum? FacilityType { get; set; }
        /// <summary>
        /// Gets or sets the size of the bed.
        /// </summary>
        /// <value>
        /// The size of the bed.
        /// </value>
        /// public virtual BedSizeEnum? BedSize { get; set; }
        /// <summary>
        /// Gets or sets the size of the resource.
        /// </summary>
        /// <value>
        /// The size of the resource.
        /// </value>
        /// public virtual ResSizeEnum? ResSize { get; set; }

        /// <summary>
        /// Gets or sets the overall rating.
        /// </summary>
        /// <value>
        /// The overall rating.
        /// </value>
        public virtual int? OverallRating { get; set; }
        /// <summary>
        /// Gets or sets the overall rating function.
        /// </summary>
        /// <value>
        /// The overall rating function.
        /// </value>
   ///     public virtual FunctionEnum? OverallRatingFn { get; set; }
        /// <summary>
        /// Gets or sets the survey rating.
        /// </summary>
        /// <value>
        /// The survey rating.
        /// </value>
        public virtual int? SurveyRating { get; set; }
        /// <summary>
        /// Gets or sets the survey rating function.
        /// </summary>
        /// <value>
        /// The survey rating function.
        /// </value>
       /// public virtual FunctionEnum? SurveyRatingFn { get; set; }
        /// <summary>
        /// Gets or sets the quality rating.
        /// </summary>
        /// <value>
        /// The quality rating.
        /// </value>
        public virtual int? QualityRating { get; set; }
        /// <summary>
        /// Gets or sets the quality rating function.
        /// </summary>
        /// <value>
        /// The quality rating function.
        /// </value>
      ///  public virtual FunctionEnum? QualityRatingFn { get; set; }
        /// <summary>
        /// Gets or sets the staffing rating.
        /// </summary>
        /// <value>
        /// The staffing rating.
        /// </value>
        public virtual int? StaffingRating { get; set; }
        /// <summary>
        /// Gets or sets the staffing rating function.
        /// </summary>
        /// <value>
        /// The staffing rating function.
        /// </value>
        /// public virtual FunctionEnum? StaffingRatingFn { get; set; }
        /// <summary>
        /// Gets or sets the rn staffing rating.
        /// </summary>
        /// <value>
        /// The rn staffing rating.
        /// </value>
        /// public virtual int? RNStaffingRating { get; set; }
        /// <summary>
        /// Gets or sets the rn staffing rating function.
        /// </summary>
        /// <value>
        /// The rn staffing rating function.
        /// </value>
        /// public virtual FunctionEnum? RNStaffingRatingFn { get; set; }
        /// <summary>
        /// Gets or sets the staffing flag.
        /// </summary>
        /// <value>
        /// The staffing flag.
        /// </value>
        /// public virtual string StaffingFlag { get; set; }
        /// <summary>
        /// Gets or sets the pt staffing flag.
        /// </summary>
        /// <value>
        /// The pt staffing flag.
        /// </value>
        /// public virtual string PTStaffingFlag { get; set; }
        /// <summary>
        /// Gets or sets the aidhrd state average. 
        /// The CNA staffing level state average
        /// </summary>
        /// <value>
        /// The aidhrd state average of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? AIDHRDStateAvg { get; set; }
        /// <summary>
        /// Gets or sets the VOCHRD state average. 
        /// The LPN staffing level state average
        /// </summary>
        /// <value>
        /// The VOCHRD state average of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? VOCHRDStateAvg { get; set; }
        /// <summary>
        /// Gets or sets the RNHRD state average. 
        /// The RN staffing level state average
        /// </summary>
        /// <value>
        /// The RNHRD state average of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? RNHRDStateAvg { get; set; }
        /// <summary>
        /// Gets or sets the TOTLICHRD state average. 
        /// All licensed professional staffing level state average
        /// </summary>
        /// <value>
        /// The TOTLICHRD state average of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? TOTLICHRDStateAvg { get; set; }
        /// <summary>
        /// Gets or sets the TOTHRD state average. 
        /// All nursing staffing level state average
        /// </summary>
        /// <value>
        /// The TOTHRD state average of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? TOTHRDStateAvg { get; set; }
        /// <summary>
        /// Gets or sets the PTHRD state average. 
        /// All Physical Therapist staffing level state average
        /// </summary>
        /// <value>
        /// The PTHRD state average of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? PTHRDStateAvg { get; set; }
        /// <summary>
        /// Gets or sets the ExpAide. 
        /// </summary>
        /// <value>
        /// The ExpAide of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? ExpAide { get; set; }
        /// <summary>
        /// Gets or sets the ExpLpn. 
        /// </summary>
        /// <value>
        /// The ExpLpn of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? ExpLpn { get; set; }
        /// <summary>
        /// Gets or sets the ExpRn. 
        /// </summary>
        /// <value>
        /// The ExpRn of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? ExpRn { get; set; }
        /// <summary>
        /// Gets or sets the ExpTotal. 
        /// </summary>
        /// <value>
        /// The ExpTotal of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? ExpTotal { get; set; }
        /// <summary>
        /// Gets or sets the AdjAide. 
        /// </summary>
        /// <value>
        /// The AdjAide of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? AdjAide { get; set; }
        /// <summary>
        /// Gets or sets the AdjLpn. 
        /// </summary>
        /// <value>
        /// The AdjLpn of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? AdjLpn { get; set; }
        /// <summary>
        /// Gets or sets the AdjRn. 
        /// </summary>
        /// <value>
        /// The AdjRn of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? AdjRn { get; set; }
        /// <summary>
        /// Gets or sets the AdjTotal. 
        /// </summary>
        /// <value>
        /// The AdjTotal of sql data type of decimal(19,5)
        /// </value>
        /// public virtual decimal? AdjTotal { get; set; }
        
        #region Cycle 1

        /// <summary>
        /// Gets or sets the cycle1 defs.
        /// </summary>
        /// <value>
        /// Maps to cycle_1_defs
        /// </value>
        /// public virtual int? Cycle1Defs { get; set; }
        /// <summary>
        /// Gets or sets the cycle1 N defs.
        /// </summary>
        /// <value>
        /// Maps to cycle_1_nfromdefs.
        /// </value>
        /// public virtual int? Cycle1NFromDefs { get; set; }
        /// <summary>
        /// Gets or sets the cycle 1 N from Comp.
        /// </summary>
        /// <value>
        /// Maps to cycle_1_nfromcomp.
        /// </value>
        /// public virtual int? Cycle1NFromComp { get; set; }
        /// <summary>
        /// Gets or sets the cycle1 defs score.
        /// </summary>
        /// <value>
        /// Maps to cycle_1_defs_score.
        /// </value>
        /// public virtual int? Cycle1DefsScore { get; set; }

        /// <summary>
        /// Gets or sets the cycle1 survey date.
        /// </summary>
        /// <value>
        /// Maps to CYCLE_1_SURVEY_DATE.
        /// </value>
        /// public virtual DateTime? Cycle1SurveyDate { get; set; }
        /// <summary>
        /// Gets or sets the cycle1 number revis.
        /// </summary>
        /// <value>
        /// Maps to CYCLE_1_NUMREVIS.
        /// </value>
        /// public virtual int? Cycle1NumRevis { get; set; }
        /// <summary>
        /// Gets or sets the cycle1 revisit score.
        /// </summary>
        /// <value>
        /// Maps to CYCLE_1_REVISIT_SCORE.
        /// </value>
        /// public virtual int? Cycle1RevisitScore { get; set; }
        /// <summary>
        /// Gets or sets the cycle1 total score.
        /// </summary>
        /// <value>
        /// Maps to CYCLE_1_TOTAL_SCORE.
        /// </value>
        /// public virtual int? Cycle1TotalScore { get; set; }
        #endregion

        #region Cycle 2

        /// <summary>
        /// Gets or sets the cycle2 defs.
        /// </summary>
        /// <value>
        /// Maps to cycle_2_defs
        /// </value>
        /// public virtual int? Cycle2Defs { get; set; }
        /// <summary>
        /// Gets or sets the cycle2 N defs.
        /// </summary>
        /// <value>
        /// Maps to cycle_2_nfromdefs.
        /// </value>
        /// public virtual int? Cycle2NFromDefs { get; set; }
        /// <summary>
        /// Gets or sets the cycle 2 N from Comp.
        /// </summary>
        /// <value>
        /// Maps to cycle_2_nfromcomp.
        /// </value>
        /// public virtual int? Cycle2NFromComp { get; set; }
        /// <summary>
        /// Gets or sets the cycle2 defs score.
        /// </summary>
        /// <value>
        /// Maps to cycle_2_defs_score.
        /// </value>
        /// public virtual int? Cycle2DefsScore { get; set; }

        /// <summary>
        /// Gets or sets the cycle2 survey date.
        /// </summary>
        /// <value>
        /// Maps to CYCLE_2_SURVEY_DATE.
        /// </value>
        /// public virtual DateTime? Cycle2SurveyDate { get; set; }
        /// <summary>
        /// Gets or sets the cycle2 number revis.
        /// </summary>
        /// <value>
        /// Maps to CYCLE_2_NUMREVIS.
        /// </value>
        /// public virtual int? Cycle2NumRevis { get; set; }
        /// <summary>
        /// Gets or sets the cycle2 revisit score.
        /// </summary>
        /// <value>
        /// Maps to CYCLE_2_REVISIT_SCORE.
        /// </value>
        /// public virtual int? Cycle2RevisitScore { get; set; }
        /// <summary>
        /// Gets or sets the cycle2 total score.
        /// </summary>
        /// <value>
        /// Maps to CYCLE_2_TOTAL_SCORE.
        /// </value>
        /// public virtual int? Cycle2TotalScore { get; set; }
        #endregion

        #region Cycle 3

        /// <summary>
        /// Gets or sets the cycle3 defs.
        /// </summary>
        /// <value>
        /// Maps to cycle_3_defs
        /// </value>
        /// public virtual int? Cycle3Defs { get; set; }
        /// <summary>
        /// Gets or sets the cycle3 N defs.
        /// </summary>
        /// <value>
        /// Maps to cycle_3_nfromdefs.
        /// </value>
        /// public virtual int? Cycle3NFromDefs { get; set; }
        /// <summary>
        /// Gets or sets the cycle 3 N from Comp.
        /// </summary>
        /// <value>
        /// Maps to cycle_3_nfromcomp.
        /// </value>
        /// public virtual int? Cycle3NFromComp { get; set; }
        /// <summary>
        /// Gets or sets the cycle3 defs score.
        /// </summary>
        /// <value>
        /// Maps to cycle_3_defs_score.
        /// </value>
        /// public virtual int? Cycle3DefsScore { get; set; }

        /// <summary>
        /// Gets or sets the cycle3 survey date.
        /// </summary>
        /// <value>
        /// Maps to CYCLE_3_SURVEY_DATE.
        /// </value>
        /// public virtual DateTime? Cycle3SurveyDate { get; set; }
        /// <summary>
        /// Gets or sets the cycle3 number revis.
        /// </summary>
        /// <value>
        /// Maps to CYCLE_3_NUMREVIS.
        /// </value>
        /// public virtual int? Cycle3NumRevis { get; set; }
        /// <summary>
        /// Gets or sets the cycle3 revisit score.
        /// </summary>
        /// <value>
        /// Maps to CYCLE_3_REVISIT_SCORE.
        /// </value>
        /// public virtual int? Cycle3RevisitScore { get; set; }
        /// <summary>
        /// Gets or sets the cycle3 total score.
        /// </summary>
        /// <value>
        /// Maps to CYCLE_3_TOTAL_SCORE.
        /// </value>
        /// public virtual int? Cycle3TotalScore { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the weighted all cycles score.
        /// </summary>
        /// <value>
        /// Maps to WEIGHTED_ALL_CYCLES_SCORE with sql type string
        /// </value>
        public virtual string SurveyScore { get; set; }
        /// <summary>
        /// Gets or sets the incident count.
        /// </summary>
        /// <value>
        /// Maps to incident_cnt.
        /// </value>
        /// public virtual int IncidentCnt { get; set; }
        /// <summary>
        /// Gets or sets the CMPLNT count.
        /// </summary>
        /// <value>
        /// Maps to cmplnt_cnt.
        /// </value>
        /// public virtual int CmplntCnt { get; set; }
        /// <summary>
        /// Gets or sets the count of fines national average.
        /// </summary>
        /// <value>
        /// The fine count nat average. Maps to FINE_CNT.
        /// </value>
        /// public virtual int? FineCntNatAvg { get; set; }
        /// <summary>
        /// Gets or sets the total dollar amount of fines national average.
        /// </summary>
        /// <value>
        /// The fine tot nat average. Maps to FINE_TOT.
        /// </value>
        /// public virtual int? FineTotNatAvg { get; set; }
        /// <summary>
        /// Gets or sets the payden count.
        /// </summary>
        /// <value>
        /// The payden count.Maps to PAYDEN_CNT.
        /// </value>
        /// public virtual int? PaydenCnt { get; set; }
        /// <summary>
        /// Gets or sets the tot penlty count.
        /// </summary>
        /// <value>
        /// The tot penlty count. Maps to TotPenltyCnt.
        /// </value>
        /// public virtual int? TotPenltyCnt { get; set; }

        #region QM Scores

        /// <summary>
        /// Gets or sets the QM401 score.
        /// </summary>
        /// <value>
        /// Maps to the QM401_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM401Score { get; set; }

        /// <summary>
        /// Gets or sets the QM402 score.
        /// </summary>
        /// <value>
        /// Maps to the QM402_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM402Score { get; set; }

        /// <summary>
        /// Gets or sets the QM404 score.
        /// </summary>
        /// <value>
        /// Maps to the QM404_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM403Score { get; set; }

        /// <summary>
        /// Gets or sets the QM404 score.
        /// </summary>
        /// <value>
        /// Maps to the QM404_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM404Score { get; set; }

        /// <summary>
        /// Gets or sets the QM405 score.
        /// </summary>
        /// <value>
        /// Maps to the QM405_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM405Score { get; set; }
        /// <summary>
        /// Gets or sets the QM406 score.
        /// </summary>
        /// <value>
        /// Maps to the QM406_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM406Score { get; set; }
        /// <summary>
        /// Gets or sets the QM407 score.
        /// </summary>
        /// <value>
        /// Maps to the QM407_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM407Score { get; set; }
        /// <summary>
        /// Gets or sets the QM408 score.
        /// </summary>
        /// <value>
        /// Maps to the QM408_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM408Score { get; set; }
        /// <summary>
        /// Gets or sets the QM409 score.
        /// </summary>
        /// <value>
        /// Maps to the QM409_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM409Score { get; set; }
        /// <summary>
        /// Gets or sets the QM410 score.
        /// </summary>
        /// <value>
        /// Maps to the QM410_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM410Score { get; set; }
        /// <summary>
        /// Gets or sets the QM411 score.
        /// </summary>
        /// <value>
        /// Maps to the QM411_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM411Score { get; set; }
        /// <summary>
        /// Gets or sets the QM415 score.
        /// </summary>
        /// <value>
        /// Maps to the QM415_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM415Score { get; set; }
        /// <summary>
        /// Gets or sets the QM419 score.
        /// </summary>
        /// <value>
        /// Maps to the QM419_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM419Score { get; set; }
        /// <summary>
        /// Gets or sets the QM424 score.
        /// </summary>
        /// <value>
        /// Maps to the QM424_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM424Score { get; set; }
        /// <summary>
        /// Gets or sets the QM425 score.
        /// </summary>
        /// <value>
        /// Maps to the QM425_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM425Score { get; set; }
        /// <summary>
        /// Gets or sets the QM426 score.
        /// </summary>
        /// <value>
        /// Maps to the QM426_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM426Score { get; set; }
        /// <summary>
        /// Gets or sets the QM430 score.
        /// </summary>
        /// <value>
        /// Maps to the QM430_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM430Score { get; set; }
        /// <summary>
        /// Gets or sets the QM434 score.
        /// </summary>
        /// <value>
        /// Maps to the QM434_score with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM434Score { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public virtual decimal? QM471Score { get; set; }
		public virtual decimal? QM521Score { get; set; }
		public virtual decimal? QM522Score { get; set; }
		public virtual decimal? QM523Score { get; set; }
		public virtual decimal? QM451Score { get; set; }
		public virtual decimal? QM452Score { get; set; }
		#endregion

		#region QM State

		/// <summary>
		/// Gets or sets the QM401 State.
		/// </summary>
		/// <value>
		/// Maps to the QM401_state with sql type decimal(19,1).
		/// </value>
		public virtual decimal? QM401State { get; set; }

        /// <summary>
        /// Gets or sets the QM402 State.
        /// </summary>
        /// <value>
        /// Maps to the QM402_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM402State { get; set; }

        /// <summary>
        /// Gets or sets the QM404 score.
        /// </summary>
        /// <value>
        /// Maps to the QM404_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM403State { get; set; }

        /// <summary>
        /// Gets or sets the QM404 score.
        /// </summary>
        /// <value>
        /// Maps to the QM404_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM404State { get; set; }

        /// <summary>
        /// Gets or sets the QM405 score.
        /// </summary>
        /// <value>
        /// Maps to the QM405_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM405State { get; set; }
        /// <summary>
        /// Gets or sets the QM406 score.
        /// </summary>
        /// <value>
        /// Maps to the QM406_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM406State { get; set; }
        /// <summary>
        /// Gets or sets the QM407 score.
        /// </summary>
        /// <value>
        /// Maps to the QM407_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM407State { get; set; }
        /// <summary>
        /// Gets or sets the QM408 score.
        /// </summary>
        /// <value>
        /// Maps to the QM408_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM408State { get; set; }
        /// <summary>
        /// Gets or sets the QM409 score.
        /// </summary>
        /// <value>
        /// Maps to the QM409_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM409State { get; set; }
        /// <summary>
        /// Gets or sets the QM410 score.
        /// </summary>
        /// <value>
        /// Maps to the QM410_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM410State { get; set; }
        /// <summary>
        /// Gets or sets the QM411 score.
        /// </summary>
        /// <value>
        /// Maps to the QM411_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM411State { get; set; }
        /// <summary>
        /// Gets or sets the QM415 score.
        /// </summary>
        /// <value>
        /// Maps to the QM415_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM415State { get; set; }
        /// <summary>
        /// Gets or sets the QM419 score.
        /// </summary>
        /// <value>
        /// Maps to the QM419_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM419State { get; set; }
        /// <summary>
        /// Gets or sets the QM424 score.
        /// </summary>
        /// <value>
        /// Maps to the QM424_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM424State { get; set; }
        /// <summary>
        /// Gets or sets the QM425 score.
        /// </summary>
        /// <value>
        /// Maps to the QM425_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM425State { get; set; }
        /// <summary>
        /// Gets or sets the QM426 score.
        /// </summary>
        /// <value>
        /// Maps to the QM426_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM426State { get; set; }
        /// <summary>
        /// Gets or sets the QM430 score.
        /// </summary>
        /// <value>
        /// Maps to the QM430_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM430State { get; set; }
        /// <summary>
        /// Gets or sets the QM434 score.
        /// </summary>
        /// <value>
        /// Maps to the QM434_state with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM434State { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public virtual decimal? QM471State { get; set; }
		public virtual decimal? QM521State { get; set; }
		public virtual decimal? QM522State { get; set; }
		public virtual decimal? QM523State { get; set; }
		public virtual decimal? QM451State { get; set; }
		public virtual decimal? QM452State { get; set; }
		#endregion

		#region QM Nation

		/// <summary>
		/// Gets or sets the QM401 Nation.
		/// </summary>
		/// <value>
		/// Maps to the QM401_nation with sql type decimal(19,1).
		/// </value>
		public virtual decimal? QM401Nation { get; set; }

        /// <summary>
        /// Gets or sets the QM402 Nation.
        /// </summary>
        /// <value>
        /// Maps to the QM402_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM402Nation { get; set; }

        /// <summary>
        /// Gets or sets the QM404 score.
        /// </summary>
        /// <value>
        /// Maps to the QM404_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM403Nation { get; set; }

        /// <summary>
        /// Gets or sets the QM404 score.
        /// </summary>
        /// <value>
        /// Maps to the QM404_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM404Nation { get; set; }

        /// <summary>
        /// Gets or sets the QM405 score.
        /// </summary>
        /// <value>
        /// Maps to the QM405_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM405Nation { get; set; }
        /// <summary>
        /// Gets or sets the QM406 score.
        /// </summary>
        /// <value>
        /// Maps to the QM406_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM406Nation { get; set; }
        /// <summary>
        /// Gets or sets the QM407 score.
        /// </summary>
        /// <value>
        /// Maps to the QM407_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM407Nation { get; set; }
        /// <summary>
        /// Gets or sets the QM408 score.
        /// </summary>
        /// <value>
        /// Maps to the QM408_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM408Nation { get; set; }
        /// <summary>
        /// Gets or sets the QM409 score.
        /// </summary>
        /// <value>
        /// Maps to the QM409_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM409Nation { get; set; }
        /// <summary>
        /// Gets or sets the QM410 score.
        /// </summary>
        /// <value>
        /// Maps to the QM410_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM410Nation { get; set; }
        /// <summary>
        /// Gets or sets the QM411 score.
        /// </summary>
        /// <value>
        /// Maps to the QM411_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM411Nation { get; set; }
        /// <summary>
        /// Gets or sets the QM415 score.
        /// </summary>
        /// <value>
        /// Maps to the QM415_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM415Nation { get; set; }
        /// <summary>
        /// Gets or sets the QM419 score.
        /// </summary>
        /// <value>
        /// Maps to the QM419_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM419Nation { get; set; }
        /// <summary>
        /// Gets or sets the QM424 score.
        /// </summary>
        /// <value>
        /// Maps to the QM424_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM424Nation { get; set; }
        /// <summary>
        /// Gets or sets the QM425 score.
        /// </summary>
        /// <value>
        /// Maps to the QM425_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM425Nation { get; set; }
        /// <summary>
        /// Gets or sets the QM426 score.
        /// </summary>
        /// <value>
        /// Maps to the QM426_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM426Nation { get; set; }
        /// <summary>
        /// Gets or sets the QM430 score.
        /// </summary>
        /// <value>
        /// Maps to the QM430_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM430Nation { get; set; }
        /// <summary>
        /// Gets or sets the QM434 score.
        /// </summary>
        /// <value>
        /// Maps to the QM434_nation with sql type decimal(19,1).
        /// </value>
        public virtual decimal? QM434Nation { get; set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public virtual decimal? QM471Nation { get; set; }
		public virtual decimal? QM521Nation { get; set; }
		public virtual decimal? QM522Nation { get; set; }
		public virtual decimal? QM523Nation { get; set; }
		public virtual decimal? QM451Nation { get; set; }
		public virtual decimal? QM452Nation { get; set; }
		#endregion

		/// <summary>
		/// Gets or sets the StaffingScore score.
		/// </summary>
		/// <value>
		/// Maps to the StaffingScore with sql type nvarchar.
		/// </value>
		public virtual string StaffingScore { get; set; }



        /// <summary>
        /// Gets or sets the Most Severe Health Deficiency.
        /// </summary>
        /// <value>
        /// Most Severe Health Deficiency.
        /// </value>
        public virtual string MostSevereHealthDeficiency { get; set; }

        /// <summary>
        /// Gets or sets the Most Severe Fire Safety Deficiency.
        /// </summary>
        /// <value>
        /// Most Severe Fire Safety Deficiency.
        /// </value>
        public virtual string MostSevereFireSafetyDeficiency { get; set; }



        #region individual measures
        /// <summary>
        /// Gets or sets the Reported CNA Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Reported CNA Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string ReportedCNAStaffingHoursperResidentperDay { get; set; }

        /// <summary>
        /// Gets or sets the Reported LPNS Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Reported LPNS Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string ReportedLPNStaffingHoursperResidentperDay { get; set; }

        /// <summary>
        /// Gets or sets the Reported RN Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Reported RN Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string ReportedRNStaffingHoursperResidentperDay { get; set; }

        /// <summary>
        /// Gets or sets the Licensed Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Licensed Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string LicensedStaffingHoursperResidentperDay { get; set; }

        /// <summary>
        /// Gets or sets the Total Nurse Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Total Nurse Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string TotalNurseStaffingHoursperResidentperDay { get; set; }

        /// <summary>
        /// Gets or sets Physical Therapist Staffing Hours per Resident Per Day.
        /// </summary>
        /// <value>
        /// Maps to the Physical Therapist Staffing Hours per Resident Per Day with sql type string.
        /// </value>
        public virtual string PhysicalTherapistStaffingHoursperResidentPerDay { get; set; }
        /// <summary>
        /// Gets or sets the Health Survey Date.
        /// </summary>
        /// <value>
        /// The Health Survey Date.
        /// </value>
        public virtual DateTime? HealthSurveyDate { get; set; }

        /// <summary>
        /// Gets or sets the Fire Safety Survey Date
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public virtual DateTime? FireSafetySurveyDate { get; set; }

        /// <summary>
        /// Gets or sets the TotalHealthDeficiencies score.
        /// </summary>
        /// <value>
        /// Maps to the Total Health Deficiencies with sql type decimal(19,5).
        /// </value>
        public virtual decimal? TotalHealthDeficiencies { get; set; }

        /// <summary>
        /// Gets or sets the Total Fire Safety Deficiencies s.
        /// </summary>
        /// <value>
        /// Maps to the Total Fire Safety Deficiencies with sql type decimal(19,1).
        /// </value>
        public virtual decimal? TotalFireSafetyDeficiencies { get; set; }
        #endregion


        #region individual State measures
        /// <summary>
        /// Gets or sets the Reported CNA Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Reported CNA Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string ReportedCNAStaffingHoursperResidentperDayState { get; set; }

        /// <summary>
        /// Gets or sets the Reported LPNS Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Reported LPNS Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string ReportedLPNStaffingHoursperResidentperDayState { get; set; }

        /// <summary>
        /// Gets or sets the Reported RN Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Reported RN Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string ReportedRNStaffingHoursperResidentperDayState { get; set; }

        /// <summary>
        /// Gets or sets the Licensed Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Licensed Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string LicensedStaffingHoursperResidentperDayState { get; set; }

        /// <summary>
        /// Gets or sets the Total Nurse Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Total Nurse Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string TotalNurseStaffingHoursperResidentperDayState { get; set; }

        /// <summary>
        /// Gets or sets Physical Therapist Staffing Hours per Resident Per Day.
        /// </summary>
        /// <value>
        /// Maps to the Physical Therapist Staffing Hours per Resident Per Day with sql type string.
        /// </value>
        public virtual string PhysicalTherapistStaffingHoursperResidentPerDayState { get; set; }
        /// <summary>
        /// Gets or sets the Health Survey Date.
        /// </summary>
        /// <value>
        /// The Health Survey Date.
        /// </value>
        public virtual DateTime? HealthSurveyDateState { get; set; }

        /// <summary>
        /// Gets or sets the Fire Safety Survey Date
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public virtual DateTime? FireSafetySurveyDateState { get; set; }

        /// <summary>
        /// Gets or sets the TotalHealthDeficiencies score.
        /// </summary>
        /// <value>
        /// Maps to the Total Health Deficiencies with sql type decimal(19,5).
        /// </value>
        public virtual decimal? TotalHealthDeficienciesState { get; set; }

        /// <summary>
        /// Gets or sets the Total Fire Safety Deficiencies s.
        /// </summary>
        /// <value>
        /// Maps to the Total Fire Safety Deficiencies with sql type decimal(19,1).
        /// </value>
        public virtual decimal? TotalFireSafetyDeficienciesState { get; set; }
        #endregion

        #region individual Nation measures
        /// <summary>
        /// Gets or sets the Reported CNA Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Reported CNA Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string ReportedCNAStaffingHoursperResidentperDayNation { get; set; }

        /// <summary>
        /// Gets or sets the Reported LPNS Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Reported LPNS Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string ReportedLPNStaffingHoursperResidentperDayNation { get; set; }

        /// <summary>
        /// Gets or sets the Reported RN Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Reported RN Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string ReportedRNStaffingHoursperResidentperDayNation { get; set; }

        /// <summary>
        /// Gets or sets the Licensed Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Licensed Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string LicensedStaffingHoursperResidentperDayNation { get; set; }

        /// <summary>
        /// Gets or sets the Total Nurse Staffing Hours per Resident per Day.
        /// </summary>
        /// <value>
        /// Maps to the Total Nurse Staffing Hours per Resident per Day with sql type string.
        /// </value>
        public virtual string TotalNurseStaffingHoursperResidentperDayNation { get; set; }

        /// <summary>
        /// Gets or sets Physical Therapist Staffing Hours per Resident Per Day.
        /// </summary>
        /// <value>
        /// Maps to the Physical Therapist Staffing Hours per Resident Per Day with sql type string.
        /// </value>
        public virtual string PhysicalTherapistStaffingHoursperResidentPerDayNation { get; set; }
        /// <summary>
        /// Gets or sets the Health Survey Date.
        /// </summary>
        /// <value>
        /// The Health Survey Date.
        /// </value>
        public virtual DateTime? HealthSurveyDateNation { get; set; }

        /// <summary>
        /// Gets or sets the Fire Safety Survey Date
        /// </summary>
        /// <value>
        /// 
        /// </value>
        public virtual DateTime? FireSafetySurveyDateNation { get; set; }

        /// <summary>
        /// Gets or sets the TotalHealthDeficiencies score.
        /// </summary>
        /// <value>
        /// Maps to the Total Health Deficiencies with sql type decimal(19,5).
        /// </value>
        public virtual decimal? TotalHealthDeficienciesNation { get; set; }

        /// <summary>
        /// Gets or sets the Total Fire Safety Deficiencies s.
        /// </summary>
        /// <value>
        /// Maps to the Total Fire Safety Deficiencies with sql type decimal(19,1).
        /// </value>
        public virtual decimal? TotalFireSafetyDeficienciesNation { get; set; }
        #endregion

        #endregion
    }

	/// <summary>
	/// NHibernate Mapping for NursingHomeTarget.
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecordMap{Monahrq.Wing.NursingHomeCompare.NHC.NursingHomeTarget}" />
	public class NursingHomeTargetMap : DatasetRecordMap<NursingHomeTarget>
    {
        public NursingHomeTargetMap()
        {
            var indexName = string.Format("IDX_{0}", EntityTableName);

            Map(x => x.ProviderId).Length(12).Index(indexName);
        // Map(x => x.FacilityType).CustomSqlType("int").Index(indexName);
        // Map(x => x.BedSize).CustomType<EnumType<BedSizeEnum>>().Index(indexName);
        // Map(x => x.ResSize).CustomType<EnumType<ResSizeEnum>>().Index(indexName);
            Map(x => x.OverallRating).CustomSqlType("int").Nullable();
        // Map(x => x.OverallRatingFn).CustomType<EnumType<FunctionEnum>>().Nullable().Index(indexName);
            Map(x => x.SurveyRating).CustomSqlType("int").Nullable();
        // Map(x => x.SurveyRatingFn).CustomType<EnumType<FunctionEnum>>().Nullable().Index(indexName);
            Map(x => x.QualityRating).CustomSqlType("int").Nullable();
        // Map(x => x.QualityRatingFn).CustomType<EnumType<FunctionEnum>>().Nullable().Index(indexName);
            Map(x => x.StaffingRating).CustomSqlType("int").Nullable();
        // Map(x => x.StaffingRatingFn).CustomType<EnumType<FunctionEnum>>().Nullable().Index(indexName);
        // Map(x => x.RNStaffingRating).CustomSqlType("int").Nullable();
        // Map(x => x.RNStaffingRatingFn).CustomType<EnumType<FunctionEnum>>().Nullable().Index(indexName);
        // Map(x => x.StaffingFlag).Length(50);
        // Map(x => x.PTStaffingFlag).Length(50);

        // Map(x => x.AIDHRDStateAvg).Scale(5).Nullable();
        // Map(x => x.VOCHRDStateAvg).Scale(5).Nullable();
        // Map(x => x.RNHRDStateAvg).Scale(5).Nullable();
        // Map(x => x.TOTLICHRDStateAvg).Scale(5).Nullable();
        // Map(x => x.TOTHRDStateAvg).Scale(5).Nullable();
        // Map(x => x.PTHRDStateAvg).Scale(5).Nullable();

       //  Map(x => x.ExpAide).Scale(5).Nullable();
       //  Map(x => x.ExpLpn).Scale(5).Nullable();
       //  Map(x => x.ExpRn).Scale(5).Nullable();
       //  Map(x => x.ExpTotal).Scale(5).Nullable();
       //  Map(x => x.AdjAide).Scale(5).Nullable();
       //  Map(x => x.AdjLpn).Scale(5).Nullable();
       //  Map(x => x.AdjRn).Scale(5).Nullable();
       //  Map(x => x.AdjTotal).Scale(5).Nullable();

            // Cycle 1
            //Map(x => x.Cycle1Defs).Nullable();
            //Map(x => x.Cycle1NFromDefs).Nullable();
            //Map(x => x.Cycle1NFromComp).Nullable();
            //Map(x => x.Cycle1DefsScore).Nullable();
            //Map(x => x.Cycle1SurveyDate).Nullable();
            //Map(x => x.Cycle1NumRevis).Nullable();
            //Map(x => x.Cycle1RevisitScore).Nullable();
            //Map(x => x.Cycle1TotalScore).Nullable();

            // Cycle 2
            //Map(x => x.Cycle2Defs).Nullable();
            //Map(x => x.Cycle2NFromDefs).Nullable();
            //Map(x => x.Cycle2NFromComp).Nullable();
            //Map(x => x.Cycle2DefsScore).Nullable();
            //Map(x => x.Cycle2SurveyDate).Nullable();
            //Map(x => x.Cycle2NumRevis).Nullable();
            //Map(x => x.Cycle2RevisitScore).Nullable();
            //Map(x => x.Cycle2TotalScore).Nullable();

            // Cycle 3
            //Map(x => x.Cycle3Defs).Nullable();
            //Map(x => x.Cycle3NFromDefs).Nullable();
            //Map(x => x.Cycle3NFromComp).Nullable();
            //Map(x => x.Cycle3DefsScore).Nullable();
            //Map(x => x.Cycle3SurveyDate).Nullable();
            //Map(x => x.Cycle3NumRevis).Nullable();
            //Map(x => x.Cycle3RevisitScore).Nullable();
            //Map(x => x.Cycle3TotalScore).Nullable();

            Map(x => x.SurveyScore).Length(12).Index(indexName);

            // Map(x => x.IncidentCnt).Default("0");
            // Map(x => x.CmplntCnt).Default("0");
            // Map(x => x.FineCntNatAvg).Nullable();
            // Map(x => x.FineTotNatAvg).Nullable();
            // Map(x => x.PaydenCnt).Nullable();
            // Map(x => x.TotPenltyCnt).Nullable();

            Map(x => x.QM401Score).Scale(1).Nullable();
            Map(x => x.QM402Score).Scale(1).Nullable();
            Map(x => x.QM403Score).Scale(1).Nullable();
            Map(x => x.QM404Score).Scale(1).Nullable();
            Map(x => x.QM405Score).Scale(1).Nullable();
            Map(x => x.QM406Score).Scale(1).Nullable();
            Map(x => x.QM407Score).Scale(1).Nullable();
            Map(x => x.QM408Score).Scale(1).Nullable();
            Map(x => x.QM409Score).Scale(1).Nullable();
            Map(x => x.QM410Score).Scale(1).Nullable();
            Map(x => x.QM411Score).Scale(1).Nullable();
            Map(x => x.QM415Score).Scale(1).Nullable();
            Map(x => x.QM419Score).Scale(1).Nullable();
            Map(x => x.QM424Score).Scale(1).Nullable();
            Map(x => x.QM425Score).Scale(1).Nullable();
            Map(x => x.QM426Score).Scale(1).Nullable();
            Map(x => x.QM430Score).Scale(1).Nullable();
            Map(x => x.QM434Score).Scale(1).Nullable();

			Map(x => x.QM471Score).Scale(1).Nullable();
			Map(x => x.QM521Score).Scale(1).Nullable();
			Map(x => x.QM522Score).Scale(1).Nullable();
			Map(x => x.QM523Score).Scale(1).Nullable();
			Map(x => x.QM451Score).Scale(1).Nullable();
			Map(x => x.QM452Score).Scale(1).Nullable();

			Map(x => x.QM401State).Scale(1).Nullable();
            Map(x => x.QM402State).Scale(1).Nullable();
            Map(x => x.QM403State).Scale(1).Nullable();
            Map(x => x.QM404State).Scale(1).Nullable();
            Map(x => x.QM405State).Scale(1).Nullable();
            Map(x => x.QM406State).Scale(1).Nullable();
            Map(x => x.QM407State).Scale(1).Nullable();
            Map(x => x.QM408State).Scale(1).Nullable();
            Map(x => x.QM409State).Scale(1).Nullable();
            Map(x => x.QM410State).Scale(1).Nullable();
            Map(x => x.QM411State).Scale(1).Nullable();
            Map(x => x.QM415State).Scale(1).Nullable();
            Map(x => x.QM419State).Scale(1).Nullable();
            Map(x => x.QM424State).Scale(1).Nullable();
            Map(x => x.QM425State).Scale(1).Nullable();
            Map(x => x.QM426State).Scale(1).Nullable();
            Map(x => x.QM430State).Scale(1).Nullable();
            Map(x => x.QM434State).Scale(1).Nullable();

			Map(x => x.QM471State).Scale(1).Nullable();
			Map(x => x.QM521State).Scale(1).Nullable();
			Map(x => x.QM522State).Scale(1).Nullable();
			Map(x => x.QM523State).Scale(1).Nullable();
			Map(x => x.QM451State).Scale(1).Nullable();
			Map(x => x.QM452State).Scale(1).Nullable();

			Map(x => x.QM401Nation).Scale(1).Nullable();
            Map(x => x.QM402Nation).Scale(1).Nullable();
            Map(x => x.QM403Nation).Scale(1).Nullable();
            Map(x => x.QM404Nation).Scale(1).Nullable();
            Map(x => x.QM405Nation).Scale(1).Nullable();
            Map(x => x.QM406Nation).Scale(1).Nullable();
            Map(x => x.QM407Nation).Scale(1).Nullable();
            Map(x => x.QM408Nation).Scale(1).Nullable();
            Map(x => x.QM409Nation).Scale(1).Nullable();
            Map(x => x.QM410Nation).Scale(1).Nullable();
            Map(x => x.QM411Nation).Scale(1).Nullable();
            Map(x => x.QM415Nation).Scale(1).Nullable();
            Map(x => x.QM419Nation).Scale(1).Nullable();
            Map(x => x.QM424Nation).Scale(1).Nullable();
            Map(x => x.QM425Nation).Scale(1).Nullable();
            Map(x => x.QM426Nation).Scale(1).Nullable();
            Map(x => x.QM430Nation).Scale(1).Nullable();
            Map(x => x.QM434Nation).Scale(1).Nullable();

			Map(x => x.QM471Nation).Scale(1).Nullable();
			Map(x => x.QM521Nation).Scale(1).Nullable();
			Map(x => x.QM522Nation).Scale(1).Nullable();
			Map(x => x.QM523Nation).Scale(1).Nullable();
			Map(x => x.QM451Nation).Scale(1).Nullable();
			Map(x => x.QM452Nation).Scale(1).Nullable();

			Map(x => x.StaffingScore).Length(12).Index(indexName);

            Map(x => x.MostSevereHealthDeficiency).Length(12).Index(indexName);
            Map(x => x.MostSevereFireSafetyDeficiency).Length(12).Index(indexName);
            Map(x => x.FileDate).Nullable();
            

            Map(x => x.ReportedCNAStaffingHoursperResidentperDay).Length(12).Index(indexName);
            Map(x => x.ReportedLPNStaffingHoursperResidentperDay).Length(12).Index(indexName);
            Map(x => x.ReportedRNStaffingHoursperResidentperDay).Length(12).Index(indexName);
            Map(x => x.LicensedStaffingHoursperResidentperDay).Length(12).Index(indexName);
            Map(x => x.TotalNurseStaffingHoursperResidentperDay).Length(12).Index(indexName);
            Map(x => x.PhysicalTherapistStaffingHoursperResidentPerDay).Length(12).Index(indexName);
            Map(x => x.HealthSurveyDate).Nullable();
            Map(x => x.FireSafetySurveyDate).Nullable();
            Map(x => x.TotalHealthDeficiencies).Scale(1).Nullable();
            Map(x => x.TotalFireSafetyDeficiencies).Scale(1).Nullable();

            Map(x => x.ReportedCNAStaffingHoursperResidentperDayState).Length(12).Index(indexName);
            Map(x => x.ReportedLPNStaffingHoursperResidentperDayState).Length(12).Index(indexName);
            Map(x => x.ReportedRNStaffingHoursperResidentperDayState).Length(12).Index(indexName);
            Map(x => x.LicensedStaffingHoursperResidentperDayState).Length(12).Index(indexName);
            Map(x => x.TotalNurseStaffingHoursperResidentperDayState).Length(12).Index(indexName);
            Map(x => x.PhysicalTherapistStaffingHoursperResidentPerDayState).Length(12).Index(indexName);
            Map(x => x.HealthSurveyDateState).Nullable();
            Map(x => x.FireSafetySurveyDateState).Nullable();
            Map(x => x.TotalHealthDeficienciesState).Scale(1).Nullable();
            Map(x => x.TotalFireSafetyDeficienciesState).Scale(1).Nullable();

            Map(x => x.ReportedCNAStaffingHoursperResidentperDayNation).Length(12).Index(indexName);
            Map(x => x.ReportedLPNStaffingHoursperResidentperDayNation).Length(12).Index(indexName);
            Map(x => x.ReportedRNStaffingHoursperResidentperDayNation).Length(12).Index(indexName);
            Map(x => x.LicensedStaffingHoursperResidentperDayNation).Length(12).Index(indexName);
            Map(x => x.TotalNurseStaffingHoursperResidentperDayNation).Length(12).Index(indexName);
            Map(x => x.PhysicalTherapistStaffingHoursperResidentPerDayNation).Length(12).Index(indexName);
            Map(x => x.HealthSurveyDateNation).Nullable();
            Map(x => x.FireSafetySurveyDateNation).Nullable();
            Map(x => x.TotalHealthDeficienciesNation).Scale(1).Nullable();
            Map(x => x.TotalFireSafetyDeficienciesNation).Scale(1).Nullable();

        }
    }
}