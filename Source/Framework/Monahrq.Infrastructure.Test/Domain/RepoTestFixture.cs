using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.TestSupport;
using Rhino.Mocks;
using Monahrq.Infrastructure.Configuration;

namespace Monahrq.Infrastructure.Test.Domain
{
    public class RepoTestFixture: MefTestFixture
    {
        protected override void ComposeFixtureInstances()
        {
            base.ComposeFixtureInstances();
            var config = MockRepository.GenerateStub<IConfigurationService>();
            Container.ComposeExportedValue<IConfigurationService>(config);
            var logger = MockRepository.GenerateStub<ILogWriter>();
            Container.ComposeExportedValue<ILogWriter>(LogNames.Operations, logger);
        }
    }
}
