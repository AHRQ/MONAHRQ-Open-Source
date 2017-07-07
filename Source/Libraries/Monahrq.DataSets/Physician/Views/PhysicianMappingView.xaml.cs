using System.ComponentModel.Composition;
using Monahrq.DataSets.Physician.ViewModels;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;

namespace Monahrq.DataSets.Physician.Views
{
    /// <summary>
    /// Interaction logic for PhysicianView.xaml
    /// </summary>
    [ViewExport(typeof(PhysicianMappingView), RegionName = RegionNames.PhysicianMappingView)]
    public partial class PhysicianMappingView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicianMappingView"/> class.
        /// </summary>
        public PhysicianMappingView()
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
        public PhysicianMappingViewModel Model
        {
            get { return DataContext as PhysicianMappingViewModel; }
            set { DataContext = value; }
        }
    }
}
