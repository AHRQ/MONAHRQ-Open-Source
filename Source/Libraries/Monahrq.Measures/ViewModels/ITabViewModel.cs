namespace Monahrq.Measures.ViewModels
{
    /// <summary>
    /// Inerface for Tab model
    /// </summary>
    public interface ITabViewModel
    {
        /// <summary>
        /// Gets or sets the base title.
        /// </summary>
        /// <value>
        /// The base title.
        /// </value>
        string BaseTitle { get; set; }
        /// <summary>
        /// Gets the tab title.
        /// </summary>
        /// <value>
        /// The tab title.
        /// </value>
        string TabTitle { get; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is tab selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tab selected; otherwise, <c>false</c>.
        /// </value>
        bool IsTabSelected { get; set; }
      
    }
}
