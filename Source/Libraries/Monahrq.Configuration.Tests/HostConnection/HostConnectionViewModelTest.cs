using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.TestSupport;
using Monahrq.Configuration.HostConnection;
using Rhino.Mocks;
using Microsoft.Practices.Prism.Events;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Configuration;
using System.Data.SqlClient;
using Monahrq.Sdk.DataProvider;
using System.Configuration;

namespace Monahrq.Configuration.Tests.HostConnection
{
    [TestClass]
    public class HostConnectionViewModelTest : MefTestFixture
    {
        protected override System.Collections.Generic.IEnumerable<Type> InjectedTypes
        {
            get
            {
                return new[] { typeof(HostConnectionViewModel) };
            }
        }

        const string datasource = "datasource";
        const string catalog = "catalog";
        const string username = "username";
        const string password = "password";
        bool? integratedsec = false;

        System.Data.Common.DbConnectionStringBuilder Builder
        {
            get
            {
                var builder = new SqlConnectionStringBuilder();
                builder.DataSource = datasource;
                builder.InitialCatalog = catalog;
                builder.UserID = username;
                builder.Password = password;
                builder.IntegratedSecurity = integratedsec.GetValueOrDefault();
                return builder;
            }
        }

        protected override void ComposeFixtureInstances()
        {
            base.ComposeFixtureInstances();
            var events = MockRepository.GenerateStub<IEventAggregator>();
            Container.ComposeExportedValue<IEventAggregator>(events);
            var dbCreator = MockRepository.GenerateStub<IDatabaseCreator>();
            Container.ComposeExportedValue<IDatabaseCreator>(CreatorNames.Sql, dbCreator);
        }

        void LoadConfiguration(bool? integrated)
        {
            var config = MockRepository.GenerateStub<IConfigurationService>();
            integratedsec = integrated;
            var conn = integratedsec.HasValue ? Builder.ConnectionString : string.Empty;
            var temp = ConnectionStringSettingsElement.Default;
            config.ConnectionSettings = new ConnectionStringSettings("Monahrq", conn, temp.ProviderName);
            Container.ComposeExportedValue<IConfigurationService>(config);
        }

        [TestMethod]
        [Ignore]
        public void TestLoadWithPassword()
        {
            LoadConfiguration(false);
            var viewModel = Container.GetExportedValue<HostConnectionViewModel>();
            Assert.AreEqual(datasource, viewModel.Host);
            Assert.AreEqual(catalog, viewModel.Database);
            Assert.AreEqual(username, viewModel.User);
            Assert.AreEqual(password, viewModel.Password);
            Assert.AreEqual(false, viewModel.UseIntegratedSecurity);
        }

        [TestMethod]
        [Ignore]
        public void TestLoadWithIntegratedSec()
        {
            LoadConfiguration(true);
            var viewModel = Container.GetExportedValue<HostConnectionViewModel>();
            Assert.AreEqual(datasource, viewModel.Host);
            Assert.AreEqual(catalog, viewModel.Database);
            Assert.IsTrue(string.IsNullOrEmpty(viewModel.User));
            Assert.IsTrue(string.IsNullOrEmpty(viewModel.Password));
            Assert.AreEqual(true, viewModel.UseIntegratedSecurity);
        }

        [TestMethod]
        [Ignore]
        public void TestLoadWithNoConfig()
        {
            LoadConfiguration(null);
            var viewModel = Container.GetExportedValue<HostConnectionViewModel>();
            Assert.IsTrue(string.IsNullOrEmpty(viewModel.Host));
            Assert.IsTrue(string.IsNullOrEmpty(viewModel.Database));
            Assert.IsTrue(string.IsNullOrEmpty(viewModel.User));
            Assert.IsTrue(string.IsNullOrEmpty(viewModel.Password));
            Assert.AreEqual(false, viewModel.UseIntegratedSecurity);
        }


    }
}
