using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using LinqKit;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Infrastructure.Types;
using Monahrq.Sdk.Types;
using Monahrq.Websites.Model;
using Monahrq.Websites.ViewModels;
using NHibernate;
using NHibernate.Linq;
using Monahrq.Infrastructure.Entities.Domain.WebsiteContentManagement;
using BDWebsitePage = Monahrq.Infrastructure.Entities.Domain.BaseData.BaseWebsitePage;
using BDWebsitePageZone = Monahrq.Infrastructure.Entities.Domain.BaseData.BaseWebsitePageZone;


namespace Monahrq.Websites.Services
{
    public enum FilterKeys { None, ReportName, ReportType, Website, RecommendedAudiences }

    [Export(typeof(IWebsiteDataService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WebsiteDataService : DataServiceBase, IWebsiteDataService
    {
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public IDomainSessionFactoryProvider Provider { get; set; }

        [Import]
        public IConfigurationService ConfigService  { get; set; } 

        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public IBaseDataService BaseDataService { get; set; }

        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public IRegionManager RegionManager { get; set; }

        [Import(LogNames.Session)]
        public ILogWriter SessionLog { get; set; }

        public ObservableCollection<WebsiteViewModel> GetAllWebsites()
        {
            var websites = new ObservableCollection<WebsiteViewModel>();

            List<Website> result = new List<Website>();
            using (var session = GetSession())
            {
                result = session.Query<Website>().Select(x => new Website
                {
                    Id = x.Id,
                    Name = x.Name,
                    Audiences = x.Audiences,
                    ReportedQuarter = x.ReportedQuarter,
                    ReportedYear = x.ReportedYear,
                    Description = x.Description,
                    ActivityLog = x.ActivityLog,
                    CurrentStatus = x.CurrentStatus,
                    //RegionTypeContext = x.RegionTypeContext,
                    //StateContext = x.StateContext,
                    //SelectedReportingStates = x.SelectedReportingStates
                }).Distinct().ToList();
            }

            var distinctWesbite = result.DistinctBy(x => x.Id).ToList();
            var allLogs = result.Select(x => Tuple.Create(x.Id, x.ActivityLog.FirstOrDefault())).ToList();

            distinctWesbite.ForEach(dw =>
            {
                var logs = allLogs.Where(x => x.Item1 == dw.Id).Select(l => l.Item2).Distinct();
                dw.ActivityLog = logs.ToList();
            });

            distinctWesbite.ForEach(item => websites.Add(new WebsiteViewModel
            {
                Website = item,
                RegionManager = RegionManager,
                WebsiteDataService = this,
                BaseDataService = BaseDataService,
            }));

            return websites;
        }

        public bool IsDatasetHasCountyData(string query)
        {
            var result = false;
            int queryResult;
            using (var session = GetStatelessSession())
            {
                queryResult = session.CreateSQLQuery(query).UniqueResult<int>();
            }
            result = queryResult > 0;
            return result;
        }

        public T ExecuteScalar<T>(string query)
        {
            using (var session = GetStatelessSession())
            {
                return session.CreateSQLQuery(query)
                              .UniqueResult<T>();
            }
        }
        public void DeleteMeasureOverride(ref WebsiteMeasure websiteMeasure, Action<WebsiteMeasure, Exception> callback)
        {
            // Exception error = null;

            var overrideMeasure = websiteMeasure.OverrideMeasure;

            using (var session = GetSession())
            {
                try
                {
                    using (var tx = session.BeginTransaction())
                    {
                        websiteMeasure.OverrideMeasure = null;
                        websiteMeasure = session.Merge(websiteMeasure);
                        overrideMeasure.ClearTopics();
                        overrideMeasure = session.Merge(overrideMeasure);
                        //session.Delete(overrideMeasure);						// In theory, there are no more DB references to this object; However, NHibernate still complains that the object we be cascaded and re saved.

                        session.Flush();
                        tx.Commit();
                        callback(websiteMeasure, null);
                    }
                }
                catch (Exception exc)
                {
                    callback(websiteMeasure, exc.GetBaseException());
                    Log(exc, "Removing Website Measure Override from DB", new EntityDescriptor(websiteMeasure));
                }
            }
        }

        public void SaveMeasureOverride(Measure measureOverride, Action<Measure, Exception> callback)
        {
            // Exception error = null;
            using (var session = GetSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    try
                    {

                        measureOverride = session.Merge(measureOverride);

                        session.Flush();

                        tx.Commit();

                        callback(measureOverride, null);
                    }
                    catch (Exception exc)
                    {
                        callback(measureOverride, exc.GetBaseException());
                        Log(exc.GetBaseException(), "Saving to DB", new EntityDescriptor(measureOverride));
                    }

                }
            }
            //Save(measureOverride, (o, e) =>
            //    {
            //        if (e == null)
            //        {
            //            callback(o as Measure, error);
            //        }
            //        else
            //        {
            //            error = e;
            //            callback(o as Measure, e);
            //        }
            //    });
        }

        public bool SaveOrUpdateWebsite(/*ref*/ Website website)
        {
            var hasErrors = false;

            try
            {
                website.CleanBeforeSave();

                using (var session = GetSession())
                {
                    session.Evict(website);

                    using (var tx = session.BeginTransaction())
                    {
                        // HACK: Isolate the use case that resides on the Website.Measures i.e. saving a measure then saving the website causes a session nHiberate issue
                        // This only occurs
                        if (/*!website.IsPersisted Website MUST not yet be saved &&*/
                                 website.Measures.Any()
                                && website.Measures.Any(m => m.OverrideMeasure != null && m.OverrideMeasure.IsPersisted) /*Override measure must have already been created*/
                            )
                        {
                            var savedWebsite = session.Merge(website);

                            // HACK: This is to force nHibernate to render the object as persisted once it commits to the database
                            if (savedWebsite != null)
                                website.Id = savedWebsite.Id;

                            // Michael Graves
                            // There is issue with just assigning Id as above.  The WebsiteMeasures never gets an Id (via this save path), thus WebsiteMeasure.IsPersitent is always false.
                            // I repeat, Once a OverrideMeasure is saved, this Path is always taken.  So once Override measure is saved, WebsiteMeasures never
                            // get an Id, thus never Persistent (flag only).
                            // There are a few places that check WebsiteMeasure.IsPersisted and will fail their logic.
                            // - When 'Continuing' from the WebsiteDatasetsView, measures are loaded.  Here, the code populates the Measures IsSelected field
                            //   for the WebsiteMeasuresView.   If WebsiteMeasure is persisted, then the value from the db should be used (thus the code
                            //	 shouldn't attempt to overwrite the WebsiteMeasures.IsSelected value, else, it should use the 'inplace' logic to select the
                            //   IsSelected (checkbox) field.   
                            //	**** Explains why a Website's Selected Measures seem to get lost some times.
                            // - I need to check the WebsiteMeasures.IsPersisted during a single/batch Measure Edit to know if code should also save entire
                            //   if the WebsiteMeasure isn't already persisted.  (As of now, IsPersistent is always null (when this path was used on last Save),
                            //   so whole website Save is always done).
                            // - Maybe others, haven't checked.
                            // - Code requires the WebsiteMeasure.IsPersisted field and cannot use the MeasureOriginal or MeasureOverride.  MeasureOriginal is
                            //   always IsPersisted = true, and MeasureOverride is true after an edit/update.  (Not sure why MeasureOverrides get special
                            //	 treatment and go to the database despite being saved with no attachment to the website, but eh, that;s the logic)
                            // I'd wager there are other Id's never getting a DB value because of this.   This seems to be a linchpin problem, LINCHPIN i say!!
                            // Going to try the following instead, otherwise will have to resort to copying over the Ids for WebsiteMeasures.  Without checking
                            // yet, this makes me wonder how/if Reports get Ids after an OverrideMeasure is saved.

                            //website = savedWebsite;


                            // Silly, that doesn't work either.  All it does is change the pointer the local variable 'website' is pointing to.  What is need is
                            // the passed in variable (likely 'base.ManageViewModel.WebsiteViewModel.Website') to point to 'savedWebsite'.  
                            //	So either:
                            //		A)	website parameter needs to be made "ref"
                            //		B) 'savedWebsite' needs to be passed out, and left to the caller to reassign to the main variable.
                            //		C) Function needs to accept WebsiteViewModel, and then save and reassign it's ".Website" object
                            //
                            //	Thus, none of the "website = savedWebsite;" in the catch blocks below are doing anything.
                            //
                            //	Too big a change to validate now, shouldn't be to hard to do A, the ref parameter.  Leaving as it was.


                        }
                        else if (!website.IsPersisted)
                        {
                            try
                            {
                                session.SaveOrUpdate(website);
                            }
                            catch (NonUniqueObjectException)
                            {
                                session.Clear();
                                var savedWebsite = session.Merge(website);
                                if (savedWebsite != null)
                                    website = savedWebsite;
                            }
                        }
                        else
                        {
                            try
                            {
                                session.SaveOrUpdate(website);
                            }
                            catch (NonUniqueObjectException)
                            {
                                session.Clear();
                                var savedWebsite = session.Merge(website);
                                if (savedWebsite != null)
                                    website = savedWebsite;
                            }
                        }

                        session.Flush();
                        tx.Commit();
                    }

                    //session.Refresh(website);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                hasErrors = true;
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e.GetBaseException());
                Log(e.GetBaseException(), "Saving to DB", new EntityDescriptor(website));
            }
            return hasErrors;
        }

        /// <summary>
        /// Return true if an error occurs
        /// </summary>
        /// <param name="website"></param>
        /// <returns></returns>
        public bool SaveNewWebsite(Website website)
        {
            return SaveOrUpdateWebsite(website);
        }

        /// <summary>
        /// Return true if an error occurs
        /// </summary>
        /// <param name="website"></param>
        /// <returns></returns>
        public bool UpdateWebsite(Website website)
        {
            return SaveOrUpdateWebsite(website);
        }

        public async void SaveOrUpdateWebsiteAsync(Website website, Action<OperationResult<Website>> completionCallback)
        {
            //var hasErrors = false;

            await Task.Run(() =>
            {
                try
                {
                    website.CleanBeforeSave();

                    using (var session = GetSession())
                    {
                        session.Evict(website);

                        using (var tx = session.BeginTransaction())
                        {
                            // HACK: Isolate the use case that resides on the Website.Measures i.e. saving a measure then saving the website causes a session nHiberate issue
                            // This only occurs
                            if ( /*!website.IsPersisted Website MUST not yet be saved &&*/
                                website.Measures.Any()
                                && website.Measures.Any(m => m.OverrideMeasure != null && m.OverrideMeasure.IsPersisted)
                                /*Override measure must have already been created*/
                                )
                            {
                                var savedWebsite = session.Merge(website);

                                // HACK: This is to force nHibernate to render the object as persisted once it commits to the database
                                if (savedWebsite != null)
                                    website.Id = savedWebsite.Id;

                                // Michael Graves
                                // There is issue with just assigning Id as above.  The WebsiteMeasures never gets an Id (via this save path), thus WebsiteMeasure.IsPersitent is always false.
                                // I repeat, Once a OverrideMeasure is saved, this Path is always taken.  So once Override measure is saved, WebsiteMeasures never
                                // get an Id, thus never Persistent (flag only).
                                // There are a few places that check WebsiteMeasure.IsPersisted and will fail their logic.
                                // - When 'Continuing' from the WebsiteDatasetsView, measures are loaded.  Here, the code populates the Measures IsSelected field
                                //   for the WebsiteMeasuresView.   If WebsiteMeasure is persisted, then the value from the db should be used (thus the code
                                //	 shouldn't attempt to overwrite the WebsiteMeasures.IsSelected value, else, it should use the 'inplace' logic to select the
                                //   IsSelected (checkbox) field.   
                                //	**** Explains why a Website's Selected Measures seem to get lost some times.
                                // - I need to check the WebsiteMeasures.IsPersisted during a single/batch Measure Edit to know if code should also save entire
                                //   if the WebsiteMeasure isn't already persisted.  (As of now, IsPersistent is always null (when this path was used on last Save),
                                //   so whole website Save is always done).
                                // - Maybe others, haven't checked.
                                // - Code requires the WebsiteMeasure.IsPersisted field and cannot use the MeasureOriginal or MeasureOverride.  MeasureOriginal is
                                //   always IsPersisted = true, and MeasureOverride is true after an edit/update.  (Not sure why MeasureOverrides get special
                                //	 treatment and go to the database despite being saved with no attachment to the website, but eh, that;s the logic)
                                // I'd wager there are other Id's never getting a DB value because of this.   This seems to be a linchpin problem, LINCHPIN i say!!
                                // Going to try the following instead, otherwise will have to resort to copying over the Ids for WebsiteMeasures.  Without checking
                                // yet, this makes me wonder how/if Reports get Ids after an OverrideMeasure is saved.

                                //website = savedWebsite;


                                // Silly, that doesn't work either.  All it does is change the pointer the local variable 'website' is pointing to.  What is need is
                                // the passed in variable (likely 'base.ManageViewModel.WebsiteViewModel.Website') to point to 'savedWebsite'.  
                                //	So either:
                                //		A)	website parameter needs to be made "ref"
                                //		B) 'savedWebsite' needs to be passed out, and left to the caller to reassign to the main variable.
                                //		C) Function needs to accept WebsiteViewModel, and then save and reassign it's ".Website" object
                                //
                                //	Thus, none of the "website = savedWebsite;" in the catch blocks below are doing anything.
                                //
                                //	Too big a change to validate now, shouldn't be to hard to do A, the ref parameter.  Leaving as it was.


                            }
                            else if (!website.IsPersisted)
                            {
                                try
                                {
                                    session.SaveOrUpdate(website);
                                }
                                catch (NonUniqueObjectException)
                                {
                                    session.Clear();
                                    var savedWebsite = session.Merge(website);
                                    if (savedWebsite != null)
                                        website = savedWebsite;
                                }
                            }
                            else
                            {
                                try
                                {
                                    session.SaveOrUpdate(website);
                                }
                                catch (NonUniqueObjectException)
                                {
                                    session.Clear();
                                    var savedWebsite = session.Merge(website);
                                    if (savedWebsite != null)
                                        website = savedWebsite;
                                }
                            }

                            session.Flush();
                            tx.Commit();
                        }

                        //session.Refresh(website);
                        completionCallback(new OperationResult<Website>() { Status = true, Model = website });
                    }
                }
                catch (ThreadAbortException e)
                {
                    Log(e.GetBaseException(), "Saving to DB", new EntityDescriptor(website));
                }
                catch (Exception e)
                {
                    //hasErrors = true;

                    Log(e.GetBaseException(), "Saving to DB", new EntityDescriptor(website));
                    completionCallback(new OperationResult<Website> { Status = false, Exception = e.GetBaseException() });
                }
            });
            //return hasErrors;
        }

        public IEnumerable<TopicViewModel> GetTopicViewModels(TopicTypeEnum? type = null)
        {
            using (var session = GetSession())
            {
                var query = session.Query<TopicCategory>();
                if (type.HasValue)
                    query = query.Where(tc => tc.TopicType == type.Value);

                return query.OrderBy(x => x.Name).Select(t => new TopicViewModel(t)).ToList();
            }
        }

        public IEnumerable<TopicCategory> GetTopicCategories()
        {
            using (var session = GetSession())
            {
                return session.Query<TopicCategory>().OrderBy(x => x.Name).ToList();
            }
        }

        public IEnumerable<MeasureModel> GetMeasureViewModels(Expression<Func<Measure, bool>> queryExpression)
        {
            using (var session = GetSession())
            {
                return session.Query<Measure>()
                               .Where(queryExpression)
                               //.Cacheable()
                               //.CacheRegion("Website:Measures")
                               //.CacheMode(CacheMode.Normal)
                               .OrderBy(x => x.Name)
                               .Select(t => new MeasureModel(t, null))
                               .ToList();
            }
        }
        public IEnumerable<WebsitePage> GetWebsitePages(Expression<Func<WebsitePage, bool>> queryExpression)
        {

            using (var session = GetSession())
            {
                return session.Query<WebsitePage>()
                                .Where(queryExpression)
                                //.Cacheable()
                                //.CacheRegion("Website:Measures")
                                //.CacheMode(CacheMode.Normal)
                                .OrderBy(x => x.Name)
                                .ToList();
            }
        }
        public IEnumerable<WebsitePageModel> GetWebsitePageModels(Expression<Func<WebsitePage, bool>> queryExpression)
        {

            using (var session = GetSession())
            {
                return session.Query<WebsitePage>()
                                .Where(queryExpression)
                                //.Cacheable()
                                //.CacheRegion("Website:Measures")
                                //.CacheMode(CacheMode.Normal)
                                .OrderBy(x => x.Name)
                                .Select(wp => new WebsitePageModel(wp, true))
                                .ToList();
            }
        }
        public IEnumerable<BDWebsitePage> GetBaseDataWebsitePages(Expression<Func<BDWebsitePage, bool>> queryExpression)
        {

            using (var session = GetSession())
            {
                return session.Query<BDWebsitePage>()
                                .Where(queryExpression)
                                //.Cacheable()
                                //.CacheRegion("Website:Measures")
                                //.CacheMode(CacheMode.Normal)
                                .OrderBy(x => x.Name)
                                .ToList();
            }
        }
        public IEnumerable<BDWebsitePageZone> GetBaseDataWebsitePageZones(Expression<Func<BDWebsitePageZone, bool>> queryExpression)
        {

            using (var session = GetSession())
            {
                return session.Query<BDWebsitePageZone>()
                                .Where(queryExpression)
                                //.Cacheable()
                                //.CacheRegion("Website:Measures")
                                //.CacheMode(CacheMode.Normal)
                                .OrderBy(x => x.Name)
                                .ToList();
            }
        }
        public IEnumerable<string> GetDataSets()
        {
            List<string> datasets;
            using (var session = GetSession())
            {
                datasets = session.Query<Target>().Select(x => x.Name).ToList();
                datasets.Insert(0, "All Datasets");
            }

            return datasets;
        }

        /// <summary>
        /// Return true if an error occurs
        /// </summary>
        /// <param name="website"></param>
        /// <returns></returns>
        public bool DeleteWebsite(Website website)
        {
            bool error;

            using (var session = GetSession())
            {
                try
                {
                    using (var trans = session.BeginTransaction())
                    {
                        website.Reports.Clear();
                        website.Measures.Clear();
                        website.Datasets.Clear();
                        website.Hospitals.Clear();
                        website.NursingHomes.Clear();
                        website.Menus.Clear();
                        website.Themes.Clear();

                        website = session.Merge(website);

                        session.Evict(website);

                        session.Delete(website);

                        trans.Commit();
                    }

                    error = false;
                }
                catch (Exception e)
                {
                    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e.GetBaseException());
                    error = true;
                }
            }

            return error;
        }

        public IEnumerable<DatasetSummary> GetDatasetSummary(int? id = null)
        {
            var datasets = new List<DatasetSummary>();

            using (var session = GetSession())
            {
                // DeleteUnfinishedRecords(session);

                var datasetList = session.Query<Dataset>()
                                                .Where(item => item.IsFinished)
                                                .Take(500).ToList();

                datasetList.RemoveAll(c => c.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge") && c.DRGMDCMappingStatus != DrgMdcMappingStatusEnum.Completed);

                datasets = datasetList.Select(ds => new DatasetSummary(ds, ds.ContentType)).ToList();
            }
            return datasets;
        }

        /// <summary>
        /// Deletes the unfinished records.
        /// I really hate doing this because it is a hack big time. - Jason
        /// TODO: resolve this once the one-way in/out data service is available.
        /// </summary>
        /// <param name="session">The session.</param>
        private void DeleteUnfinishedRecords(ISession session)
        {
            var toReconcile = session.Query<Dataset>().Where(item => !item.IsFinished).ToList();

            using (var trans = session.BeginTransaction())
            {
                foreach (var item in toReconcile)
                {
                    var targetType = session.Query<Target>().SingleOrDefault(f => f.Name.ToLower() == item.ContentType.Name.ToLower());

                    if (targetType == null)
                        continue;

                    string targetTypeName = null;

                    try
                    {
                        var type = Type.GetType(targetType.ClrType);

                        if (type != null)
                            targetTypeName = type.Name;
                    }
                    catch { }

                    if (string.IsNullOrEmpty(targetTypeName))
                        continue;

                    session.Evict(item);

                    // HQL queries
                    var targetDelete = string.Format("delete from {0} o where o.Dataset.Id = {1}",
                                                     targetTypeName, item.Id);
                    //var transactionType = typeof(ContentPartTransactionRecord);
                    var transactionDelete =
                        string.Format(
                            "delete from DatasetTransactionRecord ct where ct.Dataset.Id = {0}",
                            item.Id);

                    var importDelete = string.Format("delete from Dataset d where d.Id = {0}", item.Id);

                    // Delete Target data first.
                    session.CreateQuery(targetDelete)
                           .SetTimeout(300)
                           .ExecuteUpdate();

                    // Delete Transactional record
                    session.CreateQuery(transactionDelete)
                           .SetTimeout(300)
                           .ExecuteUpdate();

                    // finally delete content item record.
                    session.CreateQuery(importDelete)
                           .SetTimeout(300)
                           .ExecuteUpdate();
                }

                trans.Commit();
            }

            session.Flush();
        }

        public IList<HospitalViewModel> GetHospitalsForWebsite(IEnumerable<string> states)
        {
            //var hospitals = new List<Hospital>();
            using (var session = GetSession())
            {
                return session.Query<Hospital>()
                                   .Where(h => states.Contains(h.State) && !h.IsArchived && !h.IsDeleted)
                                   .DistinctBy(h => h.Id)
                                   .Select(hospital => new HospitalViewModel(hospital))
                                   .ToList();
            }
        }

        public int GetHospitalsForWebsiteCount(IEnumerable<string> states)
        {
            if (states == null) return 0;

            using (var session = GetSession())
            {
                return session.Query<Hospital>()
                                   .Count(h => states.Contains(h.State) && !h.IsArchived && !h.IsDeleted);
            }
        }


        public IList<Target> GetInstalledDatasets()
        {
            using (var session = GetSession())
            {
                return session.Query<Target>()
                              .OrderBy(t => t.DisplayOrder)
                              //.Cacheable().CacheRegion("Websites:InstalledDatasets").CacheMode(CacheMode.Normal)
                              .ToList();
            }
        }

        public IList<ReportViewModel> GetReports(Expression<Func<Report, bool>> queryExpression = null)
        {
            //WaitCursor.Show();

            IList<ReportViewModel> result = new List<ReportViewModel>();

            try
            {
                using (var session = GetSession())
                {
                    var reportsQuery = session.Query<Report>();
                    //.Cacheable()
                    //.CacheRegion("Website:Reports")
                    //.CacheMode(CacheMode.Normal);

                    var tempresult = queryExpression == null ? reportsQuery : reportsQuery.Where(queryExpression);

                    result = tempresult.Select(r => new ReportViewModel(r)).ToList().OrderBy(r => r.Category).ThenBy(r => r.Name).ToList();
                }
            }
            catch (Exception e)
            {
                Log(e.GetBaseException(), "Loading all", new EntityDescriptor(typeof(Report).Name));
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e.GetBaseException());
            }
            return result;
        }

        public IList<ReportViewModel> GetReports(string hqlQuery)
        {
            IList<Report> result = new List<Report>();

            try
            {
                using (var session = GetSession())
                {
                    result = string.IsNullOrEmpty(hqlQuery)
                                            ? session.Query<Report>()
                                                      //.Cacheable()
                                                      //.CacheRegion("Website:Reports")
                                                      //.CacheMode(CacheMode.Normal)
                                                      .ToFuture().ToList()
                                            : session.CreateQuery(hqlQuery)
                                                     //.SetCacheable(true)
                                                     //.SetCacheMode(CacheMode.Normal)
                                                     //.SetCacheRegion("Website:Reports")
                                                     .List<Report>();
                }

                if (result != null && result.Any())
                {
                    result = result.OrderBy(r => r.Category).ThenBy(r => r.Name).ToList();
                }
            }
            catch (Exception e)
            {
                Log(e.GetBaseException(), "Loading all", new EntityDescriptor(typeof(Report).Name));
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e.GetBaseException());
            }
            return result.Select(r => new ReportViewModel(r)).ToList();
        }

        /// <summary>
        /// Gets the websites for measure.
        /// </summary>
        /// <param name="measureId">The measure identifier.</param>
        /// <returns></returns>
        public IList<Website> GetWebsitesForMeasure(int measureId)
        {
            using (var session = GetStatelessSession())
            {
                return session.Query<Website>().Where(x => x.Measures.Any(wm => wm.OriginalMeasure.Id == measureId)).ToList();
            }
        }

        public IList<string> GetWebsiteNamesForMeasure(int measureId)
        {
            using (var session = GetStatelessSession())
            {
                return session.Query<Website>()
                              .Where(x => x.Measures.Any(wm => wm.IsSelected && wm.OriginalMeasure.Id == measureId))
                              .Select(w => w.Name).ToList();
            }
        }

        /// <summary>
        /// Gets the websites for a specfified report.
        /// </summary>
        /// <param name="reportId">The report identifier.</param>
        /// <returns></returns>
        public IList<Website> GetWebsitesForReport(int reportId)
        {
            using (var session = GetStatelessSession())
            {
                return session.Query<Website>().Where(x => x.Reports.Any(wm => wm.Report.Id == reportId)).ToList();
            }
        }

        /// <summary>
        /// Gets the website names for report.
        /// </summary>
        /// <param name="reportId">The report identifier.</param>
        /// <returns></returns>
        public IList<string> GetWebsiteNamesForReport(int reportId)
        {
            using (var session = GetStatelessSession())
            {
                return session.Query<Website>()
                              .Where(x => x.Reports.Any(wm => wm.Report.Id == reportId))
                              .Select(w => w.Name).ToList();
            }
        }

        /// <summary>
        /// Gets the CCR for hospital.
        /// </summary>
        /// <param name="cmsProviderId">The CMS provider identifier.</param>
        /// <param name="reportingYr">The reporting yr.</param>
        /// <returns></returns>
        public string GetCCRForHospital(string cmsProviderId, string reportingYr)
        {
            var query = string.Format("select top 1 [dbo].fnGetCostToChargeRatio('{0}','{1}') from Hospitals ", reportingYr, cmsProviderId);
            using (var session = GetStatelessSession())
            {
                return session.CreateSQLQuery(query)
                              .UniqueResult<float>().ToString();
            }
        }

        /// <summary>
        /// Gets the get applicable zip code radii for the website metadata generation.
        /// </summary>
        /// <value>
        /// The get applicable zip code radii for the website metadata generation.
        /// </value>
        public IList<SelectListItem> ApplicableZipCodeRadii
        {
            get
            {
                return Website.ApplicableZipCodeRadii
                                      .Select(x => new SelectListItem
                                      {
                                          Text = x.ToString(CultureInfo.InvariantCulture),
                                          Value = x.ToString(CultureInfo.InvariantCulture),
                                          Model = x
                                      }).ToList();
            }
        }

        /// <summary>
        /// Gets the get applicable zip code radii for the website metadata generation.
        /// </summary>
        /// <value>
        /// The get applicable zip code radii for the website metadata generation.
        /// </value>
        public IList<SelectListItem> ApplicableSiteFonts
        {
            get
            {
                return Website.ApplicableSiteFonts
                                      .Select(x => new SelectListItem
                                      {
                                          Text = x.ToString(CultureInfo.InvariantCulture),
                                          Value = x.ToString(CultureInfo.InvariantCulture),
                                          Model = x
                                      }).ToList();
            }
        }

        public IList<SelectListItem> GetStates()
        {
            var states = new List<SelectListItem>();
            using (var sess = base.GetStatelessSession())
            {
                states = sess.Query<State>()
                             .Select(x => new SelectListItem
                             {
                                 Text = x.Name.ToString(CultureInfo.InvariantCulture),
                                 Value = x.Abbreviation.ToString(CultureInfo.InvariantCulture),
                                 Model = x
                             })
                             .ToList();
            }

            return states;
        }


        /// <summary>
        /// Gets the applicable website themes.
        /// </summary>
        /// <value>
        /// The applicable website themes.
        /// </value>
        public IList<string> ApplicableWebsiteThemes
        {
            get
            {
                var themes = ConfigService.MonahrqSettings.Themes.OfType<MonahrqThemeElement>()
                                                                 .Select(theme => theme.Name).ToList();
                return themes;
            }
        }

        /// <summary>
        /// Gets the applicable reporting states for the website metadata generation.
        /// </summary>
        public IList<SelectListItem> GetApplicableReportingStates(IEnumerable<string> statesToReturn = null)
        {
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            IList<SelectListItem> states = new List<SelectListItem>();

            if (statesToReturn == null)
            {
                if (!MonahrqContext.ReportingStatesContext.Any())
                    MonahrqContext.ReportingStatesContext = configService.HospitalRegion.DefaultStates.OfType<string>().Distinct().ToList();

                if (MonahrqContext.ReportingStatesContext.Any())
                {
                    statesToReturn = MonahrqContext.ReportingStatesContext.Distinct().ToList();
                }
            }
            //else
            //{
            using (var session = base.GetStatelessSession())
            {
                states = session.Query<State>()
                                .Where(s => statesToReturn.Contains(s.Abbreviation))
                                .Select(s => new SelectListItem { Value = s.Abbreviation, Text = s.Name, Model = s })
                                .ToList();
            }

            //}

            return states;

            //return HospitalRegion.Default.SelectedStates.Select(s => new SelectListItem()
            //{
            //    Value = s.Abbreviation,
            //    Text = s.Name,
            //    Model = s 

            //}).ToList();

        }

        public int GetHospitalsCountForValidation(Expression<Func<Hospital, bool>> additionalCriteria = null, IEnumerable<string> statesToReturn = null)
        {
            var states = GetApplicableReportingStates(statesToReturn)
                .Select(x => x.Model as State)
                .ToList();


            using (var session = GetSession())
            {
                var stateCrit = PredicateBuilder.False<Hospital>();
                states.ForEach(s => stateCrit = stateCrit.Or(h => h.State.ToUpper() == s.Abbreviation.ToUpper()));
                var crit = additionalCriteria ?? PredicateBuilder.True<Hospital>().And(stateCrit);
                return session.Query<Hospital>().Count(crit);
            }
        }

        public IList<string> Categories { get { return _getEnumDescriptors<ReportCategory>(); } }

        public IList<AudienceModel> Audiences { get { return _getModelFromEnums<Audience, AudienceModel>(); } }

        public IList<string> ReportTypes { get { return _getReportTypes(); } }

        public IList<KeyValuePair<FilterKeys, string>> FilterEnumerations
        {
            get
            {
                return new List<KeyValuePair<FilterKeys, string>>
                    {
                    new KeyValuePair<FilterKeys, string>(FilterKeys.None, string.Empty),
                    new KeyValuePair<FilterKeys, string>(FilterKeys.ReportName, "Report Name"),
                    new KeyValuePair<FilterKeys, string>(FilterKeys.ReportType, "Report Type"),
                    new KeyValuePair<FilterKeys, string>(FilterKeys.Website, "Website"),
                    new KeyValuePair<FilterKeys, string>(FilterKeys.RecommendedAudiences, "Recommended Audiences")
                };
            }
        }

        public Tuple<int, int> GetCustomRegionToPopulationMappingInfo()
        {
            // count the number of custom region an dpopulation data imported
            // count the number of custom region imported, but population data
            // count the number of Population file imported, but not region
            using (var session = Provider.SessionFactory.OpenSession())
            {
                string query = @"Declare @CustomRegionCount int, @CustomRegionPopulationCount int

                                 SELECT @CustomRegionCount = COUNT(*)
                                 FROM [dbo].[Regions]
                                 WHERE RegionType = 'CustomRegion'
                                 
                                 SELECT @CustomRegionPopulationCount = count(*)
                                 FROM [dbo].[RegionPopulationStrats]
                                 WHERE RegionType = 0
                                 
                                 SELECT @CustomRegionCount as CustomRegionCount, 
                                 	@CustomRegionPopulationCount as CustomRegionPopulationCount";

                var item = session.CreateSQLQuery(query)
                    .AddScalar("CustomRegionCount", NHibernateUtil.Int32)
                    .AddScalar("CustomRegionPopulationCount", NHibernateUtil.Int32)
                    .List<object[]>().Select(r => Tuple.Create<int, int>((int)r[0], (int)r[1])).FirstOrDefault();

                return item;
            }
        }

        public bool HasImportedRegionId(string regionType, int contentItemRecordId)
        {
            var query = string.Format("SELECT count(1) - count(case when {0} is null then 1 end) FROM [dbo].[Targets_InpatientTargets] WHERE Dataset_Id = {1} ", regionType, contentItemRecordId);
            return Convert.ToBoolean(ExecuteScalar<int>(query));
        }

        public bool IsCustomRegionDefined()
        {
            using (var session = GetSession())
            {
                var query = string.Format("SELECT count(*) FROM [dbo].[Regions] where RegionType not in ('{0}', '{1}')", typeof(HealthReferralRegion).Name, typeof(HospitalServiceArea).Name);
                return session.CreateSQLQuery(query).UniqueResult<int>() > 0;
            }
        }

        public bool IsMedicareDataImported()
        {
            //using (var session = GetStatelessSession())
            //{
            var query = @"SELECT COUNT(*) FROM [dbo].[Targets_MedicareProviderChargeTargets]";
            return ExecuteScalar<int>(query) > 0;
            //}
        }

        public List<NursingHome> GetNursingHomesForWebsite(IEnumerable<string> states)
        {
            var nusingHomes = new List<NursingHome>();
            using (var session = GetStatelessSession())
            {
                states.ToList().ForEach(state => nusingHomes.AddRange(session.Query<NursingHome>()
                    .Where(ns => ns.State == state && ns.ProviderId != null && !ns.IsDeleted).ToList()));
            }
            return nusingHomes;
        }

        public ObservableCollection<string> ReportingYears
        {
            // return a list of years from 2009 up to and including the current (run-time) year
            get
            {
                const int firstYear = 2009;
                return Enumerable.Range(firstYear, DateTime.Now.Year - firstYear + 1)
                                .Select(y => y.ToString())
                                .ToObservableCollection<string>();
            }
        }

        #region private helper methods

        private IList<TModel> _getModelFromEnums<TEnum, TModel>()
        {
            return (from TEnum val in Enum.GetValues(typeof(TEnum)) select (TModel)Activator.CreateInstance(typeof(TModel), new object[] { val })).ToList();
        }

        private IList<string> _getEnumDescriptors<T>()
        {
            var list = new List<string>();

            foreach (
                var members in from object val in Enum.GetValues(typeof(T)) select typeof(T).GetMember(val.ToString())
                )
            {
                list.AddRange(
                    members.Select(memberInfo => memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false))
                           .Select(att => ((DescriptionAttribute)att[0]).Description));
            }
            return list;
        }

        //private IEnumerable<string> _getReportTemplateFilePaths()
        //{
        //    var files = new List<string>();
        //    var directory = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\BaseData\Reports");

        //    try
        //    {
        //        files.AddRange(directory.EnumerateFiles().Select(file => file.FullName));
        //    }

        //    catch (UnauthorizedAccessException uaEx)
        //    {
        //        EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(uaEx);
        //    }
        //    catch (PathTooLongException pathEx)
        //    {
        //        EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(pathEx);

        //    }

        //    return files;
        //}

        //private string _getFileContent(string path)
        //{
        //    try
        //    {
        //        using (var sr = new StreamReader(path))
        //        {

        //            var line = sr.ReadToEnd();
        //            return line;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
        //    }

        //    return null;
        //}

        private IList<string> _getReportTypes()
        {
            try
            {
                //return (from file in _getReportTemplateFilePaths()
                //        select _getFileContent(file)
                //        into result select ReportManifest.Deserialize(result)
                //        into report select report.Name).ToList();

                var factory = new ReportManifestFactory();
                return factory.InstalledManifests.Select(manifest => manifest.Name).ToList();
            }
            catch (Exception e)
            {
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e.GetBaseException());
            }

            return null;
        }

        #endregion


        public bool CheckIfCustomRegionsDefined(IEnumerable<int> datasetIds, IEnumerable<string> states, string websiteRegionType)
        {
            var result = true;

            if (datasetIds == null || !datasetIds.Any()) return result;

            var ipQuery = string.Format(@"IF ( (SELECT COUNT(DISTINCT IP.[CustomRegionId]) 
                                                FROM [dbo].[Targets_InpatientTargets] IP 
                                                WHERE  IP.[CustomRegionId] IS NOT NULL AND IP.[Dataset_Id] IN ({0}) ) > 0)
	SELECT 1;	
ELSE
	SELECT 0;", string.Join(",", datasetIds));

            using (var session = base.GetStatelessSession())
            {
                if (ExecuteScalar<int>(ipQuery) == 1)
                {
                    result = session.Query<CustomRegion>()
                                    .Any(reg => states.Contains(reg.State));

                    if (result && websiteRegionType.EqualsIgnoreCase(typeof(CustomRegion).Name))
                        result = session.Query<RegionPopulationStrats>()
                                        .Any(rps => rps.RegionType == (int)RegionTypeEnum.Custom);
                }
            }

            return result;
        }


        public bool CheckIfCustomRegionsMatchLibrary(List<int> datasetIds, List<string> states, string websiteRegionType)
        {
            var ipQuery = string.Format(@";WITH ValidIPRegions(RegionId, DatasetId) AS
(
	SELECT DISTINCT r.[ImportRegionId] 'RegionId', IP.[Dataset_Id] 'DatasetId'
				 FROM [dbo].[Targets_InpatientTargets] IP 
					LEFT JOIN [dbo].[Regions] r ON r.[ImportRegionId] = IP.[CustomRegionID] AND UPPER(r.RegionType) = 'CUSTOMREGION'
				 WHERE  IP.[CustomRegionId] IS NOT NULL AND IP.[Dataset_Id] IN ({0})
)
SELECT 
	 COUNT(CASE
		WHEN vipr.RegionId > 0 THEN 1
		ELSE 0
	END) 
FROM ValidIPRegions vipr
WHERE vipr.RegionId IS NULL 
  AND vipr.DatasetId IN ({0})", string.Join(",", datasetIds));

            return ExecuteScalar<int>(ipQuery) == 0;
        }

        public IList<Menu> GetApplicableMenuItems()
        {
            IList<Menu> menuItems = new List<Menu>();
            using (var session = base.GetStatelessSession())
            {
                menuItems = session.QueryOver<Menu>()
                                        .Where(x => x.Owner == null && x.Type != "menu-config")
                                        .List().ToList();
            }
            return menuItems.ToList();
        }
    }
}
