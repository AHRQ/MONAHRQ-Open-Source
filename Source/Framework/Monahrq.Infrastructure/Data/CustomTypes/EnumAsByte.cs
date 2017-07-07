using System.Data;

namespace Monahrq.Infrastructure.Data.CustomTypes
{
    /// <summary>
    /// A custom Nhibernate type that converts a enumeration value to a byte and vice versa.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.CustomTypes.CustomType" />
    public class EnumAsByte : CustomType
	{
        /// <summary>
        /// Gets the type of the underlying.
        /// </summary>
        /// <value>
        /// The type of the underlying.
        /// </value>
        protected override DbType UnderlyingType
		{
			get
			{
				return DbType.Byte;
			}
		}
	}
}