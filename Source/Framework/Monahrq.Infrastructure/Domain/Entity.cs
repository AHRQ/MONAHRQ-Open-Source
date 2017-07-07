using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain
{
    [Serializable]
    public abstract class Entity<TId> : IEntity<TId>
    {
        /// <summary>
        /// Creates the bulk insert mapper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable">The data table.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public virtual IBulkMapper CreateBulkInsertMapper<T>(DataTable dataTable, T instance = default(T), Target target = null)
            where T : class
        {
            var entityType = typeof(T);
            var theMapperType = typeof(BulkMapper<>);
            var theEntityMapperType = theMapperType.MakeGenericType(entityType);
            var ctor = theEntityMapperType.GetConstructors()[0];
            var result = ctor.Invoke(new object[] { dataTable, instance, target }) as IBulkMapper;
            return result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
        /// </summary>
        protected Entity()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected virtual void Initialize()
        { }

        /// <summary>
        /// Cleans the before save.
        /// </summary>
        public virtual void CleanBeforeSave()
        { }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [XmlAttribute]
        public virtual TId Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [XmlAttribute]
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is persisted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is persisted; otherwise, <c>false</c>.
        /// </value>
        [XmlIgnore]
        public bool IsPersisted { get { return !Equals(Id, default(TId)); } }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is changed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is changed; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsChanged { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is deleted; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is deleted; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SkipAudit { get; set; }

        #region overrides & helper



        ///// <summary>
        ///// Determines whether the specified <see cref="System.Object" }, is equal to this instance.
        ///// </summary>
        ///// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        ///// <returns>
        /////   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        ///// </returns>
        //public override bool Equals(object obj)
        //{
        //    return Equals(obj as Entity<TId>);
        //}


        ///// <summary>
        ///// Equalses the specified other.
        ///// </summary>
        ///// <param name="other">The other.</param>
        ///// <returns></returns>
        //public virtual bool Equals(Entity<TId> other)
        //{
        //    if (other == null)
        //        return false;
        //    if (ReferenceEquals(this, other))
        //        return true;
        //    if (!IsTransient(this) && !IsTransient(other) && Equals(Id, other.Id))
        //    {
        //        var otherType = other.GetUnproxiedType();
        //        var thisType = GetUnproxiedType();
        //        return thisType.IsAssignableFrom(otherType) ||
        //               otherType.IsAssignableFrom(thisType);
        //    }

        //    return false;
        //}

        ///// <summary>
        ///// Determines whether the specified entity is transient.
        ///// </summary>
        ///// <param name="obj">The object.</param>
        ///// <returns></returns>
        //private static bool IsTransient(Entity<TId> obj)
        //{
        //    return obj != null &&
        //           Equals(obj.Id, default(TId));
        //}

        ///// <summary>
        ///// Gets the type of the unproxied entity.
        ///// </summary>
        ///// <returns></returns>
        //private Type GetUnproxiedType()
        //{
        //    return GetType();
        //}


        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        //public override int GetHashCode()
        //{
        //    return Equals(Id, default(TId))
        //               ? base.GetHashCode()
        //               : Id.GetHashCode();
        //}

        public override bool Equals(object obj)
        {
            return EntityEquals(obj as Entity<TId>);
        }

        protected bool EntityEquals(Entity<TId> other)
        {
            if (other == null || !GetType().IsInstanceOfType(other))
            {
                return false;
            }

            // One entity is transient and the other is persistent.
            if (IsPersisted ^ other.IsPersisted)
            {
                return false;
            }

            // Both entities are not saved.
            if (IsPersisted && other.IsPersisted)
            {
                return ReferenceEquals(this, other);
            }

            // Compare transient instances.
            return Id.Equals(other.Id);
        }

        // The hash code is cached because a requirement of a hash code is that
        // it does not change once calculated. For example, if this entity was
        // added to a hashed collection when transient and then saved, we need
        // the same hash code or else it could get lost because it would no 
        // longer live in the same bin.
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            cachedHashCode = IsPersisted ? base.GetHashCode() : Id.GetHashCode();
            return cachedHashCode.Value;
        }

        // Maintain equality operator semantics for entities.
        public static bool operator ==(Entity<TId> x, Entity<TId> y)
        {
            // By default, == and Equals compares references. In order to 
            // maintain these semantics with entities, we need to compare by 
            // identity value. The Equals(x, y) override is used to guard 
            // against null values; it then calls EntityEquals().
            return Object.Equals(x, y);
        }

        // Maintain inequality operator semantics for entities. 
        public static bool operator !=(Entity<TId> x, Entity<TId> y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return !string.IsNullOrEmpty(Name)
                       ? Name
                       : Id.ToString();
        }

        #endregion

        #region IDataErrorInfo & Validation Members

        //public virtual string this[string columnName]
        //{
        //    get { return OnValidate(columnName); }
        //}

        //protected virtual string OnValidate(string columnName)
        //{
        //    //var context = new ValidationContext(this) { MemberName = columnName };

        //    //var results = new Collection<ValidationResult>();
        //    //bool isValid = Validator.TryValidateObject(this, context, results, true);

        //    //Error = !isValid ? results[0].ErrorMessage : null;
        //    //return Error;
        //    return null;
        //}

        //public virtual string Error { get; private set; }

        #endregion

        #region INotifyPropertyChanged Members
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the property whose name matches <see cref="propertyName"/>.
        /// </summary>
        /// <param name="propertyName">Optional. The name of the property whose value has changed.</param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void NotifyPropertyChanged(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                NotifyPropertyChanged(name);
            }
        }

        protected void NotifyPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propertyName = ExtractPropertyName(propertyExpression);
            NotifyPropertyChanged(propertyName);
        }

        protected string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException(@"BaseNotify: Property is not a member", "propertyExpression");
            }

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException(@"BaseNotify: Property is not a property", "propertyExpression");
            }

            var getMethod = property.GetGetMethod(true);

            if (getMethod == null)
            {

                throw new ArgumentException(@"BaseNotify: Property has no getter", "propertyExpression");
            }

            if (getMethod.IsStatic)
            {
                throw new ArgumentException(@"BaseNotify: Property is static", "propertyExpression");
            }

            return memberExpression.Member.Name;
        }

        #endregion

        #region INotifyDataErrorInfo

        private readonly ConcurrentDictionary<string, IEnumerable<string>> _errors = new ConcurrentDictionary<string, IEnumerable<string>>();

        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire entity.
        /// </summary>
        /// 
        [field: NonSerialized]
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <param name="propertyName">The name of the property to retrieve validation errors for; or null or <see cref="F:System.String.Empty" />, to retrieve entity-level errors.</param>
        /// <returns>
        /// The validation errors for the property or entity.
        /// </returns>
        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return null;

            IEnumerable<string> errors;
            _errors.TryGetValue(propertyName, out errors);
            return errors;
        }

        public bool HasErrorExcluding(List<string> excludedproperties)
        {
            return !(excludedproperties != null && _errors.Keys.All(excludedproperties.Contains));
        }

        public bool HasError(List<string> propertiesToCheck)
        {
            return propertiesToCheck != null && _errors.Keys.Any(propertiesToCheck.Contains);
        }


        /// <summary>
        /// Clears the errors.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpresssion">The property expresssion.</param>
        protected virtual void ClearErrors<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propertyName = ExtractPropertyName(propertyExpresssion);
            if (!_errors.ContainsKey(propertyName)) return;

            IEnumerable<string> removed;
            _errors.TryRemove(propertyName, out removed);
            RaiseErrorsChanged(propertyExpresssion);
        }

        /// <summary>
        /// Gets a value that indicates whether the entity has validation errors.
        /// </summary>
        public virtual bool HasErrors
        {
            get { return _errors.Any(); }
        }

        /// <summary>
        /// Raises the errors changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void RaiseErrorsChanged(string propertyName)
        {
            if (ErrorsChanged != null)
            {
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raises the errors changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpresssion">The property expresssion.</param>
        protected virtual void RaiseErrorsChanged<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propertyName = ExtractPropertyName(propertyExpresssion);
            RaiseErrorsChanged(propertyName);
        }

        /// <summary>
        /// Validates the entity asynchronously.
        /// </summary>
        /// <returns></returns>
        public Task ValidateAsync()
        {
            return Task.Run(() => Validate());
        }

        private readonly object _lock = new object();

        /// <summary>
        /// Validates the entity.
        /// </summary>
        public virtual void Validate()
        {
            lock (_lock)
            {
                var validationContext = new ValidationContext(this, null, null);
                var validationResults = new List<ValidationResult>();

                Validator.TryValidateObject(this, validationContext, validationResults, true);

                _errors.ToList().ForEach(x =>
                {
                    if ((validationResults.All(r => r.MemberNames.All(m => m != x.Key))))
                    {
                        IEnumerable<string> outList;
                        var tryRemove = _errors.TryRemove(x.Key, out outList);
                        RaiseErrorsChanged(x.Key);
                    }
                });

                var q = from r in validationResults
                        from m in r.MemberNames
                        group r by m
                            into g
                        select g;

                q.ToList().ForEach(x =>
                {
                    var messages = x.Select(r => r.ErrorMessage).ToList();
                    if (_errors.ContainsKey(x.Key))
                    {
                        IEnumerable<string> outList;
                        var tryRemove = _errors.TryRemove(x.Key, out outList);
                    }
                    _errors.TryAdd(x.Key, messages);
                    RaiseErrorsChanged(x.Key);
                });

            }
        }

        #endregion
    }

    public static class EntityExtensions
    {
        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <param name="shouldCloneId">if set to <c>true</c> [should clone identifier].</param>
        /// <returns>
        /// The copied object.
        /// </returns>
        /// <exception cref="System.ArgumentException">The type must be serializable.;source</exception>
        public static T Clone<T, TId>(this T source, bool shouldCloneId = false) where T : IEntity<TId>
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            var result = default(T);

            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                result = (T)formatter.Deserialize(stream);
            }

            if (!shouldCloneId)
                result.Id = default(TId);

            return result;
        }
    }

    [ImplementPropertyChanged]
    [Serializable]
    public abstract class OwnedEntity<TOwner, TOwnerId, TId> : Entity<TId>, IOwnedEntity<TOwner, TOwnerId, TId>
        where TOwner : Entity<TOwnerId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OwnedEntity{TOwner, TOwnerId, TId}"/> class.
        /// </summary>
        protected OwnedEntity()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnedEntity{TOwner, TOwnerId, TId}"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="name">The name.</param>
        protected OwnedEntity(TOwner owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>
        /// The owner.
        /// </value>
        public virtual TOwner Owner { get; set; }
    }

    public abstract class EntityExtension<TExtended, TId> : OwnedEntity<TExtended, TId, TId>
        where TExtended : Entity<TId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityExtension{TExtended, TId}"/> class.
        /// </summary>
        protected EntityExtension()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityExtension{TExtended, TId}"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="name">The name.</param>
        protected EntityExtension(TExtended owner, string name)
            : base(owner)
        {
            Name = name;
        }
    }

    public abstract class EnumLookupEntity<TId> : Entity<TId>
    {
        public virtual int Value { get; set; }
    }
}

