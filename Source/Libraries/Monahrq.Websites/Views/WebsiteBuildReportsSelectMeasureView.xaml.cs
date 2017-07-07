using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.Sdk.Regions;

namespace Monahrq.Websites.Views
{
    /// <summary>
    /// Interaction logic for WebsiteBuildReportsSelectMeasureView.xaml
    /// </summary>
    [Export(ViewNames.WebsiteBuildReportsSelectMeasureView)]
    public partial class WebsiteBuildReportsSelectMeasureView : UserControl
    {
        public WebsiteBuildReportsSelectMeasureView()
        {
            InitializeComponent();
        }
    }
}
