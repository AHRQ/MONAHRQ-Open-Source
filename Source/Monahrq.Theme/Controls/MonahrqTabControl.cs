using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Sdk.ViewModels;

namespace Monahrq.Theme.Controls
{

	/// <summary>
	/// Tab control.
	/// </summary>
	/// <seealso cref="System.Windows.Controls.TabControl" />
	public class MonahrqTabControl : TabControl
    {
		// override int SelectedIndex { get; set; }

		/// <summary>
		/// Initializes the <see cref="MonahrqTabControl"/> class.
		/// </summary>
		static MonahrqTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MonahrqTabControl),
                new FrameworkPropertyMetadata(typeof(MonahrqTabControl)));
        }

		/// <summary>
		/// Gets the reg manager.
		/// </summary>
		/// <value>
		/// The reg manager.
		/// </value>
		private static IRegionManager RegManager
        {
            get { return ServiceLocator.Current.GetInstance<IRegionManager>(); }
        }

		/// <summary>
		/// The previous tab item
		/// </summary>
		ITabItem previousTabItem;

		/// <summary>
		/// Handles the SelectionChanged event of the MonahrqTabControl control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
		private void MonahrqTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = e.OriginalSource as MonahrqTabControl;
            
            if (tabControl != null)
            {
                var regionName = RegionManager.GetRegionName(tabControl);

                if (tabControl.SelectedIndex == -1) return;

                var removedViewModel = e.RemovedItems.Count > 0 ? ((UserControl)e.RemovedItems[0]).DataContext as ITabItem : null;
                if (removedViewModel != null && removedViewModel.ShouldValidate)
                {
                    removedViewModel.ValidateOnChange();
                    if (!removedViewModel.IsValid)
                    {
                        tabControl.SelectionChanged -= MonahrqTabControl_SelectionChanged;
                        tabControl.SelectedIndex = removedViewModel.Index;
                        tabControl.SelectionChanged += MonahrqTabControl_SelectionChanged;

                        e.Handled = true;
                        return;
                    }
                }

                var index = tabControl.SelectedIndex >= 0 ? tabControl.SelectedIndex : 0;

                foreach (var item in tabControl.Items.OfType<UserControl>().ToList())
                {
                    var viewmodel = item.DataContext as ITabItem;

                    if (viewmodel != null)
                    {
                        if (viewmodel.Index == index)
                        {
                            if (!string.IsNullOrEmpty(regionName))
                            {
                                if (RegManager.Regions.ContainsRegionWithName(regionName))
                                    RegManager.Regions[regionName].Activate(item);
                            }
                            previousTabItem = viewmodel;
                            //viewmodel.OnIsActive();
                        }
                        else
                        {
                            if (index == 0 || index ==-1) break;

                            if(previousTabItem != null && viewmodel.Index == previousTabItem.Index)
                                viewmodel.TabChanged();

                            if (!string.IsNullOrEmpty(regionName))
                            {
                                if (RegManager.Regions.ContainsRegionWithName(regionName))
                                    RegManager.Regions[regionName].Deactivate(item);
                            }
                        }
                    }
                }
            }
        }

		#region TabStripAlignment

		/// <summary>
		/// TabStripAlignment Dependency Property
		/// </summary>
		public static readonly DependencyProperty TabStripAlignmentProperty =
            DependencyProperty.Register("TabStripAlignment", typeof(HorizontalAlignment), typeof(MonahrqTabControl),
                new FrameworkPropertyMetadata(HorizontalAlignment.Right,
                    new PropertyChangedCallback(OnTabStripAlignmentChanged)));

		/// <summary>
		/// Gets or sets the TabStripAlignment property. This dependency property
		/// indicates ....
		/// </summary>
		/// <value>
		/// The tab strip alignment.
		/// </value>
		public HorizontalAlignment TabStripAlignment
        {
            get { return (HorizontalAlignment)GetValue(TabStripAlignmentProperty); }
            set { SetValue(TabStripAlignmentProperty, value); }
        }

		/// <summary>
		/// Handles changes to the TabStripAlignment property.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnTabStripAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MonahrqTabControl target = (MonahrqTabControl)d;
            HorizontalAlignment oldTabStripAlignment = (HorizontalAlignment)e.OldValue;
            HorizontalAlignment newTabStripAlignment = target.TabStripAlignment;
            //target.HorizontalAlignment = newTabStripAlignment;
            target.OnTabStripAlignmentChanged(oldTabStripAlignment, newTabStripAlignment);
        }

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the TabStripAlignment property.
		/// </summary>
		/// <param name="oldTabStripAlignment">The old tab strip alignment.</param>
		/// <param name="newTabStripAlignment">The new tab strip alignment.</param>
		protected virtual void OnTabStripAlignmentChanged(HorizontalAlignment oldTabStripAlignment,
            HorizontalAlignment newTabStripAlignment)
        {
        }

		#endregion

		#region RegionName

		/// <summary>
		/// RegionName Dependency Property
		/// </summary>
		public static readonly DependencyProperty RegionNameProperty =
            DependencyProperty.Register("RegionName", typeof(string)
                , typeof(MonahrqTabControl)
                , new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnRegionNameChanged)));

		/// <summary>
		/// Gets or sets the RegionName property. This dependency property
		/// indicates ....
		/// </summary>
		/// <value>
		/// The name of the region.
		/// </value>
		public string RegionName
        {
            get { return (string)GetValue(RegionNameProperty); }
            set { SetValue(RegionNameProperty, value); }
        }

		/// <summary>
		/// Called when [region name changed].
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MonahrqTabControl target = (MonahrqTabControl)d;
            string oldRegionName = (string)e.OldValue;
            string newRegionName = target.RegionName;
            if (newRegionName != null) RegionManager.SetRegionName(target, newRegionName);
            target.OnRegionNameChanged(oldRegionName, newRegionName);
        }

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the RegionName property.
		/// </summary>
		/// <param name="oldRegionName">Old name of the region.</param>
		/// <param name="newRegionName">New name of the region.</param>
		protected virtual void OnRegionNameChanged(string oldRegionName, string newRegionName)
        {
            RaiseRegionAttachedEvent(newRegionName);
        }

		#endregion

		#region RegionAttached

		/// <summary>
		/// RegionAttached Routed Event
		/// </summary>
		public static readonly RoutedEvent RegionAttachedEvent = EventManager.RegisterRoutedEvent("RegionAttached",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MonahrqTabControl));

		/// <summary>
		/// Occurs when ...
		/// </summary>
		public event RoutedEventHandler RegionAttached
        {
            add { AddHandler(RegionAttachedEvent, value); }
            remove { RemoveHandler(RegionAttachedEvent, value); }
        }

		/// <summary>
		/// A helper method to raise the RegionAttached event.
		/// </summary>
		/// <param name="RegionName">Name of the region.</param>
		/// <returns></returns>
		protected RegionAttachedEventArgs RaiseRegionAttachedEvent(string RegionName)
        {
            return RaiseRegionAttachedEvent(this, RegionName);
        }

		/// <summary>
		/// A static helper method to raise the RegionAttached event on a target element.
		/// </summary>
		/// <param name="target">UIElement or ContentElement on which to raise the event</param>
		/// <param name="RegionName">Name of the region.</param>
		/// <returns></returns>
		internal static RegionAttachedEventArgs RaiseRegionAttachedEvent(DependencyObject target, string RegionName)
        {
            if (target == null) return null;

            RegionAttachedEventArgs args = new RegionAttachedEventArgs();
            args.RegionName = RegionName;
            args.RoutedEvent = RegionAttachedEvent;
            RoutedEventHelper.RaiseEvent(target, args);
            return args;
        }

		#endregion

		/// <summary>
		/// Called when <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" /> is called.
		/// </summary>
		public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //SelectionChanged -= MonahrqTabControl_SelectionChanged;
            //SelectedIndex = 0;
            SelectionChanged += MonahrqTabControl_SelectionChanged;
        }
    }

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="System.Windows.RoutedEventArgs" />
	public class RegionAttachedEventArgs : RoutedEventArgs
    {
		/// <summary>
		/// Gets or sets the name of the region.
		/// </summary>
		/// <value>
		/// The name of the region.
		/// </value>
		public string RegionName { get; set; }

    }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="p_oSender">The p o sender.</param>
	/// <param name="p_eEventArgs">The <see cref="PreviewSelectionChangedEventArgs"/> instance containing the event data.</param>
	public delegate void PreviewSelectionChangedEventHandler(object p_oSender, PreviewSelectionChangedEventArgs p_eEventArgs);

	/// <summary>
	/// 
	/// </summary>
	public class PreviewSelectionChangedEventArgs
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="PreviewSelectionChangedEventArgs"/> class.
		/// </summary>
		/// <param name="addedItems">The added items.</param>
		/// <param name="removedItems">The removed items.</param>
		internal PreviewSelectionChangedEventArgs(IList addedItems, IList removedItems)
        {
            this.AddedItems = addedItems;
            this.RemovedItems = removedItems;
        }
		/// <summary>
		/// The cancel
		/// </summary>
		public bool Cancel;
		/// <summary>
		/// Gets the added items.
		/// </summary>
		/// <value>
		/// The added items.
		/// </value>
		public IList AddedItems { get; private set; }
		/// <summary>
		/// Gets the removed items.
		/// </summary>
		/// <value>
		/// The removed items.
		/// </value>
		public IList RemovedItems { get; private set; }
    }
}
