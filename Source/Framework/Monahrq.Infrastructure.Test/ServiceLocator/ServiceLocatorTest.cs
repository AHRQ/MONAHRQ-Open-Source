using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.TestSupport;

namespace Monahrq.Infrastructure.Test.ServiceLocator
{
	using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
	
	[TestClass]
    public class ServiceLocatorTest: MefTestFixture
    {
        protected override System.Collections.Generic.IEnumerable<Type> InjectedTypes
        {
            get
            {
                return new[] {typeof(GetsLocator), 

                    typeof(FoundByLocator), 
                        typeof(Injected), 
                    typeof(Imported)
                };
            }
        }

        [TestMethod]
        public void TestLocatorImported()
        {
            var target = Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<GetsLocator>();
            Assert.IsNotNull(target.Found);
            Assert.IsNotNull(target.Injected);
            PrivateObject priv = new PrivateObject(target);
            var val = priv.GetProperty("Imported");
            Assert.IsNotNull(val);
        }
    }


    [Export]
    public class GetsLocator
    {
       
        public FoundByLocator Found
        {
            get;
            set;
        }

        public Injected Injected
        {
            get;
            set;
        }

        [Import]
        private Imported Imported
        {
            get;
            set;
        }

        [ImportingConstructor]
        public GetsLocator(Injected injected)
        {
            Injected = injected;
            Found = Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<FoundByLocator>();
        }

    }

    [Export]
    public class FoundByLocator
    {
    }

    [Export]
    public class Imported
    { 
    }

    [Export]
    public class Injected
    {
    }

}
