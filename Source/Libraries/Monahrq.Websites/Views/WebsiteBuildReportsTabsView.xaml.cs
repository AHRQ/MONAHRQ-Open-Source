using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Websites.ViewModels;
using Monahrq.Sdk.Common;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Attributes;
using NHibernate.Action;

namespace Monahrq.Websites.Views
{
    [ViewExport(typeof(WebsiteBuildReportsTabsView), RegionName = RegionNames.WebsiteManageRegion)]
    public partial class WebsiteBuildReportsTabsView : UserControl, INameScope
    {
        public WebsiteBuildReportsTabsView()
        {
            InitializeComponent();
            _reportsDataGrid.Loaded += _reportsDataGrid_Loaded;
        }

        void _reportsDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //_reportsDataGrid.ContextMenu = new ContextMenu();
            //
            //var editMenuItem = new MenuItem
            //	{
            //		Command = Model.NavigateToDetailsCommand,
            //		CommandParameter = Model.SelectedReport,
            //		Header = "Edit"
            //	};
            //
            //_reportsDataGrid.ContextMenu.Items.Add(editMenuItem);


            //var createNewMenuItem = new MenuItem
            //{
            //    Command = Model.CreateNewReportFromContextMenuCommand,
            //    CommandParameter = Model.SelectedReport,
            //    Header = "Create new report from this template"
            //};

            // _reportsDataGrid.ContextMenu.Items.Add(createNewMenuItem);


        }

        #region INameScope Members

        Dictionary<string, object> items = new Dictionary<string, object>();

        object INameScope.FindName(string name)
        {
            return items[name];
        }

        void INameScope.RegisterName(string name, object scopedElement)
        {
            items.Add(name, scopedElement);
        }

        void INameScope.UnregisterName(string name)
        {
            items.Remove(name);
        }

        #endregion

        #region Properties
        private int _lastCheckedItemIndex { get; set; }
        #endregion

        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public WebsiteReportsViewModel Model
        {
            get
            {
                return DataContext as WebsiteReportsViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        private void _reportsDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WaitCursor.Show();
            if (sender == null) return;
            var dgr = sender as DataGrid;

            if (dgr == null) return;

            var report = dgr.SelectedItem as ReportViewModel;
            if (report != null)
            {
                Model.NavigateToDetailsCommand.Execute(report);
            }
        }

        private void ReportsDataGrid_OnChecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null) return;
            var isChecked = checkBox.IsChecked.HasValue && checkBox.IsChecked != false;

            var currentReport = checkBox.Tag as ReportViewModel;
            if (currentReport == null) return;

            //var reportTypes = Model.SelectedReports.Select(rpt => rpt.Type).ToList();

            if (currentReport.ValidateBeforSelection(new WebsiteReportValidableSelectableStruct { Items = Model.SelectedReports.OfType<object>().ToList(), WebsiteAudiences = Model.CurrentWebsite.Audiences }))
                currentReport.IsSelected = isChecked;

            // currentReport.RaisePropertyChanged();

        }

        private void ReportsDataGrid_OnChecked(object sender, MouseButtonEventArgs e)
        {

        }

        private void SelectionReports_KeyUp(object sender, KeyEventArgs e)
        {
            CheckBox chkboxSelectionReports = sender as CheckBox;
            ReportViewModel newCheckedItem = chkboxSelectionReports.DataContext as ReportViewModel;

            int newCheckedItemIndex = Model.ReportsCollectionView.IndexOf(newCheckedItem);

            if ((System.Windows.Input.Keyboard.IsKeyUp(System.Windows.Input.Key.LeftShift) || System.Windows.Input.Keyboard.IsKeyUp(System.Windows.Input.Key.RightShift)) && newCheckedItemIndex != _lastCheckedItemIndex)
            {
                int start = Math.Min(_lastCheckedItemIndex, newCheckedItemIndex);
                int end = Math.Max(_lastCheckedItemIndex, newCheckedItemIndex);

                int countToTake = end - start;

                var itemsToCheck = new List<ReportViewModel>();
                foreach (var o in Model.ReportsCollectionView.OfType<ReportViewModel>().Select((x, i) => new { x, i }))
                {
                    if (o.i > start && o.i < end)
                        itemsToCheck.Add(o.x);
                }

                foreach (ReportViewModel itemToCheck in itemsToCheck)
                {
                    itemToCheck.IsSelected = true;
                }
                _lastCheckedItemIndex = -1;
            }
            else
            {
                _lastCheckedItemIndex = newCheckedItemIndex;
            }
        }

    }
}
