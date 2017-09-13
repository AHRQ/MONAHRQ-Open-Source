using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Configuration.HostConnection;
using Monahrq.Configuration.Properties;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.Modules;
using Monahrq.Sdk.Regions;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using Monahrq.Infrastructure;
using Monahrq.Sdk.Attributes.Wings;

namespace Monahrq.Configuration
{
    static class Constants
    {
        public const string WingGuid = "{F0089BAB-568E-4565-8844-E0A4661E332B}";
    }

    [WingModuleAttribute(typeof(ConfigurationModule), Constants.WingGuid, "Configuration Management", "provides Configuration Management Functionality", InitializationMode = InitializationMode.WhenAvailable)]
    public class ConfigurationModule : IModule
    {
        private readonly ILogWriter _logger;
        private readonly IPluginModuleTracker _pluginTracker;
        private readonly IRegionManager _regionManager;
        private readonly IConfigurationService _configurationService;

        [ImportingConstructor]
        public ConfigurationModule(IServiceLocator locator)
        {
            var logger = locator.GetInstance<ILogWriter>();
            var regionManager = locator.GetInstance<IRegionManager>();
            var pluginTracker = locator.GetInstance<IPluginModuleTracker>();
            var configurationService = locator.GetInstance<IConfigurationService>();
            if (logger == null)
            {
                throw new NullReferenceException("logger of type \"ILoggerFacade\" is required and should not be null.");
            }

            if (regionManager == null)
            {
                throw new NullReferenceException("regionManager should not be null.");
            }

            if (configurationService == null)
            {
                throw new NullReferenceException("logger of type \"IConfigurationService\" is required and should not be null.");
            }

            if (pluginTracker == null)
            {
                throw new NullReferenceException("pluginTracker should not be null.");
            }

            _logger = logger;
            _regionManager = regionManager;
            _configurationService = configurationService;
            _pluginTracker = pluginTracker;
            _pluginTracker.RecordModuleConstructed(GetType().Assembly.GetName().Name);

            // Register the views in this module.
            //_regionManager.RegisterViewWithRegion("DataSetListView", typeof(Views.DataSetListView));
        }

        public void Initialize()
        {
            _pluginTracker.RecordModuleInitialized("Monahrq.Configuration");
        }
    }
}
