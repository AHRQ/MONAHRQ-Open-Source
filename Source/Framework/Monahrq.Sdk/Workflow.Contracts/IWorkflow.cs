using System;

namespace Monahrq.Sdk.Workflow.Contracts
{
    /// <summary>
    /// Defines the Monahrq Workflow interface.
    /// </summary>
    public interface IWorkflow
    {
        #region Events

        /// <summary>
        /// The <c>WorkflowComplete</c> event is raised when the workflow has finished
        /// execution.
        /// </summary>
        event EventHandler<WorkflowProgressEventArgs> Progress;

        /// <summary>
        /// This event is raised to indicate the percentage completion of the workflow.  
        /// </summary>
        event EventHandler<WorkflowCompleteEventArgs> WorkflowComplete;

        #endregion
        
        #region Properties

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        WorkflowStatusEnum Status { get; }

        /// <summary>
        /// Returns the unique identifier for this workflow
        /// </summary>
        string WorkflowID { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Begins the execution of the request.  When the execution has completed, the 
        /// WorkflowComplete event will be raised.
        /// </summary>
        void Execute();

        /// <summary>
        /// Cancels this workflow
        /// </summary>
        void Cancel();

        #endregion
    }
}
