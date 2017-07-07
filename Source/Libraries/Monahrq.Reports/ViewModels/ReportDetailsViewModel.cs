using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Utility;
using Rpts = Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.ViewModels;
using NHibernate;
using NHibernate.Linq;
using Monahrq.Infrastructure.Entities.Domain.Reports.Validators;
using Monahrq.Infrastructure.Types;

namespace Monahrq.Reports.ViewModels
{
    /// <summary>
    /// View mode class for report details
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.ViewModels.DetailsViewModel{Monahrq.Infrastructure.Entities.Domain.Reports.Report}" />
    [Export(typeof(ReportDetailsViewModel))]
    public class ReportDetailsViewModel : DetailsViewModel<Report>
    {
        #region Fields and Constants

        private const string REPORT_NAME_ERROR_MESSAGE = "Report Name is required.";
        private const string REPORT_NAME_INVALID_LENGTH_MESSAGE = "Please enter a Report Title using no more than 250 characters.";
        private string _name;
        private string _footnote;
        private IList<Audience> _audiences;

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the navigate all reports.
        /// </summary>
        /// <value>
        /// The navigate all reports.
        /// </value>
        public DelegateCommand NavigateAllReports { get; set; }

        /// <summary>
        /// Gets or sets the new report command.
        /// </summary>
        /// <value>
        /// The new report command.
        /// </value>
        public DelegateCommand NewReportCommand { get; set; }

        #endregion

        #region Imports

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDetailsViewModel"/> class.
        /// </summary>
        public ReportDetailsViewModel()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the index of the active tab.
        /// </summary>
        /// <value>
        /// The index of the active tab.
        /// </value>
        public int ActiveTabIndex { get; set; }

        /// <summary>
        /// Gets or sets the professional preview image.
        /// </summary>
        /// <value>
        /// The professional preview image.
        /// </value>
        public BitmapImage ProfessionalPreviewImage { get; set; }

        /// <summary>
        /// Gets or sets the consumer preview image.
        /// </summary>
        /// <value>
        /// The consumer preview image.
        /// </value>
        public BitmapImage ConsumerPreviewImage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has attributes.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has attributes; otherwise, <c>false</c>.
        /// </value>
        public bool HasAttributes { get; set; }

        /// <summary>
        /// Gets or sets the footnote.
        /// </summary>
        /// <value>
        /// The footnote.
        /// </value>
        [StringLength(1000)]
        public string Footnote
        {
            get { return _footnote; }
            set
            {
                _footnote = value;
                RaisePropertyChanged(() => Footnote);
                Model.Footnote = Footnote;
            }
        }

        /// <summary>
        /// Gets or sets the audience.
        /// </summary>
        /// <value>
        /// The audience.
        /// </value>
        public AudiencesViewModel Audience { get; set; }

        //public IList<Audience> _Audiences {
        //    get { return this.Model.Audiences ; }
        //}

        /// <summary>
        /// Gets or sets a value indicating whether this instance is user created instance.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is user created instance; otherwise, <c>false</c>.
        /// </value>
        public bool IsUserCreatedInstance { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is custom and show interpretation.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is custom and show interpretation; otherwise, <c>false</c>.
        /// </value>
        public bool IsCustomAndShowInterpretation { get { return IsUserCreatedInstance && Model.ShowInterpretationText; } }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required(ErrorMessage = REPORT_NAME_ERROR_MESSAGE)]
        [StringLength(250, ErrorMessage = REPORT_NAME_INVALID_LENGTH_MESSAGE)]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    Model.Name = _name;
                    Validate();
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        /// <summary>
        /// Gets or sets the filter collection view.
        /// </summary>
        /// <value>
        /// The filter collection view.
        /// </value>
        public CollectionView FilterCollectionView { get; set; }

        /// <summary>
        /// Gets or sets the index of the report attributes tab.
        /// </summary>
        /// <value>
        /// The index of the report attributes tab.
        /// </value>
        public int ReportAttributesTabIndex { get; set; }

        /// <summary>
        /// Gets or sets the selected icon set.
        /// </summary>
        /// <value>
        /// The selected icon set.
        /// </value>
        public ComparisonKeyIconSet SelectedIconSet { get; set; }

        //[NonEmptyList("Model.Audiences",ErrorMessage = "Must select, at least, one Audience")]
        //public bool HasConsumersAudience {
        //    get {
        //        if (Model.Audiences == null) return false;
        //        return Model.Audiences.Any(a => a == Rpts.Audience.Consumers);
        //    }
        //    set {
        //        if (value) Model.Audiences.Add(Rpts.Audience.Consumers);
        //        else Model.Audiences.Remove(Rpts.Audience.Consumers);
        //        Validate();
        //        RaisePropertyChanged(() => HasConsumersAudience);
        //    }
        //}

        /// <summary>
        /// Gets or sets the index of the active preview image tab.
        /// </summary>
        /// <value>
        /// The index of the active preview image tab.
        /// </value>
        public int ActivePreviewImageTabIndex { get; set; }

        #endregion

        #region Commands

        #endregion

        #region Methods

        /// <summary>
        /// Overrides the OnNavigatedTo from the base class.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            var param = navigationContext.Parameters["Report"];
            ActiveTabIndex = 0;
            if (param != null)
            {
                LoadModel(int.Parse(param));
                ActivePreviewImageTabIndex = Model == null
                    ? 0
                    : Model.HasProfessionalsAudience ? 0 : Model.HasConsumersAudience ? 1 : 0;
            }

            EventAggregator.GetEvent<SetContextualHelpContextEvent>().Publish("Reports");
        }

        /// <summary>
        /// Determines whether [is navigation target] [the specified navigation context].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        ///   <c>true</c> if [is navigation target] [the specified navigation context]; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        //public override void OnNavigatedFrom(NavigationContext navigationContext)
        //{
        //    if (!Model.IsPersisted || IsDirty(Model))
        //    {
        //        if (MessageBox.Show("The data has been edited. Are you sure you want to leave before saving?", "Modifaction Verification", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
        //        {
        //            return;
        //            UriQuery q = new UriQuery();
        //            q.Add("Report", Model.Id.ToString());
        //            //RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(typeof(ReportDetailsView).Name + q, UriKind.RelativeOrAbsolute));
        //            RegionManager.NavigateTo(RegionNames.MainContent, this.GetType(), q);
        //        }
        //    }
        //    base.OnNavigatedFrom(navigationContext);
        //}

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected override void InitCommands()
        {
            base.InitCommands();
            NavigateAllReports = new DelegateCommand(OnNavigateAllReports);
            NewReportCommand = new DelegateCommand(OnNewReport);
        }

        /// <summary>
        /// Called when report details are modified and user wants to save.
        /// </summary>
        /// <param name="enableNotificantions"></param>
        public override async void OnSave(bool enableNotificantions = true)
        {
            Validate();
            if (HasErrors) return;

            if (!Model.HasConsumersAudience && !Model.HasProfessionalsAudience)
            {
                MessageBox.Show("Please enter recommended audience(s) to add a new report.");
                return;
            }

            var errorOccurred = false;
            Exception saveException = null;
            var progressService = new ProgressService();

            progressService.SetProgress("Saving report", 0, false, true);

            // await Task.Delay(500);

            var operationComplete = await progressService.Execute(() =>
            {
                base.OnSave(false);
            },
            opResult =>
            {
                if (!opResult.Status && opResult.Exception != null)
                {
                    saveException = opResult.Exception.GetBaseException();
                    errorOccurred = true;
                }
                else
                {
                    saveException = null;
                    errorOccurred = false;
                }
            },
            new CancellationToken());

            if (operationComplete)
            {
                progressService.SetProgress("Completed", 100, true, false);

                if (!errorOccurred && saveException == null)
                {
                    Notify(string.Format("{0} {1} has been successfully saved.", Model.Name,
                                                              Model.GetType().Name));
                    RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(RegionNames.MainReportView, UriKind.Relative));
                }
                else
                {
                    EventAggregator.GetEvent<GenericNotificationEvent>().Publish(saveException.ToString());
                    Logger.Log(saveException.ToString(), Category.Exception, Priority.High);
                }
            }
        }

        /// <summary>
        /// Executes the load.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="id">The identifier.</param>
        protected override void ExecLoad(ISession session, object id)
        {
            base.ExecLoad(session, id);

            Model.WebsitesForReportDisplay = GetWebsites(Model.Id);
            var images = Model.GetPreviewImages();
            ProfessionalPreviewImage = images["Professional"];
            ConsumerPreviewImage = images["Consumer"];

            //AttributeCollectionSet = AttributeViewModelFactory.Get(Model);
            Audience = new AudiencesViewModel(Model);
            Footnote = Model.Footnote;
            IsUserCreatedInstance = !Model.IsDefaultReport;
            Name = Model.Name;
            HasAttributes = (Model.Filters != null && Model.Filters.Count > 0) || (Model.Columns != null && Model.Columns.Count > 0)
                || (Model.ComparisonKeyIcons != null && Model.ComparisonKeyIcons.Count > 0);

            if (Model.Columns != null && Model.Columns.Any())
            {
                if (Model.Columns.Any(x => x == null))
                    Model.Columns = Model.Columns.RemoveNullValues();

                Model.Columns = Model.Columns.DistinctBy(c => c.Name).ToList();
            }

            if (Model.Filters != null)
            {
                FilterCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(Model.Filters.ToList());
                FilterCollectionView.GroupDescriptions.Clear();
                FilterCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Type"));

                ReportAttributesTabIndex = (Model.Filters != null && Model.Filters.Count > 0)
                    ? 0 : (Model.Columns != null && Model.Columns.Count > 0)
                    ? 1 : (Model.ComparisonKeyIcons != null && Model.ComparisonKeyIcons.Count > 0)
                    ? 2 : 0;
            }
            if (Model.ComparisonKeyIcons != null)
            {
                Model.ComparisonKeyIcons.RemoveAll(x => x == null);
                SelectedIconSet = Model.ComparisonKeyIcons.FirstOrDefault(x => x != null && x.IsIncluded);
            }
            OriginalModelHashCode = GetModelHashCode(Model);
        }

        /// <summary>
        /// Called when [new report].
        /// </summary>
        private void OnNewReport()
        {
            var report = new Report(Model.SourceTemplate)
            {
                IsDefaultReport = false,
            };
            report.DateCreated = null;
            report.LastReportManifestUpdate = System.Data.SqlTypes.SqlDateTime.MinValue.Value; //DateTime.MinValue;	// new DateTime(2000, 01, 01);
            IsUserCreatedInstance = true;
            report.Filters = Model.Filters;
            report.ComparisonKeyIcons.RemoveAll(x => x == null);

            if (report.Filters != null && report.Filters.Any())
                report.Filters.ForEach(rf => rf.Owner = report);

            Model = report;
            Audience = new AudiencesViewModel(Model);

            OriginalModelHashCode = GetModelHashCode(Model);
        }

        /// <summary>
        /// Called when cancel is clicked and navigates to all reports screen.
        /// </summary>
        public override void OnCancel()
        {
            OnNavigateAllReports();
        }

        /// <summary>
        /// To navigate to all reports screen
        /// </summary>
        private void OnNavigateAllReports()
        {
            if (!Model.IsPersisted || IsDirty(Model))
            {
                if (MessageBox.Show("The data has been edited. Are you sure you want to leave before saving?", "Modification Verification", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }
            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(RegionNames.MainReportView, UriKind.Relative));
        }

        /// <summary>
        /// Gets the websites.
        /// </summary>
        /// <param name="reportId">The report identifier.</param>
        /// <returns></returns>
        private List<string> GetWebsites(int reportId)
        {
            using (var session = DataServiceProvider.SessionFactory.OpenStatelessSession())
            {
                return session.Query<Website>()
                              .Where(x => x.Reports.Any(r => r.Report.Id == reportId))
                              .Select(w => w.Name)
                              .ToList();
            }
        }

        #endregion
    }
}
