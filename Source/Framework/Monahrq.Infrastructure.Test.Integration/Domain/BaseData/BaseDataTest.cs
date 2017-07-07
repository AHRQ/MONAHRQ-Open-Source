using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using log4net;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Infrastructure.Test.Integration.Services;

namespace Monahrq.Infrastructure.Test.Integration.Domain.BaseData
{
	using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
	
	[TestClass, Ignore]
    public class BaseDataTest : RepositoryTestFixture<BaseDataService,IBaseDataService, BaseDataTest>
    {

        private IEnumerable<Type> ApplyTestClassMaps(IEnumerable<Type> temp)
        {
            var mappingTypeProvider = new ClassMappingTypeProvider();
            return temp.Concat(mappingTypeProvider.ProviderTypes).Concat(mappingTypeProvider.ConventionTypes);
        }

        // see Monahrq.Infrastructure.Test.Services.BaseDataServiceTests...
        [TestMethod]
        public void BaseDataServiceNotNull()
        {
            var service = Container.GetExportedValue<IBaseDataService>();
            Assert.IsNotNull(service);
        }
    }
}
