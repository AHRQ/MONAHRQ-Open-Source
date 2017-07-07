using System;
using System.Reflection;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.HospitalRegionMapping.Categories;
using Monahrq.DataSets.HospitalRegionMapping.Context;
using Monahrq.DataSets.HospitalRegionMapping.Mapping;
using Monahrq.DataSets.HospitalRegionMapping.Regions;
using Monahrq.DataSets.NHC.Views;
using Monahrq.DataSets.Physician.Views;
using Monahrq.Infrastructure;
using Monahrq.Sdk.Modules;
using Monahrq.Sdk.Extensions;
using Monahrq.DataSets.Views;
using Monahrq.Sdk.Attributes.Wings;
using System.Windows;
using Monahrq.Sdk.Regions;
using DataImportWizard = Monahrq.DataSets.Views.DataImportWizard;
using RegionNames = Monahrq.Sdk.Regions.RegionNames;
using Monahrq.DataSets.HospitalRegionMapping.Hospitals;

namespace Monahrq.DataSets
{
    /// <summary>
    /// The Datasets Module constants class.
    /// </summary>
    static class Constants
    {
        /// <summary>
        /// The wing unique identifier
        /// </summary>
        public const string WingGuid = "{D91DE5C6-D098-4D77-B77E-8B414DFE1B8D}";
    }
    /// <summary>
    /// The monahrq dataset modules. This class load initializes the Monahrq.Datasets modules and registers the view with the PRISM region Manager (<seealso cref="IRegionManager"/>).
    /// </summary>
    [WingModuleAttribute(typeof(DataSetsModule), Constants.WingGuid, "Datasets Management", "Dataset Management Functionality",
     DependsOnModuleNames = new string[] { "Base Data" }, InitializationMode = InitializationMode.WhenAvailable)]
    public class DataSetsModule : IModule
    {
        private readonly ILoggerFacade _logger;
        private readonly IPluginModuleTracker _pluginTracker;
        private IRegionManager RegionManager { get; set; }
        private readonly IEventAggregator _events;
        private bool IsInitialized { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSetsModule"/> class.
        /// </summary>
        /// <param name="locator">The locator.</param>
        /// <param name="events">The events.</param>
        /// <exception cref="NullReferenceException">
        /// logger of type \"ILoggerFacade\" is required and should not be null.
        /// or
        /// regionManager should not be null.
        /// or
        /// pluginTracker should not be null.
        /// </exception>
        [ImportingConstructor]
        public DataSetsModule(IServiceLocator locator, IEventAggregator events)
        {
            _events = events;
            _events.GetEvent<MessageUpdateEvent>()
                .Publish(new MessageUpdateEvent()
                    {
                        Message = this.GetType().GetCustomAttribute<WingModuleAttribute>().Description
                    });
            Application.Current.DoEvents();
            var logger = locator.GetInstance<ILoggerFacade>();
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
            RegionManager = regionManager;
            _pluginTracker = pluginTracker;
            _pluginTracker.RecordModuleConstructed(this.GetType().Assembly.GetName().Name);

            _events.GetEvent<MessageUpdateEvent>()
                .Publish(new MessageUpdateEvent()
                    {
                        Message = string.Empty
                    });
            Application.Current.DoEvents();
            IsInitialized = false;
        }

        /// <summary>
        /// Notifies the module that it has be initialized.
        /// </summary>
        public void Initialize()
        {
            _pluginTracker.RecordModuleInitialized("Monahrq.DataSets");

            if (IsInitialized) return;

            RegionManager.RegisterViewWithRegion(RegionNames.DataSetsRegion, typeof(DataSetListView));
            //RegionManager.RegisterViewWithRegion("DataSetListView", typeof(Views.DataSetListView));
            RegionManager.RegisterViewWithRegion("DataImportWizard", typeof(DataImportWizard));

            RegionManager.RegisterViewWithRegion(RegionNames.HospitalsMainRegion, typeof(MappingView));
            RegionManager.RegisterViewWithRegion(HospitalRegionMapping.Mapping.RegionNames.Regions, typeof(RegionsView));
            RegionManager.RegisterViewWithRegion(HospitalRegionMapping.Mapping.RegionNames.Categories, typeof(CategoriesView));
            RegionManager.RegisterViewWithRegion(HospitalRegionMapping.Mapping.RegionNames.Hospitals, typeof(HospitalsView));
            RegionManager.RegisterViewWithRegion(ViewNames.ContextView, typeof(ContextView));

            RegionManager.RegisterViewWithRegion(RegionNames.NursingHomes, typeof(NursingHomesView));
            RegionManager.RegisterViewWithRegion(RegionNames.PhysicianListView, typeof(PhysicianListView));

            RegionManager.RegisterViewWithRegion(RegionNames.PhysicianMappingView, typeof(PhysicianMappingView));
            RegionManager.RegisterViewWithRegion(RegionNames.MedicalPracticesView, typeof(MedicalPracticesView));
            //RegionManager.RegisterViewWithRegion(RegionNames.MedicalPracticeEditView, typeof(MedicalPracticeEditView));

            IsInitialized = true;
        }
    }
}
