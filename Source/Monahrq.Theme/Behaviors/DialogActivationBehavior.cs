using System.Collections.Specialized;
using System.Windows;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Regions.Behaviors;

namespace Monahrq.Theme.Behaviors
{
	/// <summary>
	/// Defines a behavior that creates a Dialog to display the active view of the target <see cref="IRegion" />.
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Regions.RegionBehavior" />
	/// <seealso cref="Microsoft.Practices.Prism.Regions.Behaviors.IHostAwareRegionBehavior" />
	public abstract class DialogActivationBehavior : RegionBehavior, IHostAwareRegionBehavior
    {
		/// <summary>
		/// The key of this behavior
		/// </summary>
		public const string BEHAVIOR_KEY = "DialogActivation";

		/// <summary>
		/// The content dialog
		/// </summary>
		private IWindow _contentDialog;

		/// <summary>
		/// Gets or sets the <see cref="DependencyObject" /> that the <see cref="IRegion" /> is attached to.
		/// </summary>
		/// <value>
		/// A <see cref="DependencyObject" /> that the <see cref="IRegion" /> is attached to.
		/// This is usually a <see cref="FrameworkElement" /> that is part of the tree.
		/// </value>
		public DependencyObject HostControl { get; set; }

		/// <summary>
		/// Performs the logic after the behavior has been attached.
		/// </summary>
		protected override void OnAttach()
        {
            Region.ActiveViews.CollectionChanged += ActiveViews_CollectionChanged;
        }

		/// <summary>
		/// Override this method to create an instance of the <see cref="IWindow" /> that
		/// will be shown when a view is activated.
		/// </summary>
		/// <returns>
		/// An instance of <see cref="IWindow" /> that will be shown when a
		/// view is activated on the target <see cref="IRegion" />.
		/// </returns>
		protected abstract IWindow CreateWindow();

		/// <summary>
		/// Handles the CollectionChanged event of the ActiveViews control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		private void ActiveViews_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                CloseContentDialog();
                PrepareContentDialog(e.NewItems[0]);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                CloseContentDialog();
            }
        }

		/// <summary>
		/// Gets the style for view.
		/// </summary>
		/// <returns></returns>
		private Style GetStyleForView()
        {
            return HostControl.GetValue(RegionPopupBehaviors.ContainerWindowStyleProperty) as Style;
        }

		/// <summary>
		/// Prepares the content dialog.
		/// </summary>
		/// <param name="view">The view.</param>
		private void PrepareContentDialog(object view)
        {
            _contentDialog = CreateWindow();
            _contentDialog.Content = view;
            _contentDialog.Owner = HostControl;
            _contentDialog.Closed += ContentDialogClosed;
            _contentDialog.Style = GetStyleForView();
            _contentDialog.Show();
        }

		/// <summary>
		/// Closes the content dialog.
		/// </summary>
		private void CloseContentDialog()
        {
            if (_contentDialog != null)
            {
                _contentDialog.Closed -= ContentDialogClosed;
                _contentDialog.Close();
                _contentDialog.Content = null;
                _contentDialog.Owner = null;
            }
        }

		/// <summary>
		/// Contents the dialog closed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ContentDialogClosed(object sender, System.EventArgs e)
        {
            Region.Deactivate(_contentDialog.Content);
            CloseContentDialog();
        }
    }
}
