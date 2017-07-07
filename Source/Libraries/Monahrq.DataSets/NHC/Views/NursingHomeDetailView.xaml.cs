using System.ComponentModel.Composition;
using Monahrq.Sdk.Regions;
using Monahrq.DataSets.NHC.ViewModels;

namespace Monahrq.DataSets.NHC.Views
{
    /// <summary>
    /// Interaction logic for NursingHomeDetail.xaml
    /// </summary>
    //[ViewExport(typeof(NursingHomeDetailView), RegionName = ViewNames.NursingHomeDetail)]
    [Export(ViewNames.NursingHomeDetail)]
    public partial class NursingHomeDetailView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NursingHomeDetailView"/> class.
        /// </summary>
        public NursingHomeDetailView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        [Import]
        public NursingHomeDetailViewModel Model
        {
            get { return DataContext as NursingHomeDetailViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        /// Handles the IsKeyboardFocusedChanged event of the ComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ComboBox_IsKeyboardFocusedChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e) {

        }
    }
}
