using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.DataSets.ViewModels;
using Monahrq.Sdk.Regions;

namespace Monahrq.DataSets.Views
{
    /// <summary>
    /// Interaction logic for MappingReImportView.xaml
    [Export(ViewNames.MappingReImportView)]
    public partial class MappingReImportView : UserControl
    {
        [ImportingConstructor]
        public MappingReImportView(MappingReimportViewModel viewmodel)
        {
            InitializeComponent();
            if (viewmodel != null)
                this.DataContext = viewmodel;
        }
    }
}


