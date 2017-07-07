using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Domain.Categories;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Hospitals.Mapping;
using Monahrq.Infrastructure.Types;
using NHibernate;

namespace Monahrq.Infrastructure.Services.Hospitals
{
    /// <summary>
    /// The hospital registry service domain service.
    /// </summary>
    public interface IHospitalRegistryService
    {
        /// <summary>
        /// Gets the hospital count for category.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="regionType">Type of the region.</param>
        /// <param name="stateIds">The state ids.</param>
        /// <returns></returns>
        int GetHospitalCountForCategory(int categoryId, string regionType, string[] stateIds);
        /// <summary>
        /// Gets the hospital count for region.
        /// </summary>
        /// <param name="regionType">Type of the region.</param>
        /// <param name="regionId">The region identifier.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        int GetHospitalCountForRegion(string regionType, int regionId, string state);
        /// <summary>
        /// Refreshes the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        void Refresh<TEntity>(TEntity entity)
            where TEntity : IEntity<int>;

        /// <summary>
        /// Generates the mapping reference.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="abbrevList">The abbrev list.</param>
        /// <param name="regionType">Type of the region.</param>
        /// <returns></returns>
        HospitalMappingReference GenerateMappingReference<T>(IEnumerable<string> abbrevList, Type regionType);

        /// <summary>
        /// Generates the mapping reference.
        /// </summary>
        /// <param name="statesToReference">The states to reference.</param>
        /// <param name="regionType">Type of the region.</param>
        /// <returns></returns>
        HospitalMappingReference GenerateMappingReference(IEnumerable<State> statesToReference, Type regionType);

        /// <summary>
        /// Hospital Registry representing th latest registry available to th system
        /// </summary>
        HospitalRegistry CurrentRegistry { get; }

        /// <summary>
        /// Create a transient region and returns to client for modification
        /// </summary>
        CustomRegion CreateRegion();

        /// <summary>
        /// Saves the given current region
        /// </summary>
        void Save(CustomRegion region);

        /// <summary>
        /// Saves the specified region population strats.
        /// </summary>
        /// <param name="regionPopulationStrats">The region population strats.</param>
        void Save(RegionPopulationStrats regionPopulationStrats);

        /// <summary>
        /// Create a transient hospital and returns to client for modification
        /// </summary>
        Hospital CreateHospital();


        /// <summary>
        /// Creates the hospital archive.
        /// </summary>
        /// <param name="archivedhospital">The archivedhospital.</param>
        /// <returns></returns>
        Hospital CreateHospitalArchive(Hospital archivedhospital);

        /// <summary>
        /// Saves the given current region
        /// </summary>
        void Save(Hospital hospital);

        /// <summary>
        /// Saves the asynchronous.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <param name="completeCallback">The complete callback.</param>
        Task<bool> SaveAsync(Hospital hospital, Action<OperationResult<Hospital>> completeCallback);

        /// <summary>
        /// Saves the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        void Save(Category category);
        /// <summary>
        /// Gets the states.
        /// </summary>
        /// <param name="abbrevs">The abbrevs.</param>
        /// <param name="returnAllStatesIfNoAbbrevs">if set to <c>true</c> [return all states if no abbrevs].</param>
        /// <returns></returns>
        IEnumerable<State> GetStates(string[] abbrevs, bool returnAllStatesIfNoAbbrevs = true);

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        TEntity Get<TEntity>(int id)
            where TEntity : IEntity<int>;

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="aggregate">The aggregate.</param>
        /// <returns></returns>
        T Get<TEntity, T>(int id, Expression<Func<TEntity, T>> aggregate)
            where TEntity : IEntity<int>;

        /// <summary>
        /// Gets the specified query spec.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="querySpec">The query spec.</param>
        /// <returns></returns>
        TEntity Get<TEntity>(Expression<Func<TEntity, bool>> querySpec)
            where TEntity : class, IEntity<int>;

        /// <summary>
        /// Get2s the specified query spec.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="querySpec">The query spec.</param>
        /// <returns></returns>
        TEntity Get2<TEntity>(Expression<Func<TEntity, bool>> querySpec)
            where TEntity : class, IEntity<int>;

        /// <summary>
        /// Deletes the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        void Delete(Category category);
        /// <summary>
        /// Deletes the specified region.
        /// </summary>
        /// <param name="region">The region.</param>
        void Delete(Region region);

        /// <summary>
        /// Deletes all.
        /// </summary>
        /// <param name="hospitals">The hospitals.</param>
        void DeleteAll(IEnumerable<Hospital> hospitals);
        /// <summary>
        /// Deletes the specified hospital.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        void Delete(Hospital hospital);
        /// <summary>
        /// Saves the specified selected categories.
        /// </summary>
        /// <param name="selectedCategories">The selected categories.</param>
        /// <param name="selectedHospitals">The selected hospitals.</param>
        void Save(IEnumerable<HospitalCategory> selectedCategories, IEnumerable<Hospital> selectedHospitals);

        /// <summary>
        /// Creates the hospital category.
        /// </summary>
        /// <param name="newCategoryName">New name of the category.</param>
        /// <returns></returns>
        HospitalCategory CreateHospitalCategory(string newCategoryName);

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <returns></returns>
        State GetState(Region region);

        /// <summary>
        /// Gets the states abbreviations.
        /// </summary>
        /// <param name="abbrevs">The abbrevs.</param>
        /// <returns></returns>
        IEnumerable<string> GetStatesAbbreviations(params string[] abbrevs);

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <returns></returns>
        IEnumerable<Category> GetCategories(Hospital hospital);
        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        IEnumerable<HospitalCategoryDto> GetCategories(Expression<Func<HospitalCategory, bool>> criteria = null);

        /// <summary>
        /// Gets the counties.
        /// </summary>
        /// <returns></returns>
        IEnumerable<County> GetCounties(params string[] states);

        /// <summary>
        /// Gets the state of the county by fips and.
        /// </summary>
        /// <param name="fipsCounty">The fips county.</param>
        /// <param name="stateAbrevation">The state abrevation.</param>
        /// <returns></returns>
        County GetCountyByFIPSAndState(string fipsCounty, string stateAbrevation);

        /// <summary>
        /// Gets the category hospital counts.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns></returns>
        IEnumerable<HospitalCategoryDto> GetCategoryHospitalCounts(IEnumerable<HospitalCategory> categories);

        /// <summary>
        /// Applies the categories to hospitals.
        /// </summary>
        /// <param name="selectedCategories">The selected categories.</param>
        /// <param name="selectedHospitals">The selected hospitals.</param>
        void ApplyCategoriesToHospitals(IEnumerable<Category> selectedCategories, IEnumerable<Hospital> selectedHospitals);

        /// <summary>
        /// Gets the state by zip code.
        /// </summary>
        /// <param name="zipCode">The zip code.</param>
        /// <returns></returns>
        State GetStateByZipCode(string zipCode);

        /// <summary>
        /// Gets the regions by zip code.
        /// </summary>
        /// <param name="zipCode">The zip code.</param>
        /// <returns></returns>
        IDictionary<string, Region> GetRegionsByZipCode(string zipCode);

        /// <summary>
        /// Gets the hospital's latest available cost to charge ratio.
        /// </summary>
        /// <param name="providerId">providerId</param>
        /// <returns></returns>
        decimal? GetLatestCostToChargeRatio(string providerId);

        /// <summary>
        /// Gets the custom region to population mapping count.
        /// </summary>
        /// <returns></returns>
        int GetCustomRegionToPopulationMappingCount(IEnumerable<string> states);

        /// <summary>
        /// Gets the regions.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="states">The states.</param>
        /// <param name="selectedRegionType">Type of the selected region.</param>
        /// <param name="includeCounts">if set to <c>true</c> [include counts].</param>
        /// <returns></returns>
        IList<Region> GetRegions(ISession session, IEnumerable<string> states, Type selectedRegionType, bool includeCounts = true);

        /// <summary>
        /// Rollbacks the custom hospital to base hospital.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <returns></returns>
        Hospital RollbackCustomHospitalToBaseHospital(Hospital hospital);
    }

    /// <summary>
    /// The hospital category data transfer object (DTO).
    /// </summary>
    public class HospitalCategoryDto
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public HospitalCategory Category { get; set; }
        /// <summary>
        /// Gets the hospital count.
        /// </summary>
        /// <value>
        /// The hospital count.
        /// </value>
        public int HospitalCount { get { return Category.OwnerCount; } }
    }
}
