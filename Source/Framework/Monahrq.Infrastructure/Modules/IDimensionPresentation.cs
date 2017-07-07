namespace Monahrq.Sdk.Modules
{
    /// <summary>
    /// The dimension presentation interface.
    /// </summary>
    public interface IDimensionPresentation
    {
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        string Text { get; set; }
    }

    /// <summary>
    /// The dimension presentation entity.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Modules.IDimensionPresentation" />
    public class DimensionPresentation : IDimensionPresentation
    {
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text { get; set; }
    }
}
