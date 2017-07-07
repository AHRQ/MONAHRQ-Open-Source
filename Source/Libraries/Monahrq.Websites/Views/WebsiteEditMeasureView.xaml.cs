using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using Monahrq.Websites.ViewModels;
using Monahrq.Infrastructure.Domain.Common;

namespace Monahrq.Websites.Views
{
    /// <summary>
    /// Interaction logic for ManageMeasuresView.xaml
    /// </summary>
    [ViewExport(typeof(WebsiteEditMeasureView), RegionName = RegionNames.WebsiteManageRegion)]
    public partial class WebsiteEditMeasureView
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="WebsiteEditMeasureView"/> class.
		/// </summary>
		/// <param name="model">The Model.SelectedMeasure.</param>
		public WebsiteEditMeasureView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the Model.SelectedMeasure.
        /// </summary>
        /// <value>
        /// The Model.SelectedMeasure.
        /// </value>
        [Import]
        public WebsiteEditMeasureViewModel Model
        {
            get
            {
                return DataContext as WebsiteEditMeasureViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        #region Control Events


        private void AlternateMeasureName_KeyDown(object sender, KeyEventArgs e)
        {
            RestrictTextLength(sender, e, "Alternate Measure Name", Model.SelectedMeasure.MeasureNameMaxLength);
        }

        // WPF doesn't send spacebar to keydown
        private void AlternateMeasureName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                RestrictTextLength(sender, e, "Alternate Measure Name", Model.SelectedMeasure.MeasureNameMaxLength);
            }

            e.Handled = false;
        }

        private void MoreInformation_KeyDown(object sender, KeyEventArgs e)
        {
            RestrictTextLength(sender, e, "More Information", Model.SelectedMeasure.MoreInformationMaxLength);
        }

        // WPF doesn't send spacebar to keydown
        private void MoreInformation_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                RestrictTextLength(sender, e, "More Information", Model.SelectedMeasure.MoreInformationMaxLength);
            }

            e.Handled = false;
        }

        private void Url_KeyDown(object sender, KeyEventArgs e)
        {
            RestrictTextLength(sender, e, "URL", Model.SelectedMeasure.UrlMaxLength);
        }

        // WPF doesn't send spacebar to keydown
        private void Url_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                RestrictTextLength(sender, e, "URL", Model.SelectedMeasure.UrlMaxLength);
            }

            e.Handled = false;
        }

        private void UrlTitle_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as WebsiteMeasuresViewModel;
            RestrictTextLength(sender, e, "URL Title", Model.SelectedMeasure.UrlTitleMaxLength);
        }

        // WPF doesn't send spacebar to keydown
        private void UrlTitle_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                RestrictTextLength(sender, e, "URL Title", Model.SelectedMeasure.UrlTitleMaxLength);
            }

            e.Handled = false;
        }

        private void Footnotes_KeyDown(object sender, KeyEventArgs e)
        {
            RestrictTextLength(sender, e, "Footnotes", Model.SelectedMeasure.FootnotesMaxLength);
        }

        // WPF doesn't send spacebar to keydown
        private void Footnotes_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                RestrictTextLength(sender, e, "Footnotes", Model.SelectedMeasure.FootnotesMaxLength);
            }

            e.Handled = false;
        }

        // Per the spec, show messagebox if length exceeds max. But in case the database has text too long, allow the user to only press Backspace or Delete
        private void RestrictTextLength(object sender, KeyEventArgs e, string fieldname, int maxlength)
        {
            var textbox = sender as TextBox;

            if (textbox != null && textbox.Text.Length >= maxlength && e.Key != Key.Back && e.Key != Key.Delete)
            {
                MessageBox.Show(string.Format("Please enter '{0}' using fewer than {1} characters.", fieldname, maxlength),
                    "Text length restriction",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        #endregion
    }
}
