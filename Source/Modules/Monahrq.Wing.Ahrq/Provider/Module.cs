using Monahrq.DataSets.Model;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Ahrq.Model;
using System;
using Microsoft.Practices.Prism.Modularity;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Entities.Domain.Measures;

namespace Monahrq.Wing.Ahrq.Provider
{
    /// <summary>
    /// Class for Provider Wing Id.
    /// </summary>
    static class  Constants
    {
        public const string WingGuid = "E5CF8D23-503F-4611-9845-83AF3F4678B4";
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WingGuid);

        public const string WingTargetGuid = "55f6326d-082c-40cf-bb33-fcf1078943a8";
        public static readonly Guid WingTargetGuidAsGuid = Guid.Parse(WingTargetGuid);
    }

    /// <summary>
    /// Module class for provider.
    /// </summary>
    /// <seealso cref="Monahrq.Wing.Ahrq.AhrqModuleBase{Monahrq.Wing.Ahrq.Provider.ProviderTarget}" />
    [WingModuleAttribute(typeof(Module), Constants.WingGuid, "AHRQ-QI Provider Data", "Provides Services for AHRQ-QI Provider Data",
        DependsOnModuleNames = new string[] { "Base Data" }, InitializationMode = InitializationMode.WhenAvailable, DisplayOrder = 4)]
    public partial class Module : AhrqModuleBase<ProviderTarget>
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
        /// Gets the measure file's name.
        /// </summary>
        /// <value>
        /// The name of the measure file.
        /// </value>
        protected override string MeasureFileName
        {
            get 
            {
                return "MeasuresAhrqProvider.csv";
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
                return "MeasuresAhrqProviderTopics.csv";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Module"/> class.
        /// </summary>
        /// <param name="measureService">The measure service.</param>
        [ImportingConstructor]
        public Module(IMeasureService measureService): base(measureService)
        {
        }
    }
}
