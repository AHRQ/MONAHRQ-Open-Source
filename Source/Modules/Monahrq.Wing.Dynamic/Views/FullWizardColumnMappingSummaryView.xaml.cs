using System.Windows;
using Monahrq.DataSets.ViewModels.MappingSummary;
using Monahrq.Wing.Dynamic.ViewModels;

namespace Monahrq.Wing.Dynamic.Views
{
    /// <summary>
    /// Interaction logic for DefaultWizardColumnMappingSummaryView.xaml
    /// </summary>
    public partial class FullWizardColumnMappingSummaryView
    {
        public FullWizardColumnMappingSummaryView()
        {
            InitializeComponent();
            Loaded += DefaultWizardColumnMappingSummaryView_Loaded;
        }

        void DefaultWizardColumnMappingSummaryView_Loaded(object sender, RoutedEventArgs e)
        {
            dynamic doc = theBrowser.Document;
            doc.Write(Model.DocumentContent);
        }

        FullWizrdReportViewModel Model
        {
            get
            {
                return DataContext as FullWizrdReportViewModel;
            }
        }
    }
}
