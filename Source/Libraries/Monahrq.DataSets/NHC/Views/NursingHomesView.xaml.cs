using System.Windows;
using Monahrq.DataSets.NHC.ViewModels;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using System.Diagnostics;

namespace Monahrq.DataSets.NHC.Views
{
    /// <summary>
    /// Interaction logic for NursingHomesView.xaml
    /// </summary>
    [ViewExport(typeof(NursingHomesView), RegionName = RegionNames.NursingHomes)]
    public partial class NursingHomesView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NursingHomesView"/> class.
        /// </summary>
        public NursingHomesView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public NursingHomesViewModel Model
        {
            get { return DataContext as NursingHomesViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        /// Handles the OnClick event of the Hyperlink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(@"https://data.medicare.gov/data/nursing-home-compare"));
        }
    }
}
