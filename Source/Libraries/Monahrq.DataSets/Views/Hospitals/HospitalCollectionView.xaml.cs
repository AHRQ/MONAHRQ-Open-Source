using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.DataSets.ViewModels.Hospitals;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Regions.Core;

namespace Monahrq.DataSets.Views.Hospitals
{
    //[ExportAsView(ViewNames.HospitalCollectionView, Category = "DataSets", MenuName = "Hospitals", MenuItemName = "Hospitals")]
    //[ExportViewToRegion(ViewNames.HospitalCollectionView, RegionNames.HospitalCollectionRegion)]
    //[Export(typeof(HospitalCollectionView))]
    public partial class HospitalCollectionView : UserControl
    {
        public HospitalCollectionView()
        {
            InitializeComponent();
        }

        [Import]
        public HospitalCollectionViewModel Model
        {
            get
            {
                return DataContext as HospitalCollectionViewModel;
            }
            set
            {
                DataContext = value;
            }
        }
    }
}
