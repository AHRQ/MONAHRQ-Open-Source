using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Events;
using Monahrq.Sdk.Modules;
using Monahrq.Sdk.Regions;
using Monahrq.Infrastructure.Integration;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Types;

namespace Monahrq.ViewModels
{
    /// <summary>
    /// View model for left navigation pane
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    [Export]
    public class LeftNavigationViewModel : INotifyPropertyChanged
    {
        #region Fields and Constants

        /// <summary>
        /// The navigation items
        /// </summary>
        private ObservableCollection<MenuItem> _navigationItems;

        #endregion

        #region Imports

        /// <summary>
        /// The region manager
        /// </summary>
        private readonly IRegionManager _regionManager;
        //public EventHandler DataSetsClick { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the data sets click.
        /// </summary>
        /// <value>
        /// The data sets click.
        /// </value>
        public ICommand DataSetsClick { get; set; }

        /// <summary>
        /// Gets or sets the measures click.
        /// </summary>
        /// <value>
        /// The measures click.
        /// </value>
        public ICommand MeasuresClick { get; set; }

        /// <summary>
        /// Gets or sets the reports click.
        /// </summary>
        /// <value>
        /// The reports click.
        /// </value>
        public ICommand ReportsClick { get; set; }

        /// <summary>
        /// Gets or sets the websites click.
        /// </summary>
        /// <value>
        /// The websites click.
        /// </value>
        public ICommand WebsitesClick { get; set; }

        /// <summary>
        /// Gets or sets the settings click.
        /// </summary>
        /// <value>
        /// The settings click.
        /// </value>
        public ICommand SettingsClick { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftNavigationViewModel" /> class.
        /// </summary>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="regionManager">The region manager.</param>
        /// <exception cref="System.NullReferenceException">regionManager should not be null.</exception>
        [ImportingConstructor]
        public LeftNavigationViewModel(IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            _regionManager = regionManager ?? ServiceLocator.Current.GetInstance<IRegionManager>();


            if (_regionManager == null)
            {
                throw new NullReferenceException("regionManager should not be null.");
            }

            _regionManager = regionManager;

            //this.DataSetsClick = new EventHandler(DataSetsClickExecute);
            this.DataSetsClick = new DelegateCommand<object>(this.DataSetsClickExecute, this.DataSetsClickCanExecute);
            this.MeasuresClick = new DelegateCommand<object>(this.MeasuresClickExecute, this.MeasuresClickCanExecute);
            this.ReportsClick = new DelegateCommand<object>(this.ReportsClickExecute, this.ReportsClickCanExecute);
            this.WebsitesClick = new DelegateCommand<object>(this.WebsitesClickExecute, this.WebsitesClickCanExecute);
            this.SettingsClick = new DelegateCommand<object>(this.SettingsClickExecute, this.SettingsClickCanExecute);

            eventAggregator.GetEvent<DisableNavigationEvent>()
                           .Subscribe(dne => NavigationDisabled = dne.DisableUIElements);
        }


        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the navigtion items.
        /// </summary>
        /// <value>
        /// The navigtion items.
        /// </value>
        public ObservableCollection<MenuItem> NavigationItems
        {
            get { return _navigationItems; }
            set
            {
                _navigationItems = value ?? new ObservableCollection<MenuItem>();
                NotifyPropertyChanged("NavigationItems");

            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [navigation disabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [navigation disabled]; otherwise, <c>false</c>.
        /// </value>
        public bool NavigationDisabled { get; set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get { return MonahrqContext.ApplicationVersion; } }

        #endregion

        #region Methods        
        /// <summary>
        /// To load the data when DataSets from left navigation pane is clicked
        /// </summary>
        /// <param name="arg">The argument.</param>
        private void DataSetsClickExecute(object arg)
        {
            using (ApplicationCursor.SetCursor(Cursors.Wait)) {
                // Load the Load Data view
                _regionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MainDataSetView, UriKind.Relative));
            }
        }
        /// <summary>
        /// To load the data when DataSets from left navigation pane  can be clicked or not
        /// </summary>
        /// <param name="arg">The argument.</param>
        private bool DataSetsClickCanExecute(object arg)
        {
            // TODO: Check to make sure everything on the old view is saved?
            return true;
        }
        /// <summary>
        /// To load the data when Measures from left navigation pane is clicked
        /// </summary>
        /// <param name="arg">The argument.</param>
        private void MeasuresClickExecute(object arg)
        {
            using (ApplicationCursor.SetCursor(Cursors.Wait)) {
                // Load the Projects view
                _regionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MainMeasuresView, UriKind.Relative));
            }
        }
        /// <summary>
        /// To load the data when Measures from left navigation pane can be clicked or not
        /// </summary>
        /// <param name="arg">The argument.</param>
        private bool MeasuresClickCanExecute(object arg)
        {
            // TODO: Check to make sure everything on the old view is saved?
            return true;
        }
        /// <summary>
        /// To load the data when Reports from left navigation pane is clicked
        /// </summary>
        /// <param name="arg">The argument.</param>
        private void ReportsClickExecute(object arg) {
            using (ApplicationCursor.SetCursor(Cursors.Wait)) {
                // Load the Projects view
                _regionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MainReportView, UriKind.Relative));
            }
        }
        /// <summary>
        /// To load the data when Reports from left navigation pane can be clicked or not
        /// </summary>
        /// <param name="arg">The argument.</param>
        private bool ReportsClickCanExecute(object arg)
        {
            // TODO: Check to make sure everything on the old view is saved?
            return true;
        }

        //todo: Import view must be satified before getting the control
        //TESTING IF WebsiteCollectionView LoADING CORRECTLY. 
        //[Import(ViewNames.WebsiteCollectionView)] private UserControl TestWebsiteCollectionViewImports;
        /// <summary>
        /// To load the data when  Website from left navigation pane can is clicked
        /// </summary>
        /// <param name="arg">The argument.</param>
        private void WebsitesClickExecute(object arg) {
            using (ApplicationCursor.SetCursor(Cursors.Wait)) {
                //if (TestWebsiteCollectionViewImports == null) throw new NullReferenceException("LeftNavigationViewModel.cs method: WebsitesClickExecute(object arg) has import error. WebsiteCollectionView are not loading correctly");
                // Load the Projects view
                _regionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.WebsiteCollectionView, UriKind.Relative));
            }
        }
        /// <summary>
        /// To load the data when  Website from left navigation pane can be clicked or not
        /// </summary>
        /// <param name="arg">The argument.</param>
        private bool WebsitesClickCanExecute(object arg)
        {
            // TODO: Check to make sure everything on the old view is saved?
            return true;
        }

        /// <summary>
        /// To load the data when Settings from left navigation pane can is clicked
        /// </summary>
        /// <param name="arg">The argument.</param>
        private void SettingsClickExecute(object arg)
        {
            // Load the Projects view
            //_regionManager.RequestNavigate(RegionNames.MainContent, new Uri("SettingsView", UriKind.Relative));
            //IPopupDialogService popup = ServiceLocator.Current.GetInstance<IPopupDialogService>();
            //popup.ShowMessage("Test Message", "Title", PopupDialogButtons.OK | PopupDialogButtons.Cancel | PopupDialogButtons.Yes | PopupDialogButtons.No | PopupDialogButtons.Abort | PopupDialogButtons.Retry | PopupDialogButtons.Ignore);
            //popup.ShowMessage("Test Message", "Title");
            _regionManager.RequestNavigate(RegionNames.MainContent, new Uri("ManageSettingsView", UriKind.Relative));
        }
        /// <summary>
        /// To load the data when Settings from left navigation pane can be clicked or not
        /// </summary>
        /// <param name="arg">The argument.</param>
        private bool SettingsClickCanExecute(object arg)
        {
            // TODO: Check to make sure everything on the old view is saved?
            return true;
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
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
