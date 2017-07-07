using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Rhino.Mocks;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository;
using Monahrq.Wing.Discharge.Inpatient;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Sdk.Test.Integration
{
    [TestClass]
    [Ignore]
    public class UnitTest1 : RepositoryTestFixture
    {

        protected override void ComposeFixtureInstances()
        {
            AddLoggerStubs();
            base.ComposeFixtureInstances();
            var config = MockRepository.GenerateMock<IConfigurationService>();
            var builder = new SqlConnectionStringBuilder();
            builder.InitialCatalog = "MONAHRQ_50";
            builder.DataSource = "castor";
            builder.IntegratedSecurity = true;

            var temp = config.ConnectionSettings;
            temp.ConnectionString = builder.ConnectionString;
            config.Stub(c => c.ConnectionSettings).Return(temp);
            Container.ComposeExportedValue<IConfigurationService>(config);
        }

        protected override IEnumerable<Type> InjectedTypes
        {
            get
            {
                return new Type[] { 
                    typeof(WingRepository), 
                    //typeof(WingsService), 
                    typeof(DomainSessionFactoryProvider),
                    typeof(TargetRepository),
                    typeof(InpatientModule)

        };
            }
        }
        [TestMethod]
        public void WingServiceExportPositive()
        {
            var wing = Container.GetExportedValues<IModule>().Where(mod => mod is InpatientModule).FirstOrDefault();
            wing.Initialize();
        }

    }
}
