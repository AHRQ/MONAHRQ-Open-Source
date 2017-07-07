namespace Monahrq.Sdk.Modules
{
    /// <summary>
    /// The module installer interface.
    /// </summary>
    public interface IModuleInstaller
    {
        /// <summary>
        /// Installs this instance.
        /// </summary>
        /// <returns></returns>
        bool Install();
        /// <summary>
        /// Installs the database.
        /// </summary>
        /// <returns></returns>
        bool InstallDb();
        /// <summary>
        /// Updates this instance.
        /// </summary>
        /// <returns></returns>
        bool Update();
        /// <summary>
        /// Updates the database.
        /// </summary>
        /// <returns></returns>
        bool UpdateDb();
    }
}