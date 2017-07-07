using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;

namespace Monahrq.Sdk.Regions.Core
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Regions.Core.IFluentRegionManager" />
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    [Export(typeof(IFluentRegionManager))]
    public class FluentRegionManager : IFluentRegionManager, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        public IRegionManager RegionManager { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentRegionManager"/> class.
        /// </summary>
        /// <param name="regionManager">The region manager.</param>
        [ImportingConstructor]
        public FluentRegionManager(IRegionManager regionManager)
        {
            RegionManager = regionManager;
        }

        /// <summary>
        /// Gets or sets the fluent views.
        /// </summary>
        /// <value>
        /// The fluent views.
        /// </value>
        [ImportMany(AllowRecomposition = true)]
        public Lazy<UserControl, IExportAsViewMetadata>[] FluentViews { get; set; }
        /// <summary>
        /// The fluent views
        /// </summary>
        private readonly Dictionary<string, string> _fluentViews = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the views.
        /// </summary>
        /// <value>
        /// The views.
        /// </value>
        [ImportMany(AllowRecomposition = true)]
        public Lazy<UserControl, IExportViewToRegionMetadata>[] Views { get; set; }
        /// <summary>
        /// The processed views
        /// </summary>
        private readonly List<string> _processedViews = new List<string>();

        /// <summary>
        /// The regions
        /// </summary>
        private readonly Dictionary<string, UIElement> _regions = new Dictionary<string, UIElement>();

        /// <summary>
        /// Gets the view information.
        /// </summary>
        /// <param name="viewName">Name of the view.</param>
        /// <returns></returns>
        private Lazy<UserControl, IExportViewToRegionMetadata> _getViewInfo(string viewName)
        {
            return (from v in Views where v.Metadata.ViewTypeForRegion.Equals(viewName) select v).FirstOrDefault();
        }

        /// <summary>
        /// Gets the <see cref="IExportViewToRegionMetadata"/> with the specified view type.
        /// </summary>
        /// <value>
        /// The <see cref="IExportViewToRegionMetadata"/>.
        /// </value>
        /// <param name="viewType">Type of the view.</param>
        /// <returns></returns>
        public IExportViewToRegionMetadata this[string viewType]
        {
            get
            {
                return (from v in Views
                        where v.Metadata.ViewTypeForRegion.Equals(viewType, StringComparison.InvariantCultureIgnoreCase)
                        select v.Metadata).FirstOrDefault();
            }
        }

        /// <summary>
        /// Registers the view.
        /// </summary>
        /// <param name="viewName">Name of the view.</param>
        /// <exception cref="ArgumentNullException">viewName</exception>
        public void RegisterView(string viewName)
        {
            
            if (viewName==null)
            {
                throw new ArgumentNullException("viewName");
            }

            var viewInfo = _getViewInfo(viewName);

            string targetRegion;
            UserControl _view;

            if (viewInfo != null)
            {
                targetRegion = viewInfo.Metadata.TargetRegion;
                _view = viewInfo.Value;
            }
            else
            { /*Fluent views are views with no specifically designates regions*/
                _fluentViews.TryGetValue(viewName, out targetRegion);
                _view =
                    (from v in FluentViews where v.Metadata.ExportedViewType.Equals(viewName) select v.Value).
                        FirstOrDefault();
            }

            if (string.IsNullOrEmpty(targetRegion) || _processedViews.Contains(viewName))return;
          
            RegionManager.RegisterViewWithRegion(targetRegion,  _view.GetType());
            _processedViews.Add(viewName);
        }

        /// <summary>
        /// Exports the view to region.
        /// </summary>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="regionTag">The region tag.</param>
        public void ExportViewToRegion(string viewName, string regionTag)
        {
            _fluentViews.Add(viewName, regionTag);

        }
        /// <summary>
        /// Exports the view to region.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="regionTag">The region tag.</param>
        public void ExportViewToRegion<T>(string regionTag) where T : UserControl
        {
            ExportViewToRegion(typeof(T).FullName, regionTag);
        }


        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            foreach (var type in 
                from view in 
                    Views where !_processedViews.Contains(view.Metadata.ViewTypeForRegion) 
                select view.Metadata.ViewTypeForRegion)
            {
                RegisterView(type);
            }
        }
    }
}
