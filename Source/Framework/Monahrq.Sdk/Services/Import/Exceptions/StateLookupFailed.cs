using System;
using System.Runtime.Serialization;

namespace Monahrq.Sdk.Services.Import.Exceptions
{
    /// <summary>
    /// State Loop Failed custom exception used in the generic <seealso cref="EntityFileImporter"/> when validation of state values are needed.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Services.Import.Exceptions.EntityFileImportException" />
    [Serializable]
    public class StateLookupFailed : EntityFileImportException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateLookupFailed"/> class.
        /// </summary>
        public StateLookupFailed()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="StateLookupFailed"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public StateLookupFailed(string message)
            : base(message)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="StateLookupFailed"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public StateLookupFailed(string message, Exception innerException)
            : base(message, innerException)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="StateLookupFailed"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected StateLookupFailed(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}
