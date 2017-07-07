using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Monahrq.DataSets.ViewModels.Crosswalk
{
    /// <summary>
    /// Interaction logic for FieldsViewCorrect.xaml
    /// </summary>
    public partial class FieldsViewCorrect : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldsViewCorrect"/> class.
        /// </summary>
        public FieldsViewCorrect()
        {
            InitializeComponent();
            Loaded += FieldsViewCorrect_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the FieldsViewCorrect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void FieldsViewCorrect_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (Model == null) return;
                var temp = Model.DataContextObject.CreateCrosswalkMappingFieldViewModels();
                if (temp != null)
                {
                    Model.MappedFieldModels = temp;
                }
                Model.ReconcileCrosswalkFromDataContext();
            }
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public FieldsViewModel Model
        {
            get
            {
                return DataContext as FieldsViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ACombo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ACombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var temp = sender as ComboBox;
        }
    }
}
