using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Practices.Prism.Modularity;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Physician.Physicians.Models;
using NHibernate.Linq;

namespace Monahrq.Wing.Physician.Physicians
{
    internal static class PhysicianConstants
    {
        public const string WING_GUID = "{BB53CFD5-6912-4F8A-A955-BD78119C871F}";
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WING_GUID);

        public const string WING_TARGET_GUID = "9DB4571F-6D73-4516-970B-59ED09C51413";
        public static readonly Guid WingTargetGuidAsGuid = Guid.Parse(WING_TARGET_GUID);

        public const string WING_TARGET_NAME = "Physician Data";
    }

    [WingModule(typeof(PhysicianModule), PhysicianConstants.WING_GUID, PhysicianConstants.WING_TARGET_NAME, PhysicianConstants.WING_TARGET_NAME,
        DependsOnModuleNames = new[] { "Base Data" }, InitializationMode = InitializationMode.WhenAvailable, DisplayOrder = 7)]
    public class PhysicianModule : TargetedModuleBase<PhysicianTarget>
    {
        public PhysicianModule()
        { }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Subscribe();
        }

        //protected override void OnWingAdded()
        //{
        //    base.OnWingAdded();
        //    // ImportMeasures();
        //}

        [Import(LogNames.Session)]
        private ILogWriter Logger { get; set; }
        
        ///// <summary>
        ///// Factories the wizard steps.
        ///// </summary>
        ///// <param name="model">The model.</param>
        ///// <param name="datasetId">The dataset identifier.</param>
        ///// <returns></returns>
        //protected IStepCollection FactoryWizardSteps(DataTypeModel model, int? datasetId)
        //{
        //    return new WizardSteps(model, datasetId);
        //}

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

        /// <summary>
        /// Gets the measures XML.
        /// </summary>
        /// <value>
        /// The measures XML.
        /// </value>
        private XDocument MeasuresXml
        {
            get
            {
                var resourceNames = Enumerable.ToList(typeof(Dummy).Assembly.GetManifestResourceNames());
                var resourceName = resourceNames.FirstOrDefault(r => r.ContainsIgnoreCase("PhysicianMeasures.xml"));

                if (string.IsNullOrEmpty(resourceName)) return new XDocument();

                using (var str = typeof(Dummy).Assembly.GetManifestResourceStream(resourceName)) //Measures
                {
                    if (str == null) return new XDocument();
                    return XDocument.Load(str);
                }
            }
        }

        public override bool InstallDb()
        {
            ImportMeasures();

            return true;
        }

        protected void ImportMeasures()
        {
            try
            {
                using (var session = SessionFactoryProvider.SessionFactory.OpenSession())
                {
                    var target = session.Query<Target>().FirstOrDefault(t => t.Name == TargetAttribute.Name);
                    var measuresElement = MeasuresXml.Element("measures");

                    if (measuresElement != null)
                    {
                        var measureCount = 0;
                        foreach (var measureElement in measuresElement.Elements("measure"))
                        {
                            var code = string.Format("MPCH-{0}", measureCount++);

                            if (session.Query<Measure>().Any(m => m.Name == code))
                                continue;

                            var measure = measureElement.ToMeasure(target, code);
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
    }
}
