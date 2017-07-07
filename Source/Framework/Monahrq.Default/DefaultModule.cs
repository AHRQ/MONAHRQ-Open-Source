using System;
using System.ComponentModel.Composition;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Default.Controllers;
using Monahrq.Default.Views;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Modules;
using Monahrq.Sdk.Regions;

namespace Monahrq.Default
{
    /// <summary>
    /// class for wing unique identifier
    /// </summary>
    static class Constants
    {
        /// <summary>
        /// The wing unique identifier
        /// </summary>
        public const string WingGuid = "{9DF5C51A-8ED6-4609-AAD2-72E1CC0B32CD}";
    }
    /// <summary>
    /// A module for the quickstart.
    /// </summary>
    [WingModuleAttribute(typeof(DefaultModule), Constants.WingGuid,
        "Default Module", "Default Application Functionality")]
    public class DefaultModule : IModule
    {


        /// <summary>
        /// Gets the popup region.
        /// </summary>
        /// <value>
        /// The popup region.
        /// </value>
        IRegion PopupRegion
        {
            get
            {
                return _regionManager.Regions[RegionNames.Modal];
            }
        }

        /// <summary>
        /// Gets or sets the original popup region style.
        /// </summary>
        /// <value>
        /// The original popup region style.
        /// </value>
        static Style OriginalPopupRegionStyle { get; set; }
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILoggerFacade _logger;
        /// <summary>
        /// The plugin tracker
        /// </summary>
        private readonly IPluginModuleTracker _pluginTracker;
        /// <summary>
        /// The region manager
        /// </summary>
        private readonly IRegionManager _regionManager;
        /// <summary>
        /// Gets or sets the simple import complete handler.
        /// </summary>
        /// <value>
        /// The simple import complete handler.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        ISimpleImportCompleteHandler SimpleImportCompleteHandler { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Monahrq.Default" /> class.
        /// </summary>
        /// <param name="locator">The locator.</param>
        /// <exception cref="System.ArgumentNullException">logger
        /// or
        /// moduleTracker</exception>
        [ImportingConstructor]
        public DefaultModule(ILoggerFacade logger, IRegionManager regionManager, IPluginModuleTracker pluginTracker, IEventAggregator events)
        {
            Events = events;
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

            Events.GetEvent<PleaseStandByEvent>().Subscribe(payload =>
               {
                   Events.GetEvent<ResumeNormalProcessingEvent>().Publish(Empty.Value);
                   PleaseStandByHandler.Handle(payload);
               });
            Events.GetEvent<ResumeNormalProcessingEvent>().Subscribe(payload => ResumeNormalProcessingHandler.Handle(payload));

        }

        /// <summary>
        /// Gets or sets the resume normal processing handler.
        /// </summary>
        /// <value>
        /// The resume normal processing handler.
        /// </value>
        [Import]
        ResumeNormalProcessingHandler ResumeNormalProcessingHandler { get; set; }
        /// <summary>
        /// Gets or sets the please stand by handler.
        /// </summary>
        /// <value>
        /// The please stand by handler.
        /// </value>
        [Import]
        PleaseStandByHandler PleaseStandByHandler { get; set; }




        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        IEventAggregator Events { get; set; }

        /// <summary>
        /// Notifies the module that it has be initialized.
        /// </summary>
        public void Initialize()
        {
            _logger.Log("Monahrq.Default demonstrates logging during Initialize().", Category.Info, Priority.Medium);

            //_regionManager.RegisterViewWithRegion(Sdk.Regions.RegionNames.Modal, typeof(DataProvider.Administration.DataProviderAdministratorView));

            _pluginTracker.RecordModuleInitialized("Monahrq.Default");





        }
    }
}
