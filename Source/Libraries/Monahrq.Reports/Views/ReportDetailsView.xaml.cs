using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility.Extensions;
using Monahrq.Reports.ViewModels;
using System.Windows.Controls;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using System.Windows.Media;
using NHibernate.Mapping;

namespace Monahrq.Reports.Views
{

   // [Export(Sdk.Regions.ViewNames.ReportDetailsView)]
    [ViewExport(typeof(ReportDetailsView), RegionName = RegionNames.MainContent)]
    public partial class ReportDetailsView : UserControl
    {
        public ReportDetailsView()
        {
            InitializeComponent();
        }

        [Import]
        public ReportDetailsViewModel Model
        {
            get
            {
                return DataContext as ReportDetailsViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var check = sender as CheckBox;
            if (check != null && check.Tag == null) return;
            var checkedFilterValueName = check.DataContext != null
                ? (check.DataContext as FilterValue) != null ? (check.DataContext as FilterValue).Name : string.Empty
                : string.Empty;

            var listView = GetParent(check, typeof(ListView));
            if (listView != null)
            {
                var filterValues = ((ListView)listView).Items.OfType<FilterValue>().Where(x => x.RadioGroupName == check.Tag.ToString() && x.Name != checkedFilterValueName);
                filterValues.ForEach(x => x.Value = false);
            }
        }

        private DependencyObject GetParent(UIElement element, Type parentType)
        {
            if (element == null || parentType == null) return null;

            DependencyObject parent = VisualTreeHelper.GetParent(element);

            while (parent != null && parent.GetType() != parentType)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }

        private void RadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var rdb = sender as RadioButton;
            var dataContext = rdb.DataContext as ComparisonKeyIconSet;
            if (dataContext != null && Model != null)
                Model.SelectedIconSet = dataContext;
        }
    }
}
