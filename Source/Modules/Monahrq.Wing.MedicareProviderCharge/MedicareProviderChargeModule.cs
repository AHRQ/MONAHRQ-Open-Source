using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Practices.Prism.Modularity;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.MedicareProviderCharge.Models;
using NHibernate.Linq;

namespace Monahrq.Wing.MedicareProviderCharge
{
    /// <summary>
    /// Class for Constants
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// The wing unique identifier
        /// </summary>
        public const string WING_GUID = "{38678BDB-0B0F-4437-A249-6E82FBF2CA31}";
        /// <summary>
        /// The wing unique identifier as unique identifier
        /// </summary>
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WING_GUID);

        /// <summary>
        /// The wing target unique identifier
        /// </summary>
        public const string WING_TARGET_GUID = "C2F0EE89-E183-4F36-B449-3FB97A0987D7";
        /// <summary>
        /// The wing target unique identifier as unique identifier
        /// </summary>
        public static readonly Guid WingTargetGuidAsGuid = Guid.Parse(WING_TARGET_GUID);
    }

    /// <summary>
    /// Provides Services for Medicare Provider Charge Data
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Modules.Wings.TargetedModuleBase{Monahrq.Wing.MedicareProviderCharge.MedicareProviderChargeTarget}" />
    [WingModule(typeof(MedicareProviderChargeModule), Constants.WING_GUID, "Medicare Provider Charge Data", "Provides Services for Medicare Provider Charge Data",
        DependsOnModuleNames = new[] { "Base Data" }, InitializationMode = InitializationMode.WhenAvailable, DisplayOrder = 6)]
    public class MedicareProviderChargeModule : TargetedModuleBase<MedicareProviderChargeTarget>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MedicareProviderChargeModule"/> class.
        /// </summary>
        public MedicareProviderChargeModule()
        {}

        /// <summary>
        /// Called when [initialize].
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            Subscribe();
        }

        /// <summary>
        /// Called when [wing added].
        /// </summary>
        protected override void OnWingAdded()
        {
            base.OnWingAdded();
           // ImportMeasures();
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        private ILogWriter Logger { get; set; }

        //[Import]
        //private IMeasureService MeasureSvc { get; set; }

        /// <summary>
        /// Factories the wizard steps.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        protected IStepCollection FactoryWizardSteps(DataTypeModel model)
        {
            return new WizardSteps(model);
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
                              args.WizardSteps = FactoryWizardSteps(args.Data);
                          }
                      });
        }

        /// <summary>
        /// Gets the measures XML.
        /// </summary>
        /// <value>
        /// The measures XML.
        /// </value>
        protected virtual XDocument MeasuresXml
        {
            get
            {
                using (var str = GetType().Assembly.GetManifestResourceStream(GetType(), "Measures.xml"))
                {
                    return XDocument.Load(str);
                }
            }
        }

        /// <summary>
        /// Installs the database.
        /// </summary>
        /// <returns></returns>
        public override bool InstallDb()
        {
            //var result = base.InstallDb();

            //if (result)
            ImportMeasures();

            return true;
        }

        /// <summary>
        /// Imports the measures.
        /// </summary>
        protected void ImportMeasures()
        {
            try
            {
                using (var session = SessionFactoryProvider.SessionFactory.OpenSession())
                {
                    var target = session.Query<Target>().FirstOrDefault(t => t.Name == TargetAttribute.Name);
                    var xElement = MeasuresXml.Element("measures");

                    if (xElement != null)
                    {
                        var measureCount = 0;
                        foreach (var measureXml in xElement.Elements("measure"))
                        {
                            var code = string.Format("MPCH-{0}", measureCount++);

                            if(session.Query<Measure>().Any(m => m.Name == code))
                                continue;

                            var measure = measureXml.ToMeasure(target, code, typeof(HospitalMeasure));
                            using (var tx = session.BeginTransaction())
                            {
                                session.SaveOrUpdate(measure);

                                tx.Commit();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        protected override void OnApplyDatasetHints()
        {
            Target<MedicareProviderChargeTarget>(x => x.AverageCoveredCharges).ApplyMappingHints("Average Covered Charges");
            Target<MedicareProviderChargeTarget>(x => x.AverageTotalPayments).ApplyMappingHints("Average Total Payments");
            Target<MedicareProviderChargeTarget>(x => x.DRG).ApplyMappingHints("DRG Definition");
            Target<MedicareProviderChargeTarget>(x => x.ProviderId).ApplyMappingHints("Provider Id");
            Target<MedicareProviderChargeTarget>(x => x.TotalDischarges).ApplyMappingHints("Total Discharges");
        }
    }
}
