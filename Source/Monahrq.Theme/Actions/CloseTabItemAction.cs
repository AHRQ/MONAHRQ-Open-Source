using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Monahrq.Theme.Actions
{
	/// <summary>
	/// Triggered action that handles a closed tab.
	/// </summary>
	/// <seealso cref="System.Windows.Interactivity.TriggerAction{System.Windows.DependencyObject}" />
	public class CloseTabItemAction : TriggerAction<DependencyObject>
    {
		/// <summary>
		/// Invokes the action.
		/// </summary>
		/// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
		protected override void Invoke(object parameter)
        {
            this.TabControl.Items.Remove(this.TabItem);
        }

		/// <summary>
		/// The tab control property
		/// </summary>
		public static readonly DependencyProperty TabControlProperty =
            DependencyProperty.Register("TabControl", typeof(TabControl), typeof(CloseTabItemAction), new PropertyMetadata(default(TabControl)));

		/// <summary>
		/// Gets or sets the tab control.
		/// </summary>
		/// <value>
		/// The tab control.
		/// </value>
		public TabControl TabControl
        {
            get { return (TabControl)GetValue(TabControlProperty); }
            set { SetValue(TabControlProperty, value); }
        }

		/// <summary>
		/// The tab item property
		/// </summary>
		public static readonly DependencyProperty TabItemProperty =
            DependencyProperty.Register("TabItem", typeof(TabItem), typeof(CloseTabItemAction), new PropertyMetadata(default(TabItem)));

		/// <summary>
		/// Gets or sets the tab item.
		/// </summary>
		/// <value>
		/// The tab item.
		/// </value>
		public TabItem TabItem
        {
            get { return (TabItem)GetValue(TabItemProperty); }
            set { SetValue(TabItemProperty, value); }
        }
    }
}