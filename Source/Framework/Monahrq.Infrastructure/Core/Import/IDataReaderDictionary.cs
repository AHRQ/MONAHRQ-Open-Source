using System;
using System.Collections.Generic;
using Monahrq.Infrastructure.Entities.Events;

namespace Monahrq.Infrastructure.Entities.Core.Import
{
    /// <summary>
    /// The <see cref=" System.Data.IDataReader"/> disctionary interface.
    /// </summary>
    public interface IDataReaderDictionary
    {
        /// <summary>
        /// Gets the data folder.
        /// </summary>
        /// <value>
        /// The data folder.
        /// </value>
        string DataFolder { get; }
        /// <summary>
        /// Gets the <see cref="System.Data.IDataReader"/> with the specified reader name.
        /// </summary>
        /// <value>
        /// The <see cref="System.Data.IDataReader"/>.
        /// </value>
        /// <param name="readerName">Name of the reader.</param>
        /// <returns></returns>
        System.Data.IDataReader this[string readerName] { get; }
        /// <summary>
        /// Gets the version attribute.
        /// </summary>
        /// <value>
        /// The version attribute.
        /// </value>
        VersionedComponentExportAttribute VersionAttribute { get; }
        /// <summary>
        /// Occurs when [requesting was executed].
        /// </summary>
        event EventHandler<ExtendedEventArgs<bool>> RequestingWasExecuted;
        /// <summary>
        /// Gets the programmabilities.
        /// </summary>
        /// <value>
        /// The programmabilities.
        /// </value>
        IEnumerable<IProgrammability> Programmabilities { get; }
    }
}
