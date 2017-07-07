using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.ViewModels;
using Monahrq.Views;
using PropertyChanged;
using Monahrq.Infrastructure;
using Xceed.Wpf.Toolkit.Panels;

namespace Monahrq.ViewModels
{
    /// <summary>
    /// View model class for manage settings screen
    /// </summary>
    /// <seealso cref="Monahrq.ViewModels.BaseViewModel" />
    /// <seealso cref="Microsoft.Practices.Prism.Regions.INavigationAware" />
    [Export(typeof(ManageSettingsViewModel)), ImplementPropertyChanged]
    public class ManageSettingsViewModel : BaseViewModel, INavigationAware
    {
        #region Fields and Constants

        /// <summary>
        /// The active tab item index
        /// </summary>
        private int _activeTabItemIndex;
        /// <summary>
        /// The database manager view model
        /// </summary>
        private DatabaseManagerViewModel _databaseManagerViewModel;
        /// <summary>
        /// The manage connection strings view model
        /// </summary>
        private ManageConnectionStringsViewModel _manageConnectionStringsViewModel;
        /// <summary>
        /// The manage wing flutters view model
        /// </summary>
        private ManageWingFluttersViewModel _manageWingFluttersViewModel;

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the cancel command.
        /// </summary>
        /// <value>
        /// The cancel command.
        /// </value>
        public DelegateCommand CancelCommand { get; set; }

        /// <summary>
        /// Gets or sets the show all websites command.
        /// </summary>
        /// <value>
        /// The show all websites command.
        /// </value>
        public DelegateCommand ShowAllWebsitesCommand { get; set; }

        /// <summary>
        /// Gets or sets the save command.
        /// </summary>
        /// <value>
        /// The save command.
        /// </value>
        public DelegateCommand SaveCommand { get; set; }

        /// <summary>
        /// Gets or sets the tab selection changed command.
        /// </summary>
        /// <value>
        /// The tab selection changed command.
        /// </value>
        public DelegateCommand TabSelectionChangedCommand { get; set; }

        #endregion

        #region Imports

        // This class is not a "tab" so it inherits from BaseViewModel instead of BaseTabViewModel, and must define these separately
        /// <summary>
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public IRegionManager RegionManager { get; set; }

        /// <summary>
        /// Gets or sets the database manager view model.
        /// </summary>
        /// <value>
        /// The database manager view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public DatabaseManagerViewModel DatabaseManagerViewModel
        {
            get { return _databaseManagerViewModel; }
            set
            {
                _databaseManagerViewModel = value;
                if (_databaseManagerViewModel != null)
                    _databaseManagerViewModel.Parent = this;
            }
        }

        /// <summary>
        /// Gets or sets the manage connection strings view model.
        /// </summary>
        /// <value>
        /// The manage connection strings view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public ManageConnectionStringsViewModel ManageConnectionStringsViewModel
        {
            get { return _manageConnectionStringsViewModel; }
            set
            {
                _manageConnectionStringsViewModel = value;
                if (_manageConnectionStringsViewModel != null)
                    _manageConnectionStringsViewModel.Parent = this;
            }
        }

        /// <summary>
        /// Gets or sets the manage wing flutters view model.
        /// </summary>
        /// <value>
        /// The manage wing flutters view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public ManageWingFluttersViewModel ManageWingFluttersViewModel
        {
            get { return _manageWingFluttersViewModel; }
            set
            {
                _manageWingFluttersViewModel = value;
                if (_manageWingFluttersViewModel != null)
                    _manageWingFluttersViewModel.Parent = this;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the index of the active tab item.
        /// </summary>
        /// <value>
        /// The index of the active tab item.
        /// </value>
        //[DoNotCheckEquality]
        public int ActiveTabItemIndex
        {
            get { return _activeTabItemIndex; }
            set
            {
                //TabItems[_activeTabItemIndex].IsActive = false;


                _activeTabItemIndex = value == -1 ? 0 : value;

                foreach (var tabItem in TabItems)
                {
                    var item = tabItem as SettingsViewModel;
                    if(item == null) continue;

                    if(item.Index!= _activeTabItemIndex)
                        item.IsActive = false;
                }
                TabItems[_activeTabItemIndex].IsActive = true;

                //RaisePropertyChanged(() => ActiveTabItemIndex);

                SetContextualHelpContextEvent(_activeTabItemIndex);
            }
        }

        /// <summary>
        /// Gets or sets the tab items.
        /// </summary>
        /// <value>
        /// The tab items.
        /// </value>
        public ObservableCollection<ITabViewModel> TabItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [navigation disabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [navigation disabled]; otherwise, <c>false</c>.
        /// </value>
        public bool NavigationDisabled { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Called when next tab is clicked.
        /// </summary>
        public void OnNextTab()
        {
            //if (ActiveTabItemIndex >= TabItems.Count)
            //    ActiveTabItemIndex = TabItems.Count - 1;

            //TabItems[ActiveTabItemIndex].IsActive = true;

            //ActiveTabItemIndex++;
        }

        /// <summary>
        /// Called when imports are valid.
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            SaveCommand = new DelegateCommand(ExecuteSaveCommand, () => true);

            TabItems = new ObservableCollection<ITabViewModel>
                {
                    DatabaseManagerViewModel,
                    ManageConnectionStringsViewModel,
                    ManageWingFluttersViewModel
                };

            RegionManager.RegisterViewWithRegion("DatabaseManagerRegion", typeof(DatabaseManagerView));
            RegionManager.RegisterViewWithRegion("DbConnectionsManagerRegion", typeof(ManageConnectionStringsView));
            RegionManager.RegisterViewWithRegion("ManageWingsFluttersRegion", typeof(ManageWingsFluttersView));

            Events.GetEvent<DisableNavigationEvent>()
                  .Subscribe(dne => NavigationDisabled = dne.DisableUIElements);
        }

        /// <summary>
        /// Sets the contextual help context event.
        /// </summary>
        /// <param name="activeTabItemIndex">Index of the active tab item.</param>
        private void SetContextualHelpContextEvent(int activeTabItemIndex)
        {
            var helpContext = "MONAHRQ SETTINGS";

            if (activeTabItemIndex == 0)
            {
                helpContext = "Database Manager Settings";
            }
            //else if (activeTabItemIndex == 1)
            //{
            //    helpContext = "Database Manager Settings";
            //}
            else if (activeTabItemIndex == 2)
            {
                helpContext = "Wings and Flutters";
            }

            Events.GetEvent<SetContextualHelpContextEvent>().Publish(helpContext);
        }

        /// <summary>
        /// Executes the save command.
        /// </summary>
        public void ExecuteSaveCommand()
        {
            TabItems[ActiveTabItemIndex].OnSave();
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ActiveTabItemIndex = -1;

            foreach (var tabItem in TabItems)
            {
                var type = tabItem.GetType();
                tabItem.OnReset();
                var originalValues = GetViewModelHashCode(tabItem, type);
                if (tabItem as BaseNotify != null)
                    (tabItem as BaseNotify).OriginalHashValue = originalValues;
            }

            //TabItems[ActiveTabItemIndex].IsActive = true;

            Events.GetEvent<SetContextualHelpContextEvent>().Publish("Database Manager Settings");
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
            //throw new NotImplementedException();
            return true;
        }

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            var postModification = new Dictionary<Type, string>();
            foreach (var tabItem in TabItems)
            {
                var type = tabItem.GetType();
                postModification.Add(type, GetViewModelHashCode(tabItem, type));
            }
        }

        #endregion
    }

    /// <summary>
    /// View model for Settings
    /// </summary>
    /// <seealso cref="Monahrq.ViewModels.BaseNotify" />
    /// <seealso cref="Monahrq.Sdk.ViewModels.ITabViewModel" />
    /// <seealso cref="Microsoft.Practices.Prism.Regions.INavigationAware" />
    [Serializable]
    [DataContract]
    public abstract class SettingsViewModel : BaseNotify, ITabViewModel, INavigationAware
    {
        /// <summary>
        /// The is active
        /// </summary>
        private bool _isActive;

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            IsActiveChanged += OnIsActiveChanged;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the object is active.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the object is active; otherwise <see langword="false" />.
        /// </value>
        //[DoNotCheckEquality]
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;

                if (IsActiveChanged != null)
                {
                    IsActiveChanged(this, new EventArgs());
                }
            }
        }
        /// <summary>
        /// Notifies that the value for <see cref="P:Microsoft.Practices.Prism.IActiveAware.IsActive" /> property has changed.
        /// </summary>
        public event EventHandler IsActiveChanged;

        /// <summary>
        /// Called when [save].
        /// </summary>
        public abstract void OnSave();

        /// <summary>
        /// Called when [cancel].
        /// </summary>
        public abstract void OnCancel();

        /// <summary>
        /// Called when [reset].
        /// </summary>
        public abstract void OnReset();

        /// <summary>
        /// Called when [is active changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        public virtual void OnIsActiveChanged(object sender, EventArgs eventArgs)
        {}

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public abstract void OnNavigatedTo(NavigationContext navigationContext);

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        /// <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public abstract bool IsNavigationTarget(NavigationContext navigationContext);

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public abstract void OnNavigatedFrom(NavigationContext navigationContext);

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        [DataMember]
        public abstract int Index { get; set; }
    }
}
