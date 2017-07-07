using Monahrq.DataSets.ViewModels.MappingSummary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Monahrq.DataSets.Views
{
    /// <summary>
    /// Interaction logic for DefaultWizardColumnMappingSummaryView.xaml
    /// </summary>
    public partial class DefaultWizardColumnMappingSummaryView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWizardColumnMappingSummaryView"/> class.
        /// </summary>
        public DefaultWizardColumnMappingSummaryView()
        {
            InitializeComponent();
            Loaded += DefaultWizardColumnMappingSummaryView_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the DefaultWizardColumnMappingSummaryView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void DefaultWizardColumnMappingSummaryView_Loaded(object sender, RoutedEventArgs e)
        {
            dynamic doc = theBrowser.Document;
            doc.Write(Model.DocumentContent);
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        Report Model
        {
            get
            {
                return DataContext as Report;
            }
        }
    }
}
