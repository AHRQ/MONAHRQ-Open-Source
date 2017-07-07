using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.Data.SqlClient;

using Monahrq.Infrastructure.Test.Integration.Domain;
using Monahrq.Infrastructure.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using System.Collections.Generic;
using Monahrq.Infrastructure.Entities.Domain;
using LinqKit;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository;
using System.Configuration;


namespace Monahrq.Infrastructure.Test.Integration.Domain
{
	using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
	
	[TestClass, Ignore]
    public class WingServiceTest : RepositoryTestFixture
    {

        protected override void ComposeFixtureInstances()
        {
            AddLoggerStubs();
            base.ComposeFixtureInstances();
        }

        protected override IEnumerable<Type> InjectedTypes
        {
            get
            {
                return new Type[] { 
                    typeof(WingRepository), 
                    //typeof(WingsService), 
                    typeof(DomainSessionFactoryProvider),
                    typeof(TargetRepository)
                  
        };
            }
        }

        [TestMethod]
        public void WingServiceExportPositive()
        {
            var service = Container.GetExportedValue<IDomainSessionFactoryProvider>();
            Assert.IsNotNull(service);
        }

        //[TestMethod]
        //public void WingServiceSavePositive()
        //{
        //    var service = Container.GetExportedValue<IWingsService>();
        //    Wing wing = WingRepository.New(Guid.NewGuid().ToString());
        //    var notExpected = wing.Id;
        //    wing.WingGUID = Guid.NewGuid();
        //    service.Save(wing);
        //    var crit = PredicateBuilder.True<Wing>()
        //        .And(item => item.WingGUID == wing.WingGUID);
        //    var result = service.Search(crit);
        //    Assert.IsTrue(result.Result.Count == 1);
        //    var target = result.Result.FirstOrDefault();
        //    Assert.AreNotEqual(notExpected, result);
        //    Assert.AreEqual(wing.Id, target.Id);
        //}

        //[TestMethod]
        //public void WingServiceSelectPositive()
        //{
        //    var service = Container.GetExportedValue<IWingsService>();
        //    var criteria = PredicateBuilder.True<Wing>();
        //    var op = service.Search(criteria);
        //    Assert.IsNotNull(op);
        //    var result = op.Result;
        //    var wing = result.FirstOrDefault();
        //    if (wing == null)
        //    {
        //        wing = WingRepository.New(Guid.NewGuid().ToString());
        //        service.Save(wing);
        //    }

        //    var targetCriteria = PredicateBuilder.True<Target>()
        //        .And(item => item.Owner.Id == wing.Id);
        //    var targetSearch = service.Search(targetCriteria);
        //    var target = targetSearch.Result.FirstOrDefault();
        //    if (target == null)
        //    {
        //        target = TargetRepository.New(wing, Guid.NewGuid().ToString());
        //        service.Save(wing);
        //    }
        //    targetSearch = service.Search(targetCriteria);
        //    target = targetSearch.Result.FirstOrDefault();
        //    Assert.AreEqual(target.Owner.Id, wing.Id);
        //}


        //[TestMethod]
        //public void WingServiceAddPositive()
        //{
        //    var service = Container.GetExportedValue<IWingsService>();
        //    var wing = WingRepository.New(Guid.NewGuid().ToString());
        //    var notExpected = wing.Id;
        //    wing.WingGUID = Guid.NewGuid();
        //    service.Save(wing);
        //    Assert.IsTrue(wing.Id != notExpected);
        //    var crit = PredicateBuilder.True<Wing>()
        //        .And(m => m.Id == wing.Id);
        //    var op = service.Search(crit);
        //    Assert.IsNotNull(op);
        //    Assert.IsTrue(op.Result.Count == 1);
        //    Assert.AreEqual(wing.Id, op.Result.First().Id);
        //    Assert.AreEqual(wing.Name, op.Result.First().Name);
        //}

       

        //[TestMethod]
        //public void WingServiceAddPositiv2e()
        //{
        //    var service = Container.GetExportedValue<IWingsService>();
        //    var wing = WingRepository.New( Guid.NewGuid().ToString());
        //    var notExpected = wing.Id;
        //    wing.WingGUID = Guid.NewGuid();
        //    var target = TargetRepository.New(wing, Guid.NewGuid().ToString());
        //    target.Description = Guid.NewGuid().ToString();
        //    var element = ElementRepository.New(target, Guid.NewGuid().ToString());
        //    service.Save(wing);
        //    Assert.IsTrue(wing.Id != notExpected);
        //    var crit = PredicateBuilder.True<Wing>()
        //        .And(m => m.Id == wing.Id);
        //    var op = service.Search(crit);
        //    Assert.IsNotNull(op);
        //    Assert.IsTrue(op.Result.Count == 1);
        //    Assert.AreEqual(wing.Id, op.Result.First().Id);
        //    Assert.AreEqual(wing.Name, op.Result.First().Name);
        //}
    }
}
