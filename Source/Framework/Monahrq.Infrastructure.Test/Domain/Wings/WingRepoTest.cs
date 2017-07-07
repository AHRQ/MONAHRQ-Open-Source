using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository;
using Monahrq.TestSupport;
using Monahrq.TestSupport.Loggers;

namespace Monahrq.Infrastructure.Test.Domain.Wings
{
    [TestClass]
    public class WingRepoTest: MefTestFixture
    {

        protected override IEnumerable<Type> InjectedTypes
        {
            get
            {
                return new[] { typeof(WingRepository) };
            }
        }

        protected override void ComposeFixtureInstances()
        {
            var provider = Rhino.Mocks.MockRepository.GenerateMock<IDomainSessionFactoryProvider>();
            Container.ComposeExportedValue<ILogWriter>(LogNames.Session, new DummyLogger());
            Container.ComposeExportedValue<ILogWriter>(LogNames.Operations, new DummyLogger());
            Container.ComposeExportedValue<IDomainSessionFactoryProvider>(provider);
        }
 
    }

   
}
