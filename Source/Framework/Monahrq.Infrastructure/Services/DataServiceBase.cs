using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Data.Transformers;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Context;

namespace Monahrq.Infrastructure.Services
{
    public interface IDataServiceBase
    {
        /// <summary>
        /// Gets or sets the wings session factory provider.
        /// </summary>
        /// <value>
        /// The wings session factory provider.
        /// </value>
        [Import]
        IDomainSessionFactoryProvider SessionFactoryProvider { get; set; }

        /// <summary>
        /// Gets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        IEventAggregator EventAggregator { get; }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        void OnImportsSatisfied();

        /// <summary>
        /// Gets all asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<List<T>> GetAllAsync<T>();
        /// <summary>
        /// Saves the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        Task<T> SaveAsync<T>(T t);
        /// <summary>
        /// Deletes the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        Task<bool> DeleteAsync<T>(T t);
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query.</param>
        /// <param name="callback">The callback.</param>
        void GetAll<T>(Expression<Func<T, bool>> query, Action<List<T>, Exception> callback) where T : class, IEntity;
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
        /// Refreshes the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        T Refresh<T>(T item) where T : class, IEntity;
        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        void Save<T, TKey>(T entity, Action<object, Exception> callback) where T : class, IEntity<TKey>;
        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        void Save<T>(T entity, Action<object, Exception> callback) where T : class, IEntity;
        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        void Delete<T>(T entity, Action<bool, Exception> callback) where T : class, IEntity;

        /// <summary>
        /// Queries the list.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="hqlQuery">The HQL query.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="outputColumns">The output columns.</param>
        void Query<TResult>(string hqlQuery, Action<IEnumerable<TResult>, Exception> callback, params string[] outputColumns);

        /// <summary>
        /// Executes the scalar sp.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure.</param>
        /// <param name="paramters">The paramters.</param>
        /// <returns></returns>
        bool ExecuteNonQuerySp(string storedProcedure, IDictionary<string, object> paramters);

        /// <summary>
        /// Executes the query SQL data reader.
        /// </summary>
        /// <typeparam name="T">the type class you want to populate from the resultant datareader.</typeparam>
        /// <param name="storedProcedure">The stored procedure.</param>
        /// <param name="inParams">The in params.</param>
        /// <param name="outParams">The out params.</param>
        /// <returns></returns>
        IList<T> ExecuteQuerySQLDataReader<T>(string storedProcedure, IList<DbParameter> inParams, IList<DbParameter> outParams = null)
            where T : class, new();

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        void Dispose();
    }

    /// <summary>
    /// This class is a service for CRUD operations with NHibernate, it provided generic CRUD methods for INamedEnity Types
    /// All Syncrounous method return callback action with result and exception
    /// All Task based methods return TaskCompletionSource with completetion status, exceptions and results. Supports progress report and cancelation
    /// All Exceptions are automatically logged and publis using Event Aggregator, with event ServiceErrorEventArgs
    /// Provides Session and StatlessSession for bulk loads
    /// </summary>
    [Export(typeof(IDataServiceBase))]
    public abstract partial class DataServiceBase : IPartImportsSatisfiedNotification, IDataServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataServiceBase"/> class.
        /// </summary>
        protected DataServiceBase()
        {}

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        ILogWriter Logger { get; set; }

        /// <summary>
        /// Gets or sets the wings session factory provider.
        /// </summary>
        /// <value>
        /// The wings session factory provider.
        /// </value>
        [Import]
        public IDomainSessionFactoryProvider SessionFactoryProvider { get; set; }

        /// <summary>
        /// Gets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public IEventAggregator EventAggregator { get; set; }

        #region Session Managment

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <returns></returns>
        protected ISession GetSession()
        {

            if (SessionFactoryProvider != null)
            {
                return SessionFactoryProvider.SessionFactory.OpenSession();
            }

            SessionFactoryProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            return SessionFactoryProvider.SessionFactory.OpenSession();
        }
        /// <summary>
        /// Gets the stateless session.
        /// </summary>
        /// <returns></returns>
        protected IStatelessSession GetStatelessSession()
        {
            if (SessionFactoryProvider != null)
            {
                return SessionFactoryProvider.SessionFactory.OpenStatelessSession();
            }

            SessionFactoryProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            return SessionFactoryProvider.SessionFactory.OpenStatelessSession();
        }


        #endregion

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public virtual void OnImportsSatisfied()
        {
            //_initialised = true;
        }

        #region Task based calls

        /// <summary>
        /// Gets all asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<List<T>> GetAllAsync<T>()
        {
            var tcs = new TaskCompletionSource<List<T>>();
            try
            {
                List<T> list;
                using (var session = GetSession())
                {
                    list = session.Query<T>().ToList();
                }
                tcs.SetResult(list);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
                Log(e, "Loading all", new EntityDescriptor(typeof(T).Name));
            }

            return tcs.Task;
        }

        /// <summary>
        /// Saves the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public Task<T> SaveAsync<T>(T t)
        {
            var tcs = new TaskCompletionSource<T>();
            try
            {
                using (var session = GetSession())
                {
                    using (var tx = session.BeginTransaction())
                    {
                        session.SaveOrUpdate(t);
                        tx.Commit();
                    }
                }

                tcs.SetResult(t);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
                Log(e, "Saving", new EntityDescriptor(typeof(T).Name));
            }

            return tcs.Task;
        }

        /*Return true if deleted, false if exceptions occored*/
        /// <summary>
        /// Deletes the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public Task<bool> DeleteAsync<T>(T t)
        {
            var tcs = new TaskCompletionSource<bool>();
            try
            {
                using (var session = GetSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        session.Delete(t);

                        trans.Commit();

                        if (trans.WasCommitted)
                        {
                            session.Flush();
                        }
                    }
                }
                tcs.SetResult(true);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
                tcs.SetResult(false);
                Log(e, "Deleting", new EntityDescriptor(typeof(T).Name));
            }

            return tcs.Task;
        }

        #endregion

        #region Synchronous Methods

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query.</param>
        /// <param name="callback">The callback.</param>
        public void GetAll<T>(Expression<Func<T, bool>> query, Action<List<T>, Exception> callback)
            where T : class, IEntity
        {
            Exception error = null;
            List<T> result = null;

            try
            {
                using (var session = GetSession())
                {
                    result = session.Query<T>().Where(query).ToList();
                }
            }
            catch (Exception e)
            {
                error = e;
                Log(e, "Loading all", new EntityDescriptor(typeof(T).Name));
            }

            callback(result, error);
        }


        /// <summary>
        /// Gets all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback">The callback.</param>
        public void GetAll<T>(Action<List<T>, Exception> callback) where T : class, IEntity
        {
            Exception error = null;
            var result = new List<T>();

            try
            {
                using (var session = GetSession())
                {
                    result = new List<T>(session.QueryOver<T>().List());
                }
            }
            catch (Exception e)
            {
                error = e;
                Log(e, "Loading all", new EntityDescriptor(typeof(T).Name));
            }

            callback(result, error);
        }

        /// <summary>
        /// Gets the entity by identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="callback">The callback.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void GetEntityById<T>(object id, Action<T, Exception> callback)
        {
            Exception error = null;
            T result = default(T);

            try
            {
                using (var session = GetSession())
                {
                    result = session.Get<T>(id);
                }

                if (result == null)
                    throw new InvalidOperationException(String.Format("{0} with ID {1} does not exsist", typeof(T).Name, id));
            }
            catch (Exception e)
            {
                error = e;
                Log(e, "Getting entity from DB", new EntityDescriptor(typeof(T).Name, id.ToString()));
            }

            callback(result, error);
        }

        /// <summary>
        /// Refreshes the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        public void RefreshEntity<T>(T entity, Action<bool, Exception> callback) where T : class, IEntity
        {
            Exception error = null;
            bool result = false;

            try
            {
                using (var session = GetSession())
                {
                    session.Refresh(entity);
                }
                result = true;
            }
            catch (Exception e)
            {
                error = e;
                Log(e, "Refreshing entity", new EntityDescriptor(entity));
            }

            callback(result, error);
        }

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        public void Save<T, TKey>(T entity, Action<object, Exception> callback) where T : class, IEntity<TKey>
        {
            Exception error = null;
            object result = null;
            try
            {
                using (var session = GetSession())
                {
                    using (var tx = session.BeginTransaction())
                    {
                        session.SaveOrUpdate(entity);

                        tx.Commit();

                        if (tx.WasCommitted)
                        {
                            result = entity;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                error = e;
                Log(e, "Saving to DB", new EntityDescriptor(entity));
            }

            callback(result, error);
        }

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        public void Save<T>(T entity, Action<object, Exception> callback) where T : class, IEntity
        {
            Exception error = null;
            object result = null;
            try
            {
                using (var session = GetSession())
                {
                    using (var tx = session.BeginTransaction())
                    {
                        session.Evict(entity);

                        if (!entity.IsPersisted)
                            session.SaveOrUpdate(entity);
                        else
                            entity = session.Merge(entity);

                        session.Flush();

                        tx.Commit();

                        if (tx.WasCommitted)
                        {

							if (!CurrentSessionContext.HasBind(SessionFactoryProvider.SessionFactory))
								CurrentSessionContext.Bind(session);

							session.Refresh(entity);
                            result = entity;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                error = e;
                Log(e, "Saving to DB", new EntityDescriptor(entity));
            }

            callback(result, error);
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        public void Delete<T>(T entity, Action<bool, Exception> callback) where T : class, IEntity
        {
            Exception error = null;
            bool result = false;

            try
            {
                using (var session = GetSession())
                {
                    using (var tx = session.BeginTransaction())
                    {
                        session.Evict(entity);
                        session.Delete(entity);
                        session.Flush();

                        tx.Commit();

                        if (tx.WasCommitted)
                        {
                            result = true;
                        }
                    }
                }

                result = true;
            }
            catch (Exception e)
            {
                error = e;
                Log(e, "Deleting", new EntityDescriptor(entity));
            }

            callback(result, error);
        }

        /// <summary>
        /// Queries the specified object utilizing nhibernate hql and returns a developer specified return type that matches the output columns.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="hqlQuery">The HQL query.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="outputColumns">The output columns.</param>
        public void Query<TResult>(string hqlQuery, Action<IEnumerable<TResult>, Exception> callback, params string[] outputColumns)
        {
            Exception error = null;
            IEnumerable<TResult> result = new List<TResult>();
            try
            {
                string hqlQueryWithColumns = hqlQuery;
                if (outputColumns != null && outputColumns.Any())
                {
                    if (hqlQuery.ContainsIgnoreCase("select"))
                    {
                        hqlQueryWithColumns = hqlQueryWithColumns.Replace(hqlQuery.SubStrBefore("where"), null);
                        hqlQueryWithColumns = string.Format("select {0} {1}", string.Join(",", outputColumns), hqlQueryWithColumns);
                        hqlQueryWithColumns = string.Format("select {0} {1}", string.Join(",", outputColumns), hqlQueryWithColumns);
                    }
                }

                using (var session = GetSession())
                {
                    IQuery query = session.CreateQuery(hqlQueryWithColumns);

                    if (outputColumns != null && outputColumns.Any())
                    {
                        var transformer = new TupleToPropertyResultTransformer(typeof(TResult), outputColumns);
                        result = query.SetResultTransformer(transformer).List<TResult>();
                    }
                    else
                    {
                        result = query.List<TResult>();
                    }
                }

            }
            catch (Exception e)
            {
                error = e;
                Log(e, "HQL Query against DB.", null);
            }

            callback(result, error);
        }

        /// <summary>
        /// Refreshes the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public T Refresh<T>(T item) where T : class, IEntity
        {
            using (var session = GetSession())
            {
                session.Refresh(item);
            }
            return item;
        }
        #endregion

        #region Temporary T-SQL methods

        /// <summary>
        /// Executes the scalar sp.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure.</param>
        /// <param name="paramters">The paramters.</param>
        /// <returns></returns>
        public bool ExecuteNonQuerySp(string storedProcedure, IDictionary<string, object> paramters)
        {
            using (var session = GetSession())
            {
                ISQLQuery query = session.CreateSQLQuery(storedProcedure);
                foreach (var paramter in paramters)
                {
                    query.SetParameter(paramter.Key, paramter.Value);
                }

                return query.ExecuteUpdate() >= 0;
            }
        }

        /// <summary>
        /// Executes the query SQL data reader.
        /// </summary>
        /// <typeparam name="T">the type class you want to populate from the resultant datareader.</typeparam>
        /// <param name="storedProcedure">The stored procedure.</param>
        /// <param name="inParams">The in params.</param>
        /// <param name="outParams">The out params.</param>
        /// <returns></returns>
        public IList<T> ExecuteQuerySQLDataReader<T>(string storedProcedure, IList<DbParameter> inParams, IList<DbParameter> outParams = null)
            where T : class, new()
        {
            var sqlparams = new SqlParameter[inParams.Count];

            var i = 0;
            foreach (var parameter in inParams)
            {
                sqlparams[i] = new SqlParameter(parameter.Name, parameter.Value);
                i++;
            }

            if (outParams != null && outParams.Count > 0)
            {
                foreach (var parameter in outParams)
                {
                    sqlparams[i] = new SqlParameter("@" + parameter.Name, parameter.Type)
                    {
                        Direction = ParameterDirection.Output
                    };
                }
            }

            using (var session = GetSession())
            {
                using (var sqlCommand = session.Connection.CreateCommand())
                {
                    sqlCommand.Connection = session.Connection;
                    sqlCommand.CommandText = storedProcedure;
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    foreach (var parameter in sqlparams)
                        sqlCommand.Parameters.Add(parameter);


                    using (IDataReader dataReader = sqlCommand.ExecuteReader())
                    {
                        if (outParams != null && outParams.Count > 0)
                        {
                            foreach (var titanParameter in outParams)
                            {
                                titanParameter.Value = ((SqlParameter)sqlCommand.Parameters["@" + titanParameter.Name]).Value;
                            }
                        }

                        return new GenericObjectMapper<T>().MapReaderToObjectList(dataReader as SqlDataReader);
                    }
                }
            }
        }

        #endregion

        #region Exception Handling

        /*When error occurs :
          1. Log error with Method, Enity type, name and id
          3. Replace session after after error 
          4. Reload affected enitity if applies */

        protected void Log(Exception e, string method, EntityDescriptor entityDescriptor)
        {
            if (Logger == null)
            {
                Logger = ServiceLocator.Current.GetInstance<ILogWriter>(LogNames.Session);
            }
            /*Logerror*/
            Logger.Write(e, TraceEventType.Error, new Dictionary<string, string>
                {
                    {"Method", method},
                    {"Enitity Type", entityDescriptor.Type},
                    {"Name", entityDescriptor.Name},
                    {"ID", entityDescriptor.Id}
                },
                "Error in generic Data Access Layer");

            /*Publish Error*/
            ServiceLocator.Current.GetInstance<IEventAggregator>()
                .GetEvent<ServiceErrorEvent>()
                .Publish(new ServiceErrorEventArgs(e, entityDescriptor));

            /*Reset Session*/
            ReplaceSessionAfterError();
        }

        protected void ReplaceSessionAfterError()
        {
            ReleaseSessions();
        }

        #endregion
    }

    #region Db Parameter
    /// <summary>
    /// The custom database parameter used to transform into SqlDataParameters
    /// </summary>
    [Serializable]
    public class DbParameter
    {
        private readonly bool _isOutPut;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbParameter"/> class.
        /// </summary>
        /// <param name="isOutput">if set to <c>true</c> [is output].</param>
        public DbParameter(bool isOutput = false)
        {
            _isOutPut = isOutput;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public DbType Type { get; set; }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>
        /// The direction.
        /// </value>
        public ParameterDirection Direction
        {
            get
            {
                return _isOutPut ? ParameterDirection.Output : ParameterDirection.Input;
            }
        }
    }
    #endregion

    #region GenericObjectMapper
    /// <summary>
    /// A generic object mapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericObjectMapper<T> where T : new()
    {
        /// <summary>
        /// Maps the reader to object list.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        public List<T> MapReaderToObjectList(SqlDataReader reader)
        {
            var resultList = new List<T>();
            while (reader.Read())
            {
                var item = new T();
                Type t = item.GetType();

                foreach (PropertyInfo property in t.GetProperties())
                {
                    Type type = property.PropertyType;
                    string readerValue = string.Empty;

                    if (reader[property.Name] != DBNull.Value)
                    {
                        readerValue = reader[property.Name].ToString();
                    }

                    if (!string.IsNullOrEmpty(readerValue))
                    {
                        property.SetValue(item, readerValue.To(type), null);
                    }

                }
            }
            return resultList;
        }

        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public IList<T> ConvertTo(DataTable table)
        {
            if (table == null)
            {
                return null;
            }

            var rows = table.Rows.Cast<DataRow>().ToList();

            return ConvertTo(rows);
        }

        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="rows">The rows.</param>
        /// <returns></returns>
        public IList<T> ConvertTo(IList<DataRow> rows)
        {
            IList<T> list = null;

            if (rows != null)
            {
                list = rows.Select(CreateItem).ToList();
            }

            return list;
        }

        /// <summary>
        /// Creates the item.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        public T CreateItem(DataRow row)
        {
            T obj = default(T);
            if (row != null)
            {
                obj = Activator.CreateInstance<T>();

                foreach (DataColumn column in row.Table.Columns)
                {
                    PropertyInfo prop = obj.GetType().GetProperty(column.ColumnName);
                    try
                    {
                        object value = row[column.ColumnName];
                        if (prop.PropertyType.IsEnum)
                        {
                            value = Enum.Parse(prop.PropertyType, value.ToString(), true);
                        }

                        prop.SetValue(obj, value, null);
                    }
                    catch (ArgumentException)
                    {
                        if (row[column.ColumnName] is Guid)
                        {
                            prop.SetValue(obj, row[column.ColumnName].ToString(), null);
                        }
                    }
                    catch
                    {
                        // You can log something here
                        //throw;
                    }
                }
            }

            return obj;
        }
    }

    /// <summary>
    /// A class to cast object to specified types.
    /// </summary>
    public static class TypeCaster
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object To(this string value, Type t)
        {
            return Convert.ChangeType(value, t);
        }
    }
    #endregion
}
