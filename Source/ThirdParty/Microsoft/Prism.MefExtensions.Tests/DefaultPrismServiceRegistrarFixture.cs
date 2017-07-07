//===================================================================================
// Microsoft patterns & practices
// Composite Application Guidance for Windows Presentation Foundation and Silverlight
//===================================================================================
// Copyright (c) Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===================================================================================
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
//===================================================================================
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Regions.Behaviors;
using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Practices.Prism.MefExtensions.Tests
{
    [TestClass]
    public class DefaultPrismServiceRegistrarFixture
    {
        [TestMethod]
        public void NullAggregateCatalogThrows()
        {
            try
            {
                DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(null);
                Assert.Fail("Expected exception not thrown");
            }
            catch (Exception)
            {
                // no op
            }
        }

        [TestMethod]
        public void SingleSelectorItemsSourceSyncBehaviorIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            foreach (var part in container.Catalog.Parts)
            {
                foreach (var export in part.ExportDefinitions)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Part contract name => '{0}'", export.ContractName));
                }
            }

            var exportedValue = container.GetExportedValue<SelectorItemsSourceSyncBehavior>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleRegionLifetimeBehaviorIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            foreach (var part in container.Catalog.Parts)
            {
                foreach (var export in part.ExportDefinitions)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Part contract name => '{0}'", export.ContractName));
                }
            }

            var exportedValue = container.GetExportedValue<RegionMemberLifetimeBehavior>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleIRegionViewRegistryIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<IRegionViewRegistry>();

            Assert.IsNotNull(exportedValue);
        }


        [TestMethod]
        public void SingleContentControlRegionAdapterIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<ContentControlRegionAdapter>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleIModuleInitializerIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);
            container.ComposeExportedValue<AggregateCatalog>(catalog);

            SetupLoggerForTest(container);
            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<IModuleInitializer>();

            Assert.IsNotNull(exportedValue);
        }


        [TestMethod]
        public void SingleIEventAggregatorIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<IEventAggregator>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleSelectorRegionAdapterIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<SelectorRegionAdapter>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleBindRegionContextToDependencyObjectBehaviorIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<BindRegionContextToDependencyObjectBehavior>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleIRegionManagerIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<IRegionManager>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleRegionAdapterMappingsIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<RegionAdapterMappings>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleItemsControlRegionAdapterIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<ItemsControlRegionAdapter>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleSyncRegionContextWithHostBehaviorIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<SyncRegionContextWithHostBehavior>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleIRegionNavigationServiceIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<IRegionNavigationService>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleIRegionNavigationContentLoaderIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<IRegionNavigationContentLoader>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleRegionManagerRegistrationBehaviorIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<RegionManagerRegistrationBehavior>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleIModuleManagerIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            container.ComposeExportedValue<IModuleCatalog>(new ModuleCatalog());

            container.ComposeExportedValue<AggregateCatalog>(catalog);
            SetupLoggerForTest(container);
            SetupServiceLocator(container);

            DumpExportsFromCompositionContainer(container);

            var exportedValue = container.GetExportedValue<IModuleManager>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleDelayedRegionCreationBehaviorIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<DelayedRegionCreationBehavior>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleAutoPopulateRegionBehaviorIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<AutoPopulateRegionBehavior>();

            Assert.IsNotNull(exportedValue);
        }

        [TestMethod]
        public void SingleRegionActiveAwareBehaviorIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<RegionActiveAwareBehavior>();

            Assert.IsNotNull(exportedValue);
        }

#if !SILVERLIGHT
        [TestMethod]
        public void SingleMefFileModuleTypeLoaderIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            container.ComposeExportedValue<AggregateCatalog>(catalog);
            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<MefFileModuleTypeLoader>();

            Assert.IsNotNull(exportedValue);
        }
#endif

        [TestMethod]
        public void SingleIRegionBehaviorFactoryIsRegisteredWithContainer()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            var newCatalog = DefaultPrismServiceRegistrar.RegisterRequiredPrismServicesIfMissing(catalog);

            CompositionContainer container = new CompositionContainer(newCatalog);

            SetupServiceLocator(container);

            var exportedValue = container.GetExportedValue<IRegionBehaviorFactory>();

            Assert.IsNotNull(exportedValue);
        }

        private void DumpExportsFromCompositionContainer(CompositionContainer container)
        {
            foreach (var part in container.Catalog.Parts)
            {
                foreach (var export in part.ExportDefinitions)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Part contract name => '{0}'", export.ContractName));
                }
            }
        }

        private void SetupServiceLocator(CompositionContainer container)
        {
            container.ComposeExportedValue<IServiceLocator>(new MefServiceLocatorAdapter(container));
            IServiceLocator serviceLocator = container.GetExportedValue<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => serviceLocator);
        }

        private static void SetupLoggerForTest(CompositionContainer container)
        {
            ILoggerFacade logger = new Mock<ILoggerFacade>().Object;
            container.ComposeExportedValue<ILoggerFacade>(logger);
        }
    }
}
