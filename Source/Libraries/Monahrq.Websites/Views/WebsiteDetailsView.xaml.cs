using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.Sdk.Regions;
using Monahrq.Websites.ViewModels;
using Monahrq.Sdk.Attributes;

namespace Monahrq.Websites.Views
{
    [ViewExport(typeof(WebsiteDetailsView), RegionName = RegionNames.WebsiteDetailsRegion)]
    public partial class WebsiteDetailsView
    {
        public WebsiteDetailsView()
        {
            InitializeComponent();
        }

        [Import]
        public WebsiteDetailsViewModel Model
        {
            get { return DataContext as WebsiteDetailsViewModel; }
            set { DataContext = value; }
        }
    }
}
