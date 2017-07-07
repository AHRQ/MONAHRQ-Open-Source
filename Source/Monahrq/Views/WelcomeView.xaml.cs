using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using System.Windows.Controls;
using System.Windows;
using Monahrq.Theme.PopupDialog;
using Monahrq.ViewModels;
using System.Diagnostics;
using System.Windows.Navigation;

namespace Monahrq.Views
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    [ViewExport(typeof(WelcomeView), RegionName = RegionNames.MainContent)]
    public partial class WelcomeView : UserControl
    {
        //private readonly IRegionManager _regionManager;

        [ImportingConstructor]
        public WelcomeView(WelcomeViewModel welcomeViewModel)
        {
           // _regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            InitializeComponent();

            if (welcomeViewModel == null) throw new NullReferenceException("welcomeViewModel");
            DataContext = welcomeViewModel;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(string.Format("There was a problem opening up the website. Please see your administrator. Error  : {0}", ex.Message), "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void showMessageClick(object sender, RoutedEventArgs e)
        {
            var dialog = ServiceLocator.Current.GetInstance<IPopupDialogService>();
            dialog.Buttons = PopupDialogButtons.Yes | PopupDialogButtons.No;
            dialog.Message = "Here's your message";
            EventHandler handler = null;
            handler = (o, args) =>
                {
                    MessageBox.Show(string.Format("You Clicked: {0} ", dialog.ClickedButton));
                    dialog.Closed -= handler;
                };
            dialog.Closed += handler;
            dialog.ShowMessage();
        }
    }
}
