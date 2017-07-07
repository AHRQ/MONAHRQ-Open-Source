using System;
namespace Monahrq.Default.DataProvider.Administration
{
    /// <summary>
    ///  Interface for data provider
    /// </summary>
    public interface IDataProviderAdministratorViewModel
    {
        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        /// <value>
        /// The connections.
        /// </value>
        System.ComponentModel.ICollectionView Connections { get; set; }
        /// <summary>
        /// Gets or sets the current connection model.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        NamedConnectionModel Current { get; set; }
    }
}
