using System.ComponentModel.Composition;
using System.Reflection;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure;
using Monahrq.Sdk.Attributes.Wings;

namespace Monahrq.Sdk.Services.Contracts
{
    /// <summary>
    /// Base class for all <see cref="IDatasetWing{T}"/> implementations which provide name and description information
    /// from a <see cref="WingModuleAttribute"/> applied to the type parameter <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The entity type that this <see cref="DatasetWing{T}"/> describes</typeparam>
    /// <seealso cref="Monahrq.Sdk.Services.Contracts.IDatasetWing{T}" />
    public abstract class DatasetWing<T> : IDatasetWing<T>
    {
        /// <summary>
        /// Gets or sets the session log.
        /// </summary>
        /// <value>
        /// The session log.
        /// </value>
        [Import(LogNames.Session)]
        ILoggerFacade SessionLog { get; set; }

        /// <summary>
        /// Gets or sets the operations log.
        /// </summary>
        /// <value>
        /// The operations log.
        /// </value>
        [Import(LogNames.Operations)]
        ILoggerFacade OperationsLog { get; set; }

        /// <summary>
        /// The name of this <see cref="DatasetWing{T}"/> as defined by <see cref="WingModuleAttribute"/>
        /// </summary>
        public string Name
        {
            get
            {
                return typeof(T).GetCustomAttribute<WingModuleAttribute>().ContractName;
            }
        }

        /// <summary>
        /// The description of this <see cref="DatasetWing{T}"/> as defined by <see cref="WingModuleAttribute"/>
        /// </summary>
        public string Description
        {
            get
            {
                return typeof(T).GetCustomAttribute<WingModuleAttribute>().Description;
            }
        }
    }
}