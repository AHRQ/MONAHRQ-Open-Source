using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Logging;

namespace Monahrq.Infrastructure.Logging
{
    public static class LoggingExtensions
    {
        public static void LogException(this ILoggerFacade logger, Exception e, string message, params object[] args)
        {
            var sb = new StringBuilder(1024);
            sb.AppendFormat(message, args);
            sb.AppendLine();
            sb.Append(e.ToString());
            logger.Log(sb.ToString(), Category.Exception, Priority.High);
        }
    }
}
