using System;
using Microsoft.Practices.Prism.Commands;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;
using System.Collections.Generic;
using Monahrq.Infrastructure.Extensions;
using Microsoft.Practices.Prism.Regions;

namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    /// The abstract/base class for all list based tab view models.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Monahrq.Sdk.ViewModels.ListViewModel{TEntity}" />
    /// <seealso cref="Monahrq.Sdk.ViewModels.ITabItem" />
    [ImplementPropertyChanged]
    public abstract class ListTabViewModel<TEntity> : ListViewModel<TEntity>, ITabItem
        where TEntity : class, IEntity, new()
    {
        #region Fields and Constants

        /// <summary>
        /// The default displayed items
        /// </summary>
        private const int DEFAULT_DISPLAYED_ITEMS = 75;
        /// <summary>
        /// The is active
        /// </summary>
        private bool _isActive;
        /// <summary>
        /// The page size
        /// </summary>
        private int _pageSize;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ListTabViewModel{TEntity}"/> class.
        /// </summary>
        protected ListTabViewModel()
        {
            IsActiveChanged -= ListTabViewModel_IsActiveChanged;
            IsActiveChanged += ListTabViewModel_IsActiveChanged;
            IsValid = true;
            SubListTabViewModels = new List<SubListTabViewModel>();
            ParentViewModel = null;
        }

        /// <summary>
        /// Handles the IsActiveChanged event of the ListTabViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void ListTabViewModel_IsActiveChanged(object sender, EventArgs e)
        {
            if (IsActive)
                OnLoad();
            else
                OnUnLoad();
        }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
        }

        /// <summary>
        /// Called when [un load].
        /// </summary>
        protected virtual void OnUnLoad() { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        public virtual int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>
        /// The size of the page.
        /// </value>
        public int PageSize
        {
            get
            {
                if (_pageSize == 0)
                    _pageSize = DEFAULT_DISPLAYED_ITEMS;

                return _pageSize;
            }
            set { _pageSize = value; }
        }

        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        /// <value>
        /// The current page.
        /// </value>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is last item fetched.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is last item fetched; otherwise, <c>false</c>.
        /// </value>
        public bool IsLastItemFetched { get; set; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid { get; set; }

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
        /// Gets or sets the view control.
        /// </summary>
        /// <value>
        /// The view control.
        /// </value>
        public Type ViewControl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the object is active.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the object is active; otherwise <see langword="false" />.
        /// </value>
        [DoNotCheckEquality]
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                OnIsActive();
            }
        }

        /// <summary>
        /// Notifies that the value for <see cref="P:Microsoft.Practices.Prism.IActiveAware.IsActive" /> property has changed.
        /// </summary>
        public event EventHandler IsActiveChanged;

        /// <summary>
        /// Gets or sets the sub list tab view models.
        /// </summary>
        /// <value>
        /// The sub list tab view models.
        /// </value>
        public IList<SubListTabViewModel> SubListTabViewModels { get; set; }

        /// <summary>
        /// Gets a value indicating whether [should validate].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [should validate]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool ShouldValidate
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the parent view model.
        /// </summary>
        /// <value>
        /// The parent view model.
        /// </value>
        public ListTabViewModel<TEntity> ParentViewModel { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the next command.
        /// </summary>
        /// <value>
        /// The next command.
        /// </value>
        protected DelegateCommand NextCommand { get; set; }

        /// <summary>
        /// Gets or sets the back command.
        /// </summary>
        /// <value>
        /// The back command.
        /// </value>
        protected DelegateCommand BackCommand { get; set; }

        #endregion

        #region Methods

        #region Init Methods.
        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected override void InitCommands()
        {
            base.InitCommands();
            NextCommand = new DelegateCommand(OnNext);
            BackCommand = new DelegateCommand(OnBack);
        }
        #endregion

        #region Navigation Methods.
        /// <summary>
        /// Called when [next].
        /// </summary>
        protected virtual void OnNext()
        {
            SubListTabViewModels.ForEach(svm =>
            {
                if (svm.SyncNavigationActions)
                    svm.ListTabViewModel.OnNext();
            });
        }

        /// <summary>
        /// Called when [back].
        /// </summary>
        protected virtual void OnBack()
        {
            SubListTabViewModels.ForEach(svm =>
            {
                if (svm.SyncNavigationActions)
                    svm.ListTabViewModel.OnBack();
            });
        }

        /// <summary>
        /// Called when the host of the TabItem has been navigate to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public virtual void OnHostNavigatedTo(NavigationContext navigationContext)
        {
            SubListTabViewModels.ForEach(svm =>
            {
                if (svm.SyncNavigationActions)
                    svm.ListTabViewModel.OnHostNavigatedTo(navigationContext);
            });
        }

        /// <summary>
        /// Called when [host navigated from].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public virtual void OnHostNavigatedFrom(NavigationContext navigationContext)
        {
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            //SubListTabViewModels.ForEach(svm =>
            //{
            //	if (svm.SyncNavigationActions)
            //		svm.ListTabViewModel.OnNavigatedTo(navigationContext);
            //});
        }
        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
                    
            //SubListTabViewModels.ForEach(svm =>
            //{
            //	if (svm.SyncNavigationActions)
            //		svm.ListTabViewModel.OnNavigatedFrom(navigationContext);
            //});
        }
        #endregion

        #region State Methods.
        /// <summary>
        /// Called when [is active].
        /// </summary>
        public void OnIsActive()
        {
            if (IsActiveChanged != null)
                IsActiveChanged(this, EventArgs.Empty);

            SubListTabViewModels.ForEach(svm =>
            {
                if (svm.SyncStateActions)
                    svm.ListTabViewModel.OnIsActive();
            });
        }

        /// <summary>
        /// Called when [pre save].
        /// </summary>
        public virtual void OnPreSave()
        { }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public virtual void Refresh()
        {
            SubListTabViewModels.ForEach(svm =>
            {
                if (svm.SyncStateActions)
                    svm.ListTabViewModel.Refresh();
            });
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public virtual void Reset()
        {
            SubListTabViewModels.ForEach(svm =>
            {
                if (svm.SyncStateActions)
                    svm.ListTabViewModel.Reset();
            });
        }

        /// <summary>
        /// Called when [tab changed].
        /// </summary>
        /// <returns></returns>
        public virtual bool TabChanged()
        {
            SubListTabViewModels.ForEach(svm =>
            {
                if (svm.SyncStateActions)
                    svm.ListTabViewModel.TabChanged();
            });
            return true;
        }

        /// <summary>
        /// Validates the on change.
        /// </summary>
        public virtual void ValidateOnChange()
        {
            IsValid = true;

            SubListTabViewModels.ForEach(svm =>
            {
                if (svm.SyncStateActions)
                {
                    svm.ListTabViewModel.ValidateOnChange();
                    IsValid = IsValid && svm.ListTabViewModel.IsValid;
                    // if (!IsValid) break;
                }
            });
        }
        #endregion

        #region SubListTabViewModel Methods.
        /// <summary>
        /// Adds the sub list tab view model.
        /// </summary>
        /// <param name="subModel">The sub model.</param>
        public void AddSubListTabViewModel(ListTabViewModel<TEntity> subModel)
        {
            SubListTabViewModels.Add(new SubListTabViewModel(subModel));
            subModel.ParentViewModel = this;
        }
        /// <summary>
        /// Adds the sub list tab view model.
        /// </summary>
        /// <param name="subModel">The sub model.</param>
        public void AddSubListTabViewModel(SubListTabViewModel subModel)
        {
            SubListTabViewModels.Add(subModel);
            subModel.ListTabViewModel.ParentViewModel = this;
        }
        #endregion

        #endregion

        #region Types.
        /// <summary>
        /// The base class for a child/sub tab view model.
        /// </summary>
        /// <seealso cref="Monahrq.Sdk.ViewModels.ListViewModel{TEntity}" />
        /// <seealso cref="Monahrq.Sdk.ViewModels.ITabItem" />
        public class SubListTabViewModel
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SubListTabViewModel"/> class.
            /// </summary>
            public SubListTabViewModel()
            {
                ListTabViewModel = null;
                SyncNavigationActions = true;
                SyncCompositionActions = true;
                SyncStateActions = true;
            }
            /// <summary>
            /// Initializes a new instance of the <see cref="SubListTabViewModel"/> class.
            /// </summary>
            /// <param name="model">The model.</param>
            public SubListTabViewModel(ListTabViewModel<TEntity> model) : this()
            {
                ListTabViewModel = model;
            }
            /// <summary>
            /// Gets or sets the list tab view model.
            /// </summary>
            /// <value>
            /// The list tab view model.
            /// </value>
            public ListTabViewModel<TEntity> ListTabViewModel { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [synchronize navigation actions].
            /// </summary>
            /// <value>
            /// <c>true</c> if [synchronize navigation actions]; otherwise, <c>false</c>.
            /// </value>
            public bool SyncNavigationActions { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [synchronize composition actions].
            /// </summary>
            /// <value>
            /// <c>true</c> if [synchronize composition actions]; otherwise, <c>false</c>.
            /// </value>
            public bool SyncCompositionActions { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [synchronize state actions].
            /// </summary>
            /// <value>
            /// <c>true</c> if [synchronize state actions]; otherwise, <c>false</c>.
            /// </value>
            public bool SyncStateActions { get; set; }
        }
        #endregion
    }
}
