using System;
using System.Data;

namespace Monahrq.Infrastructure.Data.CustomTypes
{
    /// <summary>
    /// A custom Nhibernate type that the back and forth convertion of a full qualified assembly type to a string and from a string to a full qualified assembly type.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.CustomTypes.CustomType" />
    public class AssemblyQualifiedTypeString : CustomType
	{
        /// <summary>
        /// Gets the type of the underlying.
        /// </summary>
        /// <value>
        /// The type of the underlying.
        /// </value>
        protected override DbType UnderlyingType
		{
			get { return DbType.String; }
		}

        /// <summary>
        /// Retrieve an instance of the mapped class from a JDBC resultset.
        /// Implementors should handle possibility of null values.
        /// </summary>
        /// <param name="rs">a IDataReader</param>
        /// <param name="names">column names</param>
        /// <param name="owner">the containing entity</param>
        /// <returns></returns>
        public override object NullSafeGet(IDataReader rs, string[] names, object owner)
		{
			int ordinal = rs.GetOrdinal(names[0]);
			if (rs.IsDBNull(ordinal))
			{
				return null;
			}
			else
			{
				return Type.GetType(rs[ordinal].ToString());
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
        public override void NullSafeSet(IDbCommand cmd, object value, int index)
		{
			var temp = (value as Type) as object ?? DBNull.Value as object;
			object valueToSet = (temp == DBNull.Value) ? DBNull.Value as object
					: (value as Type).AssemblyQualifiedName;
			((IDbDataParameter)cmd.Parameters[index]).Value = valueToSet;
		}
	}
}