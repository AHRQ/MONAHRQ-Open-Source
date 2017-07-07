using System.ComponentModel.Composition;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Events;
using Monahrq.DataSets.ViewModels.Hospitals;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Regions;

namespace Monahrq.DataSets.Views.Hospitals
{
    /// <summary>
    /// Interaction logic for HospitalsDataTabView.xaml
    /// </summary>
    [Export(ViewNames.HospitalsDataTabView)]
    public partial class HospitalsDataTabView : UserControl
    {
        public HospitalsDataTabView()
        {
            InitializeComponent();
            Loaded += delegate
            {
                EventAggregator.GetEvent<ResumeNormalProcessingEvent>().Publish(Empty.Value);
            };
        }

        [Import]
        IEventAggregator EventAggregator { get; set; }

        [Import]
        public HospitalsTabDataViewModel Model
        {
            get
            {
                return DataContext as HospitalsTabDataViewModel;
            }
            set
            {
                DataContext = value;
            }
        }
    }
}
