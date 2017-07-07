using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.Websites.ViewModels;

namespace Monahrq.Websites.Views
{
    /// <summary>
    /// Interaction logic for WebsitesView.xaml
    /// </summary>
    /// 
    /// TODO: THIS IS NOT IMPORTING CORRECTLY FIX IT
    [Export(typeof(WebsiteCollectionView))]
    public partial class WebsiteCollectionView : UserControl
    {
        public WebsiteCollectionView()
        {
            InitializeComponent();
        }

        [Import]
        WebsiteCollectionViewModel Model
        {
            get
            {
                return DataContext as WebsiteCollectionViewModel;
            }
            set
            {
                DataContext = value;
            }
        }
    }
}
