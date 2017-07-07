using System;

namespace Monahrq.Infrastructure.Exceptions
{
    /// <summary>
    /// This is a placeholder exception used to exit the application gracefully.
    /// </summary>
    [Serializable]
    public class ApplicationExitException : MonahrqCoreException
    {
        /// <summary>
        /// Initializes a new instance of the <see>
        ///                                       <cref>T:ApplicationExitException</cref>
        ///                                   </see>
        ///     class.
        /// </summary>
        public ApplicationExitException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see>
        ///                                       <cref>T:ApplicationExitException</cref>
        ///                                   </see>
        ///     class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ApplicationExitException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see>
        ///                                       <cref>T:ApplicationExitException</cref>
        ///                                   </see>
        ///     class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ApplicationExitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
