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
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Regions.Behaviors;
using Microsoft.Practices.Prism.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.Prism.Tests.Regions.Behaviors
{
    [TestClass]
    public class ClearChildViewsRegionBehaviorFixture
    {
        [TestMethod]
        public void WhenClearChildViewsPropertyIsNotSet_ThenChildViewsRegionManagerIsNotCleared()
        {
            var regionManager = new MockRegionManager();

            var region = new Region();
            region.RegionManager = regionManager;

            var behavior = new ClearChildViewsRegionBehavior();
            behavior.Region = region;
            behavior.Attach();

            var childView = new MockFrameworkElement();
            region.Add(childView);

            Assert.AreEqual(regionManager, childView.GetValue(RegionManager.RegionManagerProperty));

            region.RegionManager = null;

            Assert.AreEqual(regionManager, childView.GetValue(RegionManager.RegionManagerProperty));
        }

        [TestMethod]
        public void WhenClearChildViewsPropertyIsTrue_ThenChildViewsRegionManagerIsCleared()
        {
            var regionManager = new MockRegionManager();

            var region = new Region();
            region.RegionManager = regionManager;

            var behavior = new ClearChildViewsRegionBehavior();
            behavior.Region = region;
            behavior.Attach();

            var childView = new MockFrameworkElement();
            region.Add(childView);

            ClearChildViewsRegionBehavior.SetClearChildViews(childView, true);

            Assert.AreEqual(regionManager, childView.GetValue(RegionManager.RegionManagerProperty));

            region.RegionManager = null;

            Assert.IsNull(childView.GetValue(RegionManager.RegionManagerProperty));
        }

        [TestMethod]
        public void WhenRegionManagerChangesToNotNullValue_ThenChildViewsRegionManagerIsNotCleared()
        {
            var regionManager = new MockRegionManager();

            var region = new Region();
            region.RegionManager = regionManager;

            var behavior = new ClearChildViewsRegionBehavior();
            behavior.Region = region;
            behavior.Attach();

            var childView = new MockFrameworkElement();
            region.Add(childView);

            childView.SetValue(ClearChildViewsRegionBehavior.ClearChildViewsProperty, true);

            Assert.AreEqual(regionManager, childView.GetValue(RegionManager.RegionManagerProperty));

            region.RegionManager = new MockRegionManager();

            Assert.IsNotNull(childView.GetValue(RegionManager.RegionManagerProperty));
        }
    }
}
