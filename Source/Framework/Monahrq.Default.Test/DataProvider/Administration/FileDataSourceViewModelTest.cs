using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Default.DataProvider.Administration;
using Monahrq.Default.DataProvider.Administration.File;
using Monahrq.Default.DataProvider.MsAccess;
using Monahrq.Default.DataProvider.MsExcel;
using Monahrq.Default.DataProvider.Text;
using Monahrq.Sdk.DataProvider;
using Monahrq.TestSupport;

using Rhino.Mocks;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Sdk.DataProvider.Builder;
using Monahrq.Default.DataProvider;
using System.Collections.ObjectModel;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure;

namespace Monahrq.Default.Test.DataProvider.Administration
{
    [TestClass]
    public class FileDataSourceViewModelTest : MefTestFixture
    {
        protected override void ComposeFixtureInstances()
        {
            base.ComposeFixtureInstances();
            var manager = MockRepository.GenerateStub<IRegionManager>();
            Container.ComposeExportedValue<IRegionManager>(manager);
            var region = MockRepository.GenerateStub<IRegion>();
            var regions = MockRepository.GenerateStub<IRegionCollection>();
            var views = new viewsDouble();
            manager.Expect(m => m.Regions[DialogRegionNames.ConnectionStringViewRegion]).Return(region);
            region.Expect(r => r.ActiveViews).Return(views);
            Container.ComposeExportedValue<IRegion>(region);
        }

        //protected override IEnumerable<Type> InjectedTypes
        //{
        //    get
        //    {
        //        //return new[] { typeof(MsExcelConnectionStringView) };
        //    }
        //}

        [TestMethod]
        public void CanGetTheView()
        {
            var view = Container.GetExportedValue<IFileConnectionStringView>(DataProviderStrings.MsExcelBuilderViewName);
            Assert.IsNotNull(view);
        }

        protected override System.ComponentModel.Composition.Hosting.AggregateCatalog ConfigureAggregateCatalog()
        {
            var agg = base.ConfigureAggregateCatalog();
            var more = new AssemblyCatalog(typeof(FileDatasourceViewModel).Assembly);
            agg.Catalogs.Add(more);
            return agg;
        }


        [TestMethod]
        public void CtorTestPositive()
        {
            var model = Container.GetExportedValue<IFileDatasourceViewModel>();
            Assert.AreEqual(string.Empty, model.CurrentFile);
            Assert.IsNull(model.CurrentDataProvider);
            Assert.IsNull(model.CurrentView);
            var priv = new PrivateObject(model);
            var provs = priv.GetProperty("OleDbProviders") as Lazy<IEnumerable<IDataProviderController>>;
            Assert.IsTrue(provs.Value.Any());
        }

        [TestMethod]
        public void AccdbFileSetsProviderPositive()
        {
            var model = Container.GetExportedValue<IFileDatasourceViewModel>();
            model.CurrentFile = "foo.accdb";
            var prov = model.CurrentDataProvider;
            Assert.IsNotNull(prov);
            Assert.IsInstanceOfType(prov, typeof(MsAccessAceDataProviderController));
            Assert.IsNotNull(model.CurrentView);
            Assert.IsInstanceOfType(model.CurrentView, typeof(MsAccessConnectionStringView));
        }

        [TestMethod]
        public void MdbFileSetsProviderPositive()
        {
            var model = Container.GetExportedValue<IFileDatasourceViewModel>();
            model.CurrentFile = "foo.mdb";
            var prov = model.CurrentDataProvider;
            Assert.IsNotNull(prov);
            Assert.IsInstanceOfType(prov, typeof(MsAccessAceDataProviderController));
            Assert.IsNotNull(model.CurrentView);
            Assert.IsInstanceOfType(model.CurrentView, typeof(MsAccessConnectionStringView));
        }

        [TestMethod]
        public void XlsFileSetsProviderPositive()
        {
            var model = Container.GetExportedValue<IFileDatasourceViewModel>();
            model.CurrentFile = "foo.xls";
            var prov = model.CurrentDataProvider;
            Assert.IsNotNull(prov);
            Assert.IsInstanceOfType(prov, typeof(MsExcelAceDataProviderController));
            Assert.IsNotNull(model.CurrentView);
            Assert.IsInstanceOfType(model.CurrentView, typeof(MsExcelConnectionStringView));
        }

        [TestMethod]
        public void XlsxFileSetsProviderPositive()
        {
            var model = Container.GetExportedValue<IFileDatasourceViewModel>();
            model.CurrentFile = "foo.xlsx";
            var prov = model.CurrentDataProvider;
            Assert.IsNotNull(prov);
            Assert.IsInstanceOfType(prov, typeof(MsExcelAceDataProviderController));
            Assert.IsNotNull(model.CurrentView);
            Assert.IsInstanceOfType(model.CurrentView, typeof(MsExcelConnectionStringView));
        }

        [TestMethod]
        public void CsvFileSetsProviderPositive()
        {
            var model = Container.GetExportedValue<IFileDatasourceViewModel>();
            model.CurrentFile = "foo.csv";
            var prov = model.CurrentDataProvider;
            Assert.IsNotNull(prov);
            Assert.IsInstanceOfType(prov, typeof(TextAceDataProviderController));
            Assert.IsNotNull(model.CurrentView);
            Assert.IsInstanceOfType(model.CurrentView, typeof(TextFileConnectionStringView));
        }

        ///TODO Need to test that Region.Activate is passed correct type of view
        //  in    LoadConnectionStringViewForProvider()
        //  at-> TargetRegion.Activate(View); <- what type is "View" ????


        [TestMethod]
        public void TestConnElement()
        {
            //var element = new NamedConnectionElement();
            //element.ControllerType = typeof(TextAceDataProviderController).AssemblyQualifiedName;
            //element.ConnectionString = 
        }

    }

    class viewsDouble : ObservableCollection<object>, IViewsCollection { }
}