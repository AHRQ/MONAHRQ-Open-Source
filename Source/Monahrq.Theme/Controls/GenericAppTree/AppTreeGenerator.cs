using System.Collections.Generic;
using System.Linq;

namespace Monahrq.Theme.Controls.GenericAppTree
{
	/// <summary>
	/// Generates data provides for the AppTreeContainer control.
	/// </summary>
	public class AppTreeGenerator
    {
		/// <summary>
		/// Generates the specified data providers.
		/// </summary>
		/// <param name="dataProviders">The data providers.</param>
		/// <returns></returns>
		public AppTreeContainer Generate(IEnumerable<IAppTreeDataProvider> dataProviders)
        {
            return Generate(dataProviders, false);
        }

		/// <summary>
		/// Generates the specified data providers.
		/// </summary>
		/// <param name="dataProviders">The data providers.</param>
		/// <param name="enablePreview">if set to <c>true</c> [enable preview].</param>
		/// <returns></returns>
		public AppTreeContainer Generate(IEnumerable<IAppTreeDataProvider> dataProviders, bool enablePreview)
        {
            var root = new AppTreeContainer("root", "root");

            foreach (var item in dataProviders.Where(el => el.CanBeDisplayed))
            {
                var path = item.Path.Split('/');
                var lastContainer = root;

                int i;
                for (i = 0; i < path.Length; i++)
                {
                    var container = lastContainer.ContainerChildren.FirstOrDefault(el => el.Header == path[i]);
                    if (container == null)
                    {
// ReSharper disable SpecifyACultureInStringConversionExplicitly
                        container = new AppTreeContainer(i.ToString(), path[i], enablePreview);
// ReSharper restore SpecifyACultureInStringConversionExplicitly
                        lastContainer.AddChild(container);
                    }

                    lastContainer = container;
                }

// ReSharper disable SpecifyACultureInStringConversionExplicitly
                lastContainer.AddChild(new AppTreeItem(i.ToString(), item.Name, item, item.Icon));
// ReSharper restore SpecifyACultureInStringConversionExplicitly
            }

            return root;
        }
    }
}