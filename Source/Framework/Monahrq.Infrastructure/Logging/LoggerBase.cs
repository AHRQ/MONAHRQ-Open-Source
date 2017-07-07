using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure;

namespace Monahrq.Sdk.Logging
{
    /// <summary>
    /// The abstract/base Logger class.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.Logging.ILoggerFacade" />
    /// <seealso cref="Monahrq.Infrastructure.ILogWriter" />
    public abstract class LoggerBase : ILoggerFacade, ILogWriter
    {
        /// <summary>
        /// Write a new log entry with the specified category and priority.
        /// </summary>
        /// <param name="message">Message body to log.</param>
        /// <param name="category">Category of the entry.</param>
        /// <param name="priority">The priority of the entry.</param>
        protected abstract void OnLog(string message, Category category, Priority priority);

        /// Write a new log entry with the specified category and priority.
        /// </summary>
        /// <param name="message">Message body to log.</param>
        /// <param name="category">Category of the entry.</param>
        /// <param name="priority">The priority of the entry.</param>
        public void Log(string message, Category category, Priority priority)
        {
            OnLog(message, category, priority);
        }

        /// <summary>
        /// Writes the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void Write(Exception exception)
        {
            Write(exception.InnerException ?? exception, TraceEventType.Error);
        }

        /// <summary>
        /// Writes the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="severity">The severity.</param>
        public void Write(Exception exception, TraceEventType severity)
        {
            Write(exception.InnerException ?? exception, severity, null);
        }

        /// <summary>
        /// Writes the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="items">The items.</param>
        public void Write(Exception exception, TraceEventType severity, System.Collections.IDictionary items)
        {
            string message = string.Format("{0}{1}{2}{1}", exception.Message, System.Environment.NewLine, exception.StackTrace);
            Write(message, severity, items);
        }

        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Write(string message)
        {
            Write(message, TraceEventType.Information, null);
        }

        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="severity">The severity.</param>
        public void Write(string message, TraceEventType severity)
        {
            Write(message, severity, null);
        }

        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="items">The items.</param>
        public void Write(string message, TraceEventType severity, System.Collections.IDictionary items)
        {
            var dictionary = items == null 
                                    ? new Dictionary<string, string>() 
                                    : items.Keys.OfType<object>().Select(k =>
                                        {
                                            return new KeyValuePair<string, string>(k.ToString(), items[k] == null
                                                                                                      ? string.Empty
                                                                                                      : items[k].ToString());
                                        })
                        .ToDictionary(k => k.Key, v => v.Value);

            message = dictionary.Count == 0 ? message
                : string.Format("{0}{1}{2}",
                message,
                System.Environment.NewLine + "\t",
                string.Join(System.Environment.NewLine + "\t",
                    dictionary.Select(kvp => string.Format("{0}: {1}", kvp.Key, kvp.Value))));
            var category = 0 != (int)(severity & (TraceEventType.Critical | TraceEventType.Error))
                ? Category.Exception
                : 0 != (int)(severity & TraceEventType.Warning)
                ? Category.Warn
                : Category.Info;
            var priority = severity == TraceEventType.Critical
                ? Priority.High
                : severity == TraceEventType.Error
                ? Priority.Medium
                : severity == TraceEventType.Warning
                ? Priority.Low
                : Priority.None;
            Log(message, category, priority);
        }

        /// <summary>
        /// Debugs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void Debug(string message, params object[] args)
        {
            Write(string.Format(message, args), TraceEventType.Verbose);
        }

        /// <summary>
        /// Warnings the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void Warning(string message, params object[] args)
        {
            Write(string.Format(message, args), TraceEventType.Warning);
        }

        /// <summary>
        /// Informations the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void Information(string message, params object[] args)
        {
            Write(string.Format(message, args), TraceEventType.Information);
        }
    }
}
