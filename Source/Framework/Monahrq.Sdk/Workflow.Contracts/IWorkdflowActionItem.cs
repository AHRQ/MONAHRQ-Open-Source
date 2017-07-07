using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Sdk.Workflow.Contracts
{
    /// <summary>
    /// An <c>ActionItem</c> encapsulates a single unit of work within a workflow.
    /// </summary>
    public interface IWorkflowActionItem
    {
        /// <summary>
        /// The <c>ActionComplete</c> event is raised when the action item has finished.
        /// </summary>
        event EventHandler<WorkflowActionEventArgs> ActionComplete;

        /// <summary>
        /// Executes this action item.
        /// </summary>
        void Execute();
    }

    /// <summary>
    /// Event arguments for the <c>ActionComplete</c> event that is fired by an action item.
    /// </summary>
    public class WorkflowActionEventArgs : EventArgs
    {
        /// <summary>
        /// Flag indicating if the action item succeeded.
        /// </summary>
        public readonly bool Succeeded;

        /// <summary>
        /// Flag indicating whether the connection was dropped during execution of the action item
        /// </summary>
        public readonly bool ConnectionDropped = false;

        /// <summary>
        /// Error message
        /// </summary>
        public readonly Exception FailureReason;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WorkflowActionEventArgs"/> class.
        /// </summary>
        public WorkflowActionEventArgs()
        {
            Succeeded = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WorkflowActionEventArgs"/> class.
        /// </summary>
        /// <param name="failureReason">The failure reason.</param>
        public WorkflowActionEventArgs(Exception failureReason)
        {
            FailureReason = failureReason;
            Succeeded = failureReason == null ? true : false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WorkflowActionEventArgs"/> class.
        /// </summary>
        /// <param name="failureReason">The failure reason.</param>
        /// <param name="connectionDropped">if set to <c>true</c> [connection dropped].</param>
        public WorkflowActionEventArgs(Exception failureReason, bool connectionDropped)
            : this(failureReason)
        {
            ConnectionDropped = connectionDropped;
            if (ConnectionDropped) FailureReason = new Exception("Internet connection dropped!");
            Succeeded = FailureReason == null ? true : false;
        }

    }
}
