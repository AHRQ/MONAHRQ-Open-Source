using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure
{
    /// <summary>
    /// The custom logger MEF DI contract names
    /// </summary>
    public static class LogNames
    {
        /// <summary>
        /// The session
        /// </summary>
        public const string Session = "session";
        /// <summary>
        /// The operations
        /// </summary>
        public const string Operations = "operations";
    }

    /// <summary>
    /// The log writer interface.
    /// </summary>
    public interface ILogWriter 
    {
        /// <summary>
        /// Writes the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void Write(Exception exception);
        /// <summary>
        /// Writes the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="severity">The severity.</param>
        void Write(Exception exception, System.Diagnostics.TraceEventType severity);
        /// <summary>
        /// Writes the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="items">The items.</param>
        void Write(Exception exception, System.Diagnostics.TraceEventType severity, IDictionary items);
        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Write(string message);
        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="severity">The severity.</param>
        void Write(string message, System.Diagnostics.TraceEventType severity);
        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="items">The items.</param>
        void Write(string message, System.Diagnostics.TraceEventType severity, IDictionary items);
        /// <summary>
        /// Debugs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Debug(string message, params object[] args);
        /// <summary>
        /// Warnings the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Warning(string message, params object[] args);
        /// <summary>
        /// Informations the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Information(string message, params object[] args);
    }

    /// <summary>
    /// A custom Null Logger
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.ILogWriter" />
    public class NullLogger : ILogWriter
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="NullLogger"/> class from being created.
        /// </summary>
        NullLogger() { }
        /// <summary>
        /// The instance
        /// </summary>
        static ILogWriter _instance = new NullLogger();
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static ILogWriter Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Writes the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void Write(Exception exception)
        { }

        /// <summary>
        /// Writes the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="severity">The severity.</param>
        public void Write(Exception exception, System.Diagnostics.TraceEventType severity)
        { }

        /// <summary>
        /// Writes the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="items">The items.</param>
        public void Write(Exception exception, System.Diagnostics.TraceEventType severity, IDictionary items)
        { }

        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Write(string message)
        { }

        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="severity">The severity.</param>
        public void Write(string message, System.Diagnostics.TraceEventType severity)
        { }

        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="items">The items.</param>
        public void Write(string message, System.Diagnostics.TraceEventType severity, IDictionary items)
        { }

        /// <summary>
        /// Debugs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void Debug(string message, params object[] args)
        { }

        /// <summary>
        /// Warnings the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void Warning(string message, params object[] args)
        {}

        /// <summary>
        /// Informations the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void Information(string message, params object[] args)
        {}

        /// <summary>
        /// Errors the specified ex.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="details">The details.</param>
        /// <param name="args">The arguments.</param>
        public void Error(Exception ex, string details, params object[] args)
        { }
    }
}
