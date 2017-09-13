using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Modules;
using Monahrq.Sdk.Regions;
using System;
using System.Linq;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Modules;
using Monahrq.Reports.Views;
using NHibernate.Linq;
using Monahrq.Sdk.Attributes.Wings;
using NHibernate;

namespace Monahrq.Reports
{

    static class Constants
    {
        public const string WingGuid = "{1C4B6348-BCE1-4D67-B943-07E9C7AFF685}";
    }

    /// <summary>
    /// Report module class
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Modularity.IModule" />
    /// <seealso cref="Monahrq.Infrastructure.Modules.IModuleRegionRegistrar" />
    [Export(typeof(IModuleRegionRegistrar))]
    [WingModule(typeof(ReportsModule), Constants.WingGuid, "Reports Wing", "Provides Services Creating Reports",
       DependsOnModuleNames = new string[] { "Base Data" }, InitializationMode = InitializationMode.WhenAvailable)]
    public class ReportsModule : IModule, IModuleRegionRegistrar
    {
        private readonly ILogWriter _logger;
        private readonly IPluginModuleTracker _pluginTracker;
        private IRegionManager _regionManager;
        private readonly IServiceLocator _locator;
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsModule"/> class.
        /// </summary>
        /// <param name="locator">The locator.</param>
        /// <exception cref="NullReferenceException">
        /// logger of type \"ILoggerFacade\" is required and should not be null.
        /// or
        /// regionManager should not be null.
        /// or
        /// pluginTracker should not be null.
        /// or
        /// Event Aggregator should not be null.
        /// </exception>
        [ImportingConstructor]
        public ReportsModule(IServiceLocator locator)
        {
            _locator = locator;
            var logger = locator.GetInstance<ILogWriter>();
            var regionManager = locator.GetInstance<IRegionManager>();
            var pluginTracker = locator.GetInstance<IPluginModuleTracker>();
            var eventAggregator = locator.GetInstance<IEventAggregator>();

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

            if (eventAggregator == null)
            {
                throw new NullReferenceException("Event Aggregator should not be null.");
            }

            _logger = logger;
            _regionManager = regionManager;
            _pluginTracker = pluginTracker;
            _pluginTracker.RecordModuleConstructed(this.GetType().Assembly.GetName().Name);
            _eventAggregator = eventAggregator;

            // Register the views in this module.
            //_regionManager.RegisterViewWithRegion("DataSetListView", typeof(Views.DataSetListView));
        }

        /// <summary>
        /// Notifies the module that it has be initialized.
        /// </summary>
        public void Initialize()
        {
            if (MonahrqContext.ForceDbUpGrade || MonahrqContext.ForceDbRecreate)
            {
                return;
            }
            _pluginTracker.RecordModuleInitialized("Monahrq.Reports");
            LoadDefaultTemplates();
        }

        /// <summary>
        /// Loads the default templates.
        /// </summary>
        private void LoadDefaultTemplates()
        {
            _eventAggregator.GetEvent<MessageUpdateEvent>()
                   .Publish(new MessageUpdateEvent { Message = "Loading reports module" });

            var manifestFactory = new ReportManifestFactory();
            var provider = ServiceLocator.Current.GetInstance<Infrastructure.Entities.Domain.IDomainSessionFactoryProvider>();
            using (var session = provider.SessionFactory.OpenSession())
            {

                foreach (var manifest in manifestFactory.InstalledManifests)
                {
                    using (var trans = session.BeginTransaction())
                    {
                        try
                        {
							var existingReport =
								session.Query<Report>().Where(r =>
										r.IsDefaultReport &&
										r.Name.ToLower() == manifest.Name.ToLower())
										.FirstOrDefault();

							if (existingReport != null &&
								existingReport.LastReportManifestUpdate.HasValue &&
								existingReport.LastReportManifestUpdate.Value.TrimMilliseconds() >= manifest.FileLastModifiedDate.TrimMilliseconds()) continue;

							if (existingReport == null)
							{
								Report report = new Report(manifest) { IsDefaultReport = true };
								session.SaveOrUpdate(report);
							}
							else
							{
								existingReport.AssignFrom(manifest);
								session.SaveOrUpdate(existingReport);
							}

							session.Flush();
                            trans.Commit();
                        }
                        catch (Exception exc)
                        {
                            trans.Rollback();
                            _logger.Write(exc, "An error occurred when trying to save report \"{1}\"", manifest.Name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the report.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="report">The report.</param>
        private void DeleteReport(ISession session, Report report)
		{
			foreach (var websitePage in report.WebsitePages)
			{
				foreach (var websitePageZone in websitePage.WebsitePageZones)
					session.Delete(websitePageZone);
				session.Delete(websitePage);
			}
			session.Delete(report);
		}

        /// <summary>
        /// Registers the regions.
        /// </summary>
        /// <param name="regionManager">The region manager.</param>
        public void RegisterRegions(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _regionManager.RegisterViewWithRegion(RegionNames.MainContent, typeof (MainReportView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainContent, typeof (ReportDetailsView));
        }
    }
}
