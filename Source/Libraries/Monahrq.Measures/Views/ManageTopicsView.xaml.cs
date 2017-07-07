using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Data;
using Monahrq.Measures.ViewModels;
using System.Windows.Controls;
using Monahrq.Sdk.Attributes;
using NHibernate.Mapping;

namespace Monahrq.Measures.Views
{
    /// <summary>
    /// Interaction logic for ManageTopicsView.xaml
    [ViewExport(typeof(ManageTopicsView), RegionName = "MeasuresManageRegion")]
    public partial class ManageTopicsView : UserControl
    {
        public ManageTopicsView()
        {
            InitializeComponent();
        }

        [Import]
        public ManageTopicsViewModel Model
        {
            get
            {
                return DataContext as ManageTopicsViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

    }
}
