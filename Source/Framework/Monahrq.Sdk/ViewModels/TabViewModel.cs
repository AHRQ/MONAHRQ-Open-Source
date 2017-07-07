using System;
using Microsoft.Practices.Prism.Commands;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    /// The abstract/base tab View Model, that works with the IActveAware functionality needed for Prism Compositability, 
    /// as well as, standardize tab view models lifecycle management.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Monahrq.Sdk.ViewModels.DetailsViewModel{TEntity}" />
    /// <seealso cref="Monahrq.Sdk.ViewModels.ITabItem" />
    public abstract class TabViewModel<TEntity> : DetailsViewModel<TEntity>, ITabItem
        where TEntity : class, IEntity, new()
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TabViewModel{TEntity}"/> class.
        /// </summary>
        protected TabViewModel()
        {
            IsValid = true;
        }

        #endregion

        #region Fields and Constants

        /// <summary>
        /// The is active
        /// </summary>
        private bool isActive;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded { get; set; }
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
        /// Gets or sets a value indicating whether this instance is initial load.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is initial load; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialLoad { get; set; }

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
            get { return isActive; }
            set
            {
                isActive = value;
                OnIsActive();
            }
        }

        /// <summary>
        /// Notifies that the value for <see cref="P:Microsoft.Practices.Prism.IActiveAware.IsActive" /> property has changed.
        /// </summary>
        public event EventHandler IsActiveChanged;

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

        /// <summary>
        /// Called when [is active].
        /// </summary>
        public virtual void OnIsActive()
        {
            if (IsActiveChanged != null) IsActiveChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when [next].
        /// </summary>
        protected virtual void OnNext()
        { }

        /// <summary>
        /// Called when [back].
        /// </summary>
        protected virtual void OnBack()
        { }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected new virtual void InitCommands()
        {
            base.InitCommands();
            NextCommand = new DelegateCommand(OnNext);
            BackCommand = new DelegateCommand(OnBack);
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public void Refresh() { }

        /// <summary>
        /// Called when [tab changed].
        /// </summary>
        /// <returns></returns>
        public virtual bool TabChanged()
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
        /// Resets this instance.
        /// </summary>
        public void Reset() { }
        /// <summary>
        /// Called when [pre save].
        /// </summary>
        public void OnPreSave()
        {
        }

        #endregion
    }
}
