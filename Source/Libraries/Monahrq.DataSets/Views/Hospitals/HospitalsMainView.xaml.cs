using System.ComponentModel.Composition;
using System.Windows.Controls;
using Monahrq.DataSets.ViewModels.Hospitals;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Regions.Core;

namespace Monahrq.DataSets.Views.Hospitals
{
    //[ExportAsView(ViewNames.HospitalsMainView, Category = "DataSets", MenuName = "Hospitals")]
    //[ExportViewToRegion(ViewNames.HospitalsMainView, RegionNames.HospitalsMainRegion)]
    //[Export(typeof(HospitalsMainView))]
    public partial class HospitalsMainView : UserControl
    {
        public HospitalsMainView()
        {
            InitializeComponent();
        }

        [Import]
        public HospitalMainViewModel Model
        {
            get
            {
                return DataContext as HospitalMainViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

       
    }
}


 

        