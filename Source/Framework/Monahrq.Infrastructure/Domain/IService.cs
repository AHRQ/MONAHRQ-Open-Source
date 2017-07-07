using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Domain;
using Monahrq.Infrastructure.Entities.Domain;
using NHibernate;

namespace Monahrq.Infrastructure.Entities.Domain
{
    public interface IService<TModel, TKey>
        where TModel : IEntity<TKey>
    {

     
        SynchronizationContext SyncRoot { get; }

       // IOperationResult<SearchResult<TModel, TKey>> Empty { get; }

        TKey Create(TModel model);
        IAsyncResult BeginCreate(TModel model, AsyncCallback callback, object userState);
        TKey EndCreate(IAsyncResult asyncResult);

        TModel Undo(TModel model);
        IAsyncResult BeginUndo(TModel model, AsyncCallback callback, object userState);
        TModel EndUndo(IAsyncResult asyncResult);

        SearchResult<TModel, TKey> Retrieve(Expression<Func<TModel, bool>> criteria);
        IAsyncResult BeginRetrieve(Expression<Func<TModel, bool>> criteria, AsyncCallback callback, object userState);
        SearchResult<TModel, TKey> EndRetrieve(IAsyncResult asyncResult); 

        TModel RetrieveByKey(TKey key);

        void Update(TModel model);
        IAsyncResult BeginUpdate(TModel model, AsyncCallback callback, object userState);
        void EndUpdate(IAsyncResult asyncResult);

        void Delete(TModel model);

        IAsyncResult BeginDelete(TModel model, AsyncCallback callback, object userState);
        void EndDelete(IAsyncResult asyncResult);

        Expression<Func<TModel, bool>> True();

        Expression<Func<TModel, bool>> False();
    }
}
