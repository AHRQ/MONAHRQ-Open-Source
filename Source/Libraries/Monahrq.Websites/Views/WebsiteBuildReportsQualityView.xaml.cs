using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.Sdk.Regions;
using Monahrq.Websites.ViewModels;
namespace Monahrq.Websites.Views
{
    /// <summary>
    /// Interaction logic for WebsiteBuildReportsQualityView.xaml
    /// </summary>
    [Export(ViewNames.WebsiteBuildReportsQualityView)]
    public partial class WebsiteBuildReportsQualityView : UserControl
    {
        [ImportingConstructor]
        public WebsiteBuildReportsQualityView()//WebsiteBuildReportsQualityViewModel model)
        {
            InitializeComponent();
            //DataContext = model;
        }

        //[Import]
        //public WebsiteBuildReportsQualityViewModel Model
        //{
        //    get
        //    {
        //        return DataContext as WebsiteBuildReportsQualityViewModel;
        //    }
        //    set
        //    {
        //        DataContext = value;
        //    }
        //}
    }
}
