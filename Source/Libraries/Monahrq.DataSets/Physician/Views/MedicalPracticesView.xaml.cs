using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;

namespace Monahrq.DataSets.Physician.Views
{
    /// <summary>
    /// Interaction logic for MedicalPractice.xaml
    /// </summary>
    [ViewExport(typeof(MedicalPracticesView), RegionName = RegionNames.MedicalPracticesView)]
    public partial class MedicalPracticesView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MedicalPracticesView"/> class.
        /// </summary>
        public MedicalPracticesView()
        {
            InitializeComponent();
        }
    }
}
