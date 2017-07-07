using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.Sdk.Regions;
using Monahrq.Websites.ViewModels;

namespace Monahrq.Websites.Views
{
    /// <summary>
    /// Interaction logic for WebsiteBuildReportsHospitalsView.xaml
    /// </summary>
    [Export(ViewNames.WebsiteBuildReportsHospitalsView)]
    public partial class WebsiteBuildReportsHospitalsView : UserControl
    {
        public WebsiteBuildReportsHospitalsView()
        {
            InitializeComponent();
        }

        [Import]
        WebsiteSettingsViewModel Model
        {
            get
            {
                return DataContext as WebsiteSettingsViewModel;
            }
            set
            {
                DataContext = value;
            }
        }
    }
}
