using System;

namespace Monahrq.Infrastructure.Configuration
{
    /// <summary>
    /// The monahrq settings configuration interface.
    /// </summary>
    public interface IMonahrqSettings
    {
        /// <summary>
        /// Gets or sets the update script to run at startup.
        /// </summary>
        /// <value>
        /// The update script to run at startup.
        /// </value>
        string UpdateScriptToRunAtStartup { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is first run.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is first run; otherwise, <c>false</c>.
        /// </value>
        bool IsFirstRun { get; set; }
        /// <summary>
        /// Gets or sets the size of the batch.
        /// </summary>
        /// <value>
        /// The size of the batch.
        /// </value>
        int BatchSize { get; set; }

        /// <summary>
        /// Gets or sets the last folder.
        /// </summary>
        /// <value>
        /// The last folder.
        /// </value>
        string LastFolder { get; set; }
        /// <summary>
        /// Gets or sets the long timeout.
        /// </summary>
        /// <value>
        /// The long timeout.
        /// </value>
        TimeSpan LongTimeout { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [rebuild database].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [rebuild database]; otherwise, <c>false</c>.
        /// </value>
        bool RebuildDatabase { get; set; }
        /// <summary>
        /// Gets or sets the short timeout.
        /// </summary>
        /// <value>
        /// The short timeout.
        /// </value>
        TimeSpan ShortTimeout { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [debug SQL].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [debug SQL]; otherwise, <c>false</c>.
        /// </value>
        bool DebugSql { get; set; }
        /// <summary>
        /// Gets the named connections.
        /// </summary>
        /// <value>
        /// The named connections.
        /// </value>
        NamedConnectionElementCollection NamedConnections { get; }
        /// <summary>
        /// Gets the themes.
        /// </summary>
        /// <value>
        /// The themes.
        /// </value>
        MonahrqThemeElementCollection Themes { get; }
        /// <summary>
        /// Gets the banners.
        /// </summary>
        /// <value>
        /// The themes.
        /// </value>
        MonahrqBannerElementCollection Banners { get; }
        /// <summary>
        /// Gets the entity connection settings.
        /// </summary>
        /// <value>
        /// The entity connection settings.
        /// </value>
        ConnectionStringSettingsElement EntityConnectionSettings { get; }
        /// <summary>
        /// Gets the win qi connection settings.
        /// </summary>
        /// <value>
        /// The win qi connection settings.
        /// </value>
        ConnectionStringSettingsElement WinQiConnectionSettings { get; }

        //ConnectionStringsSection ConnectionStrings { get; }

        /// <summary>
        /// Gets or sets the update check URL.
        /// </summary>
        /// <value>
        /// The update check URL.
        /// </value>
        string UpdateCheckUrl { get; set; }
        /// <summary>
        /// Gets or sets the monahrq version.
        /// </summary>
        /// <value>
        /// The monahrq version.
        /// </value>
        string MonahrqVersion { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [use API for physicians].
        /// </summary>
        /// <value>
        /// <c>true</c> if [use API for physicians]; otherwise, <c>false</c>.
        /// </value>
        bool UseApiForPhysicians { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [data access components installed].
        /// </summary>
        /// <value>
        /// <c>true</c> if [data access components installed]; otherwise, <c>false</c>.
        /// </value>
        bool DataAccessComponentsInstalled { get; set; }
        /// <summary>
        /// Gets or sets the HospitalRegion structure.
        /// </summary>
        HospitalRegionElement HospitalRegion { get; set; }
    }
}
