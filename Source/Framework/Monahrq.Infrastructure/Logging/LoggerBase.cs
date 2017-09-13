using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
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

        public void Write(Exception exception)
        {
            var bt = new StackTrace();
            var method = bt.GetFrame(1).GetMethod();
            Write(exception, "Error in {0}.{1}({2})",
                method.DeclaringType?.FullName ?? "<anonymous type>",
                method.Name,
                string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))
                );
        }
        /// <summary>
        /// Write a new log entry with the specified category and priority.
        /// </summary>
        /// <param name="message">Message body to log.</param>
        /// <param name="category">Category of the entry.</param>
        /// <param name="priority">The priority of the entry.</param>
        public void Log(string message, Category category, Priority priority)
        {
            OnLog(message, category, priority);
        }

        public void Write(Exception exception, string message)
        {
            WriteInternal(message, null, TraceEventType.Error, null, exception);
        }

        public void Write(Exception exception, string message, params object[] args)
        {
            WriteInternal(message, args, TraceEventType.Error, null, exception);
        }

        /// <summary>
        /// Writes the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="severity">The severity.</param>
        public void Write(Exception exception, TraceEventType severity, string message, params object[] args)
        {
            WriteInternal(message, args, severity, null, exception);
        }

        /// <summary>
        /// Writes the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="items">The items.</param>
        public void Write(Exception exception, TraceEventType severity, System.Collections.IDictionary items, string message, params object[] args)
        {
            WriteInternal(message, args, severity, items, exception);
        }

        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Write(string message)
        {
            WriteInternal(message, null, TraceEventType.Information, null, null);
        }

        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="severity">The severity.</param>
        public void Write(string message, TraceEventType severity)
        {
            WriteInternal(message, null, severity, null, null);
        }

        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="items">The items.</param>
        public void Write(string message, TraceEventType severity, System.Collections.IDictionary items)
        {
            WriteInternal(message, null, severity, items, null);
        }

        void WriteInternal(string message, object[] args, TraceEventType severity, System.Collections.IDictionary items, Exception e)
        {
            if (args?.Length > 0 || items?.Count > 0 || e != null)
            {
                var sb = new StringBuilder(message?.Length ?? 0
                    + ((args?.Length ?? 0) * 100) // assume average string.Format argument size is 100
                    + ((items?.Count ?? 0) * 100) // assume average item size is 100
                    + (e == null ? 0 : 4096) // allocate 4k for exception
                    );
                // construct message from arguments
                if (message != null)
                    sb.AppendFormat(message, args);

                // append any data items
                if (items?.Count > 0)
                    foreach (var key in items.Keys)
                        sb.AppendFormat("\n\t{0}: {1}", key, items[key]);

                // finally append exception stack
                if (e != null)
                    AppendException(sb, e);

                // compile string
                message = sb.ToString();
            }

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

            OnLog(message, category, priority);
        }

        #region Exception Parsing
        /// <summary>
        /// Appends the exception, it's details, and all of it's inner exceptions to the given <see cref="StringBuilder"/> instance
        /// </summary>
        private void AppendException(StringBuilder sb, Exception ex)
        {
            if (ex == null)
                return;

            sb.AppendFormat("\n{0}: {1}", ex.GetType().Name, ex.Message);

            IEnumerable<Exception> innerExceptions = Enumerable.Empty<Exception>();
            // ReSharper disable PossibleMultipleEnumeration
            try
            {
                var customHandling = HandleException(sb, ref innerExceptions, ex as ReflectionTypeLoadException)
                      || HandleException(sb, ref innerExceptions, ex as AggregateException)
                      || HandleException(sb, ref innerExceptions, ex as SqlException)
                      || HandleException(sb, ref innerExceptions, ex as CompositionException);
                if (!customHandling)
                {
                    if (ex.InnerException != null)
                        innerExceptions = new[] {ex.InnerException};

                    foreach (var key in ex.Data.Keys)
                        sb.Append($"\n\t{key}: {ex.Data[key]}");
                }
            }
            catch { }

            sb.AppendLine();
            sb.Append(ex.StackTrace);

            foreach (var ie in innerExceptions)
                AppendException(sb, ie);
            // ReSharper restore PossibleMultipleEnumeration
        }

        private bool HandleException(StringBuilder sb, ref IEnumerable<Exception> innerExceptions, AggregateException e)
        {
            if (e == null) return false;
            innerExceptions = e.InnerExceptions;
            return true;
        }
        private bool HandleException(StringBuilder sb, ref IEnumerable<Exception> innerExceptions, ReflectionTypeLoadException e)
        {
            if (e == null) return false;
            innerExceptions = innerExceptions.Union(e.LoaderExceptions);
            sb.Append("\n\tTypes: ");
            sb.Append(string.Join(", ", e.Types.Select(t => t.FullName)));
            return true;
        }
        private bool HandleException(StringBuilder sb, ref IEnumerable<Exception> innerExceptions, SqlException e)
        {
            if (e == null) return false;
            sb.Append($"\n\tServer: {e.Server}");
            foreach (SqlError err in e.Errors)
            {
                sb.Append("\n\tError: ");
                sb.Append(err.ToString());
            }
            return true;
        }
        private bool HandleException(StringBuilder sb, ref IEnumerable<Exception> innerExceptions, CompositionException e)
        {
            if (e == null) return false;
            innerExceptions = e.RootCauses;
            // no reason to log these separately since they're included in e.Message
            //foreach (var error in e.Errors)
            //    sb.AppendFormat("\n\tError: {0} ({1})", error.Description, error.Element.DisplayName);
            return true;
        }
        #endregion

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
