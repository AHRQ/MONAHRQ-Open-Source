using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using Microsoft.SqlServer;

namespace Monahrq.Infrastructure.Domain.Wings
{
    [Export(typeof(ITargetDeletionVisitor))]
    public class TargetDeletionVisitor : ITargetDeletionVisitor
    {
        public string TargetType
        {
            get { return string.Empty; }
        }

        public int Order { get { return 0; } }

        //[Import]
        //public IDomainSessionFactoryProvider DataProvider { get; set; }

        public async void Visit(Entities.Domain.IEntity entity, VisitorOptions options /*NHibernate.ISession session*/)
        {
            await Task.Delay(30000);

            var result = await Task<bool>.Run(() =>
            {
                
                if (options == null || options.DataProvider == null || entity == null) return Task.FromResult<bool>(false);
                var dataset = entity as Dataset;
                if (dataset == null) return Task.FromResult<bool>(false);
                if (dataset.ContentType == null) return Task.FromResult<bool>(false);
                var defaultTimeOut = 40000;

                var targetType = dataset.ContentType.IsCustom ? dataset.ContentType.DbSchemaName : dataset.ContentType.ClrType != null ? Type.GetType(dataset.ContentType.ClrType).EntityTableName() : string.Empty;

                if (string.IsNullOrEmpty(targetType)) Task.FromResult<bool>(false);

                string targetDeleteQuery = string.Empty;
                //var enableConstraint = string.Format("ALTER TABLE  {0} CHECK CONSTRAINT FK_TARGETS_{1}_DATASETS", targetType, targetType.Replace("Targets_", ""));
                var enableConstraint = string.Format("ALTER TABLE  {0} CHECK CONSTRAINT ALL;", targetType /*, targetType.Replace("Targets_", "")*/);

                //Checks if there is any lock on any of monahrq tables before kicking the deletion
                var count = 0;
                while (!CheckTableLock() && count < 100)
                {
                    Thread.Sleep(1000);
                    count++;
                }

                using (var sess = options.DataProvider.SessionFactory.OpenSession())
                {
                    using (var trx = sess.BeginTransaction())
                    {
                        try
                        {
                            if (!dataset.ContentType.IsCustom) // If first party wing targets execute via Hql query
                            {
                                targetDeleteQuery = string.Format("delete from {0} where Dataset_Id = {1} ", targetType, dataset.Id);

                                if (dataset.ContentType.Name.In(new string[] { "Inpatient Discharge", "ED Treat And Release" }))
                                    defaultTimeOut = 75000;

                                sess.CreateSQLQuery(targetDeleteQuery)
                                    .SetTimeout(defaultTimeOut)
                                    .ExecuteUpdate();
                            }
                            else // otherwise, if open source wing target use t-sql query
                            {
                                targetDeleteQuery = string.Format("delete from {0} where Dataset_Id = {1}", dataset.ContentType.DbSchemaName, dataset.Id);

                                sess.CreateSQLQuery(targetDeleteQuery)
                                    .SetTimeout(defaultTimeOut)
                                    .ExecuteUpdate();
                            }

                            sess.CreateSQLQuery(enableConstraint)
                                .SetTimeout(defaultTimeOut)
                                .ExecuteUpdate();

                            trx.Commit();

                            return Task.FromResult<bool>(true);
                        }
                        catch (Exception exc)
                        {
                            if (trx.IsActive)
                                trx.Rollback();

                            var excToUse = exc.GetBaseException();
                            options.Logger.Write(excToUse, System.Diagnostics.TraceEventType.Critical);

                            return Task.FromResult<bool>(false);
                        }
                    }
                }

            });


            //var worker = new BackgroundWorker();
            //worker.DoWork += (o, e) =>
            //{
            //    if (options == null || options.DataProvider == null|| entity == null) return;
            //    var dataset = entity as Dataset;
            //    if (dataset == null) return;
            //    if (dataset.ContentType == null) return;
            //    var defaultTimeOut = 40000;

            //    var targetType = Type.GetType(dataset.ContentType.ClrType).EntityTableName();

            //    string targetDeleteQuery = string.Empty;
            //    //var enableConstraint = string.Format("ALTER TABLE  {0} CHECK CONSTRAINT FK_TARGETS_{1}_DATASETS", targetType, targetType.Replace("Targets_", ""));
            //    var enableConstraint = string.Format("ALTER TABLE  {0} CHECK CONSTRAINT ALL;", targetType /*, targetType.Replace("Targets_", "")*/);
            //    using (var sess = options.DataProvider.SessionFactory.OpenSession())
            //    {
            //        using (var trx = sess.BeginTransaction())
            //        {
            //            if (!dataset.ContentType.IsCustom) // If first party wing targets execute via Hql query
            //            {
            //                targetDeleteQuery = string.Format("delete from {0} where Dataset_Id = {1} ", targetType, dataset.Id);

            //                if (dataset.ContentType.Name.In(new string[] { "Inpatient Discharge", "ED Treat And Release" }))
            //                    defaultTimeOut = 75000;

            //                sess.CreateSQLQuery(targetDeleteQuery)
            //                    .SetTimeout(defaultTimeOut)
            //                    .ExecuteUpdate();
            //            }
            //            else // otherwise, if open source wing target use t-sql query
            //            {
            //                targetDeleteQuery = string.Format("delete from {0} where Dataset_Id = {1}", dataset.ContentType.DbSchemaName, dataset.Id);

            //                sess.CreateSQLQuery(targetDeleteQuery)
            //                    .SetTimeout(defaultTimeOut)
            //                    .ExecuteUpdate();
            //            }

            //            sess.CreateSQLQuery(enableConstraint)
            //                .SetTimeout(defaultTimeOut)
            //                .ExecuteUpdate();

            //            trx.Commit();
            //        }
            //    }

            //};
            //worker.RunWorkerAsync();
        }

        public bool CheckTableLock()
        {
            var dataserviceProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();

            var query = string.Format("SELECT Count(rsc_objid) AS Kount FROM sys.syslockinfo WHERE rsc_objid != 0");

            using (var session = dataserviceProvider.SessionFactory.OpenStatelessSession())
            {
                var tableLockedCount = session.CreateSQLQuery(query)
                    .AddScalar("Kount", NHibernateUtil.String)
                    .List<string>()
                    .Select(x => Convert.ToInt32(x))
                    .FirstOrDefault();

                return tableLockedCount == 0;
            }
        }
    }
}
