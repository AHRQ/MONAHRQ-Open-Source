using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.Sdk.Attributes;
using Monahrq.ViewModels;

namespace Monahrq.Views
{
    /// <summary>
    /// Interaction logic for StatusbarView.xaml
    /// </summary>
    [ViewExport(typeof(StatusbarView), RegionName = "StatusBarRegion")]
    public partial class StatusbarView : UserControl
    {
        public StatusbarView()
        {
            InitializeComponent();

            //ProgressBar.IsIndeterminate = true;
        }

        [Import]
        StatusbarViewModel Model
        {
            get { return DataContext as StatusbarViewModel; }
            set { DataContext = value; }
        } 

    }
}
