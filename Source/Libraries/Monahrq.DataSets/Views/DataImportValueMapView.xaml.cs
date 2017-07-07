using Monahrq.DataSets.ViewModels;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    /// Interaction logic for DataImportValueMapView.xaml
    /// </summary>
    [Export("DataImportValueMapView")]
    public partial class DataImportValueMapView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataImportValueMapView" /> class.
        /// </summary>
        /// <param name="dataImportValueMapViewModel">The data import value map view model.</param>
        [ImportingConstructor]
        public DataImportValueMapView(DataImportValueMapViewModel dataImportValueMapViewModel)
        {
            InitializeComponent();

            if (dataImportValueMapViewModel != null)
                this.DataContext = dataImportValueMapViewModel;
        }
    }
}
