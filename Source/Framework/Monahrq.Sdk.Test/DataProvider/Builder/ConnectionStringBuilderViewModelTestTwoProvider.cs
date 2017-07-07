using System;
using System.Linq;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Sdk.DataProvider.Builder;
using Monahrq.Sdk.DataProvider;

namespace Monahrq.Sdk.Test.DataProvider.Builder
{
    [TestClass]
    public class ConnectionStringBuilderViewModelTestTwoProvider : ConnectionStringBuilderViewModelTestTwoProviderBase
    {
        IConnectionStringView Dummy = new DummyStringView();
        IConnectionStringView AnotherDummy = new AnotherDummyStringView();
        [TestMethod]
        public void TestCtorPositive()
        {
            ComposeViews();
            var viewModel =  Container.GetExportedValue<IConnectionStringBuilderViewModel>();
            Assert.IsTrue(viewModel.ProviderExports.OfType<IDataProviderControllerExportAttribute>().Count() == 2);
        }

        [TestMethod]
        public void TestProviderChangeViewChangePositive()
        {
            ComposeViews();
            var viewModel = Container.GetExportedValue<IConnectionStringBuilderViewModel>();
            viewModel.ProviderExports.MoveCurrentToFirst();
            var first = viewModel.ProviderExports.OfType<IDataProviderControllerExportAttribute>().First();
            var second = viewModel.ProviderExports.OfType<IDataProviderControllerExportAttribute>().Last();
            var newSelection = viewModel.CurrentProviderExport == first ? second : first;
            var currentView = viewModel.CurrentProviderExport.ViewType == AnotherDummy.GetType() ? AnotherDummy : Dummy;
            var expectedView = currentView.GetType() == AnotherDummy.GetType() ? Dummy : AnotherDummy;
            Assert.IsTrue(currentView.GetType() == viewModel.View.GetType());
            viewModel.CurrentProviderExport = newSelection;
            Assert.IsTrue(expectedView.GetType() == viewModel.View.GetType());
        }

        private void ComposeViews()
        {
            Container.ComposeExportedValue<IConnectionStringView>(AnotherDummyNames.ViewName, new AnotherDummyStringView());
            Container.ComposeExportedValue<IConnectionStringView>(DummyNames.ViewName, new DummyStringView());
        }
    }
}
