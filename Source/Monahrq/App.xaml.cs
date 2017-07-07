using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Events;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Exceptions;
using Monahrq.Infrastructure.FileSystem;
using Monahrq.Sdk.Logging;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        #region Fields
        internal const string APP_RESTART_MUTEXT_NAME = "MonahrqRestart";
        internal const string SINGLE_APP_INSTANCE_MUTEX_NAME = "MonahrqSingleAppInstance";
        internal const string UPDATE_ACTIVATION_MUTEX_NAME = "MonahrqUpdateActivation";
        private static Mutex _appRestartMutex;
        private static Mutex _singleAppInstanceMutex;
        #endregion // Fields

        private static SessionLogger Logger { get { return new SessionLogger(new CallbackLogger()); }}

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (CheckInstanceRunning() == false)
            {
                try
                {
                    TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

                    MonahrqContext.IsInializing = true;
            
                    var bootstrapper = new MonahrqBootstrapper();
                    bootstrapper.Run();

                    MonahrqContext.IsInializing = false;

                    // Initialize application mutext to ensure only a single instance of the application is running at a time.
                    _singleAppInstanceMutex = new Mutex(true, SINGLE_APP_INSTANCE_MUTEX_NAME);
                }
                catch (ApplicationExitException)
                {
                    //Do nothing... just exit.
                }
                catch (RuntimeWrappedException runtimeExp)
                {
                    HandleError(runtimeExp);
                }
                catch (Exception appExp)
                {
                    if (appExp.InnerException is ApplicationExitException)
                        return;

                    HandleError(appExp);
                }
                finally
                {
                    try
                    {
                        if (_singleAppInstanceMutex != null)
                        {
                            _singleAppInstanceMutex.ReleaseMutex();
                            _singleAppInstanceMutex.Close();
                            _singleAppInstanceMutex = null;
                        }
                    }
                    catch
                    {
                        Logger.Write("Failed to release single app instance mutex");
                    }

                    try
                    {
                        if (_appRestartMutex != null)
                            _appRestartMutex.ReleaseMutex();
                    }
                    catch
                    {
                        Logger.Write("Failed to release app restart mutex");
                    }
                }
            }
            else
            {
                MessageBox.Show("Another instance of Monahrq is already running!", MonahrqContext.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // I think this is redundant. If we detect a fatal error during the bootstrap sequence, we should set 
            // OnExplicitShutdown and call Current.Shutdown (once) there.
            if (Current.ShutdownMode == System.Windows.ShutdownMode.OnExplicitShutdown)
            {
                Current.Shutdown();
            }
        }

        #region Helper Methods

        /// <summary>
        /// Checks to see if there's another instance of the application alrady running
        /// </summary>
        /// <returns></returns>
        private static bool CheckInstanceRunning()
        {
            try
            {
                try
                {
                    _appRestartMutex = Mutex.OpenExisting(APP_RESTART_MUTEXT_NAME);

                    //RestartApplication();
                    // SALogger.LogTrace("Application retart in progress, waiting for shutdown of other instance before proceeding");

                    // Wait for the application to close before continuing.
                    WaitHandle.WaitAll(new WaitHandle[] { _appRestartMutex });
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    // If we're here then the mutex was never created.
                }
                finally
                {
                    if (_appRestartMutex != null)
                    {
                        // SALogger.LogTrace("Application resstart resuming, other instance has shutdown.");
                        RestartApplication();
                        _appRestartMutex.Close();
                        _appRestartMutex = null;
                    }
                }

                try
                {
                    // Open the single instance mutex to see if already exists. If so 
                    // then another instance is already running.
                    _singleAppInstanceMutex = Mutex.OpenExisting(SINGLE_APP_INSTANCE_MUTEX_NAME);

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            catch (RuntimeWrappedException runtimeExp)
            {
                Logger.Write(runtimeExp.GetBaseException(), TraceEventType.Error /*"An unhandled runtime exception occurred while starting Monahrq"*/);
                return true;
            }
            catch (Exception exp)
            {
                Logger.Write(exp.GetBaseException(), TraceEventType.Error /*"An unhandled exception occurred while starting Monahrq"*/);
                return true;
            }
        }

        /// <summary>
        /// Restarts the application.
        /// </summary>
        private static void RestartApplication()
        {
            // SALogger.LogTrace("RestartApplication invoked");
            _appRestartMutex = new Mutex(true, APP_RESTART_MUTEXT_NAME);

            try
            {
                if (_singleAppInstanceMutex != null)
                {
                    _singleAppInstanceMutex.ReleaseMutex();
                    _singleAppInstanceMutex.Close();
                    _singleAppInstanceMutex = null;
                }
            }
            catch (Exception /*exp*/)
            {
                // SALogger.LogTrace(exp, "Failed to release single app instance mutex");
            }

            Current.Run();
        }

        /// <summary>
        /// Generic Handler for unexpected exceptions.
        /// </summary>
        /// <param name="exp">The exp.</param>
        private static void HandleError(Exception exp)
        {
            if (Logger != null)
            {
                Logger.Log(exp.GetBaseException().Message, Category.Exception, Priority.High);
                Logger.Log(exp.GetBaseException().StackTrace, Category.Exception, Priority.High);
            }
            // DO NOT USE current.MainWindow here as either part could be null

            if (exp is ApplicationExitException)
            {
                // DO nothing,,, just exit
            }
            if (exp is ApplicationException)
            {
                // If an application exception is encountered then it must have been published
                // by the Exception handling block.  Display message as is.
                MessageBox.Show(exp.GetBaseException().Message, MonahrqContext.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (exp is ReflectionTypeLoadException)
            {
                var exc = exp as ReflectionTypeLoadException;
                var msg = exc.LoaderExceptions.ToList().Aggregate(string.Empty, (current, loadException) => current + loadException.GetBaseException().Message + Environment.NewLine);
                string types = exc.Types != null && exc.Types.Any() ? string.Join(",", exc.Types.Select(t => t.FullName).ToList()) : null;

                throw new ApplicationException(string.Format("Loader Messages: {1}{0}{0}Types: {2}", Environment.NewLine, msg, types));
            }
            else
            {
                // All other exception types are unexpected so we need to publish them so that
                // they are first logged to the event logged, and then wrapped in a user friendly
                // generic error message.
                try
                {
                    MessageBox.Show(exp.GetBaseException().Message, MonahrqContext.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception newExp)
                {
                    MessageBox.Show(newExp.GetBaseException().Message, MonahrqContext.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            if (Current != null)
            {
                Current.Shutdown();
            }
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (Logger != null)
            {
                var exception = e.Exception.Flatten().GetBaseException();
                Logger.Log(exception.Message, Category.Exception, Priority.High);
                Logger.Log(exception.StackTrace, Category.Exception, Priority.High);
            }
        }
        #endregion // Helper Methods

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<ShutdownEvent>().Publish(new ShutdownEvent());
            base.OnExit(e);
        }
    }
}
