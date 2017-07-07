using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Sdk.DataProvider.Builder;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.DataProvider;

namespace Monahrq.Sdk.Test.DataProvider.Builder
{
    [TestClass]
    public class SchemaIniTest : ConnectionStringBuilderViewModelTestOneProviderBase
    {
        [TestMethod]
        public void AssignToSchemaIni()
        {
            var model = new ConnectionStringViewModel();
            var prov = Container.GetExportedValue<IDataProviderController>();
            model.Reset(prov);
            Assert.IsNotNull(model);

            var schemaIni = new SchemaIniElement("name", "value");
            model.Configuration = new NamedConnectionElement();
            Assert.IsTrue(model.Configuration.SchemaIniSettings.Count == 0);

            model.Configuration.SchemaIniSettings.Add(schemaIni);
            Assert.IsTrue(model.Configuration.SchemaIniSettings.Count == 1);
            
            var col = new SchemaIniElementCollection();
            col.Add(schemaIni);
            model.Configuration.SchemaIniSettings = col;
            Assert.IsTrue(model.Configuration.SchemaIniSettings.Count == 1);
            
            var SchemaIniSettings = new SchemaIniElementCollection();
            SchemaIniSettings.Add(schemaIni);
            model.Configuration = new NamedConnectionElement();
            model.Configuration.SchemaIniSettings = SchemaIniSettings;
            Assert.IsTrue(model.Configuration.SchemaIniSettings.Count == 1);
        }
    }
}
