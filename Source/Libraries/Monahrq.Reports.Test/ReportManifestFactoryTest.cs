using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Entities.Domain.Reports;

namespace Monahrq.Reports.Test
{
    [TestClass]
    [Ignore]
    public class ReportManifestFactoryTest
    {
        [TestMethod]
        public void TestSerializePositive()
        {
            var factory = new ReportManifestFactory();
            Assert.IsTrue(factory.InstalledManifests.Any());

        }
    }
}
