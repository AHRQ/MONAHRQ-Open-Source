using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Sdk.DataProvider;
using System.Data.OleDb;
using System.Data.SqlClient;
using Rhino.Mocks;
using Monahrq.Sdk.DataProvider.Builder;

namespace Monahrq.Sdk.Test.DataProvider
{
    [TestClass]
    public class DataProviderControllerExportAttributeTest
    {
        List<Type> providers = DbProviderFactories.GetFactoryClasses()
            .Rows.OfType<DataRow>().Select(row => Type.GetType(row["AssemblyQualifiedName"].ToString())).ToList();
          
        [TestMethod]
        public void OleDbProviderViewProviderNameIsFound()
        {
            var mockControllerType = 
                MockRepository.GenerateMock<IDataProviderController<SqlClientFactory>>().GetType();
            var mockViewType =
                MockRepository.GenerateMock<IConnectionStringView>().GetType();
             
            IDataProviderControllerExportAttribute temp = new 
                DataProviderControllerExportAttribute(
                "Dummy"
                , mockControllerType
                , mockViewType
                , SupportedPlatforms.x86, "");
            Assert.IsTrue(providers.Contains(temp.DbProviderFactoryType));
            Assert.IsTrue(temp.DbProviderFactoryType == typeof(SqlClientFactory));
        }

        [TestMethod]
        public void OleDbProviderViewStringBuilderPositive()
        {
            var expected = "foobar";
            IDataProviderControllerExportAttribute temp = new 
                DataProviderControllerExportAttribute(
                "Dummy"
                , typeof(DummyProviderController)
                , typeof(DummyStringView)
                , SupportedPlatforms.x86, "Provider="+ expected);
            Assert.IsNotNull(temp.CreateConnectionStringBuilder());
            PrivateObject privSub = new PrivateObject(temp,
               new PrivateType(typeof(DataProviderControllerExportAttribute)));
            var builder = temp.CreateConnectionStringBuilder();
            Assert.IsInstanceOfType(builder, typeof(OleDbConnectionStringBuilder));
            Assert.AreEqual((builder as OleDbConnectionStringBuilder).Provider, expected);
        }

        [TestMethod]
        public void OleDbProviderViewContractTypePositive()
        {
            IDataProviderControllerExportAttribute temp = new
                DataProviderControllerExportAttribute(
                "Dummy"
                , typeof(DummyProviderController)
                , typeof(DummyStringView)
                , SupportedPlatforms.x86, "");
            Assert.AreEqual(typeof(DummyProviderController), temp.ControllerType);
        }
    }
}
