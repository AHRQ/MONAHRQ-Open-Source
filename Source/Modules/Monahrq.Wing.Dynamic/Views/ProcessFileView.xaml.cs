using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace Monahrq.Wing.Dynamic.Views
{
    /// <summary>
    /// Interaction logic for ProcessFileView.xaml
    /// </summary>
    [Export("ProcessFileView")]
    public partial class ProcessFileView : UserControl
    {

        public ProcessFileView()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            Loaded += delegate
            {
                ImportData();
            };
        }

        [Import]
        IDataImporter Model
        {
            // return the parent window DataContext as the Model for this class
            get { return DataContext as IDataImporter; }
        }

        public void ImportData()
        {
            Model.StartImport();
        }
    }
}
