using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Extensions;


namespace Monahrq.ViewModels
{
    /// <summary>
    /// Splash view model class.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    [Export]
    public class SplashViewModel : INotifyPropertyChanged
    {
        #region Declarations
        private string _status;
        private string _registeredUserName;
        private string _registeredUserCompany;
        #endregion

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="SplashViewModel"/> class.
        /// </summary>
        /// <param name="eventAggregator">The event aggregator.</param>
        [ImportingConstructor]
        public SplashViewModel(IEventAggregator eventAggregator)
        {
            _status = "Initializing application...";

         
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the name of the registered user.
        /// </summary>
        /// <value>
        /// The name of the registered user.
        /// </value>
        public string RegisteredUserName
        {
            get { return _registeredUserName; }
            set
            {
                _registeredUserName = value;
                NotifyPropertyChanged("RegisteredUserName");
            }
        }

        /// <summary>
        /// Gets or sets the registered user company.
        /// </summary>
        /// <value>
        /// The registered user company.
        /// </value>
        public string RegisteredUserCompany
        {
            get { return _registeredUserCompany; }
            set
            {
                _registeredUserCompany = value;
                NotifyPropertyChanged("RegisteredUserCompany");
            }
        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status
        {
            get { return _status; }
            set
            {
                _status = string.IsNullOrWhiteSpace(value) ? string.Empty : string.Format("{0}...", value);
                NotifyPropertyChanged("Status");
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version
        {
            get { return string.Format("Version {0} {1}", MonahrqContext.ApplicationName.Replace("Monahrq ", null), DateTime.Now.Year); }
        }
        #endregion
 

        #region Private Methods
        /// <summary>
        /// Updates the message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void UpdateMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            Dispatcher.CurrentDispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                Status = string.Concat(Environment.NewLine, message, "...");
            }));
            Application.Current.DoEvents();
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
