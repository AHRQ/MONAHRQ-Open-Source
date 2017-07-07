using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Regions;

namespace Monahrq.Theme.Behaviors
{
	/// <summary>
	/// Behavior that monitors a <see cref="IRegion" /> object and
	/// changes the value for the <see cref="IRegionManagerAware.RegionManager" /> property when
	/// an object that implements <see cref="IRegionManagerAware" /> gets added or removed
	/// from the collection.
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Regions.IRegionBehavior" />
	public class RegionManagerAwareBehavior : IRegionBehavior
    {
		/// <summary>
		/// Name that identifies the <see cref="RegionManagerAwareBehavior" /> behavior in a collection of <see cref="IRegionBehavior" />.
		/// </summary>
		public const string BehaviorKey = "RegionManagerAware";

		/// <summary>
		/// The region that this behavior is extending
		/// </summary>
		public IRegion Region { get; set; }

		/// <summary>
		/// Attaches the behavior to the specified region
		/// </summary>
		public void Attach()
        {
            INotifyCollectionChanged collection = this.GetCollection();
            if (collection != null)
            {
                collection.CollectionChanged += OnCollectionChanged;
            }
        }

		/// <summary>
		/// Detaches the behavior from the <see cref="INotifyCollectionChanged" />.
		/// </summary>
		public void Detach()
        {
            INotifyCollectionChanged collection = this.GetCollection();
            if (collection != null)
            {
                collection.CollectionChanged -= OnCollectionChanged;
            }
        }

		/// <summary>
		/// Called when [collection changed].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    IRegionManager correspondingRegionManager = this.Region.RegionManager;

                    // If the view was created with a scoped region manager, the behavior uses that region manager instead.
                    FrameworkElement element = item as FrameworkElement;
                    if (element != null)
                    {
                        IRegionManager attachedRegionManager = element.GetValue(RegionManager.RegionManagerProperty) as IRegionManager;
                        if (attachedRegionManager != null)
                        {
                            correspondingRegionManager = attachedRegionManager;
                        }
                    }

                    InvokeInRegionManagerAwareElement(item, regionManagerAware => regionManagerAware.RegionManager = correspondingRegionManager);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    InvokeInRegionManagerAwareElement(item, regionManagerAware => regionManagerAware.RegionManager = null);
                }
            }

            // May need to handle other action values (reset, replace). Currently the ViewsCollection class does not raise CollectionChanged with these values.
        }

		/// <summary>
		/// Invokes the in region manager aware element.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="invocation">The invocation.</param>
		private static void InvokeInRegionManagerAwareElement(object item, Action<IRegionManagerAware> invocation)
        {
            var regionManagerAware = item as IRegionManagerAware;
            if (regionManagerAware != null)
            {
                invocation(regionManagerAware);
            }

            var frameworkElement = item as FrameworkElement;
            if (frameworkElement != null)
            {
                var regionManagerAwareDataContext = frameworkElement.DataContext as IRegionManagerAware;
                if (regionManagerAwareDataContext != null)
                {
                    // If a view doesn't have a data context (view model) it will inherit the data context from the parent view.
                    // The following check is done to avoid setting the RegionManager property in the view model of the parent view by mistake. 

                    var frameworkElementParent = frameworkElement.Parent as FrameworkElement;
                    if (frameworkElementParent != null)
                    {
                        var regionManagerAwareDataContextParent = frameworkElementParent.DataContext as IRegionManagerAware;
                        if (regionManagerAwareDataContextParent != null)
                        {
                            if (regionManagerAwareDataContext == regionManagerAwareDataContextParent)
                            {
                                // If all of the previous conditions are true, it means that this view doesn't have a view model
                                // and is using the view model of its visual parent.
                                return;
                            }
                        }
                    }

                    // If any of the previous conditions is false, the view has its own view model and it implements IRegionManagerAware
                    invocation(regionManagerAwareDataContext);
                }
            }
        }

		/// <summary>
		/// Gets the collection.
		/// </summary>
		/// <returns></returns>
		private INotifyCollectionChanged GetCollection()
        {
            return this.Region.ActiveViews;
        }
    }
}
