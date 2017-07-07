using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure;
using NHibernate;
using NHibernate.Linq;
using Monahrq.Sdk.Extensibility.Utility.Extensions;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Data;

namespace Monahrq.Sdk.Extensibility.Data
{
    public class ExtensionRepository<T> : IExtensionRepository<T> where T : class
    {
        private readonly ISessionLocator _sessionLocator;

        public ExtensionRepository(ISessionLocator sessionLocator)
        {
            _sessionLocator = sessionLocator;
            Logger = NullLogger.Instance;
        }

        public ILogWriter Logger { get; set; }

        protected virtual ISessionLocator SessionLocator
        {
            get { return _sessionLocator; }
        }

        protected virtual ISession Session
        {
            get { return SessionLocator.For(typeof(T)); }
        }

        public virtual IQueryable<T> Table
        {
            get { return Session.Query<T>(); }
        }

        #region IRepository<T> Members

        void IExtensionRepository<T>.Create(T entity)
        {
            Create(entity);
        }

        void IExtensionRepository<T>.Update(T entity)
        {
            Update(entity);
        }

        void IExtensionRepository<T>.Delete(T entity)
        {
            Delete(entity);
        }

        void IExtensionRepository<T>.Copy(T source, T target)
        {
            Copy(source, target);
        }

        void IExtensionRepository<T>.Flush()
        {
            Flush();
        }

        T IExtensionRepository<T>.Get(int id)
        {
            return Get(id);
        }

        T IExtensionRepository<T>.Get(Expression<Func<T, bool>> predicate)
        {
            return Get(predicate);
        }

        IQueryable<T> IExtensionRepository<T>.Table
        {
            get { return Table; }
        }

        int IExtensionRepository<T>.Count(Expression<Func<T, bool>> predicate)
        {
            return Count(predicate);
        }

        IEnumerable<T> IExtensionRepository<T>.Fetch(Expression<Func<T, bool>> predicate)
        {
            return Fetch(predicate).ToReadOnlyCollection();
        }

        IEnumerable<T> IExtensionRepository<T>.Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order)
        {
            return Fetch(predicate, order).ToReadOnlyCollection();
        }

        IEnumerable<T> IExtensionRepository<T>.Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order, int skip,
                                            int count)
        {
            return Fetch(predicate, order, skip, count).ToReadOnlyCollection();
        }

        #endregion

        public virtual T Get(int id)
        {
            return Session.Get<T>(id);
        }

        public virtual T Get(Expression<Func<T, bool>> predicate)
        {
            return Fetch(predicate).SingleOrDefault();
        }

        public virtual void Create(T entity)
        {
            Logger.Debug("Create {0}", entity);
            Session.Save(entity);
        }

        public virtual void Update(T entity)
        {
            Logger.Debug("Update {0}", entity);
            Session.Evict(entity);
            Session.Merge(entity);
        }

        public virtual void Delete(T entity)
        {
            Logger.Debug("Delete {0}", entity);
            Session.Delete(entity);
        }

        public virtual void Copy(T source, T target)
        {
            Logger.Debug("Copy {0} {1}", source, target);
            var metadata = Session.SessionFactory.GetClassMetadata(typeof(T));
            var values = metadata.GetPropertyValues(source, EntityMode.Poco);
            metadata.SetPropertyValues(target, values, EntityMode.Poco);
        }

        public virtual void Flush()
        {
            Session.Flush();
        }

        public virtual int Count(Expression<Func<T, bool>> predicate)
        {
            return Fetch(predicate).Count();
        }

        public virtual IQueryable<T> Fetch(Expression<Func<T, bool>> predicate)
        {
            return Table.Where(predicate);
        }

        public virtual IQueryable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order)
        {
            var orderable = new Orderable<T>(Fetch(predicate));
            order(orderable);
            return orderable.Queryable;
        }

        public virtual IQueryable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order, int skip,
                                           int count)
        {
            return Fetch(predicate, order).Skip(skip).Take(count);
        }
    }
}
