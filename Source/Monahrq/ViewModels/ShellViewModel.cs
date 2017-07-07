using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Regions;
using PropertyChanged;

namespace Monahrq.ViewModels
{
    /// <summary>
    /// View model class for Shell
    /// </summary>
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    [Export(typeof(ShellViewModel))]
    [ImplementPropertyChanged]
    public class ShellViewModel : IPartImportsSatisfiedNotification
    {
        #region Fields and Constants
        private const string APP_TITLE = "Monahrq - Input your data. Output your website.";
        private const double ERROR_OPEN_PANEL_HEIGHT = 40.0;
        private const double GENERIC_OPEN_PANEL_HEIGHT = 55.0;
        private const double PROGRESS_OPEN_PANEL_HEIGHT = 10.0;
        private const double PANEL_CLOSED_HEIGHT = 0;
        private readonly object _genericNotificationPanelLockObject = new object();

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        [Import]
        public IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        [Import]
        public IRegionManager RegionManager { get; set; }

        /// <summary>
        /// Gets or sets the help view model.
        /// </summary>
        /// <value>
        /// The help view model.
        /// </value>
        [Import]
        public HelpViewModel HelpViewModel { get; set; }

        //[Import]
        //ResumeNormalProcessingHandler ResumeNormalProcessingHandler { get; set; }

        //[Import]
        //PleaseStandByHandler PleaseStandByHandler { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the close error command.
        /// </summary>
        /// <value>
        /// The close error command.
        /// </value>
        public ICommand CloseErrorCommand { get; set; }

        /// <summary>
        /// Gets or sets the reset command.
        /// </summary>
        /// <value>
        /// The reset command.
        /// </value>
        public ICommand ResetCommand { get; set; }

        /// <summary>
        /// Gets or sets the close notification command.
        /// </summary>
        /// <value>
        /// The close notification command.
        /// </value>
        public ICommand CloseNotificationCommand { get; set; }

        /// <summary>
        /// Gets or sets the open session command.
        /// </summary>
        /// <value>
        /// The open session command.
        /// </value>
        public ICommand OpenSessionCommand { get; set; }

        /// <summary>
        /// Gets or sets the open help command.
        /// </summary>
        /// <value>
        /// The open help command.
        /// </value>
        public ICommand OpenHelpCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        public ShellViewModel()
        {
            NotificationPanel = PANEL_CLOSED_HEIGHT;
            GenericNotificationPanel = PANEL_CLOSED_HEIGHT;


            CloseErrorCommand = new DelegateCommand(() =>
            {
                NotificationPanel = PANEL_CLOSED_HEIGHT;
            });

            CloseNotificationCommand = new DelegateCommand(() =>
            {
                GenericNotificationPanel = PANEL_CLOSED_HEIGHT;
            });

            GenericMessageOpacity = 1;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the application title.
        /// </summary>
        /// <value>
        /// The application title.
        /// </value>
        public string ApplicationTitle
        {
            get
            {
                var title = APP_TITLE;
                if (MonahrqContext.IsAdministrator())
                    title += "  (Administrator)";

                return title;
            }
        }

        /// <summary>
        /// Gets or sets the generic notification panel.
        /// </summary>
        /// <value>
        /// The generic notification panel.
        /// </value>
        public double GenericNotificationPanel { get; set; }

        /// <summary>
        /// Gets or sets the generic notification message.
        /// </summary>
        /// <value>
        /// The generic notification message.
        /// </value>
        public string GenericNotificationMessage { get; set; }

        /// <summary>
        /// Gets or sets the type of the generic notification.
        /// </summary>
        /// <value>
        /// The type of the generic notification.
        /// </value>
        public ENotificationType GenericNotificationType { get; set; }

        /// <summary>
        /// Gets or sets the notification panel.
        /// </summary>
        /// <value>
        /// The notification panel.
        /// </value>
        public double NotificationPanel { get; set; }

        /// <summary>
        /// Gets or sets the progress panel.
        /// </summary>
        /// <value>
        /// The progress panel.
        /// </value>
        public double ProgressPanel { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the progress.
        /// </summary>
        /// <value>
        /// The progress.
        /// </value>
        public int Progress { get; set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version
        {
            get { return MonahrqContext.ApplicationVersion; }
        }

        /// <summary>
        /// Gets the popup region.
        /// </summary>
        /// <value>
        /// The popup region.
        /// </value>
        public IRegion PopupRegion
        {
            get
            {
                return RegionManager.Regions[RegionNames.Modal];
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [navigation disabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [navigation disabled]; otherwise, <c>false</c>.
        /// </value>
        public bool NavigationDisabled { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            _subsribeToEvents();

            OpenSessionCommand = new DelegateCommand(OnOpenSessionLog);
            OpenHelpCommand = new DelegateCommand(OpenContextualHelp);
        }

        /// <summary>
        /// Called when [open session log].
        /// </summary>
        private void OnOpenSessionLog()
        {
            var sessionLogPath = Path.Combine(MonahrqContext.MyDocumentsApplicationDirPath, "Logs", "Session.html");

            //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Session.Log");

            // Call the session log.
            using (var sessionLocProc = new Process())
            {
                sessionLocProc.StartInfo.WorkingDirectory = Path.GetDirectoryName(sessionLogPath);
                sessionLocProc.StartInfo.FileName = sessionLogPath;

                //grouperProc.StartInfo.UseShellExecute = false;
                sessionLocProc.StartInfo.CreateNoWindow = true;
                sessionLocProc.Start();
            }
        }

        /// <summary>
        /// Opens the contextual help.
        /// </summary>
        private void OpenContextualHelp()
        {
            EventAggregator.GetEvent<OpenContextualHelpContextEvent>().Publish(string.Empty);
        }

        /// <summary>
        /// Gets or sets the generic message opacity.
        /// </summary>
        /// <value>
        /// The generic message opacity.
        /// </value>
        public double GenericMessageOpacity { get; set; }

        /// <summary>
        /// Subsribes to events.
        /// </summary>
        private void _subsribeToEvents()
        {
            EventAggregator.GetEvent<DisableNavigationEvent>()
                           .Subscribe(dne => NavigationDisabled = dne.DisableUIElements);

            EventAggregator.GetEvent<ErrorNotificationEvent>().Subscribe(err =>
            {
                EventAggregator.GetEvent<ResumeNormalProcessingEvent>().Publish(Empty.Value);
                NotificationPanel = ERROR_OPEN_PANEL_HEIGHT;
                var errorToUse = err.GetBaseException();

                if (errorToUse is AggregateException)
                {
                    var aggEx = errorToUse as AggregateException;

                    errorToUse = aggEx.Flatten();
                }

                ErrorMessage = errorToUse.GetBaseException().Message;

                //Task.Run(() =>
                //{
                //    try
                //    {
                //        var player = new MediaPlayer();
                //        player.Open(new Uri(Path.Combine(MonahrqContext.BinFolderPath, "doh.mp3"), UriKind.Absolute));
                //        player.Volume = 0.075; // player.Volume / 3;
                //        player.Play();
                //        // player.Volume = 0.025;
                //    }
                //    catch {}
                //});

#if DEBUG
                ErrorMessage = ErrorMessage + " source: " + errorToUse.Source;
#endif
            });

            //EventAggregator.GetEvent<ProgressNotificationEvent>().Subscribe(p =>
            //{
            //    Progress = p;
            //    ProgressPanel = Progress == 100 ? PANEL_CLOSED_HEIGHT : PROGRESS_OPEN_PANEL_HEIGHT;
            //});


            EventAggregator.GetEvent<GenericNotificationEvent>().Subscribe(msg =>
                                                                {
                                                                    _outputNotificationMessage(msg, ENotificationType.Info);
                                                                });
            EventAggregator.GetEvent<GenericNotificationExEvent>().Subscribe(msg =>
                                                                {
                                                                    _outputNotificationMessage(msg.Message, msg.NotificationType);
                                                                });

            //EventAggregator.GetEvent<ResumeNormalProcessingEvent>().Subscribe(payload => ResumeNormalProcessingHandler.Handle(payload));
        }
        /// <summary>
        /// Outputs the notification message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="notificationType">Type of the notification.</param>
        private void _outputNotificationMessage(String message, ENotificationType notificationType = ENotificationType.Info)
        {
            lock (_genericNotificationPanelLockObject)
            {
                GenericNotificationPanel = GENERIC_OPEN_PANEL_HEIGHT;
                GenericNotificationMessage = message;
                GenericNotificationType = notificationType;

                //TODO : Use Xaml to change the OPACITY of the control rather than doing it programatically
                if ((int)GenericMessageOpacity <= 0)
                {
                    // Tasking prevents the UI thread from being locked up
                    var task = Task.Factory.StartNew(() =>
                            {
                                GenericMessageOpacity = 0;
                                for (int i = 0; i < 20; i++)
                                {
                                    GenericMessageOpacity += 0.05;
                                    Application.DoEvents();
                                    Thread.Sleep(50);
                                }

                            });

                }

                if (GenericNotificationPanel != 0)
                {
                    // Tasking prevents the UI thread from being locked up
                    var task2 = Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(6000);
                        for (int i = 0; i < 20; i++)
                        {
                            GenericMessageOpacity -= 0.05;
                            Application.DoEvents();
                            Thread.Sleep(25);
                        }
                        GenericNotificationPanel = 0;
                        Application.DoEvents();
                    });
                }
            }
        }


        #endregion

    }
}
