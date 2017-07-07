using System;
using System.Windows;
using System.Windows.Input;

namespace WpfRichText.Ex.Commands
{
	/// <summary>
	/// This class facilitates associating a key binding in XAML markup to a command
	/// defined in a View Model by exposing a Command dependency property.
	/// The class derives from Freezable to work around a limitation in WPF when data-binding from XAML.
	/// </summary>
	/// <seealso cref="System.Windows.Freezable" />
	/// <seealso cref="System.Windows.Input.ICommand" />
	public class CommandReference : Freezable, ICommand
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="CommandReference"/> class.
		/// </summary>
		public CommandReference()
        {
            // Blank
        }

		/// <summary>
		/// The command property
		/// </summary>
		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandReference), 
            new PropertyMetadata(new PropertyChangedCallback(OnCommandChanged)));

		/// <summary>
		/// Gets or sets the command.
		/// </summary>
		/// <value>
		/// The command.
		/// </value>
		public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

		#region ICommand Members

		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		/// <returns>
		/// true if this command can be executed; otherwise, false.
		/// </returns>
		public bool CanExecute(object parameter)
        {
            if (Command != null)
                return Command.CanExecute(parameter);
            return false;
        }

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		public void Execute(object parameter)
        {
            Command.Execute(parameter);
        }

		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Called when [command changed].
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandReference commandReference = d as CommandReference;
            ICommand oldCommand = e.OldValue as ICommand;
            ICommand newCommand = e.NewValue as ICommand;

            if (oldCommand != null)
            {
                oldCommand.CanExecuteChanged -= commandReference.CanExecuteChanged;
            }
            if (newCommand != null)
            {
                newCommand.CanExecuteChanged += commandReference.CanExecuteChanged;
            }
        }

		#endregion

		#region Freezable

		/// <summary>
		/// When implemented in a derived class, creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class.
		/// </summary>
		/// <returns>
		/// The new instance.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
