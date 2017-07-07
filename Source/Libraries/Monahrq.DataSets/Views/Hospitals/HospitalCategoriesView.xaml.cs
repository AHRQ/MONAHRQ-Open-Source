using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Regions.Core;
using Monahrq.DataSets.ViewModels.Hospitals;

namespace Monahrq.DataSets.Views.Hospitals
{
    [ExportAsView(ViewNames.HospitalCategoriesView, Category = "DataSets", MenuName = "Hospitals", MenuItemName = "Hospital Categories")]
    [ExportViewToRegion(ViewNames.HospitalCategoriesView, RegionNames.HospitalCategoriesRegion)]
    [Export(typeof(HospitalCategoriesView))]
    public partial class HospitalCategoriesView : UserControl
    {
        public HospitalCategoriesView()
        {
            InitializeComponent();
        }

        [Import]
        public HospitalCategoryCollectionViewModel Model
        {
            get
            {
                return DataContext as HospitalCategoryCollectionViewModel;
            }
            set
            {
                DataContext = value;
            }
        }
    }
}
