using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Util;
using Monahrq.Infrastructure.Entities.Domain;
using System.Threading;
using System.Diagnostics;
using System.Linq.Expressions;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Data;
using LinqKit;

namespace Monahrq.Infrastructure.Entities.Domain
{
    /// <summary>
    /// Base class for all repositories those uses NHibernate.
    /// </summary>
    /// <typeparam name="TModel">Entity type</typeparam>
    /// <typeparam name="TKey">Primary key type of the entity</typeparam>
    public abstract class RepositoryBase<TModel, TKey> :
        IRepository<TModel, TKey>
        where TModel : class, Domain.IEntity<TKey>
    {

        protected abstract IDomainSessionFactoryProvider DomainSessionFactoryProvider
        {
            get;
            set;
        }

        public IDomainSessionFactoryProvider SessionFactoryProvider
        {
            get { return this.DomainSessionFactoryProvider; }
        }


        [Import(LogNames.Operations)]
        protected ILogWriter Logger
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the NHibernate session object to perform database operations.
        /// </summary>
        protected ISession Session
        {
            get
            {
                return UnitOfWork.Current == null ? DomainSessionFactoryProvider.SessionFactory.OpenSession()
                    : UnitOfWork.Current.Session;
            }
        }

        public TKey Create(TModel model)
        {
            Session.Save(model);
            return model.Id;
        }

        void LogAction(string action, Action task)
        {
            if (Logger != null)
            {
                Logger.Write(string.Format("Begin {0}.{1}", this.GetType(), action), TraceEventType.Verbose);
            }
            try
            {
                task();
            }
            catch (Exception ex)
            {
                if (Logger != null)
                {
                    Logger.Write(string.Format("{0}.{1} Failed. See log for details", this.GetType().FullName, action), TraceEventType.Verbose);
                }
                throw new RepositoryException(ex, "Repository action failed: {0}.{1}", this.GetType().FullName, action);
            }
            finally
            {
                if (Logger != null)
                {
                    Logger.Write(string.Format("{0}.{1} complete", this.GetType(), action), TraceEventType.Verbose);
                }
            }
        }

        T LogAction<T>(string action, Func<T> task)
        {
            if (Logger != null)
            {
                Logger.Write(string.Format("Begin {0}.{1}", this.GetType(), action), TraceEventType.Verbose);
            }
            try
            {
                return task();
            }
            catch (Exception ex)
            {
                if (Logger != null)
                {
                    Logger.Write(string.Format("{0}.{1} Failed. See log for details", this.GetType().FullName, action), TraceEventType.Verbose);
                }
                throw new RepositoryException(ex, "Repository action failed: {0}.{1}", this.GetType().FullName, action);
            }
            finally
            {
                if (Logger != null)
                {
                    Logger.Write(string.Format("{0}.{1} complete", this.GetType(), action), TraceEventType.Verbose);
                }
            }
        }

        public IQueryable<TModel> Retrieve(Expression<Func<TModel, bool>> criteria)
        {
            return LogAction("Retrieve", () => Session
                .Query<TModel>()
                .AsExpandable()
                .Where(criteria));
        }

        public TModel RetrieveByKey(TKey key)
        {
            return Session.Get<TModel>(key);
        }

        public void Update(TModel model)
        {
            if (!model.IsChanged) return;
            if (object.Equals(model.Id, default(TKey)))
            {
                Session.Save(model);
            }
            else
            {
                Session.Merge(model);
            }
        }

        public void EndUpdate(IAsyncResult asyncResult)
        {
            var result = (AsyncResult<TModel>)asyncResult;
            AsyncResult<TModel>.End(asyncResult);
        }

        public void Delete(TModel model)
        {
            Session.Delete(model);
        }

        public TModel Undo(TModel model)
        {
            if (object.Equals(model.Id, default(TKey)))
            {
                return RetrieveByKey(model.Id);
            }
            else
            {
                return default(TModel);
            }
        }

    }
}
