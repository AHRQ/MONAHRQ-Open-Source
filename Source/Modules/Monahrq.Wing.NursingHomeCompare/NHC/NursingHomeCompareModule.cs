using System;
using System.ComponentModel.Composition;
using System.IO;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.Hospitals;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.NursingHomeCompare.NHC.Model;

namespace Monahrq.Wing.NursingHomeCompare.NHC
{
    static class Constants
    {
        public const string WING_GUID = "C289056E-5144-4352-9A2B-14C76A6C86F3";
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WING_GUID);
    }

    [WingModule(typeof(NusringHomeCompareModule), Constants.WING_GUID,
     "Nursing Home Compare Data", "Provides Services for Nursing Home Compare Data",
     DependsOnModuleNames = new[] { "Base Data" }, DisplayOrder = 7)]
    public class NusringHomeCompareModule : TargetedModuleWithMeasuresAndTopics<NursingHomeTarget>
    {
		#region Properties.
		/// <summary>
		/// Gets or sets the logger.
		/// </summary>
		/// <value>
		/// The logger.
		/// </value>
		[Import(LogNames.Session)]
        ILogWriter Logger { get; set; }

        /// <summary>
        /// Gets or sets the measure SVC.
        /// </summary>
        /// <value>
        /// The measure SVC.
        /// </value>
        [Import]
        IMeasureService MeasureSvc { get; set; }

		public override String MeasureFilePath
		{
			get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures", "MeasuresNursingHomeCompare.csv"); }
		}
		public override String MeasureTopicFilePath
		{
			get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures", "MeasuresNursingHomeCompareTopics.csv"); }
		}
		#endregion

		#region Methods.
		#region Constructor/Initialize Methods.
		/// <summary>
		/// Initializes a new instance of the <see cref="NusringHomeCompareModule"/> class.
		/// </summary>
		/// <param name="measureService">The measure service.</param>
		//[ImportingConstructor]
        //public NusringHomeCompareModule(IMeasureService measureService)
        //{
        //    MeasureSvc = measureService;
      	//}

		/// <summary>
		/// Called when [initialize].
		/// </summary>
		protected override void OnInitialize()
        {
            base.OnInitialize();
            Subscribe();
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
                                args.WizardSteps = new WizardSteps(args.Data, args.ExistingDatasetId);
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
                MeasureSvc.ImportMeasures(TargetAttribute.Name, MeasureFilePath, typeof(NursingHomeMeasure));
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "Error importing measures");
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
                Logger.Write(ex, "Error importing measure topics");
            }
        }
		#endregion
		#endregion
	}
}
