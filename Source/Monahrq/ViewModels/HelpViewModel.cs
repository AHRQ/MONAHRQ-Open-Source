using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Events;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Regions;
using Monahrq.Theme.Events;
using Monahrq.Theme.PopupDialog;
using PropertyChanged;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;

namespace Monahrq.ViewModels
{
    /// <summary>
    /// Class for help related operations
    /// </summary>
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    [ImplementPropertyChanged]
    [Export]
    public class HelpViewModel : IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Gets or sets the region MGR.
        /// </summary>
        /// <value>
        /// The region MGR.
        /// </value>
        private IRegionManager RegionMgr { get; set; }
        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        protected IEventAggregator Events { get; set; }

        //public ICommand OpenCloseClick { get; set; }
        /// <summary>
        /// Gets or sets the popup click.
        /// </summary>
        /// <value>
        /// The popup click.
        /// </value>
        public ICommand PopupClick { get; set; }
        /// <summary>
        /// Gets or sets the data import value map click.
        /// </summary>
        /// <value>
        /// The data import value map click.
        /// </value>
        public ICommand DataImportValueMapClick { get; set; }

        /// <summary>
        /// Gets or sets the open help click.
        /// </summary>
        /// <value>
        /// The open help click.
        /// </value>
        public ICommand OpenHelpClick { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [help window is open].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [help window is open]; otherwise, <c>false</c>.
        /// </value>
        private bool HelpWindowIsOpen { get; set; }
        /// <summary>
        /// Gets or sets the content of the help.
        /// </summary>
        /// <value>
        /// The content of the help.
        /// </value>
        public string HelpContent { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="HelpViewModel"/> class.
        /// </summary>
        public HelpViewModel()
        {
            RegionMgr = ServiceLocator.Current.GetInstance<IRegionManager>();
            Events = ServiceLocator.Current.GetInstance<IEventAggregator>();

            HelpWindowIsOpen = true;
            HelpContent = "";

            // this.OpenCloseClick = new DelegateCommand(this.OpenCloseClickExecute,() => this.OpenCloseClickCanExecute(null));
            OpenHelpClick = new DelegateCommand<string>(LoadHelpContent);
            PopupClick = new DelegateCommand(PopupClickExecute, () => PopupClickCanExecute(null));
            DataImportValueMapClick = new DelegateCommand(DataImportValueMapClickExecute, () => DataImportValueMapClickCanExecute(null));
        }

        /// <summary>
        /// Loads the content of the help.
        /// </summary>
        /// <param name="directHelpContent">Content of the direct help.</param>
        private void LoadHelpContent(string directHelpContent = null)
        {
            // TODO: Pull help out of database? based upon the view name. Start with that as the context sensitive help.
            // TODO: Subscribe to a shell window change to update help?
            //var activeView = RegionMgr.Regions[RegionNames.MainContent].ActiveViews.FirstOrDefault();
            //if (activeView != null)
            //    HelpContent = activeView.ToString().SubStrAfterLast(".");

            var helpPage = string.Format("{0}.html", directHelpContent ?? HelpContent);
            Help.ShowHelp(null, MonahrqContext.HelpFolderPath, helpPage);
        }

        /// <summary>
        /// Resolves the contextual help view.
        /// </summary>
        /// <returns></returns>
        private string ResolveContextualHelpView()
        {
            int viewIndex = 4;
            //if (!string.IsNullOrEmpty(HelpContent) && _helpContentPaths.Any(v => v.Item1.EqualsIgnoreCase(HelpContent)))
            //{
            //    var helpItems = _helpContentPaths.Where(i => i.Item1.EqualsIgnoreCase(HelpContent)).ToList();

            //    if (helpItems.Count > 0)
            //    {
            //        object childView = null;

            //        foreach (var item in helpItems)
            //        {
            //            if (!string.IsNullOrEmpty(item.Item2) && !string.IsNullOrEmpty(item.Item3))
            //                childView = RegionMgr.Regions[item.Item2].ActiveViews.FirstOrDefault(v => v.ToString().SubStrAfterLast(".").EqualsIgnoreCase(item.Item3));

            //            if (childView != null)
            //                break;
            //        }

            //        if (childView != null)
            //        {
            //            var childHelpItem =
            //                _helpContentPaths.FirstOrDefault(
            //                    i =>
            //                    i.Item1.EqualsIgnoreCase(HelpContent) &&
            //                    i.Item3.EqualsIgnoreCase(childView.ToString().SubStrAfterLast(".")));

            //            if (childHelpItem != null)
            //                viewIndex = _helpContentPaths.IndexOf(childHelpItem);
            //        }
            //        else
            //            viewIndex = _helpContentPaths.IndexOf(helpItems.Count == 0 ? helpItems[0] : helpItems.First());
            //    }

            // RegionMgr.Regions[RegionNames.MainContent].ActiveViews.FirstOrDefault().ToString() 
            //}

            if (!string.IsNullOrEmpty(HelpContent) && _helpContentPaths.Any(h => h.Item1.EqualsIgnoreCase(HelpContent)))
                viewIndex = _helpContentPaths.FindIndex(help => help.Item1.EqualsIgnoreCase(HelpContent));

            return _helpContentPaths[viewIndex].Item4;
        }

        /// <summary>
        /// The help content paths
        /// </summary>
        private readonly List<Tuple<string, string, string, string>> _helpContentPaths = new List<Tuple<string, string, string, string>>
            {
                new Tuple<string, string, string, string>("Reports",null,null,"BUILDING_AND_USING_YOUR_REPORTS_LIBRARY"),
                new Tuple<string, string, string, string>("Websites",null,null,"CONFIGURING_YOUR_MONAHRQ_WEBSITE"),
                new Tuple<string, string, string, string>("WebsiteSettings","WebsiteSettingsRegion","WebsiteSettingsView","CUSTOMIZING_AND_PUBLISHING_YOUR_WEBSITE"),
                new Tuple<string, string, string, string>("Measures",null,null,"MANAGING_YOUR_MEASURES_LIBRARY"),
                new Tuple<string, string, string, string>("Datasets",null,null,"CHOOSING_AND_MANAGING_YOUR_DATASET_LIBRARY"), 
                //new Tuple<string, string, string, string>("Default",null,null,"TROUBLESHOOTING"), 
                new Tuple<string, string, string, string>("Default",null,null,"MONAHRQ_OVERVIEW"),
            };


        #region Open / Close Window Commands

        /// <summary>
        /// Opens the close click execute.
        /// </summary>
        private void OpenCloseClickExecute()
        {
            if (HelpWindowIsOpen)
            {
                HelpWindowIsOpen = false;
            }
            else
            {
                LoadHelpContent();
                HelpWindowIsOpen = true;
            }
            Events.GetEvent<HelpOpenCloseEvent>().Publish(HelpWindowIsOpen);
        }

        /// <summary>
        /// To check whether Open,close, click can be executed or not.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        private bool OpenCloseClickCanExecute(object arg)
        {
            return true;
        }

        #endregion Open / Close Window Commands

        #region Popup Testing

        /// <summary>
        /// Popups the click execute.
        /// </summary>
        public void PopupClickExecute()
        {
            IPopupDialogService popupService = ServiceLocator.Current.GetInstance<IPopupDialogService>();
            Events.GetEvent<PopupDialogClickEvent>().Subscribe(e => PopupResponse(e));

            // TODO: Temporarily using this button to test the popups.
            popupService.ShowMessage("This is a sample message.", "Title", PopupDialogButtons.OK | PopupDialogButtons.Cancel | PopupDialogButtons.Yes | PopupDialogButtons.No | PopupDialogButtons.Abort | PopupDialogButtons.Retry | PopupDialogButtons.Ignore);
            //ContentControl ctrl = new ContentControl();
        }

        /// <summary>
        /// Popups the click can execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public bool PopupClickCanExecute(object arg)
        {
            return true;
        }

        /// <summary>
        /// Popups the response.
        /// </summary>
        /// <param name="button">The button.</param>
        public void PopupResponse(PopupDialogButtons button)
        {
            System.Windows.MessageBox.Show(button.ToString() + " button pushed");
            Events.GetEvent<PopupDialogClickEvent>().Unsubscribe(e => PopupResponse(e));

        }

        #endregion

        #region DataImportValueMap Testing

        /// <summary>
        /// Datas the import value map click execute.
        /// </summary>
        public void DataImportValueMapClickExecute()
        {
            RegionMgr.RequestNavigate(RegionNames.MainContent, new Uri("DataImportValueMapView", UriKind.Relative));
        }

        /// <summary>
        /// Datas the import value map click can execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public bool DataImportValueMapClickCanExecute(object arg)
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            Events.GetEvent<SetContextualHelpContextEvent>().Subscribe(context =>
                {
                    HelpContent = context;
                });

            Events.GetEvent<OpenContextualHelpContextEvent>().Subscribe(LoadHelpContent);
        }
    }
}
