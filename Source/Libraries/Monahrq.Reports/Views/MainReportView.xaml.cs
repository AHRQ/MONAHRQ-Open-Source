using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Reports.ViewModels;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Common;
using Monahrq.Sdk.Regions;

namespace Monahrq.Reports.Views
{

    [ViewExport(typeof(MainReportView), RegionName = RegionNames.MainContent)]
    public partial class MainReportView : INameScope
    {
        public MainReportView()
        {
            InitializeComponent();
        }

        #region INameScope Members

        readonly HybridDictionary _items = new HybridDictionary();

        object INameScope.FindName(string name)
        {
            return _items[name];
        }

        void INameScope.RegisterName(string name, object scopedElement)
        {
            _items.Add(name, scopedElement);
        }

        void INameScope.UnregisterName(string name)
        {
            _items.Remove(name);
        }

        #endregion

        [Import]
        public MainReportsViewModel Model
        {
            get
            {
                return DataContext as MainReportsViewModel;
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

            var report = dgr.SelectedItem as Report;
            if (report != null)
            {
                Model.NavigateToDetailsCommand.Execute(report);
            }
        }


    }
}
