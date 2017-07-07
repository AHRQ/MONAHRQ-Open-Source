using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;
using NHibernate;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System.Linq.Expressions;
using LinqKit;
using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.Modules.Settings;

namespace Monahrq.DataSets.HospitalRegionMapping.Hospitals
{
    /// <summary>
    /// The hospital filter helper extension method class.
    /// </summary>
    public static class HospitalFilterHelperExtensions
    {
        static readonly object _sync = new object();

        /// <summary>
        /// Builds the state criteria.
        /// </summary>
        /// <param name="payload">The payload.</param>
        private static void BuildStateCriteria(HospitalRegionElement payload)
        {
            lock (_sync)
            {
                var criteria = PredicateBuilder.False<State>();
                payload.DefaultStates.OfType<string>().ToList().ForEach(
                    state => criteria = criteria.Or(c => c.Abbreviation == state));
                StateCriteria = criteria;
            }
        }

        /// <summary>
        /// Gets or sets the lazy provider.
        /// </summary>
        /// <value>
        /// The lazy provider.
        /// </value>
        static Lazy<IDomainSessionFactoryProvider> LazyProvider { get; set; }
        /// <summary>
        /// Gets or sets the state criteria.
        /// </summary>
        /// <value>
        /// The state criteria.
        /// </value>
        static Expression<Func<State, bool>> StateCriteria { get; set; }

        /// <summary>
        /// Initializes the <see cref="HospitalFilterHelperExtensions"/> class.
        /// </summary>
        static HospitalFilterHelperExtensions()
        {
            LazyProvider = new Lazy<IDomainSessionFactoryProvider>(() => ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>(), true);

            ServiceLocator.Current.GetInstance<IEventAggregator>()
              .GetEvent<HospitalRegionSettingsSavedEvent>()
              .Subscribe(BuildStateCriteria);
        }

        /// <summary>
        /// Asserts the preliminaries.
        /// </summary>
        private static void AssertPreliminaries()
        {
            if (StateCriteria == null)
            {
                lock (_sync)
                {
                    if (StateCriteria == null)
                    {
                        var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
                        BuildStateCriteria(configService.HospitalRegion);
                    }
                }
            }
        }

        /// <summary>
        /// Opens the session.
        /// </summary>
        /// <returns></returns>
        static ISession OpenSession()
        {
            return LazyProvider.Value.SessionFactory.OpenSession();
        }

        /// <summary>
        /// Determines whether the specified name contains name.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the specified name contains name; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsName(this Hospital viewModel, string name)
        {
            return string.IsNullOrEmpty(name) ||
                   (
                       viewModel != null &&
                       viewModel.Name.ContainsCaseInsensitive(name)
                   );
        }

        /// <summary>
        /// Determines whether the specified category name has category.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="categoryName">Name of the category.</param>
        /// <returns>
        ///   <c>true</c> if the specified category name has category; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasCategory(this Hospital viewModel, string categoryName)
        {
            return viewModel.Categories.Any(c => c.Name.ToLower().Contains(categoryName.ToLower()));
        }

        /// <summary>
        /// Determines whether the specified custom region name has region.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="customRegionName">Name of the custom region.</param>
        /// <returns>
        ///   <c>true</c> if the specified custom region name has region; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasRegion(this Hospital viewModel, string customRegionName)
        {
            return string.IsNullOrEmpty(customRegionName) || (viewModel.SelectedRegion != null && viewModel.SelectedRegion.Name.ContainsCaseInsensitive(customRegionName));

        }

        /// <summary>
        /// Determines whether the specified state abbrev or name has state.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="stateAbbrevOrName">Name of the state abbrev or.</param>
        /// <returns>
        ///   <c>true</c> if the specified state abbrev or name has state; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasState(this Hospital viewModel, string stateAbbrevOrName)
        {
            return string.IsNullOrWhiteSpace(stateAbbrevOrName) ||
                       (
                            (viewModel != null && viewModel.State != null &&
                             viewModel.State.ContainsCaseInsensitive(stateAbbrevOrName)) ||
                            (viewModel != null && viewModel.State != null &&
                             viewModel.State.ContainsCaseInsensitive(stateAbbrevOrName))
                       );
        }


        /// <summary>
        /// Determines whether [has CMS provider identifier] [the specified CMS provider identifier].
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="cmsProviderID">The CMS provider identifier.</param>
        /// <returns>
        ///   <c>true</c> if [has CMS provider identifier] [the specified CMS provider identifier]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasCmsProviderID(this Hospital viewModel, string cmsProviderID)
        {
            return string.IsNullOrWhiteSpace(cmsProviderID) ||
                       (
                           viewModel != null && viewModel.CmsProviderID != null &&
                           viewModel.CmsProviderID.ContainsCaseInsensitive(cmsProviderID)
                       );
        }

        /// <summary>
        /// Hospitalses the selected.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="selectedHospitalType">Type of the selected hospital.</param>
        /// <returns></returns>
        public static bool HospitalsSelected(this Hospital viewModel, string selectedHospitalType)
        {
            switch (selectedHospitalType)
            {
                case "Base Hospitals":
                    return viewModel.IsSourcedFromBaseData;
                case "Custom Hospitals":
                    return !viewModel.IsSourcedFromBaseData;
                default:
                    return true;
            }
        }
    }
}
