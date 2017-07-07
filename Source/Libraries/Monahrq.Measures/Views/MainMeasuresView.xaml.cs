using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.Measures.ViewModels;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.ViewModels;

namespace Monahrq.Measures.Views
{

    [ViewExport(typeof(MainMeasuresView), RegionName = RegionNames.MainContent)]
    public partial class MainMeasuresView : TabOwnerUserControl
    {
        public MainMeasuresView()
        {
            InitializeComponent();
            //Loaded += delegate
            //{
            //    Model.InitTabs();
            //    tabControl.SelectedItem = MeasuresTab;
            //};
        }

        [Import]
        MainMeasuresViewModel Model
        {
            get
            {
                return DataContext as MainMeasuresViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        public override void OnUpdateTabIndex(TabIndexSelecteor obj)
        {
            // if (obj < 0) obj = 0;
            if (obj == null || string.IsNullOrEmpty(obj.TabName) || obj.TabName != MeasureTabs.Name) return;

            if (MeasureTabs.SelectedIndex == -1 && obj.TabIndex == 0)
            {
                MeasureTabs.SelectedIndex = -1;
            }
            MeasureTabs.SelectedIndex = obj.TabIndex;
        }
    }
}
