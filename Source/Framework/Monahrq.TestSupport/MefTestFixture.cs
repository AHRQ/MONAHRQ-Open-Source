using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Monahrq.Infrastructure;
using Monahrq.TestSupport.Configuration;

namespace Monahrq.TestSupport
{
    public class MefTestFixture : MonahrqCleanConfigurationFixture
    {
        public static CompositionContainer Container
        {
            get;
            private set;
        }

        protected virtual IEnumerable<Type> InjectedTypes
        {
            get
            {
                return new Type[] { 
                     
                };
            }
        }

        [TestInitialize]
        public void RegisterTypes()
        {
            this.RegisterDITypes(InjectedTypes.ToArray());
        }

        protected void RegisterDITypes(IEnumerable<Type> types)
        {
            var typeCatalog = new TypeCatalog(types);
            var catalog = ConfigureAggregateCatalog();
            catalog.Catalogs.Add(typeCatalog);
            Container = new CompositionContainer(catalog);
            ComposeFixtureInstances();
        }

        protected virtual AggregateCatalog ConfigureAggregateCatalog()
        {
            return new AggregateCatalog();
        }

        protected virtual void ComposeFixtureInstances()
        {
            AddLoggerStubs();
            Container.ComposeExportedValue<IServiceLocator>(new Locator(Container));

        }

        [Export(typeof(IServiceLocator))]
        public class Locator : Microsoft.Practices.Prism.MefExtensions.MefServiceLocatorAdapter
        {
            [ImportingConstructor]
            public Locator(CompositionContainer container)
                : base(container)
            {
                Microsoft.Practices.ServiceLocation.ServiceLocator.SetLocatorProvider(()=>this);
            }
        }

        protected virtual void AddLoggerStubs()
        {
            var logger = MockRepository.GenerateStub<ILogWriter>();
            Container.ComposeExportedValue<ILogWriter>(LogNames.Session, logger);
            Container.ComposeExportedValue<ILogWriter>(LogNames.Operations, logger);
        }
    }
}
