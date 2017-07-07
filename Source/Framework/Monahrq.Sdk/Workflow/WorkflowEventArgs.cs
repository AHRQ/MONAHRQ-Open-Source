using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Sdk.Workflow
{
    /// <summary>
    /// Event arugments for the <c>WorflowComplete</c> event that is fired by a <c>Workflow</c>.
    /// </summary>
    public class WorkflowCompleteEventArgs : EventArgs
    {
        private WorkflowStatusEnum _workflowStatus;

        /// <summary>
        /// Flag indicating if the action item succeeded.
        /// </summary>
        public bool Succeeded { get { return _workflowStatus == WorkflowStatusEnum.Success; } }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Workflow"/> was cancelled.
        /// </summary>
        /// <value><c>true</c> if cancelled; otherwise, <c>false</c>.</value>
        public bool Cancelled { get { return _workflowStatus == WorkflowStatusEnum.Cancelled; } }

        /// <summary>
        /// If the <c>Succeeded</c> flag is set to <c>false</c> then this property
        /// will indicate the exception that was thrown.
        /// </summary>
        public readonly Exception FailureReason;

        /// <summary>
        /// Unique id of workflow that generated this event.
        /// </summary>
        public readonly string WorkflowId;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ActionEventArgs"/> class.
        /// </summary>
        /// <param name="workflowId">The workflow ID.</param>
        public WorkflowCompleteEventArgs(string workflowId)
        {
            _workflowStatus = WorkflowStatusEnum.Success;
            WorkflowId = workflowId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ActionEventArgs"/> class.
        /// </summary>
        /// <param name="workflowId">The workflow ID.</param>
        /// <param name="failureReason">The failure reason.</param>
        public WorkflowCompleteEventArgs(string workflowId, Exception failureReason)
        {
            WorkflowId = workflowId;
            _workflowStatus = WorkflowStatusEnum.Error;
            if (failureReason == null)
                failureReason = new ApplicationException("The operation was cancelled because the internet connection was dropped.");
            FailureReason = failureReason;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ActionEventArgs"/> class.
        /// </summary>
        /// <param name="workflowId">The workflow ID.</param>
        /// <param name="cancelled">if set to <c>true</c> [cancelled].</param>
        public WorkflowCompleteEventArgs(string workflowId, bool cancelled)
        {
            WorkflowId = workflowId;
            _workflowStatus = cancelled == true ? WorkflowStatusEnum.Cancelled : WorkflowStatusEnum.Error;
            FailureReason = new ApplicationException("Operation failed.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ActionEventArgs"/> class.
        /// </summary>
        /// <param name="workflowId">The workflow ID.</param>
        /// <param name="failureReason">The failure reason.</param>
        /// <param name="cancelled">if set to <c>true</c> [cancelled].</param>
        public WorkflowCompleteEventArgs(string workflowId, Exception failureReason, bool cancelled)
        {
            WorkflowId = workflowId;
            _workflowStatus = cancelled == true ? WorkflowStatusEnum.Cancelled : WorkflowStatusEnum.Error;
            if (_workflowStatus == WorkflowStatusEnum.Error) FailureReason = failureReason;
        }

    }


    /// <summary>
    /// Event arugments for the <c>WorflowComplete</c> event that is fired by a <c>Workflow</c>.
    /// </summary>
    public class WorkflowProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Unique id of workflow that generated this event.
        /// </summary>
        public readonly string WorkflowID;

        /// <summary>
        /// This value ranges from 0-100
        /// </summary>
        public readonly int PercentComplete;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ActionEventArgs"/> class.
        /// </summary>
        /// <param name="workflowID">The workflow ID.</param>
        /// <param name="percentComplete">The percent complete.</param>
        public WorkflowProgressEventArgs(string workflowID, int percentComplete)
        {
            WorkflowID = workflowID;
            PercentComplete = percentComplete;
        }

    }
}
