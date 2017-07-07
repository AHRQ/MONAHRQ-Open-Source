using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using System;
using System.ComponentModel.Composition;
using System.IO;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;

namespace Monahrq.Wing.Ahrq
{
    /// <summary>
    /// Ahrq base module.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Monahrq.Sdk.Modules.Wings.TargetedModuleWithMeasuresAndTopics{T}" />
    public abstract class AhrqModuleBase<T> : TargetedModuleWithMeasuresAndTopics<T>
        where T : DatasetRecord
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
        /// Gets the name of the measure file.
        /// </summary>
        /// <value>
        /// The name of the measure file.
        /// </value>
        protected abstract string MeasureFileName { get; }
        /// <summary>
        /// Gets the name of the measure topics file.
        /// </summary>
        /// <value>
        /// The name of the measure topics file.
        /// </value>
        protected abstract string MeasureTopicsFileName { get; }
        /// <summary>
        /// Gets or sets the measure SVC.
        /// </summary>
        /// <value>
        /// The measure SVC.
        /// </value>
        IMeasureService MeasureSvc { get; set; }

        /// <summary>
        /// Gets the measure file path.
        /// </summary>
        /// <value>
        /// The measure file path.
        /// </value>
        public override String MeasureFilePath
		{
			get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures", MeasureFileName); }
		}
        /// <summary>
        /// Gets the measure topic file path.
        /// </summary>
        /// <value>
        /// The measure topic file path.
        /// </value>
        public override String MeasureTopicFilePath
		{
			get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Measures", MeasureTopicsFileName); }
		}
        #endregion


        /// <summary>
        /// Abstract method to get the wizards steps.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <returns></returns>
        protected abstract IStepCollection FactoryWizardSteps(DataTypeModel model, int? datasetId);

        /// <summary>
        /// Initializes a new instance of the <see cref="AhrqModuleBase{T}"/> class.
        /// </summary>
        /// <param name="measureService">The measure service.</param>
        public AhrqModuleBase(IMeasureService measureService)
        {
            MeasureSvc = measureService;
        }

        //public AhrqModuleBase()
        //{
        //    if(MeasureSvc == null)
        //        MeasureSvc = ServiceLocator.Current.GetInstance<IMeasureService>();
        //}

        /// <summary>
        /// Called when [initialize].
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            Subscribe();
            // ImportMeasures();
            // ImportMeasureTopics();
        }

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
                        args.WizardSteps = FactoryWizardSteps(args.Data, args.ExistingDatasetId);
                    }
                });
        }

        /// <summary>
        /// Imports the measures.
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
        /// Imports the measure topics.
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
    }
}
