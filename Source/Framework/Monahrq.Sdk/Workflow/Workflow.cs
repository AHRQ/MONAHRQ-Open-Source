using System;
using System.Collections.Generic;
using Monahrq.Sdk.Workflow.Contracts;

namespace Monahrq.Sdk.Workflow
{
    /// <summary>
    /// The <c>Workflow</c> class encapsulates a workflow around the domain services.  This
    /// object will sequence the domain service requests across the different requests.  This workflow
    /// executes asynchronously.  When the execute method is kicked off, the workflow will return a unique id
    /// that will identify the workflow.  When the workflow is complete, the WorkflowComplete Event will be
    /// triggered.
    /// </summary>
    public abstract class Workflow : IWorkflow
    {
        #region Private Memebers
        private string _workflowID = Guid.NewGuid().ToString();
        private Dictionary<string, IWorkflowActionItem> _actionItems = new Dictionary<string, IWorkflowActionItem>();
        #endregion

        #region Events

        /// <summary>
        /// The <c>WorkflowComplete</c> event is raised when the workflow has finished
        /// execution.
        /// </summary>
        abstract public event EventHandler<WorkflowCompleteEventArgs> WorkflowComplete;

        /// <summary>
        /// This event is raised to indicate the percentage completion of the workflow.  
        /// </summary>
        public event EventHandler<WorkflowProgressEventArgs> Progress;

        #endregion


        #region Protected Members

        /// <summary>
        /// Flag indicates that the workflow should be cancelled.
        /// </summary>
        protected bool _cancelWorkflow = false;

        /// <summary>
        /// Indicates the current status of the workflow.
        /// </summary>
        protected WorkflowStatusEnum _status = WorkflowStatusEnum.Pending;

        /// <summary>
        /// Gets the collection action items.
        /// </summary>
        /// <value>The action items.</value>
        protected Dictionary<string, IWorkflowActionItem> ActionItems { get { return _actionItems; } }

        /// <summary>
        /// Executes the workflow.
        /// </summary>
        abstract protected void ExecuteWorkflow();


        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the unique identifier for this workflow
        /// </summary>
        public string WorkflowID { get { return _workflowID; } }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        public WorkflowStatusEnum Status { get { return _status; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Begins the execution of the request.  The request is asyncronous.  When the execution
        /// has completed, the WorkflowComplete event will be raised.
        /// </summary>
        public void Execute()
        {
            if (_status != WorkflowStatusEnum.Pending)
                throw new InvalidOperationException("The Workflow was in an incorrect state!");

            _status = WorkflowStatusEnum.Executing;

            //Call the implementattion of the derived class to begin execution
            ExecuteWorkflow();
        }

        /// <summary>
        /// Cancels this workflow
        /// </summary>
        public void Cancel()
        {
            if (_status != WorkflowStatusEnum.Pending && _status != WorkflowStatusEnum.Executing)
                throw new InvalidOperationException("The workflow can not be cancelled. It's is in an incorrect state!");

            _cancelWorkflow = true;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Raises the progress event.
        /// </summary>
        /// <param name="percentComplete">The percent complete.</param>
        internal void RaiseProgressEvent(int percentComplete)
        {
            if (Progress != null)
                Progress(this, new WorkflowProgressEventArgs(_workflowID, percentComplete));
        }

        /// <summary>
        /// Raises the progress event.
        /// </summary>
        /// <param name="actionsComplete">The actions complete.</param>
        /// <param name="totalActions">The total actions.</param>
        internal void RaiseProgressEvent(int actionsComplete, int totalActions)
        {
            double percent = ((double)actionsComplete / (double)totalActions) * 100;
            double percentComplete = Math.Round(percent);

            if (Progress != null)
                Progress(this, new WorkflowProgressEventArgs(_workflowID, Convert.ToInt16(percentComplete)));
        }

        #endregion
    }
}
