namespace Monahrq.Sdk.Modules
{
    /// <summary>
    /// Provides ability for plugin modules to inform the shellview of their state.
    /// </summary>
    public interface IPluginModuleTracker
    {
        /// <summary>
        /// Records the plugin module is loading.
        /// </summary>
        /// <param name="moduleName">Name of the plugin module.</param>
        /// <param name="bytesReceived">The number of bytes downloaded.</param>
        /// <param name="totalBytesToReceive">The total bytes to receive.</param>
        void RecordModuleDownloading(string moduleName, long bytesReceived, long totalBytesToReceive);

        /// <summary>
        /// Records the plugin module has been loaded.
        /// </summary>
        /// <param name="moduleName"/>
        void RecordModuleLoaded(string moduleName);

        /// <summary>
        /// Records the plugin module has been constructed.
        /// </summary>
        /// <param name="moduleName"/>
        void RecordModuleConstructed(string moduleName);

        /// <summary>
        /// Records the plugin module has been initialized.
        /// </summary>
        /// <param name="moduleName"/>
        void RecordModuleInitialized(string moduleName);
    }
}
