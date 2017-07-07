using System;
using System.Data;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Monahrq.Infrastructure.Data.CustomTypes
{
    /// <summary>
    /// the abstract custom Nhibernate type that can be used to create other custom Nhibernate types.
    /// </summary>
    /// <seealso cref="NHibernate.UserTypes.IUserType" />
    public abstract class CustomType : IUserType
	{
        /// <summary>
        /// The maximum string length
        /// </summary>
        public const int MAX_STRING_LENGTH = 10000;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomType"/> class.
        /// </summary>
        protected CustomType()
		{
			VariantSqlType = new SqlType[1] { new SqlType(UnderlyingType) };
		}

        /// <summary>
        /// Gets the type of the underlying.
        /// </summary>
        /// <value>
        /// The type of the underlying.
        /// </value>
        protected abstract DbType UnderlyingType { get; }

        /// <summary>
        /// Gets or sets the type of the variant SQL.
        /// </summary>
        /// <value>
        /// The type of the variant SQL.
        /// </value>
        private SqlType[] VariantSqlType { get; set; }

        /// <summary>
        /// The SQL types for the columns mapped by this type.
        /// </summary>
        public SqlType[] SqlTypes
		{
			get
			{
				return VariantSqlType;
			}
		}

        /// <summary>
        /// The type returned by <c>NullSafeGet()</c>
        /// </summary>
        public Type ReturnedType
		{
			get { return typeof(object); }
		}

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="x">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public new bool Equals(object x, object y)
		{
			return object.Equals(x, y);
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
        /// Retrieve an instance of the mapped class from a JDBC resultset.
        /// Implementors should handle possibility of null values.
        /// </summary>
        /// <param name="rs">a IDataReader</param>
        /// <param name="names">column names</param>
        /// <param name="owner">the containing entity</param>
        /// <returns></returns>
        public virtual object NullSafeGet(IDataReader rs, string[] names, object owner)
		{
			int ordinal = rs.GetOrdinal(names[0]);
			if (rs.IsDBNull(ordinal))
			{
				return null;
			}
			else
			{
				return rs[ordinal];
			}
		}

        /// <summary>
        /// Write an instance of the mapped class to a prepared statement.
        /// Implementors should handle possibility of null values.
        /// A multi-column type should be written to parameters starting from index.
        /// </summary>
        /// <param name="cmd">a IDbCommand</param>
        /// <param name="value">the object to write</param>
        /// <param name="index">command parameter index</param>
        public virtual void NullSafeSet(IDbCommand cmd, object value, int index)
		{
			object valueToSet = (value == null) ? DBNull.Value : value;
			((IDbDataParameter)cmd.Parameters[index]).Value = valueToSet;
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
			return value;
		}

        /// <summary>
        /// Are objects of this type mutable?
        /// </summary>
        public bool IsMutable
		{
			get { return false; }
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
			return original;
		}

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
			return cached;
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
			return value;
		}
	}
}