using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Domain;
using NHibernate;

namespace Monahrq.Infrastructure.Domain
{
    public abstract class ServiceBase<TModel, TKey> : IService<TModel, TKey>
        where TModel : class, IEntity<TKey>
    {

        protected ISession Session { get { return UnitOfWork.Current.Session; } }

        static SynchronizationContext syncRoot = new SynchronizationContext();


        public SynchronizationContext SyncRoot 
        {
            get
            {
                return syncRoot;
            }
        }

        
        [Import(LogNames.Operations)]
        protected ILogWriter Logger
        {
            get;
            private set;
        }

        public TKey Create(TModel model)
        {
            Session.Save(model);
            return model.Id;
        }

        public IAsyncResult BeginCreate(TModel model, AsyncCallback callback, object userState)
        {
            AsyncResult<TKey> asyncResult = new AsyncResult<TKey>(callback, userState);

            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    var result = Create(model);
                    asyncResult.SetComplete(result, false);
                });

            return asyncResult;
        }
        public TKey EndCreate(IAsyncResult asyncResult)
        {
            var result = (AsyncResult<TKey>)asyncResult;
            AsyncResult<TKey>.End(asyncResult);
            return result.Result;
        }

        void LogAction(string action, Action task)
        {
            Logger.Write(string.Format("Begin {0}.{1}", this.GetType(), action), TraceEventType.Verbose);
            task();
            Logger.Write(string.Format("{0}.{1} complete", this.GetType(), action), TraceEventType.Verbose);
        }

        public SearchResult<TModel, TKey> Retrieve(Expression<Func<TModel, bool>> criteria)
        {
            SearchResult<TModel, TKey> result = null;
            LogAction("Retrieve", ()=>
                {
                    result = DoRetrieve(criteria);
                });
            return result;
        }
        private SearchResult<TModel, TKey> DoRetrieve(Expression<Func<TModel, bool>> criteria)
        {
                var query = Session.QueryOver<TModel>();
                var select = query.Where(criteria);
                var items = select.List();
                return new SearchResult<TModel, TKey>(items);
            
        }
        public IAsyncResult BeginRetrieve(Expression<Func<TModel, bool>> criteria, AsyncCallback callback, object userState)
        {
            Logger.Write("Begin Async", TraceEventType.Verbose);
            AsyncResult<SearchResult<TModel, TKey>> asyncResult =
                new AsyncResult<SearchResult<TModel, TKey>>(callback, userState);

            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    var result = Retrieve(criteria);
                    asyncResult.SetComplete(result, false);
                });

            return asyncResult;
        }

        public SearchResult<TModel, TKey> EndRetrieve(IAsyncResult asyncResult)
        {
            var result = (AsyncResult<SearchResult<TModel, TKey>>)asyncResult;
            AsyncResult<SearchResult<TModel, TKey>>.End(asyncResult);
            Logger.Write("End Async", TraceEventType.Verbose);
            return result.Result;
        }

        public TModel RetrieveByKey(TKey key)
        {
            return Session.Get<TModel>(key);
        }

        public IAsyncResult BeginRetrieveByKey(TKey key, AsyncCallback callback, object userState)
        {
            AsyncResult<TModel> asyncResult = new AsyncResult<TModel>(callback, userState);

            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    var result = RetrieveByKey(key);
                    asyncResult.SetComplete(result, false);
                });

            return asyncResult;
        }

        public TModel EndRetrieveByKey(IAsyncResult asyncResult)
        {
            var result = (AsyncResult<TModel>)asyncResult;
            AsyncResult<TModel>.End(asyncResult);
            return result.Result;
        }

        public void Update(TModel model)
        {
            if (!model.IsChanged) return;
            Session.SaveOrUpdate(model); 
        }

        public IAsyncResult BeginUpdate(TModel model, AsyncCallback callback, object userState)
        {
            AsyncResult<TModel> asyncResult = new AsyncResult<TModel>(callback, userState);

            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    Update(model);
                    try
                    {
                        asyncResult.SetComplete(model, false);
                    }
                    catch (Exception ex)
                    {
                        asyncResult.SetComplete(ex, false);
                    }
                });

            return asyncResult;
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

        public IAsyncResult BeginDelete(TModel model, AsyncCallback callback, object userState)
        {
            AsyncResult<TModel> asyncResult = new AsyncResult<TModel>(callback, userState);

            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    Delete(model);
                    try
                    {
                        asyncResult.SetComplete(model, false);
                    }
                    catch (Exception ex)
                    {
                        asyncResult.SetComplete(ex, false);
                    }
                });

            return asyncResult;
        }

        public void EndDelete(IAsyncResult asyncResult)
        {
            var result = (AsyncResult<TModel>)asyncResult;
            AsyncResult<TModel>.End(asyncResult);
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

        public IAsyncResult BeginUndo(TModel model, AsyncCallback callback, object userState)
        {
            AsyncResult<TModel> asyncResult = new AsyncResult<TModel>(callback, userState);

            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    var result = Undo(model);
                    asyncResult.SetComplete(result, false);
                });

            return asyncResult;
        }

        public TModel EndUndo(IAsyncResult asyncResult)
        {
            var result = (AsyncResult<TModel>)asyncResult;
            AsyncResult<TModel>.End(asyncResult);
            return result.Result;
        }

        public Expression<Func<TModel, bool>> True()
        {
            return LinqKit.PredicateBuilder.True<TModel>();
        }

        public Expression<Func<TModel, bool>> False()
        {
            return LinqKit.PredicateBuilder.False<TModel>();
        }
    }
}
