using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Events;
using Monahrq.Infrastructure.Services;
using Monahrq.Reports.Model;
using Monahrq.Sdk.Regions;
using System.Windows.Data;
using Monahrq.Sdk.ViewModels;
using NHibernate;
using NHibernate.Linq;
using PropertyChanged;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Types;
using Monahrq.Reports.Views;

namespace Monahrq.Reports.ViewModels
{
    /// <summary>
    /// Enum for filter
    /// </summary>
    public enum FilterKeys { None, ReportName, ReportType, Website, RecommendedAudiences }

    /// <summary>
    /// View model class for  reports
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.ViewModels.ListViewModel{Monahrq.Infrastructure.Entities.Domain.Reports.Report}" />
    /// <seealso cref="Microsoft.Practices.Prism.IActiveAware" />
    [Export(typeof(MainReportsViewModel))]
    [ImplementPropertyChanged]
    public class MainReportsViewModel : ListViewModel<Report>, IActiveAware
    {
        #region Fields and Constants

        const string ALL_CATEGORIES = "All Report Categories";
        private const string REPORT_NAME_ERROR_MESSAGE = "Report Name is required.";
        private const string REPORT_NAME_INVALID_LENGTH_MESSAGE = "Please enter a Report Title using no more than 250 characters.";
        private string _selectedCategory;
        private string _filterText;
        private KeyValuePair<FilterKeys, string> _selectedFilter;
        private string _title;
        public string _reportType;
        private Report _selectedReport;

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the navigate to details command, to navigate to details page
        /// </summary>
        /// <value>
        /// The navigate to details command.
        /// </value>
        public DelegateCommand<Report> NavigateToDetailsCommand { get; set; }

        /// <summary>
        /// Gets or sets the preview command to facilitate the preview for the report
        /// </summary>
        /// <value>
        /// The preview command.
        /// </value>
        public DelegateCommand<Report> PreviewCommand { get; set; }

        /// <summary>
        /// Gets or sets the create new report from context menu command
        /// </summary>
        /// <value>
        /// The create new report from context menu command.
        /// </value>
        public DelegateCommand<Report> CreateNewReportFromContextMenuCommand { get; set; }

        /// <summary>
        /// Gets or sets the on add new report command.
        /// </summary>
        /// <value>
        /// The on add new report command.
        /// </value>
        public DelegateCommand OnAddNewReportCommand { get; set; }

        /// <summary>
        /// Gets or sets the close preview command.
        /// </summary>
        /// <value>
        /// The close preview command.
        /// </value>
        public DelegateCommand ClosePreviewCommand { get; set; }

        #endregion

        #region Imports

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MainReportsViewModel"/> class.
        /// </summary>
        public MainReportsViewModel()
        {
            IsActiveChanged -= OnIsActiveChanged;
            IsActiveChanged += OnIsActiveChanged;

            IsInitialLoad = false;
        }

        /// <summary>
        /// Called when [is active changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnIsActiveChanged(object sender, EventArgs eventArgs)
        {
            if (!IsActive) return;

            OnLoad();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Notifies that the value for <see cref="P:Microsoft.Practices.Prism.IActiveAware.IsActive" /> property has changed.
        /// </summary>
        public event EventHandler IsActiveChanged;

        /// <summary>
        /// Gets or sets a value indicating whether the object is active.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the object is active; otherwise <see langword="false" />.
        /// </value>
        [DoNotCheckEquality]
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;

                if (IsActiveChanged != null)
                    IsActiveChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the is preview open.
        /// </summary>
        /// <value>
        /// The is preview open.
        /// </value>
        public Visibility IsPreviewOpen { get; set; }

        /// <summary>
        /// Gets or sets the is add report open.
        /// </summary>
        /// <value>
        /// The is add report open.
        /// </value>
        public Visibility IsAddReportOpen { get; set; }

        /// <summary>
        /// Gets or sets the selected report.
        /// </summary>
        /// <value>
        /// The selected report.
        /// </value>
        public Report SelectedReport
        {
            get { return _selectedReport; }
            set
            {
                _selectedReport = value;
                if (_selectedReport == null) return;

                var images = _selectedReport.GetPreviewImages();
                _selectedReport.ProfessionalPreviewImage = images["Professional"];
                _selectedReport.ConsumerPreviewImage = images["Consumer"];

                ActivePreviewImageTabIndex = _selectedReport.HasProfessionalsAudience ? 0 : _selectedReport.HasConsumersAudience ? 1 : 0;

                RaisePropertyChanged(() => _selectedReport.ProfessionalPreviewImage);
                RaisePropertyChanged(() => _selectedReport.ConsumerPreviewImage);
            }
        }

        /// <summary>
        /// Gets or sets the selected category.
        /// </summary>
        /// <value>
        /// The selected category.
        /// </value>
        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;

                CollectionItems.Filter = null;
                CollectionItems.Filter = CompositeFilter;

                if (CollectionItems.Count > 0)
                {
                    CollectionItems.MoveCurrentToFirst();
                }
                // select 1st report by default
                SelectedReport = CollectionItems.CurrentItem as Report;

                RaisePropertyChanged(() => SelectedCategory);
            }
        }

        /// <summary>
        /// Gets or sets the report categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public List<string> Categories { get; set; }

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                CollectionItems.Filter = null;
                CollectionItems.Filter = CompositeFilter;
                RaisePropertyChanged(() => FilterText);
                CollectionItems.Refresh();
            }
        }

        //readonly Dictionary<Audience, string> _audiences = new Dictionary<Audience, string>
        //{
        //    { Audience.Consumers, Audience.Consumers.GetDescription()},
        //    { Audience.Professionals, Audience.Professionals.GetDescription()}
        //};

        /// <summary>
        /// Composites the filter, to determine whether the filter is valid or not
        /// </summary>
        /// <param name="reportdisp">The reportdisp.</param>
        /// <returns></returns>
        bool CompositeFilter(object reportdisp)
        {
            var report = reportdisp as Report;
            if (report == null) return true;
            bool fail = SelectedCategory != ALL_CATEGORIES;
            if (fail)
            {
                fail = report.Category.ToString() != SelectedCategory;
                if (fail) return false;
            }

            bool pass = SelectedFilter.Key == FilterKeys.None;
            if (!pass)
            {
                pass = SelectedFilter.Key == FilterKeys.RecommendedAudiences && TestAudience(report);
            }
            if (!pass)
            {
                pass = SelectedFilter.Key == FilterKeys.ReportName && TestName(report);
            }
            if (!pass)
            {
                pass = SelectedFilter.Key == FilterKeys.ReportType && TestType(report);
            }
            if (!pass)
            {
                pass = SelectedFilter.Key == FilterKeys.Website && TestWebsite(report);
            }
            return pass;

        }

        /// <summary>
        /// Tests the website.
        /// </summary>
        /// <param name="vm">The report.</param>
        /// <returns></returns>
        private bool TestWebsite(Report vm)
        {
            return ArrayTest(vm.WebsitesForReportDisplay.Select(s => s));
        }

        /// <summary>
        /// Tests the type.
        /// </summary>
        /// <param name="vm">The report.</param>
        /// <returns></returns>
        private bool TestType(Report vm)
        {
            return StringTest(vm.SourceTemplate.Name);
        }

        /// <summary>
        /// Tests the report name.
        /// </summary>
        /// <param name="vm">The report.</param>
        /// <returns></returns>
        private bool TestName(Report vm)
        {
            return StringTest(vm.Name);
        }

        /// <summary>
        /// Tests the report audience.
        /// </summary>
        /// <param name="vm">The report.</param>
        /// <returns></returns>
        private bool TestAudience(Report vm)
        {
            if (vm.Audiences != null && vm.Audiences.FirstOrDefault() == Audience.None &&
               FilterText != string.Empty)
                return false;

            return vm.Audiences != null && vm.Audiences.Any(audience => StringTest(audience.GetDescription()));
        }

        /// <summary>
        /// Arrays the test.
        /// </summary>
        /// <param name="arry">The string arary.</param>
        /// <returns></returns>
        private bool ArrayTest(IEnumerable<string> arry)
        {
            var array = arry.ToArray();
            if (!array.Any()) return false;
            var result = StringTest(array.First());
            if (!result && array.Count() == 1) return false;
            return result || ArrayTest(array.Skip(1));
        }

        /// <summary>
        /// To test the provided string is valid or not.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        private bool StringTest(string s)
        {
            return s.ToLower().Contains(FilterText.ToLower());
        }

        /// <summary>
        /// Gets or sets the selected filter.
        /// </summary>
        /// <value>
        /// The selected filter.
        /// </value>
        public KeyValuePair<FilterKeys, string> SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                _selectedFilter = value;
                CollectionItems.Filter = null;
                CollectionItems.Filter = CompositeFilter;
                RaisePropertyChanged(() => SelectedFilter);
            }
        }

        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        /// <value>
        /// The filters.
        /// </value>
        public IList<KeyValuePair<FilterKeys, string>> Filters { get; set; }

        [Required(ErrorMessage = REPORT_NAME_ERROR_MESSAGE)]
        [StringLength(250, ErrorMessage = REPORT_NAME_INVALID_LENGTH_MESSAGE)]
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    Validate();
                    RaisePropertyChanged(() => Title);
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the report.
        /// </summary>
        /// <value>
        /// The type of the report.
        /// </value>
        public string ReportType
        {
            get { return _reportType; }
            set
            {
                _reportType = value;
                CollectionItems.Filter = null;
                CollectionItems.Filter = CompositeFilter;
                RaisePropertyChanged(() => SelectedFilter);
            }
        }

        /// <summary>
        /// Gets or sets the report types.
        /// </summary>
        /// <value>
        /// The report types.
        /// </value>
        public List<string> ReportTypes { get; set; }

        /// <summary>
        /// Gets or sets the audiences collection.
        /// </summary>
        /// <value>
        /// The audiences.
        /// </value>
        public ObservableCollection<AudienceModel> Audiences { get; set; }

        /// <summary>
        /// Gets or sets the index of the active preview image tab.
        /// </summary>
        /// <value>
        /// The index of the active preview image tab.
        /// </value>
        public int ActivePreviewImageTabIndex { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected override void InitCommands()
        {
            base.InitCommands();
            NavigateToDetailsCommand = new DelegateCommand<Report>(OnNavigateToDetails);
            PreviewCommand = new DelegateCommand<Report>(OnPreview);
            ClosePreviewCommand = new DelegateCommand(OnClosePreview);
            CreateNewReportFromContextMenuCommand = new DelegateCommand<Report>(OnNewContextReport);
            OnAddNewReportCommand = new DelegateCommand(OnAddNewReport);
        }

        /// <summary>
        /// Called when [add new report].
        /// </summary>
        private void OnAddNewReport()
        {
            IsAddReportOpen = Visibility.Visible;
            base.ClearErrors(() => Title);
            base.ClearErrors(() => Audiences);
        }

        /// <summary>
        /// Initializes the properties.
        /// </summary>
        protected override void InitProperties()
        {
            base.InitProperties();
            Title = string.Empty; //New report Title
            IsAddReportOpen = Visibility.Collapsed;
            IsPreviewOpen = Visibility.Collapsed;

            Categories = GetReportCategories();
            ReportTypes = GetReportTypes();
            Audiences = GetAudiences();

            Audiences.ForEach(x =>
            {
                x.PropertyChanged += (o, e) =>
                {
                    if (Audiences.Any(s => s.IsSelected)) return;

                    MessageBox.Show("At least on recommended audience must be selected.", string.Format("MONAHRQ {0}", MonahrqContext.ApplicationVersion.SubStrBeforeLast(".")));
                };
            });
        }

        //protected async override void OnLoad()
        //{
        //    base.OnLoad();

        //    CollectionItems =
        //        new ListCollectionView(
        //            CollectionItems.OfType<Report>().OrderBy(x => x.Category).ThenBy(x => x.Name).ToList());

        //    if (CollectionItems.Count > 0)
        //    {
        //        CollectionItems.MoveCurrentToFirst();
        //    }

        //    SelectedReport = CollectionItems.CurrentItem as Report;
        //    CollectionItems.Filter = CompositeFilter;

        //    ReportType = ReportTypes[0];
        //    SelectedCategory = Categories[0];

        //    Filter = _filterEnumerationsMainView;
        //    FilterText = string.Empty;
        //    SelectedFilter = Filter.FirstOrDefault();
        //}

        /// <summary>
        /// The filter enumerations main view
        /// </summary>
        private readonly IList<KeyValuePair<FilterKeys, string>> _filterEnumerationsMainView = new List
            <KeyValuePair<FilterKeys, string>>()
        {
            new KeyValuePair<FilterKeys, string>(FilterKeys.None, string.Empty),
            new KeyValuePair<FilterKeys, string>(FilterKeys.ReportName, "Report Name"),
            new KeyValuePair<FilterKeys, string>(FilterKeys.ReportType, "Report Type"),
            new KeyValuePair<FilterKeys, string>(FilterKeys.Website, "Website"),
            new KeyValuePair<FilterKeys, string>(FilterKeys.RecommendedAudiences, "Custom Report"),
            new KeyValuePair<FilterKeys, string>(FilterKeys.RecommendedAudiences, "Recommended Audiences"),
            new KeyValuePair<FilterKeys, string>(FilterKeys.RecommendedAudiences, "All Audiences"),
            new KeyValuePair<FilterKeys, string>(FilterKeys.RecommendedAudiences, "Consumer Audience"),


        };

        private bool _isActive;

        /// <summary>
        /// Override the excel load to provide functionality.
        /// </summary>
        /// <param name="session">The session.</param>
        protected override void ExecLoad(ISession session)
        {
            //base.ExecLoad(session);
            var allReports = session.Query<Report>()
                .OrderByDescending(x => x.IsDefaultReport ? x.DateCreated.Value.Date : x.DateCreated.Value)
                .ThenBy(x => x.Category)
                .ThenBy(x => x.Name)
                .ToFuture()
                .ToObservableCollection();
            var allSites = GetAllWebsites(session);

            AssignWebsiteNames(allReports, allSites);

            CollectionItems = new ListCollectionView(allReports);

            if (CollectionItems.Count > 0)
            {
                CollectionItems.MoveCurrentToFirst();
            }


            SelectedReport = CollectionItems.CurrentItem as Report;
            CollectionItems.Filter = CompositeFilter;
            CollectionItems.MoveCurrentToFirst();
            ReportType = ReportTypes[0];
            SelectedCategory = Categories[0];

            Filters = _filterEnumerationsMainView;
            FilterText = string.Empty;
            SelectedFilter = Filters.FirstOrDefault();
        }

        /// <summary>
        /// Assigns the website names.
        /// </summary>
        /// <param name="allReports">All reports.</param>
        /// <param name="allSites">All sites.</param>
        private void AssignWebsiteNames(ObservableCollection<Report> allReports, List<Tuple<string, List<int>>> allSites)
        {
            foreach (var report in allReports)
            {
                report.WebsitesForReportDisplay = allSites.Where(x => x.Item2.Contains(report.Id))
                    .Select(x => x.Item1)
                    .DistinctBy(x => x)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets all websites.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        private List<Tuple<string, List<int>>> GetAllWebsites(ISession session)
        {
            if (!session.IsOpen)
                session = DataserviceProvider.SessionFactory.OpenSession();

            return session.Query<Website>()
                 .Select(x => Tuple.Create(x.Name, x.Reports.Select(r => r.Report.Id).ToList()))
                 .ToList();
        }

        /// <summary>
        /// Called when close preview is clciked to hide the preview window.
        /// </summary>
        private void OnClosePreview()
        {
            if (SelectedReport != null)
            {
                SelectedReport.ProfessionalPreviewImage = null;
                SelectedReport.ConsumerPreviewImage = null;
            }
            IsPreviewOpen = Visibility.Collapsed;
        }

        /// <summary>
        /// Called when preview is clicked to show the preview.
        /// </summary>
        /// <param name="report">The report.</param>
        private void OnPreview(Report report)
        {
            var images = report.GetPreviewImages();
            SelectedReport.ProfessionalPreviewImage = images["Professional"];
            SelectedReport.ConsumerPreviewImage = images["Consumer"];

            IsPreviewOpen = Visibility.Visible;
            ActivePreviewImageTabIndex = report.HasProfessionalsAudience ? 0 : report.HasConsumersAudience ? 1 : 0;
        }

        /// <summary>
        /// Called when cancel is clicked to hide the add report screen.
        /// </summary>
        protected override void OnCancel()
        {
            IsAddReportOpen = Visibility.Collapsed;
            Reset();
        }

        /// <summary>
        /// Called when navigate to details.
        /// </summary>
        /// <param name="report">The report.</param>
        private void OnNavigateToDetails(Report report)
        {
            //WaitCursor.Show();

            //if (!RegionManager.Regions[RegionNames.MainContent].Views.Any(reg => reg.GetType().Name.EqualsIgnoreCase(typeof(ReportDetailsView).Name)))
            //    RegionManager.RegisterViewWithRegion(RegionNames.MainContent, typeof(ReportDetailsView));

            SelectedReport = report;
            if (report == null) return;
            UriQuery q = new UriQuery();
            q.Add("Report", report.Id.ToString());
            //RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(typeof(ReportDetailsView).Name + q, UriKind.RelativeOrAbsolute));
            RegionManager.NavigateTo(RegionNames.MainContent, typeof(ReportDetailsView), q);

        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset()
        {
            IsAddReportOpen = Visibility.Collapsed;
            Title = string.Empty;

            Audiences.Single(a => a.Value == (int)Audience.Professionals).IsSelected = true;
            Audiences.Where(a => a.Value != (int)Audience.Professionals).ForEach(a => a.IsSelected = false);

        }

        /// <summary>
        /// Called when save is clicked to save the report changes.
        /// </summary>
        protected override async void OnSave()
        {
            base.OnSave();

            if (HasErrors) return;

            if (!(Audiences[1].IsSelected || Audiences[2].IsSelected))
            {
                MessageBox.Show(
                    "Please enter a report type, report name, and recommended audience(s) to add a new report.");
                return;
            }

            try
            {
                var errorOccurred = false;
                var progressService = new ProgressService();

                progressService.SetProgress("Creating report", 0, false, true);

                //await Task.Delay(500);

                var operationComplete = await progressService.Execute(() =>
                {
                    //Get all selected enum values but the default/non-selected value of zero "0"
                    var selectedAudiences =
                        Audiences.Where(a => a.IsSelected && a.Value != 0).Select(a => a.Enum).ToList();
                    //.Aggregate<AudienceModel, uint>(0, (current, a) => current + (uint)a.Value);
                    var factory = new ReportManifestFactory();
                    var manifest = factory.InstalledManifests.FirstOrDefault(x => x.Name == ReportType);
                    var report = new Report(manifest) { Name = Title }; //, Audiences = selectedAudiences.ToList()
                    AddNewItemCommand.Execute(report);
                },
                opResult =>
                {
                    progressService.SetProgress("Completed", 100, true, false);
                    if (opResult.Status && opResult.Exception != null)
                    {
                        errorOccurred = true;
                        NotifyError(opResult.Exception.GetBaseException(), typeof(Report), Title);
                    }
                    else
                    {
                        errorOccurred = false;
                    }
                }, new CancellationToken());

                if (operationComplete && !errorOccurred)
                {
                    OnLoad();
                    Notify(String.Format("New report {0} has been added", Title));
                }
            }
            catch (Exception exc)
            {
                NotifyError(exc, typeof(Report), Title);
            }
            finally
            {
                Reset();
            }
        }

        /// <summary>
        /// Called when [new context report].
        /// </summary>
        /// <param name="reportView">The report view.</param>
        private async void OnNewContextReport(Report reportView)
        {
            var errorOccurred = false;
            var progressService = new ProgressService();

            progressService.SetProgress("Creating report", 0, false, true);

            //await Task.Delay(500);
            var report = new Report(reportView.SourceTemplate);
            report.Name = "New report based on " + report.Name;
            report.IsDefaultReport = false;

            var operationComplete = await progressService.Execute(() =>
            {
                AddNewItemCommand.Execute(report);
            },
            opResult =>
            {
                progressService.SetProgress("Completed", 100, true, false);
                if (opResult.Status && opResult.Exception != null)
                {
                    errorOccurred = true;
                    NotifyError(opResult.Exception.GetBaseException(), typeof(Report), Title);
                }
                else
                {
                    errorOccurred = false;
                }
            }, new CancellationToken());

            if (operationComplete && !errorOccurred)
            {
                OnLoad();
                OnNavigateToDetails(report);
            }
        }

        /// <summary>
        /// Called when new report is added.
        /// </summary>
        /// <param name="report">The report.</param>
        protected override void OnAddNewItem(Report report)
        {
            base.OnAddNewItem(report);
            base.ClearErrors(() => Title);
            base.ClearErrors(() => Audiences);
        }

        /// <summary>
        /// Called when report is deleted.
        /// </summary>
        /// <param name="report">The report.</param>
        protected async override void OnDelete(Report report)
        {
            if (report.WebsitesForReportDisplay != null && report.WebsitesForReportDisplay.Count > 0)
            {
                MessageBox.Show("The report is already been used in a website, please remove it from the website before deleting it", "Delete report", MessageBoxButton.OK);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this report?", "Delete Report", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

            var msg = string.Format("{0} report has been deleted.", report.Name);

            var errorOccurred = false;
            var progressService = new ProgressService();

            progressService.SetProgress("Deleting report", 0, false, true);

            // await Task.Delay(500);

            var operationComplete = await progressService.Execute(() =>
            {
                using (var session = DataserviceProvider.SessionFactory.OpenSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        try
                        {
                            session.Evict(report);
                            session.Delete(report);
                            session.Flush();
                            trans.Commit();
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            },
            opResult =>
            {
                if (!opResult.Status || opResult.Exception != null)
                {
                    errorOccurred = true;
                    var ex = opResult.Exception.GetBaseException();
                    EventAggregator.GetEvent<GenericNotificationEvent>().Publish(ex.ToString());
                    Logger.Log(ex.ToString(), Category.Exception, Priority.High);
                }

                progressService.SetProgress("Completed", 100, true, false);
            },
            new CancellationToken());

            if (operationComplete && !errorOccurred)
            {
                OnLoad();
                Notify(msg);
            }
        }

        /// <summary>
        /// Gets the audiences.
        /// </summary>
        /// <returns></returns>
        private static ObservableCollection<AudienceModel> GetAudiences()
        {
            return (from Audience val in Enum.GetValues(typeof(Audience))
                    select (AudienceModel)Activator.CreateInstance(typeof(AudienceModel), new object[] { val })).ToList().ToObservableCollection();
        }

        /// <summary>
        /// Gets the report types.
        /// </summary>
        /// <returns></returns>
        private List<string> GetReportTypes()
        {
            try
            {
                var factory = new ReportManifestFactory();
                return factory.InstalledManifests.Select(manifest => manifest.Name).ToList();
            }
            catch (Exception e)
            {
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
            }
            return null;
        }

        /// <summary>
        /// Gets the report categories.
        /// </summary>
        /// <returns></returns>
        private static List<string> GetReportCategories()
        {
            var list = new List<string>();
            list.Insert(0, ALL_CATEGORIES);

            foreach (var members in from object val in Enum.GetValues(typeof(ReportCategory)) select typeof(ReportCategory).GetMember(val.ToString()))
            {
                list.AddRange(
                    members.Select(memberInfo => memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false))
                           .Select(att => ((DescriptionAttribute)att[0]).Description));
            }
            return list;
        }

        /// <summary>
        /// Called when [navigated to].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            EventAggregator.GetEvent<SetContextualHelpContextEvent>().Publish("BUILDING AND USING REPORTS LIBRARY");
        }

        /// <summary>
        /// Determines whether navigation 
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        ///   <c>true</c> if [is navigation target] [the specified navigation context]; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        ///Overrides OnNavigatedFrom from the base class.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        #endregion
    }
}
