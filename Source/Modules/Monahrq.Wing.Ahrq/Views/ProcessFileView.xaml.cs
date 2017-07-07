using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Monahrq.Wing.Ahrq.Views
{
    /// <summary>
    /// Interaction logic for ProcessFileView.xaml
    /// </summary>
    [Export("ProcessFileView")]
    public partial class ProcessFileView : BaseAhrqProcessFileView
    {
      //  [ImportingConstructor]
        public ProcessFileView()
        {
            InitializeComponent();
        }
    }
}
