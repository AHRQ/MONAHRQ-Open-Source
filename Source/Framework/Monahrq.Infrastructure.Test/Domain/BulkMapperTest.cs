using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.Test.Domain
{
    [TestClass]
    public class BulkMapperTest
    {
        [TestMethod]
        public void TestDefaultBulkMapperPositive()
        {
            var dummy = new Dummy();
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("SomeVal", typeof(int)));
            dt.Columns.Add(new DataColumn("SomeOtherVal", typeof(string)));
            var mapper = dummy.CreateBulkInsertMapper<Dummy>(dt);
        }
    }


    public class Dummy : Entity<int>
    {
        public int SomeVal { get; set; }
        public string SomeOtherVal { get; set; }
    }
}
