using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Monahrq.DataSets.ViewModels.Validation
{
    /// <summary>
    /// Interaction logic for ValidationResultView.xaml
    /// </summary>
    public partial class ValidationResultView : UserControl
    {
        public ValidationResultView()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new ValidationResultsSummary(100, 1000, null);
            }
        }
    }
}
