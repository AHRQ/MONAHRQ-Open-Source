using System.Collections.Generic;
using Monahrq.Sdk.DataProvider;

namespace Monahrq.Sdk.Services
{
    /// <summary>
    /// The Monahrq data provider service interface/contract.
    /// </summary>
    public interface IDataProviderService
    {
        /// <summary>
        /// Gets the controllers.
        /// </summary>
        /// <value>
        /// The controllers.
        /// </value>
        IEnumerable<IDataProviderController> Controllers { get; }
        /// <summary>
        /// Gets the controller.
        /// </summary>
        /// <param name="controllerName">Name of the controller.</param>
        /// <returns></returns>
        IDataProviderController GetController(string controllerName);
        /// <summary>
        /// Gets the registered provider exports.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IDataProviderControllerExportAttribute> GetRegisteredProviderExports();
    }
}
