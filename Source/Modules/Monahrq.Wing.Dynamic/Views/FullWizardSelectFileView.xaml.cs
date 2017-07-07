using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Wing.Dynamic.ViewModels;

namespace Monahrq.Wing.Dynamic.Views
{
    /// <summary>
    /// Interaction logic for FullWizardSelectFileView.xaml
    /// </summary>
    [Export]
    public partial class FullWizardSelectFileView
    {

        public IRegionManager Manager
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IRegionManager>();
            }
        }


        FullWizardSelectFileViewModel Model
        {
            get { return DataContext as FullWizardSelectFileViewModel; }
        }

        public FullWizardSelectFileView()
        {
            InitializeComponent();
            Loaded += delegate
            {
                if (Model != null)
                    Model.DataContextObject.Histogram = null;
            };
        }
    }
}
