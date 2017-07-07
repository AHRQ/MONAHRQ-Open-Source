using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Types;
using Monahrq.Sdk.Types;
using Monahrq.Websites.Model;
using Monahrq.Websites.ViewModels;
using Monahrq.Infrastructure.Entities.Domain.WebsiteContentManagement;
using BDWebsitePage = Monahrq.Infrastructure.Entities.Domain.BaseData.BaseWebsitePage;
using BDWebsitePageZone = Monahrq.Infrastructure.Entities.Domain.BaseData.BaseWebsitePageZone;

namespace Monahrq.Websites.Services
{
    public interface IWebsiteDataService
    {
        bool IsDatasetHasCountyData(string query);
        T ExecuteScalar<T>(string query);
        /// <summary>
        /// Gets the dataset summary.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        IEnumerable<DatasetSummary> GetDatasetSummary(int? id = null);
        /// <summary>
        /// Gets the applicable reporting states for the website metadata generation.
        /// </summary>
        IList<SelectListItem> GetApplicableReportingStates(IEnumerable<string> statesToReturn = null);
        /// <summary>
        /// Gets the get applicable zip code radii for the website metadata generation.
        /// </summary>
        /// <value>
        /// The get applicable zip code radii for the website metadata generation.
        /// </value>
        IList<SelectListItem> ApplicableZipCodeRadii { get; }

        /// <summary>
        /// Get all the hospitals from applicable Hospitals
        /// </summary>
        //IList<Hospital> GetApplicableHospitals(Expression<Func<Hospital, bool>> additionalCriteria = null, bool onlyFirst = false);
        int GetHospitalsCountForValidation(Expression<Func<Hospital, bool>> additionalCriteria = null, IEnumerable<string> statesToReturn = null);

        /// <summary>
        /// Gets the applicable site fonts.
        /// </summary>
        /// <value>
        /// The applicable site fonts.
        /// </value>
        IList<SelectListItem> ApplicableSiteFonts { get; }
        /// <summary>
        /// Gets the applicable website themes.
        /// </summary>
        /// <value>
        /// The applicable website themes.
        /// </value>
        IList<string> ApplicableWebsiteThemes { get; }
        /// <summary>
        /// Gets the websites for measure.
        /// </summary>
        /// <param name="measureId">The measure identifier.</param>
        /// <returns></returns>
        IList<Website> GetWebsitesForMeasure(int measureId);

        IList<string> GetWebsiteNamesForMeasure(int measureId);

        /// <summary>
        /// Gets the installed datasets.
        /// </summary>
        /// <returns></returns>
        IList<Target> GetInstalledDatasets();
        //List<KeyValuePair<int, string>> GetInstalledDatasets();
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback">The callback.</param>
        void GetAll<T>(Action<List<T>, Exception> callback) where T : class, IEntity;
        /// <summary>
        /// Gets the entity by identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="callback">The callback.</param>
        void GetEntityById<T>(object id, Action<T, Exception> callback);
        /// <summary>
        /// Refreshes the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        void RefreshEntity<T>(T entity, Action<bool, Exception> callback) where T : class, IEntity;
        /// <summary>
        /// Gets all websites.
        /// </summary>
        /// <returns></returns>
        ObservableCollection<WebsiteViewModel> GetAllWebsites();
        /// <summary>
        /// Saves the or update website.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <returns></returns>
        bool SaveOrUpdateWebsite(Website website);


        /// <summary>
        /// Saves the or update website asynchronusly.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="completionCallback">The completion callback.</param>
        void SaveOrUpdateWebsiteAsync(Website website, Action<OperationResult<Website>> completionCallback);
        /// <summary>
        /// Saves the new website.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <returns></returns>
        bool SaveNewWebsite(Website website);    //string name, string description, string year, ReportingQuarter quarter, Audience audience, WebsiteState status);
        /// <summary>
        /// Updates the website.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <returns></returns>
        bool UpdateWebsite(Website website);
        /// <summary>
        /// Deletes the website.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <returns></returns>
        bool DeleteWebsite(Website website);
		/// <summary>
        /// Delete the measure override.
		/// </summary>
		/// <param name="websiteMeasure"></param>
		/// <param name="callback"></param>
		void DeleteMeasureOverride(ref WebsiteMeasure websiteMeasure, Action<WebsiteMeasure, Exception> callback);
        /// <summary>
        /// Saves the measure override.
        /// </summary>
        /// <param name="measureOverride">The measure override.</param>
        /// <param name="callback">The callback.</param>
        void SaveMeasureOverride(Measure measureOverride, Action<Measure, Exception> callback);
        /// <summary>
        /// Gets the topic view models.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TopicViewModel> GetTopicViewModels(TopicTypeEnum? type = null);
        /// <summary>
        /// Gets the measure view models.
        /// </summary>
        /// <returns></returns>
        IEnumerable<MeasureModel> GetMeasureViewModels(Expression<Func<Measure, bool>> queryExpression);
        /// <summary>
        /// Gets the data sets.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetDataSets();
        /// <summary>
        /// Gets the topic categories.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TopicCategory> GetTopicCategories();
        /// <summary>
        /// Refreshes the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        T Refresh<T>(T item) where T : class, IEntity;
        /// <summary>
        /// Gets the reports.
        /// </summary>
        /// <returns></returns>
        IList<ReportViewModel> GetReports(Expression<Func<Report, bool>> queryExpression = null);

        IList<ReportViewModel> GetReports(string hqlQuery);
		/// <summary>
		/// Gets WebsitePages.
		/// </summary>
		/// <returns></returns>
		IEnumerable<WebsitePage> GetWebsitePages(Expression<Func<WebsitePage, bool>> queryExpression);
		/// <summary>
		/// Gets WebsitePageModels.
		/// </summary>
		/// <returns></returns>
		IEnumerable<WebsitePageModel> GetWebsitePageModels(Expression<Func<WebsitePage, bool>> queryExpression);
		/// <summary>
		/// Gets BaseDataWebsitePages.
		/// </summary>
		/// <returns></returns>
		IEnumerable<BDWebsitePage> GetBaseDataWebsitePages(Expression<Func<BDWebsitePage, bool>> queryExpression);
		/// <summary>
		/// Gets BaseDataWebsitePageZones.
		/// </summary>
		/// <returns></returns>
		IEnumerable<BDWebsitePageZone> GetBaseDataWebsitePageZones(Expression<Func<BDWebsitePageZone, bool>> queryExpression);
		/// <summary>
		/// Gets the categories.
		/// </summary>
		/// <value>
		/// The categories.
		/// </value>
		IList<string> Categories { get; }
        /// <summary>
        /// Gets the audiences.
        /// </summary>
        /// <value>
        /// The audiences.
        /// </value>
        IList<AudienceModel> Audiences { get; }
        /// <summary>
        /// Gets the report types.
        /// </summary>
        /// <value>
        /// The report types.
        /// </value>
        IList<string> ReportTypes { get; }

        /// <summary>
        /// Gets the states.
        /// </summary>
        /// <value>
        /// The states.
        /// </value>
        IList<SelectListItem> GetStates();

        /// <summary>
        /// Gets the filter enumerations main view.
        /// </summary>
        /// <value>
        /// The filter enumerations main view.
        /// </value>
        IList<KeyValuePair<FilterKeys, string>> FilterEnumerations { get; }

        /// <summary>
        /// Gets the websites for report.
        /// </summary>
        /// <param name="reportId">The report identifier.</param>
        /// <returns></returns>
        IList<Website> GetWebsitesForReport(int reportId);

        /// <summary>
        /// Gets the hospitals for website.
        /// </summary>
        /// <param name="states">The states.</param>
        /// <returns></returns>
        IList<HospitalViewModel> GetHospitalsForWebsite(IEnumerable<string> states);

        /// <summary>
        /// Gets the hospitals for website count.
        /// </summary>
        /// <param name="states">The states.</param>
        /// <returns></returns>
        int GetHospitalsForWebsiteCount(IEnumerable<string> states);

        IList<string> GetWebsiteNamesForReport(int reportId);

        /// <summary>
        /// Gets the CCR for hospital.
        /// </summary>
        /// <param name="cmsProviderId">The CMS provider identifier.</param>
        /// <param name="reportingYr">The reporting yr.</param>
        /// <returns></returns>
        string GetCCRForHospital(string cmsProviderId, string reportingYr);

        /// <summary>
        /// Gets Information about region to population mapping for dependency check
        /// </summary>
        /// <returns></returns>
        Tuple<int, int> GetCustomRegionToPopulationMappingInfo();

        bool HasImportedRegionId(string regionType, int contentItemRecordId);

        bool IsCustomRegionDefined();

        bool IsMedicareDataImported();

        bool CheckIfCustomRegionsDefined(IEnumerable<int> datasetIds, IEnumerable<string> states, string websiteRegionType);

        ObservableCollection<string> ReportingYears { get; }

        List<NursingHome> GetNursingHomesForWebsite(IEnumerable<string> states);

        bool CheckIfCustomRegionsMatchLibrary(List<int> datasetIds, List<string> states, string websiteRegionType);

        IList<Menu> GetApplicableMenuItems();
    }
}
