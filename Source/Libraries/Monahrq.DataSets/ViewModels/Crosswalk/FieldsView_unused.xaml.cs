using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Monahrq.DataSets.ViewModels.Crosswalk
{
    /// <summary>
    /// Interaction logic for CrosswalkMappingView.xaml
    /// </summary>
    public partial class FieldsView_unused : UserControl, Monahrq.DataSets.ViewModels.Crosswalk.IFieldsView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldsView_unused"/> class.
        /// </summary>
        public FieldsView_unused()
        {
            InitializeComponent();
            Loaded += FieldsView_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the FieldsView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void FieldsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (Model == null) return;
                var temp =  Model.DataContextObject.CreateCrosswalkMappingFieldViewModels();
                if(temp != null)
                {
                    Model.MappedFieldModels = temp;
                }
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
        /// Handles the TextChanged event of the TextBox control. Left-side filter
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Model.ElementFilterChangedCommand.CanExecute(null))
            {
                var tb = sender as TextBox;
                if (tb == null) return;

                Model.CurrentElementFilter = tb.Text;

                Debug.WriteLine(string.Format("ElementFilter: {0}", tb.Text));
                Model.ElementFilterChangedCommand.Execute(tb.Text);
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the Filter control.  right-side filter.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Model.FieldEntryFilterChangedCommand.CanExecute(null))
            {
                Model.FieldEntryFilterChangedCommand.Execute(null);
            }
        }
    }
}
