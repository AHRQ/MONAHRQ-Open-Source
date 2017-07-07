using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.TestSupport;
using Rhino.Mocks;
using Monahrq.Infrastructure.Configuration;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using log4net.Config;
using log4net;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Test.Integration.Services;
using Microsoft.Practices.ServiceLocation;

namespace Monahrq.Infrastructure.Test.Integration.Domain
{
	using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    public class RepositoryTestFixture  : MefTestFixture
    {

        protected virtual bool LogSql
        {
            get { return false; }
          
        }

        protected override void ComposeFixtureInstances()
        {
            base.ComposeFixtureInstances();
            var mockConfig = MockRepository.GenerateMock<IConfigurationService>();
            ConnectionStringSettings settings = new ConnectionStringSettings("monahrq", Properties.Settings.Default.TestConnection);
            IMonahrqSettings monahrqSettings = MockRepository.GenerateMock<IMonahrqSettings>();
            monahrqSettings.Stub(ms => ms.RebuildDatabase).Return(true);
            monahrqSettings.Stub(ms => ms.DebugSql).Return(LogSql);
            monahrqSettings.Stub(ms => ms.LongTimeout).Return(TimeSpan.FromMinutes(3));
            monahrqSettings.Stub(ms => ms.ShortTimeout).Return(TimeSpan.FromMinutes(1));
            monahrqSettings.Stub(ms => ms.BatchSize).Return(1000);
            mockConfig.Stub(cfg => cfg.ConnectionSettings).Return(settings);
            mockConfig.Stub(cfg => cfg.MonahrqSettings).Return(monahrqSettings);
            Container.ComposeExportedValue<IConfigurationService>(mockConfig);
        }

    }


    public class RepositoryTestFixture<TServiceUnderTest, TContractType, TTestType> : RepositoryTestFixture
    {

        [ClassInitialize()]
        public static void MyTestInitialize(TestContext testContext)
        {
            // Take care the log4net.config file is added to the deployment files of the testconfig
            FileInfo fileInfo;
            string fullPath = Path.Combine(System.Environment.CurrentDirectory, "log4net.config");
            fileInfo = new FileInfo(fullPath);
            // Reload the configuration
            XmlConfigurator.Configure(fileInfo);
            // test to see if it works
            // Logger instance named "MyApp".
            ILog log = LogManager.GetLogger(typeof(TTestType));
            log.Info("check");
        }

        protected override System.Collections.Generic.IEnumerable<Type> InjectedTypes
        {
            get
            {
                var typesForDerivedTests = new Type[] 
                {
                    typeof(DomainSessionFactoryProvider) 
                    , typeof(TServiceUnderTest)
                //    , typeof(ConfigurationService)
                    , typeof(ClassMappingTypeProvider)};
                
                var typesFromBaseTypeAndCurrentType = base.InjectedTypes;
                var classMappingProvider = new ClassMappingTypeProvider();
                var temp = typesFromBaseTypeAndCurrentType
                    .Concat(typesForDerivedTests)
                    .Concat(classMappingProvider.ProviderTypes)
                    .Concat(classMappingProvider.ConventionTypes);
                return temp.Distinct();
            }
        }

        public virtual void ServiceCtorPositive()
        {
            var temp = FactoryService();
            Assert.IsNotNull(temp);
        }

        const string empty = "";

        protected TContractType FactoryService(string key = empty)
        {
            return ServiceLocator.Current.GetInstance<TContractType>(empty);
        }


    }
}
