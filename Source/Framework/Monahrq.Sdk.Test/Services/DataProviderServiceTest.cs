using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.Services;
using Monahrq.TestSupport;
using Monahrq.Sdk.Test.DataProvider;

namespace Monahrq.Sdk.Test.Services
{
    [TestClass]
    public class DataProviderServiceTest : MefTestFixture
    {

        protected override IEnumerable<Type> InjectedTypes
        {
            get
            {
                return base.InjectedTypes.Concat(
                new Type[]
                    {
                        typeof(DataProviderService),
                        typeof(DummyProviderController)
                    }).ToArray();
            }
        }

        [TestMethod]
        public void DataProviderServicePositive()
        {
            var service = Container.GetExportedValue<IDataProviderService>();
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void DataProviderServiceAttributedControllersPositive()
        {
            var service = Container.GetExportedValue<IDataProviderService>();
            var viewTypes = service.GetRegisteredProviderExports();
            Assert.IsNotNull(viewTypes);
            Assert.IsTrue(viewTypes.Any());
            Assert.IsTrue(viewTypes.Where(t=>t.ControllerType.IsAssignableFrom(typeof(DummyProviderController))).Any());
        }

        [TestMethod]
        public void DataProviderServiceGetControllerPositive()
        {
            var key = DummyNames.ControllerName;
            var service = Container.GetExportedValue<IDataProviderService>();
            var instance = service.GetController(key);
            Assert.IsInstanceOfType(instance, typeof(DummyProviderController));
        }

        [TestMethod]
        public void DataProviderServiceGetGenericControllerPositive()
        {
            var expected = new DummyProviderController();
            Container.ComposeExportedValue<IDataProviderController>(expected);
            var service = Container.GetExportedValue<IDataProviderService>();
            var instance = service.GetController(expected.Name);
            Assert.AreSame(instance, expected);
        }
      
    }

    

}
