using System;
using System.Linq;
using System.IO;
using log4net;
using log4net.Config;
using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Services;
using NHibernate;
using Monahrq.Infrastructure.Test.Integration.Services;
using System.Collections.Generic;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Services.Websites;
using LinqKit;

namespace Monahrq.Infrastructure.Test.Integration.Domain.Websites
{
	using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
	
	[TestClass, Ignore]
    public class WebsiteServiceTest : RepositoryTestFixture<WebsiteService,IWebsiteService,  WebsiteServiceTest>
    {

        [TestMethod]
        public override void ServiceCtorPositive()
        {
            base.ServiceCtorPositive();
        }


        [TestMethod]
        public void WebsiteCreatePositive()
        {
            var service = FactoryService();
            var website = service.CreateWebsite();
            Assert.AreEqual(0, website.ActivityLog.Count);
            Assert.AreEqual(0, website.Id);
            Assert.AreEqual(null, website.CurrentStatus);
        }

        [TestMethod]
        public void WebsiteStartPositive()
        {
            var service = FactoryService();
            var website = service.CreateWebsite();
            website.Name = "This is the name!";
            service.Save(website);
            Assert.AreEqual(WebsiteState.Initialized, website.CurrentStatus);
        }



        [TestMethod]
        public void WebsiteCanSelect()
        {
            var service = FactoryService();
            var website = service.CreateWebsite();
            website.Name = "This is the name!";
            service.Save(website);
            var id = website.Id;
            var criteria = PredicateBuilder.False<Website>();
            criteria = criteria.Or(ws => ws.Id == id);
            var result = service.Search(criteria);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(id, result.First().Id);
        }


        [TestMethod]
        public void WebsiteCanCreateAndThenDelete()
        {
            var service = FactoryService();
            var website = service.CreateWebsite();
            website.Name = "This is the name!";
            service.Save(website);
            var id = website.Id;
            var criteria = PredicateBuilder.False<Website>();
            criteria = criteria.Or(ws => ws.Id == id);
            var result = service.Search(criteria);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(id, result.First().Id);
            service.Delete(result.First());
            result = service.Search(criteria);
            Assert.AreEqual(0, result.Count());
            
        }

    }
}
