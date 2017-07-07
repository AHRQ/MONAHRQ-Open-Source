using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Events;
using PropertyChanged;

namespace Monahrq.DataSets.ViewModels
{
    
    [Export]
    [ImplementPropertyChanged]
    public class ExportMappingViewModel:INavigationAware
    {
        private  IRegionManager RegionMgr { get; set; }
        private IEventAggregator EventAggregator { get; set; }

        public ExportMappingViewModel()
        {
            RegionMgr = ServiceLocator.Current.GetInstance<IRegionManager>();
            EventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            EventAggregator.GetEvent<NavigateToMappingImportExport>().Subscribe(DisplayRecord);
        }

        private void DisplayRecord(DataTypeDetailsViewModel data)
        {
            Record = data;
        }


        public DataTypeDetailsViewModel Record { get; set; }
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var x = navigationContext.Parameters;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            var x = navigationContext.Parameters;
        }
    }
}
