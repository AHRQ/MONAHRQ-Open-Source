using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;

namespace Monahrq.Theme.Adapters
{
	/// <summary>
	/// Adapter for DockPanels.
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Regions.RegionAdapterBase{System.Windows.Controls.DockPanel}" />
	[Export(typeof(DockPanelRegionAdapter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DockPanelRegionAdapter : RegionAdapterBase<DockPanel>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="DockPanelRegionAdapter"/> class.
		/// </summary>
		/// <param name="behaviorFactory">The behavior factory.</param>
		[ImportingConstructor]
        public DockPanelRegionAdapter(IRegionBehaviorFactory behaviorFactory) :
            base(behaviorFactory)
        {
        }

		/// <summary>
		/// Template method to adapt the object to an <see cref="T:Microsoft.Practices.Prism.Regions.IRegion" />.
		/// </summary>
		/// <param name="region">The new region being used.</param>
		/// <param name="regionTarget">The object to adapt.</param>
		protected override void Adapt(IRegion region, DockPanel regionTarget)
        {
            region.Views.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    foreach (FrameworkElement element in e.NewItems)
                        regionTarget.Children.Add(element);
                //Handle remove event as well.. 
            };
        }

		/// <summary>
		/// Template method to create a new instance of <see cref="T:Microsoft.Practices.Prism.Regions.IRegion" />
		/// that will be used to adapt the object.
		/// </summary>
		/// <returns>
		/// A new instance of <see cref="T:Microsoft.Practices.Prism.Regions.IRegion" />.
		/// </returns>
		protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }
    }
}
