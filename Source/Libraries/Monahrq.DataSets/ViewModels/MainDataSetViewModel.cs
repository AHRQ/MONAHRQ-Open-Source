using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Monahrq.DataSets.HospitalRegionMapping.Context;
using Monahrq.DataSets.HospitalRegionMapping.Events;
using Monahrq.DataSets.HospitalRegionMapping.Mapping;
using Monahrq.DataSets.NHC.ViewModels;
using Monahrq.DataSets.Physician.ViewModels;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.ViewModels;
using PropertyChanged;
using Monahrq.Infrastructure.Configuration;

namespace Monahrq.DataSets.ViewModels
{

    /// <summary>
    /// The main dataset viewmodel.
    /// </summary>
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    /// <seealso cref="Microsoft.Practices.Prism.Regions.INavigationAware" />
    [Export(typeof(MainDataSetViewModel))]
    [ImplementPropertyChanged]
    public class MainDataSetViewModel : IPartImportsSatisfiedNotification, INavigationAware
    {
        #region Fields and Constants

        private int _activeTabItemIndex;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the index of the active tab item.
        /// </summary>
        /// <value>
        /// The index of the active tab item.
        /// </value>
        [DoNotCheckEquality]
        public int ActiveTabItemIndex
        {
            get
            {
                return _activeTabItemIndex;
            }
            set
            {
                _activeTabItemIndex = value;
                if (!IsRegionContextVisible || _activeTabItemIndex == 0 || !TabItems[_activeTabItemIndex].IsActive)
                {
                    if(!TabItems[_activeTabItemIndex].IsActive)
                        TabItems[_activeTabItemIndex].IsActive = true;
                }

                SetcontextualHelp(_activeTabItemIndex);
            }
        }

        /// <summary>
        /// Setcontextuals the help.
        /// </summary>
        /// <param name="tabItemIndex">Index of the tab item.</param>
        void SetcontextualHelp(int tabItemIndex)
        {
            string helpConext;

            switch (tabItemIndex)
            {
                case 0:
                    helpConext = "MANAGING YOUR DATASETS LIBRARY";
                    break;
                case 1:
                    helpConext = "Managing and Importing Hospital Level Dat";
                    break;
                case 2:
                    helpConext = "Managing CMS Nursing Home Compare Data";
                    break;
                case 3:
                    helpConext = "Managing CMS Physician Data";
                    break;
                default:
                    helpConext = "MANAGING YOUR DATASETS LIBRARY";
                    break;
            }
            EventAggregator.GetEvent<SetContextualHelpContextEvent>().Publish(helpConext);
        }

        /// <summary>
        /// Gets or sets the total dimensions.
        /// </summary>
        /// <value>
        /// The total dimensions.
        /// </value>
        public int TotalDimensions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the tab items.
        /// </summary>
        /// <value>
        /// The tab items.
        /// </value>
        public List<ITabItem> TabItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is region context visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is region context visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsRegionContextVisible { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the new geo context command.
        /// </summary>
        /// <value>
        /// The new geo context command.
        /// </value>
        public DelegateCommand NewGeoContextCommand { get; private set; }

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Gets or sets the mapping view model.
        /// </summary>
        /// <value>
        /// The mapping view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public MappingViewModel MappingViewModel { get; set; }

        /// <summary>
        /// Gets or sets the dataset view model.
        /// </summary>
        /// <value>
        /// The dataset view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public DataSetListViewModel DatasetViewModel { get; set; }

        /// <summary>
        /// Gets or sets the context view model.
        /// </summary>
        /// <value>
        /// The context view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public ContextViewModel ContextViewModel { get; set; }

        /// <summary>
        /// Gets or sets the nursing homes view model.
        /// </summary>
        /// <value>
        /// The nursing homes view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public NursingHomesViewModel NursingHomesViewModel { get; set; }

        /// <summary>
        /// Gets or sets the physician mapping view model.
        /// </summary>
        /// <value>
        /// The physician mapping view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public PhysicianMappingViewModel PhysicianMappingViewModel { get; set; }

        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public IConfigurationService ConfigurationService { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MainDataSetViewModel"/> class.
        /// </summary>
        public MainDataSetViewModel()
        { }

        #endregion

        #region Methods

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            IsRegionContextVisible = !ConfigurationService.HospitalRegion.IsDefined && ActiveTabItemIndex != 0;

            EventAggregator.GetEvent<ContextAppliedEvent>().Subscribe((message) =>
            {
                if (message.EqualsIgnoreCase("Saved"))
                {
                    IsRegionContextVisible = false;

                    if (ActiveTabItemIndex >= 0)
                        TabItems[ActiveTabItemIndex].IsActive = true;
                }
                IsRegionContextVisible = false;
            });

            NewGeoContextCommand = new DelegateCommand(OnNewGeoContextCommand);
            TabItems = new List<ITabItem>()
            {
                DatasetViewModel,
                MappingViewModel,
                NursingHomesViewModel, 
                PhysicianMappingViewModel
            };
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext != null && navigationContext.Parameters.Any(p => p.Key.EqualsIgnoreCase("TabIndex")))
            {
                var tabIndex = navigationContext.Parameters["TabIndex"];
                ActiveTabItemIndex = !string.IsNullOrEmpty(tabIndex) ? int.Parse(tabIndex) : 0;
            }
            else
            {
                ActiveTabItemIndex = 0;
            }

            ITabItem tab = TabItems[ActiveTabItemIndex];
            if (tab != null)
            {
                if (!tab.IsActive)
                    tab.IsActive = true;
                else
                {
                    tab.OnIsActive();
                }
            }

            if (ActiveTabItemIndex == 0)
                EventAggregator.GetEvent<SetContextualHelpContextEvent>().Publish("MANAGING YOUR DATASETS LIBRARY");
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
            TabItems.ForEach(x => x.IsActive = false);
            ActiveTabItemIndex = 0;
        }

        /// <summary>
        /// Called when [new geo context command].
        /// </summary>
        private void OnNewGeoContextCommand()
        {
            IsRegionContextVisible = true;
            ContextViewModel.Refresh();
        }

        #endregion
    }
}
