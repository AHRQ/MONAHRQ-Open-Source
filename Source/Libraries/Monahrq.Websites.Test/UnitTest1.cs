using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Sdk.Extensibility;
using Monahrq.TestSupport;
using Monahrq.Infrastructure.Configuration;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.MefExtensions.Regions;
using Monahrq.Websites.ViewModels;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure;

namespace Monahrq.Websites.Test
{
    [TestClass]
    [Ignore]
    public class UnitTest1: MefTestFixture
    {

        protected override System.Collections.Generic.IEnumerable<Type> InjectedTypes
        {
            get
            {
                return new[] {
                    typeof(DomainDataServicesProvider)
                };
            }
        }

        protected override System.ComponentModel.Composition.Hosting.AggregateCatalog ConfigureAggregateCatalog()
        {
            var cat = base.ConfigureAggregateCatalog();
            cat.Catalogs.Add(new AssemblyCatalog(typeof(IEntity).Assembly));
            cat.Catalogs.Add(new AssemblyCatalog(typeof(IDomainSessionFactoryProvider).Assembly));
            cat.Catalogs.Add(new AssemblyCatalog(typeof(IMonahrqShell).Assembly));
            cat.Catalogs.Add(new AssemblyCatalog(typeof(RegionManager).Assembly));
            cat.Catalogs.Add(new AssemblyCatalog(typeof(MefRegionViewRegistry).Assembly));
            cat.Catalogs.Add(new AssemblyCatalog(typeof(WebsiteDatasetsViewModel).Assembly));
            return cat;
        }

        protected override void ComposeFixtureInstances()
        {
            base.ComposeFixtureInstances();
            var config = Container.GetExportedValue<IConfigurationService>();
            var settings = config.ConnectionSettings;
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(settings.ConnectionString);
            builder.InitialCatalog = "MONAHRQ_50";
            builder.DataSource = @".\SQLExpress";
            builder.IntegratedSecurity = true;
            settings.ConnectionString = builder.ConnectionString;
            config.ConnectionSettings = settings;
            var logger = Container.GetExportedValue<ILogWriter>(LogNames.Session);
            Container.ComposeExportedValue<ILogWriter>(logger);
        }

        //[TestMethod]
        public void TestMethod1()
        {
            //var temp = Container.GetExportedValue<Websites.ViewModels.WebsiteDatasetsViewModel>();
            //var result = temp.GetDatasetSummary();
        }
    }
}
