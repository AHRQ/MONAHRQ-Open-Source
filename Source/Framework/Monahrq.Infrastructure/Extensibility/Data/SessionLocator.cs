using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Type;
using Monahrq.Infrastructure.Entities.Domain;
using System.ComponentModel.Composition;
using NHibernate.Context;
using Monahrq.Infrastructure.Data;

namespace Monahrq.Sdk.Extensibility.Data
{

    [Export(typeof(ISessionLocator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SessionLocator : ISessionLocator
    {
        private readonly IDomainSessionFactoryProvider _sessionFactoryHolder;

        [ImportingConstructor]
        public SessionLocator(
            IDomainSessionFactoryProvider sessionFactoryHolder,
            [Import(LogNames.Operations)]ILogWriter logger)
            : this(sessionFactoryHolder)
        {
            Logger = logger;
        }

        public SessionLocator(
            IDomainSessionFactoryProvider sessionFactoryHolder)
        {
            _sessionFactoryHolder = sessionFactoryHolder;
            Logger = NullLogger.Instance;
        }

        ~SessionLocator()
        {
            // TODO: CLEANUP 
        }

        public ILogWriter Logger { get; set; }

        /// <summary>
        /// DO NOT USE THIS! See Jason/Blair.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public ISession For(Type entityType)
        {
            Logger.Debug("Acquiring session for {0}", entityType);
            return //UnitOfWork.Current == null ?
                CreateSession()
                  //: UnitOfWork.Current.Session
                  ;
        }

        private static ISession TheSession { get; set; }
 
        private ISession CreateSession()
        {
            return _sessionFactoryHolder.SessionFactory.OpenSession();

            //if(!CurrentSessionContext.HasBind(sessionFactory))
            //{
            //    Logger.Information("Opening database session");
            //    CurrentSessionContext.Bind(sessionFactory.OpenSession(new SessionInterceptor()));
            //}
            //TheSession = sessionFactory.OpenSession();
            //return TheSession;
        }

        class SessionInterceptor : IInterceptor
        {
            private ISession _session;

            bool IInterceptor.OnLoad(object entity, object id, object[] state, string[] propertyNames, IType[] types)
            {
                return false;
            }

            bool IInterceptor.OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types)
            {
                return false;
            }

            bool IInterceptor.OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
            {
                return false;
            }

            void IInterceptor.OnDelete(object entity, object id, object[] state, string[] propertyNames, IType[] types)
            {
            }

            void IInterceptor.OnCollectionRecreate(object collection, object key)
            {
            }

            void IInterceptor.OnCollectionRemove(object collection, object key)
            {
            }

            void IInterceptor.OnCollectionUpdate(object collection, object key)
            {
            }

            void IInterceptor.PreFlush(ICollection entities)
            {
            }

            void IInterceptor.PostFlush(ICollection entities)
            {
            }

            bool? IInterceptor.IsTransient(object entity)
            {
                return null;
            }

            int[] IInterceptor.FindDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types)
            {
                return null;
            }

            object IInterceptor.Instantiate(string entityName, EntityMode entityMode, object id)
            {
                return null;
            }

            string IInterceptor.GetEntityName(object entity)
            {
                return null;
            }

            object IInterceptor.GetEntity(string entityName, object id)
            {
                return null;
            }

            void IInterceptor.AfterTransactionBegin(ITransaction tx)
            {
            }

            void IInterceptor.BeforeTransactionCompletion(ITransaction tx)
            {
            }

            void IInterceptor.AfterTransactionCompletion(ITransaction tx)
            {
            }

            SqlString IInterceptor.OnPrepareStatement(SqlString sql)
            {
                return sql;
            }

            void IInterceptor.SetSession(ISession session)
            {
                _session = session;
            }
        }
    }
}
