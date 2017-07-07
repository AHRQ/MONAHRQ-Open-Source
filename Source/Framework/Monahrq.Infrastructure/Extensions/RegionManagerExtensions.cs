using System;
using System.Linq;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Regions;

namespace Monahrq.Infrastructure.Extensions
{
    public static class RegionManagerExtensions
    {
        /// <summary>
        /// Extension method for the IRegionManaer RequestNavigate functionality.
        /// </summary>
        /// <param name="regionManager">The region manager.</param>
        /// <param name="regionName">Name of the region.</param>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="query">The query.</param>
        /// <exception cref="System.ArgumentNullException">
        /// regionName;Please provide a valid regionName.
        /// or
        /// viewName;Please provide a valid viewName.
        /// </exception>
        public static void NavigateTo(this IRegionManager regionManager, string regionName, string viewName, UriQuery query = null)
        {
            if (string.IsNullOrEmpty(regionName)) throw new ArgumentNullException("regionName", @"Please provide a valid regionName.");
            if (string.IsNullOrEmpty(viewName)) throw new ArgumentNullException("viewName", @"Please provide a valid viewName.");

            regionManager.RequestNavigate(regionName, query != null
                ? new Uri(viewName + query, UriKind.RelativeOrAbsolute)
                : new Uri(viewName, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Extension method for the IRegionManaer RequestNavigate functionality.
        /// </summary>
        /// <param name="regionManager">The region manager.</param>
        /// <param name="regionName">Name of the region.</param>
        /// <param name="viewType">Type of the view.</param>
        /// <param name="query">The query.</param>
        /// <exception cref="System.ArgumentNullException">viewType;Please provide a valid viewType.</exception>
        public static void NavigateTo(this IRegionManager regionManager, string regionName, Type viewType, UriQuery query = null)
        {
            if (viewType == null) throw new ArgumentNullException("viewType", @"Please provide a valid viewType.");

            if (!regionManager.Regions[regionName].Views.Any(reg => reg.GetType().Name.EqualsIgnoreCase(viewType.Name)))
                regionManager.RegisterViewWithRegion(regionName, viewType);

            NavigateTo(regionManager, regionName, viewType.Name, query);
        }
    }
}