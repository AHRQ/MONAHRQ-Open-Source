using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Types;
using Monahrq.Websites.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;

namespace Monahrq.Websites.Views
{
    [ViewExport(typeof(WebsiteSettingsView), RegionName = RegionNames.WebsiteSettingsRegion)]
    public partial class WebsiteSettingsView : UserControl
    {
        private const string ThemeSelectionConfirmation = "Changing the color theme for your site will modify the Brand Colors, Accent Colors, and Consumer only color palette already selected in the Advanced Color Options subsection.  Would you like to continue?";

        public WebsiteSettingsView()
        {
            InitializeComponent();
            Loaded += WebsiteSettingsView_Loaded;
        }

        private void WebsiteSettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            ChkExpandContent.IsChecked = true;
            ChkExpandTheme.IsChecked = true;
            var args = new RoutedEventArgs(Button.ClickEvent);
            ChkExpandContent.RaiseEvent(args);
            ChkExpandTheme.RaiseEvent(args);
        }

        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public WebsiteSettingsViewModel Model
        {
            get
            {
                return DataContext as WebsiteSettingsViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        private void cmbSelectState_OnSelected(object sender, SelectionChangedEventArgs e)
        {
            var context = DataContext as WebsiteSettingsViewModel;
            if (context != null)
            {
                //Task.Run(() =>
                //{
                //    if (context.WebsiteViewModel.Website != null && !context.WebsiteViewModel.Website.IsPersisted)
                //    {
                //        context.PopulateNursingHomes();
                //        context.NursingHomeSelectionCompleteCommand.Execute("true");
                //    }
                //});
                context.EnableHospitalSectionLink = e.AddedItems.OfType<SelectListItem>()
                                                                .ToList().Any(item => item.IsSelected);
            }
            e.Handled = true;
        }

        private void OnContentExpandAllSelectionChanged(object sender, RoutedEventArgs args)
        {
            var chkBox = sender as CheckBox;
            var ischkd = chkBox != null && chkBox != null && chkBox.IsChecked.HasValue ? chkBox.IsChecked.Value : false;

            SelectHospitalExpander.IsExpanded = ischkd;
            GuideToolExpander.IsExpanded = ischkd;
            SelectZipcodeRadaiiGeoDescriptionExpander.IsExpanded = ischkd;
            AboutUsExpander.IsExpanded = ischkd;
            FeedBackExpander.IsExpanded = ischkd;
            SeoAnalyticsAndMappingExpander.IsExpanded = ischkd;
            OutPutFolderExpander.IsExpanded = ischkd;
        }

        private void OnThemeExpandAllSelectionChanged(object sender, RoutedEventArgs args)
        {
            var chkBox = sender as CheckBox;
            var ischkd = chkBox != null && chkBox != null && chkBox.IsChecked.HasValue ? chkBox.IsChecked.Value : false;

            SiteTheme.IsExpanded = ischkd;
            AdvancedColorOptions.IsExpanded = ischkd;
            LogoAndImages.IsExpanded = ischkd;
            Font.IsExpanded = ischkd;
        }

        private void SelectAllExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _cmbSelectRadius.SelectAll();
            Model.SetRadiusStateSelectionButtonContent(true);
        }
    }


    public class PreviewButtonConverter : IMultiValueConverter
    {

        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values != null && values.All(x => x != null && (bool)x);
        }

        public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return new[] { value };
        }
    }

    public class MenuGridVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var menus = value as ListCollectionView;

            if (menus == null || menus.Count == 0) return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class MenuGridWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            var containerWidth = double.Parse(value.ToString());

            return Math.Ceiling((containerWidth - 40) / 2.0);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
