using System;

namespace Monahrq.Sdk.StateMachine
{
    /// <summary>
    /// The state machine event class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Monahrq.Sdk.StateMachine.IStateStep" />
    public class StateEvent<T> : IStateStep where T : EventArgs
    {
        /// <summary>
        /// The begin
        /// </summary>
        private readonly Action _begin;

        /// <summary>
        /// The handler
        /// </summary>
        private readonly EventHandler<T> _handler;

        /// <summary>
        /// The unregister
        /// </summary>
        private readonly Action<EventHandler<T>> _unregister;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateEvent{T}"/> class.
        /// </summary>
        /// <param name="begin">The begin.</param>
        /// <param name="register">The register.</param>
        /// <param name="unregister">The unregister.</param>
        public StateEvent(Action begin, Action<EventHandler<T>> register, Action<EventHandler<T>> unregister)
        {
            _begin = begin;
            _unregister = unregister;
            _handler = Completed;
            register(_handler);
        }

        /// <summary>
        /// Completeds the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        public void Completed(object sender, T args)
        {
            Result = args;
            _unregister(_handler);
            Invoked();
        }

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public T Result { get; private set; }

        /// <summary>
        /// Invokes this instance.
        /// </summary>
        public void Invoke()
        {
            _begin();
        }

        /// <summary>
        /// Gets or sets the invoked.
        /// </summary>
        /// <value>
        /// The invoked.
        /// </value>
        public Action Invoked { get; set; }
    }

    public class StateEvent : IStateStep
    {
        /// <summary>
        /// The begin
        /// </summary>
        private readonly Action _begin;

        /// <summary>
        /// The handler
        /// </summary>
        private readonly EventHandler _handler;

        /// <summary>
        /// The unregister
        /// </summary>
        private readonly Action<EventHandler> _unregister;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateEvent"/> class.
        /// </summary>
        /// <param name="begin">The begin.</param>
        /// <param name="register">The register.</param>
        /// <param name="unregister">The unregister.</param>
        public StateEvent(Action begin, Action<EventHandler> register, Action<EventHandler> unregister)
        {
            _begin = begin;
            _unregister = unregister;
            _handler = Completed;
            register(_handler);
        }

        /// <summary>
        /// Completeds the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void Completed(object sender, EventArgs args)
        {
            Result = args;
            _unregister(_handler);
            Invoked();
        }

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public EventArgs Result { get; private set; }

        /// <summary>
        /// Invokes this instance.
        /// </summary>
        public void Invoke()
        {
            _begin();
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
