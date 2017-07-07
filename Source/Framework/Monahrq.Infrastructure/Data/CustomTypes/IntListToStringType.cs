using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Monahrq.Infrastructure.Data.CustomTypes
{

	/// <summary>
	/// Custom Nibernate type that converts a list of Int32 to a comma delimited string list and vice versa.
	/// </summary>
	public class IntListToStringType : IUserType
	{
		#region IUserType Members

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="x">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <param name="y">The y.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public new bool Equals(object x, object y)
		{
			if (x == null || y == null)
				return true;

			var la = (IList<int>)x;
			var ra = (IList<int>)y;

			return la.Equals(ra);        // use the Equals Overloads! Important

		}

		/// <summary>
		/// Return a deep copy of the persistent state, stopping at entities and at collections.
		/// </summary>
		/// <param name="value">generally a collection element or entity field</param>
		/// <returns>
		/// a copy
		/// </returns>
		public virtual object DeepCopy(object value)
		{
			if (value == null) return null;

			var dic = (IList<int>)value;
			var array = new int[dic.Count];
			dic.CopyTo(array, 0);

			return array.ToList();
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public virtual int GetHashCode(object x)
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
		/// <exception cref="T:NHibernate.HibernateException">HibernateException</exception>
		public virtual object NullSafeGet(IDataReader rs, string[] names, object owner)
		{
			//We need to do this to check for null before creating a guid
			object dbResult = NHibernateUtil.String.NullSafeGet(rs, names[0], null, owner);
			if (dbResult == null)
				return new List<int>();

			var result = dbResult.ToString().Split(',').ToList().ConvertAll(int.Parse);
			return result;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="value"></param>
		/// <param name="index"></param>
		public void NullSafeSet(IDbCommand cmd, object value, int index)
		{
			var list = value as IList<int>;

			if (list == null || list.Count == 0)
			{
				NHibernateUtil.String.NullSafeSet(cmd, null, index, null);
				return;
			}

			var dbValue = string.Join(",", list);
			NHibernateUtil.String.NullSafeSet(cmd, dbValue, index, null);
		}


		/// <summary>
		/// The type returned by <c>NullSafeGet()</c>
		/// </summary>
		public virtual Type ReturnedType
		{
			get { return typeof(List<int>); }
		}


		/// <summary>
		/// The SQL types for the columns mapped by this type.
		/// </summary>
		public SqlType[] SqlTypes
		{
			get { return new SqlType[] { new StringSqlType(CustomType.MAX_STRING_LENGTH) }; }
		}

		/// <summary>
		/// Reconstruct an object from the cacheable representation. At the very least this
		/// method should perform a deep copy if the type is mutable. (optional operation)
		/// </summary>
		/// <param name="cached">the object to be cached</param>
		/// <param name="owner">the owner of the cached object</param>
		/// <returns>
		/// a reconstructed object from the cacheable representation
		/// </returns>
		public object Assemble(object cached, object owner)
		{
			return DeepCopy(cached);
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
		/// During merge, replace the existing (<paramref name="target"/>) value in the entity
		/// we are merging to with a new (<paramref name="original"/>) value from the detached
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

		#endregion
	}


}