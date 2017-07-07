using Monahrq.DataSets.Model;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Ahrq.Model;
using System;
using Microsoft.Practices.Prism.Modularity;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Entities.Domain.Measures;

namespace Monahrq.Wing.Ahrq.Composite
{
    /// <summary>
    /// Class for composite Wing Id.
    /// </summary>
    static class  Constants
    {
        /// <summary>
        /// The wing unique identifier
        /// </summary>
        public const string WingGuid = "{DF8B3C1D-BA63-4913-BC28-F82DB31F368F}";
        /// <summary>
        /// The wing unique identifier as unique identifier
        /// </summary>
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WingGuid);

        /// <summary>
        /// The wing target unique identifier
        /// </summary>
        public const string WingTargetGuid = "518f647f-eb1f-4a8f-8036-7b32d178c758";
        /// <summary>
        /// The wing target unique identifier as unique identifier
        /// </summary>
        public static readonly Guid WingTargetGuidAsGuid = Guid.Parse(WingTargetGuid);
    }

    /// <summary>
    /// Module class for composite
    /// </summary>
    /// <seealso cref="Monahrq.Wing.Ahrq.AhrqModuleBase{Monahrq.Wing.Ahrq.Composite.CompositeTarget}" />
    [WingModuleAttribute(typeof(Module), Constants.WingGuid, "AHRQ-QI Composite Data", "Provides Services for AHRQ-QI Composite Data",
        DependsOnModuleNames = new string[] { "Base Data" }, InitializationMode = InitializationMode.WhenAvailable, DisplayOrder = 3)]
    public partial class Module : AhrqModuleBase<CompositeTarget>
    {
        /// <summary>
        /// Overrides the Wizard steps methods.
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
                return  "MeasuresAhrqComposite.csv";
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
                return "MeasuresAhrqCompositeTopics.csv";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Module"/> class.
        /// </summary>
        /// <param name="measureService">The measure service.</param>
        [ImportingConstructor]
        public Module(IMeasureService measureService)
            : base(measureService)
        {}
    }
}
