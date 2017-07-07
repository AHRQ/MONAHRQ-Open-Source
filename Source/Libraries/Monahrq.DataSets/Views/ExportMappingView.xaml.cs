using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.DataSets.ViewModels;

namespace Monahrq.DataSets.Views
{
    /// <summary>
    /// Interaction logic for ExportMappingView.xaml
    /// </summary>
    [Export("ExportMappingView")]
    public partial class ExportMappingView : UserControl
    {
        [ImportingConstructor]
        public ExportMappingView(ExportMappingViewModel viewmodel)
        {
            InitializeComponent();
            if (viewmodel != null)
                this.DataContext = viewmodel;
        }
    }
}
