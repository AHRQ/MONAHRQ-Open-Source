using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Sdk.Services.Import;

namespace Monahrq.Sdk.Common.FileImport
{
    /// <summary>
    /// The file Import View interface
    /// </summary>
    public interface IFileImportView
    {
        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        FileImportViewModel Model { get; }
    }
}
