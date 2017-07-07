using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Utility.Extensions;
using Monahrq.Measures.Views;
using Monahrq.Sdk.Common;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.ViewModels;
using PropertyChanged;

namespace Monahrq.Measures.ViewModels
{
    /// <summary>
    /// View model class for Measures main screen
    /// </summary>
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    /// <seealso cref="Microsoft.Practices.Prism.Regions.INavigationAware" />
    [ImplementPropertyChanged]
    [Export(typeof(MainMeasuresViewModel))]
    public class MainMeasuresViewModel : IPartImportsSatisfiedNotification, INavigationAware
    {
        #region Fields and Constants

        /// <summary>
        /// The error open panel height
        /// </summary>
        public const double ErrorOpenPanelHeight = 40.0;
        /// <summary>
        /// The progress open panel height
        /// </summary>
        public const double ProgressOpenPanelHeight = 10.0;
        /// <summary>
        /// The panel closed height
        /// </summary>
        public const double PanelClosedHeight = 0;

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the measures view model.
        /// </summary>
        /// <value>
        /// The measures view model.
        /// </value>
        [Import]
        ManageMeasuresViewModel MeasuresViewModel { get; set; }

        /// <summary>
        /// Gets or sets the topics view model.
        /// </summary>
        /// <value>
        /// The topics view model.
        /// </value>
        [Import]
        ManageTopicsViewModel TopicsViewModel { get; set; }

        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import]
        public IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        [Import]
        public IRegionManager RegionManager { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the close error command.
        /// </summary>
        /// <value>
        /// The close error command.
        /// </value>
        public DelegateCommand CloseErrorCommand { get; set; }
        /// <summary>
        /// Gets or sets the reset command.
        /// </summary>
        /// <value>
        /// The reset command.
        /// </value>
        public DelegateCommand ResetCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MainMeasuresViewModel"/> class.
        /// </summary>
        public MainMeasuresViewModel()
        {

            NotificationPanel = PanelClosedHeight;

            CloseErrorCommand = new DelegateCommand(() =>
                {
                    NotificationPanel = PanelClosedHeight;
                });
        }

        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the tab items.
        /// </summary>
        /// <value>
        /// The tab items.
        /// </value>
        public ObservableCollection<ITabItem> TabItems { get; set; }

        /// <summary>
        /// Gets or sets the notification panel.
        /// </summary>
        /// <value>
        /// The notification panel.
        /// </value>
        public double NotificationPanel { get; set; }

        /// <summary>
        /// Gets or sets the progress panel.
        /// </summary>
        /// <value>
        /// The progress panel.
        /// </value>
        public double ProgressPanel { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the progress.
        /// </summary>
        /// <value>
        /// The progress.
        /// </value>
        public int Progress { get; set; }

        /// <summary>
        /// The active tab item index
        /// </summary>
        private int _activeTabItemIndex;
        /// <summary>
        /// Gets or sets the index of the active tab item.
        /// </summary>
        /// <value>
        /// The index of the active tab item.
        /// </value>
        public int ActiveTabItemIndex
        {
            get { return _activeTabItemIndex; }
            set
            {
                if (_activeTabItemIndex == value) return;
                //TabItems[_activeTabIndex].IsActive = false;
                _activeTabItemIndex = value;
                //TabItems[_activeTabIndex].IsActive = true;
                SetContextualHelp(_activeTabItemIndex);
            }
        }

        /// <summary>
        /// Sets the contextual help based upon the tab index.
        /// </summary>
        /// <param name="tabIndex">Index of the tab.</param>
        private void SetContextualHelp(int tabIndex)
        {
            if (tabIndex == 0)
                EventAggregator.GetEvent<SetContextualHelpContextEvent>().Publish("Managing and Customizing Measures");
            else if (tabIndex == 1)
                EventAggregator.GetEvent<SetContextualHelpContextEvent>().Publish("Add, Edit, or Remove Topics");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            // SubsribeToEvents();
            TabItems = new ObservableCollection<ITabItem>
                {
                    MeasuresViewModel,
                    TopicsViewModel
                };

            //RegionManager.RegisterViewWithRegion("MeasuresRegion", typeof(ManageMeasuresView));
            //RegionManager.RegisterViewWithRegion("TopicsRegion", typeof(ManageTopicsView));
        }

        /// <summary>
        /// Subsribes to events.
        /// </summary>
        private void SubsribeToEvents()
        {
            EventAggregator.GetEvent<ErrorNotificationEvent>().Subscribe(err =>
                {
                    NotificationPanel = ErrorOpenPanelHeight;
                    ErrorMessage = err.Message;
                });

            EventAggregator.GetEvent<ProgressNotificationEvent>().Subscribe(p =>
                {
                    Progress = p;
                    ProgressPanel = Progress == 100 ? PanelClosedHeight : ProgressOpenPanelHeight;
                });
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            WaitCursor.Show();
            //ActiveTabIndex = 0;   
         
            //if (ActiveTabIndex == -1)
            //    EventAggregator.GetEvent<UpdateTabIndexEvent>()
            //        .Publish(new TabIndexSelecteor { TabName = "MeasureTabs", TabIndex = 0 });

            if (ActiveTabItemIndex != 0)
            {
                ActiveTabItemIndex = 0;
                EventAggregator.GetEvent<UpdateTabIndexEvent>().Publish(new TabIndexSelecteor { TabName = "MeasureTabs", TabIndex = 0 });
            }

            if (!TabItems[ActiveTabItemIndex].IsActive)
            {
                TabItems[ActiveTabItemIndex].IsActive = true;
                //TabItems[ActiveTabItemIndex].OnIsActive();
            }

            if (MeasuresViewModel.IsNavigatedFromManageMeasuresDetailsView)
            {
                TabItems[ActiveTabItemIndex].IsActive = false;
                MeasuresViewModel.IsNavigatedFromManageMeasuresDetailsView = false;
            }

            //if (TabItems.Count > 0) TabItems[ActiveTabIndex].IsActive = true;
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
            //TabItems.ForEach(x => x.IsActive = false);
            //if (ActiveTabItemIndex == 1)
            EventAggregator.GetEvent<UpdateTabIndexEvent>().Publish(new TabIndexSelecteor() { TabName = "MeasureTabs", TabIndex = -1 });
        }

        #endregion
    }
}
