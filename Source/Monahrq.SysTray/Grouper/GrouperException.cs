using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Exceptions
{
    /// <summary>
    /// Class for handling exception raised by Grouper element.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Exceptions.MonahrqCoreException" />
    /// <seealso cref="System.Runtime.Serialization.ISerializable" />
    public class GrouperException : MonahrqCoreException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperException"/> class.
        /// </summary>
        public GrouperException() : base() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public GrouperException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public GrouperException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected GrouperException(SerializationInfo info, StreamingContext context) { }
    }

    /// <summary>
    /// Class for handling IO exceptions raised by Grouper element
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Exceptions.GrouperException" />
    /// <seealso cref="System.Runtime.Serialization.ISerializable" />
    public class GrouperIOException : GrouperException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperIOException"/> class.
        /// </summary>
        public GrouperIOException() : base() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperIOException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public GrouperIOException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperIOException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public GrouperIOException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperIOException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected GrouperIOException(SerializationInfo info, StreamingContext context) { }
    }

    /// <summary>
    /// Class for handling exceptions raised by Grouper element
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Exceptions.GrouperException" />
    /// <seealso cref="System.Runtime.Serialization.ISerializable" />
    public class GrouperProcException : GrouperException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperProcException"/> class.
        /// </summary>
        public GrouperProcException() : base() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperProcException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public GrouperProcException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperProcException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public GrouperProcException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperProcException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected GrouperProcException(SerializationInfo info, StreamingContext context) { }
    }

    /// <summary>
    /// Class for handling exceptions raised by Grouper element
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Exceptions.GrouperException" />
    /// <seealso cref="System.Runtime.Serialization.ISerializable" />
    public class GrouperRecordException : GrouperException, ISerializable
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperRecordException"/> class.
        /// </summary>
        public GrouperRecordException() : base() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperRecordException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public GrouperRecordException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperRecordException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public GrouperRecordException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperRecordException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="fieldName">Name of the field.</param>
        public GrouperRecordException(string message, string fieldName) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperRecordException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="inner">The inner.</param>
        public GrouperRecordException(string message, string fieldName, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperRecordException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected GrouperRecordException(SerializationInfo info, StreamingContext context) { }
    }
}
