using System;
using System.Data;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Monahrq.Infrastructure.Data.CustomTypes
{

    /// <summary>
    /// Type to allow NHibernate to persist DateTime? objects
    /// </summary>
    public class NullableDateTimeType : IUserType
	{
		/// <summary>
		/// Gets a value indicating whether the value is mutable
		/// </summary>
		public bool IsMutable
		{
			get
			{
				// This item is immutable:
				return false;
			}
		}

		/// <summary>
		/// Gets the type returned by NullSafeGet()
		/// </summary>
		public Type ReturnedType
		{
			get
			{
				return typeof(DateTime?);
			}
		}

		/// <summary>
		/// Gets the SQL types for the columns mapped by this type. 
		/// </summary>
		public SqlType[] SqlTypes
		{
			get
			{
				return new[]
                {
                    new SqlType(DbType.DateTime)
                };
			}
		}

		/// <summary>
		/// Reconstruct an object from the cacheable representation. At the very least this method should perform a deep copy if the type is mutable.
		/// </summary>
		/// <param name="cached">The cached object</param>
		/// <param name="owner">The owner object</param>
		/// <returns>The assemled object</returns>
		public object Assemble(object cached, object owner)
		{
			// Used for caching. As our object is immutable we can return as is:
			return cached;
		}

		/// <summary>
		/// Return a deep copy of the persistent state, stopping at entities and at collections. 
		/// </summary>
		/// <param name="value">The item to copy</param>
		/// <returns>The copied item</returns>
		public object DeepCopy(object value)
		{
			// We deep copy the item by creating a new instance with the same contents.
			// Note that this happens for free with value types because of the way 
			// that method parameters work:
			if (value == null)
			{
				return null;
			}

			return value as DateTime?;
		}

		/// <summary>
		/// Transform the object into its cacheable representation. At the very least this method should perform a deep copy 
		/// if the type is mutable. That may not be enough for some implementations, however; for example, associations must 
		/// be cached as identifier values.
		/// </summary>
		/// <param name="value">The cached object</param>
		/// <returns>The dassassemled object</returns>
		public object Disassemble(object value)
		{
			// Used for caching. As our object is immutable we can return as is:
			return value;
		}

		/// <summary>
		/// Compare two instances of the class mapped by this type for persistent "equality" ie. equality of persistent state 
		/// </summary>
		/// <param name="x">The first item</param>
		/// <param name="y">The second item</param>
		/// <returns>A value indicating whether the items are equal</returns>
		public new bool Equals(object x, object y)
		{
			if (x == null && y == null)
			{
				return true;
			}

			if (x == null)
			{
				return false;
			}

			return x.Equals(y);
		}

		/// <summary>
		/// Get a hashcode for the instance, consistent with persistence "equality" 
		/// </summary>
		/// <param name="x">The value to get the hash code for</param>
		/// <returns>The hash code</returns>
		public int GetHashCode(object x)
		{
			if (x == null)
			{
				return 0;
			}

			return x.GetHashCode();
		}

		/// <summary>
		/// Retrieve an instance of the mapped class from a resultset. Implementors should handle possibility of null values. 
		/// </summary>
		/// <param name="rs">The reader</param>
		/// <param name="names">The item names</param>
		/// <param name="owner">The owner object</param>
		/// <returns>The object requested</returns>
		public object NullSafeGet(IDataReader rs, string[] names, object owner)
		{
			// We get the DateTime from the database using the NullSafeGet used to get strings from NHibernateUtil:
			return NHibernateUtil.DateTime.NullSafeGet(rs, names[0]) as DateTime?;
		}

		/// <summary>
		/// Write an instance of the mapped class to a prepared statement. Implementors should handle possibility of null values. A multi-column type should be written to parameters starting from index. 
		/// </summary>
		/// <param name="cmd">The command</param>
		/// <param name="value">The value to use</param>
		/// <param name="index">The index to set</param>
		public void NullSafeSet(IDbCommand cmd, object value, int index)
		{
			// Convert to the correct type and set:
			var dateTimeValue = value as DateTime?;

			if (dateTimeValue == null)
			{
				NHibernateUtil.DateTime.NullSafeSet(cmd, null, index);
			}
			else
			{
				NHibernateUtil.DateTime.NullSafeSet(cmd, dateTimeValue.Value, index);
			}
		}

		/// <summary>
		/// During merge, replace the existing (target) value in the entity we are merging to with a new (original) 
		/// value from the detached entity we are merging. For immutable objects, or null values, it is safe to 
		/// simply return the first parameter. For mutable objects, it is safe to return a copy of the first parameter. 
		/// For objects with component values, it might make sense to recursively replace component values. 
		/// </summary>
		/// <param name="original">The original value</param>
		/// <param name="target">The target value</param>
		/// <param name="owner">The owner object</param>
		/// <returns>The replacement object</returns>
		public object Replace(object original, object target, object owner)
		{
			// As our object is immutable we can just return the original  
			return original;
		}
	}
}