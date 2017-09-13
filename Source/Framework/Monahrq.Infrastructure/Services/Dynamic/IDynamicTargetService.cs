using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using NHibernate;
using Monahrq.Infrastructure.Domain.Flutters;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Sdk.Extensibility;

namespace Monahrq.Infrastructure.Services.Dynamic
{
    /// <summary>
    /// The dynamic open source dataset target service interface.
    /// </summary>
    public interface IDynamicTargetService
    {
        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        Lazy<IMonahrqShell> Context { get; }

        /// <summary>
        /// Gets the wings folder.
        /// </summary>
        /// <value>
        /// The wings folder.
        /// </value>
        string WingsFolder { get; }

        /// <summary>
        /// Gets the flutters folder.
        /// </summary>
        /// <value>
        /// The flutters folder.
        /// </value>
        string FluttersFolder { get; }

        /// <summary>
        /// Gets all the installed open source wing targets.
        /// </summary>
        /// <returns></returns>
        bool GetInstalledOSTargets();

        /// <summary>
        /// Copies the files to directory.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        IEnumerable<DynamicTarget> CopyTargetFilesToDirectory(string filePath);

        /// <summary>
        /// Uninstalls the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="statusCallback">The status callback.</param>
        /// <param name="exceptionCallback">The exception callback.</param>
        /// <returns></returns>
        Task<bool> Uninstall(Target target, CancellationToken cancellationToken, Action<OpenSourceUnInstallResult> statusCallback, Action<OpenSourceUnInstallResult> exceptionCallback);

        /// <summary>
        /// Gets or sets the installed dynamic wing targets.
        /// </summary>
        /// <value>
        /// The installed dynamic wings.
        /// </value>
        ObservableCollection<DynamicTarget> InstalledDynamicTargets { get; set; }

        /// <summary>
        /// Imports the measures.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="session">The session.</param>
        void ImportMeasures(Target target, DynamicTarget dynamicTarget, ISession session = null);

        /// <summary>
        /// Imports the reports.
        /// </summary>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="session">The session.</param>
        void ImportReports(DynamicTarget dynamicTarget, ISession session = null);

        /// <summary>
        /// Reconciles the migrations of the dynamic wing target.
        /// </summary>
        /// <param name="dynamicTarget">The dynamic target.</param>
        void ReconcileMigrations(DynamicTarget dynamicTarget, IMonahrqShell monahrqShell = null);

        /// <summary>
        /// Called when [apply dynamic target dataset hints].
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="session">The session.</param>
        void OnApplyDynamicTargetDatasetHints(Target target, DynamicTarget dynamicTarget, ISession session = null);

        /// <summary>
        /// Installs the dynamic target.
        /// </summary>
        /// <param name="wingTarget">The wing target.</param>
        /// <param name="wing">The wing.</param>
        /// <param name="session">The session.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        OpenSourceInstallResult InstallDynamicTarget(DynamicTarget wingTarget, Wing wing, ISession session);

        /// <summary>
        /// Installs the dynamic target asynchronous.
        /// </summary>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="wing">The wing.</param>
        /// <param name="session">The session.</param>
        /// <param name="monahrqShell">The monahrq shell.</param>
        /// <param name="progressCallback">The progress callback.</param>
        /// <returns></returns>
        Task<bool> InstallDynamicTargetAsync(DynamicTarget dynamicTarget, Wing wing, ISession session, IMonahrqShell monahrqShell,
            Action<OpenSourceInstallResult> progressCallback);

        /// <summary>
        /// Uploads the and extract flutter files.
        /// </summary>
        /// <param name="uploadFilePath">The upload file path.</param>
        /// <param name="ctx">The CTX.</param>
        /// <param name="statusCallback">The status callback.</param>
        /// <param name="exceptionCallback">The exception callback.</param>
        /// <returns></returns>
        Task<bool> InstallFlutterFiles(string uploadFilePath, CancellationToken ctx,
            Action<OSFlutterInstallResult> statusCallback,
            Action<OSFlutterInstallResult> exceptionCallback);

        /// <summary>
        /// Gets the reports for flutter.
        /// </summary>
        /// <param name="flutterConfig">The flutter configuration.</param>
        /// <returns></returns>
        Task<List<Report>> GetReportsForFlutter(FlutterConfig flutterConfig);

        /// <summary>
        /// Saves the flutter.
        /// </summary>
        /// <param name="flutter">The flutter.</param>
        /// <returns></returns>
        Task<bool> SaveFlutter(Flutter flutter);

        /// <summary>
        /// Uninstalls the specified flutter.
        /// </summary>
        /// <param name="flutter">The flutter.</param>
        /// <param name="ctx">The CTX.</param>
        /// <param name="resultCallback">The result callback.</param>
        /// <param name="exceptionCallback">The exception callback.</param>
        /// <returns></returns>
        Task<bool> Uninstall(Flutter flutter, CancellationToken ctx, Action<OpenSourceUnInstallResult> resultCallback,
            Action<OpenSourceUnInstallResult> exceptionCallback);
    }
}
