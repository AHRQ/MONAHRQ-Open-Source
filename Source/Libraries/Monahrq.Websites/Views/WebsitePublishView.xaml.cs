using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using Monahrq.Websites.ViewModels;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Services.Generators;

namespace Monahrq.Websites.Views
{
    /// <summary>
    /// Interaction logic for WebsitePublishView.xaml
    /// </summary>
    [ViewExport(typeof(WebsitePublishView), RegionName = RegionNames.WebsiteManageRegion)]
    public partial class WebsitePublishView : UserControl
    {
        public WebsitePublishView()
        {
            InitializeComponent();
        }

        [Import]
        public WebsitePublishViewModel Model
        {
            get
            {
                return DataContext as WebsitePublishViewModel;
            }
            set
            {
                value.LogDataGrid = PublishLogDataGrid;
                DataContext = value;
            }
        }
	}
}
