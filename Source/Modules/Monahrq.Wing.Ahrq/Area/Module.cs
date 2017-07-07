using Monahrq.DataSets.Model;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Ahrq.Model;
using System;
using Microsoft.Practices.Prism.Modularity;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Entities.Domain.Measures;

namespace Monahrq.Wing.Ahrq.Area
{
    /// <summary>
    /// Class for Wing Id.
    /// </summary>
    static class Constants
    {
        /// <summary>
        /// Unique identifier for the Wing.
        /// </summary>
        public const string WingGuid = "{D667D878-B237-40F4-BCCC-8DCE79047B36}";
        /// <summary>
        /// The wing unique identifier as unique identifier
        /// </summary>
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WingGuid);

        /// <summary>
        /// Unique identifier for the Wing target
        /// </summary>
        public const string WingTargetGuid = "1bee79f2-3c56-40bc-95e2-56c22dc598d7";
        /// <summary>
        /// The wing target unique identifier as unique identifier
        /// </summary>
        public static readonly Guid WingTargetGuidAsGuid = Guid.Parse(WingTargetGuid);
    }

    /// <summary>
    /// Module class for Ahrq
    /// </summary>
    /// <seealso cref="Monahrq.Wing.Ahrq.AhrqModuleBase{Monahrq.Wing.Ahrq.Area.AreaTarget}" />
    [WingModuleAttribute(typeof(Module), Constants.WingGuid, "AHRQ-QI Area Data", "Provides Services for AHRQ-QI Area Data",
        DependsOnModuleNames = new string[] { "Base Data" }, InitializationMode = InitializationMode.WhenAvailable, DisplayOrder = 2)]
    public partial class Module : AhrqModuleBase<AreaTarget>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Module"/> class.
        /// </summary>
        /// <param name="measureService">The measure service.</param>
        [ImportingConstructor]
        public Module(IMeasureService measureService)
            : base(measureService)
        {
        }

        /// <summary>
        /// Factories the wizard steps.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <returns></returns>
        protected override IStepCollection FactoryWizardSteps(DataTypeModel model, int? datasetId)
        {
            return new WizardSteps(model, datasetId);
        }

        /// <summary>
        /// Gets the name of the measure file.
        /// </summary>
        /// <value>
        /// The name of the measure file.
        /// </value>
        protected override string MeasureFileName
        {
            get
            {
                return "MeasuresAhrqArea.csv";
            }
        }

        /// <summary>
        /// Gets the name of the measure topics file.
        /// </summary>
        /// <value>
        /// The name of the measure topics file.
        /// </value>
        protected override string MeasureTopicsFileName
        {
            get
            {
                return "MeasuresAhrqAreaTopics.csv";
            }
        }
    }
}
