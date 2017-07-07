using Monahrq.DataSets.ViewModels;
using System.Windows.Controls;
using Monahrq.DataSets.Model;

namespace Monahrq.DataSets.Views
{
    /// <summary>
    /// Interaction logic for DefaultWizardColumnMappingView.xaml
    /// </summary>
    public partial class DefaultWizardColumnMappingView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWizardColumnMappingView"/> class.
        /// </summary>
        public DefaultWizardColumnMappingView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public ElementMappingViewModelBase<DatasetContext> Model
        {
            get
            {
                return DataContext as ElementMappingViewModelBase<DatasetContext>;
            }
            set
            {
                DataContext = value;
            }
        }
 
        /// <summary>
        /// Handles the TextChanged event of the Left-side filter TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
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
