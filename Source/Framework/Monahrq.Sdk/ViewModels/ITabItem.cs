using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Regions;

namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    /// The interface for all tab view models. This interface is the contract to interact with Prism framework.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.IActiveAware" />
    public interface ITabItem : IActiveAware
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        bool IsLoaded { get; set; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        int Index { get; set; }

        /// <summary>
        /// Gets or sets the name of the header.
        /// </summary>
        /// <value>
        /// The name of the header.
        /// </value>
        string HeaderName { get; set; }

        /// <summary>
        /// Gets or sets the name of the region.
        /// </summary>
        /// <value>
        /// The name of the region.
        /// </value>
        string RegionName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is initial load.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is initial load; otherwise, <c>false</c>.
        /// </value>
        bool IsInitialLoad { get; set; }

        /// <summary>
        /// Called when [is active].
        /// </summary>
        void OnIsActive();

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        void Refresh();
		
        /// <summary>
        /// Called when [tab changed].
        /// </summary>
        /// <returns></returns>
        bool TabChanged();

        /// <summary>
        /// Gets a value indicating whether [should validate].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [should validate]; otherwise, <c>false</c>.
        /// </value>
        bool ShouldValidate { get; }

        /// <summary>
        /// Validates the on change.
        /// </summary>
        void ValidateOnChange();

        /// <summary>
        /// Resets this instance.
        /// </summary>
        void Reset();

        /// <summary>
        /// Called when [pre save].
        /// </summary>
        void OnPreSave();
    }
}
