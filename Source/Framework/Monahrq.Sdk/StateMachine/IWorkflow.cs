using System;

namespace Monahrq.Sdk.StateMachine
{
    /// <summary>
    /// The state machine workflow step interface.
    /// </summary>
    public interface IStateStep
    {
        /// <summary>
        /// Invokes this instance.
        /// </summary>
        void Invoke();
        /// <summary>
        /// Gets or sets the invoked.
        /// </summary>
        /// <value>
        /// The invoked.
        /// </value>
        Action Invoked { get; set; }
    }
}
