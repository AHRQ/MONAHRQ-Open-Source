using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Measures.Service
{
    /// <summary>
    /// Custom exception class to handle Measure related exceptions
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class MeasureServiceException: Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureServiceException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MeasureServiceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return string.Format("{0}{1}{2}", base.Message, Environment.NewLine, "Please consult the session log for details.");
            }
        }

      

    }
}
