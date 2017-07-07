using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Monahrq.DataSets.Model;
using Monahrq.DataSets.ViewModels;

namespace Monahrq.DataSets.Controls
{
    /// <summary>
    /// Interaction logic for DragDropMappingControl.xaml
    /// </summary>
    public partial class DragDropMappingControl : UserControl
    {
        public DragDropMappingControl()
        {
            InitializeComponent();
        }
        
        /* old code using telerik drag manager
         *

        ctor:
        RequiredFields.AddHandler(RadDragAndDropManager.DragQueryEvent, new EventHandler<DragDropQueryEventArgs>(OnDragQuery));
        RequiredFields.AddHandler(RadDragAndDropManager.DragInfoEvent, new EventHandler<DragDropEventArgs>(OnDragInfo));
        OptionalFields.AddHandler(RadDragAndDropManager.DragQueryEvent, new EventHandler<DragDropQueryEventArgs>(OnDragQuery));
        OptionalFields.AddHandler(RadDragAndDropManager.DragInfoEvent, new EventHandler<DragDropEventArgs>(OnDragInfo));
        TargetFields.AddHandler(RadDragAndDropManager.DropQueryEvent, new EventHandler<DragDropQueryEventArgs>(OnDropQuery));
        TargetFields.AddHandler(RadDragAndDropManager.DragQueryEvent, new EventHandler<DragDropQueryEventArgs>(OnDragQuery));
        TargetFields.AddHandler(RadDragAndDropManager.DropInfoEvent, new EventHandler<DragDropEventArgs>(OnDropInfo));
        TargetFields.AddHandler(RadDragAndDropManager.DragInfoEvent, new EventHandler<DragDropEventArgs>(OnDragInfo));

        private void OnDragQuery(object sender, DragDropQueryEventArgs e)
        {

            if (e.Options.Status == DragStatus.DragQuery)
            {
                var draggedItem = e.Options.Source as ListBoxItem;
                e.QueryResult = true;
                e.Handled = true;
                // Create Drag and Arrow Cue
                e.Options.DragCue = new ContentControl
                    {
                        Content = draggedItem.DataContext,
                        ContentTemplate = Resources["DragCueTemplate"] as DataTemplate
                    };
                e.Options.ArrowCue = RadDragAndDropManager.GenerateArrowCue();
                // Set the payload (this is the item that is currently dragged)
                e.Options.Payload = draggedItem.DataContext as MTargetField;
            }
            if (e.Options.Status == DragStatus.DropSourceQuery)
            {
                e.QueryResult = true;
                e.Handled = true;
            }
        }

        private void OnDragInfo(object sender, DragDropEventArgs e)
        {

            //PrepareToMonitorAutoScrollZones(e);
            // if we are dropping on the appropriate listbox, then remove the item from the first listbox.
            if (e.Options.Status == DragStatus.DragComplete)
            {
                var itemsControl = e.Options.Source.FindItemsConrolParent() as ItemsControl;

                var originalItem = e.Options.Payload as MTargetField;
                var itemsSource = itemsControl.ItemsSource as ListCollectionView;

                itemsSource.Remove(originalItem);
                itemsSource.Refresh();
            }
        }

        private void OnDropQuery(object sender, DragDropQueryEventArgs e)
        {
            var gridControl = e.Options.Destination as Grid;
            AutoScrollFromScrollingZones(gridControl);
            
            var destination = e.Options.Destination as ListBoxItem; // ItemsControl;
            var dropTargetType = e.Options.Destination.GetType().Name;
            if (e.Options.Status == DragStatus.DropDestinationQuery &&
                destination != null)
            {
                e.QueryResult = true;
                e.Handled = true;
            }
        }

        private Border droppingTarget;
        public Border DroppingTarget {
            get { return droppingTarget; }
            set {
                if (droppingTarget != null) { //remove effects
                    droppingTarget.BorderBrush = null;
                }
                droppingTarget = value;
                if (droppingTarget != null) { //apply effects
                    droppingTarget.BorderBrush = Brushes.Red;
                }
            }
        }


        private void OnDropInfo(object sender, DragDropEventArgs e)
        {
            // if we are dropping on the appropriate listbox, then add the dragged item to it.
            var destination = e.Options.Destination as ListBoxItem; // ItemsControl;
            //var source = e.Options.Source as ListBoxItem;
            //ContentControl dragCue = e.Options.DragCue as ContentControl;

            if (e.Options.Status == DragStatus.DropPossible && destination != null)
            {
                var border = destination.FindVisualChildren<Border>().FirstOrDefault();
                DroppingTarget = border;
                destination.Background = new SolidColorBrush(Color.FromRgb(43, 128, 165));
                e.Handled = true;
            }
            else if (e.Options.Status == DragStatus.DropImpossible)
            {
                DroppingTarget = null;
                if (destination != null) destination.Background = null;
                e.Handled = true;
            }
            else if (e.Options.Status == DragStatus.DropComplete &&
                 destination != null)
            {
                var destinationListBox = destination.FindItemsConrolParent() as ItemsControl;

                if (destinationListBox != null)
                {
                    MTargetField monahrqField = e.Options.Payload as MTargetField;
                    var destinationSelectedItem = destination.DataContext as MOriginalField;
                    var vm = DataContext as ElementMappingViewModelBase<DatasetContext>;
                    if (destinationSelectedItem.TargetField != null) vm.ExecuteRemoveMapping(destinationSelectedItem);
                    destinationSelectedItem.TargetField = monahrqField;
                    monahrqField.MappedField = destinationSelectedItem;
                    destinationSelectedItem.IsAutoMapped = false;

                    destinationSelectedItem.OnMappingChanged(EventArgs.Empty);

                    (destinationListBox.ItemsSource as ListCollectionView).Refresh();

                    e.Handled = true;
                }

            }
        }
        */

        private bool isDragging = false;
        const string AutoScrollUpZone = "AutoScrollUpZone";
        const string AutoScrollDownZone = "AutoScrollDownZone";
        private static void AutoScrollFromScrollingZones(Grid gridControl)
        {
            if (gridControl == null) return;

            var parent = ControlExtensions.FindVisualParent<Grid>(gridControl);
            if (parent == null) return;

            var scrollViewer = parent.FindVisualChildren<ScrollViewer>().FirstOrDefault();
            if (scrollViewer == null) return;

            string autoScrollZoneName = string.Empty;
            if (gridControl != null) autoScrollZoneName = gridControl.Name;

            if (autoScrollZoneName == AutoScrollDownZone)
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + ListBoxAutoScrollBehavior.VerticalScrollStep);

            if (autoScrollZoneName == AutoScrollUpZone)
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - ListBoxAutoScrollBehavior.VerticalScrollStep);
        }
        
        private void OnListBoxItemMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            var item = (ListBoxItem) sender;
            var field = item.DataContext as MTargetField;
            if (field == null)
                return;
            this.isDragging = true;
            Debug.WriteLine(string.Format("Starting drag: {0}", field.Caption));
            var o = new DataObject(DataFormats.StringFormat, field.Caption);
            o.SetData("Field", field);
            o.SetData("FieldList", item.FindItemsConrolParent());
            DragDrop.DoDragDrop(this, o, DragDropEffects.Link);
            e.Handled = true;
        }

        private void OnTargetFieldDragEnter(object sender, DragEventArgs e)
        {
            if (!this.isDragging)
                return;
            Debug.WriteLine("OnTargetFieldDragEnter: " + sender);
            var element = e.OriginalSource as FrameworkElement;
            if (element == null)
                return;
            var border = element.FindParentWithName<Border>("TargetField");
            if (border == null)
                return;
            border.BorderBrush = Brushes.Red;
            Mouse.SetCursor(Cursors.Hand);
            e.Handled = true;
        }

        private void OnTargetFieldDragExit(object sender, DragEventArgs e)
        {
            if (!this.isDragging)
                return;
            Debug.WriteLine("OnTargetFieldDragExit: " + sender);
            var element = e.OriginalSource as FrameworkElement;
            if (element == null)
                return;
            var border = element.FindParentWithName<Border>("TargetField");
            if (border == null)
                return;
            border.BorderBrush = Brushes.Transparent;
            Mouse.SetCursor(Cursors.No);
            e.Handled = true;
        }

        /*private void OnTargetFieldGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (!this.isDragging || e.Effects != DragDropEffects.Link)
            {
                // user isn't dragging a column
                Mouse.SetCursor(Cursors.No);
                return;
            }

            var over = e.OriginalSource as FrameworkElement ?? Mouse.DirectlyOver as FrameworkElement;
            if (over == null)
                return;

            //todo: make sure we're hovering over a column
            Mouse.SetCursor(Cursors.Hand);
            e.Handled = true;
            Debug.WriteLine(string.Format("OnTargetFieldGiveFeedback: over element of type {0}, context type {1}", over.GetType().Name, over.DataContext?.GetType().Name ?? "none"));
        }*/


        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            //this is only called for the control itself. OnTargetFieldGiveFeedback is intended to provide positive feedback

            base.OnGiveFeedback(e);
            if (!this.isDragging || e.Effects != DragDropEffects.Link)
            { 
                // user isn't dragging a column
                Mouse.SetCursor(Cursors.No);
                e.Handled = true;
                return;
            }
            
            var over = e.OriginalSource as FrameworkElement ?? Mouse.DirectlyOver as FrameworkElement;
            if (over == null)
                return;

            var overGrid = over as Grid;
            if (overGrid != null)
            {
                AutoScrollFromScrollingZones(overGrid);
                e.Handled = true;
                return;
            }

            //todo: make sure we're hovering over a column
            //Debug.WriteLine(string.Format("OnGiveFeedback: element of type {0}, context type {1}", over.GetType().Name, over.DataContext?.GetType().Name ?? "none"));
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            if (!this.isDragging)
                return;
            this.isDragging = false;

            if (!e.Data.GetDataPresent("Field") || !e.Data.GetDataPresent("FieldList"))
            {
                Debug.WriteLine("Drag aborted. Data is not what we're looking for.");
                return;
            }

            var vm = this.DataContext as IElementMappingViewModel;
            if (vm == null)
                return; // unsupported viewmodel type

            var target = (FrameworkElement)e.OriginalSource;
            var monahrqField = (MTargetField)e.Data.GetData("Field");
            if (monahrqField == null)
                return;
            var destinationSelectedItem = (MOriginalField)target.DataContext;
            
            // remove existing mapping
            if (destinationSelectedItem.TargetField != null)
                vm.ExecuteRemoveMapping(destinationSelectedItem);

            // set new mapping
            destinationSelectedItem.TargetField = monahrqField;
            monahrqField.MappedField = destinationSelectedItem;
            destinationSelectedItem.IsAutoMapped = false;
            destinationSelectedItem.OnMappingChanged(EventArgs.Empty);
            ((ListCollectionView)TargetFields.ItemsSource).Refresh();

            // remove field from columns to be mapped
            var itemsControl = (ListBox)e.Data.GetData("FieldList");
            var itemsSource = (ListCollectionView)itemsControl.ItemsSource;
            itemsSource.Remove(monahrqField);
            itemsSource.Refresh();

            Debug.WriteLine("Completing drag: {0} -> {1}", monahrqField.Caption, destinationSelectedItem.DisplayName);
        }
    }

    public static class ControlExtensions
    {
        public static ItemsControl FindItemsConrolParent(this FrameworkElement target)
        {
            ItemsControl result = null;
            result = target.Parent as ItemsControl;
            if (result != null)
                return result;

            result = ItemsControl.ItemsControlFromItemContainer(target);
            
            return result ?? FindVisualParent<ItemsControl>(target);
        }
        public static T FindVisualParent<T>(FrameworkElement target) where T : FrameworkElement
        {
            if (target == null)
            {
                return null;
            }
            var visParent = VisualTreeHelper.GetParent(target);
            var result = visParent as T;
            if (result != null)
            {
                return result;
            }
            return FindVisualParent<T>(visParent as FrameworkElement);
        }

        public static T FindParentWithName<T>(this FrameworkElement target, string name)
            where T : DependencyObject
        {
            for (; target != null && target.Name != name; target = (target.Parent as FrameworkElement)) ;
            return target as T;
        }

        public static T FindVisualChildByName<T>(this DependencyObject parent, string name) where T : DependencyObject {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                string controlName = child.GetValue(Control.NameProperty) as string;
                if (controlName == name) {
                    return child as T;
                }
                else {
                    T result = FindVisualChildByName<T>(child, name);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject {
            if (depObj != null) {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T) {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child)) {
                        yield return childOfChild;
                    }
                }
            }
        }

    }

    public class ListBoxAutoScrollBehavior : Behavior<ListBox>
    {
        internal const double VerticalScrollStep = 1.5;

        protected override void OnAttached()
        {
            base.OnAttached();

            EventManager.RegisterClassHandler(typeof(ScrollViewer), UIElement.DragOverEvent, new DragEventHandler(OnListBoxScrollViewerDrag), true);
        }

        private static void OnListBoxScrollViewerDrag(object sender, DragEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null)
                return;

            var relative = e.GetPosition(scrollViewer);
                
            if (relative.Y > 0 && relative.Y < 40)
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (VerticalScrollStep * ((40 - relative.Y) / 40)));

            if (relative.Y > scrollViewer.ActualHeight - 40 && relative.Y < scrollViewer.ActualHeight)
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + (VerticalScrollStep * ((40 - (scrollViewer.ActualHeight - relative.Y)) / 40)));

            if (relative.X > 0 && relative.X < 40)
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - (20 * ((40 - relative.X) / 40)));

            if (relative.X > scrollViewer.ActualWidth - 40 && relative.X < scrollViewer.ActualWidth)
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (20 * ((40 - (scrollViewer.ActualWidth - relative.X)) / 40)));
        }
    }
}
