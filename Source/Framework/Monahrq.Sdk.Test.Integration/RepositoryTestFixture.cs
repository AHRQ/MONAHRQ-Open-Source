using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.TestSupport;
using Rhino.Mocks;
using Monahrq.Infrastructure;

namespace Monahrq.Sdk.Test.Integration
{
    public class RepositoryTestFixture: MefTestFixture
    {
        protected void AddLoggerStubs()
        {
            var logger = MockRepository.GenerateStub<ILogWriter>();
            Container.ComposeExportedValue<ILogWriter>(LogNames.Session, logger);
            Container.ComposeExportedValue<ILogWriter>(LogNames.Operations,logger);
        }
    }
}
