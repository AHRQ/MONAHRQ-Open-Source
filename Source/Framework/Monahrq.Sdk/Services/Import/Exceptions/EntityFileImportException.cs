using System;
using System.Runtime.Serialization;

namespace Monahrq.Sdk.Services.Import.Exceptions
{
    /// <summary>
    /// A custom generic exception thrown when the file and/or line being imported is not formated correctly.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class EntityFileImportException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFileImportException"/> class.
        /// </summary>
        public EntityFileImportException()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFileImportException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="values">The values.</param>
        public EntityFileImportException(string message, params object[] values)
            : base(string.Format(message, values))
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFileImportException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public EntityFileImportException(string message)
            : base(message)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFileImportException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public EntityFileImportException(string message, Exception innerException)
            : base(message, innerException)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFileImportException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected EntityFileImportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}

