using System;

namespace Monahrq.Sdk.Generators
{
    /// <summary>
    /// Custom Exception Type to be used in the <see cref="Monahrq.Sdk.Services.Generators.WebsiteGenerator"/> to designate that there
    /// was a website publishing exception.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public sealed class PublishException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PublishException(string message) : base(message)
        {}
    }
}
