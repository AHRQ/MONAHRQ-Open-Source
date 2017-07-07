using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.SysTray.trayNotification;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Monahrq.SysTray
{
    /// <summary>
    /// Sys tray module entry point main class
    /// </summary>
    static class Program
    {
        #region Fields
        /// <summary>
        /// The current ip target identifier
        /// </summary>
        private static int _currentIpTargetId;
        /// <summary>
        /// The monahrq configured connection string
        /// </summary>
        private static string _monahrqConfiguredConnectionString;
        /// <summary>
        /// The log file path
        /// </summary>
        private static string _logFilePath;
        #endregion // Fields

        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        [STAThread]
        static void Main(string[] args)
        {
            Stopwatch grouperStopwatch = new Stopwatch();
            try
            {
                grouperStopwatch.Start();

                bool instanceCountOne;
                using (var mtex = new Mutex(true, "Monahrq.SysTrayApp", out instanceCountOne))
                {
                    if (instanceCountOne)
                    {
                        if (!args.Any())
                            return;

                        if (args.Length == 1 && args[0].Contains("|"))
                        {
                            var argsArray = args[0].Split('|');
                            _currentIpTargetId = int.Parse(argsArray[0]);

                            if (argsArray[1].StartsWith("'") && argsArray[1].EndsWith("'"))
                                _monahrqConfiguredConnectionString = argsArray[1].Replace("'", null);
                            else
                                _monahrqConfiguredConnectionString = argsArray[1];

                            if (argsArray[2].StartsWith("'") && argsArray[2].EndsWith("'"))
                                _logFilePath = argsArray[2].Replace("'", null);
                            else
                                _logFilePath = argsArray[2];
                        }
                        else
                        {
                            _currentIpTargetId = int.Parse(args[0]);
                            if (args[1].StartsWith("'") && args[1].EndsWith("'"))
                                _monahrqConfiguredConnectionString = args[1].Replace("'", null);
                            else
                                _monahrqConfiguredConnectionString = args[1];

                            if (args[2].StartsWith("'") && args[2].EndsWith("'"))
                                _logFilePath = args[2].Replace("'", null);
                            else
                                _logFilePath = args[2];

                            //_monahrqConfiguredConnectionString = args[1];
                            //_logFilePath = args[2];

                            
                        }
                        Log("Monahrq.SysTray IP Dataset Id: " + _currentIpTargetId);
                        Log("Monahrq.SysTray.Grouper Log File Path: " + _logFilePath);
                        

                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.ThreadException += Application_ThreadException;
                        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                        var trayContext = new SysTrayApplicationContext(_monahrqConfiguredConnectionString, _currentIpTargetId, _logFilePath);
                        Application.Run(trayContext);

                        mtex.ReleaseMutex();
                    }
                    else
                    {
                        MessageBox.Show(@"A Monahrq MS-DRG Grouper process is still running for the current dataset import. Please wait until the process is complete to either run the grouper via the dataset screen or re-importing the dataset.", MonahrqContext.ApplicationName, MessageBoxButtons.OK);

                        var dbHelper = new SysTrayDbHelper(_monahrqConfiguredConnectionString);
                        var query = string.Format("update dbo.[wings_datasets] set [DRGMDCMappingStatusMessage]='{0}' where [Id]={1}", DrgMdcMappingStatusEnum.Pending, _currentIpTargetId);
                        dbHelper.ExecuteNonQuery(query);
                    }
                }
                grouperStopwatch.Stop();

            }
            catch (Exception exc)
            {
                var excToUse = exc.GetBaseException();

                if (exc is AggregateException)
                {
                    excToUse = ((AggregateException)exc).Flatten().GetBaseException();
                }

                var message = excToUse.Message + Environment.NewLine + excToUse.StackTrace;

                Log(message);
            }
            finally
            {
                
                Log(string.Format("Final Grouper Execution Time: {0}:{1}:{2}", grouperStopwatch.Elapsed.TotalHours, grouperStopwatch.Elapsed.TotalMinutes, grouperStopwatch.Elapsed.TotalSeconds));
//#if DEBUG
//                var appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "grouper", "Msgmce");

//                var inputFilesToDelete = Directory.EnumerateFiles(appPath, "*.inp").ToList();
//                var uploadFilesToDelete = Directory.EnumerateFiles(appPath, "*.upl").ToList();

//                inputFilesToDelete.ForEach(CleanupFiles);
//                uploadFilesToDelete.ForEach(CleanupFiles);
////#endif
            }
        }

        /// <summary>
        /// Cleanups the files.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        private static void CleanupFiles(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch { }
        }

        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var excToUse = (e.ExceptionObject as Exception).GetBaseException();
            var message = excToUse.Message + Environment.NewLine + excToUse.StackTrace;
            Log(message);
        }

        /// <summary>
        /// Handles the ThreadException event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ThreadExceptionEventArgs"/> instance containing the event data.</param>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            var excToUse = e.Exception.GetBaseException();
            var message = excToUse.Message + Environment.NewLine + excToUse.StackTrace;
            Log(message);
        }

        /// <summary>
        /// The file lock
        /// </summary>
        private static readonly object[] _fileLock = {};
        //private static readonly ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void Log(string message)
        {
           // _readWriteLock.EnterWriteLock();

            //try
            //{
                var filePath = string.IsNullOrEmpty(_logFilePath)
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SysTrayApplicationContext.SYSTRAY_LOGFILE_NAME)
                    : Path.Combine(_logFilePath, SysTrayApplicationContext.SYSTRAY_LOGFILE_NAME);

                lock (_fileLock)
                {
                    using (var file = new StreamWriter(filePath, true))
                    {
                        file.WriteLine(message);

                        file.Close();
                    }
                }
            //}
            //finally
            //{
            //   // _readWriteLock.ExitReadLock();
            //}
        }
    }
}
