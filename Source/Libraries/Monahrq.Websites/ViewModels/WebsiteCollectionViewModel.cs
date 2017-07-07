using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Sdk.Regions;
using Monahrq.Theme.Behaviors;
using PropertyChanged;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Websites.Events;
using Monahrq.Default.ViewModels;
using Monahrq.Sdk.Events;
using Monahrq.Infrastructure.Entities.Events;
using System.ComponentModel;
using Monahrq.Websites.Services;

namespace Monahrq.Websites.ViewModels
{
    [Export, ImplementPropertyChanged]
    public class WebsiteCollectionViewModel : BaseViewModel, INavigationAware
    {
        #region Fields and Constants

        private const string _WebSiteReportedPeriod = "Reported Period";
        private const string _WebsiteTargetAudience = "Target Audience";
        private const string _WebsiteStatusInProgress = "Status: In Progress";
        private const string _WebsitestatusComplete = "Status: Complete";
        private const string _websiteDisplayName = "Name";
        private string _selectedSort;

        #endregion

        #region Commands

        public ICommand NewWebsiteCommand { get; set; }

        #endregion

        #region Properties

        ICollectionView ActivityLogView { get; set; }

        public WebsiteViewModel SelectedWebsite { get; set; }

        [DoNotSetChanged]
        public string SelectedSort
        {
            get { return _selectedSort; }
            set
            {
                _selectedSort = value;
                RaisePropertyChanged(() => SelectedSort);
                UpdateWebsitesListView();
            }
        }

        public CollectionViewSource WebsitesCVS { get; set; }

        private ObservableCollection<WebsiteViewModel> WebsiteViewModels { get; set; }

        public int WebsiteCount { get; set; }

        public ObservableCollection<string> SortEnumeration { get; set; }

        #endregion

        #region Imports

        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        IRegionManager RegionManager { get; set; }

        [Import]
        IWebsiteDataService WebsiteDataService { get; set; }

        [Import]
        IBaseDataService BaseDataService { get; set; }

        #endregion

        #region Events

        void OnRequestSelectedWebsite(ExtendedEventArgs<WebsiteViewModel> args)
        {
            args.Data = SelectedWebsite;
        }

        void OnWebsiteSelected(WebsiteViewModel vm)
        {
            SelectedWebsite = vm;
        }

        void OnWebsiteCreatedOrUpdated(ExtendedEventArgs<GenericWebsiteEventArgs> args)
        {
            if (args == null)
                return;

            if (WebsiteViewModels.Any(ws => ws.Website.Id == args.Data.Website.Website.Id)) // Updated
            {
                var wvm = WebsiteViewModels.SingleOrDefault(w => w.Website.Id == args.Data.Website.Website.Id);

                if (wvm != null)
                {
                    wvm.Website = args.Data.Website.Website;
                }
            }
            else // new
            {
                WebsiteViewModels.Add(args.Data.Website);

                //var msg = String.Format("Website {0} has been added", vm.Website.Name);
                Events.GetEvent<GenericNotificationEvent>().Publish(args.Data.Message);

                SelectedWebsite = args.Data.Website;

                // SZ: commented out duplicate call
                //Events.GetEvent<GenericNotificationEvent>().Publish(args.Data.Message);
            }
        }

        //void OnWebsiteUpdated(WebsiteViewModel vm)
        //{
        //    // TODO: re-apply the sortby function now in case the current sort-property was changed on the website.

        //    var msg = String.Format("Website {0} has been updated", vm.Website.Name);
        //    EventAggregator.GetEvent<GenericNotificationEvent>().Publish(msg);

        //    WebsitesCVS.View.Refresh();
        //    SelectedWebsite = vm;
        //}

        void OnWebsiteDeleted(WebsiteViewModel vm)
        {
            var name = vm.Website.Name;
            WebsiteViewModels.Remove(vm);

            var msg = String.Format("Website {0} has been deleted", name);
            Events.GetEvent<GenericNotificationEvent>().Publish(msg);

            WebsitesCVS.View.Refresh();

            // Select the 1st one in the list or null.
            // TODO: better to select 1 below the one just deleted.
            SelectedWebsite = WebsiteViewModels.Any() ? WebsiteViewModels[0] : null;
        }

        #endregion

        #region Methods

        private void UpdateWebsitesListView()
        {
            switch (SelectedSort)
            {
                case _WebSiteReportedPeriod:
                    WebsitesCVS.Source = WebsiteViewModels.OrderBy(wvm => wvm.ReportedPeriod);
                    break;
                case _WebsiteTargetAudience:
                    WebsitesCVS.Source = WebsiteViewModels.OrderBy(wvm => wvm.Website.Audiences);
                    break;
                case _WebsiteStatusInProgress:
                    WebsitesCVS.Source = WebsiteViewModels.Where(wvm => wvm.CurrentStatusLabel == WebsiteViewModel.CurrentStatusLabelInProgress);
                    break;
                case _WebsitestatusComplete:
                    WebsitesCVS.Source = WebsiteViewModels.Where(wvm => wvm.CurrentStatusLabel == WebsiteViewModel.CurrentStatusLabelComplete);
                    break;
                default: // Name is the default 
                    WebsitesCVS.Source = WebsiteViewModels.OrderBy(wvm => wvm.DisplayName);
                    break;
            }

        }

        private void ExecuteNewWebsiteCommand()
        {
            var q = new UriQuery { { "WebsiteId", "-1" } };

            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.WebsiteManageView + q, UriKind.Relative));
        }

        public override void OnImportsSatisfied()
        {
            NewWebsiteCommand = new DelegateCommand(ExecuteNewWebsiteCommand, () => true);

            //Events.GetEvent<WebsiteCreatedOrUpdatedEvent>().Subscribe(OnWebsiteCreatedOrUpdated);
            Events.GetEvent<WebsiteDeletedEvent>().Subscribe(OnWebsiteDeleted);
            Events.GetEvent<WebsiteSelectedEvent>().Subscribe(OnWebsiteSelected);
            Events.GetEvent<RequestSelectedWebsiteEvent>().Subscribe(OnRequestSelectedWebsite);
        }

        private void LoadData()
        {
            //var baseData = ServiceLocator.Current.GetInstance<IBaseDataService>();
            //var quarters = baseData.ReportingQuarters.Where(data => data.Id != null);
            if (WebsiteViewModels != null)
                WebsiteViewModels.Clear();

            SortEnumeration = new ObservableCollection<string> 
            { 
                _websiteDisplayName, 
                _WebSiteReportedPeriod,
                _WebsiteTargetAudience,
                _WebsiteStatusInProgress, 
                _WebsitestatusComplete 
            };

            WebsiteViewModels = WebsiteDataService.GetAllWebsites();
            WebsitesCVS = new CollectionViewSource { Source = WebsiteViewModels };
            WebsiteCount = WebsiteViewModels.Count;

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Events.GetEvent<SetContextualHelpContextEvent>().Publish("CREATING, PUBLISHING, AND MANAGING YOUR WEBSITE");
            LoadData();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //throw new NotImplementedException();
        }

        #endregion

    }
}
