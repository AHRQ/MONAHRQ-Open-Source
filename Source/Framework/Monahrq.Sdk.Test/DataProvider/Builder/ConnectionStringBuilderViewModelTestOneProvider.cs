using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Sdk.DataProvider.Builder;
using Monahrq.Sdk.Services;
using Monahrq.TestSupport;

using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;
using System.Collections.Generic;
using Monahrq.Sdk.DataProvider;


namespace Monahrq.Sdk.Test.DataProvider.Builder
{

    [TestClass]
    public class ConnectionStringBuilderViewModelTestOneProvider : ConnectionStringBuilderViewModelTestOneProviderBase
    {

        [TestMethod]
        public void CtorPositive()
        {
            var model = Container.GetExportedValue<IConnectionStringBuilderViewModel>();
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.ProviderExports);
            PrivateObject privSub = new PrivateObject(model,
              new PrivateType(typeof(ConnectionStringBuilderViewModel)));
            Assert.IsNotNull(privSub.GetFieldOrProperty("TargetRegion"));
        }

        [TestMethod]
        public void CtorPositiveProvidersContainsOne()
        {
            var model = Container.GetExportedValue<IConnectionStringBuilderViewModel>();
            Assert.AreEqual(1, model.ProviderExports.OfType<IDataProviderControllerExportAttribute>().Count());
        }

        [TestMethod]
        public void CtorPositiveProvidersExportCurrentIsSet()
        {
            var model = Container.GetExportedValue<IConnectionStringBuilderViewModel>();
            Assert.IsTrue(!model.ProviderExports.IsEmpty);
            Assert.IsNull(model.CurrentProviderExport);
            model.ProviderExports.MoveCurrentToFirst();
            Assert.IsNotNull(model.CurrentProviderExport);
        }

        [TestMethod]
        public void CtorPositiveViewSet()
        {
            var model = Container.GetExportedValue<IConnectionStringBuilderViewModel>();
            Assert.IsTrue(!model.ProviderExports.IsEmpty);
            Assert.IsNull(model.CurrentProviderExport);
            model.ProviderExports.MoveCurrentToFirst();
            Assert.IsNotNull(model.View);
        }

    }
}
