using System;

namespace Monahrq.Sdk.StateMachine
{
    /// <summary>
    /// The state machine action class.
    /// </summary>
    /// <example>
    /// var doSomethingStep = new StateStepAction();
    ///
    /// Exception exception = null;
    ///  string result = string.Empty;
    ///
    /// doSomethingStep.Execute = () => ServiceProvider.DoSomethingService(myParameter, (ex, res) =>
    ///                        {
    ///                            exception = ex;
    ///                            result = res;
    ///                            doSomethingStep.Invoked();
    ///                        });
    /// yield return doSomethingStep;
    /// </example>
    /// <seealso cref="Monahrq.Sdk.StateMachine.IStateStep" />
    public class StateAction : IStateStep
    {
        /// <summary>
        /// The immediate
        /// </summary>
        private readonly bool _immediate;

        /// <summary>
        /// Gets or sets the execute.
        /// </summary>
        /// <value>
        /// The execute.
        /// </value>
        public Action Execute { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateAction"/> class.
        /// </summary>
        public StateAction()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateAction"/> class.
        /// </summary>
        /// <param name="immediate">if set to <c>true</c> [immediate].</param>
        public StateAction(bool immediate)
        {
            _immediate = immediate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateAction"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public StateAction(Action action)
        {
            _immediate = false;
            Execute = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateAction"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="immediate">if set to <c>true</c> [immediate].</param>
        public StateAction(Action action, bool immediate)
        {
            _immediate = immediate;
            Execute = action;
        }

        /// <summary>
        /// Invokes this instance.
        /// </summary>
        public void Invoke()
        {
            Execute();
            if (_immediate)
            {
                Invoked();
            }
        }

        /// <summary>
        /// Gets or sets the invoked.
        /// </summary>
        /// <value>
        /// The invoked.
        /// </value>
        public Action Invoked { get; set; }
    }
}