using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using Monahrq.ViewModels;

namespace Monahrq.Views
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    [ViewExport(typeof(AboutViewWindow), RegionName = RegionNames.Modal)]
    public partial class AboutViewWindow
    {
        IRegion Region { get; set; }
        public AboutViewWindow()
        {
            Region = ServiceLocator.Current.GetInstance<IRegionManager>().Regions[RegionNames.Modal];
            InitializeComponent();
            Region.ActiveViews.ToList().ForEach(vw =>
                       Region.Deactivate(vw));
        }

        private void CmdClose_OnClick(object sender, RoutedEventArgs e)
        {
            Region.Deactivate(this);
            Region.Remove(this);
        }

        [Import]
        protected AboutViewModel Model
        {
            set
            {
                DataContext = value;
            }
        }
    }
}
