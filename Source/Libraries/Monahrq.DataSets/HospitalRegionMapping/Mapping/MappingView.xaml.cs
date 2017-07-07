using System.ComponentModel.Composition;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Events;
using Monahrq.Sdk.Attributes;

namespace Monahrq.DataSets.HospitalRegionMapping.Mapping
{
    /// <summary>
    /// Interaction logic for HospitalsDataTabView.xaml
    /// </summary>
    [ViewExport(typeof(MappingView), RegionName = Monahrq.Sdk.Regions.RegionNames.HospitalsMainRegion)]
    public partial class MappingView : UserControl
    {

        bool WasLoaded { get; set; }
        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import]
        public IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingView"/> class.
        /// </summary>
        public MappingView()
        {
            InitializeComponent();
            Loaded += MappingView_Loaded;
        }

        void MappingView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public MappingViewModel Model
        {
            get
            {
                return DataContext as MappingViewModel;
            }
            set { DataContext = value; }
        }
    }
}
