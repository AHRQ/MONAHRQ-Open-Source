using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure;

namespace Monahrq.TestSupport.Loggers
{
    [Export(typeof(ILogWriter))]
    public class DummyLogger : ILogWriter
    {

        public void Write(Exception exception)
        {

        }

        public void Write(Exception exception, System.Diagnostics.TraceEventType severity)
        {

        }

        public void Write(Exception exception, System.Diagnostics.TraceEventType severity, System.Collections.IDictionary items)
        {

        }

        public void Write(string message)
        {
        }

        public void Write(string message, System.Diagnostics.TraceEventType severity)
        {
        }

        public void Write(string message, System.Diagnostics.TraceEventType severity, System.Collections.IDictionary items)
        {
        }


        public void Debug(string message, params object[] args)
        {
        }

        public void Warning(string message, params object[] args)
        {
        }

        public void Information(string message, params object[] args)
        {
        }
    }
}
