using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;
using System.Collections.Generic;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Sdk.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository;
using Monahrq.Sdk.Extensions;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Core.Attributes;


namespace Monahrq.Sdk.Test.Modules.Wings
{
    //[TestClass]
    //public class WingModuleTest
    //{
    //    [TestMethod]
    //    public void LoadElementScopeTest()
    //    {
    //        var mod = new Testmod();
    //        var wing = mod.FactoryWing();
    //        mod.LoadTarget(wing);
    //        var target = wing.Targets.First();
    //        var scopeTypes = target.Elements.Select(elem => elem.Scope).Where(s => s != null);
    //        Assert.IsTrue(scopeTypes.Count()==2);
    //    }
    //}

    [WingModule(typeof(Testmod), "{FE8F0410-B789-45D6-9AD5-2A65DC3AEEA4}", "Test Mod", "Test Mod")]
    public class Testmod : WingModule
    {

       
    }


    [WingScope("TestEnum", typeof(TestEnum), "This is a test")]
    public enum TestEnum
    {
        [WingScopeValue("Val1", 100, "TestVal1")]
        Val1,
        [WingScopeValue("Val2", 300, "TestVal2")]
        Val2,
    }

    [WingTarget("TestTarget", "ecf295d8-d21e-4318-a43f-2a6675a7835d", false,
        PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
        PublisherEmail = "moanhrq@ahrq.gov",
        PublisherWebsite = "http://monahrq.ahrq.gov/")]
    public class TestTarget : DatasetRecord
    {
        [WingTargetElement("Nullable", false, 0)]
        public TestEnum? TheEnum { get; set; }
        [WingTargetElement("Notnullable", false, 0)]
        public TestEnum TheOtherEnum { get; set; }
    }
}
