using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

namespace Monahrq.Infrastructure.Entities.Domain
{
    /// <summary>
    /// This interface must be implemented by all repositories to ensure UnitOfWork to work.
    /// </summary>
    public interface IRepository
    {

    }

    /// <summary>
    /// This interface is implemented by all repositories to ensure implementation of fixed methods.
    /// </summary>
    /// <typeparam name="TEntity">Main Entity type this repository works on</typeparam>
    /// <typeparam name="TPrimaryKey">Primary key type of the entity</typeparam>
    public interface IRepository<TModel, TKey> : IRepository 
            where TModel : Domain.IEntity<TKey>
    {
        TKey Create(TModel model);
        //IAsyncResult BeginCreate(TModel model, AsyncCallback callback, object userState);
        //TKey EndCreate(IAsyncResult asyncResult);

        IQueryable<TModel> Retrieve(Expression<Func<TModel, bool>> criteria);
        //IAsyncResult BeginRetrieve(Expression<Func<TModel, bool>> criteria, AsyncCallback callback, object userState);
        //SearchResult<TModel, TKey> EndRetrieve(IAsyncResult asyncResult);

        TModel RetrieveByKey(TKey key);
        //IAsyncResult BeginRetrieveByKey(TKey key, AsyncCallback callback, object userState);
        //TModel EndRetrieveByKey(IAsyncResult asyncResult);


        void Update(TModel model);
        //IAsyncResult BeginUpdate(TModel model, AsyncCallback callback, object userState);
        //void EndUpdate(IAsyncResult asyncResult);

        void Delete(TModel model);
        //IAsyncResult BeginDelete(TModel model, AsyncCallback callback, object userState);
        //void EndDelete(IAsyncResult asyncResult);

        TModel Undo(TModel model);
        //IAsyncResult BeginUndo(TModel key, AsyncCallback callback, object userState);
        //TModel EndUndo(IAsyncResult asyncResult);

        IDomainSessionFactoryProvider SessionFactoryProvider { get; }
    }

    
}
