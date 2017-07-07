using System;
using System.Data;
namespace Monahrq.Infrastructure.Configuration
{
    /// <summary>
    /// The custom named connection element interface.
    /// </summary>
    public interface INamedConnectionElement
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        string ConnectionString { get; set; }
        /// <summary>
        /// Gets or sets the type of the controller.
        /// </summary>
        /// <value>
        /// The type of the controller.
        /// </value>
        string ControllerType { get; set; }
        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="target">The target.</param>
        void CopyTo(NamedConnectionElement target);
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; set; }
        /// <summary>
        /// Gets or sets the select from.
        /// </summary>
        /// <value>
        /// The select from.
        /// </value>
        string SelectFrom { get; set; }
        /// <summary>
        /// Gets the schema ini settings.
        /// </summary>
        /// <value>
        /// The schema ini settings.
        /// </value>
        SchemaIniElementCollection SchemaIniSettings { get; }
        /// <summary>
        /// Creates the provider.
        /// </summary>
        /// <returns></returns>
        object CreateProvider();
    }
}
