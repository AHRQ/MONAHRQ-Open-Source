using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Events;
using Monahrq.Sdk.Regions;
using Monahrq.Websites.Events;
using Monahrq.Websites.ViewModels;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Theme.Controls;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Sdk.ViewModels;

namespace Monahrq.Websites.Views
{
    /// <summary>
    /// Interaction logic for WebsiteManageView.xaml
    /// </summary>
    [Export(typeof(WebsiteManageView)), PartCreationPolicy(CreationPolicy.Shared)]
    public partial class WebsiteManageView : TabOwnerUserControl
    {
        public WebsiteManageView()
        {
            InitializeComponent();
            Loaded += WebsiteManageView_Loaded;
        }

        void WebsiteManageView_Loaded(object sender, RoutedEventArgs e)
        {}

        [Import]
        public WebsiteManageViewModel Model
        {
            get { return DataContext as WebsiteManageViewModel; }
            set { DataContext = value; }
        }

        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public IRegionManager RegionManager { get; set; }
        private void region_Loaded(object sender, RoutedEventArgs e) {
            //var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            //var RegionName = ((RegionAttachedEventArgs)e).RegionName;
            //if (RegionName == null) return;
            //regionManager.RegisterViewWithRegion(RegionName, typeof(WebsiteDetailsView));
            //regionManager.RegisterViewWithRegion(RegionName, typeof(WebsiteDatasetsView));
            //regionManager.RegisterViewWithRegion(RegionName, typeof(WebsiteMeasuresView));
            //regionManager.RegisterViewWithRegion(RegionName, typeof(WebsiteBuildReportsTabsView));
            //regionManager.RegisterViewWithRegion(RegionName, typeof(WebsiteSettingsView));
            //regionManager.RegisterViewWithRegion(RegionName, typeof(WebsitePublishView));
        }


        public override void OnUpdateTabIndex(TabIndexSelecteor obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.TabName) || obj.TabName != WebsiteTabs.Name) return;

            WebsiteTabs.SelectedIndex = obj.TabIndex;
        }
    }

   
}
