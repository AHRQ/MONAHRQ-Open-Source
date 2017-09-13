using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure;

namespace Monahrq.Sdk.Logging
{
    /// <summary>
    /// A logger that holds on to log entries until a callback delegate is set, then plays back log entries and sends new log entries.
    /// </summary>
    //[Export(LogNames.Operations, typeof(ILogWriter))]
    //[Export(LogNames.Operations, typeof(ILoggerFacade))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CallbackLogger : LoggerBase
    {
        private readonly Queue<Tuple<string, Category, Priority>> _savedLogs =
            new Queue<Tuple<string, Category, Priority>>();

        Action<string, Category, Priority> _callback; 
        /// <summary>
        /// Gets or sets the callback to receive logs.
        /// </summary>
        /// <value>An Action&lt;string, Category, Priority&gt; callback.</value>
        public Action<string, Category, Priority> Callback 
        {
            get
            {
                Action<string, Category, Priority> action = null;
                Interlocked.Exchange<Action<string, Category, Priority>>(ref action, _callback);
                return action;
            }
            set
            {
                Interlocked.Exchange<Action<string, Category, Priority>>(ref _callback, value);
            }
        }

        /// <summary>
        /// Write a new log entry with the specified category and priority.
        /// </summary>
        /// <param name="message">Message body to log.</param>
        /// <param name="category">Category of the entry.</param>
        /// <param name="priority">The priority of the entry.</param>
        protected override void OnLog(string message, Category category, Priority priority)
        {
            var  action = Callback;          
            if (action != null)
            {
                action(message, category, priority);
            }
            else
            {
                _savedLogs.Enqueue(new Tuple<string, Category, Priority>(message, category, priority));
            }
        }

        /// <summary>
        /// Replays the saved logs if the Callback has been set.
        /// </summary>
        public void ReplaySavedLogs()
        {
            var action = Callback;
            if (action != null)
            {
                var log = _savedLogs.Dequeue();
                action(log.Item1, log.Item2, log.Item3);
                ReplaySavedLogs();
            }
        }
    }
}
