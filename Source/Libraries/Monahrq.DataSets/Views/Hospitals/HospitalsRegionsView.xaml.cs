using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Monahrq.DataSets.Services;
using Monahrq.DataSets.ViewModels.Hospitals;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Regions.Core;
using Remotion.Linq.Collections;

namespace Monahrq.DataSets.Views.Hospitals
{

    [ExportAsView(ViewNames.HospitalsRegionsView, Category = "DataSets", MenuName = "Hospitals", MenuItemName = "Hospital Regions")]
    [ExportViewToRegion(ViewNames.HospitalsRegionsView, RegionNames.HospitalRegionsRegion)]
    [Export(typeof(HospitalsRegionsView))]
    public partial class HospitalsRegionsView : UserControl  
    {
        public HospitalsRegionsView()
        {
            InitializeComponent();
            this.regionsDataGrid.Loaded += _regionsDataGridLoaded;
           
        }

        void _regionsDataGridLoaded(object sender, RoutedEventArgs e)
        {
            var datagrid = sender as DataGrid;
            datagrid.SelectionChanged += _datagridSelectionChanged;
        }

        static void _datagridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid.SelectedItem == null) return;
            if (grid.Dispatcher.CheckAccess())
            {
                grid.Dispatcher.Invoke(() => grid.ScrollIntoView(grid.SelectedItem, null));
            }
        }

        [Import]
        public RegionCollectionViewModel Model
        {
            get
            {
                return DataContext as RegionCollectionViewModel;
            }
            set
            {
                DataContext = value;
              
            }
        }

        void Model_RegionCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            regionsDataGrid.CommitEdit();
            regionsDataGrid.CancelEdit();
        }


        /*This is an event handler to support single click on a row to enter 
         * edit mode (Default mode is double click, which is not user friendly)*/

        private void _dataGridCellPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cell = sender as DataGridCell;
            if (cell == null || cell.IsEditing || cell.IsReadOnly) return;

            if (!cell.IsFocused)
            {
                cell.Focus();
            }

            
            var dataGrid = FindVisualParent<DataGrid>(cell);
            if (dataGrid == null) return;

            if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
            {
                if (!cell.IsSelected)
                    cell.IsSelected = true;
            }
            else
            {
                var row = FindVisualParent<DataGridRow>(cell);
                if (row != null && !row.IsSelected)
                {
                    row.IsSelected = true;
                }
            }
        }

      

        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            var parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var dataGrid = FindVisualParent<DataGridCell>(btn);
            dataGrid.IsEditing = true;
        }
    }
}
