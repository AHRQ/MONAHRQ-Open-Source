using System;

namespace Monahrq.Sdk.StateMachine
{
    /// <summary>
    /// The custom state machine invald action trigger exception.
    /// </summary>
    /// <seealso cref="System.Exception" />
    class InvalidTriggerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTriggerException"/> class.
        /// </summary>
        public InvalidTriggerException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTriggerException"/> class.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public InvalidTriggerException(string msg) : base(msg) { }
    }
}
