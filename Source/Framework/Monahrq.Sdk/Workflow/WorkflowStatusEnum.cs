using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Sdk.Workflow
{
    /// <summary>
    /// Enumerates the states that the workflow can be in.
    /// </summary>
    public enum WorkflowStatusEnum
    {
        /// <summary>
        /// The workflow has not been executed
        /// </summary>
        Pending,

        /// <summary>
        /// The workflow is executing
        /// </summary>
        Executing,

        /// <summary>
        /// The workflow has been canceled
        /// </summary>
        Cancelled,

        /// <summary>
        /// The workflow encountered an error
        /// </summary>
        Error,

        /// <summary>
        /// The workflow executed successfully
        /// </summary>
        Success,
    }
}
