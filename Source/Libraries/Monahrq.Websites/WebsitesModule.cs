using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Sdk.Modules;
using Monahrq.Sdk.Regions;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Modules;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Websites.Views;

namespace Monahrq.Websites
{

    static class Constants
    {
        public const string WingGuid = "{C36C0DB0-0B73-4C5B-9A5F-4233AA781A58}";
    }

    [Export(typeof(IModuleRegionRegistrar))]
    [WingModule(typeof(WebsitesModule), Constants.WingGuid, "Websites Wing", "Provides Services Creating Websites",
       DependsOnModuleNames = new [] { "Base Data" }, InitializationMode = InitializationMode.WhenAvailable)]
    public class WebsitesModule : IModule, IModuleRegionRegistrar
    {
        private readonly ILogWriter _logger;
        private readonly IPluginModuleTracker _pluginTracker;
        private IRegionManager _regionManager;

        [ImportingConstructor]
        public WebsitesModule(IServiceLocator locator)
        {
            var logger = locator.GetInstance<ILogWriter>();
            var regionManager = locator.GetInstance<IRegionManager>();
            var pluginTracker = locator.GetInstance<IPluginModuleTracker>();
            if (logger == null)
            {
                throw new NullReferenceException("logger of type \"ILoggerFacade\" is required and should not be null.");
            }

            if (regionManager == null)
            {
                throw new NullReferenceException("regionManager should not be null.");
            }

            if (pluginTracker == null)
            {
                throw new NullReferenceException("pluginTracker should not be null.");
            }

            _logger = logger;
            _regionManager = regionManager;
            _pluginTracker = pluginTracker;
            _pluginTracker.RecordModuleConstructed(this.GetType().Assembly.GetName().Name);

            // Register the views in this module.
            //_regionManager.RegisterViewWithRegion("DataSetListView", typeof(Views.DataSetListView));
        }

        public void Initialize()
        {
            _pluginTracker.RecordModuleInitialized("Monahrq.Websites");

            //_regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
           
        }

        public void RegisterRegions(IRegionManager regionManager)
        {
            try
            {
                _regionManager = regionManager;
                _regionManager.RegisterViewWithRegion(RegionNames.MainContent, typeof (WebsiteCollectionView));
                _regionManager.RegisterViewWithRegion(RegionNames.MainContent, typeof (WebsiteManageView));

                if (!_regionManager.Regions.ContainsRegionWithName(RegionNames.MainContent)) return;

                var mainContentRegion =
                    regionManager.Regions.ToList().FirstOrDefault(r => r.Name.EqualsIgnoreCase(RegionNames.MainContent));

                if (mainContentRegion == null) return;

                if (mainContentRegion.RegionManager == null)
                    mainContentRegion.RegionManager = regionManager.CreateRegionManager();

                mainContentRegion.RegionManager.RegisterViewWithRegion(RegionNames.WebsiteManageRegion, typeof (WebsiteDetailsView));
                mainContentRegion.RegionManager.RegisterViewWithRegion(RegionNames.WebsiteManageRegion, typeof (WebsiteDatasetsView));
                mainContentRegion.RegionManager.RegisterViewWithRegion(RegionNames.WebsiteManageRegion, typeof (WebsiteMeasuresView));
                mainContentRegion.RegionManager.RegisterViewWithRegion(RegionNames.WebsiteManageRegion, typeof (WebsiteBuildReportsTabsView));
                mainContentRegion.RegionManager.RegisterViewWithRegion(RegionNames.WebsiteManageRegion, typeof (WebsiteSettingsView));
                mainContentRegion.RegionManager.RegisterViewWithRegion(RegionNames.WebsiteManageRegion, typeof (WebsitePublishView));
				mainContentRegion.RegionManager.RegisterViewWithRegion(RegionNames.WebsiteSettingsRegion, typeof (WebsitePagesView));
				mainContentRegion.RegionManager.RegisterViewWithRegion(RegionNames.WebsiteSettingsRegion, typeof (WebsitePagesListView));
			}
            catch (Exception exc)
            {
                _logger.Write(exc);
                throw;
            }
        }
    }
}
