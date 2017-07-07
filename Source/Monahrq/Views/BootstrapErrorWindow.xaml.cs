using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace Monahrq.Views
{
    /// <summary>
    /// This window displays an error message with a hyperlink that can occur during bootstrap sequence.
    /// </summary>
    public partial class BootstrapErrorWindow : Window
    {
        public BootstrapErrorWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void CmdClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
