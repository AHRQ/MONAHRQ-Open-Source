using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Monahrq.DataSets.Physician.ViewModels;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Types;
using NHibernate.Util;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.DataSets.Physician.Views
{
    /// <summary>
    /// Interaction logic for PhysicianDetailView.xaml
    /// </summary>
    [Export(ViewNames.PhysicianDetail)]
    public partial class PhysicianDetailView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicianDetailView"/> class.
        /// </summary>
        public PhysicianDetailView()
        {
            InitializeComponent();
            Loaded += PhysicianDetailView_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the PhysicianDetailView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void PhysicianDetailView_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        [Import]
        public PhysicianDetailViewModel Model
        {
            get { return DataContext as PhysicianDetailViewModel; }
            set { DataContext = value; }
        }

        #region ComboBoxHospitals Methods.
        /// <summary>
        /// Called when [ComboBox hospitals data context changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnComboBoxHospitalsDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var hospitalList = (DataContext as PhysicianDetailViewModel).HospitalAffiliationList;
			hospitalList.CollectionChanged += DisableComboBoxToggleControlWhenNoItems;
			DisableComboBoxToggleControlWhenNoItems(hospitalList, null);
		}
        /// <summary>
        /// Called when [ComboBox hospitals loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnComboBoxHospitalsLoaded(object sender, RoutedEventArgs e)
		{
			var hospitalList = (DataContext as PhysicianDetailViewModel).HospitalAffiliationList;
			DisableComboBoxToggleControlWhenNoItems(hospitalList, null);
		}
        /// <summary>
        /// Disables the ComboBox toggle control when no items.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void DisableComboBoxToggleControlWhenNoItems(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			var personList = sender as ObservableCollection<SelectListItem>;
			
			if (personList == null) return;
			if (personList.Count == 0)
			{
				var toggleButtons = ComboBoxHospitals.GetVisualChildren<ToggleButton>();
				var toggleButton = toggleButtons.FirstOrNull() as ToggleButton;
				if (toggleButton != null)	toggleButton.IsEnabled = false;
			}
			else
			{
				var toggleButtons = ComboBoxHospitals.GetVisualChildren<ToggleButton>();
				var toggleButton = toggleButtons.FirstOrNull() as ToggleButton;
				if (toggleButton != null) toggleButton.IsEnabled = true;
			}
		}
		#endregion

    }

    /// <summary>
    /// The error message visibility converter
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class ErrorMessageVisibilityConverter : IMultiValueConverter
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
            if (values == null) return Visibility.Visible;
            var isSearching = values[0] != null && ((bool)values[0]);
            if (!isSearching || (values[2] != null && string.IsNullOrEmpty(values[2].ToString()))) return Visibility.Collapsed;
            var items = (ICollectionView)values[1];
            var messageVisibility = values[1] != null && (items != null) && items.Any() ? Visibility.Collapsed : Visibility.Visible;
            return messageVisibility;
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
