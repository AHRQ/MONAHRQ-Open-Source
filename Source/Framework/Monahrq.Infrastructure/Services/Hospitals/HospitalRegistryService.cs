using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LinqKit;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Data.Transformers;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Hospitals.Mapping;
using Monahrq.Infrastructure.Types;
using Monahrq.Infrastructure.Utility;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using Remotion.Linq.Utilities;
using Category = Monahrq.Infrastructure.Domain.Categories.Category;

namespace Monahrq.Infrastructure.Services.Hospitals
{
    /// <summary>
    /// The hospital registry domain service.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Services.Hospitals.IHospitalRegistryService" />
    [Export(typeof (IHospitalRegistryService))]
    public class HospitalRegistryService : IHospitalRegistryService
    {
        /// <summary>
        /// The hospital registry
        /// </summary>
        private static HospitalRegistry _hospitalRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalRegistryService"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        [ImportingConstructor]
        public HospitalRegistryService(IDomainSessionFactoryProvider provider)
        {
            Provider = provider;

            if (_hospitalRegistry == null)
            {
                using (var session = Provider.SessionFactory.OpenStatelessSession())
                {
                    _hospitalRegistry = (from registry in session.Query<HospitalRegistry>()
                        where registry.DataVersion ==
                              (
                                  from reg2 in session.Query<HospitalRegistry>()
                                  select reg2.DataVersion
                                  ).Max()
                        select registry
                        ).FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        private IDomainSessionFactoryProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        private ILogWriter Logger { get; set; }

        /// <summary>
        /// Hospital Registry representing th latest registry available to th system
        /// </summary>
        public HospitalRegistry CurrentRegistry
        {
            get
            {
                return _hospitalRegistry;
            }
        }

        /// <summary>
        /// Generates the mapping reference.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="abbrevList">The abbrev list.</param>
        /// <param name="regionType">Type of the region.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentTypeException">regionType</exception>
        /// <exception cref="Remotion.Linq.Utilities.ArgumentTypeException">regionType</exception>
        public HospitalMappingReference GenerateMappingReference<T>(IEnumerable<string> abbrevList, Type regionType)
        {
            if (!regionType.IsSubclassOf(typeof (Region)))
                throw new ArgumentTypeException("regionType", typeof (Region), regionType);

            if (!(abbrevList ?? Enumerable.Empty<string>()).Any())
            {
                return new HospitalMappingReference(regionType);
            }
            return GenerateMappingReference(this.GetStates(abbrevList.ToArray()), regionType);
        }

        /// <summary>
        /// Generates the mapping reference.
        /// </summary>
        /// <param name="statesToReference">The states to reference.</param>
        /// <param name="regionType">Type of the region.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentTypeException">regionType</exception>
        /// <exception cref="Remotion.Linq.Utilities.ArgumentTypeException">regionType</exception>
        public HospitalMappingReference GenerateMappingReference(IEnumerable<State> statesToReference, Type regionType)
        {
            /*changed statelist.ToList() to , Possible multiple enumerations:(inga)*/
            if (!regionType.IsSubclassOf(typeof (Region)))
                throw new ArgumentTypeException("regionType", typeof (Region), regionType);

            var statesToReferenceList = (statesToReference ?? Enumerable.Empty<State>()).ToList();

            IList<CustomRegion> customRegions = new List<CustomRegion>();
            IList<HealthReferralRegion> hrrRegion = new List<HealthReferralRegion>();
            IList<HospitalServiceArea> hsaRegion = new List<HospitalServiceArea>();
            var regionResultTemp = new List<Region>();
            var hospitals = new List<Hospital>();
            var cmsLookup = new List<System.Tuple<int, string, string>>();
            using (var sess = Provider.SessionFactory.OpenSession())
            {
                foreach (var state in statesToReferenceList)
                {
                    var stateAbbrev = state.Abbreviation;

                    // Load region based off of state and selected regional context
                    customRegions = sess.CreateCriteria<CustomRegion>()
                        .Add(Restrictions.InsensitiveLike("State", stateAbbrev))
                        .List<CustomRegion>();

                    if (MonahrqContext.SelectedRegionType == typeof (HealthReferralRegion))
                    {
                        hrrRegion = sess.CreateCriteria<HealthReferralRegion>()
                            .Add(Restrictions.InsensitiveLike("State", stateAbbrev))
                            .List<HealthReferralRegion>();
                    }
                    else if (MonahrqContext.SelectedRegionType == typeof (HospitalServiceArea))
                    {
                        hsaRegion = sess.CreateCriteria<HospitalServiceArea>()
                            .Add(Restrictions.InsensitiveLike("State", stateAbbrev))
                            .List<HospitalServiceArea>();
                    }

                    // Load hospitals per state context
                    IList<Hospital> hospitalList = sess.Query<Hospital>()
                        .Where(h => h.State == stateAbbrev.ToUpper() && !h.IsArchived && !h.IsDeleted)
                        .DistinctBy(h => h.Id)
                        //.Cacheable()
                        //.CacheRegion("HospitalsPerStateContext:" + state.Abbreviation)
                        .ToList();

                    RemoveDuplicateHospitals(ref hospitalList);

                    if (hospitalList.Any())
                    {
                        hospitals.AddRange(hospitalList);
                    }

                    // Merge regions based off both state and regional context
                    if (MonahrqContext.SelectedRegionType == typeof (HealthReferralRegion) && hrrRegion.Any())
                    {
                        regionResultTemp.AddRange(hrrRegion.OfType<Region>().DistinctBy(f => f.Id).ToList());
                        regionResultTemp.AddRange(customRegions.OfType<Region>().DistinctBy(f => f.Id).ToList());
                    }
                    else if (MonahrqContext.SelectedRegionType == typeof (HospitalServiceArea) && hsaRegion.Any())
                    {
                        regionResultTemp.AddRange(hsaRegion.OfType<Region>().DistinctBy(f => f.Id).ToList());
                        regionResultTemp.AddRange(customRegions.OfType<Region>().DistinctBy(f => f.Id).ToList());
                    }
                    else
                    {
                        regionResultTemp.AddRange(customRegions.OfType<Region>().DistinctBy(f => f.Id).ToList());
                    }

                    // Set CMSProviderId lookup
                    var tempCmsLookup = sess.CreateCriteria<Hospital>()
                        .Add(Restrictions.Eq("State", stateAbbrev))
                        .Add(Restrictions.Eq("IsDeleted", false))
                        .Add(Restrictions.IsNotNull("CmsProviderID"))
                        .SetProjection(Projections.ProjectionList()
                            .Add(Projections.Alias(Projections.Property("CmsProviderID"), "CmsProviderID"))
                            .Add(Projections.Alias(Projections.Property("Id"), "Id"))
                            .Add(Projections.Alias(Projections.Property("Name"), "Name")))
                        .SetResultTransformer(new AliasToBeanResultTransformer(typeof (Hospital)))
                        .List<Hospital>();

                    if (tempCmsLookup.Any())
                    {
                        var orderedCmsLookup =
                            tempCmsLookup.Where(h => !h.IsArchived)
                                .OrderByDescending(c => c.Id)
                                .GroupBy(c => c.CmsProviderID)
                                .Select(x => x.First())
                                .ToList()
                                .OrderBy(x => x.CmsProviderID);
                        cmsLookup.AddRange(
                            orderedCmsLookup.ToList()
                                .Select(x => new System.Tuple<int, string, string>(x.Id, x.CmsProviderID, x.Name))
                                .ToList());
                    }
                }
            }

            // Order Hospitals by State then Name.
            hospitals = hospitals.OrderBy(h => h.State)
                .ThenBy(h => h.Name)
                .ToList();

            return new HospitalMappingReference(regionType, statesToReferenceList, hospitals, regionResultTemp.ToList(),
                cmsLookup);
        }

        /// <summary>
        /// Removes the duplicate hospitals.
        /// </summary>
        /// <param name="hospitals">The hospitals.</param>
        private void RemoveDuplicateHospitals(ref IList<Hospital> hospitals)
        {
            if (hospitals == null)
                return;

            var hospsToDelete = new List<Hospital>();
            var finalList = new List<Hospital>();

            foreach (Hospital currValue in hospitals)
            {
                if (!finalList.Any(h => h.Name.EqualsIgnoreCase(currValue.Name) &&
                                        h.State.ToUpper() == currValue.State.ToUpper() &&
                                        h.LocalHospitalId.EqualsIgnoreCase(currValue.LocalHospitalId) &&
                                        h.IsDeleted == currValue.IsDeleted))
                    finalList.Add(currValue);
                else
                    hospsToDelete.Add(currValue);
            }
            hospitals = new List<Hospital>(finalList);

            if (hospsToDelete != null && hospsToDelete.Any())
            {
                Task.Factory.StartNew(() => DeleteAll(hospsToDelete));
            }
        }

        /// <summary>
        /// Gets the hospital count for region.
        /// </summary>
        /// <param name="regionType">Type of the region.</param>
        /// <param name="regionId">The region identifier.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public int GetHospitalCountForRegion(string regionType, int regionId, string state)
        {
            var resultCount = 0;

            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine("with SelectedRegion (Id, Name, State) as");
            queryBuilder.AppendLine("(");
            queryBuilder.AppendLine("  select distinct r.Id , r.Name, r.State ");
            queryBuilder.AppendLine("  from  [dbo].[Regions] r ");
            queryBuilder.AppendLine("  where r.RegionType = '" + regionType + "' ");
            queryBuilder.AppendLine("	and r.Id = " + regionId + " ");
            queryBuilder.AppendLine("	and r.State = '" + state + "' ");
            queryBuilder.AppendLine(")");
            queryBuilder.AppendLine("SELECT count(h.[Id]) ");
            queryBuilder.AppendLine("FROM [dbo].[Hospitals] h ");
            queryBuilder.AppendLine("	 inner join SelectedRegion sr on sr.State = h.State ");
            queryBuilder.AppendLine("Where h.IsArchived = 0 and h.IsDeleted=0 ");
            queryBuilder.AppendLine("And (h.HealthReferralRegion_Id = sr.Id or ");
            queryBuilder.AppendLine("	   h.HospitalServiceArea_Id = sr.Id or ");
            queryBuilder.AppendLine("	   h.CustomRegion_Id = sr.Id) ");

            using (var session = Provider.SessionFactory.OpenStatelessSession())
            {
                resultCount = session.CreateSQLQuery(queryBuilder.ToString()).UniqueResult<int>();
            }

            return resultCount;
        }

        /// <summary>
        /// Gets the hospital count for category.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="regionType">Type of the region.</param>
        /// <param name="stateIds">The state ids.</param>
        /// <returns></returns>
        public int GetHospitalCountForCategory(int categoryId, string regionType, string[] stateIds)
        {
            int resultCount;

            var stateIdsString = stateIds.ToArray()
                .Aggregate<string, string>(null, (current, stateId) => current + string.Format("'{0}',", stateId));

            if (stateIdsString.EndsWith(","))
                stateIdsString = stateIdsString.SubStrBeforeLast(",");

            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine("with SelectedRegion (Id, Name, State) as");
            queryBuilder.AppendLine("(");
            queryBuilder.AppendLine("  select distinct r.Id , r.Name, r.State ");
            queryBuilder.AppendLine("  from  [dbo].[Regions] r ");
            queryBuilder.AppendLine("  where r.RegionType = '" + regionType + "' ");
            queryBuilder.AppendLine("	and r.State in (" + stateIdsString + ") ");
            queryBuilder.AppendLine(")");
            queryBuilder.AppendLine("SELECT count(h.[Id]) ");
            queryBuilder.AppendLine("FROM [dbo].[Hospitals] h ");
            queryBuilder.AppendLine("	 inner join SelectedRegion sr on sr.State = h.State ");
            queryBuilder.AppendLine("	 inner join Hospitals_HospitalCategories hc on hc.Hospital_Id = h.Id ");
            queryBuilder.AppendLine("Where h.IsArchived = 0 and h.IsDeleted=0 ");
            queryBuilder.AppendLine("and hc.Category_Id = " + categoryId + " ");
            queryBuilder.AppendLine("And (h.HealthReferralRegion_Id = sr.Id or ");
            queryBuilder.AppendLine("	   h.HospitalServiceArea_Id = sr.Id or ");
            queryBuilder.AppendLine("	   h.CustomRegion_Id = sr.Id) ");

            using (var session = Provider.SessionFactory.OpenStatelessSession())
            {
                resultCount = session.CreateSQLQuery(queryBuilder.ToString()).UniqueResult<int>();
            }

            return resultCount;
        }

        /// <summary>
        /// Create a transient region and returns to client for modification
        /// </summary>
        /// <returns></returns>
        public CustomRegion CreateRegion()
        {
            using (var session = Provider.SessionFactory.OpenStatelessSession())
            {
                var registry = session.Get<HospitalRegistry>(CurrentRegistry.Id);
                return new CustomRegion(registry);
            }
        }

        /// <summary>
        /// Create a transient hospital and returns to client for modification
        /// </summary>
        /// <returns></returns>
        public Hospital CreateHospital()
        {
            return new Hospital(CurrentRegistry);
        }

        /// <summary>
        /// Creates the hospital archive.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <returns></returns>
        public Hospital CreateHospitalArchive(Hospital hospital)
        {
            Hospital archivedhospital;
            using (var session = Provider.SessionFactory.OpenSession())
            {
                var hospital2Archive = session.Query<Hospital>()
                    .FirstOrDefault(entity => entity.Id == hospital.Id);

                if (hospital2Archive == null) return null;

                session.Evict(hospital);

                archivedhospital = hospital2Archive.Archive();
            }

            return archivedhospital;
        }

        /// <summary>
        /// Saves the given current region
        /// </summary>
        /// <param name="region"></param>
        public void Save(CustomRegion region)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    try
                    {
                        if (!region.IsPersisted)
                            session.SaveOrUpdate(region);
                        else
                            session.Merge(region);

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Saves the specified region population strats.
        /// </summary>
        /// <param name="regionPopulationStrats">The region population strats.</param>
        public void Save(RegionPopulationStrats regionPopulationStrats)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    try
                    {
                        if (!regionPopulationStrats.IsPersisted)
                            session.SaveOrUpdate(regionPopulationStrats);
                        else
                            session.Merge(regionPopulationStrats);

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <returns></returns>
        public State GetState(Region region)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                return session.Query<State>().FirstOrDefault(r => r.Abbreviation.ToUpper() == region.State.ToUpper());
            }
        }

        /// <summary>
        /// Gets the states.
        /// </summary>
        /// <param name="abbrevs">The abbrevs.</param>
        /// <param name="returnAllStatesIfNoAbbrevs">if set to <c>true</c> [return all states if no abbrevs].</param>
        /// <returns></returns>
        public IEnumerable<State> GetStates(string[] abbrevs, bool returnAllStatesIfNoAbbrevs = true)
        {
            if (!returnAllStatesIfNoAbbrevs && (abbrevs == null || abbrevs.Length == 0))
                return new List<State>();

            var crit = abbrevs == null || abbrevs.Length == 0
                ? PredicateBuilder.True<State>()
                : PredicateBuilder.False<State>();

            var cacheRegion = "Hospitals:AvailableStates";
            if (abbrevs != null)
            {
                crit = abbrevs.Aggregate(crit, (current, abbrev) => current.Or(state => state.Abbreviation == abbrev));

                if (abbrevs.Any())
                {
                    cacheRegion += ":";
                    cacheRegion = abbrevs.Aggregate(cacheRegion, (current, abbrev) => current + abbrev);
                }
            }

            using (var session = Provider.SessionFactory.OpenSession())
            {
                // TODO: OPTIMIZE THIS. TOO SLOW.
                return session.Query<State>()
                    .Cacheable()
                    .CacheRegion(cacheRegion)
                    .Where(crit).ToList();
            }
        }

        /// <summary>
        /// Gets the states abbreviations.
        /// </summary>
        /// <param name="abbrevs">The abbrevs.</param>
        /// <returns></returns>
        public IEnumerable<string> GetStatesAbbreviations(params string[] abbrevs)
        {
            var crit = abbrevs == null || abbrevs.Length == 0
                ? PredicateBuilder.True<State>()
                : PredicateBuilder.False<State>();

            //var cacheRegion = "StateAbbreviations";
            if (abbrevs != null)
            {
                crit = abbrevs.Aggregate(crit, (current, abbrev) => current.Or(state => state.Abbreviation == abbrev));

                //if (abbrevs.Any())
                //{
                //    cacheRegion += ":";
                //    cacheRegion = abbrevs.Aggregate(cacheRegion, (current, abbrev) => current + abbrev);
                //}
            }

            using (var session = Provider.SessionFactory.OpenSession())
            {
                return session.Query<State>()
                    //.Cacheable()
                    //.CacheRegion(cacheRegion)
                    .Where(crit).Select(s => s.Abbreviation).ToList();
            }
        }

        /// <summary>
        /// Gets the counties.
        /// </summary>
        /// <param name="states"></param>
        /// <returns></returns>
        public IEnumerable<County> GetCounties(string[] states)
        {
            var resultList = new List<County>();
            var querySpec =
                new Func<County, bool>(c => states.Any(s => c.State != null && s.ToLower() == c.State.ToLower()));


            var cacheRegion = "CountiesByStates";
            if (MonahrqContext.ReportingStatesContext != null && MonahrqContext.ReportingStatesContext.Any())
            {
                cacheRegion += ":";
                cacheRegion = MonahrqContext.ReportingStatesContext.Aggregate(cacheRegion,
                    (current, state) => current + state);
            }

            using (var session = Provider.SessionFactory.OpenSession())
            {
                // BUG: ReportingStatesContext is currently only being set by the user in the ContextView (which happens *after* this ViewModel ctor),
                // but ReportingStatesContext should be persistent when the user restarts Monahrq.
                resultList = session.Query<County>()
                    .Cacheable()
                    .CacheRegion(cacheRegion)
                    .Where(querySpec).ToList();
            }
            return resultList;
        }

        /// <summary>
        /// Saves the given current region
        /// </summary>
        /// <param name="hospital"></param>
        public void Save(Hospital hospital)
        {
            hospital.CleanBeforeSave();
            using (var session = Provider.SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    if (hospital.Version == null)
                    {
                        hospital.Version = CurrentRegistry.DataVersion;
                    }

                    session.Evict(hospital);

                    if (!hospital.IsPersisted)
                        session.SaveOrUpdate(hospital);
                    else
                        session.Merge(hospital);

                    session.Flush();

                    tx.Commit();
                }
            }
        }

        /// <summary>
        /// Saves the asynchronous.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <param name="completeCallback">The complete callback.</param>
        /// <returns></returns>
        public async Task<bool> SaveAsync(Hospital hospital, Action<OperationResult<Hospital>> completeCallback)
        {
            return await Task.Run(() =>
            {
                hospital.CleanBeforeSave();
                using (var session = Provider.SessionFactory.OpenSession())
                {
                    using (var tx = session.BeginTransaction())
                    {
                        try
                        {
                            if (hospital.Version == null)
                            {
                                hospital.Version = CurrentRegistry.DataVersion;
                            }

                            session.Evict(hospital);

                            if (!hospital.IsPersisted)
                                session.SaveOrUpdate(hospital);
                            else
                                session.Merge(hospital);

                            session.Flush();

                            tx.Commit();

                            completeCallback(new OperationResult<Hospital> {Model = hospital, Status = true});

                            return true;

                        }
                        catch (Exception exc)
                        {
                            tx.Rollback();

                            completeCallback(new OperationResult<Hospital>
                            {
                                Exception = exc.GetBaseException(),
                                Status = false
                            });

                            return false;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Saves the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        public void Save(Category category)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    try
                    {
                        if (category.Version == null)
                        {
                            category.Version = CurrentRegistry.DataVersion;
                        }

                        if (!category.IsPersisted)
                            session.SaveOrUpdate(category);
                        else
                            session.Merge(category);

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        public void Delete(Category category)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.Evict(category);

                    session.Delete(category);

                    session.Flush();

                    tx.Commit();
                }
            }
        }

        /// <summary>
        /// Deletes the specified region.
        /// </summary>
        /// <param name="region">The region.</param>
        public void Delete(Region region)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.Evict(region);

                    session.Delete(region);

                    session.Flush();

                    tx.Commit();
                }
            }
        }

        /// <summary>
        /// Deletes the specified hospital.
        /// </summary>
        /// <param name="hospitals">The hospitals.</param>
        public void DeleteAll(IEnumerable<Hospital> hospitals)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {

                using (var tx = session.BeginTransaction())
                {

                    foreach (var hospital in hospitals)
                    {
                        try
                        {
                            if (!session.Query<Hospital>().Any(h => h.Id == hospital.Id)) continue;

                            session.Evict(hospital);

                            if (hospital.IsSourcedFromBaseData && hospital.IsArchived)
                            {
                                hospital.IsDeleted = true;
                                session.Merge(hospital);
                            }
                            else if (hospital.IsSourcedFromBaseData && !hospital.IsArchived)
                            {
                                hospital.IsArchived = true;
                                hospital.ArchiveDate = DateTime.Now;
                                session.Merge(hospital);
                            }
                            else
                            {
                                if (hospital.Categories.Any())
                                {
                                    hospital.Categories.Clear();
                                }
                                hospital.IsDeleted = true;
                                session.Merge(hospital);
                            }
                        }
                        catch
                            (InvalidConstraintException)
                        {
                            hospital.IsDeleted = true;
                            Save(hospital);
                        }
                        catch
                            (ConstraintException)
                        {
                            hospital.IsDeleted = true;
                            Save(hospital);
                        }
                        catch (Exception ex)
                        {
                            HandleException(ex, "HospitalRegistryService.Delete", hospital.Name,
                                typeof (Hospital).Name);
                        }
                    }
                    session.Flush();

                    tx.Commit();
                }

            }
        }


        /// <summary>
        /// Deletes the specified hospital.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        public void Delete(Hospital hospital)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                try
                {
                    using (var tx = session.BeginTransaction())
                    {
                        session.Evict(hospital);

                        if (hospital.IsSourcedFromBaseData && hospital.IsArchived)
                        {
                            hospital.IsDeleted = true;
                            session.Merge(hospital);
                        }
                        else if (hospital.IsSourcedFromBaseData && !hospital.IsArchived)
                        {
                            hospital.IsArchived = true;
                            hospital.ArchiveDate = DateTime.Now;
                            session.Merge(hospital);
                        }
                        else
                        {
                            if (hospital.Categories.Any())
                            {
                                hospital.Categories.Clear();
                            }
                            hospital.IsDeleted = true;
                            session.Merge(hospital);
                        }

                        session.Flush();

                        tx.Commit();
                    }
                }
                catch (InvalidConstraintException)
                {
                    hospital.IsDeleted = true;
                    Save(hospital);
                }
                catch (ConstraintException)
                {
                    hospital.IsDeleted = true;
                    Save(hospital);
                }
                catch (Exception ex)
                {
                    HandleException(ex, "HospitalRegistryService.Delete", hospital.Name, typeof (Hospital).Name);
                }
            }
        }

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="method">The method.</param>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="entityType">Type of the entity.</param>
        public void HandleException(Exception exception, string method, string entityName, string entityType)
        {
            if (Logger == null)
            {
                Logger = ServiceLocator.Current.GetInstance<ILogWriter>(LogNames.Session);
            }
            /*Logerror*/
            Logger.Write(exception, TraceEventType.Error, new Dictionary<string, string>
            {
                {"Method", method},
                {"Enitity Type", entityType},
                {"Name", entityName}
                //, {"ID", entityDescriptor.Id}
            });

            /*Publish Error*/
            ServiceLocator.Current.GetInstance<IEventAggregator>()
                .GetEvent<ServiceErrorEvent>()
                .Publish(new ServiceErrorEventArgs(exception, entityType, entityName));
        }

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public IEnumerable<HospitalCategoryDto> GetCategories(Expression<Func<HospitalCategory, bool>> criteria = null)
        {
            IEnumerable<HospitalCategoryDto> categories = new List<HospitalCategoryDto>();

            using (var session = Provider.SessionFactory.OpenSession())
            {
                if (criteria == null)
                {
                    categories = session.Query<HospitalCategory>()
                        .Select(cat => new HospitalCategoryDto {Category = cat})
                        .ToList();
                }
                else
                {
                    categories = session.Query<HospitalCategory>()
                        .Where(criteria)
                        .Select(cat => new HospitalCategoryDto {Category = cat})
                        .ToList();
                }
            }

            return categories;
        }

        // TODO: optimize. This method is called many times when every HospitalViewModel is constructed.
        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <returns></returns>
        public IEnumerable<Category> GetCategories(Hospital hospital)
        {
            // when user clicks Add, the new hospital is a transient object and won't have categories yet
            if (!hospital.IsPersisted) return new List<Category>();

            return hospital.Categories.ToList();
        }

        /// <summary>
        /// Gets the category hospital counts.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns></returns>
        public IEnumerable<HospitalCategoryDto> GetCategoryHospitalCounts(IEnumerable<HospitalCategory> categories)
        {
            var crit = PredicateBuilder.False<HospitalCategory>();
            categories.ToList().ForEach(cat => crit = crit.Or(test => test.Id == cat.Id));

            var categoryList = new List<HospitalCategoryDto>();
            // TODO: optimize this. It's very slow to open the in Datasets | Hospitals | Categories tab view
            using (var session = Provider.SessionFactory.OpenSession())
            {
                categoryList = session.Query<HospitalCategory>()
                    //.Cacheable()
                    //.CacheRegion("HospitalCategoriesPerCriteria")
                    //.CacheMode(CacheMode.Refresh)
                    .Where(crit)
                    .Select(cat => new HospitalCategoryDto {Category = cat}).ToList();
            }

            return categoryList;
        }

        /// <summary>
        /// Saves the specified selected categories.
        /// </summary>
        /// <param name="selectedCategories">The selected categories.</param>
        /// <param name="selectedHospitals">The selected hospitals.</param>
        public void Save(IEnumerable<HospitalCategory> selectedCategories, IEnumerable<Hospital> selectedHospitals)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    var hospitals = selectedHospitals as IList<Hospital> ?? selectedHospitals.ToList();
                    var hospitalCategories = selectedCategories as IList<HospitalCategory> ??
                                             selectedCategories.ToList();
                    foreach (var hospital in hospitals)
                    {
                        hospital.Categories.Clear();
                        foreach (var category in hospitalCategories.ToList())
                        {
                            if (hospital.Categories.All(hc => hc.Id != category.Id))
                                hospital.Categories.Add(category);
                        }

                        if (!hospital.IsPersisted)
                            session.SaveOrUpdate(hospital);
                        else
                            session.Merge(hospital);
                    }
                    
                    tx.Commit();
                }
            }
        }

        /// <summary>
        /// Creates the hospital category.
        /// </summary>
        /// <param name="newCategoryName">New name of the category.</param>
        /// <returns></returns>
        public HospitalCategory CreateHospitalCategory(string newCategoryName)
        {
            return new HospitalCategory(CurrentRegistry)
            {
                Name = newCategoryName
            };
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="aggregate">The aggregate.</param>
        /// <returns></returns>
        public T Get<TEntity, T>(int id, Expression<Func<TEntity, T>> aggregate)
            where TEntity : IEntity<int>
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                var result = session.Query<TEntity>()
                    .Where(entity => entity.Id == id)
                    .Select(entity => aggregate.Invoke(entity)).FirstOrDefault();
                return result;
            }
        }

        /// <summary>
        /// Gets the specified query spec.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="querySpec">The query spec.</param>
        /// <returns></returns>
        public TEntity Get<TEntity>(Expression<Func<TEntity, bool>> querySpec)
            where TEntity : class, IEntity<int>
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                return session.Query<TEntity>().FirstOrDefault(querySpec);
            }
        }

        /// <summary>
        /// Get2s the specified query spec.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="querySpec">The query spec.</param>
        /// <returns></returns>
        public TEntity Get2<TEntity>(Expression<Func<TEntity, bool>> querySpec)
            where TEntity : class, IEntity<int>
        {

            using (var session = Provider.SessionFactory.OpenStatelessSession())
            {
                return session.QueryOver<TEntity>().Where(querySpec).SingleOrDefault<TEntity>();
            }
        }

        /// <summary>
        /// Refreshes the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        public void Refresh<T>(T entity)
            where T : IEntity<int>
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                session.Refresh(entity);
            }
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public TEntity Get<TEntity>(int id) where TEntity : IEntity<int>
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                return session.Get<TEntity>(id);
            }
        }

        /// <summary>
        /// Gets the state by zip code.
        /// </summary>
        /// <param name="zipCode">The zip code.</param>
        /// <returns></returns>
        public State GetStateByZipCode(string zipCode)
        {
            State state = null;
            using (var session = Provider.SessionFactory.OpenStatelessSession())
            {
                state = session.CreateCriteria<ZipCodeToHRRAndHSA>()
                    .Add(Restrictions.Eq("Zip", zipCode))
                    .SetProjection(Projections.Property("State"))
                    .UniqueResult<State>();
            }
            return state;
        }

        /// <summary>
        /// Gets the regions by zip code.
        /// </summary>
        /// <param name="zipCode">The zip code.</param>
        /// <returns></returns>
        public IDictionary<string, Region> GetRegionsByZipCode(string zipCode)
        {
            var regionResults = new Dictionary<string, Region>();

            using (var session = Provider.SessionFactory.OpenStatelessSession())
            {
                var firstOrDefault = session.CreateCriteria<ZipCodeToHRRAndHSA>()
                    .Add(Restrictions.Eq("Zip", zipCode))
                    .SetProjection(Projections.ProjectionList()
                        .Add(Projections.Property("Zip"), "Item1")
                        .Add(Projections.Property("HRRNumber"), "Item2")
                        .Add(Projections.Property("HSANumber"), "Item3"))
                    .SetResultTransformer(
                        new DelegateTransformer<System.Tuple<string, int?, int?>>(
                            x =>
                                new System.Tuple<string, int?, int?>(x[0].ToString(), int.Parse(x[1].ToString()),
                                    int.Parse(x[2].ToString()))))
                    .FutureValue<System.Tuple<string, int?, int?>>();

                if (firstOrDefault == null) return regionResults;
                
                var hrr = session.CreateCriteria<HealthReferralRegion>()
                    .Add(Restrictions.Eq("ImportRegionId", firstOrDefault.Value.Item2))
                    .FutureValue<HealthReferralRegion>();

                regionResults.Add("HRR", hrr.Value);
                
                var hsa = session.CreateCriteria<HospitalServiceArea>()
                    .Add(Restrictions.Eq("ImportRegionId", firstOrDefault.Value.Item3))
                    .FutureValue<HospitalServiceArea>();

                regionResults.Add("HSA", hsa.Value);
            }

            return regionResults;
        }

        /// <summary>
        /// Gets the state of the county by fips and.
        /// </summary>
        /// <param name="fipsCounty">The fips county.</param>
        /// <param name="stateAbrevation">The state abrevation.</param>
        /// <returns></returns>
        public County GetCountyByFIPSAndState(string fipsCounty, string stateAbrevation)
        {
            County result;
            var querySpec =
                new Func<County, bool>(
                    c =>
                        c.CountyFIPS.ToLower() == fipsCounty.ToLower() && c.State.ToLower() == stateAbrevation.ToLower());

            using (var session = Provider.SessionFactory.OpenSession())
            {
                result = session.Query<County>()
                    //.Cacheable()
                    //.CacheRegion(cacheRegion)
                    .SingleOrDefault(querySpec);
            }
            return result;
        }

        /// <summary>
        /// Applies the categories to hospitals.
        /// </summary>
        /// <param name="selectedCategories">The selected categories.</param>
        /// <param name="selectedHospitals">The selected hospitals.</param>
        public void ApplyCategoriesToHospitals(IEnumerable<Category> selectedCategories,
            IEnumerable<Hospital> selectedHospitals)
        {
            var hospCrit = PredicateBuilder.False<Hospital>();
            selectedHospitals.ToList().ForEach(hospital => hospCrit = hospCrit.Or(h => hospital.Id == h.Id));
            var catCrit = PredicateBuilder.False<HospitalCategory>();
            selectedCategories.ToList().ForEach(cat => catCrit = catCrit.Or(c => cat.Id == c.Id));
            using (var session = Provider.SessionFactory.OpenSession())
            {
                var tx = session.BeginTransaction();
                try
                {
                    var categories = session.Query<HospitalCategory>()
                        .Where(catCrit).ToList();

                    (from hospital in session.Query<Hospital>().Where(hospCrit)
                        select hospital).ToList().ForEach(hospital =>
                                                            {
                                                                hospital.Categories.Clear();
                                                                categories.ForEach(cat => hospital.Categories.Add(cat));
                                                                session.SaveOrUpdate(hospital);
                                                            });
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the latest cost to charge ratio.
        /// </summary>
        /// <param name="proividerId">The proivider identifier.</param>
        /// <returns></returns>
        public decimal? GetLatestCostToChargeRatio(string proividerId)
        {
            if (string.IsNullOrEmpty(proividerId)) return null;

            CostToCharge ccr;
            using (var session = Provider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    ccr = session.Query<CostToCharge>()
                        .Where(x => x.ProviderID.ToLower() == proividerId.ToLower())
                        .OrderByDescending(x => x.Year)
                        .FirstOrDefault();
                    trans.Commit();
                }
            }
            return ccr != null ? (decimal?) Convert.ToDecimal(ccr.Ratio) : null;
        }

        /// <summary>
        /// Gets the custom region to population mapping count.
        /// </summary>
        /// <param name="states"></param>
        /// <returns></returns>
        public int GetCustomRegionToPopulationMappingCount(IEnumerable<string> states)
        {
            var stateClause = string.Join(",", states.ToList().Select(s => "'" + s + "'"));

            using (var session = Provider.SessionFactory.OpenStatelessSession())
            {
                var query = string.Format(@"SELECT count(DISTINCT(hr.Id))
                                 FROM [Regions] hr JOIN [{0}] hrp on hr.ImportRegionId = hrp.RegionID and hrp.RegionType= 0
                                 WHERE hr.[State] in ({1}) and UPPER(hr.[RegionType]) = 'CUSTOMREGION';",
                    typeof (RegionPopulationStrats).EntityTableName(), stateClause);

                var count = session.CreateSQLQuery(query)
                    .UniqueResult<int>();

                return count;
            }
        }

        /// <summary>
        /// Gets the regions.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="states">The states.</param>
        /// <param name="selectedRegionType">Type of the selected region.</param>
        /// <param name="includeCounts">if set to <c>true</c> [include counts].</param>
        /// <returns></returns>
        public IList<Region> GetRegions(ISession session, IEnumerable<string> states, Type selectedRegionType,
            bool includeCounts = true)
        {
            var finalRegionList = new List<Region>();

            foreach (var item in states.ToList())
            {
                var regionResultTemp = new List<Region>();
                var regionCustomTemp = new List<Region>();

                var state = item;

                if (selectedRegionType == typeof (HealthReferralRegion))
                {
                    var hrr = session.Query<HealthReferralRegion>().Where(x => state == x.State)
                        //.Cacheable()
                        //.CacheRegion("HealthReferralRegion:" + state)
                        .DistinctBy(x => x.Id)
                        .ToList();
                    if (includeCounts)
                        hrr.ForEach(
                            c =>
                                c.HospitalCount =
                                    session.CreateSQLQuery(
                                        string.Format(
                                            "select distinct(count(h.Id)) from {0} h where h.HealthReferralRegion_Id = {1} AND h.IsDeleted =0",
                                            typeof (Hospital).EntityTableName(), c.Id)).UniqueResult<int>());

                    regionResultTemp.AddRange(hrr);
                }
                else if (selectedRegionType == typeof (HospitalServiceArea))
                {
                    var hsa = session.Query<HospitalServiceArea>().Where(x => state == x.State)
                        //.Cacheable()
                        //.CacheRegion("HospitalServiceArea:" + state)
                        .DistinctBy(x => x.Id)
                        .ToList();
                    if (includeCounts)
                        hsa.ForEach(
                            c =>
                                c.HospitalCount =
                                    session.CreateSQLQuery(
                                        string.Format(
                                            "select distinct(count(h.Id)) from {0} h where h.HospitalServiceArea_Id = {1} AND h.IsDeleted =0",
                                            typeof (Hospital).EntityTableName(), c.Id)).UniqueResult<int>());

                    regionResultTemp.AddRange(hsa);
                }
                finalRegionList.AddRange(regionResultTemp);

                // Now Custom Regions
                regionCustomTemp.AddRange(session.Query<CustomRegion>().Where(x => state == x.State)
                    //.Cacheable()
                    //.CacheRegion("CustomRegion:" + state)
                    .DistinctBy(x => x.Id)
                    .ToList());
                if (includeCounts)
                    regionCustomTemp.ForEach(
                        c =>
                            c.HospitalCount =
                                session.CreateSQLQuery(
                                    string.Format(
                                        "select distinct(count(h.Id)) from {0} h where h.CustomRegion_Id = {1} AND h.IsDeleted =0",
                                        typeof (Hospital).EntityTableName(), c.Id)).UniqueResult<int>());

                finalRegionList.AddRange(regionCustomTemp);
            }

            return finalRegionList.DistinctBy(x => x.Id).ToList();
        }

        /// <summary>
        /// Rollbacks the custom hospital to base hospital.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <returns></returns>
        public Hospital RollbackCustomHospitalToBaseHospital(Hospital hospital)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                        session.Evict(hospital);

                        var baseHospital =
                            session.Query<Hospital>()
                                .Where(h => h.CmsProviderID == hospital.CmsProviderID && h.IsSourcedFromBaseData)
                                .Distinct()
                                .ToList()
                                .FirstOrDefault();

                        if (baseHospital == null)
                            return null;

                        hospital.IsDeleted = true;
                        baseHospital.IsArchived = false;
                        baseHospital.LinkedHospitalId = null;
                        baseHospital.ArchiveDate = null;

                        Save(hospital);
                        baseHospital = session.Merge(baseHospital);

                        session.Flush();
                        trans.Commit();

                    return baseHospital;

                }
            }
        }
    }
}
