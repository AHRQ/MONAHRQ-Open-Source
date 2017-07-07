using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Monahrq.Infrastructure.Utility;
using NHibernate.UserTypes;
using NHibernate.SqlTypes;
using System.Runtime.Serialization;
using NHibernate;

namespace Monahrq.Infrastructure.Data.CustomTypes
{
    /// <summary>
    /// The custom NHibernate type that serializes an object to a Json string representation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="NHibernate.UserTypes.IUserType" />
    public class JsonToStringType<T> : IUserType where T : class , new()
    {
        /// <summary>
        /// Reconstruct an object from the cacheable representation. At the very least this
        /// method should perform a deep copy if the type is mutable. (optional operation)
        /// </summary>
        /// <param name="cached">the object to be cached</param>
        /// <param name="owner">the owner of the cached object</param>
        /// <returns>
        /// a reconstructed object from the cachable representation
        /// </returns>
        public object Assemble(object cached, object owner)
        {
            return DeepCopy(cached);
        }

        /// <summary>
        /// Return a deep copy of the persistent state, stopping at entities and at collections.
        /// </summary>
        /// <param name="value">generally a collection element or entity field</param>
        /// <returns>
        /// a copy
        /// </returns>
        public object DeepCopy(object value)
        {
            return Clone((T)value);
        }

        /// <summary>
        /// Transform the object into its cacheable representation. At the very least this
        /// method should perform a deep copy if the type is mutable. That may not be enough
        /// for some implementations, however; for example, associations must be cached as
        /// identifier values. (optional operation)
        /// </summary>
        /// <param name="value">the object to be cached</param>
        /// <returns>
        /// a cacheable representation of the object
        /// </returns>
        public object Disassemble(object value)
        {
            return DeepCopy(value);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="x">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public new bool Equals(object x, object y)
        {
            if (x == null || y == null)
                return false;

            var lo = (T)x;
            var ro = (T)y;

            return lo.Equals(ro);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        /// <summary>
        /// Are objects of this type mutable?
        /// </summary>
        public bool IsMutable
        {
            get { return true; }
        }

        /// <summary>
        /// Retrieve an instance of the mapped class from a JDBC resultset.
        /// Implementors should handle possibility of null values.
        /// </summary>
        /// <param name="rs">a IDataReader</param>
        /// <param name="names">column names</param>
        /// <param name="owner">the containing entity</param>
        /// <returns></returns>
        public object NullSafeGet(System.Data.IDataReader rs, string[] names, object owner)
        {
            //We need to do this to check for null before creating a guid
            var dbResult = NHibernateUtil.String.NullSafeGet(rs, names[0], null, owner);
            if (dbResult == null)
                return default(T);

            var jsonObject = JsonHelper.Deserialize<T>(dbResult.ToString());

            var result = jsonObject;
            return result;
        }

        /// <summary>
        /// Write an instance of the mapped class to a prepared statement.
        /// Implementors should handle possibility of null values.
        /// A multi-column type should be written to parameters starting from index.
        /// </summary>
        /// <param name="cmd">a IDbCommand</param>
        /// <param name="value">the object to write</param>
        /// <param name="index">command parameter index</param>
        public void NullSafeSet(System.Data.IDbCommand cmd, object value, int index)
        {
            var jsonObject = value as T;

            if (jsonObject == null)
            {
                NHibernateUtil.String.NullSafeSet(cmd, null, index, null);
                return;
            }
            var dbValue = JsonHelper.Serialize(jsonObject);
            NHibernateUtil.String.NullSafeSet(cmd, dbValue, index, null);
        }

        /// <summary>
        /// During merge, replace the existing (<paramref name="target" />) value in the entity
        /// we are merging to with a new (<paramref name="original" />) value from the detached
        /// entity we are merging. For immutable objects, or null values, it is safe to simply
        /// return the first parameter. For mutable objects, it is safe to return a copy of the
        /// first parameter. For objects with component values, it might make sense to
        /// recursively replace component values.
        /// </summary>
        /// <param name="original">the value from the detached entity being merged</param>
        /// <param name="target">the value in the managed entity</param>
        /// <param name="owner">the managed entity</param>
        /// <returns>
        /// the value to be merged
        /// </returns>
        public object Replace(object original, object target, object owner)
        {
            return DeepCopy(original);
        }

        /// <summary>
        /// The type returned by <c>NullSafeGet()</c>
        /// </summary>
        public Type ReturnedType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// The SQL types for the columns mapped by this type.
        /// </summary>
        public SqlType[] SqlTypes
        {
            get { return new SqlType[] { new StringSqlType(CustomType.MAX_STRING_LENGTH) }; }
        }

        /// <summary>
        /// Clones the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The type must be serializable.;source</exception>
        public static T Clone(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
