
using System;
using System.ComponentModel.Composition;
using System.Windows.Data;
using Monahrq.DataSets.ViewModels;
using Monahrq.Sdk.Regions;

namespace Monahrq.DataSets.Views
{
    /// <summary>
    /// The main dataset view user control.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    [Export(ViewNames.MainDataSetView)]
    public partial class MainDataSetView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainDataSetView"/> class.
        /// </summary>
        public MainDataSetView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        [Import]
        public MainDataSetViewModel Model
        {
            get
            {
                return DataContext as MainDataSetViewModel;
            }
            set
            {
                DataContext = value;
            }
        }
    }

    /// <summary>
    /// The custom view header value converter.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class HeaderConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = (value as string);
            if (val == null) return string.Empty;
            val = val.Replace(" Data", "");
            var header = !val.Contains("Measures") ? string.Format("Import or delete data files. Edit {0} details and geographic data", val) : "Import or delete data files.";
            return header;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
