using System.ComponentModel.Composition;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.ViewModels;
using Monahrq.Default.DataProvider.Administration.File;

namespace Monahrq.DataSets.Views
{
    /// <summary>
    /// Interaction logic for DefaultWizardSelectFileView.xaml
    /// </summary>
    [Export]
    public partial class DefaultWizardSelectFileView : UserControl
    {
        /// <summary>
        /// Gets the manager.
        /// </summary>
        /// <value>
        /// The manager.
        /// </value>
        public IRegionManager Manager
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IRegionManager>();
            }
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        DefaultWizardSelectFileViewModel Model
        {
            get { return DataContext as DefaultWizardSelectFileViewModel; }
        }

        /// <summary>
        /// Gets or sets the datasource view.
        /// </summary>
        /// <value>
        /// The datasource view.
        /// </value>
        IFileDatasourceView DatasourceView 
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWizardSelectFileView"/> class.
        /// </summary>
        public DefaultWizardSelectFileView()
        {
            InitializeComponent();
            Loaded += delegate
            {
                Model. DataContextObject.Histogram = null;
            };
        }
    }
}
