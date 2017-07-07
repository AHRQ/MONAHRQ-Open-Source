using System;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.DataProvider.Builder;

namespace Monahrq.Default.DataProvider.Administration.File
{
    /// <summary>
    /// Interface for file related operations
    /// </summary>
    public interface IFileDatasourceViewModel
    {
        /// <summary>
        /// Gets or sets the current file.
        /// </summary>
        /// <value>
        /// The current file.
        /// </value>
        string CurrentFile { get; set; }
        /// <summary>
        /// Gets the current data provider.
        /// </summary>
        /// <value>
        /// The current data provider.
        /// </value>
        OleDbDataProviderController CurrentDataProvider { get; }
        /// <summary>
        /// Gets the connection element.
        /// </summary>
        /// <value>
        /// The connection element.
        /// </value>
        NamedConnectionElement ConnectionElement { get; }
        /// <summary>
        /// Gets or sets a value indicating whether the implemented instance has header.
        /// </summary>
        /// <value>
        /// <c>true</c> if this implemented instance has header; otherwise, <c>false</c>.
        /// </value>
        bool HasHeader { get; set; }

        //DelegateCommand CancelCommand { get; }
        //DelegateCommand SaveConnectionCommand { get; }
        //DelegateCommand TestConnectionCommand { get; }

        /// <summary>
        /// Gets the select file command.
        /// </summary>
        /// <value>
        /// The select file command.
        /// </value>
        DelegateCommand SelectFileCommand { get; }
        /// <summary>
        /// Gets the current view.
        /// </summary>
        /// <value>
        /// The current view.
        /// </value>
        IFileConnectionStringView CurrentView { get; }
    }
}
