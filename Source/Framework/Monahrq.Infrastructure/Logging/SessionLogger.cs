using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Sdk.Logging
{
    /// <summary>
    /// Implementation of <see cref="ILogWriter" /> that logs into a <see cref="TextWriter" />.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Logging.LoggerBase" />
    [Export(LogNames.Session, typeof(ILogWriter))]
    [Export(LogNames.Session, typeof(ILoggerFacade))]
    [Export(LogNames.Operations, typeof(ILogWriter))] // operations logging was previously handled by CallbackLogger
    [Export(LogNames.Operations, typeof(ILoggerFacade))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SessionLogger : LoggerBase
    {
        /// <summary>
        /// Gets the name of the log file.
        /// </summary>
        /// <value>
        /// The name of the log file.
        /// </value>
        public static string LogFileName
        {
            get { return Path.Combine(MonahrqContext.MyDocumentsApplicationDirPath, "Logs", "session.html"); }
        }

        /// <summary>
        /// The is configured
        /// </summary>
        public static bool IsConfigured = false;

        /// <summary>
        /// The deafault maximum file size
        /// </summary>
        public const int DEAFAULT_MAXIMUM_FILE_SIZE = 10 * 1024 * 1024;

        /// <summary>
        /// Gets or sets the data provider.
        /// </summary>
        /// <value>
        /// The data provider.
        /// </value>
        [Import]
        IDomainSessionFactoryProvider DataProvider { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionLogger"/> class.
        /// </summary>
        /// <param name="opsLogger">The ops logger.</param>
        public SessionLogger(ILoggerFacade opsLogger)
            : this()
        { }

        [ImportingConstructor]
        public SessionLogger()
        {
            var logsDirectoryPath = Path.Combine(MonahrqContext.MyDocumentsApplicationDirPath, "Logs");
            if (!Directory.Exists(logsDirectoryPath))
                Directory.CreateDirectory(logsDirectoryPath);

            // This class uses 3 lines of code to create and initialize an instance of log4net. It's recommended to create a wrapper class
            // for log4net to hide those details. There are 2 wrapper classes available near the bottom of this link that we could use if
            // we want: http://social.msdn.microsoft.com/Forums/vstudio/en-US/7db558e7-ccc0-46f4-b74e-35bd633e014c/use-log4net-in-a-dll-project
            // Read the main app config file to get the log4net settings...
            FileInfo configFileInfo = new FileInfo("monahrq.exe.config");
            //var path = Path.Combine(logsDirectoryPath, "session.log");
            log4net.GlobalContext.Properties["LogFilePath"] = logsDirectoryPath;
            //log4net.Config.XmlConfigurator.ConfigureAndWatch(configFileInfo);

            if (IsOldFile(LogFileName))
            {
                File.Delete(LogFileName);
                IsConfigured = false;
            }

            if (!IsConfigured)
            {
                var appender = new CustomRollingAppender()
                {
                    File = LogFileName,
                    DateTimeStrategy = new CustomRollingAppender.LocalDateTime(),
                    DatePattern = "yyyy-MM-dd",
                    PreserveLogFileNameExtension = true,
                    Encoding = Encoding.Default,
                    AppendToFile = true,
                    LockingModel = new FileAppender.InterProcessLock(),
                    RollingStyle = CustomRollingAppender.RollingMode.Size,
                    MaxFileSize = 2,
                    MaximumFileSize = "10MB",
                    Layout = new CustomPatternLayout("%custom"),
                    SecurityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this)
                };

                BasicConfigurator.Configure(appender);
                LogSystemInforamtion();
            }
            IsConfigured = true;

        }

        /// <summary>
        /// Determines whether [is old file] [the specified file name].
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        ///   <c>true</c> if [is old file] [the specified file name]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOldFile(string fileName)
        {
            if (!File.Exists(fileName)) return false;

            var fileInfo = new FileInfo(LogFileName);

            return DateTime.Today.Subtract(fileInfo.CreationTime).Days >= 2 && fileInfo.Length > DEAFAULT_MAXIMUM_FILE_SIZE;

        }

        /// <summary>
        /// Logs the system inforamtion.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        private void LogSystemInforamtion()
        {
            var driveInfo = new DriveInfo(AppDomain.CurrentDomain.BaseDirectory);
            var div = 1024 * 1024 * 1000;
            var info = new StringBuilder();
            info.Append($@"<hr/><pre>
<h1>MONAHRQ Session: {DateTime.Now.ToLongDateString()}, {DateTime.Now.ToShortTimeString()}</h1>
 &mdash; Operating System: {Environment.OSVersion}
 &mdash; Available Hard Disk Space/Total Hard Disk: {(int)(driveInfo.AvailableFreeSpace / div)} GB/{(int)
                (driveInfo.TotalSize / div)} GB</p>"
            );
            this.OnLog(info.ToString(), Category.Info, Priority.High);
        }

        // Create a log4net instance for this class.
        // System.Reflection.MethodBase.GetCurrentMethod().DeclaringType is equivalent to typeof(LoggingExample) but is more portable
        // i.e. you can copy the code directly into another class without needing to edit the code.
        /// <summary>
        /// The log
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Write a new log entry with the specified category and priority.
        /// </summary>
        /// <param name="message">Message body to log.</param>
        /// <param name="category">Category of the entry.</param>
        /// <param name="priority">The priority of the entry.</param>
        protected override void OnLog(string message, Category category, Priority priority)
        {
            /*try
            {
                OperationLog.Log(message, category, priority);
            }
            finally
            {*/
            // log it to the log4net level depending on the Category
            switch (category)
            {
                case Category.Exception:
                    log.Error(message);
                    break;

                case Category.Warn:
                    log.Warn(message);
                    break;

                case Category.Debug:
                    if (log.IsDebugEnabled) log.Debug(message);
                    break;

                case Category.Info:
                    if (log.IsInfoEnabled) log.Info(message);
                    break;
            }
            //}
        }

    }
}
