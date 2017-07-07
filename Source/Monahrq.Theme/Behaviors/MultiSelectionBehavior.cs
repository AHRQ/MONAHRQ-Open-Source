using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Monahrq.Theme.Behaviors
{
	/// <summary>
	/// Behavior to add MultiSection to Listboxes.
	/// </summary>
	/// <seealso cref="System.Windows.Interactivity.Behavior{System.Windows.Controls.ListBox}" />
	public class MultiSelectionBehavior : Behavior<ListBox>
    {
		/// <summary>
		/// Called after the behavior is attached to an AssociatedObject.
		/// </summary>
		/// <remarks>
		/// Override this to hook up functionality to the AssociatedObject.
		/// </remarks>
		protected override void OnAttached()
        {
            base.OnAttached();
            if (SelectedItems != null)
            {
                AssociatedObject.SelectedItems.Clear();
                foreach (var item in SelectedItems)
                {
                    AssociatedObject.SelectedItems.Add(item);
                }
            }
        }

		/// <summary>
		/// Gets or sets the selected items.
		/// </summary>
		/// <value>
		/// The selected items.
		/// </value>
		public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

		/// <summary>
		/// The selected items property
		/// </summary>
		public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList), typeof(MultiSelectionBehavior), new UIPropertyMetadata(null, SelectedItemsChanged));

		/// <summary>
		/// Selecteds the items changed.
		/// </summary>
		/// <param name="o">The o.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void SelectedItemsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var behavior = o as MultiSelectionBehavior;
            if (behavior == null)
                return;

            var oldValue = e.OldValue as INotifyCollectionChanged;
            var newValue = e.NewValue as INotifyCollectionChanged;

            if (oldValue != null)
            {
                oldValue.CollectionChanged -= behavior.SourceCollectionChanged;
                behavior.AssociatedObject.SelectionChanged -= behavior.ListBoxSelectionChanged;
            }
            if (newValue != null)
            {
                behavior.AssociatedObject.SelectedItems.Clear();
                foreach (var item in (IEnumerable)newValue)
                {
                    behavior.AssociatedObject.SelectedItems.Add(item);
                }

                behavior.AssociatedObject.SelectionChanged += behavior.ListBoxSelectionChanged;
                newValue.CollectionChanged += behavior.SourceCollectionChanged;
            }
        }

		/// <summary>
		/// The is updating target
		/// </summary>
		private bool _isUpdatingTarget;
		/// <summary>
		/// The is updating source
		/// </summary>
		private bool _isUpdatingSource;

		/// <summary>
		/// Sources the collection changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isUpdatingSource)
                return;

            try
            {
                _isUpdatingTarget = true;

                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        AssociatedObject.SelectedItems.Remove(item);
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        AssociatedObject.SelectedItems.Add(item);
                    }
                }
            }
            finally
            {
                _isUpdatingTarget = false;
            }
        }

		/// <summary>
		/// ListBoxes the selection changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
		private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingTarget)
                return;

            var selectedItems = SelectedItems;
            if (selectedItems == null)
                return;

            try
            {
                _isUpdatingSource = true;

                foreach (var item in e.RemovedItems)
                {
                    selectedItems.Remove(item);
                }

                foreach (var item in e.AddedItems)
                {
                    selectedItems.Add(item);
                }
            }
            finally
            {
                _isUpdatingSource = false;
            }
        }

    }

	/// <summary>
	/// A sync behaviour for a multiselector.
	/// </summary>
	public static class MultiSelectorBehaviours
    {
		/// <summary>
		/// The synchronized selected items
		/// </summary>
		public static readonly DependencyProperty SynchronizedSelectedItems = DependencyProperty.RegisterAttached(
            "SynchronizedSelectedItems", typeof(IList), typeof(MultiSelectorBehaviours), new PropertyMetadata(null, OnSynchronizedSelectedItemsChanged));

		/// <summary>
		/// The synchronization manager property
		/// </summary>
		private static readonly DependencyProperty _synchronizationManagerProperty = DependencyProperty.RegisterAttached(
            "_synchronizationManager", typeof(SynchronizationManager), typeof(MultiSelectorBehaviours), new PropertyMetadata(null));

		/// <summary>
		/// Gets the synchronized selected items.
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <returns>
		/// The list that is acting as the sync list.
		/// </returns>
		public static IList GetSynchronizedSelectedItems(DependencyObject dependencyObject)
        {
            return (IList)dependencyObject.GetValue(SynchronizedSelectedItems);
        }

		/// <summary>
		/// Sets the synchronized selected items.
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <param name="value">The value to be set as synchronized items.</param>
		public static void SetSynchronizedSelectedItems(DependencyObject dependencyObject, IList value)
        {
            dependencyObject.SetValue(SynchronizedSelectedItems, value);
        }

		/// <summary>
		/// Gets the synchronization manager.
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <returns></returns>
		private static SynchronizationManager Get_synchronizationManager(DependencyObject dependencyObject)
        {
            return (SynchronizationManager)dependencyObject.GetValue(_synchronizationManagerProperty);
        }

		/// <summary>
		/// Sets the synchronization manager.
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <param name="value">The value.</param>
		private static void Set_synchronizationManager(DependencyObject dependencyObject, SynchronizationManager value)
        {
            dependencyObject.SetValue(_synchronizationManagerProperty, value);
        }

		/// <summary>
		/// Called when [synchronized selected items changed].
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnSynchronizedSelectedItemsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                SynchronizationManager synchronizer = Get_synchronizationManager(dependencyObject);
                synchronizer.StopSynchronizing();

                Set_synchronizationManager(dependencyObject, null);
            }

            IList list = e.NewValue as IList;
            Selector selector = dependencyObject as Selector;

            // check that this property is an IList, and that it is being set on a ListBox
            if (list != null && selector != null)
            {
                SynchronizationManager synchronizer = Get_synchronizationManager(dependencyObject);
                if (synchronizer == null)
                {
                    synchronizer = new SynchronizationManager(selector);
                    Set_synchronizationManager(dependencyObject, synchronizer);
                }

                synchronizer.StartSynchronizingList();
            }
        }

		/// <summary>
		/// A synchronization manager.
		/// </summary>
		private class SynchronizationManager
        {
			/// <summary>
			/// The multi selector
			/// </summary>
			private readonly Selector _multiSelector;
			/// <summary>
			/// The synchronizer
			/// </summary>
			private TwoListSynchronizer _synchronizer;

			/// <summary>
			/// Initializes a new instance of the <see cref="SynchronizationManager" /> class.
			/// </summary>
			/// <param name="selector">The selector.</param>
			internal SynchronizationManager(Selector selector)
            {
                _multiSelector = selector;
            }

			/// <summary>
			/// Starts synchronizing the list.
			/// </summary>
			public void StartSynchronizingList()
            {
                IList list = GetSynchronizedSelectedItems(_multiSelector);

                if (list != null)
                {
                    _synchronizer = new TwoListSynchronizer(GetSelectedItemsCollection(_multiSelector), list);
                    _synchronizer.StartSynchronizing();
                }
            }

			/// <summary>
			/// Stops synchronizing the list.
			/// </summary>
			public void StopSynchronizing()
            {
                _synchronizer.StopSynchronizing();
            }

			/// <summary>
			/// Gets the selected items collection.
			/// </summary>
			/// <param name="selector">The selector.</param>
			/// <returns></returns>
			/// <exception cref="InvalidOperationException">Target object has no SelectedItems property to bind.</exception>
			public static IList GetSelectedItemsCollection(Selector selector)
            {
                if (selector is MultiSelector)
                {
                    return (selector as MultiSelector).SelectedItems;
                }
                else if (selector is ListBox)
                {
                    return (selector as ListBox).SelectedItems;
                }
                else
                {
                    throw new InvalidOperationException("Target object has no SelectedItems property to bind.");
                }
            }

        }
    }

	/// <summary>
	/// Keeps two lists synchronized.
	/// </summary>
	/// <seealso cref="System.Windows.IWeakEventListener" />
	public class TwoListSynchronizer : IWeakEventListener
    {
		/// <summary>
		/// The default converter
		/// </summary>
		private static readonly IListItemConverter _defaultConverter = new DoNothingListItemConverter();
		/// <summary>
		/// The master list
		/// </summary>
		private readonly IList _masterList;
		/// <summary>
		/// The master target converter
		/// </summary>
		private readonly IListItemConverter _masterTargetConverter;
		/// <summary>
		/// The target list
		/// </summary>
		private readonly IList _targetList;


		/// <summary>
		/// Initializes a new instance of the <see cref="TwoListSynchronizer" /> class.
		/// </summary>
		/// <param name="masterList">The master list.</param>
		/// <param name="targetList">The target list.</param>
		/// <param name="masterTargetConverter">The master-target converter.</param>
		public TwoListSynchronizer(IList masterList, IList targetList, IListItemConverter masterTargetConverter)
        {
            _masterList = masterList;
            _targetList = targetList;
            _masterTargetConverter = masterTargetConverter;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="TwoListSynchronizer" /> class.
		/// </summary>
		/// <param name="masterList">The master list.</param>
		/// <param name="targetList">The target list.</param>
		public TwoListSynchronizer(IList masterList, IList targetList)
            : this(masterList, targetList, _defaultConverter)
        {
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="list">The list.</param>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		/// <param name="converter">The converter.</param>
		private delegate void ChangeListAction(IList list, NotifyCollectionChangedEventArgs e, Converter<object, object> converter);

		/// <summary>
		/// Starts synchronizing the lists.
		/// </summary>
		public void StartSynchronizing()
        {
            ListenForChangeEvents(_masterList);
            ListenForChangeEvents(_targetList);

            // Update the Target list from the Master list
            SetListValuesFromSource(_masterList, _targetList, ConvertFromMasterToTarget);

            // In some cases the target list might have its own view on which items should included:
            // so update the master list from the target list
            // (This is the case with a ListBox SelectedItems collection: only items from the ItemsSource can be included in SelectedItems)
            if (!TargetAndMasterCollectionsAreEqual())
            {
                SetListValuesFromSource(_targetList, _masterList, ConvertFromTargetToMaster);
            }
        }

		/// <summary>
		/// Stop synchronizing the lists.
		/// </summary>
		public void StopSynchronizing()
        {
            StopListeningForChangeEvents(_masterList);
            StopListeningForChangeEvents(_targetList);
        }

		/// <summary>
		/// Receives events from the centralized event manager.
		/// </summary>
		/// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> calling this method.</param>
		/// <param name="sender">Object that originated the event.</param>
		/// <param name="e">Event data.</param>
		/// <returns>
		/// true if the listener handled the event. It is considered an error by the <see cref="T:System.Windows.WeakEventManager" /> handling in WPF�to register a listener for an event that the listener does not handle. Regardless, the method should return false if it receives an event that it does not recognize or handle.
		/// </returns>
		public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            HandleCollectionChanged(sender as IList, e as NotifyCollectionChangedEventArgs);

            return true;
        }

		/// <summary>
		/// Listens for change events on a list.
		/// </summary>
		/// <param name="list">The list to listen to.</param>
		protected void ListenForChangeEvents(IList list)
        {
            if (list is INotifyCollectionChanged)
            {
                CollectionChangedEventManager.AddListener(list as INotifyCollectionChanged, this);
            }
        }

		/// <summary>
		/// Stops listening for change events.
		/// </summary>
		/// <param name="list">The list to stop listening to.</param>
		protected void StopListeningForChangeEvents(IList list)
        {
            if (list is INotifyCollectionChanged)
            {
                CollectionChangedEventManager.RemoveListener(list as INotifyCollectionChanged, this);
            }
        }

		/// <summary>
		/// Adds the items.
		/// </summary>
		/// <param name="list">The list.</param>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		/// <param name="converter">The converter.</param>
		private void AddItems(IList list, NotifyCollectionChangedEventArgs e, Converter<object, object> converter)
        {
            int itemCount = e.NewItems.Count;

            for (int i = 0; i < itemCount; i++)
            {
                int insertionPoint = e.NewStartingIndex + i;

                if (insertionPoint > list.Count)
                {
                    list.Add(converter(e.NewItems[i]));
                }
                else
                {
                    list.Insert(insertionPoint, converter(e.NewItems[i]));
                }
            }
        }

		/// <summary>
		/// Converts from master to target.
		/// </summary>
		/// <param name="masterListItem">The master list item.</param>
		/// <returns></returns>
		private object ConvertFromMasterToTarget(object masterListItem)
        {
            return _masterTargetConverter == null ? masterListItem : _masterTargetConverter.Convert(masterListItem);
        }

		/// <summary>
		/// Converts from target to master.
		/// </summary>
		/// <param name="targetListItem">The target list item.</param>
		/// <returns></returns>
		private object ConvertFromTargetToMaster(object targetListItem)
        {
            return _masterTargetConverter == null ? targetListItem : _masterTargetConverter.ConvertBack(targetListItem);
        }

		/// <summary>
		/// Handles the collection changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IList sourceList = sender as IList;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    PerformActionOnAllLists(AddItems, sourceList, e);
                    break;
                case NotifyCollectionChangedAction.Move:
                    PerformActionOnAllLists(MoveItems, sourceList, e);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    PerformActionOnAllLists(RemoveItems, sourceList, e);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    PerformActionOnAllLists(ReplaceItems, sourceList, e);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    UpdateListsFromSource(sender as IList);
                    break;
                //default:
                //    break;
            }
        }

		/// <summary>
		/// Moves the items.
		/// </summary>
		/// <param name="list">The list.</param>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		/// <param name="converter">The converter.</param>
		private void MoveItems(IList list, NotifyCollectionChangedEventArgs e, Converter<object, object> converter)
        {
            RemoveItems(list, e, converter);
            AddItems(list, e, converter);
        }

		/// <summary>
		/// Performs the action on all lists.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="sourceList">The source list.</param>
		/// <param name="collectionChangedArgs">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		private void PerformActionOnAllLists(ChangeListAction action, IList sourceList, NotifyCollectionChangedEventArgs collectionChangedArgs)
        {
            if (sourceList == _masterList)
            {
                PerformActionOnList(_targetList, action, collectionChangedArgs, ConvertFromMasterToTarget);
            }
            else
            {
                PerformActionOnList(_masterList, action, collectionChangedArgs, ConvertFromTargetToMaster);
            }
        }

		/// <summary>
		/// Performs the action on list.
		/// </summary>
		/// <param name="list">The list.</param>
		/// <param name="action">The action.</param>
		/// <param name="collectionChangedArgs">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		/// <param name="converter">The converter.</param>
		private void PerformActionOnList(IList list, ChangeListAction action, NotifyCollectionChangedEventArgs collectionChangedArgs, Converter<object, object> converter)
        {
            StopListeningForChangeEvents(list);
            action(list, collectionChangedArgs, converter);
            ListenForChangeEvents(list);
        }

		/// <summary>
		/// Removes the items.
		/// </summary>
		/// <param name="list">The list.</param>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		/// <param name="converter">The converter.</param>
		private void RemoveItems(IList list, NotifyCollectionChangedEventArgs e, Converter<object, object> converter)
        {
            int itemCount = e.OldItems.Count;

            // for the number of items being removed, remove the item from the Old Starting Index
            // (this will cause following items to be shifted down to fill the hole).
            for (int i = 0; i < itemCount; i++)
            {
                list.RemoveAt(e.OldStartingIndex);
            }
        }

		/// <summary>
		/// Replaces the items.
		/// </summary>
		/// <param name="list">The list.</param>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		/// <param name="converter">The converter.</param>
		private void ReplaceItems(IList list, NotifyCollectionChangedEventArgs e, Converter<object, object> converter)
        {
            RemoveItems(list, e, converter);
            AddItems(list, e, converter);
        }

		/// <summary>
		/// Sets the list values from source.
		/// </summary>
		/// <param name="sourceList">The source list.</param>
		/// <param name="targetList">The target list.</param>
		/// <param name="converter">The converter.</param>
		private void SetListValuesFromSource(IList sourceList, IList targetList, Converter<object, object> converter)
        {
            StopListeningForChangeEvents(targetList);

            targetList.Clear();

            foreach (object o in sourceList)
            {
                targetList.Add(converter(o));
            }

            ListenForChangeEvents(targetList);
        }

		/// <summary>
		/// Targets the and master collections are equal.
		/// </summary>
		/// <returns></returns>
		private bool TargetAndMasterCollectionsAreEqual()
        {
            return _masterList.Cast<object>().SequenceEqual(_targetList.Cast<object>().Select(ConvertFromTargetToMaster));
        }

		/// <summary>
		/// Makes sure that all synchronized lists have the same values as the source list.
		/// </summary>
		/// <param name="sourceList">The source list.</param>
		private void UpdateListsFromSource(IList sourceList)
        {
            if (sourceList == _masterList)
            {
                SetListValuesFromSource(_masterList, _targetList, ConvertFromMasterToTarget);
            }
            else
            {
                SetListValuesFromSource(_targetList, _masterList, ConvertFromTargetToMaster);
            }
        }




		/// <summary>
		/// An implementation that does nothing in the conversions.
		/// </summary>
		/// <seealso cref="Monahrq.Theme.Behaviors.IListItemConverter" />
		internal class DoNothingListItemConverter : IListItemConverter
        {
			/// <summary>
			/// Converts the specified master list item.
			/// </summary>
			/// <param name="masterListItem">The master list item.</param>
			/// <returns>
			/// The result of the conversion.
			/// </returns>
			public object Convert(object masterListItem)
            {
                return masterListItem;
            }

			/// <summary>
			/// Converts the specified target list item.
			/// </summary>
			/// <param name="targetListItem">The target list item.</param>
			/// <returns>
			/// The result of the conversion.
			/// </returns>
			public object ConvertBack(object targetListItem)
            {
                return targetListItem;
            }
        }
    }

	/// <summary>
	/// Converts items in the Master list to Items in the target list, and back again.
	/// </summary>
	public interface IListItemConverter
    {
		/// <summary>
		/// Converts the specified master list item.
		/// </summary>
		/// <param name="masterListItem">The master list item.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		object Convert(object masterListItem);

		/// <summary>
		/// Converts the specified target list item.
		/// </summary>
		/// <param name="targetListItem">The target list item.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		object ConvertBack(object targetListItem);
    }

}
