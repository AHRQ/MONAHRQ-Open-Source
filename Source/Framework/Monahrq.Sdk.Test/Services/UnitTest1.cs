using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.DataProvider.Builder;
using Monahrq.Sdk.Services;
using Monahrq.TestSupport;
using System.ComponentModel.Composition;

namespace Monahrq.Sdk.Test.Services
{
    [TestClass]
    public class NewDummy: MefTestFixture
    {

        protected override System.Collections.Generic.IEnumerable<Type> InjectedTypes
        {
            get
            {
                return new[] {
                    typeof(DataProviderService), 
                        typeof(FooController) 
                };
            }
        }

        [TestMethod]
        public void TestDummy()
        {
            var service = Container.GetExportedValue<IDataProviderService>();
            Assert.IsNotNull(service);
            var exprts = service.Controllers;
            Assert.IsTrue(exprts.Any());
        }

        [TestMethod]
        public void TestDummyBuilder()
        {
            var service = Container.GetExportedValue<IDataProviderService>();
            Assert.IsNotNull(service);
            var provider = service.Controllers.OfType<IDataProviderController<SqlClientFactory>>().FirstOrDefault();
            Assert.IsNotNull(provider);
            var builder = provider.ProviderFactory.CreateConnectionStringBuilder();
            Assert.IsInstanceOfType(builder, typeof(SqlConnectionStringBuilder));
        }
    }


    [DataProviderControllerExportAttribute("foo",
        typeof(FooController), 
        typeof(FooTypeView), 
        SupportedPlatforms.All, "")]
    public class FooController : DataProviderController<SqlClientFactory>
    {
    }

    public class FooTypeView: IConnectionStringView
    {
        public ConnectionStringViewModel Model
        {
	        get 
            { 
                return new ConnectionStringViewModel(); 
            }
        }
    }
}
