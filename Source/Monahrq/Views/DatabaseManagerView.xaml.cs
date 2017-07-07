using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.ViewModels;

namespace Monahrq.Views
{
    /// <summary>
    /// Interaction logic for DatabaseManagerView.xaml
    /// </summary>
    [Export(typeof(DatabaseManagerView))]
    public partial class DatabaseManagerView : UserControl
    {
        public DatabaseManagerView()
        {
            InitializeComponent();
        }

        [Import]
        protected DatabaseManagerViewModel Model
        {
            set
            {
                DataContext = value;
            }
        }
    }
}
