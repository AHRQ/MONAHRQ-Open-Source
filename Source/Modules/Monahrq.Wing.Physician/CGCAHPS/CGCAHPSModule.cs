using System;
using System.ComponentModel.Composition;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Physician.CGCAHPS.Model;
using Monahrq.Infrastructure.Domain.Physicians;
using System.IO;
using Monahrq.Infrastructure.Domain.Common;

namespace Monahrq.Wing.Physician.CGCAHPS
{

	/// <summary>
	/// 
	/// </summary>
	static class CGCAHPSConstants
    {
		/// <summary>
		/// The wing unique identifier
		/// </summary>
		public const string WING_GUID = "45A53D1D-A296-4158-B496-B361EAF3109E";
		/// <summary>
		/// The wing unique identifier as unique identifier
		/// </summary>
		public static readonly Guid WingGuidAsGuid = Guid.Parse(WING_GUID);
    }

	/// <summary>
	/// Imports CGCAHPS Measures and topics base data.
	/// </summary>
	/// <seealso cref="Monahrq.Sdk.Modules.Wings.TargetedModuleWithMeasuresAndTopics{Monahrq.Wing.Physician.CGCAHPS.CGCAHPSSurveyTarget}" />
	[WingModule(typeof(CGCAHPSModule), CGCAHPSConstants.WING_GUID,
        "CG-CAHPS Survey Results Data", "Provides Services for CG-CAHPS Survey Results Data",
        DependsOnModuleNames = new[] {"Base Data"}, DisplayOrder = 7)]
    public class CGCAHPSModule : TargetedModuleWithMeasuresAndTopics<CGCAHPSSurveyTarget>
    {
		#region Properties.
		/// <summary>
		/// Gets or sets the logger.
		/// </summary>
		/// <value>
		/// The logger.
		/// </value>
		[Import(LogNames.Session)]
        protected ILogWriter Logger { get; set; }

		/// <summary>
		/// Gets or sets the measure SVC.
		/// </summary>
		/// <value>
		/// The measure SVC.
		/// </value>
		[Import]
        protected IMeasureService MeasureSvc { get; set; }

		/// <summary>
		/// Gets the measure file path.
		/// </summary>
		/// <value>
		/// The measure file path.
		/// </value>
		public override String MeasureFilePath
		{
			get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures", "MeasuresCGCAHPS.csv"); }
		}
		/// <summary>
		/// Gets the measure topic file path.
		/// </summary>
		/// <value>
		/// The measure topic file path.
		/// </value>
		public override String MeasureTopicFilePath
		{
			get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures", "MeasuresCGCAHPSTopics.csv"); }
		}
		#endregion

		#region Methods.
		#region Constructor/Initialize Methods.
		/// <summary>
		/// Called when [initialize].
		/// </summary>
		protected override void OnInitialize()
        {
            base.OnInitialize();
           // Subscribe();
        }
		#endregion

		#region Subscribe Method.
		/// <summary>
		/// Subscribes this instance.
		/// </summary>
		private void Subscribe()
        {
            Events.GetEvent<WizardStepsRequestEvent<DataTypeModel, Guid, int?>>()
                .Subscribe(args =>
                {
                    if (args.WingId == WingGUID)
                    {
                        args.WizardSteps = new WizardSteps(args.Data);
                    }
                });
        }
		#endregion

		#region Manage Measures/Topics Data Methods.
		/// <summary>
		/// Imports the measures.
		/// </summary>
		protected override void ImportMeasures()
        {
            try
            {
                MeasureSvc.ImportMeasures(TargetAttribute.Name, MeasureFilePath, typeof(PhysicianMeasure));
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

		/// <summary>
		/// Imports the measure topics.
		/// </summary>
		protected override void ImportMeasureTopics()
        {
            try
            {
                MeasureSvc.ImportMeasureTopicFile(TargetAttribute.Name, MeasureTopicFilePath, TopicTypeEnum.Physician);
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

		/// <summary>
		/// Called when [apply dataset hints].
		/// </summary>
		protected override void OnApplyDatasetHints()
        { 
            Target<CGCAHPSSurveyTarget>(target => target.CGPracticeId)
                        .ApplyMappingHints("CGPRACTICEID", "PRACTICEID", "CGPRACID", "CGPID", "CGPRACTICEID14", "CGPRACTICEID13");

            Target<CGCAHPSSurveyTarget>(target => target.NPRId)
                        .ApplyMappingHints("PHYNPRID", "PHY_NPR_ID", "NPRID", "NPR_ID");

            Target<CGCAHPSSurveyTarget>(target => target.CGId)
                       .ApplyMappingHints("CGID", "CG_ID", "CGID14", "CGID13");

            Target<CGCAHPSSurveyTarget>(target => target.DateOfLastVisit)
                       .ApplyMappingHints("DATEOFLASTVISIT", "DATE_LAST_VISIT", "LAST_VISIT_DATE");

            Target<CGCAHPSSurveyTarget>(target => target.PhysicianGender)
                       .ApplyMappingHints("PHYSICIANGENDER", "PHYSICIAN_GENDER", "PHY_GENDER","GENDER");

            Target<CGCAHPSSurveyTarget>(target => target.AdultPracticeSampleSize)
                       .ApplyMappingHints("P_ADULT_SAMPLESIZE", "P_ADULTSAMPLESIZE", "PADULTSAMPLESIZE", "PRACTICEADULTSAMPLESIZE", "PRACTICE_ADULT_SAMPLE_SIZE", "ADULT_SAMPLE_SIZE");

            Target<CGCAHPSSurveyTarget>(target => target.ChildPracticeSampleSize)
                       .ApplyMappingHints("P_CHILD_SAMPLESIZE", "P_CHILDSAMPLESIZE", "PCHILDSAMPLESIZE", "PRACTICECHILDSAMPLESIZE", "PRACTICE_CHILD_SAMPLE_SIZE", "CHILD_SAMPLE_SIZE");

            Target<CGCAHPSSurveyTarget>(target => target.SurveyCompletionDate)
                       .ApplyMappingHints("SURVEYCOMPLETIONDATE", "SURVEY_COMPLETION_DATE", "COMPLETIONDATE", "COMPLETION_DATE", "SURVEYDATE", "SURVEY_DATE");

            Target<CGCAHPSSurveyTarget>(target => target.AV_06)
                       .ApplyMappingHints("AV06", "AV_06");

            Target<CGCAHPSSurveyTarget>(target => target.AV_08)
                       .ApplyMappingHints("AV08", "AV_08");

            Target<CGCAHPSSurveyTarget>(target => target.AV_10)
                       .ApplyMappingHints("AV10", "AV_10");

            Target<CGCAHPSSurveyTarget>(target => target.AV_12)
                       .ApplyMappingHints("AV12", "AV_12");

            Target<CGCAHPSSurveyTarget>(target => target.AV_13)
                      .ApplyMappingHints("AV13", "AV_13");

            Target<CGCAHPSSurveyTarget>(target => target.AV_16)
                      .ApplyMappingHints("AV16", "AV_16");

            Target<CGCAHPSSurveyTarget>(target => target.AV_17)
                      .ApplyMappingHints("AV17", "AV_17");

            Target<CGCAHPSSurveyTarget>(target => target.AV_19)
                      .ApplyMappingHints("AV19", "AV_19");

            Target<CGCAHPSSurveyTarget>(target => target.AV_20)
                      .ApplyMappingHints("AV20", "AV_20");

            Target<CGCAHPSSurveyTarget>(target => target.AV_21)
                      .ApplyMappingHints("AV21", "AV_21");

            Target<CGCAHPSSurveyTarget>(target => target.AV_22)
                      .ApplyMappingHints("AV22", "AV_22");

            Target<CGCAHPSSurveyTarget>(target => target.AV_24)
                      .ApplyMappingHints("AV24", "AV_24");

            Target<CGCAHPSSurveyTarget>(target => target.AV_25)
                      .ApplyMappingHints("AV25", "AV_25");

            Target<CGCAHPSSurveyTarget>(target => target.AV_26)
                      .ApplyMappingHints("AV26", "AV_26");

            Target<CGCAHPSSurveyTarget>(target => target.AV_27)
                      .ApplyMappingHints("AV27", "AV_27");

            Target<CGCAHPSSurveyTarget>(target => target.AV_28)
                      .ApplyMappingHints("AV28", "AV_28");


            Target<CGCAHPSSurveyTarget>(target => target.Cd_38)
                      .ApplyMappingHints("CD38", "CD_38");

            Target<CGCAHPSSurveyTarget>(target => target.Cd_39)
                      .ApplyMappingHints("CD39", "CD_39");

            Target<CGCAHPSSurveyTarget>(target => target.Cd_40)
                       .ApplyMappingHints("CD40", "CD_40");

            Target<CGCAHPSSurveyTarget>(target => target.Cd_41)
                      .ApplyMappingHints("CD41", "CD_41");

            Target<CGCAHPSSurveyTarget>(target => target.Cd_42)
                       .ApplyMappingHints("CD42", "CD_42");

            Target<CGCAHPSSurveyTarget>(target => target.Cd_43)
                       .ApplyMappingHints("CD43", "CD_43");

            Target<CGCAHPSSurveyTarget>(target => target.Cd_44)
                       .ApplyMappingHints("CD44", "CD_44");

            Target<CGCAHPSSurveyTarget>(target => target.Cd_45)
                      .ApplyMappingHints("CD45", "CD_45");

            Target<CGCAHPSSurveyTarget>(target => target.Cd_46)
                      .ApplyMappingHints("CD46", "CD_46");

            Target<CGCAHPSSurveyTarget>(target => target.Cd_47)
                      .ApplyMappingHints("CD47", "CD_47");

            Target<CGCAHPSSurveyTarget>(target => target.Cd_48)
                       .ApplyMappingHints("CD48", "CD_48");
        }
		#endregion
		#endregion
	}
}
