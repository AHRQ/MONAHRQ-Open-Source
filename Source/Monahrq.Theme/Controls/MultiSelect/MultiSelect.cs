using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Monahrq.Theme.Controls.MultiSelect
{
	/// <summary>
	/// MultiSelect control.
	/// </summary>
	public static class MultiSelect
    {
		/// <summary>
		/// Gets the is enabled.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <returns></returns>
		public static bool GetIsEnabled(Selector target)
        {
            return (bool)target.GetValue(IsEnabledProperty);
        }

		/// <summary>
		/// Sets the is enabled.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="value">if set to <c>true</c> [value].</param>
		public static void SetIsEnabled(Selector target, bool value)
        {
            target.SetValue(IsEnabledProperty, value);
        }

		/// <summary>
		/// The is enabled property
		/// </summary>
		public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(MultiSelect),
                new UIPropertyMetadata(IsEnabledChanged));

		/// <summary>
		/// The select only property
		/// </summary>
		public static readonly DependencyProperty SelectOnlyProperty =
            DependencyProperty.RegisterAttached("SelectOnly", typeof(bool), typeof(MultiSelect),
                new UIPropertyMetadata(SelectOnlyChanged));

		/// <summary>
		/// Gets or sets a value indicating whether [select only].
		/// </summary>
		/// <value>
		///   <c>true</c> if [select only]; otherwise, <c>false</c>.
		/// </value>
		public static bool SelectOnly { get; set; }

		/// <summary>
		/// Selects the only changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void SelectOnlyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Selector selector = sender as Selector;
            if (selector == null) return;
            
            SelectOnly = (bool)e.NewValue;
        }

		/// <summary>
		/// Determines whether [is enabled changed] [the specified sender].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Selector selector = sender as Selector;
            if (selector == null) return;
            bool enabled = (bool) e.NewValue;

            DependencyPropertyDescriptor itemsSourceProperty =
                DependencyPropertyDescriptor.FromProperty(Selector.ItemsSourceProperty, typeof (Selector));
            IMultiSelectCollectionView collectionView = selector.ItemsSource as IMultiSelectCollectionView;

            if (enabled)
            {
                if (collectionView != null)
                {
                    collectionView.SetSelectOnly(SelectOnly);
                    collectionView.AddControl(selector);
                }
                   
                itemsSourceProperty.AddValueChanged(selector, ItemsSourceChanged);
            }
            else
            {
                if (collectionView != null)
                {
                    collectionView.SetSelectOnly(SelectOnly);
                    collectionView.RemoveControl(selector);
                }
                    
                itemsSourceProperty.RemoveValueChanged(selector, ItemsSourceChanged);
            }

        }

		/// <summary>
		/// Items the source changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		static void ItemsSourceChanged(object sender, EventArgs e)
        {
            Selector selector = sender as Selector;

            if (selector == null) return;

            if (GetIsEnabled(selector))
            {
                IMultiSelectCollectionView oldCollectionView;
                IMultiSelectCollectionView newCollectionView = selector.ItemsSource as IMultiSelectCollectionView;
                _collectionViews.TryGetValue(selector, out oldCollectionView);

                if (oldCollectionView != null)
                {
                    oldCollectionView.RemoveControl(selector);
                    _collectionViews.Remove(selector);
                }

                if (newCollectionView != null)
                {
                    newCollectionView.AddControl(selector);
                    _collectionViews.Add(selector, newCollectionView);
                }
            }
        }

		/// <summary>
		/// The collection views
		/// </summary>
		static readonly Dictionary<Selector, IMultiSelectCollectionView> _collectionViews =
            new Dictionary<Selector, IMultiSelectCollectionView>();
    }
}
