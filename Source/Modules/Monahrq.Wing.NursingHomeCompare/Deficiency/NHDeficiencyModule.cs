using System;
using System.ComponentModel.Composition;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using System.IO;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Wing.NursingHomeCompare.Deficiency.Model;

namespace Monahrq.Wing.NursingHomeCompare.Deficiency
{

	/// <summary>
	/// Constants.
	/// </summary>
	static class Constants
    {
		/// <summary>
		/// The wing unique identifier
		/// </summary>
		public const string WING_GUID = "3AC422B9-5279-4BFF-BC28-CBFB529F22C0";
		/// <summary>
		/// The wing unique identifier as unique identifier
		/// </summary>
		public static readonly Guid WingGuidAsGuid = Guid.Parse(WING_GUID);
    }

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Monahrq.Sdk.Modules.Wings.TargetedModuleWithMeasuresAndTopics{Monahrq.Wing.NursingHomeCompare.Deficiency.NHDeficiencyTarget}" />
	//[WingModule(typeof(NHDeficiencyModule), Constants.WING_GUID,
	//    "Nursing Home Deficiency Matrix Data", "Provides Services for Nursing Home Deficiency Matrix Data",
	//    DependsOnModuleNames = new[] {"Base Data"}, DisplayOrder = 7)]
	public class NHDeficiencyModule : TargetedModuleWithMeasuresAndTopics<NHDeficiencyTarget>
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
		/// Gets or sets the Measure Service.
		/// </summary>
		/// <value>
		/// The measure SVC.
		/// </value>
		[Import]
        IMeasureService MeasureSvc { get; set; }


		/// <summary>
		/// Gets the measure file path.
		/// </summary>
		/// <value>
		/// The measure file path.
		/// </value>
		public override string MeasureFilePath
		{
			get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures", "MeasuresDeficiency.csv"); }
		}
		/// <summary>
		/// Gets the measure topic file path.
		/// </summary>
		/// <value>
		/// The measure topic file path.
		/// </value>
		public override string MeasureTopicFilePath
		{
			get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures", "MeasuresDeficiencyTopics.csv"); }
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
            Subscribe();
        }
		#endregion

		#region Subscribe Method.
		/// <summary>
		/// Subscribes this instance.
		/// </summary>
		private void Subscribe()
        {
            //Events.GetEvent<WizardStepsRequestEvent<DataTypeModel, Guid, int?>>()
            //    .Subscribe(args =>
            //    {
            //        if (args.WingId == WingGUID)
            //        {
            //            args.WizardSteps = new WizardSteps(args.Data);
            //        }
            //    });
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
                MeasureSvc.ImportMeasureTopicFile(TargetAttribute.Name, MeasureTopicFilePath, TopicTypeEnum.NursingHome);
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }
		#endregion
		#endregion
	}
}
