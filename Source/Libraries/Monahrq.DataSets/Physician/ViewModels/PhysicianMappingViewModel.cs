using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Monahrq.Sdk.ViewModels;
using Microsoft.Practices.Prism.Regions;
using PropertyChanged;

namespace Monahrq.DataSets.Physician.ViewModels
{
    /// <summary>
    /// The parent physican mapping view model.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.ViewModels.BaseViewModel" />
    /// <seealso cref="Monahrq.Sdk.ViewModels.ITabItem" />
    [Export]
    [RegionMemberLifetime(KeepAlive = false)]
    [ImplementPropertyChanged]
    public class PhysicianMappingViewModel : BaseViewModel, ITabItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicianMappingViewModel"/> class.
        /// </summary>
        public PhysicianMappingViewModel()
        {
            IsValid = true;
        }
        #region Fields and Constants

        private int _currentTabIndex;
        private bool _isActive;

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the physician ListView model.
        /// </summary>
        /// <value>
        /// The physician ListView model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public PhysicianListViewModel PhysicianListViewModel { get; set; }

        /// <summary>
        /// Gets or sets the medical practices view model.
        /// </summary>
        /// <value>
        /// The medical practices view model.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public MedicalPracticesViewModel MedicalPracticesViewModel { get; set; }

        #endregion

        #region Proprties

        /// <summary>
        /// Notifies that the value for <see cref="P:Microsoft.Practices.Prism.IActiveAware.IsActive" /> property has changed.
        /// </summary>
        public event EventHandler IsActiveChanged;

        /// <summary>
        /// Gets or sets the index of the current tab.
        /// </summary>
        /// <value>
        /// The index of the current tab.
        /// </value>
        public int CurrentTabIndex
        {
            get
            {
                return _currentTabIndex;
            }
            set
            {
                if (_currentTabIndex == value) return;

                _currentTabIndex = value;
                TabItems.ForEach(x =>
                {
                    x.IsActive = x.Index == _currentTabIndex;
                });
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
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            TabItems = new List<ITabItem>()
            {
                PhysicianListViewModel, 
                MedicalPracticesViewModel,
            };

            //EventAggregator.GetEvent<ContextAppliedEvent>().Subscribe(s =>
            //{
            //    if (IsActive) TabItems[CurrentTabIndex].OnIsActive();
            //});

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

            var tabIndex = CurrentTabIndex < 0 ? 0 : CurrentTabIndex;
            if (TabItems[tabIndex].IsActive)
                TabItems[tabIndex].IsActive = false;

            TabItems[tabIndex].IsActive = true;
        }

        /// <summary>
        /// Called when [navigated to].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            TabItems.ForEach(x => x.IsActive = false);
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
            TabItems.ForEach(x => x.IsActive = false);
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
        /// Refreshes this instance.
        /// </summary>
        public void Refresh() { }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset() { }
        /// <summary>
        /// Called when [pre save].
        /// </summary>
        public void OnPreSave()
        {
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
        public virtual bool ShouldValidate
        {
            get { return false; }
        }
        #endregion
    }
}
