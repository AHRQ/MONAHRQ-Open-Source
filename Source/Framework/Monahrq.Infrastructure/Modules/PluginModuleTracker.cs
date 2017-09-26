using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure;

namespace Monahrq.Sdk.Modules
{
    /// <summary>
    /// The plugin module tracker
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Modules.IPluginModuleTracker" />
    [Export(typeof(IPluginModuleTracker))]
    public class PluginModuleTracker : IPluginModuleTracker
    {
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Operations, typeof(ILogWriter))]
        private ILogWriter Logger{ get; set; }

        /// <summary>
        /// Records the plugin module is loading.
        /// </summary>
        /// <param name="moduleName">Name of the plugin module.</param>
        /// <param name="bytesReceived">The number of bytes downloaded.</param>
        /// <param name="totalBytesToReceive">The total bytes to receive.</param>
        public void RecordModuleDownloading(string moduleName, long bytesReceived, long totalBytesToReceive)
        {
            Logger.Debug("Downloading {0}: {1:N} of {2:N} bytes recieved", moduleName, bytesReceived, totalBytesToReceive); 
        }

        /// <summary>
        /// Records the plugin module has been loaded.
        /// </summary>
        /// <param name="moduleName"></param>
        public void RecordModuleLoaded(string moduleName)
        {
            Logger.Debug("Module loaded: {0}", moduleName);
        }

        /// <summary>
        /// Records the plugin module has been constructed.
        /// </summary>
        /// <param name="moduleName"></param>
        public void RecordModuleConstructed(string moduleName)
        {
            Logger.Debug("Module Created: {0}", moduleName);
        }

        /// <summary>
        /// Records the plugin module has been initialized.
        /// </summary>
        /// <param name="moduleName"></param>
        public void RecordModuleInitialized(string moduleName)
        {
            Logger.Debug("Module Initialized: {0}", moduleName);
        }
    }
}
