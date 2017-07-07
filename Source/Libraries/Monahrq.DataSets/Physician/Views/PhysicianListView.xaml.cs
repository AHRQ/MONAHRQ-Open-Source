using System.ComponentModel.Composition;
using System.Windows.Data;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using Monahrq.DataSets.Physician.ViewModels;

namespace Monahrq.DataSets.Physician.Views
{
    /// <summary>
    /// Interaction logic for PhysicianListView.xaml
    /// </summary>
    [ViewExport(typeof(PhysicianListView), RegionName = RegionNames.PhysicianListView)]
    public partial class PhysicianListView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicianListView"/> class.
        /// </summary>
        public PhysicianListView()
        {
            InitializeComponent();
            Loaded += PhysicianListView_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the PhysicianListView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void PhysicianListView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Model == null || DataGridPhysician == null || Model.CurrentSelectedItem == null) return;
            DataGridPhysician.ScrollIntoView(Model.CurrentSelectedItem);
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        [Import]
        public PhysicianListViewModel Model
        {
            get { return DataContext as PhysicianListViewModel; }
            set { DataContext = value; }
        }
    }

    /// <summary>
    /// The Physician name converter. Concatentates the first and last name to make full name.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class PhysicianNameConverter : IMultiValueConverter
    {

        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding" /> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty" />.<see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding" />.<see cref="F:System.Windows.Data.Binding.DoNothing" /> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> or the default value.
        /// </returns>
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format("{0} {1}", values[0], values[1]);
        }

        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return new[] { value };
        }
    }
}
