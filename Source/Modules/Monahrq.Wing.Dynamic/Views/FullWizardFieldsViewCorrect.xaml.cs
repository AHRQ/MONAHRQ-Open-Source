using System.ComponentModel;
using System.Windows;
using Monahrq.Wing.Dynamic.ViewModels;

namespace Monahrq.Wing.Dynamic.Views
{
    /// <summary>
    /// Interaction logic for FieldsViewCorrect.xaml
    /// </summary>
    public partial class FullWizardFieldsViewCorrect
    {
        public FullWizardFieldsViewCorrect()
        {
            InitializeComponent();
            Loaded += FieldsViewCorrect_Loaded;
        }

        void FieldsViewCorrect_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            if (Model == null) return;

            var temp = Model.DataContextObject.CreateCrosswalkMappingFieldViewModels();
            if (temp != null)
            {
                Model.MappedFieldModels = temp;
            }

            Model.ReconcileCrosswalkFromDataContext();
        }

        public FullWizardFieldsViewModel Model
        {
            get
            {
                return DataContext as FullWizardFieldsViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        //private void ACombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    var temp = sender as ComboBox;
        //}
    }
}
