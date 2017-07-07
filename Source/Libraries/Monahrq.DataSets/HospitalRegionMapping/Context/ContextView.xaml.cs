using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;

namespace Monahrq.DataSets.HospitalRegionMapping.Context
{
    /// <summary>
    /// Interaction logic for SelectStatesView.xaml
    /// </summary>
    [ViewExport(typeof(ContextView), RegionName = ViewNames.ContextView)]
    public partial class ContextView : UserControl
    {
        bool WasLoaded { get; set; }
        public ContextView()
        {
            InitializeComponent();

            //DataContextChanged += delegate
            //{
            //};

            //Loaded += delegate
            //{
            //    Model.Refresh();
            //};
        }

        [Import]
        public ContextViewModel Model
        {
            get
            {
                return DataContext as ContextViewModel;
            }

            set
            {
                DataContext = value;
            }
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(@"http://www.dartmouthatlas.org/data/region/"));
        }
    }
}
