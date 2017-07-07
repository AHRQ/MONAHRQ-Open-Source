using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.Sdk.Regions;

namespace Monahrq.Websites.Views
{
    /// <summary>
    /// Interaction logic for WebsiteBuildReportsUsageView.xaml
    /// </summary>
    [Export(ViewNames.WebsiteBuildReportsUsageView)]
    public partial class WebsiteBuildReportsUsageView : UserControl
    {
        public WebsiteBuildReportsUsageView()
        {
            InitializeComponent();
        }
    }
}
