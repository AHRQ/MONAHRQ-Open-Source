using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using Monahrq.Websites.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Monahrq.Websites.Views
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    //[Export(ViewNames.WebsitePagesView)]
    [ViewExport(typeof(WebsitePagesView), RegionName = RegionNames.WebsitePagesView)]
    public partial class WebsitePagesView : UserControl
    {
        public WebsitePagesView()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;

            InitializeComponent();
            if (Model == null)
                ;// this.DataContext = new WebsitePagesViewModel();

            Loaded += WebsitePagesView_Loaded;
        }

        private void WebsitePagesView_Loaded(object sender, RoutedEventArgs e)
        {
        }

        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public WebsitePagesViewModel Model
        {
            get
            {
                return DataContext as WebsitePagesViewModel;
            }
            set
            {
                DataContext = value;
            }
        }
    }
}
