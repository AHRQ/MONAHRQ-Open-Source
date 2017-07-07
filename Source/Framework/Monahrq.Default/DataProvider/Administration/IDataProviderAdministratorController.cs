using System;
namespace Monahrq.Default.DataProvider.Administration
{
    /// <summary>
    /// Interface for data provider controller containing operations to be defined by the implemented class
    /// </summary>
    public interface IDataProviderAdministratorController
    {
        /// <summary>
        /// Gets the delete command.
        /// </summary>
        /// <value>
        /// The delete command.
        /// </value>
        Microsoft.Practices.Prism.Commands.DelegateCommand DeleteCommand { get; }
        /// <summary>
        /// Gets the modify command.
        /// </summary>
        /// <value>
        /// The modify command.
        /// </value>
        Microsoft.Practices.Prism.Commands.DelegateCommand ModifyCommand { get; }
        /// <summary>
        /// Gets the new command.
        /// </summary>
        /// <value>
        /// The new command.
        /// </value>
        Microsoft.Practices.Prism.Commands.DelegateCommand NewCommand { get; }
        /// <summary>
        /// Gets the select command.
        /// </summary>
        /// <value>
        /// The select command.
        /// </value>
        Microsoft.Practices.Prism.Commands.DelegateCommand SelectCommand { get; }
        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        IDataProviderAdministratorViewModel ViewModel { get; }
    }
}
