using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using Microsoft.Practices.Prism.Commands;

namespace Monahrq.ViewModels
{
    /// <summary>
    /// View model Class to check and download
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    [Export]
    public class CheckAndDownloadUpdateViewModel : INotifyPropertyChanged
    {
        #region Declarations
        private string _status;
        private int _progress;
        private bool _readyToInstall;

        private WebClient _webClient;               // Our WebClient that will be doing the downloading for us
        private readonly Stopwatch _stopWatch = new Stopwatch();    // The stopwatch which we will be using to calculate the download speed

        private string _tempFilePath;

        /// <summary>
        /// Gets or sets the install command.
        /// </summary>
        /// <value>
        /// The install command.
        /// </value>
        public DelegateCommand InstallCommand { get; set; }

        #endregion

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckAndDownloadUpdateViewModel"/> class.
        /// </summary>
        public CheckAndDownloadUpdateViewModel()
        {
            _status = "Initializing download...";

            InstallCommand = new DelegateCommand(InstallUpdate, () => true);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string DownloadStatus
        {
            get { return _status; }
            set
            {
                _status = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
                NotifyPropertyChanged("DownloadStatus");
            }
        }

        /// <summary>
        /// The download URL
        /// </summary>
        public string DownloadUrl;
        /// <summary>
        /// The download location
        /// </summary>
        public string DownloadLocation;
        /// <summary>
        /// Gets or sets the download progress.
        /// </summary>
        /// <value>
        /// The download progress.
        /// </value>
        public int DownloadProgress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                NotifyPropertyChanged("DownloadProgress");
            }
        }

        /// <summary>
        /// Gets the show download progress.
        /// </summary>
        /// <value>
        /// The show download progress.
        /// </value>
        public Visibility ShowDownloadProgress
        {
            get { return DownloadProgress > 0 ? Visibility.Visible : Visibility.Hidden; }
        }

        public bool ReadyToInstall
        {
            get { return _readyToInstall; }
            set
            {
                _readyToInstall = value;
                NotifyPropertyChanged("ReadyToInstall");
            }
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Determines whether this instance can install.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can install; otherwise, <c>false</c>.
        /// </returns>
        private bool CanInstall()
        {
            return ReadyToInstall && !string.IsNullOrEmpty(_tempFilePath)  && File.Exists(_tempFilePath);
        }

        /// <summary>
        /// Installs the update.
        /// </summary>
        private void InstallUpdate()
        {
            if (string.IsNullOrEmpty(_tempFilePath) || !File.Exists(_tempFilePath)) return;

            var psi = new ProcessStartInfo
                {
                    FileName = Path.GetFileName(_tempFilePath),
                    WorkingDirectory = DownloadLocation
                };

            Process.Start(psi);

            // bypass further initialization
            Application.Current.Shutdown();

            // Force the program to quit without continuing the Prism bootstrapper sequence which loads NHibernate, etc.
            // This is usually not recommended, but in this case, we should not have any files open now,
            // and the program cannot continue without the database connection.
            //                Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
        }

        /// <summary>
        /// Cancels the installation.
        /// </summary>
        public void CancelInstallation()
        {
            if (_webClient != null)
                _webClient.CancelAsync();
        }

        ///// <summary>
        ///// Updates the message.
        ///// </summary>
        ///// <param name="message">The message.</param>
        //private void UpdateMessage(string message)
        //{
        //    if (string.IsNullOrEmpty(message))
        //    {
        //        return;
        //    }

        //    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
        //    {
        //        DownloadStatus = string.Concat(Environment.NewLine, message, "...");
        //    }));

        //    Application.Current.DoEvents();
        //}

        /// <summary>
        /// Downloads the file.
        /// </summary>
        public void DownloadFile()
        {
            using (_webClient = new WebClient())
            {
                _webClient.DownloadFileCompleted += Completed;
                _webClient.DownloadProgressChanged += ProgressChanged;

                // The variable that will be holding the url address (making sure it starts with http://)
                Uri url = DownloadUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(DownloadUrl) : new Uri("http://" + DownloadUrl);

                // Start the stopwatch which we will be using to calculate the download speed
                _stopWatch.Start();

                try
                {
                    _tempFilePath = Path.Combine(DownloadLocation, Guid.NewGuid() + ".exe");

                    // Start downloading the file
                    _webClient.DownloadFileAsync(url, _tempFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // The event that will fire whenever the progress of the WebClient is changed
        /// <summary>
        /// Progresses the changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DownloadProgressChangedEventArgs"/> instance containing the event data.</param>
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Calculate download speed and output it to labelSpeed.
            var downloadSpeed = string.Format("{0} kb/s", (e.BytesReceived / 1024d / _stopWatch.Elapsed.TotalSeconds).ToString("0.00"));

            // Update the progressbar percentage only when the value is not the same.
            DownloadProgress = e.ProgressPercentage;

            // Show the percentage on our label.
            //DownloadPercentage = e.ProgressPercentage.ToString() + "%";

            // Update the label with how much data have been downloaded so far and the total size of the file we are currently downloading
            var dataDownloaded = string.Format("{0} MB of {1} MB downloaded",
                                                 (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
                                                 (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));

            DownloadStatus = string.Format("{0} at {1}", dataDownloaded, downloadSpeed);

        }

        // The event that will trigger when the WebClient is completed
        /// <summary>
        /// Completeds the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="AsyncCompletedEventArgs"/> instance containing the event data.</param>
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            // Reset the stopwatch.
            _stopWatch.Reset();

            if (e.Cancelled)
            {
                DownloadStatus = "Download has been canceled.";
            }
            else
            {
                ReadyToInstall = true;
                DownloadStatus = "Download completed!";
            }
        }

        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
}
