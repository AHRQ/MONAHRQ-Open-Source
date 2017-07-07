using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Events;
using Monahrq.Theme.Events;
using Monahrq.Sdk.Regions;
using PropertyChanged;

namespace Monahrq.ViewModels
{
    /// <summary>
    /// View model of Welcome screen
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Regions.INavigationAware" />
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    [ImplementPropertyChanged]
    [Export]
    public class WelcomeViewModel : INavigationAware, IPartImportsSatisfiedNotification
    {

        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        public IEventAggregator EventAggregator { get; set; }
        /// <summary>
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        public IRegionManager RegionManager { get; set; }


        /// <summary>
        /// Gets or sets the navigate to view command.
        /// </summary>
        /// <value>
        /// The navigate to view command.
        /// </value>
        public DelegateCommand<string> NavigateToViewCommand { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeViewModel"/> class.
        /// </summary>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="regionManager">The region manager.</param>
        [ImportingConstructor]
       public WelcomeViewModel(IEventAggregator eventAggregator, IRegionManager regionManager)
       {
           EventAggregator = eventAggregator;
           RegionManager = regionManager;

           NavigateToViewCommand = new DelegateCommand<string>(OnNavigateToCommand);
       }

        /// <summary>
        /// Called when [navigate to command].
        /// </summary>
        /// <param name="view">The view.</param>
        /// <exception cref="System.ArgumentNullException">view</exception>
        private void OnNavigateToCommand(string view)
        {
            if(string.IsNullOrEmpty(view)) throw  new ArgumentNullException("view");

            var viewUri = new Uri(view, UriKind.Relative);
            if (view.EqualsIgnoreCase(ViewNames.WebsiteManageView))
            {
                var q = new UriQuery();
                q.Add("WebsiteId", "-1");

                viewUri = new Uri(ViewNames.WebsiteManageView + q, UriKind.Relative);
            }

            RegionManager.RequestNavigate(RegionNames.MainContent, viewUri);
        }

        #region Navigation

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            EventAggregator.GetEvent<NavigationEventCurrentViewModel>().Publish(typeof (WelcomeViewModel));
            EventAggregator.GetEvent<SetContextualHelpContextEvent>().Publish("MONAHRQ OVERVIE");
        }

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        /// <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //todo
        }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            
        }

        #endregion

    }
}
