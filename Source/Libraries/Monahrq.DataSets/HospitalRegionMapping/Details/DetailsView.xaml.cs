using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Monahrq.Sdk.Regions;

namespace Monahrq.DataSets.HospitalRegionMapping.Hospitals.Details
{
    /// <summary>
    /// The hospitals details view model.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    [Export(ViewNames.DetailsView)]
    public partial class DetailsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DetailsView"/> class.
        /// </summary>
        public DetailsView()
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
        public DetailsViewModel Model
        {
            get
            {
                return DataContext as DetailsViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        #region Restrict Textbox MaxLength

        /// <summary>
        /// Handles the KeyDown event of the Name control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void Name_KeyDown(object sender, KeyEventArgs e)
        {
            NameMaxLength(sender, e);
        }
        // WPF doesn't send spacebar to keydown
        /// <summary>
        /// Handles the PreviewKeyDown event of the Name control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void Name_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) NameMaxLength(sender, e);

            e.Handled = false;
        }
        /// <summary>
        /// Names the maximum length.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void NameMaxLength(object sender, KeyEventArgs e)
        {
            var vm = DataContext as DetailsViewModel;
            RestrictTextLength(sender, e, "Name", vm.NameMaxLength);
        }

        /// <summary>
        /// Handles the KeyDown event of the Address control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void Address_KeyDown(object sender, KeyEventArgs e)
        {
            AddressMaxLength(sender, e);
        }
        /// <summary>
        /// Handles the PreviewKeyDown event of the Address control. WPF doesn't send spacebar to keydown
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void Address_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) AddressMaxLength(sender, e);

            e.Handled = false;
        }
        /// <summary>
        /// Addresses the maximum length.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void AddressMaxLength(object sender, KeyEventArgs e)
        {
            var vm = DataContext as DetailsViewModel;
            RestrictTextLength(sender, e, "Address", vm.AddressMaxLength);
        }

        /// <summary>
        /// Handles the PreviewKeyDown event of the Description control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void Description_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) DescriptionMaxLength(sender, e);

            e.Handled = false;
        }
        // WPF doesn't send spacebar to keydown
        /// <summary>
        /// Handles the KeyDown event of the Description control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void Description_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as DetailsViewModel;
            RestrictTextLength(sender, e, "Description", vm.DescriptionMaxLength);
        }
        /// <summary>
        /// Descriptions the maximum length.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void DescriptionMaxLength(object sender, KeyEventArgs e)
        {
            var vm = DataContext as DetailsViewModel;
            RestrictTextLength(sender, e, "Description", vm.DescriptionMaxLength);
        }

        // Per the spec, show messagebox if length exceeds max. But in case the database has text too long, allow the user to only press Backspace or Delete
        /// <summary>
        /// Restricts the length of the text.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        /// <param name="fieldname">The fieldname.</param>
        /// <param name="maxlength">The maxlength.</param>
        private void RestrictTextLength(object sender, KeyEventArgs e, string fieldname, int maxlength)
        {
            var textbox = sender as TextBox;

            if (textbox.Text.Length >= maxlength && e.Key != Key.Back && e.Key != Key.Delete)
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
