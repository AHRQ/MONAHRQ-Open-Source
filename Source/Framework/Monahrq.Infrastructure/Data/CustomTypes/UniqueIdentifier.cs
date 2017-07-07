using System.Data;

namespace Monahrq.Infrastructure.Data.CustomTypes
{
    /// <summary>
    /// Monahrq custom unique identifier (Guid) type.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.CustomTypes.CustomType" />
    public class UniqueIdentifier : CustomType
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
				return DbType.Guid;
			}
		}
	}
}