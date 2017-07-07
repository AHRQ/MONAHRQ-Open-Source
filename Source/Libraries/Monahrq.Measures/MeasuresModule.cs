using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Modules;
using Monahrq.Measures.Views;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Modules;
using Monahrq.Sdk.Regions;

namespace Monahrq.Measures
{
    /// <summary>
    /// Constant class which hold the wing id
    /// </summary>
    static class Constants
    {
        /// <summary>
        /// The wing unique identifier
        /// </summary>
        public const string WingGuid = "{55387000-2CC2-4ABF-B0D2-078950F833E1}";
    }

    /// <summary>
    /// Class for measures module
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Modularity.IModule" />
    /// <seealso cref="Monahrq.Infrastructure.Modules.IModuleRegionRegistrar" />
    [Export(typeof(IModuleRegionRegistrar))]
    [WingModule(typeof(MeasuresModule), Constants.WingGuid, "Measures Wing", "Provides Services Managing Measures",
      DependsOnModuleNames = new [] { "Base Data" }, InitializationMode = InitializationMode.WhenAvailable)]
    public class MeasuresModule : IModule, IModuleRegionRegistrar
    {

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        private ILoggerFacade Logger
        {
            get;
            set;
        }

        /// <summary>
        /// The plugin tracker
        /// </summary>
        private readonly IPluginModuleTracker _pluginTracker;
        /// <summary>
        /// The region manager
        /// </summary>
        private IRegionManager _regionManager;
        /// <summary>
        /// The events
        /// </summary>
        private readonly IEventAggregator _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasuresModule"/> class.
        /// </summary>
        /// <param name="locator">The locator.</param>
        /// <param name="events">The events.</param>
        /// <exception cref="NullReferenceException">
        /// regionManager should not be null.
        /// or
        /// pluginTracker should not be null.
        /// </exception>
        [ImportingConstructor]
        public MeasuresModule(IServiceLocator locator, IEventAggregator events)
        {
            _events = events;
            _events.GetEvent<MessageUpdateEvent>()
                .Publish(new MessageUpdateEvent()
                    {
                        Message = this.GetType().GetCustomAttribute<WingModuleAttribute>().Description
                    });

            Application.Current.DoEvents();

            var regionManager = locator.GetInstance<IRegionManager>();
            var pluginTracker = locator.GetInstance<IPluginModuleTracker>();

            if (regionManager == null)
            {
                throw new NullReferenceException("regionManager should not be null.");
            }

            if (pluginTracker == null)
            {
                throw new NullReferenceException("pluginTracker should not be null.");
            }


            _regionManager = regionManager;
            _pluginTracker = pluginTracker;
            _pluginTracker.RecordModuleConstructed(this.GetType().Assembly.GetName().Name);
            _events.GetEvent<MessageUpdateEvent>()
              .Publish(new MessageUpdateEvent()
              {
                  Message = string.Empty
              });
            Application.Current.DoEvents();
        }

        /// <summary>
        /// Notifies the module that it has be initialized.
        /// </summary>
        public void Initialize()
        {
            _pluginTracker.RecordModuleInitialized("Monahrq.Measures");
        }

        /// <summary>
        /// Registers the regions.
        /// </summary>
        /// <param name="regionManager">The region manager.</param>
        public void RegisterRegions(IRegionManager regionManager)
        {
            try
            {
                _regionManager = regionManager;
                _regionManager.RegisterViewWithRegion(RegionNames.MainContent, typeof (MainMeasuresView));

                if (!_regionManager.Regions.ContainsRegionWithName(RegionNames.MainContent)) return;

                var mainContentRegion =
                    regionManager.Regions.ToList().FirstOrDefault(r => r.Name.EqualsIgnoreCase(RegionNames.MainContent));

                if (mainContentRegion == null) return;

                if (mainContentRegion.RegionManager == null)
                    mainContentRegion.RegionManager = regionManager.CreateRegionManager();

                mainContentRegion.RegionManager.RegisterViewWithRegion("MeasuresManageRegion", typeof (ManageMeasuresView));
                mainContentRegion.RegionManager.RegisterViewWithRegion("MeasuresManageRegion", typeof (ManageTopicsView));
            }
            catch (Exception exc)
            {
                Logger.Log(exc.GetBaseException().Message, Category.Exception, Priority.High);
                throw;
            }

        }
    }
}
