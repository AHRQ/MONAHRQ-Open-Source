using Monahrq.DataSets.Model;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.HospitalCompare.Model;
using NHibernate.Linq;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace Monahrq.Wing.HospitalCompare
{
    static class Constants
    {
        public const string WingGuid = "5819E79E-5120-4A2F-8496-AE44D8A10021";
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WingGuid);
    }

    [WingModuleAttribute(typeof(HospitalCompareModule), Constants.WingGuid,
     "Hospital Compare Data", "Provides Services for Hospital Compare Data",
     DependsOnModuleNames = new[] { "Base Data" }, DisplayOrder = 5)]
    public partial class HospitalCompareModule : TargetedModuleWithMeasuresAndTopics<HospitalCompareTarget>
    {
		#region Properties.
		[Import(LogNames.Session)]
        ILogWriter Logger { get; set; }

        IMeasureService MeasureSvc { get; set; }

		public override String MeasureFilePath
		{
			get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures", "MeasuresHospitalCompare.csv"); }
		}
		public override String MeasureTopicFilePath
		{
			get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures", "MeasuresHospitalCompareTopics.csv"); }
		}
		#endregion

		#region Methods.
		#region Constructor Methods.
		[ImportingConstructor]
        public HospitalCompareModule(IMeasureService measureService)
        {
            MeasureSvc = measureService;
        }


        protected override void OnInitialize()
        {
            base.OnInitialize();
            Subscribe();
        }
		#endregion

		#region Subscribe Method.
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
		/// Imports the measures from a CSV file via the MeasureService.
		/// </summary>
		protected override void ImportMeasures()
        {
            try
            {
                MeasureSvc.ImportMeasures(TargetAttribute.Name, MeasureFilePath, typeof(HospitalMeasure));
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

		/// <summary>
		/// Imports the measure topics from a CSV file via the MeasureService.
		/// </summary>
		protected override void ImportMeasureTopics()
        {
            try
            {
                MeasureSvc.ImportMeasureTopicFile(TargetAttribute.Name, MeasureTopicFilePath, TopicTypeEnum.Hospital);
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
