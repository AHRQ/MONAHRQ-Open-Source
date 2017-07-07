using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Measures.ViewModels;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Common;
using Monahrq.Sdk.ViewModels;

namespace Monahrq.Measures.Views
{
    /// <summary>
    /// Interaction logic for ManageMeasuresView.xaml
    /// </summary>
    [ViewExport(typeof(ManageMeasuresView), RegionName = "MeasuresManageRegion")]
    public partial class ManageMeasuresView : UserControl
    {
        //[ImportingConstructor]
        public ManageMeasuresView()
        {
            InitializeComponent();
            Loaded += ManageMeasuresView_Loaded;
        }

        void ManageMeasuresView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        [Import]
        public ManageMeasuresViewModel Model
        {
            get
            {
                return DataContext as ManageMeasuresViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        // Left-side filter
        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Model == null) return;

            if (!Model.ApplyDataSetFilterCommand.CanExecute()) return;

            var tb = sender as TextBox;
            if (tb == null) return;
            Model.PropertyFilterText = tb.Text;
            Model.ApplyDataSetFilterCommand.Execute();
        }

        private void MeasuresDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WaitCursor.Show();
            if (sender == null) return;
            var dgr = sender as DataGrid;

            if (dgr == null) return;

            var measure = dgr.SelectedItem as Measure;
            if (measure != null)
            {
                Model.NavigateToDetailsCommand.Execute(measure);
            }
        }
    }
}
