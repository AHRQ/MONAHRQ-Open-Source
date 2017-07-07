using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using Monahrq.Sdk.ViewModels;

namespace Monahrq.Theme.Behaviors
{
	/// <summary>
	/// Behavior for scrolling DataGrids.
	/// </summary>
	/// <seealso cref="System.Windows.Interactivity.Behavior{System.Windows.Controls.DataGrid}" />
	public class DataGridScollBehavior : Behavior<DataGrid>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="DataGridScollBehavior"/> class.
		/// </summary>
		public DataGridScollBehavior() { }

		#region Properties

		/// <summary>
		/// The left button clicked
		/// </summary>
		private bool _leftButtonClicked;

		#endregion

		#region Overriden Methods

		/// <summary>
		/// Called after the behavior is attached to an AssociatedObject.
		/// </summary>
		/// <remarks>
		/// Override this to hook up functionality to the AssociatedObject.
		/// </remarks>
		protected override void OnAttached()
        {
            base.OnAttached();
            //EventManager.RegisterClassHandler(typeof(ScrollViewer), ScrollViewer.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(DataGridScollBehavior_PreviewMouseWheelChanged), true);
            EventManager.RegisterClassHandler(typeof(ScrollViewer), ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(DataGridScollBehavior_ScrollChanged), true);
        }

		#endregion

		#region Events

		/// <summary>
		/// Handles the PreviewMouseWheelChanged event of the DataGridScollBehavior control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void DataGridScollBehavior_PreviewMouseWheelChanged(object sender, RoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;

            if (scrollViewer != null)
            {
                DataGrid datagrid = GetParent(scrollViewer, typeof(DataGrid)) as DataGrid;
                if (datagrid == null) return;

                var position = Mouse.GetPosition(scrollViewer);
                if (position.X < 100) return;

                Fetch(position.Y >= (datagrid.ActualHeight * .85), datagrid);
            }
            _leftButtonClicked = true;
        }

		/// <summary>
		/// Handles the ScrollChanged event of the DataGridScollBehavior control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void DataGridScollBehavior_ScrollChanged(object sender, RoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;

            ScrollChangedEventArgs args = e as ScrollChangedEventArgs;

            if (scrollViewer != null)
            {
                DataGrid datagrid = GetParent(scrollViewer, typeof(DataGrid)) as DataGrid;
                Fetch(args.VerticalChange > 0.9 && !_leftButtonClicked, datagrid);
            }
            _leftButtonClicked = false;
        }

		#endregion

		#region Helper Methods

		/// <summary>
		/// Gets the parent.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="parentType">Type of the parent.</param>
		/// <returns></returns>
		private DependencyObject GetParent(UIElement element, Type parentType)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(element);

            while (parent != null && parent.GetType() != parentType)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }

		/// <summary>
		/// Gets the child.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="childType">Type of the child.</param>
		/// <returns></returns>
		private DependencyObject GetChild(UIElement element, Type childType)
        {
            DependencyObject child = VisualTreeHelper.GetChild(element, 0);

            while (child != null && child.GetType() != childType)
            {
                child = VisualTreeHelper.GetChild(child, 0);
            }

            return child;
        }

		/// <summary>
		/// Fetches the specified is end of scroll.
		/// </summary>
		/// <param name="isEndOfScroll">if set to <c>true</c> [is end of scroll].</param>
		/// <param name="datagrid">The datagrid.</param>
		private void Fetch(bool isEndOfScroll, DataGrid datagrid)
        {
            if (!isEndOfScroll || datagrid == null) return;

            var viewModel = (datagrid.DataContext as IPaging);
            if (viewModel != null)
                viewModel.Fetch();
        }

        #endregion
    }
}
