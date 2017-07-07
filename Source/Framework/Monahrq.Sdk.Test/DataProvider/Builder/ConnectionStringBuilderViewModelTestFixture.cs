using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Sdk.DataProvider.Builder;
using Monahrq.Sdk.Services;
using Monahrq.TestSupport;

using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure;


namespace Monahrq.Sdk.Test.DataProvider.Builder
{
    public class ConnectionStringBuilderViewModelTestFixture : MefTestFixture
    {
        protected override IEnumerable<Type> InjectedTypes
        {
            get
            {
                return base.InjectedTypes.Concat(new[] 
                { 
                   typeof(DataProviderService)
                   , typeof(DummyBuilderController)
                   , typeof(ConnectionStringBuilderViewModel)
                }).ToArray();
            }
        }

        protected override void ComposeFixtureInstances()
        {
            base.ComposeFixtureInstances();
            var regionManager = new RegionManager();
            regionManager.Regions.Add(new Region() { Name = DialogRegionNames.ConnectionStringViewRegion });
            Container.ComposeExportedValue<IRegionManager>(regionManager);
        //    Container.ComposeExportedValue<ILogWriter>(LogNames.Session, NullLogger.Instance);
        //    Container.ComposeExportedValue<ILogWriter>(LogNames.Operations, NullLogger.Instance);
        }

        [Export(typeof(IConnectionStringBuilderController))]
        public class DummyBuilderController : IConnectionStringBuilderController
        {
            public Sdk.DataProvider.IDataProviderController Provider
            {
                get;
                set;
            }

            public void TestConnection(string element)
            {

            }

            public void SaveConnection(NamedConnectionElement element)
            {

            }

            public void Cancel()
            {

            }
        }
    }

    public class ConnectionStringBuilderViewModelTestOneProviderBase : ConnectionStringBuilderViewModelTestFixture
    {
        protected override IEnumerable<Type> InjectedTypes
        {
            get
            {
                return base.InjectedTypes.Concat(new[] 
                { 
                   typeof(DummyProviderController),
                   typeof(DummyStringView)
                }).ToArray();
            }
        }
    }

    public class ConnectionStringBuilderViewModelTestTwoProviderBase : ConnectionStringBuilderViewModelTestFixture
    {
        protected override IEnumerable<Type> InjectedTypes
        {
            get
            {
                return base.InjectedTypes.Concat(new[] 
                { 
                   typeof(DummyProviderController),
                   typeof(AnotherDummyProviderController)
                }).ToArray();
            }
        }
    }


}
