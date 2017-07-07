using System.ComponentModel.Composition;
using Monahrq.DataSets.Physician.ViewModels;
using Monahrq.Sdk.Regions;

namespace Monahrq.DataSets.Physician.Views
{
    /// <summary>
    /// Interaction logic for MedicalPracticeEditView.xaml
    /// </summary>
    [Export(ViewNames.MedicalPracticeEditView)]
    public partial class MedicalPracticeEditView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MedicalPracticeEditView"/> class.
        /// </summary>
        public MedicalPracticeEditView()
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
        public MedicalPracticeEditViewModel Model
        {
            get { return DataContext as MedicalPracticeEditViewModel; }
            set { DataContext = value; }
        }
    }
}
