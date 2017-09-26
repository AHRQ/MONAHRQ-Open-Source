using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.BaseData.ViewModel;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Infrastructure.Types;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Utilities;
using Monahrq.Websites.Events;
using Monahrq.Websites.Services;
using NHibernate.Util;
using PropertyChanged;

namespace Monahrq.Websites.ViewModels
{
    [Export, ImplementPropertyChanged]
    public class WebsiteViewModel : BaseViewModel, IWebsiteViewModel
    {
        #region Fields and Constants

        public const string CurrentStatusLabelComplete = "Complete";
        public const string CurrentStatusLabelInProgress = "In Progress";
        private string _displayName;

        #endregion

        #region Constructor

        public WebsiteViewModel()
        {
            ManageCommand = new DelegateCommand<WebsiteViewModel>(ExecuteManageCommand, CanExecute);
            DeleteCommand = new DelegateCommand<WebsiteViewModel>(ExecuteDeleteCommand, CanExecute);
            ExportCommand = new DelegateCommand<WebsiteViewModel>(ExecuteExportCommand, CanExecute);
            
        }
        #endregion

        #region Imports

        public IBaseDataService BaseDataService { get; set; }

        public IWebsiteDataService WebsiteDataService { get; set; }

        public IRegionManager RegionManager { get; set; }

        #endregion

        #region Properties

        public Website Website { get; set; }

        public ICollectionView ActivityLogView { get; set; }

        public ObservableCollection<EntityViewModel<ReportingQuarter, int>> Quarters { get; set; }

        public string Description
        {
            get
            {
                return (Website == null || Website.Name == null) ? string.Empty : Website.Description;
            }
        }

        //public string Audience
        //{
        //    get
        //    {
        //        return Website == null ? string.Empty : Website.Audience.GetDescription();
        //    }
        //}

        // create pretty name for UI in case it's a new website
        public string DisplayName
        {
            get
            {
                //if (_displayName.IsNullOrEmpty())
                //{
                _displayName = (Website == null || Website.Name.IsNullOrEmpty())
                                   ? "[New Website]"
                                   : Website.Name;
                //}

                return _displayName;
            }
            set
            {
                if (Equals(_displayName, value))
                    return;

                _displayName = value;
                RaisePropertyChanged(() => "DisplayName");
            }
        }

        // combine the Quarter/Year into 1 string for UI
        public string ReportedPeriod
        {
            get
            {
                if (Website.ReportedQuarter.HasValue)
                {
                    return string.Format("{0} Quarter, {1}", Inflector.Ordinalize(Website.ReportedQuarter.Value.ToString()), Website.ReportedYear);
                }
                else
                {
                    return Website.ReportedYear;
                }

            }
        }

        public Visibility ShowContextualInfo
        {
            get
            {
                return Website != null && (Website.StateContext != null && Website.StateContext.Any()) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // map internal state into 1 string for UI
        public string CurrentStatusLabel
        {
            get
            {
                return (Website.CurrentStatus.GetValueOrDefault() <= WebsiteState.CompletedDependencyCheck) ? CurrentStatusLabelInProgress : CurrentStatusLabelComplete;
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets or sets the export command.
        /// </summary>
        /// <value>
        /// The export command.
        /// </value>
        public ICommand ExportCommand { get; set; }
    
        /// <summary>
        /// Gets or sets the manage command.
        /// </summary>
        /// <value>
        /// The manage command.
        /// </value>
        public ICommand ManageCommand { get; set; }
        /// <summary>
        /// Gets or sets the delete command.
        /// </summary>
        /// <value>
        /// The delete command.
        /// </value>
        public ICommand DeleteCommand { get; set; }

        #endregion

        #region Methods & Events
     
        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            Quarters = BaseDataService.ReportingQuarters;
        }

        /// <summary>
        /// Executes the manage command.
        /// </summary>
        /// <param name="vm">The vm.</param>
        private void ExecuteManageCommand(WebsiteViewModel vm)
        {
            if (vm == null) return;
            using (var cursor = ApplicationCursor.SetCursor(Cursors.Wait))
            {
                var q = new UriQuery { { "WebsiteId", vm.Website.Id.ToString() } };
                RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.WebsiteManageView + q, UriKind.Relative));
            }
        }

        private void ExecuteExportCommand(WebsiteViewModel vm)
        {
            try
            {
                if (vm == null) return;
                //if (WebsiteDataService == null)
                    //Events.GetEvent<ErrorNotificationEvent>().Publish();

                base.Logger.Information($"Exporting configuration for website \"{vm.DisplayName}\"");
                Website websiteToExport = null;
                using (ApplicationCursor.SetCursor(Cursors.Wait))
                {
                    WebsiteDataService.GetEntityById<Website>(vm.Website.Id, (website, exception) =>
                    {
                        if (exception == null)
                            websiteToExport = website;
                        else
                            Events.GetEvent<ErrorNotificationEvent>().Publish(exception);
                    });

                    if (websiteToExport == null) return;

                    WebsiteExporter.Export(websiteToExport);
                }
            }
            catch (Exception exc) 
            {
                Events.GetEvent<ErrorNotificationEvent>().Publish(exc);
            }

        }

        /// <summary>
        /// Executes the delete command.
        /// </summary>
        /// <param name="vm">The vm.</param>
        private void ExecuteDeleteCommand(WebsiteViewModel vm)
        {
            if (MessageBox.Show(
                string.Format("Are you sure you want to delete this website: {0}", vm.Website.Name),
                "Delete Website?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            
            if (!WebsiteDataService.DeleteWebsite(vm.Website))
            {
                // publish so the WebsiteCollectionView will refresh without this web
                Events.GetEvent<WebsiteDeletedEvent>().Publish(vm);
            }
        }

        ///// <summary>
        ///// Called when [update website tab context].
        ///// </summary>
        ///// <param name="obj">The object.</param>
        ///// <param name="website"></param>
        ///// <exception cref="System.NotImplementedException"></exception>
        //void OnUpdateWebsiteTabContext(WebsiteViewModel website)
        //{
        //    if (website != null)
        //    {
        //        Website = website.Website;
        //    }
        //}

        private bool CanExecute(WebsiteViewModel vm)
        {
            return true;
        }

        #endregion
    }
}
