using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.TestSupport;

namespace Monahrq.Infrastructure.Test
{
	using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
	
	[TestClass]
    public class TestMultipleExportAttributes: MefTestFixture
    {
        protected override IEnumerable<Type> InjectedTypes
        {
            get
            {
                return new Type[] { typeof(FooBar) };
            }
        }

        [TestMethod]
        public void ImportInterfacesSatisfied()
        {
            var foo = Container.GetExportedValue<IFoo>();
            Assert.IsNotNull(foo);
            var bar = Container.GetExportedValue<IBar>();
            Assert.IsNotNull(bar);
        }

        [TestMethod]
        public void TestGroup()
        {
            var listOfN = new List<int>();
            for (int i = 0; i < 40; i++)
            {
                listOfN.Add(i+19);
            }
            var n = listOfN.Count / 3;
            var groups = listOfN.GroupBy(i => listOfN.IndexOf(i) % n).ToList();
        }


    }

    public interface IFoo
    {
    }

    public interface IBar
    {
    }

    [Export(typeof(IFoo))]
    [Export(typeof(IBar))]
    public class FooBar : IFoo, IBar
    {
    }



}

