using System.ComponentModel.Composition;
using Monahrq.ViewModels;

namespace Monahrq.Views
{
    /// <summary>
    /// Interaction logic for DatabaseManagerView.xaml
    /// </summary>
    [Export(typeof(ManageConnectionStringsView))]
    public partial class ManageConnectionStringsView
    {
        public ManageConnectionStringsView()
        {
            InitializeComponent();
        }

        [Import]
        protected ManageConnectionStringsViewModel Model
        {
            set
            {
                DataContext = value;
            }
        }
    }
}
