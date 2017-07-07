using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.DataSets.ViewModels;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;

namespace Monahrq.DataSets.Views
{
    /// <summary>
    /// Interaction logic for DefaultDatasetMetadataEditor.xaml
    /// </summary>
    //[ViewExport(typeof(DefaultDatasetMetadataView), RegionName = RegionNames.MainContent)]
    public partial class DefaultDatasetMetadataView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDatasetMetadataView"/> class.
        /// </summary>
        public DefaultDatasetMetadataView()
        {
            InitializeComponent();
        }
    }
}
