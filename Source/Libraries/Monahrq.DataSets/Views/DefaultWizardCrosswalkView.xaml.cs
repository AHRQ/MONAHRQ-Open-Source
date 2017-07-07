using Monahrq.DataSets.ViewModels.Crosswalk;
using System.Windows.Controls;

namespace Monahrq.DataSets.Views
{
    /// <summary>
    /// Interaction logic for DefaultWizardCrosswalkView.xaml
    /// </summary>
    public partial class DefaultWizardCrosswalkView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWizardCrosswalkView"/> class.
        /// </summary>
        public DefaultWizardCrosswalkView()
        {
            InitializeComponent();
            Loaded += (o, e) =>
            {
                Model.MappedFieldModels = Model.DataContextObject.CreateCrosswalkMappingFieldViewModels();
            };
         
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        FieldsViewModel Model
        {
            get
            {
                return DataContext as FieldsViewModel;
            }
        }

    }
}
