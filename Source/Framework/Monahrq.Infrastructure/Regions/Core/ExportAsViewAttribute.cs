using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Monahrq.Sdk.Regions.Core
{
    /// Export a view 
    /// </summary>
    /// <remarks>
    /// Use this attribute to export a view. If you export using the type of the view, the full name
    /// for he type will be used as the tag for the view.
    /// </remarks>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class ExportAsViewAttribute : ExportAttribute, IExportAsViewMetadata
    {
        /// <summary>
        ///     Constructor to use type
        /// </summary>
        /// <param name="viewType">Default to type</param>
        public ExportAsViewAttribute(Type viewType)
            : this(viewType.FullName)
        {
        }

        /// <summary>
        ///     Constructor to use tag
        /// </summary>
        /// <param name="viewType">The tag</param>
        public ExportAsViewAttribute(string viewType)
            : base(typeof(UserControl))
        {
            ExportedViewType = viewType;
            Category = string.Empty;
            
        }

        /// <summary>
        /// The view type
        /// </summary>
        public string ExportedViewType { get; private set; }

        /// <summary>
        /// Set to true to automatically call deactivate on the view model when
        /// the view is unloaded
        /// </summary>
        public bool DeactivateOnUnload { get; set; }

       
        /// <summary>
        /// Optional category, for organizing views together
        /// </summary>
        public string Category { get; set; }
        public string MenuName { get; set; }
        public string MenuItemName { get; set; }
    }
}
