using System;
using System.Runtime.Serialization;

namespace Monahrq.Sdk.Services.Import.Exceptions
{
    /// <summary>
    /// custom exception used in the generic entity file import process to designate when a imported line column count is invalid.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Services.Import.Exceptions.EntityFileImportException" />
    [Serializable]
    public class InvalidDataLineException: EntityFileImportException 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDataLineException"/> class.
        /// </summary>
        /// <param name="nReqValuesPerLine">The n req values per line.</param>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="nValsFound">The n vals found.</param>
        public InvalidDataLineException(int nReqValuesPerLine, string contractName, int nValsFound):
            base(string.Format("{0} import requires {1} values per line. {2} values read.", contractName, nReqValuesPerLine, nValsFound))
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDataLineException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected InvalidDataLineException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}

