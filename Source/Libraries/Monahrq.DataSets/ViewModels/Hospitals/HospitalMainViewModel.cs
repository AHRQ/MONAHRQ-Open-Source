using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;
using Monahrq.DataSets.Services;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain.Hospitals.Mapping;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Services.Import;

namespace Monahrq.DataSets.ViewModels.Hospitals
{
    /// <summary>
    /// The Main Hospital View Model
    /// </summary>
    /// <seealso cref="Monahrq.Default.ViewModels.BaseViewModel" />
    /// <seealso cref="Microsoft.Practices.Prism.Regions.INavigationAware" />
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    [Export(typeof(HospitalMainViewModel))]
    public class HospitalMainViewModel : BaseViewModel, INavigationAware, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Gets or sets the region mapping reference.
        /// </summary>
        /// <value>
        /// The region mapping reference.
        /// </value>
        [Import(DataContracts.MAPPING_REFERENCE, AllowRecomposition = true)]
        public HospitalMappingReference RegionMappingReference { get; set; }
        /// <summary>
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        public IRegionManager RegionManager { get; set; }
        /// <summary>
        /// Gets or sets the hospital data service.
        /// </summary>
        /// <value>
        /// The hospital data service.
        /// </value>
        public IHospitalDataService HospitalDataService { get; set; }

        /// <summary>
        /// Gets or sets the region importer.
        /// </summary>
        /// <value>
        /// The region importer.
        /// </value>
        private IEntityFileImporter RegionImporter { get; set; }
        /// <summary>
        /// Gets or sets the hospital importer.
        /// </summary>
        /// <value>
        /// The hospital importer.
        /// </value>
        private IEntityFileImporter HospitalImporter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalMainViewModel"/> class.
        /// </summary>
        /// <param name="regionManager">The region manager.</param>
        /// <param name="hospitalDataService">The hospital data service.</param>
        [ImportingConstructor]
        public HospitalMainViewModel(
              IRegionManager regionManager, IHospitalDataService hospitalDataService)
        {
            
            RegionManager = regionManager;
            HospitalDataService = hospitalDataService;
        }

        /// <summary>
        /// The isok
        /// </summary>
        public bool isok;

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <exception cref="NullReferenceException">Hospital Data Service failed to initiliaze Mapping context</exception>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            try
            {
                var x = RegionManager.Regions[RegionNames.HospitalsMainRegion];

                var view = x.GetView(ViewNames.HospitalsDataTabView);
                var wizard = x.GetView(ViewNames.GeoContextWizard);
            }
            catch
            {}

            try
            {
                if (HospitalDataService != null)
                {
                    isok = !HospitalDataService.IsMappingContextAvailable;

                    if (isok)
                    {
                        RegionManager.RequestNavigate(RegionNames.HospitalsMainRegion, new Uri(ViewNames.HospitalsDataTabView, UriKind.Relative));
                        isok = !isok;
                    }
                    else
                    {
                        //ServiceLocator.Current.GetInstance<IEventAggregator>()
                        //      .GetEvent<WizardNavigateSelectStatesEvent>().Publish(new Uri(ViewNames.GeoContextWizard, UriKind.Relative));
                    }
                }
                else
                {
                    throw new NullReferenceException("Hospital Data Service failed to initiliaze Mapping context");
                }
            }
            catch (Exception)
            {}
        }

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        /// <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            var x = navigationContext;
        }

        /// <summary>
        /// Gets the type of the data region.
        /// </summary>
        /// <value>
        /// The type of the data region.
        /// </value>
        public string DataRegionType
        {
            get
            {
                if (RegionMappingReference.RegionType == typeof(HospitalServiceArea))
                {
                    return "HSA";
                }
                return RegionMappingReference.RegionType == typeof(HealthReferralRegion) ? "HRR" : "N/A";
            }
        }

        /// <summary>
        /// Handles the regionMappingContextPropertyChanged event of the  control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void _regionMappingContextPropertyChanged(object sender,
                                                          System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged();

        }
    }
}

