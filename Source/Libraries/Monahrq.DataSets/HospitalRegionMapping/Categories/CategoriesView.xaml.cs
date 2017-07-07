using System.Windows.Controls;
using System.Windows;
using Monahrq.Sdk.Attributes;

namespace Monahrq.DataSets.HospitalRegionMapping.Categories
{
    /// <summary>
    /// The hospital categories view user control.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    [ViewExport(typeof(CategoriesView), RegionName = Mapping.RegionNames.Categories)]
    public partial class CategoriesView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoriesView"/> class.
        /// </summary>
        public CategoriesView()
        {
            InitializeComponent();
            Loaded += CategoriesView_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the CategoriesView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void CategoriesView_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public CategoriesViewModel Model
        {
            get
            {
                return DataContext as CategoriesViewModel;
            }
        }
    }
}
