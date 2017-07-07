using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.Practices.Prism.Regions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Services.Hospitals;
using Monahrq.Sdk.Modules;
using Monahrq.TestSupport;

namespace Monahrq.HospitalsAndRegions.Test
{
    [TestClass]
    public class ControllerTest:MefTestFixture
    {
        protected override System.Collections.Generic.IEnumerable<Type> InjectedTypes
        {
            get
            {
                return new[] { 
                    typeof(ModuleController)
                    ,typeof(HospitalRegistryService)
                    ,typeof(DomainSessionFactoryProvider)
                 , typeof(ConfigurationService)}; 
                
            }
        }

        protected override void ComposeFixtureInstances()
        {
            base.ComposeFixtureInstances();
            Container.ComposeExportedValue<IRegionManager>(new RegionManager());
        }

        protected override AggregateCatalog ConfigureAggregateCatalog()
        {
            var cat =  base.ConfigureAggregateCatalog();
            //cat.Catalogs.Add(new AssemblyCatalog(typeof(IDomainSessionFactoryProvider).Assembly));
            return cat;
        }

        [TestMethod]
        public void CanGetControllerPositive()
        {
            var controller = Container.GetExportedValue<IClinicalDimensionController>();
            Assert.IsNotNull(controller);
        }

        [TestMethod]
        public void ControllerInitContextNullPositive()
        {
            //var controller = Container.GetExportedValue<IClinicalDimensionNavigationAware>();
            //Assert.IsNotNull(controller.CurrentView);
        }
    }
}
