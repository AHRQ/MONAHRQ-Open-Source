using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.ViewModels;

namespace Monahrq.Views
{
    /// <summary>
    /// Interaction logic for ManageSettingsView.xaml
    /// </summary>
    [Export("ManageSettingsView")]
    public partial class ManageSettingsView : UserControl
    {
        public ManageSettingsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the model.
        /// </summary>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        protected ManageSettingsViewModel Model
        {
            get { return DataContext as ManageSettingsViewModel; }
            set
            {
                DataContext = value;
            }
        }
    }
}
