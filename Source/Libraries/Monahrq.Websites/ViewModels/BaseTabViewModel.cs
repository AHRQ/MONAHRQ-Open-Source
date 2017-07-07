using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.ViewModels;
using NHibernate.Mapping.ByCode;
using PropertyChanged;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Websites.Events;
using Monahrq.Websites.Services;
using System.Text.RegularExpressions;
using System.Linq;
using Monahrq.Infrastructure.Domain.Websites;

namespace Monahrq.Websites.ViewModels
{
    public interface IWebsiteTabViewModel
    {
        void Save();
        void Continue();
        void GenerateSite();
        void RunDependencyCheck();
        void ReviewSite();
        bool ValidateAllRequiredFields();
        WebsiteManageViewModel ManageViewModel { get; set; }
        void RefreshUIElements();
    }

    //[ImplementPropertyChanged]
    //public abstract class BaseTabViewModel : BaseViewModel, IWebsiteTabViewModel, IPartImportsSatisfiedNotification
    //{
    //    protected BaseTabViewModel()
    //    {
    //        //WebsiteViewModel = GetSelectedWebsite();
    //    }

    //    [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
    //    protected IEventAggregator EventAggregator { get; set; }

    //    [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
    //    protected IConfigurationService ConfigurationService { get; set; }

    //    [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
    //    public IWebsiteManageViewModel ManageViewModel { get; set; }

    //    [Import(LogNames.Session)]
    //    protected ILoggerFacade Logger
    //    {
    //        get;
    //        set;
    //    }

    //    #region Properties

    //    /// <summary>
    //    /// Gets or sets the base title.
    //    /// </summary>
    //    /// <value>
    //    /// The base title.
    //    /// </value>
    //    public string BaseTitle { get; set; }               // TODO
    //    /// <summary>
    //    /// Gets the tab title.
    //    /// </summary>
    //    /// <value>
    //    /// The tab title.
    //    /// </value>
    //    public string TabTitle { get; private set; }        // TODO
    //    /// <summary>
    //    /// Gets or sets a value indicating whether [is tab selected].
    //    /// </summary>
    //    /// <value>
    //    ///   <c>true</c> if [is tab selected]; otherwise, <c>false</c>.
    //    /// </value>
    //    public bool IsTabSelected { get; set; }             // TODO
    //    /// <summary>
    //    /// Gets or sets the region manager.
    //    /// </summary>
    //    /// <value>
    //    /// The region manager.
    //    /// </value>
    //    [Import]
    //    public IRegionManager RegionManager { get; set; }
    //    /// <summary>
    //    /// Gets or sets the base data service.
    //    /// </summary>
    //    /// <value>
    //    /// The base data service.
    //    /// </value>
    //    [Import]
    //    public IBaseDataService BaseDataService { get; set; }
    //    /// <summary>
    //    /// Gets or sets the website data service.
    //    /// </summary>
    //    /// <value>
    //    /// The website data service.
    //    /// </value>
    //    [Import]
    //    public IWebsiteDataService WebsiteDataService { get; set; }
    //    ///// <summary>
    //    ///// Gets or sets the website view model.
    //    ///// </summary>
    //    ///// <value>
    //    ///// The website view model.
    //    ///// </value>
    //    //public WebsiteViewModel WebsiteViewModel
    //    //{
    //    //    get { return ManageViewModel != null ? ManageViewModel.WebsiteViewModel : null; }
    //    //    set { ManageViewModel.WebsiteViewModel = value; }
    //    //}
    //    #endregion

    //    #region Methods & Events
    //    /// <summary>
    //    /// Initializes the commands.
    //    /// </summary>
    //    protected abstract void InitCommands();
    //    /// <summary>
    //    /// Initializes the properties.
    //    /// </summary>
    //    protected abstract void InitProperties();
    //    /// <summary>
    //    /// Saves this instance.
    //    /// </summary>
    //    public abstract void Save();
    //    /// <summary>
    //    /// Continues this to the next tab in the website creation process.
    //    /// </summary>
    //    public abstract void Continue();

    //    public virtual void GenerateSite() { } // TODO: Why is this in the base class since it's only used in WebsitePublishViewModel. Jason
    //    public virtual void ReviewSite() { } // TODO: Why is this in the base class since it's only used in WebsitePublishViewModel. Jason
    //    public virtual void RunDependencyCheck() { }

    //    public virtual bool ValidateAllRequiredFields() { return true; }

    //    ///// <summary>
    //    ///// Gets the selected website.
    //    ///// </summary>
    //    ///// <returns></returns>
    //    //WebsiteViewModel GetSelectedWebsite()
    //    //{
    //    //    var args = new ExtendedEventArgs<WebsiteViewModel>();
    //    //    EventAggregator.GetEvent<RequestSelectedWebsiteEvent>().Publish(args);
    //    //    return args.Data;
    //    //}

    //    /// <summary>
    //    /// Called when a part's imports have been satisfied and it is safe to use.
    //    /// </summary>
    //    public override void OnImportsSatisfied()
    //    {
    //        Events.GetEvent<UpdateWebsiteTabContextEvent>().Subscribe(OnUpdateWebsiteTabContext);

    //        InitCommands();
    //        InitProperties();
    //    }

    //    /// <summary>
    //    /// Called when [update website tab context].
    //    /// </summary>
    //    /// <param name="website">The website.</param>
    //    /// <exception cref="System.NotImplementedException"></exception>
    //    protected abstract void OnUpdateWebsiteTabContext(UpdateTabContextEventArgs eventArgs);

    //    //{
    //    //    if (websiteViewModel != null)
    //    //    {
    //    //        WebsiteViewModel = websiteViewModel;
    //    //    }
    //    //}

    //    /// <summary>
    //    /// Gets the name of the folder.
    //    /// </summary>
    //    /// <param name="outputFolderName">Name of the output folder.</param>
    //    /// <returns></returns>
    //    public static string GetFolderName(string outputFolderName)
    //    {
    //        if (string.IsNullOrEmpty(outputFolderName)) return string.Empty;
    //        Regex whiteSpaces = new Regex(@"[ ]{2}");

    //        Path.GetInvalidFileNameChars().ToList().ForEach(x =>
    //        {
    //            outputFolderName = outputFolderName.Replace(x, ' ');
    //        });

    //        whiteSpaces.Matches(outputFolderName).OfType<Match>().ToList().ForEach(u =>
    //        {
    //            outputFolderName = outputFolderName.Replace(u.Value, "");
    //        });

    //        return outputFolderName;
    //    }

    //    /// <summary>
    //    /// Refreshes the UI elements.
    //    /// </summary>
    //    public virtual void RefreshUIElements()
    //    {}

    //    #endregion
    //}

    [ImplementPropertyChanged]
    public abstract class WebsiteTabViewModel : ListTabViewModel<Website>
    {
        #region Imports

        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public WebsiteManageViewModel ManageViewModel { get; set; }

        /// <summary>
        /// Gets or sets the base data service.
        /// </summary>
        /// <value>
        /// The base data service.
        /// </value>
        [Import]
        public IBaseDataService BaseDataService { get; set; }
        /// <summary>
        /// Gets or sets the website data service.
        /// </summary>
        /// <value>
        /// The website data service.
        /// </value>
        [Import]
        public IWebsiteDataService WebsiteDataService { get; set; }

        #endregion

        #region Properties

        public string BaseTitle { get; set; }

        public virtual bool ValidateAllRequiredFields() { return true; }

        public Website CurrentWebsite
        {
            get
            {
                return ManageViewModel != null && ManageViewModel.WebsiteViewModel != null ? ManageViewModel.WebsiteViewModel.Website : null;
            }
        }

        public bool IsTabVisited { get; set; }

        #endregion

        #region Methods & Events

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public abstract void Save();

        /// <summary>
        /// Continues this to the next tab in the website creation process.
        /// </summary>
        public abstract void Continue();

        /// <summary>
        /// Gets the name of the folder.
        /// </summary>
        /// <param name="outputFolderName">Name of the output folder.</param>
        /// <returns></returns>
        public static string GetFolderName(string outputFolderName)
        {
            if (string.IsNullOrEmpty(outputFolderName)) return string.Empty;
            Regex whiteSpaces = new Regex(@"[ ]{2}");

            Path.GetInvalidFileNameChars().ToList().ForEach(x =>
            {
                outputFolderName = outputFolderName.Replace(x, ' ');
            });

            whiteSpaces.Matches(outputFolderName).OfType<Match>().ToList().ForEach(u =>
            {
                outputFolderName = outputFolderName.Replace(u.Value, "");
            });

            return outputFolderName;
        }

        /// <summary>
        /// Refreshes the UI elements.
        /// </summary>
        public virtual void RefreshUIElements() { }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
			base.OnNavigatedTo(navigationContext);
        }

        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
			base.OnNavigatedFrom(navigationContext);
		}

		protected override void ListTabViewModel_IsActiveChanged(object sender, EventArgs e)
        {
            if (!IsActive) return;

            IsTabVisited = true;
            RaisePropertyChanged(() => CurrentWebsite);

            RaisePropertyChanged(() => ManageViewModel);
            RaisePropertyChanged(() => ManageViewModel.WebsiteViewModel);
            RaisePropertyChanged(() => ManageViewModel.WebsiteViewModel.Website);
            Refresh();
        }

        #endregion

    }
}
