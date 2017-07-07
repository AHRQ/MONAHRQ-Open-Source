using Microsoft.Practices.Prism.Regions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.DataSets.ViewModels;
using Monahrq.Sdk.Regions;
using Monahrq.TestSupport;
using Rhino.Mocks;
using System;
using System.ComponentModel.Composition;
using Monahrq.Sdk.Services;
using Microsoft.Practices.Prism.Events;
//using Monahrq.DataImport.BaseData.v_0_1;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure;
//using ClinicalDimensonsModule = Monahrq.Wing.ReportingEntities.ClinicalDimensons.Module;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Sdk.Extensibility;
//using Monahrq.DataImport.ClinicalDimension.v_0_1.Strategies;

namespace Monahrq.DataSets.Test
{
    [TestClass]
    public class DataSetViewModelTest : MefTestFixture
    {
        DataSetListViewModel ViewModel { get { return Container.GetExportedValue<DataSetListViewModel>(); } }
        IRegionManager RegionManager { get { return Container.GetExportedValue<IRegionManager>(); } } // get it out of the container.

        //protected override System.Collections.Generic.IEnumerable<Type> InjectedTypes
        //{
        //    get { return new[]{typeof(DataSetListViewModel), 
        //        typeof(DrgSourceLoader)
        //        ,typeof(Monahrq.DataImport.ClinicalDimension.v_0_1.ClinicalDimensionDataReaders)}; }
        //}

        protected override void ComposeFixtureInstances()
        {
            base.ComposeFixtureInstances();

            Container.ComposeExportedValue<IConfigurationService>(MockRepository.GenerateStub<IConfigurationService>());
            Container.ComposeExportedValue<IDomainSessionFactoryProvider>(MockRepository.GenerateStub<IDomainSessionFactoryProvider>());
            Container.ComposeExportedValue<IImportAuditor>(MockRepository.GenerateStub<IImportAuditor>());
            Container.ComposeExportedValue<ILogWriter>(MockRepository.GenerateStub<ILogWriter>());
            Container.ComposeExportedValue<ILogWriter>(LogNames.Session, MockRepository.GenerateStub<ILogWriter>());
            Container.ComposeExportedValue<ILogWriter>(LogNames.Operations, MockRepository.GenerateStub<ILogWriter>());
            Container.ComposeExportedValue<IDatasetService>(MockRepository.GenerateStub<IDatasetService>());
            Container.ComposeExportedValue<IEventAggregator>(MockRepository.GenerateStub<IEventAggregator>());
            Container.ComposeExportedValue<IMonahrqShell>(MockRepository.GenerateStub<IMonahrqShell>());

            // Generate the Region Manager, Region and Regions stubs.
            var regionManager = MockRepository.GenerateMock<IRegionManager>();
            var region = MockRepository.GenerateStub<IRegion>();
            var regions = MockRepository.GenerateStub<IRegionCollection>();
            regions.Stub(r => r.ContainsRegionWithName(RegionNames.MainContent)).Return(true);
            regions.Stub(r => r[RegionNames.MainContent]).Return(region);
            regionManager.Stub(r => r.Regions).Return(regions);
            
            // Stick the Region Manager in the container.
            Container.ComposeExportedValue<IRegionManager>(regionManager);
        }

        [TestMethod]
        [Ignore]
        public void ViewModelCtorPositive()
        {
            var exp = Container.GetExportedValue<IDataLoader>(DataImportContracts.ClinicalDimensions);
            // Get the active view model.
            var priv = new PrivateObject(ViewModel);

            // Get the actual region manager that was created in the constructor.
            var actual = priv.GetProperty("RegionMgr") as IRegionManager;

            // Check that the region manager that was created in the constructor isn't null.
            Assert.IsNotNull(actual, "RegionMgr is not assigned.");

            // Check that the cunstructor region manager matches the 
            Assert.AreSame(RegionManager, actual, "RegionMgr not the same instance.");
        }

        [TestMethod]
        [Ignore]
        public void ImportDataFileLoadsCorrectViewNamePositive()
        {
            // Check that the right view name is called from ImportDataFileClick

            var region = RegionManager.Regions[RegionNames.MainContent];
            region.Stub(r => r.RequestNavigate((Uri)null, null));

            ViewModel.ImportDataFileClick.Execute(null);
            
            var args = region.GetArgumentsForCallsMadeOn(r => r.RequestNavigate((Uri)null, null));
            var uri = args[0][0] as Uri;
            Assert.AreEqual(ViewNames.DataImportWizard, uri.OriginalString, "The wrong view name was returned.");
        }

        [TestMethod]
        [Ignore]
        public void ImportDataFileLoadsCorrectRegion()
        {
            // Check that the right region is called from the ImportDataFileClick

            var regions = RegionManager.Regions;
            regions.Stub(r => r.ContainsRegionWithName(string.Empty));

            ViewModel.ImportDataFileClick.Execute(null);

            var args = regions.GetArgumentsForCallsMadeOn(r => r.ContainsRegionWithName(string.Empty));
            var regionName = args[0][0] as string;
            Assert.AreEqual(RegionNames.MainContent, regionName, "The wrong region was loaded.");
        }

      
    }
}
