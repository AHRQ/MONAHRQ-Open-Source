using System;

namespace Monahrq.Sdk.StateMachine
{
    /// <summary>
    /// The custom invalid state machine exception. This custom exception is used more as a generic exception if an existing
    /// exception doesn't currently exist.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class InvalidStateMachineException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateMachineException"/> class.
        /// </summary>
        public InvalidStateMachineException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateMachineException"/> class.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public InvalidStateMachineException(string msg) : base(msg) { }
    }
}