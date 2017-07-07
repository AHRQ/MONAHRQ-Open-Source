using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Monahrq.Sdk.Regions.Core
{
    /// <summary>
    /// The export view to region metadata interface.
    /// </summary>
    public interface IExportViewToRegionMetadata
    {
        /// <summary>
        /// The tag for the viewName
        /// </summary>
        string ViewTypeForRegion { get; }

        /// <summary>
        /// The tag for the region
        /// </summary>
        string TargetRegion { get; }
    }

    /// <summary>
    ///  Tag to export a viewName to a region
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class ExportViewToRegionAttribute : ExportAttribute
    {
        /// <summary>
        ///     Allow typed viewName export
        /// </summary>
        /// <param name="viewType">The type of the viewName</param>
        /// <param name="targetRegion">The target region</param>
        public ExportViewToRegionAttribute(Type viewType, string targetRegion)
            : this(viewType.FullName, targetRegion)
        {

        }

        /// <summary>
        ///     Allow tagged viewName export
        /// </summary>
        /// <param name="viewType">The type of the viewName</param>
        /// <param name="targetRegion">The target region</param>
        public ExportViewToRegionAttribute(string viewType, string targetRegion)
            : base(typeof(UserControl))
        {
            ViewTypeForRegion = viewType;
            TargetRegion = targetRegion;
        }

        /// <summary>
        /// The tag for the viewName
        /// </summary>
        public string ViewTypeForRegion { get; private set; }

        /// <summary>
        ///  The tag for the region
        /// </summary>
        public string TargetRegion { get; private set; }
    }
}
