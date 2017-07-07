using System;
using System.Windows.Controls;

namespace Monahrq.Sdk.Regions.Core
{
    /// <summary>
    /// The fluent region manager. Use this for runtime (non-attribute based) configuration of regions
    /// </summary>
    public interface IFluentRegionManager
    {
        /// <summary>
        /// Exports the view to region.
        /// </summary>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="regionTag">The region tag.</param>
        void ExportViewToRegion(string viewName, string regionTag);
        /// <summary>
        /// Exports the view to region.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="regionTag">The region tag.</param>
        void ExportViewToRegion<T>(string regionTag) where T : UserControl;
        /// <summary>
        /// Registers the view.
        /// </summary>
        /// <param name="view">The view.</param>
        void RegisterView(string view);
        /// <summary>
        /// Gets or sets the views.
        /// </summary>
        /// <value>
        /// The views.
        /// </value>
        Lazy<UserControl, IExportViewToRegionMetadata>[] Views { get; set; }
    }
}
