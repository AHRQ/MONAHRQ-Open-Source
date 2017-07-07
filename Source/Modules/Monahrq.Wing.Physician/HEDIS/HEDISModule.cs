using System;
using System.ComponentModel.Composition;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Physician.HEDIS.Model;
using System.IO;
using Monahrq.Infrastructure.Domain.Physicians;

namespace Monahrq.Wing.Physician.HEDIS
{
	/// <summary>
	/// HEDIS constants.
	/// </summary>
	internal static class HEDISConstants
    {
		/// <summary>
		/// The wing unique identifier
		/// </summary>
		public const string WING_GUID = "29546E1F-D5C3-427B-BA45-085250003D3C";
		/// <summary>
		/// The wing unique identifier as unique identifier
		/// </summary>
		public static readonly Guid WingGuidAsGuid = Guid.Parse(WING_GUID);
    }

	/// <summary>
	/// Contains HEDIS 'global' data.
	/// </summary>
	/// <seealso cref="Monahrq.Sdk.Modules.Wings.TargetedModuleWithMeasuresAndTopics{Monahrq.Wing.Physician.HEDIS.HEDISTarget}" />
	[WingModule(typeof(HEDISModule), HEDISConstants.WING_GUID,
        "Medical Practice HEDIS Measures Data", "Provides Services for Medical Practice HEDIS Measures Data",
        DependsOnModuleNames = new[] {"Base Data"}, DisplayOrder = 7)]
    public class HEDISModule : TargetedModuleWithMeasuresAndTopics<HEDISTarget>
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
		public override string MeasureFilePath
		{
			get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures", "MeasuresHEDIS.csv"); }
		}
		/// <summary>
		/// Gets the measure topic file path.
		/// </summary>
		/// <value>
		/// The measure topic file path.
		/// </value>
		public override string MeasureTopicFilePath
		{
			get { return null; } // Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures", "MeasuresHEDISTopics.csv"); }
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
			//try
			//{
			//    var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures",
			//        "MeasuresNursingHomeCompareTopics.csv");
			//    MeasureSvc.ImportMeasureTopicFile(TargetAttribute.Name, fileName, TopicTypeEnum.NursingHome);
			//}
			//catch (Exception ex)
			//{
			//    Logger.Write(ex);
			//}
		}
		#endregion
		#endregion
	}
}
