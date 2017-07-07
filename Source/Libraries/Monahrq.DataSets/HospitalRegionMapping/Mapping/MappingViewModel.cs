using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.Practices.Prism.Regions;
using Monahrq.DataSets.HospitalRegionMapping.Categories;
using Monahrq.DataSets.HospitalRegionMapping.Events;
using Monahrq.DataSets.HospitalRegionMapping.Hospitals;
using Monahrq.DataSets.HospitalRegionMapping.Regions;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.ViewModels;
using PropertyChanged;

namespace Monahrq.DataSets.HospitalRegionMapping.Mapping
{
    /// <summary>
    /// The mapping view model. This view model handles the tab navigation for the hospital, hospital categories and hospital regions sub tabs.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.ViewModels.BaseViewModel" />
    /// <seealso cref="Monahrq.Sdk.ViewModels.ITabItem" />
    [Export]
    [ImplementPropertyChanged]
    public class MappingViewModel : BaseViewModel, ITabItem 
    {
        #region Fields and Constants

        private int _activeTabItemIndex;
        private bool _isActive;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingViewModel"/> class.
        /// </summary>
        public MappingViewModel()
        {
            IsValid = true;
        }

        #region Commands

        /// <summary>
        /// Gets the new geo context command.
        /// </summary>
        /// <value>
        /// The new geo context command.
        /// </value>
        public ICommand NewGeoContextCommand { get; private set; }

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the hospitals.
        /// </summary>
        /// <value>
        /// The hospitals.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public HospitalsViewModel Hospitals { get; set; }

        /// <summary>
        /// Gets or sets the regions.
        /// </summary>
        /// <value>
        /// The regions.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public RegionsViewModel Regions { get; set; }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public CategoriesViewModel Categories { get; set; }

        #endregion

        #region Proprties

        /// <summary>
        /// Notifies that the value for <see cref="P:Microsoft.Practices.Prism.IActiveAware.IsActive" /> property has changed.
        /// </summary>
        public event EventHandler IsActiveChanged;

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
                //if (_activeTabItemIndex == value) return;

                _activeTabItemIndex = value;

                //if (_currentTabIndex == -1) return;

                TabItems.ForEach(x =>
                {
                    if (_activeTabItemIndex == -1)
                        x.IsActive = false;
                    else
                        x.IsActive = x.Index == _activeTabItemIndex;

                    x.OnIsActive();
                });
                //NavigateToSelectedView();

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
                case 1:
                    helpConext = "Hospital Categories Tab";
                    break;
                case 2:
                    helpConext = "Customizing Regions";
                    break;
                default:
                    helpConext = "Managing and Importing Hospital Level Dat";
                    break;
            }
            EventAggregator.GetEvent<SetContextualHelpContextEvent>().Publish(helpConext);
        }

        /// <summary>
        /// Gets the name of the selected region.
        /// </summary>
        /// <value>
        /// The name of the selected region.
        /// </value>
        /// <exception cref="InvalidOperationException">UNSUPPORTED TAB INDEX</exception>
        public string SelectedRegionName
        {
            get
            {
                switch (ActiveTabItemIndex)
                {
                    case 0: return RegionNames.Hospitals;
                    case 1: return RegionNames.Categories;
                    case 2: return RegionNames.Regions;
                }
                throw new InvalidOperationException("UNSUPPORTED TAB INDEX");
            }
        }

        /// <summary>
        /// Gets the name of the selected view.
        /// </summary>
        /// <value>
        /// The name of the selected view.
        /// </value>
        /// <exception cref="InvalidOperationException">UNSUPPORTED TAB INDEX</exception>
        public string SelectedViewName
        {
            get
            {
                switch (ActiveTabItemIndex)
                {
                    case 0: return ViewNames.HospitalsView;
                    case 1: return ViewNames.CategoriesView;
                    case 2: return ViewNames.RegionsView;
                }
                throw new InvalidOperationException("UNSUPPORTED TAB INDEX");
            }
        }

        /// <summary>
        /// Gets or sets the last URI.
        /// </summary>
        /// <value>
        /// The last URI.
        /// </value>
        public Uri LastUri { get; set; }

        /// <summary>
        /// Gets or sets the last region.
        /// </summary>
        /// <value>
        /// The last region.
        /// </value>
        public string LastRegion { get; set; }

        /// <summary>
        /// Gets or sets the tab items.
        /// </summary>
        /// <value>
        /// The tab items.
        /// </value>
        public List<ITabItem> TabItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the name of the header.
        /// </summary>
        /// <value>
        /// The name of the header.
        /// </value>
        public string HeaderName { get; set; }

        /// <summary>
        /// Gets or sets the name of the region.
        /// </summary>
        /// <value>
        /// The name of the region.
        /// </value>
        public string RegionName { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is initial load.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is initial load; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialLoad { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the object is active.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the object is active; otherwise <see langword="false" />.
        /// </value>
        [DoNotCheckEquality]
        public bool IsActive
        {
            get { return _isActive; ; }
            set
            {
                _isActive = value;
                OnIsActive();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public void Refresh()
        {
            TabItems[ActiveTabItemIndex].Refresh();
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset() { }
        /// <summary>
        /// Called when [pre save].
        /// </summary>
        public void OnPreSave() {}

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            
            EventAggregator.GetEvent<RequestLoadMappingTabEvent>().Subscribe(type =>
            ActiveTabItemIndex = type == typeof(HospitalsView) ? 0 :
                                 type == typeof(CategoriesView) ? 1 :
                                 type == typeof(RegionsView) ? 2 : -1);

            TabItems = new List<ITabItem>
            {
                Hospitals, 
                Categories,
                Regions
            };

            TabItems.ForEach(x =>
            {
                if (x is HospitalsViewModel)
                    ((HospitalsViewModel) x).Parent = this;
                else if (x is CategoriesViewModel)
                    ((CategoriesViewModel) x).Parent = this;
                else if (x is RegionsViewModel)
                    ((RegionsViewModel) x).Parent = this;
            });
            IsActiveChanged -= MappingViewModel_IsActiveChanged;
            IsActiveChanged += MappingViewModel_IsActiveChanged;
        }

        /// <summary>
        /// Handles the IsActiveChanged event of the MappingViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void MappingViewModel_IsActiveChanged(object sender, EventArgs e)
        {
            if (!IsActive) return;

            var tab = TabItems[ActiveTabItemIndex >= 0 && ActiveTabItemIndex < TabItems.Count ? ActiveTabItemIndex : 0];

            tab.IsActive = tab.Index == ActiveTabItemIndex;
        }

        /// <summary>
        /// Called when [navigated to].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            TabItems.ForEach(x =>
            {
                x.IsActive = false;
            });
        }

        /// <summary>
        /// Determines whether [is navigation target] [the specified navigation context].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        ///   <c>true</c> if [is navigation target] [the specified navigation context]; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// Called when [navigated from].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            TabItems.ForEach(x =>
            {
                x.IsActive = false;
            });
        }

        /// <summary>
        /// Modifies the mapping reference.
        /// </summary>
        private void ModifyMappingReference()
        {
            ActiveTabItemIndex = 0;
            TabItems.ForEach(x =>
            {
                x.IsActive = false;
                x.IsLoaded = false;
            });
            RegionManager.RequestNavigate(Sdk.Regions.RegionNames.HospitalsMainRegion, new Uri(ViewNames.ContextView, UriKind.Relative));
        }

        /// <summary>
        /// Navigates to selected view.
        /// </summary>
        public void NavigateToSelectedView()
        {
            RegionManager.RequestNavigate(SelectedRegionName, new Uri(SelectedViewName, UriKind.Relative), navResult =>
            {
                LastRegion = SelectedRegionName;
                LastUri = navResult.Context.Uri;
            });
        }

        /// <summary>
        /// Called when [is active].
        /// </summary>
        public void OnIsActive()
        {
            if (IsActiveChanged != null)
                IsActiveChanged(this, new EventArgs());
        }

        /// <summary>
        /// Called when [tab changed].
        /// </summary>
        /// <returns></returns>
        public bool TabChanged()
        {
            return true;
        }

        /// <summary>
        /// Validates the on change.
        /// </summary>
        public void ValidateOnChange()
        {
            IsValid = true;
        }
        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets a value indicating whether [should validate].
        /// </summary>
        /// <value>
        /// <c>true</c> if [should validate]; otherwise, <c>false</c>.
        /// </value>
        public bool ShouldValidate
        {
            get { return false; } 
        }
        #endregion
    }
}

