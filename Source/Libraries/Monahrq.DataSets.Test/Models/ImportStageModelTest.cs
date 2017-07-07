using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Monahrq.Infrastructure.Configuration;
using Monahrq.DataSets.Model;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Monahrq.Sdk.DataProvider;
using Monahrq.DataSets.Services;

namespace Monahrq.DataSets.Test.Models
{
    [TestClass]
    public class ImportStageModelTest
    {
        [TestMethod]
        public void TestCtorConnectionSetPositive()
        {
            var conn = MockRepository.GenerateStub<IDataContextServices>();
            var provider = new DataproviderFactory(conn);
            var stage = new ImportStageModel(provider, 100);
            var priv = new PrivateObject(stage);
            Assert.AreSame(provider, priv.GetFieldOrProperty("ProviderFactory"));
        }

        [TestMethod]
        public void TestCtorStageSizeSetPositive()
        {

            var stageSize = 100;
            var conn = MockRepository.GenerateStub<IDataContextServices>();
            var provider = new DataproviderFactory(conn);
            var stage = new ImportStageModel(provider, stageSize);
            Assert.AreEqual(stageSize, stage.StageSize);
        }

        [TestMethod]
        public void TestCtorDefaultStageSize()
        {
            var conn = MockRepository.GenerateStub<IDataContextServices>();
            var provider = new DataproviderFactory(conn);
            var stage = new ImportStageModel(provider);
            var p = new PrivateType(typeof(ImportStageModel));
            var expected = p.GetStaticField("DefaultStageSize");
            var actual = stage.StageSize;
            Assert.AreEqual(expected, actual);
        }

        //test public List<string> GetColumnValues(string columnName)
        [TestMethod]
        public void TestGetColumnValues()
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn("foo"));
            var vals = new List<object>() { DateTime.Now, Guid.NewGuid(), string.Empty, 9999, null };
            vals.ForEach(i => dt.Rows.Add(i));

            ImportStageModel stage = MockTheStage(dt);
            var actual = stage.GetColumnValues("foo");
            var expected = vals.Select(i => (i ?? string.Empty).ToString()).ToList();
            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < actual.Count; i++)
            { 
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        private static ImportStageModel MockTheStage(DataTable dt)
        {
            var services = MockRepository.GenerateStub<IDataContextServices>();
            var prov = MockRepository.GenerateStub<IDataProviderController>();
            var connElement = MockRepository.GenerateStub<INamedConnectionElement>();
            connElement.SelectFrom = "foo";
            services.Stub(s => s.Configuration).Return(connElement);
            services.Stub(s => s.Controller).Return(prov);
            var conn = MockRepository.GenerateStub<IDbConnection>();
            conn.ConnectionString = string.Empty;
            services.ConnectionFactory = () => conn;
            var factory = new DataproviderFactory(services);
            prov.Expect(p => p.SelectTable(string.Empty, string.Empty, 100)).IgnoreArguments().Return(dt);
            return new ImportStageModel(factory);
        }

        // public IEnumerable<string> SourceFields
        [TestMethod]
        public void TestSourceFields()
        {
            var dt = new DataTable(); 
            var stage = MockTheStage(dt);
            dt.Columns.Add(new DataColumn("foo"));
            var actual = stage.SourceFields.ToList();
            var expected = new List<string>() { "foo" };
            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}
