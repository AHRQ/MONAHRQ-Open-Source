using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Services.Contracts;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace Monahrq.Sdk.Services
{
    public interface IDatasetService : IDataServiceBase
    {
        /// <summary>
        /// Cleans the un finished datasets.
        /// </summary>
        void CleanUnFinishedDatasets();
        /// <summary>
        /// Gets the installed datasets.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IDatasetWing> GetInstalledDatasets();

        /// <summary>
        /// Gets the imported data sets.
        /// </summary>
        /// <param name="datasetType">Type of the dataset.</param>
        /// <returns></returns>
        IEnumerable<KeyValuePair<int, string>> GetImportedDataSets(int? datasetType);

        /// <summary>
        /// Deletes the specified target type name.
        /// </summary>
        /// <param name="targetTypeName">Name of the target type.</param>
        /// <param name="datasetEntry">The dataset entry.</param>
        void Delete(string targetTypeName, Dataset datasetEntry);

        /// <summary>
        /// Gets the websites for dataset.
        /// </summary>
        /// <param name="datasetRecordId">The dataset record identifier.</param>
        /// <returns></returns>
        IList<String> GetWebsitesForDataset(int datasetRecordId);

        /// <summary>
        /// Gets the dataset targets.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, Target> GetDatasetTargets();
    }

    [Export(typeof(IDatasetService))]
    public class DatasetService : DataServiceBase, IDatasetService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetService"/> class.
        /// </summary>
        public DatasetService()
        {

        }

        /// <summary>
        /// Gets the installed datasets.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IDatasetWing> GetInstalledDatasets()
        {
            return ServiceLocator.Current.GetAllInstances<IDatasetWing>();
        }

        /// <summary>
        /// Gets the data sets.
        /// </summary>
        /// <param name="datasetType">Type of the dataset.</param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<int, string>> GetImportedDataSets(int? datasetType)
        {
            using (var session = GetSession())
            {
                var query = session.Query<Dataset>();

                if (datasetType.HasValue)
                {
                    query = query.Where(d => d.ContentType.Id == datasetType.Value);
                }
                return query.ToFuture().Select(ds => new KeyValuePair<int, string>(ds.Id, string.Format("{0} ({1})", ds.ReportingQuarter, ds.DateImported, ds))).ToList();
            }
        }

        public void CleanUnFinishedDatasets()
        {
            using (var session = base.GetStatelessSession())
            {
                var datasetsToDelete = session.Query<Dataset>()
                                              .Where(x => !x.IsFinished)
                                              .Select(x => new { x.Id, x.ContentType.Name })
                                              .ToDictionary(x => x.Id);

                foreach (var dataset in datasetsToDelete)
                {
                    var targetTypeClrName = session.CreateCriteria<Target>()
                                                   .Add(Restrictions.InsensitiveLike("Name", dataset.Value.ToString()))
                                                   .SetProjection(Projections.Property<Target>(f => f.ClrType))
                                                   .UniqueResult<string>();

                    if (string.IsNullOrEmpty(targetTypeClrName)) continue;

                    var targetType = Type.GetType(targetTypeClrName);

                    if (targetType == null) continue;

                    var targetTypeName = targetType.Name;

                    var datasetEntryId = dataset.Key;

                    using (var trans = session.BeginTransaction())
                    {
                        // HQL queries
                        var targetDelete = string.Format("delete from {0} o where o.Dataset.Id = {1}",
                                                         targetType.Name, datasetEntryId);
                        //var transactionType = typeof(ContentPartTransactionRecord);
                        var transactionDelete =
                            string.Format(
                                "delete from DatasetTransactionRecord ct where ct.Dataset.Id = {0}",
                                datasetEntryId);

                        // does this need entity.Dataset.Id or Dataset.Id  ??
                        //string summaryDelete = null;
                        //if (session.Query<Dataset>().Any(x => x.Id == datasetEntryId && x.Summary != null))
                        //{
                        //    var datasetEntrySummaryId = session.CreateCriteria<Dataset>()
                        //                                       .Add(Restrictions.Eq("Id", datasetEntryId))
                        //                                       .SetProjection(Projections.Property<Dataset>(x => x.Summary.Id))
                        //                                       .UniqueResult<int>();

                        //    summaryDelete = string.Format("delete from ContentItemSummaryRecord s where s.Id = {0}", datasetEntrySummaryId);
                        //}

                        var importDelete = string.Format("delete from Dataset d where d.Id = {0}", datasetEntryId);

                        // Delete Target data first.
                        session.CreateQuery(targetDelete)
                               .SetTimeout(500)
                               .ExecuteUpdate();

                        // Delete Transactional record
                        session.CreateQuery(transactionDelete)
                               .SetTimeout(500)
                               .ExecuteUpdate();

                        // deletion of HospitalCompareTarget_Footnote data
                        if (targetTypeName.EqualsIgnoreCase("HospitalCompareTarget"))
                        {
                            var hospitalCompareFootnotesDeleteHql = string.Format("DELETE FROM HospitalCompareFootnote hc WHERE hc.Dataset.Id = {0}", datasetEntryId);
                            session.CreateQuery(hospitalCompareFootnotesDeleteHql)
                                   .SetTimeout(300)
                                   .ExecuteUpdate();
                        }
                        // finally delete content item record.
                        session.CreateQuery(importDelete)
                               .SetTimeout(300)
                               .ExecuteUpdate();

                        //// now delete summary record.
                        //if (!string.IsNullOrEmpty(summaryDelete))
                        //{
                        //    session.CreateQuery(summaryDelete)
                        //           .SetTimeout(300)
                        //           .ExecuteUpdate();
                        //}

                        trans.Commit();
                    }
                }
            }
        }

        public void Delete(string targetTypeName, Dataset datasetEntry)
        {
            using (var session = base.GetSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    session.Evict(datasetEntry);

                    // HQL queries
                    var targetDelete = string.Format("delete from {0} o where o.Dataset.Id = {1}",
                                                     targetTypeName, datasetEntry.Id);
                    //var transactionType = typeof(ContentPartTransactionRecord);
                    var transactionDelete =
                        string.Format(
                            "delete from DatasetTransactionRecord ct where ct.Dataset.Id = {0}",
                            datasetEntry.Id);

                    //// does this need entity.Dataset.Id or Dataset.Id  ??
                    //string summaryDelete = null;
                    //if (datasetEntry.Summary != null)
                    //{
                    //    summaryDelete = string.Format("delete from ContentItemSummaryRecord s where s.Id = {0}",
                    //                                  datasetEntry.Summary.Id);
                    //}

                    var importDelete = string.Format("delete from Dataset c where c.Id = {0}", datasetEntry.Id);

                    // Delete Target data first.
                    session.CreateQuery(targetDelete)
                           .SetTimeout(5000)
                           .ExecuteUpdate();

                    // Delete Transactional record
                    session.CreateQuery(transactionDelete)
                           .SetTimeout(5000)
                           .ExecuteUpdate();

                    // deletion of HospitalCompareTarget_Footnote data
                    if (targetTypeName == "HospitalCompareTarget")
                    {
                        var hospitalCompareFootnotesDeleteHql = string.Format("DELETE FROM HospitalCompareFootnote hc WHERE hc.Dataset.Id = {0}", datasetEntry.Id);
                        session.CreateQuery(hospitalCompareFootnotesDeleteHql)
                           .SetTimeout(3000)
                           .ExecuteUpdate();
                    }

                    if (!datasetEntry.IsReImport)
                    {
                        // finally delete content item record.
                        session.CreateQuery(importDelete)
                            .SetTimeout(3000)
                            .ExecuteUpdate();
                    }
                    //// now delete summary record.
                    //if (!string.IsNullOrEmpty(summaryDelete))
                    //{
                    //    session.CreateQuery(summaryDelete)
                    //           .SetTimeout(300)
                    //           .ExecuteUpdate();
                    //}

                    trans.Commit();
                }
            }
        }

        public IList<String> GetWebsitesForDataset(int datasetRecordId)
        {
            using (var session = GetStatelessSession())
            {
                return session.Query<Website>()
                              .Where(x => x.Datasets.Any(wm => wm.Dataset.Id == datasetRecordId))
                              .Select(w => w.Name).ToList();
            }
        }

        public IDictionary<string, Target> GetDatasetTargets()
        {
            using (var sess = GetSession())
            {
                //return sess.Query<Target>().Where(t => !t.IsReferenceTarget)
                //                           .OrderBy(x => x.DisplayOrder)
                //                           .ToFuture()
                //                           //.Select(t => new Target(null, t.Guid, t.Name) { ClrType = t.ClrType, IsDisabled = t.IsDisabled, IsCustom = t.IsCustom, DisplayOrder = t.DisplayOrder})
                //                           .ToDictionary(t => t.Name);
                return sess.Query<Target>().Where(t => !t.IsReferenceTarget)
                                                          .OrderBy(x => x.DisplayOrder)
                                                          .ThenBy(d => d.Name)
                                                          .ToFuture()
                                                          .Select(x => new Target()
                                                          {
                                                              Id = x.Id,
                                                              Guid = x.Guid,
                                                              Name = x.Name,
                                                              DisplayName = x.DisplayName,
                                                              ClrType = x.ClrType,
                                                              Description = x.Description,
                                                              IsCustom = x.IsCustom, 
                                                          })
                                                          .ToDictionary(t => t.Name);


            }
        }
    }
}
