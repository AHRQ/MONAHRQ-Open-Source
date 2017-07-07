using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using mshtml;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure.BaseDataLoader;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Types;
using Monahrq.Sdk.Common;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Regions;
using Monahrq.Websites.Events;
using Monahrq.Websites.Model;
using Monahrq.Websites.Services;
using NHibernate;
using NHibernate.Transform;

namespace Monahrq.Websites.ViewModels
{
    [Export(typeof(WebsiteReportsViewModel))]
    public class WebsiteReportsViewModel : WebsiteTabViewModel
    {
        #region Fields and Constants

        const string ALL_CATEGORIES = "All Report Categories";
        private AudienceModel _selectedAudience;
        private Visibility _isPreviewOpen;
        private bool _isOpen;
        private MultiSelectCollectionView<ReportViewModel> _reports;
        private string _selectedCategory;
        private ObservableCollection<string> _categories;
        private string _filterText;
        private KeyValuePair<FilterKeys, string> _selectedFilter;
        private IList<KeyValuePair<FilterKeys, string>> _filter;
        private string _title;
        private string _reportType;
        private ObservableCollection<string> _reportTypes;
        private ObservableCollection<AudienceModel> _audiences;
        private int _selectedReportsCount;
        public bool IsRefreshed;
        private readonly Dictionary<Audience, string> audiences = new Dictionary<Audience, string>()


        {
            { Audience.Consumers,  Audience.Consumers.GetDescription() },
            { Audience.Professionals, Audience.Professionals.GetDescription()}
        };

        private ReportViewModel _selectedReport;

        #endregion

        #region Commands

        public DelegateCommand<ReportViewModel> NavigateToDetailsCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand<ReportViewModel> PreviewCommand { get; set; }
        public DelegateCommand<ReportViewModel> OpenTrendingCommand { get; set; }
        public DelegateCommand ClosePreviewCommand { get; set; }
        public DelegateCommand CloseTrendingCommand { get; set; }
        
        #endregion

        #region Constructor

        public WebsiteReportsViewModel()
        {
            Title = String.Empty; //New report Title
            //ShowHospitalSelectionView = Visibility.Collapsed;
            //IsPreviewOpen = Visibility.Collapsed;
            ReportsCollectionView = new MultiSelectCollectionView<ReportViewModel>(new List<ReportViewModel>());

        }

        #endregion

        #region Services
        //[Import]
        //public IWebsiteDataService WebsiteDataService { get; set; }
        #endregion

        #region Properties

        public Visibility IsPreviewOpen
        {
            get { return _isPreviewOpen; }
            set
            {
                _isPreviewOpen = value;
                RaisePropertyChanged(() => IsPreviewOpen);
            }
        }

        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
            set
            {
                _isOpen = value;
                RaisePropertyChanged(() => IsOpen);
            }
        }

        public bool IsTrendingOpened { get; set; }
        //public Visibility ShowHospitalSelectionView
        //{
        //    get; set;
        //}

        public ReportViewModel SelectedReport
        {
            get { return _selectedReport; }
            set
            {
                _selectedReport = value;
                if (_selectedReport == null) return;

                ActivePreviewImageTabIndex = _selectedReport.Report.HasProfessionalsAudience ? 0 :
                    _selectedReport.Report.HasConsumersAudience ? 1 : 0;

            }
        }

        public int ActivePreviewImageTabIndex { get; set; }

        private bool _newDataSetsAdded;

        public ObservableCollection<ReportViewModel> SelectedReports
        {
            get
            {
                var newVal = ReportsCollectionView.SelectedItems.ToObservableCollection();
                SelectedReportsCount = newVal.Count;
                return newVal;
            }
        }

        //[PropertyChanged.DoNotSetChanged]
        public MultiSelectCollectionView<ReportViewModel> ReportsCollectionView
        {
            get; set;
            //get { return _reports; }
            //set
            //{
            //    _reports = value;
            //    RaisePropertyChanged(() => ReportsCollectionView);
            //}
        }

        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;

                //// If it's not the All category, filter the list we get back.
                //// We don't have to check for null report list when constructing the ObservableCollection because default reports always exist
                //if (_selectedCategory != ALL_CATEGORIES)
                //{
                //    LoadDataAndGetWebsiteSelectedReports(_selectedCategory);
                //    //var reports = (from r in WebsiteDataService.GetReports() where r.Category == _selectedCategory select r).ToObservableCollection();
                //    //ReportsCollectionView = CollectionViewSource.GetDefaultView(reports) as ListCollectionView;
                //}
                //else
                //{
                //    LoadDataAndGetWebsiteSelectedReports();
                //    //var reports = WebsiteDataService.GetReports().ToObservableCollection();
                //    //ReportsCollectionView = CollectionViewSource.GetDefaultView(reports) as ListCollectionView;
                //}
                if (ReportsCollectionView == null) return;

                ReportsCollectionView.Filter = null;
                ReportsCollectionView.Filter = CompositeFilter;

                // select 1st report by default
                //SelectedReport = ReportsCollectionView.CurrentItem as ReportViewModel;

                RaisePropertyChanged(() => SelectedCategory);
            }
        }

        public ObservableCollection<string> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                RaisePropertyChanged(() => Categories);
            }
        }

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                if (ReportsCollectionView != null)
                {
                    ReportsCollectionView.Filter = null;
                    ReportsCollectionView.Filter = CompositeFilter;
                    //ReportsCollectionView.Refresh();
                }
                RaisePropertyChanged(() => FilterText);
            }
        }

        public int SelectedReportsCount
        {
            get { return _selectedReportsCount; }
            set
            {
                _selectedReportsCount = value;
                RaisePropertyChanged(() => SelectedReportsCount);
            }
        }

        public KeyValuePair<FilterKeys, string> SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                _selectedFilter = value;
                if (ReportsCollectionView != null)
                {
                    ReportsCollectionView.Filter = null;
                    ReportsCollectionView.Filter = CompositeFilter;
                    //ReportsCollectionView.Refresh();
                }
                RaisePropertyChanged(() => SelectedFilter);
            }
        }

        public IList<KeyValuePair<FilterKeys, string>> Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                RaisePropertyChanged(() => Filter);
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        public string ReportType
        {
            get { return _reportType; }
            set
            {
                _reportType = value;
                RaiseErrorsChanged(() => ReportType);
            }
        }

        public ObservableCollection<string> ReportTypes
        {
            get { return _reportTypes; }
            set
            {
                _reportTypes = value;
                RaisePropertyChanged(() => ReportTypes);
            }
        }

        public ObservableCollection<AudienceModel> Audiences
        {
            get { return _audiences; }
            set
            {
                _audiences = value;
                RaisePropertyChanged(() => Audiences);
            }
        }

        public AudienceModel SelectedAudience
        {
            get { return _selectedAudience; }
            set
            {
                _selectedAudience = value;
                RaisePropertyChanged(() => SelectedAudience);
            }
        }

        public bool IsAllSelected
        {
            get
            {
                return ReportsCollectionView != null && ReportsCollectionView.SelectedItems != null &&
                       ReportsCollectionView.SelectedItems.DistinctBy(r => r.Type).Count() ==
                       ReportsCollectionView.OfType<ReportViewModel>().DistinctBy(r => r.Type).Count();
            }
            set
            {
                if (value)
                {
                    var reports = ReportsCollectionView.OfType<ReportViewModel>().ToList();
                    foreach (var report in from report in reports.Where(r => !r.IsSelected)
                                           let reportTypes = SelectedReports.Select(s => s.Type).Distinct().ToList()
                                           where !reportTypes.Contains(report.Type) && report.IsDefault
                                           select report)
                    {
                        if(CurrentWebsite.Audiences.Count(audience => report.AudienceForDisplay.ContainsIgnoreCase(audience.ToString())) > 0)
                            report.IsSelected = true;
                    }
                }
                else
                {
                    var reports = ReportsCollectionView.OfType<ReportViewModel>().ToList();
                    reports.ForEach(r => r.IsSelected = false);
                }
            }
        }

        private string _headerText;
        public string HeaderText
        {
            get
            {
                _headerText = "Select Reports";
                return _headerText;
                    }
            set
            {
                _headerText = value;
            }
        }

        #endregion

        #region Methods

        protected override void InitCommands()
        {
            base.InitCommands();

            NavigateToDetailsCommand = new DelegateCommand<ReportViewModel>(OnNavigateToDetails);
            CancelCommand = new DelegateCommand(OnCancel, CanCancel);
            PreviewCommand = new DelegateCommand<ReportViewModel>(OnPreview);
            OpenTrendingCommand = new DelegateCommand<ReportViewModel>(OnOpenTrending);
            ClosePreviewCommand = new DelegateCommand(OnClosePreview);
            CloseTrendingCommand = new DelegateCommand(OnCloseTrending); 
        }  

        private void OnCloseTrending()
        {
            IsTrendingOpened = false;
        }

        private void OnOpenTrending(ReportViewModel obj)
        {
            SelectedReport = obj;
            IsTrendingOpened = true;
        }

        protected override void InitProperties()
        {
            Categories = new ObservableCollection<string>(WebsiteDataService.Categories);
            Categories.Insert(0, ALL_CATEGORIES);
            // select 1st category by default and this will fire the event to load reports with a filter
            SelectedCategory = Categories[0];

            Filter = WebsiteDataService.FilterEnumerations;
            FilterText = string.Empty;
            SelectedFilter = Filter.FirstOrDefault();

            ReportTypes = new ObservableCollection<string>(WebsiteDataService.ReportTypes);
            ReportType = ReportTypes[0];

            Audiences = new ObservableCollection<AudienceModel>(WebsiteDataService.Audiences);
            SelectedAudience = Audiences[0];

            IsPreviewOpen = Visibility.Hidden;
            IsOpen = false;
            IsTrendingOpened = false;
        }

        public void LoadData()
        {
            //FilterReportsByAudience(ref reports, WebsiteViewModel.Website.Audience);
            var newReportList = new List<ReportViewModel>();
            var reportsToFilter = new List<ReportViewModel>(ManageViewModel.AllAvailableReports);

            if (CurrentWebsite != null && CurrentWebsite.Datasets.Any())
            {

                foreach (var dataset in CurrentWebsite.Datasets)
                {
                    var datasetTypeName = dataset.Dataset.ContentType.Name;
                    foreach (var reportModel in reportsToFilter)
                    {
                        if (!datasetTypeName.In(reportModel.Datasets)) continue;

                        var datasets = CurrentWebsite.Datasets
                            .Where(x => x.Dataset.ContentType.Name == datasetTypeName)
                            .DistinctBy(x => x.Dataset.ReportingYear).Select(x => Tuple.Create(x.Dataset.ReportingYear, x.Dataset.ContentType.Name))
                            .ToList();

                        reportModel.IsTrendingEnabled = reportModel.IsTrendingReport && datasets.Count > 1;

                        if (reportModel.IsTrendingEnabled && CurrentWebsite.Reports != null
                            && CurrentWebsite.Reports.All(x => x.Id != reportModel.Id) && CurrentWebsite.Reports.Count > 0)
                        {
                            reportModel.IsSelected = reportModel.IsDefault;
                        }


                        //if (reportModel.IsTrendingEnabled)
                        //{
                        //    var defaultYear = datasets.Max(x => x.Item1);
                        //    reportModel.Years = datasets.Select(x =>
                        //        new TrendingYear()
                        //        {
                        //            Year = x.Item1,
                        //            IsDefault = x.Item1 == defaultYear,
                        //            IsSelected = true,
                        //            Quarters = GetTrendingQuarters(reportModel.Datasets, quarters)
                        //        }).OrderBy(x => x.Year).ToObservableCollection();
                        //    var defaultTrendingYear = reportModel.Years.LastOrDefault(x => x.IsDefault);
                        //    reportModel.SelectedDefaultYear = defaultTrendingYear != null ? defaultTrendingYear.Year : string.Empty;
                        //}

                        if (newReportList.Any(r => r.Id == reportModel.Id)) continue;

                        if (!reportModel.IsTrendingReport || (reportModel.IsTrendingEnabled && reportModel.IsTrendingEnabled)) newReportList.Add(reportModel);
                    }
                }
            }

            ReportsCollectionView = reportsToFilter.Where(x => x.IsSelected).ToMultiSelectListCollectionView();
            if (ReportsCollectionView == null || ReportsCollectionView.Count == 0) return;

            //ReportsCollectionView.MoveCurrentToFirst();
            //SelectedReport = ReportsCollectionView.CurrentItem as ReportViewModel;
        }

        public void LoadDataAndGetWebsiteSelectedReports(string selectedCategory = null)
        {
            // First filter reports by selected Datasets, category when applicable
            LoadData();
            // Now set default selection by audience type if selected already.
            if (CurrentWebsite != null)
            {
                if (CurrentWebsite.Reports != null && CurrentWebsite.Reports.Any())
                {
                    foreach (var websiteReport in CurrentWebsite.Reports.ToList())
                    {
                        var selectedReport = ReportsCollectionView.OfType<ReportViewModel>().SingleOrDefault(m => m.Report.Id == websiteReport.Report.Id);

                        if (selectedReport == null) continue;

                        selectedReport.WebsitesForDisplay = websiteReport.AssociatedWebsites;
                        //selectedReport.Websites = !string.IsNullOrEmpty(websiteReport.AssociatedWebsites)
                        //    ? websiteReport.AssociatedWebsites.Split(',').ToObservableCollection()
                        //    : new ObservableCollection<string>();
                        selectedReport.IsSelected = true;

                        if (selectedReport.Years != null)
                        {
                            var defaultOrYear = selectedReport.Years.FirstOrDefault(x => x.Year == websiteReport.DefaultSelectedYear);
                            selectedReport.SelectedDefaultYear = defaultOrYear != null ? defaultOrYear.Year : null;

                            selectedReport.Years.ForEach(x => x.IsSelected = false);
                            websiteReport.SelectedYears.ForEach(year =>
                            {
                                var temp = selectedReport.Years.FirstOrDefault(r => r.Year == year.Year);
                                if (temp != null) temp.IsSelected = true;
                            });
                        }
                    }

                    //foreach(var unselectedReport in ReportsCollectionView.OfType<ReportViewModel>().Where(m => !m.IsSelected).ToList())
                    //{
                    //    var audienceArgs = new List<string>();
                    //    unselectedReport.Report.Audiences.ForEach(a => audienceArgs.Add(a.ToString()));

                    //    if (!unselectedReport.IsDefault) continue;

                    //    audienceArgs.ToList()
                    //                .ForEach(audience => unselectedReport.IsSelected = CurrentWebsite.Audiences.Any(a => a.ToString().EqualsIgnoreCase(audience)));

                    //}

                    //       IsAllSelected = ReportsCollectionView.OfType<ReportViewModel>().Count() == CurrentWebsite.Reports.Count;
                }
                else
                {
                    //IsAllSelected = true;
                    foreach (var reportModel in ReportsCollectionView.OfType<ReportViewModel>().ToList())
                    {
                        var audienceArgs = new List<string>();
                        reportModel.Report.Audiences.ForEach(a => audienceArgs.Add(a.ToString()));

                        if (!reportModel.IsDefault) continue;

                        foreach (var audience in audienceArgs)
                        {
                            reportModel.IsSelected = CurrentWebsite.Audiences.Any(a => a.ToString().EqualsIgnoreCase(audience));
                        }
                    }
                }
            }
            else
            {
                // IsAllSelected = true;
                foreach (var reportModel in ReportsCollectionView.OfType<ReportViewModel>().ToList())
                {
                    reportModel.IsSelected = reportModel.IsDefault;
                }
            }

            ReportsCollectionView.Filter = CompositeFilter;
            //IsAllSelected = IsAllSelectedCheck();
            RaisePropertyChanged(() => IsAllSelected);
        }

        public override void Save()
        {
            UpdateVmWithSelectedReports();

            string message;
            bool errorsOccurredWhenSaving;
            if (!ManageViewModel.WebsiteViewModel.Website.IsPersisted)
            {
                errorsOccurredWhenSaving = WebsiteDataService.SaveNewWebsite(ManageViewModel.WebsiteViewModel.Website);
                message = string.Format("Website {0} has been added", ManageViewModel.WebsiteViewModel.Website.Name);
            }
            else
            {
                // If the website is edited, the current status must change in order to allow for the dependency check to be readily available when publishing
                ManageViewModel.WebsiteViewModel.Website.CurrentStatus = WebsiteState.Initialized;

                errorsOccurredWhenSaving = WebsiteDataService.UpdateWebsite(ManageViewModel.WebsiteViewModel.Website);
                message = string.Format("Website {0} has been updated", ManageViewModel.WebsiteViewModel.Website.Name);
            }
            // If no errors, move to the next tab
            if (!errorsOccurredWhenSaving)
            {
                RefreshUIElements();
                var eventArgs = new ExtendedEventArgs<GenericWebsiteEventArgs>
                {
                    Data = new GenericWebsiteEventArgs { Website = ManageViewModel.WebsiteViewModel, Message = message }
                };

                EventAggregator.GetEvent<WebsiteCreatedOrUpdatedEvent>().Publish(eventArgs);
                //EventAggregator.GetEvent<UpdateWebsiteTabContextEvent>().Publish(
                //    new UpdateTabContextEventArgs
                //    {
                //        WebsiteViewModel = base.ManageViewModel.WebsiteViewModel,
                //        ExecuteViewModel = WebsiteTabViewModels.Reports
                //    });
            }

        }

        private void UpdateVmWithSelectedReports()
        {
            if (ReportsCollectionView == null || ReportsCollectionView.IsEmpty) return;

            CurrentWebsite.Reports.Clear();
            var selectedReports = ReportsCollectionView.SourceCollection.OfType<ReportViewModel>().Where(x => x.IsSelected).ToList();
            //var selectedReports = ReportsCollectionView.SelectedItems.ToList();

            foreach (var reportViewModel in selectedReports)
            {
                var wr = new WebsiteReport
                {
                    Report = reportViewModel.Report,
                    IsQuarterlyTrendingEnabled = reportViewModel.IsQuarterlyTrendingEnabled,
                    SelectedYears = reportViewModel.Years != null ? reportViewModel.Years.ToList() : null,
                    DefaultSelectedYear = reportViewModel.SelectedDefaultYear,
                };

                if (CurrentWebsite.Reports.All(o => o.Report.SourceTemplate.RptId != wr.Report.SourceTemplate.RptId))
                    CurrentWebsite.Reports.Add(wr);
            }

            CurrentWebsite.Reports = CurrentWebsite.Reports.DistinctBy(rpt => rpt.Report.ReportType).ToList();

            CurrentWebsite.ActivityLog.Add(CurrentWebsite.Reports.Any() ? new ActivityLogEntry("Website reports added and/or modified.", DateTime.Now)
                : new ActivityLogEntry("Website reports removed or deleted.", DateTime.Now));

            RaisePropertyChanged(() => CurrentWebsite.Reports);
            RaisePropertyChanged(() => ManageViewModel.WebsiteViewModel.Website.Reports);
        }

        public override void Continue()
        {
            UpdateVmWithSelectedReports();

            SelectedFilter = Filter.FirstOrDefault();
            FilterText = string.Empty;
            SelectedCategory = Categories.FirstOrDefault();
        }

        public override void RefreshUIElements()
        {
            base.RefreshUIElements();

            if (SelectedReports == null || !SelectedReports.Any()) return;

            foreach (var report in SelectedReports) // Refresh associated websites
            {
                if (report.Websites == null) report.Websites = new ObservableCollection<string>();

                report.Websites = WebsiteDataService.GetWebsiteNamesForReport(report.Report.Id).ToObservableCollection();
            }
        }

        bool CompositeFilter(object reportdisp)
        {
            var report = reportdisp as ReportViewModel;

            if (report == null || (string.IsNullOrEmpty(FilterText) && SelectedCategory == ALL_CATEGORIES)) return true;

            bool fail = SelectedCategory != ALL_CATEGORIES;

            if (fail)
            {
                fail = report.Category != SelectedCategory && TestAudience(report);
                if (fail)
                    return false;
            }

            switch (SelectedFilter.Key)
            {
                case FilterKeys.None:
                    return TestAudience(report);
                case FilterKeys.ReportName:
                    return TestName(report);
                case FilterKeys.ReportType:
                    return TestType(report);
                case FilterKeys.Website:
                    return TestWebsite(report);
                //case FilterKeys.RecommendedAudiences:
                default:
                    return true;
            }

            //bool pass = SelectedFilter.Key == FilterKeys.None;
            //if (!pass)
            //{
            //    pass = SelectedFilter.Key == FilterKeys.RecommendedAudiences && TestAudience(report);
            //}
            //if (!pass)
            //{
            //    pass = SelectedFilter.Key == FilterKeys.ReportName && TestName(report);
            //}
            //if (!pass)
            //{
            //    pass = SelectedFilter.Key == FilterKeys.ReportType && TestType(report);
            //}
            //if (!pass)
            //{
            //    pass = SelectedFilter.Key == FilterKeys.Website && TestWebsite(report);
            //}
            //return pass;

        }

        private bool TestWebsite(ReportViewModel vm)
        {
            return ArrayTest(vm.Websites.Select(s => s));
        }

        private bool TestType(ReportViewModel vm)
        {
            return StringTest(vm.Type);
        }

        private bool TestName(ReportViewModel vm)
        {
            return StringTest(vm.Name);
        }

        private bool TestAudience(ReportViewModel vm)
        {

            if (vm.Report.Audiences != null && vm.Report.Audiences.FirstOrDefault() == Audience.None &&
                FilterText != string.Empty)
                return false;

            return vm.Report.Audiences != null && vm.Report.Audiences.Any(audience => StringTest(audience.GetDescription()));
        }

        private bool ArrayTest(IEnumerable<string> arry)
        {
            var itemsToTest = arry.ToList();
            if (!itemsToTest.Any()) return false;

            var result = StringTest(itemsToTest.First());

            if (!result && itemsToTest.Count == 1) return false;

            return result || ArrayTest(itemsToTest.Skip(1));
        }

        private bool StringTest(string s)
        {
            return s.ToLower().Contains(FilterText.ToLower());
        }

        public override void Refresh()
        {
            base.Refresh();
            IsTabVisited = true;

            LoadWebsiteReports();
        }

        private void LoadWebsiteReports()
        {
            IsRefreshed = false;
            var newReportList = new List<ReportViewModel>();
            if (CurrentWebsite != null)
            {
                if (ReportsCollectionView != null)
                    ReportsCollectionView.OfType<ReportViewModel>().ToList().ForEach(rpt =>
                    {
                        rpt.IsSelected = false;
                    });
                var quarters = GetAvailableQuarters().ToList();

                foreach (var dataset in CurrentWebsite.Datasets)
                {
                    var datasetTypeName = dataset.Dataset.ContentType.Name;
                    var datasets = CurrentWebsite.Datasets
                        .Where(x => x.Dataset.ContentType.Name == datasetTypeName)
                        //.DistinctBy(x => x.Dataset.ReportingYear)
                        .Select(x => Tuple.Create(x.Dataset.ReportingYear, x.Dataset.ContentType.Name))
                        .ToList();

                    foreach (var reportModel in ManageViewModel.AllAvailableReports)
                    {
                        if (!datasetTypeName.In(reportModel.Datasets)) continue;

                        reportModel.IsTrendingEnabled = reportModel.IsTrendingReport && (quarters.Count(q => reportModel.Datasets.Contains(q.Name)) > 1
                            || (datasets != null && datasets.DistinctBy(x => x.Item1).Count() > 1));

                        reportModel.IsValueChanged -= reportModel_IsValueChanged;
                        var websiteReport = CurrentWebsite.IsPersisted && CurrentWebsite.Reports != null ? CurrentWebsite.Reports.FirstOrDefault(x => x.Report.Id == reportModel.Report.Id) : null;
                        if (websiteReport != null)
                        {
                            reportModel.WebsitesForDisplay = websiteReport.AssociatedWebsites;
                            reportModel.IsQuarterlyTrendingEnabled = websiteReport.IsQuarterlyTrendingEnabled;
                            // reportModel.IsSelected = CurrentWebsite.Reports != null && CurrentWebsite.Reports.Any(x => x.Report.Id == reportModel.Report.Id);
                        }

                        reportModel.WebsiteAudiences = CurrentWebsite.Audiences.ToList();
                        //if (websiteReport == null && CurrentWebsite.Reports != null && _newDataSetsAdded)
                        //{
                        reportModel.IsSelected = (CurrentWebsite.Reports != null && CurrentWebsite.Reports.Any(x => x.Report.Id == reportModel.Report.Id)) ||
                                                  CurrentWebsite.Reports != null && !CurrentWebsite.Reports.Any() && reportModel.IsDefault && reportModel.Report.Audiences.Any(a => a.In(CurrentWebsite.Audiences));
                        //}

                        reportModel.IsValueChanged += reportModel_IsValueChanged;


                        if (reportModel.IsTrendingEnabled)
                        {
                            var period = GetTrendingYears(datasets, reportModel, quarters);
                            reportModel.Years = period.Item1;
                            reportModel.SelectedDefaultYear = period.Item2;
                            var minYear = reportModel.Years.Min(x => x.Year);
                            var expandedYear = reportModel.Years.FirstOrDefault(x => x.Year == minYear);
                            if (expandedYear != null) expandedYear.IsExpanded = true;
                        }

                        if (newReportList.Contains(reportModel)) continue;

                        if (!reportModel.IsTrendingReport || (reportModel.IsTrendingReport && reportModel.IsTrendingEnabled))
                        {
                            newReportList.Add(reportModel);
                        }
                    }
                }
            }

            ReportsCollectionView = newReportList.ToMultiSelectListCollectionView();
            if (ReportsCollectionView == null || ReportsCollectionView.Count == 0) return;

            OnClosePreview();
            OnCloseTrending();
            // ReportsCollectionView.MoveCurrentToFirst();
            // SelectedReport = ReportsCollectionView.CurrentItem as ReportViewModel;
            SelectedReportsCount = ReportsCollectionView.SelectedItems.Count;
            IsRefreshed = true;
        }

        private Tuple<ObservableCollection<TrendingYear>, string> GetTrendingYears(List<Tuple<string, string>> datasetYears, ReportViewModel reportModel, IEnumerable<Quarters> quarters)
        {
            if (CurrentWebsite.IsPersisted && CurrentWebsite.Reports != null && CurrentWebsite.Reports.Any())
            {
                var savedReport = CurrentWebsite.Reports.FirstOrDefault(x => x.Report.Name == reportModel.Name);

                if (savedReport != null && !ManageViewModel.IsTrendingYearUpdated)
                {
                    if (savedReport.SelectedYears.All(y => y.Year.In(datasetYears)))
                        return Tuple.Create(savedReport.SelectedYears.ToObservableCollection(), savedReport.DefaultSelectedYear);
                    else
                        return GetDefaultTrendingYears(datasetYears, reportModel, quarters);
                }
                else
                {
                    return GetDefaultTrendingYears(datasetYears, reportModel, quarters);
                }
            }

            return GetDefaultTrendingYears(datasetYears, reportModel, quarters);
        }

        private Tuple<ObservableCollection<TrendingYear>, string> GetDefaultTrendingYears(List<Tuple<string, string>> dasetYears, ReportViewModel reportModel, IEnumerable<Quarters> quarters)
        {
            var defaultYear = dasetYears.Max(x => x.Item1);
            var trendingYears = Tuple.Create(dasetYears.DistinctBy(y => y.Item1).Select(x => new TrendingYear
            {
                Year = x.Item1,
                IsDefault = x.Item1 == defaultYear,
               
                Quarters = quarters.Where(q => reportModel.Datasets.Contains(q.Name)).Select(quarter => new Period
                {
                    Text = quarter.Text,
                    IsSelected = true,
                }).ToList(),
                IsSelected = true
            }).OrderBy(x => x.Year).ToObservableCollection(), defaultYear);


            if(reportModel.Years != null && reportModel.Years.Any())
            {
                foreach (var tYear in trendingYears.Item1)
                {
                    var existingYear = reportModel.Years.FirstOrDefault(y => y.Year == tYear.Year);
                    if (existingYear != null)
                    {
                        tYear.Year = existingYear.Year;
                        tYear.Quarters = existingYear.Quarters;
                        tYear.IsSelected = existingYear.IsSelected;
                        tYear.IsExpanded = existingYear.IsExpanded;
                        tYear.IsDefault = existingYear.IsDefault;
                    }
                }
            }


            return trendingYears;
        }

        void reportModel_IsValueChanged(object sender, EventArgs e)
        {
            SelectedReportsCount = ReportsCollectionView.SelectedItems.Count;
            if (IsRefreshed)
            {
                RaisePropertyChanged(() => SelectedReports);
                RaisePropertyChanged(() => IsAllSelected);
            }
        }

        private void OnClosePreview()
        {
            IsPreviewOpen = Visibility.Collapsed;
            IsOpen = false;
        }

        private void OnPreview(ReportViewModel report)
        {
            IsPreviewOpen = Visibility.Visible;
            IsOpen = true;
        }

        //private void OnAddReport()
        //{
        //    IsAddReportOpen = Visibility.Visible;
        //}

        private bool CanCancel()
        {
            return true; //!Committed;
        }

        public override void OnPreSave()
        {
            if (!CurrentWebsite.IsPersisted)
                TabChanged();
        }

        protected override void OnCancel()
        {
            base.OnCancel();
            //ShowHospitalSelectionView = Visibility.Collapsed;
            Reset();
        }

        private void OnNavigateToDetails(ReportViewModel report)
        {
            WaitCursor.Show();
            if (report == null) return;
            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.ReportDetailsView, UriKind.Relative));
        }

        public override void Reset()
        {
            base.Reset();

            Title = string.Empty;

            // BUG: audienceModel is write-only here
            foreach (var audienceModel in Audiences)
            {
                audienceModel.IsSelected = false;
            }

            SelectedReports.Clear();
            SelectedReport = null;
        }

        #region Unused Code. May be deleted after beta 2 release. Jason
        //protected override void OnCommitted()
        //{
        //    if (!(Audiences[1].IsSelected || Audiences[2].IsSelected)
        //        || string.IsNullOrWhiteSpace(Title)
        //        || string.IsNullOrWhiteSpace(ReportType))
        //    {
        //        MessageBox.Show("Please enter a report type, report name, and recommended audience(s) to add a new report.");
        //        return;
        //    }

        //    var newreport = WebsiteReportDataService.SaveNewReport(Title, Audiences, ReportType);
        //    if (newreport != null)
        //    {
        //        var vm = new ReportViewModel(newreport);
        //        ReportsCollectionView.AddNewItem(vm);
        //        SelectedReport = vm;
        //        var msg = String.Format("New report {0} has been added", vm.Name);
        //        EventAggregator.GetEvent<GenericNotificationEvent>().Publish(msg);
        //    }
        //    else
        //    {
        //        MessageBox.Show("Error creating new report");
        //    }

        //    _reset();
        //}

        //private void OnNewContextReport(ReportViewModel reportViewModel)
        //{
        //    var report = new Report(SelectedReport.Report.SourceTemplate);
        //    report.Name = "New report based on " + report.Name;
        //    report.IsDefaultReport = false;

        //    if (WebsiteReportDataService.SaveReport(report))
        //    {
        //        var vm = new ReportViewModel(report);
        //        ReportsCollectionView.AddNewItem(vm);
        //        SelectedReport = vm;
        //        OnNavigateToDetails(vm);
        //    }
        //}
        #endregion

        private void OnReportDeleted(ReportViewModel reportViewModel)
        {
            var name = reportViewModel.Name;
            ReportsCollectionView.Remove(reportViewModel);

            var msg = String.Format("Report {0} has been deleted", name);
            EventAggregator.GetEvent<GenericNotificationEvent>().Publish(msg);
        }

        private void OnNewReportCreated(ReportViewModel vm)
        {
            ReportsCollectionView.AddNewItem(vm);
            SelectedReport = vm;
        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            Index = 3;

            EventAggregator.GetEvent<ReportDeletedEvent>().Subscribe(OnReportDeleted);
            EventAggregator.GetEvent<NewReportCreatedEvent>().Subscribe(OnNewReportCreated);
            //EventAggregator.GetEvent<SelectedItemsForNewlyAddedDatasets>().Subscribe(arg =>
            //{
            //    _newDataSetsAdded = arg;
            //});

        }

        public override bool TabChanged()
        {
            UpdateVmWithSelectedReports();
            return true;
        }

        private IEnumerable<Quarters> GetAvailableQuarters()
        {
            var ipDatasetIds = CurrentWebsite.Datasets.Where(x => x.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge")).Select(x => x.Dataset.Id);
            var edDatasetIds = CurrentWebsite.Datasets.Where(x => x.Dataset.ContentType.Name.EqualsIgnoreCase("ED Treat And Release")).Select(x => x.Dataset.Id);

            var query = string.Format(@";WITH AllQuarters (Quart, Name)
                           AS (
                           SELECT DISTINCT DischargeQuarter, wt.Name
                           FROM Targets_InpatientTargets ip	
                           	inner join Wings_Datasets wd ON wd.Id = ip.Dataset_Id
                           	inner join Wings_Targets wt ON wd.ContentType_Id = wt.Id
                           WHERE DischargeQuarter is not null  {0}
                           
                           ---- UNCOMMENT THE FOLLOWING CODE ONCE DQTR COLUMN HAS BEEN ADDED TO ED TARGET TABLE
                           UNION ALL
                           
                           SELECT DISTINCT DischargeQuarter, wt.Name
                           FROM Targets_TreatAndReleaseTargets ed
                           	inner join Wings_Datasets wd ON wd.Id = ed.Dataset_Id
                           	inner join Wings_Targets wt ON wd.ContentType_Id = wt.Id
                           WHERE DischargeQuarter is not null {1}
                           )
                           
                           SELECT 'Quarter '+ CAST(Quart AS NVARCHAR) as Text, Name
                           FROM AllQuarters", ipDatasetIds.Any() ? " and Dataset_Id in (" + string.Join(",", ipDatasetIds) + ")" : "",
                           edDatasetIds.Any() ? " and Dataset_Id in (" + string.Join(",", edDatasetIds) + ")" : "");

            using (var session = DataserviceProvider.SessionFactory.OpenStatelessSession())
            {
                return session.CreateSQLQuery(query)
                    .AddScalar("Text", NHibernateUtil.String)
                    .AddScalar("Name", NHibernateUtil.String)
                    .SetResultTransformer(new AliasToBeanResultTransformer(typeof(Quarters)))
                    .List().Cast<Quarters>().ToList();
            }
        }

        struct Quarters
        {
            public string Text;
            public string Name;
        }

        #endregion

    }

}