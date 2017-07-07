using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Theme.Controls.ModernUI.Presentation;

namespace Monahrq.Theme.Controls.MultiSelect
{
	/// <summary>
	/// DataGrid control.
	/// </summary>
	/// <seealso cref="System.Windows.Controls.DataGrid" />
	public class MonahrqCheckboxDataGrid : DataGrid
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="MonahrqCheckboxDataGrid"/> class.
		/// </summary>
		public MonahrqCheckboxDataGrid()
        {
            EventManager.RegisterClassHandler(typeof(DataGridCell), DataGridCell.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(OnPreviewMouseLeftButtonDown));
        }

		/// <summary>
		/// Called when [preview mouse left button down].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void OnPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            var cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (cell.Column != null && cell.Column.DisplayIndex > 0) return;

                var parentRow = cell.Parent as DataGridRow;
                if (parentRow != null)
                {
                    SelectedIndex = parentRow.GetIndex();
                }

                CurrentCell = new DataGridCellInfo(cell);
                BeginEdit(e);
                var cb = FindVisualChild<CheckBox>(cell);

                if (cb == null) return;

                //var cb = (CheckBox) obj;
                cb.Focus();
                cb.IsChecked = !cb.IsChecked;
            }
        }

		/// <summary>
		/// Finds the visual child.
		/// </summary>
		/// <typeparam name="TChild">The type of the child.</typeparam>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static TChild FindVisualChild<TChild>(DependencyObject obj) where TChild : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is TChild)
                {
                    return (TChild)child;
                }

                var childOfChild = FindVisualChild<TChild>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

		/// <summary>
		/// Called when [apply template].
		/// </summary>
		public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

		/// <summary>
		/// Handles the CheckedStateChanged event of the chkCategories control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		public void chkCategories_CheckedStateChanged(object sender, RoutedEventArgs e)
        {
            var cat = CurrentItem as ISelectable;
            var checkBox = sender as CheckBox;
            if (cat != null && checkBox != null)
            {
                cat.IsSelected = checkBox.IsChecked == true;
            }
        }
    }
}
