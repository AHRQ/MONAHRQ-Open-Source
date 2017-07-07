using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Sdk.Attributes;

namespace Monahrq.DataSets.HospitalRegionMapping.Regions
{
    [ViewExport(typeof(RegionsView), RegionName = Mapping.RegionNames.Regions)]
    public partial class RegionsView
    {
        public RegionsView()
        {
            InitializeComponent();
            regionsDataGrid.Loaded += _regionsDataGridLoaded;
            Loaded += RegionsView_Loaded;
        }

        void RegionsView_Loaded(object sender, RoutedEventArgs e)
        {
        }

        void _regionsDataGridLoaded(object sender, RoutedEventArgs e)
        {
            var datagrid = sender as DataGrid;

            if (datagrid == null) return;

            datagrid.SelectionChanged += _datagridSelectionChanged;
        }

        static void _datagridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null || grid.SelectedItem == null) return;
            if (grid.Dispatcher.CheckAccess())
            {
                grid.Dispatcher.Invoke(() => grid.ScrollIntoView(grid.SelectedItem, null));
            }
        }

        public RegionsViewModel Model
        {
            get
            {
                return DataContext as RegionsViewModel;
            }
        }


        /*This is an event handler to support single click on a row to enter 
         * edit mode (Default mode is double click, which is not user friendly)*/
        
        #region //TODO: Cleanup later Eric Large
                //private void _dataGridCellPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
                //{
                //    var cell = sender as DataGridCell;
                //    if (cell == null || cell.IsEditing || cell.IsReadOnly) return;

                //    if (!cell.IsFocused)
                //    {
                //        cell.Focus();
                //    }

                //    var dataGrid = FindVisualParent<DataGrid>(cell);
                //    if (dataGrid == null) return;

                //    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                //    {
                //        if (!cell.IsSelected)
                //            cell.IsSelected = true;
                //    }
                //    else
                //    {
                //        var row = FindVisualParent<DataGridRow>(cell);
                //        if (row != null && !row.IsSelected)
                //        {
                //            row.IsSelected = true;
                //        }
                //    }
                //}

                //static T FindVisualParent<T>(UIElement element) where T : UIElement
                //{
                //    var parent = element;
                //    while (parent != null)
                //    {
                //        var correctlyTyped = parent as T;
                //        if (correctlyTyped != null)
                //        {
                //            return correctlyTyped;
                //        }

                //        parent = VisualTreeHelper.GetParent(parent) as UIElement;
                //    }
                //    return null;
                //}

                //private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
                //{
                //    var btn = sender as Button;
                //    var grid = FindVisualParent<DataGrid>(btn);
                //    var dataGrid = FindVisualParent<DataGridRow>(btn);
                //    var cell1 = grid.GetCell(dataGrid, 5);
                //    cell1.IsEditing = true;
                //    cell1.IsEnabled = true;
                //    cell1.Focus();
                //}

                //private void TxtTitle_OnLostFocus(object sender, RoutedEventArgs e)
                //{
                //    if (Model == null) return;

                //    var item = (sender as TextBox);
                //    if (item == null) return;
                //    var region = (Region)item.DataContext;
                //    if (string.IsNullOrWhiteSpace(item.Text))
                //        item.Text = region.Name;

                //    region.Name = item.Text;
                //    Model.SaveSelectedItemCommand.Execute(region);
                //}

                //private void ButtonBase_OnLostFocus(object sender, RoutedEventArgs e)
                //{
                //    var btn = sender as Button;
                //    var grid = FindVisualParent<DataGrid>(btn);
                //    var dataGrid = FindVisualParent<DataGridRow>(btn);
                //    var cell1 = grid.GetCell(dataGrid, 4);
                //    cell1.IsEditing = false;
                //    cell1.IsEnabled = false;
                //}
        #endregion
    }

    public class RegionTypeConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var region = value as Region;
            if (region is CustomRegion)
                return "Custom Region";
            if (region is HospitalServiceArea)
                return "HSA";
            if (region is HealthReferralRegion)
                return "HRR";

            return "N/A";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class DeleteRegionButtonVisibility : IValueConverter
    {

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var test = (bool)value;
            return test ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
