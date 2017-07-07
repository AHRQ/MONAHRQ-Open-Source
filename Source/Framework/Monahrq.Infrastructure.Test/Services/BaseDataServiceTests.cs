using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Services.BaseData;
using System.ComponentModel;
using Monahrq.TestSupport;

namespace Monahrq.Infrastructure.Test
{
	using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
	
	[TestClass]
    public class BaseDataServiceTests
    {
        [TestMethod]
        public void ReportingQuartersTest()
        {
            var service = new BaseDataService(null);
            Assert.IsNotNull(service);

            // test that ReportingYears contains years sorted from 2009 to current year, as strings
            var list = service.ReportingYears;
            Assert.AreEqual("2009", list[0]);
            Assert.AreEqual("2010", list[1]);
            Assert.AreEqual("2011", list[2]);
            Assert.AreEqual("2012", list[3]);
            Assert.AreEqual("2013", list[4]);
            //Assert.AreEqual("2014", list[5]);         // uncomment on Jan-1 2014
            //Assert.AreEqual("2015", list[6]);         // uncomment on Jan-1 2015
        }
    }
}
