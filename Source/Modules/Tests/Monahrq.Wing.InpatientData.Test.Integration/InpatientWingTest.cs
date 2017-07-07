using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.Services;
using Monahrq.Sdk.Services.Contracts;
using Monahrq.TestSupport;
using Rhino.Mocks;
using Monahrq.Wing.Discharge.Inpatient;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using System.Configuration;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Wing.InpatientData.Test.Integration
{
    [TestClass]
    [Ignore]
    public class InpatientWingTest: MefTestFixture
    {

        protected override System.Collections.Generic.IEnumerable<Type> InjectedTypes
        {
            get
            {
                return new [] {typeof(FooDataSet)};
            }
        }

        DbConnectionStringBuilder GetBuilder()
        {
            var builder = new SqlConnectionStringBuilder();
            builder.InitialCatalog = "MONAHRQ_50";
            builder.DataSource = @"castor";
            builder.IntegratedSecurity = true;
            return builder;
        }

        protected override void ComposeFixtureInstances()
        {
            base.ComposeFixtureInstances();
            var logger = MockRepository.GenerateStub<ILogWriter>();
            Container.ComposeExportedValue<ILogWriter>(LogNames.Session, logger);
            Container.ComposeExportedValue<ILogWriter>(LogNames.Operations, logger);
            ComposeConfig();
        }

        private void ComposeConfig()
        {
            var conf = MockRepository.GenerateMock<IConfigurationService>();
            var builder = GetBuilder();

            var temp = ConnectionStringSettingsElement.Default;
            temp.ConnectionString = builder.ConnectionString;
            var mock = new ConnectionStringSettings("foo", temp.ConnectionString, temp.ProviderName);
            conf.Stub(c => c.ConnectionSettings).Return(mock);
            Container.ComposeExportedValue<IConfigurationService>(conf);
        }
       
        protected override AggregateCatalog ConfigureAggregateCatalog()
        {
            var catalog = base.ConfigureAggregateCatalog();
            var mods = new AssemblyCatalog(typeof(DomainSessionFactoryProvider).Assembly);
            catalog.Catalogs.Add(mods);
            mods = new AssemblyCatalog(typeof(InpatientDataset).Assembly);
            catalog.Catalogs.Add(mods);
            mods = new AssemblyCatalog(typeof(Monahrq.Sdk.Services.DatasetService).Assembly);
            catalog.Catalogs.Add(mods);
            return catalog;
        }


        [TestMethod]
        public void DatasetAvailablePositive()
        {
            var services = ServiceLocator.Current.GetInstance<DatasetService>();
            Assert.IsTrue(services.GetInstalledDatasets().Count()>1);
        }

        [TestMethod]
        public void PopulateInpatientModule()
        {
            var mod = new InpatientModule();
             
        }

    }

    [DatasetWingExport]
    public class FooDataSet: DatasetWing<FooDataSet>
    {
        

       
    }

}
