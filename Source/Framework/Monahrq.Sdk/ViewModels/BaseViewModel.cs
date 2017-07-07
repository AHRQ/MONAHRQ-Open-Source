using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Commands;
using Monahrq.Sdk.Model;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Configuration;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure;

namespace Monahrq.Sdk.ViewModels
{
    public abstract class BaseViewModel : BaseNotify, INavigationAware, IPartImportsSatisfiedNotification
    {

        #region Imports

        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        protected IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        [Import]
        protected IConfigurationService ConfigurationService { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        protected ILoggerFacade Logger { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import]
        protected IRegionManager RegionManager { get; set; }

        ////TODO: Export IDataService and Import here for database connection
        //[Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        //protected IDataServiceBase DataServiceProvider { get; set; }

        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        protected IDomainSessionFactoryProvider DataServiceProvider { get; set; }

        #endregion

        public abstract void OnImportsSatisfied();

        public abstract void OnNavigatedTo(NavigationContext navigationContext);

        public abstract bool IsNavigationTarget(NavigationContext navigationContext);

        public abstract void OnNavigatedFrom(NavigationContext navigationContext);
    }
}
