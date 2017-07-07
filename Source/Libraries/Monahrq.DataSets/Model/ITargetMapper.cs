using System.Collections.Generic;
using System.Data;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The dataset target mapper interface.
    /// </summary>
    public interface ITargetMapper
    {
        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        dynamic Target { get; }
        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified element.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        object this[Element element] { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        object this[string name] { get; set; }
        /// <summary>
        /// Gets the errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        IEnumerable<ElementMappingValueException> Errors { get; }
        /// <summary>
        /// Resets this instance.
        /// </summary>
        void Reset();
        /// <summary>
        /// Creates the bulk importer.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="target">The target.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns></returns>
        dynamic CreateBulkImporter(IDbConnection connection, Target target = null, int? batchSize = null); 
    }
}
