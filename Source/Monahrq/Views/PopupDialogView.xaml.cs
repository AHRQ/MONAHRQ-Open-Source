using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using Monahrq.ViewModels;
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

namespace Monahrq.Views
{
    /// <summary>
    /// Interaction logic for PopupDialogView.xaml
    /// </summary>
    [ViewExport(typeof(PopupDialogView), RegionName = RegionNames.Modal)]
    public partial class PopupDialogView : UserControl
    {
        private 
        IRegion Region { get; set; }

        [ImportingConstructor]
        public PopupDialogView(PopupDialogViewModel popupDialogViewModel)
        {
            InitializeComponent();
            if (popupDialogViewModel != null)
                this.DataContext = popupDialogViewModel;

            Region = ServiceLocator.Current.GetInstance<IRegionManager>().Regions[RegionNames.Modal];
            Region.ActiveViews.ToList().ForEach(vw =>
                       Region.Deactivate(vw));
        }

        private void CmdClose_OnClick(object sender, RoutedEventArgs e)
        {
            Region.Deactivate(this);
            Region.Remove(this);
        }
    }
}
