using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Monahrq.Theme.Behaviors
{
	/// <summary>
	/// Behavior to add ListBox functionality.
	/// </summary>
	public class ListBoxBehavior
    {
		/// <summary>
		/// The associations
		/// </summary>
		static Dictionary<ListBox, Capture> Associations =
            new Dictionary<ListBox, Capture>();

		/// <summary>
		/// Gets the scroll on new item.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static bool GetScrollOnNewItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollOnNewItemProperty);
        }

		/// <summary>
		/// Sets the scroll on new item.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="value">if set to <c>true</c> [value].</param>
		public static void SetScrollOnNewItem(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollOnNewItemProperty, value);
        }

		/// <summary>
		/// The scroll on new item property
		/// </summary>
		public static readonly DependencyProperty ScrollOnNewItemProperty =
            DependencyProperty.RegisterAttached(
                "ScrollOnNewItem",
                typeof(bool),
                typeof(ListBoxBehavior),
                new UIPropertyMetadata(false, OnScrollOnNewItemChanged));

		/// <summary>
		/// Called when [scroll on new item changed].
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		public static void OnScrollOnNewItemChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var listBox = d as ListBox;
            if (listBox == null) return;
            bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
            if (newValue == oldValue) return;
            if (newValue)
            {
                listBox.Loaded += new RoutedEventHandler(ListBox_Loaded);
                listBox.Unloaded += new RoutedEventHandler(ListBox_Unloaded);
            }
            else
            {
                listBox.Loaded -= ListBox_Loaded;
                listBox.Unloaded -= ListBox_Unloaded;
                if (Associations.ContainsKey(listBox))
                    Associations[listBox].Dispose();
            }
        }

		/// <summary>
		/// Handles the Unloaded event of the ListBox control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		static void ListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (Associations.ContainsKey(listBox))
                Associations[listBox].Dispose();
            listBox.Unloaded -= ListBox_Unloaded;
        }

		/// <summary>
		/// Handles the Loaded event of the ListBox control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		static void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            var incc = listBox.Items as INotifyCollectionChanged;
            if (incc == null) return;
            listBox.Loaded -= ListBox_Loaded;
            Associations[listBox] = new Capture(listBox);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <seealso cref="System.IDisposable" />
		class Capture : IDisposable
        {
			/// <summary>
			/// Gets or sets the list box.
			/// </summary>
			/// <value>
			/// The list box.
			/// </value>
			public ListBox listBox { get; set; }
			/// <summary>
			/// Gets or sets the incc.
			/// </summary>
			/// <value>
			/// The incc.
			/// </value>
			public INotifyCollectionChanged incc { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Capture"/> class.
			/// </summary>
			/// <param name="listBox">The list box.</param>
			public Capture(ListBox listBox)
            {
                this.listBox = listBox;
                incc = listBox.ItemsSource as INotifyCollectionChanged;
                if (incc != null)
                {
                    incc.CollectionChanged +=
                        new NotifyCollectionChangedEventHandler(incc_CollectionChanged);
                }
            }

			/// <summary>
			/// Handles the CollectionChanged event of the incc control.
			/// </summary>
			/// <param name="sender">The source of the event.</param>
			/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
			void incc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    listBox.ScrollIntoView(e.NewItems[0]);
                    listBox.SelectedItem = e.NewItems[0];
                }
            }

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
            {
                if (incc != null)
                    incc.CollectionChanged -= incc_CollectionChanged;
            }
        }
    }
}
