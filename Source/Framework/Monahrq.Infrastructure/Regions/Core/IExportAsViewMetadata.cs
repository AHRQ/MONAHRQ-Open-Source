namespace Monahrq.Sdk.Regions.Core
{
     /// <summary>
    ///     Meta data for the export as view
    /// </summary>
    public interface IExportAsViewMetadata
    {
        /// <summary>
        /// The tag for the view. If exported as a type, will be the full name for the type.
        /// </summary>        
        string ExportedViewType { get; }
        bool DeactivateOnUnload { get; }
        string Category { get; }
        string MenuName { get; }
        string MenuItemName { get;}
     }

}
