using System.ComponentModel.Composition;
using System.Windows;
using Monahrq.ViewModels;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;

namespace Monahrq.Views
{
    /// <summary>
    /// Interaction logic for LeftNavigation.xaml
    /// </summary>
    [ViewExport(typeof(LeftNavigationControl), RegionName = RegionNames.Navigation)]
    public partial class LeftNavigationControl
    {
        [ImportingConstructor]
        public LeftNavigationControl(LeftNavigationViewModel leftNavigationViewModel)
        {
            InitializeComponent();

            if (leftNavigationViewModel != null)
                this.DataContext = leftNavigationViewModel;
        }

        public void ShowLoadView(object sender, RoutedEventArgs e)
        {
        }
    }
}
