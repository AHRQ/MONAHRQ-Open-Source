using System;

namespace Microsoft.Practices.Prism.Commands
{
    /// <summary>
    ///     Delegate command - does it all
    /// </summary>
    public class ActionCommand<T> : IActionCommand<T>
    {
        private Action<T> _execute = obj => { };
        private readonly Func<T, bool> _canExecute = obj => true;

        public bool Overridden { get; set; }

        public ActionCommand()
        {

        }

        /// <summary>
        ///     Override the action
        /// </summary>
        /// <param name="action"></param>
        public void OverrideAction(Action<T> action)
        {
            _execute = action;
            Overridden = true;
        }

        /// <summary>
        ///     Constructor with action to perform
        /// </summary>
        /// <param name="execute">The action to execute</param>
        public ActionCommand(Action<T> execute)
        {
            _execute = execute;
        }

        /// <summary>
        ///     Constructor with action and condition
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        public ActionCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null. </param>
        public bool CanExecute(object parameter)
        {
            return _canExecute((T)parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null. </param>
        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _execute((T)parameter);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
