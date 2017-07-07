using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Monahrq.DataSets.Views
{
    /// <summary>
    /// Interaction logic for DummyView.xaml
    /// </summary>
    [Export]
    public partial class DummyView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DummyView"/> class.
        /// </summary>
        public DummyView()
        {
            InitializeComponent();
        }
    }
}
