using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace Monahrq.Sdk.Test.Integration.DataProvider
{
    [TestClass]
    [Ignore]
    public class DbCreatorTest
    {
        const string DbToCreate = "{87A1D771-B045-41FF-9313-216F1E6B65DB}";
        [TestMethod]
        public void TestCreate()
        {
            var builder = new SqlConnectionStringBuilder();
            builder.DataSource = @".\SQLExpress";
            builder.IntegratedSecurity = true;
            using (var conn = new SqlConnection(builder.ConnectionString))
            {
                conn.Open();

            }
        }
    }
}
