using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.Sdk.Regions;
using Monahrq.Websites.ViewModels;
using Microsoft.Practices.Prism.Events;
using Monahrq.Sdk.Attributes;

namespace Monahrq.Websites.Views
{
    [ViewExport(typeof(WebsiteDatasetsView), RegionName = RegionNames.WebsiteManageRegion)]
    public partial class WebsiteDatasetsView
    {
        [ImportingConstructor]
        public WebsiteDatasetsView()
        {
            InitializeComponent();
        }

        [Import]
        public WebsiteDatasetsViewModel Model
        {
            get
            {
                return DataContext as WebsiteDatasetsViewModel;
            }
            set
            {
                DataContext = value;
            }
        }
    }
}
