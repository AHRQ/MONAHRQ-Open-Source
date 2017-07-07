using Monahrq.DataSets.ViewModels;
using System.Windows.Controls;
using Monahrq.Wing.Dynamic.Models;

namespace Monahrq.Wing.Dynamic.Views
{
    /// <summary>
    /// Interaction logic for FullWizardColumnMappingView.xaml
    /// </summary>
    public partial class FullWizardColumnMappingView : UserControl
    {
        public FullWizardColumnMappingView()
        {
            InitializeComponent();
        }

        public ElementMappingViewModelBase<WizardContext> Model
        {
            get
            {
                return DataContext as ElementMappingViewModelBase<WizardContext>;
            }
            set
            {
                DataContext = value;
            }
        }

        // Left-side filter
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Model == null) return;

            if (Model.ElementFilterChangedCommand.CanExecute(null))
            {
                var tb = sender as TextBox;
                if (tb == null) return;

                Model.CurrentElementFilter = tb.Text;
                Model.ElementFilterChangedCommand.Execute(tb.Text);
            }
        }
    }
}
