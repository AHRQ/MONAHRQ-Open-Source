using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Events;
using Monahrq.DataSets.Resources;
using PropertyChanged;

namespace Monahrq.DataSets.ViewModels
{
    /// <summary>
    /// the dataset import wizard mapping reimport viewmodel.
    /// </summary>
    [Export]
    [ImplementPropertyChanged]
    public class MappingReimportViewModel
    {
        /// <summary>
        /// Gets or sets the region MGR.
        /// </summary>
        /// <value>
        /// The region MGR.
        /// </value>
        private IRegionManager RegionMgr { get; set; }
        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        private IEventAggregator EventAggregator { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingReimportViewModel"/> class.
        /// </summary>
        public MappingReimportViewModel ()
        {
            RegionMgr = ServiceLocator.Current.GetInstance<IRegionManager>();
            EventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>(); 
        }

        /// <summary>
        /// Displays the record.
        /// </summary>
        /// <param name="data">The data.</param>
        private void DisplayRecord(DataTypeDetailsViewModel data)
        {
            Record = data;
        }

        /// <summary>
        /// Gets or sets the record.
        /// </summary>
        /// <value>
        /// The record.
        /// </value>
        public DataTypeDetailsViewModel Record { get; set; }

        /// <summary>
        /// Called when [navigated to].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
           
            var hash = int.Parse(navigationContext.Parameters["hash"]);
            Record  = (DataTypeDetailsViewModel)Parameters.request(hash);
        }

        /// <summary>
        /// Determines whether [is navigation target] [the specified navigation context].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        ///   <c>true</c> if [is navigation target] [the specified navigation context]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// Called when [navigated from].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //tbd
        }
    }
}
