using System;

namespace Monahrq.Infrastructure.Domain.Wings
{
    [Serializable]
    public enum DataTypeEnum
    {
        /// <summary>
        /// The boolean
        /// </summary>
        Boolean,
        /// <summary>
        /// The date
        /// </summary>
        Date,
        /// <summary>
        /// The date time
        /// </summary>
        DateTime,
        /// <summary>
        /// The decimal
        /// </summary>
        Decimal,
        /// <summary>
        /// The double
        /// </summary>
        Double,
        /// <summary>
        /// The unique identifier
        /// </summary>
        Guid,
        /// <summary>
        /// The int16
        /// </summary>
        Int16,
        /// <summary>
        /// The int32
        /// </summary>
        Int32,
        /// <summary>
        /// The int64
        /// </summary>
        Int64,
        /// <summary>
        /// The object
        /// </summary>
        Object,
        /// <summary>
        /// The s byte
        /// </summary>
        SByte,
        /// <summary>
        /// The single
        /// </summary>
        Single,
        /// <summary>
        /// The string
        /// </summary>
        String,
    }
}