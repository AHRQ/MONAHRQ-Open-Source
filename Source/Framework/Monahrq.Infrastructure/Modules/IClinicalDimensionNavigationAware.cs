namespace Monahrq.Sdk.Modules
{
    /// <summary>
    /// the clinical dimension navigation aware interface.
    /// </summary>
    public interface IClinicalDimensionNavigationAware 
    {
        /// <summary>
        /// Gets the index of the tab.
        /// </summary>
        /// <value>
        /// The index of the tab.
        /// </value>
        int TabIndex { get; }
        /// <summary>
        /// Navigates the specified is reload.
        /// </summary>
        /// <param name="isReload">if set to <c>true</c> [is reload].</param>
        void Navigate(bool isReload = false);
    }
}
