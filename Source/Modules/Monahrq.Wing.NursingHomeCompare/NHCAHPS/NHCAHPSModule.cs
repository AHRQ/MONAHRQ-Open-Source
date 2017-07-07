using System;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.NursingHomes;
using System.IO;

namespace Monahrq.Wing.NursingHomeCompare.NHCAHPS
{

    static class Constants
    {
        public const string WING_GUID = "872457E3-F513-4262-9842-4C1FAE38782D";
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WING_GUID);
    }

    [WingModule(typeof(NHCAHPSModule), Constants.WING_GUID,
        "NH-CAHPS Survey Results Data", "Provides Services for NH-CAHPS Survey Results Data",
        DependsOnModuleNames = new[] {"Base Data"}, DisplayOrder = 7)]
    public class NHCAHPSModule : TargetedModuleWithMeasuresAndTopics<NHCAHPSSurveyTarget>
    {
		#region Properties.
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        private ILogWriter Logger { get; set; }

        /// <summary>
        /// Gets or sets the measure SVC.
        /// </summary>
        /// <value>
        /// The measure SVC.
        /// </value>
        [Import]
        private IMeasureService MeasureSvc { get; set; }

		public override string MeasureFilePath
		{
			get { return Path.Combine(MonahrqContext.BinFolderPath, "Measures", "MeasuresNHCAHPS.csv"); }
		}
		public override string MeasureTopicFilePath
		{
			get { return Path.Combine(MonahrqContext.BinFolderPath, "Measures", "MeasuresNHCAHPSTopics.csv"); }
		}
		#endregion

		#region Methods.
		#region Constructor/Initialize Methods.
        ///// <summary>
        ///// Called when [initialize].
        ///// </summary>
        //protected override void OnInitialize()
        //{
        //    base.OnInitialize();
        //    Subscribe();
        //}
		#endregion

		#region Subscribe Method.
        ///// <summary>
        ///// Subscribes this instance.
        ///// </summary>
        //private void Subscribe()
        //{
        //    Events.GetEvent<WizardStepsRequestEvent<DataTypeModel, Guid, int?>>()
        //        .Subscribe(args =>
        //        {
        //            if (args.WingId == WingGUID)
        //            {
        //                args.WizardSteps = new WizardSteps(args.Data, args.ExistingDatasetId);
        //            }
        //        });
        //}
		#endregion

		#region Manage Measures/Topics Data Methods.
        /// <summary>
        /// Imports the measures.
        /// </summary>
        protected override void ImportMeasures()
        {
            try
            {
                MeasureSvc.ImportMeasures(TargetAttribute.Name, MeasureFilePath, typeof(NursingHomeMeasure));
            }
            catch (Exception ex)
            {
                Logger.Write(ex.GetBaseException());
            }
        }

        /// <summary>
        /// Imports the measure topics.
        /// </summary>
        protected override void ImportMeasureTopics()
        {
            try
            {
                MeasureSvc.ImportMeasureTopicFile(TargetAttribute.Name, MeasureTopicFilePath, TopicTypeEnum.NursingHome);
            }
            catch (Exception ex)
            {
                Logger.Write(ex.GetBaseException());
            }
        }

        protected override void OnApplyDatasetHints()
        {
            Target<NHCAHPSSurveyTarget>(target => target.ProviderId)
                         .ApplyMappingHints("PROVIDER_ID", "PROVIDERID");

            Target<NHCAHPSSurveyTarget>(target => target.NHId)
                        .ApplyMappingHints("NHID", "NH_ID");

            Target<NHCAHPSSurveyTarget>(target => target.DateOfLastVisit)
                       .ApplyMappingHints("DATEOFLASTVISIT", "DATE_LAST_VISIT", "LAST_VISIT_DATE");

            Target<NHCAHPSSurveyTarget>(target => target.PracticeSampleSize)
                       .ApplyMappingHints("P_SAMPLESIZE", "PSAMPLESIZE", "PRACTICESAMPLESIZE", "PRACTICE_SAMPLE_SIZE", "SAMPLE_SIZE");

            Target<NHCAHPSSurveyTarget>(target => target.SurveyCompletionDate)
                       .ApplyMappingHints("SURVEYCOMPLETIONDATE", "SURVEY_COMPLETION_DATE", "COMPLETIONDATE", "COMPLETION_DATE", "SURVEYDATE", "SURVEY_DATE");

            Target<NHCAHPSSurveyTarget>(target => target.Q12)
                       .ApplyMappingHints("Q12", "Q_12");

            Target<NHCAHPSSurveyTarget>(target => target.Q11)
                       .ApplyMappingHints("Q11", "Q_11");

            Target<NHCAHPSSurveyTarget>(target => target.Q13)
                       .ApplyMappingHints("Q13", "Q_13");

            Target<NHCAHPSSurveyTarget>(target => target.Q14)
                       .ApplyMappingHints("Q14", "Q_14");

            Target<NHCAHPSSurveyTarget>(target => target.Q15)
                      .ApplyMappingHints("Q15", "Q_15");

            Target<NHCAHPSSurveyTarget>(target => target.Q17)
                      .ApplyMappingHints("Q17", "Q_17");

            Target<NHCAHPSSurveyTarget>(target => target.Q19)
                      .ApplyMappingHints("Q19", "Q_19");

            Target<NHCAHPSSurveyTarget>(target => target.Q21)
                      .ApplyMappingHints("Q21", "Q_21");

            Target<NHCAHPSSurveyTarget>(target => target.Q22)
                      .ApplyMappingHints("Q22", "Q_22");

            Target<NHCAHPSSurveyTarget>(target => target.Q24)
                      .ApplyMappingHints("Q24", "Q_24");

            Target<NHCAHPSSurveyTarget>(target => target.Q26)
                      .ApplyMappingHints("Q26", "Q_26");

            Target<NHCAHPSSurveyTarget>(target => target.Q27)
                      .ApplyMappingHints("Q27", "Q_27");

            Target<NHCAHPSSurveyTarget>(target => target.Q28)
                      .ApplyMappingHints("Q28", "Q_28");

            Target<NHCAHPSSurveyTarget>(target => target.Q29)
                      .ApplyMappingHints("Q29", "Q_29");

            Target<NHCAHPSSurveyTarget>(target => target.Q30)
                      .ApplyMappingHints("Q30", "Q_30");

            Target<NHCAHPSSurveyTarget>(target => target.Q31)
                      .ApplyMappingHints("Q31", "Q_31");

            Target<NHCAHPSSurveyTarget>(target => target.Q33)
                     .ApplyMappingHints("Q33", "Q_33");

            Target<NHCAHPSSurveyTarget>(target => target.Q35)
                     .ApplyMappingHints("Q35", "Q_35");

            Target<NHCAHPSSurveyTarget>(target => target.Q37)
                     .ApplyMappingHints("Q37", "Q_37");

            Target<NHCAHPSSurveyTarget>(target => target.Q38)
                     .ApplyMappingHints("Q38", "Q_38");

            Target<NHCAHPSSurveyTarget>(target => target.Q40)
                     .ApplyMappingHints("Q40", "Q_40");

            Target<NHCAHPSSurveyTarget>(target => target.Q42)
                     .ApplyMappingHints("Q42", "Q_42");
        }
		#endregion
		#endregion
    }
}
