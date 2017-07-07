using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Sdk.ViewModels;
using Monahrq.Theme.Controls;

namespace Monahrq.Theme.Adapters
{
	/// <summary>
	/// Adapter for TabControl regions.
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Regions.RegionAdapterBase{System.Windows.Controls.TabControl}" />
	[Export(typeof(TabControlRegionAdapter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TabControlRegionAdapter : RegionAdapterBase<TabControl>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="TabControlRegionAdapter"/> class.
		/// </summary>
		/// <param name="behaviorFactory">The behavior factory.</param>
		[ImportingConstructor]
        public TabControlRegionAdapter(IRegionBehaviorFactory behaviorFactory) :
            base(behaviorFactory)
        { }

		/// <summary>
		/// Template method to adapt the object to an <see cref="T:Microsoft.Practices.Prism.Regions.IRegion" />.
		/// </summary>
		/// <param name="region">The new region being used.</param>
		/// <param name="regionTarget">The object to adapt.</param>
		protected override void Adapt(IRegion region, TabControl regionTarget)
        {
            region.Views.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    if (e.NewItems != null)
                    {
                        foreach (FrameworkElement element in e.NewItems)
                            regionTarget.Items.Add(element);
                    }
                }

                // Handle remove event as well.. 
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    if (e.OldItems != null)
                    {
                        foreach (FrameworkElement element in e.OldItems)
                            regionTarget.Items.Remove(element);
                    }
                }

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
            return new SingleActiveRegion();
        }
    }

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Regions.RegionAdapterBase{Monahrq.Theme.Controls.MonahrqTabControl}" />
	[Export(typeof(MonahrqTabControlRegionAdapter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MonahrqTabControlRegionAdapter : RegionAdapterBase<MonahrqTabControl> {
		/// <summary>
		/// Initializes a new instance of the <see cref="MonahrqTabControlRegionAdapter"/> class.
		/// </summary>
		/// <param name="behaviorFactory">The behavior factory.</param>
		[ImportingConstructor]
        public MonahrqTabControlRegionAdapter(IRegionBehaviorFactory behaviorFactory) :
            base(behaviorFactory) { }

		/// <summary>
		/// Template method to adapt the object to an <see cref="T:Microsoft.Practices.Prism.Regions.IRegion" />.
		/// </summary>
		/// <param name="region">The new region being used.</param>
		/// <param name="regionTarget">The object to adapt.</param>
		protected override void Adapt(IRegion region, MonahrqTabControl regionTarget) {

            var count = 0;
            region.Views.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add) {
                    if (e.NewItems != null) {
                        foreach (FrameworkElement element in e.NewItems)
                        {
                            if (!regionTarget.Items.Contains(element))
                                regionTarget.Items.Add(element);

                            var userControl = element as UserControl;
                            if (userControl != null)
                            {
                                var context = userControl.DataContext as ITabItem;

                                if (context != null)
                                {
                                    context.IsInitialLoad = true;
                                    //if (count == 0)
                                    {
                                        //region.Activate(element);
                                    }
                                    context.IsInitialLoad = false;
                                }
                                //else
                                    //region.Activate(element);
                            }
                           // else
                                //region.Activate(element);
    
                            count++;
                        }
                    }
                }

                // Handle remove event as well.. 
                if (e.Action == NotifyCollectionChangedAction.Remove) {
                    if (e.OldItems != null) {
                        foreach (FrameworkElement element in e.OldItems)
                            regionTarget.Items.Remove(element);
                    }
                }

            };
        }

		/// <summary>
		/// Template method to attach new behaviors.
		/// </summary>
		/// <param name="region">The region being used.</param>
		/// <param name="regionTarget">The object to adapt.</param>
		protected override void AttachBehaviors(IRegion region, MonahrqTabControl regionTarget)
        {
            base.AttachBehaviors(region, regionTarget);
        }

		/// <summary>
		/// Template method to create a new instance of <see cref="T:Microsoft.Practices.Prism.Regions.IRegion" />
		/// that will be used to adapt the object.
		/// </summary>
		/// <returns>
		/// A new instance of <see cref="T:Microsoft.Practices.Prism.Regions.IRegion" />.
		/// </returns>
		protected override IRegion CreateRegion() {
            return new SingleActiveRegion();
        }
    }
}
