using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Data;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Measures.ViewModels;
using System.Windows;
using System.Windows.Input;
using Monahrq.Sdk.Regions;

namespace Monahrq.Measures.Views
{

    [Export(ViewNames.MeasureDetailsView)]
    public partial class MeasureDetailsView : UserControl
    {
        public MeasureDetailsView()
        {
            InitializeComponent();
            Loaded += delegate
            {
                tabControl.SelectedItem = NameAndDescriptionTab;
            };
        }

        [Import(RequiredCreationPolicy = CreationPolicy.NonShared)]
        MeasureDetailsViewModel Model
        {
            get
            {
                return DataContext as MeasureDetailsViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        #region Restrict Textbox MaxLength

        private void AlternateMeasureName_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as MeasureDetailsViewModel;
            if (vm == null)
                return;

            RestrictTextLength(sender, e, "Alternate Measure Name", vm.MeasureModel.MeasureNameMaxLength);
        }

        // WPF doesn't send spacebar to keydown
        private void AlternateMeasureName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var vm = DataContext as MeasureDetailsViewModel;
                if (vm == null)
                    return;
                RestrictTextLength(sender, e, "Alternate Measure Name", vm.MeasureModel.MeasureNameMaxLength);
            }

            e.Handled = false;
        }

        private void MoreInformation_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as MeasureDetailsViewModel;
            if (vm == null)
                return;
            RestrictTextLength(sender, e, "More Information", vm.MeasureModel.MoreInformationMaxLength);
        }

        // WPF doesn't send spacebar to keydown
        private void MoreInformation_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var vm = DataContext as MeasureDetailsViewModel;
                if (vm == null)
                    return;
                RestrictTextLength(sender, e, "More Information", vm.MeasureModel.MoreInformationMaxLength);
            }

            e.Handled = false;
        }

        private void Url_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as MeasureDetailsViewModel;
            RestrictTextLength(sender, e, "URL", vm.MeasureModel.UrlMaxLength);
        }

        // WPF doesn't send spacebar to keydown
        private void Url_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var vm = DataContext as MeasureDetailsViewModel;
                if (vm == null)
                    return;
                RestrictTextLength(sender, e, "URL", vm.MeasureModel.UrlMaxLength);
            }

            e.Handled = false;
        }

        private void UrlTitle_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as MeasureDetailsViewModel;
            if (vm == null)
                return;
            RestrictTextLength(sender, e, "URL Title", vm.MeasureModel.UrlTitleMaxLength);
        }

        // WPF doesn't send spacebar to keydown
        private void UrlTitle_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var vm = DataContext as MeasureDetailsViewModel;
                if (vm == null)
                    return;
                RestrictTextLength(sender, e, "URL Title", vm.MeasureModel.UrlTitleMaxLength);
            }

            e.Handled = false;
        }

        private void Footnotes_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as MeasureDetailsViewModel;
            if (vm == null)
                return;
            RestrictTextLength(sender, e, "Footnotes", vm.MeasureModel.FootnotesMaxLength);
        }

        // WPF doesn't send spacebar to keydown
        private void Footnotes_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var vm = DataContext as MeasureDetailsViewModel;
                if (vm == null)
                    return;
                RestrictTextLength(sender, e, "Footnotes", vm.MeasureModel.FootnotesMaxLength);
            }

            e.Handled = false;
        }

        // Per the spec, show messagebox if length exceeds max. But in case the database has text too long, allow the user to only press Backspace or Delete
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
    public class MeasureTopicSelectabilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var topic = value as TopicViewModel;

            if (topic == null || topic.TopicCategory.TopicType == TopicTypeEnum.NursingHome) return false;

            return true;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}