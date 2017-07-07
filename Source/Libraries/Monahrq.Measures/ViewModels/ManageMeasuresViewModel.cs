using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Domain.Websites.Maps;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.ViewModels;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Mapping;
using NHibernate.Transform;
using PropertyChanged;
using Monahrq.Infrastructure.Types;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Monahrq.Measures.ViewModels
{
    /// <summary>
    /// View model class for manage measures
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.ViewModels.ListTabViewModel{Monahrq.Infrastructure.Entities.Domain.Measures.Measure}" />
    [ImplementPropertyChanged]
    [Export(typeof(ManageMeasuresViewModel))]
    public class ManageMeasuresViewModel : ListTabViewModel<Measure>
    {
        #region Fields and Constants

        /// <summary>
        /// The label text for manage measures
        /// </summary>
        private const string MANAGE_MEASURES = "Manage Measures";
        /// <summary>
        /// The label text for  All datasets
        /// </summary>
        private const string ALL_DATASETS = "All Datasets";
        /// <summary>
        /// The default value for spliter width on
        /// </summary>
        public const string SPLITER_WIDTH_ON = "7";
        /// <summary>
        /// The default value for detail view width on
        /// </summary>
        public const string DETAIL_VIEW_WIDTH_ON = "0.3*";
        /// <summary>
        /// The default value for data view width on
        /// </summary>
        public const string DATA_VIEW_WIDTH_ON = "0.7*";
        /// <summary>
        /// The default value for zero width
        /// </summary>
        public const string ZERO_WIDTH = "0";
        /// <summary>
        /// The property filter text
        /// </summary>
        private string _propertyFilterText;
        /// <summary>
        /// The selected property
        /// </summary>
        private string _selectedProperty;
        /// <summary>
        /// The selected measure name type
        /// </summary>
        private string _selectedMeasureNameType;
        /// <summary>
        /// The selected data set
        /// </summary>
        private string _selectedDataSet;
        //private bool _isInitialLoad;
        /// <summary>
        /// The selected measure
        /// </summary>
        private Measure _selectedMeasure;
        /// <summary>
        /// The sorting columns
        /// </summary>
        private PagingSortingColumns _sortingColumns;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the selected data set.
        /// </summary>
        /// <value>
        /// The selected data set.
        /// </value>
        public string SelectedDataSet
        {
            get { return _selectedDataSet; }
            set
            {
                if (_selectedDataSet == value) return;
                _selectedDataSet = value;
                if (CollectionItems == null) return;

                if (IsLoaded) GetPage();
            }
        }

        /// <summary>
        /// Gets or sets the selected property.
        /// </summary>
        /// <value>
        /// The selected property.
        /// </value>
        public string SelectedProperty
        {
            get { return _selectedProperty; }
            set
            {
                if (_selectedProperty == value) return;
                _selectedProperty = value;

                if (IsLoaded)
                    GetPage();
            }
        }

        /// <summary>
        /// Gets or sets the property filter text.
        /// </summary>
        /// <value>
        /// The property filter text.
        /// </value>
        public string PropertyFilterText
        {
            get { return _propertyFilterText; }
            set
            {
                if (_propertyFilterText == value) return;
                _propertyFilterText = value;

                if (IsLoaded)
                    GetPage();
            }
        }

        /// <summary>
        /// Gets or sets the property filters.
        /// </summary>
        /// <value>
        /// The property filters.
        /// </value>
        public ObservableCollection<string> PropertyFilters { get; set; }

        /// <summary>
        /// Gets or sets the type of the selected measure name.
        /// </summary>
        /// <value>
        /// The type of the selected measure name.
        /// </value>
        public string SelectedMeasureNameType
        {
            get { return _selectedMeasureNameType; }
            set
            {
                if (_selectedMeasureNameType == value) return;
                _selectedMeasureNameType = value;
                RaisePropertyChanged(() => MeasureNameColumnSortPath);
            }
        }

        /// <summary>
        /// Gets the measure name column sort path.
        /// </summary>
        /// <value>
        /// The measure name column sort path.
        /// </value>
        public string MeasureNameColumnSortPath
        {
            get
            {
                switch (SelectedMeasureNameType)
                {
                    case "Plain Title": return "MeasureTitle.Plain";
                    case "Clinical Title": return "MeasureTitle.Clinical";
                    default: return "MeasureTitle.Plain";
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected measure.
        /// </summary>
        /// <value>
        /// The selected measure.
        /// </value>
        public Measure SelectedMeasure
        {
            get
            {
                return _selectedMeasure;  //MeasuresCollectionView.CurrentItem as MeasureModel;
            }
            set
            {
                _selectedMeasure = value;
                CollectionItems.MoveCurrentTo(value);
                RaisePropertyChanged();
            }
        }

        //public bool IsAllSelected
        //{
        //    get { return MeasuresCollectionView.OfType<MeasureModel>().All(x => x.IsSelectedForTopicAssignment); }

        //    set
        //    {
        //        foreach (var m in MeasuresCollectionView.OfType<MeasureModel>())
        //            m.IsSelectedForTopicAssignment = value;
        //    }
        //}

        /// <summary>
        /// Gets or sets a value indicating whether this instance is topics edit open.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is topics edit open; otherwise, <c>false</c>.
        /// </value>
        public bool IsTopicsEditOpen { get; set; }

        private string _headerText;
        /// <summary>
        /// Gets or sets the header text.
        /// </summary>
        /// <value>
        /// The header text.
        /// </value>
            public string HeaderText
        {
            get
            {
                _headerText = HeaderName + " (" + TotalCount + ")";
                return _headerText;
            }
            set
            {
                _headerText = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is navigated from manage measures details view.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is navigated from manage measures details view; otherwise, <c>false</c>.
        /// </value>
        public bool IsNavigatedFromManageMeasuresDetailsView { get; set; }


        /// <summary>
        /// Gets or sets the measures collection view.
        /// </summary>
        /// <value>
        /// The measures collection view.
        /// </value>
        public ListCollectionView MeasuresCollectionView { get; set; }

        /// <summary>
        /// Gets or sets the data sets collection view.
        /// </summary>
        /// <value>
        /// The data sets collection view.
        /// </value>
        public List<string> DataSetsCollectionView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is details view on.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is details view on; otherwise, <c>false</c>.
        /// </value>
        public bool IsDetailsViewOn { get; set; }

        /// <summary>
        /// Gets or sets the width of the detail view.
        /// </summary>
        /// <value>
        /// The width of the detail view.
        /// </value>
        public string DetailViewWidth { get; set; }

        /// <summary>
        /// Gets or sets the width of the spliter.
        /// </summary>
        /// <value>
        /// The width of the spliter.
        /// </value>
        public string SpliterWidth { get; set; }

        /// <summary>
        /// Gets or sets the width of the data view.
        /// </summary>
        /// <value>
        /// The width of the data view.
        /// </value>
        public string DataViewWidth { get; set; }

        /// <summary>
        /// Gets or sets the website structs.
        /// </summary>
        /// <value>
        /// The website structs.
        /// </value>
        public List<WebsiteStruct> WebsiteStructs { get; set; }

        /// <summary>
        /// Gets or sets the topics struts.
        /// </summary>
        /// <value>
        /// The topics struts.
        /// </value>
        public List<TopicStruct> TopicsStruts { get; set; }

        /// <summary>
        /// Gets or sets the dataset list.
        /// </summary>
        /// <value>
        /// The dataset list.
        /// </value>
        public List<KeyValuePair<string, int>> DatasetList { get; set; }

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        public override int TotalCount
        {
            get
            {
                return DatasetList != null ? SelectedDataSet != ALL_DATASETS ? DatasetList.FirstOrDefault(x => x.Key == SelectedDataSet).Value : DatasetList.Sum(x => x.Value) : 0;
            }

        }


        #endregion

        #region Constrcutor

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageMeasuresViewModel"/> class.
        /// </summary>
        public ManageMeasuresViewModel()
        {
            Index = 0;
            //IsInitialLoad = true;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the assign topics command.
        /// </summary>
        /// <value>
        /// The assign topics command.
        /// </value>
        public DelegateCommand<string> AssignTopicsCommand { get; set; }

        /// <summary>
        /// Gets or sets the navigate to details command.
        /// </summary>
        /// <value>
        /// The navigate to details command.
        /// </value>
        public DelegateCommand<Measure> NavigateToDetailsCommand { get; set; }

        /// <summary>
        /// Gets or sets the apply data set filter command.
        /// </summary>
        /// <value>
        /// The apply data set filter command.
        /// </value>
        public DelegateCommand ApplyDataSetFilterCommand { get; set; }

        /// <summary>
        /// Gets or sets the reset command.
        /// </summary>
        /// <value>
        /// The reset command.
        /// </value>
        public DelegateCommand ResetCommand { get; set; }

        /// <summary>
        /// Gets or sets the details view command.
        /// </summary>
        /// <value>
        /// The details view command.
        /// </value>
        public DelegateCommand DetailsViewCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected override void InitCommands()
        {
            base.InitCommands();
            NavigateToDetailsCommand = new DelegateCommand<Measure>(NavigateToDetails, CanNavigateDetails);
            ResetCommand = new DelegateCommand(Reset);
            DetailsViewCommand = new DelegateCommand(ExecuteDetailsViewCommand);
        }

        /// <summary>
        /// Initializes the properties.
        /// </summary>
        protected override void InitProperties()
        {
            //new MeasuresFilter(this);
            HeaderName = MANAGE_MEASURES;
            PropertyFilters = new ObservableCollection<string>
                {
                    ModelPropertyFilterValues.NONE,
                    ModelPropertyFilterValues.MEASURE_CODE,
                    ModelPropertyFilterValues.MEASURE_NAME,
                    ModelPropertyFilterValues.WEBSITE_NAME,
                    ModelPropertyFilterValues.TOPIC_NAME,
                    ModelPropertyFilterValues.SUBTOPIC_NAME
                };
            if (DataSetsCollectionView != null && !DataSetsCollectionView.Contains(ALL_DATASETS))
                DataSetsCollectionView.Insert(0, ALL_DATASETS);

            SelectedDataSet = ALL_DATASETS;
            SelectedProperty = ModelPropertyFilterValues.NONE;
            PropertyFilterText = string.Empty;
            SelectedMeasureNameType = "Plain Title";
        }
        /// <summary>
        /// Gets the page.
        /// </summary>
        public void GetPage()
        {
            var args = new PagingResults<Measure>();
            args.SetPagingArguments(TotalCount, 50, 1, () => GetPage());

            var pageArgs = CollectionItems as IPagingArguments ?? args;

            pageArgs.RowsCount = TotalCount;

            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                var allMeasures = session.Query<Measure>()
                    .OrderBy(x => SelectedDataSet != ALL_DATASETS ? x.Owner.Name : x.Owner.Name)
                    .Where(FilterClausePredicate)
                      .Select(x => new Measure
                      {
                          Id = x.Id,
                          Name = x.Name,
                          MeasureTitle = x.MeasureTitle,
                          MeasureType = x.MeasureType,
                          Description = x.Description,
                          HigherScoresAreBetter = x.HigherScoresAreBetter,
                          NQFEndorsed = x.NQFEndorsed,
                          NQFID = x.NQFID,
                          Source = x.Source,
                          Owner = x.Owner
                      })
                      .Skip((pageArgs.PageIndex - 1) * pageArgs.PageSize)
                      .Take(pageArgs.PageSize)
                      .ToList();

                var distinctMeasures = allMeasures.DistinctBy(m => m.Name).ToList();

                foreach (var m in distinctMeasures)
                {
                    var topics = new List<Topic>();
                    var assignedTopics = TopicsStruts.Where(x => x.MeasureCode == m.Name && !string.IsNullOrEmpty(x.TopicName) && !string.IsNullOrEmpty(x.SubTopicName)).ToList();
                    m.WebsitesForMeasureDisplay = WebsiteStructs.Where(x => x.MeasureId == m.Id).Select(w => w.WebsiteName).ToList();

                    m.TopicsForMeasureDisplay = assignedTopics.Count == 0 ? "Topics are not assigned" : string.Join(",", assignedTopics.Select(x => x.TopicName + " > " + x.SubTopicName).ToList());
                }

                CollectionItems = new PagingResults<Measure>(distinctMeasures.ToObservableCollection(), pageArgs);
                SelectedMeasure = distinctMeasures.FirstOrDefault();
                RaisePropertyChanged(() => CollectionItems);
                RaisePropertyChanged(() => HeaderText);
            }
        }

        /// <summary>
        /// Executes the load.
        /// </summary>
        /// <param name="session">The session.</param>
        protected override void ExecLoad(ISession session)
        {
            DatasetList = session.Query<Target>()
                .Select(x => new KeyValuePair<string, int>(x.Name, x.Measures.Count()))
                .ToList();

            DataSetsCollectionView = DatasetList.Select(x => x.Key).Where(x => x != "Physician Data").ToList();
            WebsiteStructs = GetWebsites(session);
            TopicsStruts = GetTopics(session);

            GetPage();

            //var allMeasures = session.Query<Measure>()
            //                      .Where(x => !x.IsOverride)
            //                      .OrderBy(x => x.Name)
            //                      .Select(x => GenExecLoadMeasure(x))
            //                      .Cacheable()
            //                      .CacheMode(CacheMode.Normal)
            //                      .CacheRegion("MeasureLists:Measures")
            //                      .ToFuture()
            //                      .ToList();

            //var distinctMeasures = allMeasures.DistinctBy(m => m.Name).ToList();

            //foreach (var m in distinctMeasures)
            //{
            //    var topics = new List<Topic>();
            //    var assignedTopics = allMeasures.Where(x => x.Name == m.Name).Select(t => t.Topics).ToList();
            //    m.WebsitesForMeasureDisplay = WebsiteStructs.Where(x => x.MeasureId == m.Id).Select(w => w.WebsiteName).ToList(); //  GetWebsites(session, m.Id);

            //    if (assignedTopics.Count() == 1) continue;

            //    assignedTopics.ForEach(topics.AddRange);
            //    topics.DistinctBy(tt => tt.Id).ForEach(t => m.AddTopic(t));
            //}

            //CollectionItems = new ListCollectionView(distinctMeasures.ToObservableCollection());
            //SelectedMeasure = distinctMeasures.FirstOrDefault();


            RaisePropertyChanged(() => CollectionItems);
            RaisePropertyChanged(() => HeaderText);
            //foreach (var item in CollectionItems.OfType<Measure>().ToList())
            //{
            //    item.WebsitesForMeasureDisplay = GetWebsites(session, item.Id);
            //}
        }

        /// <summary>
        /// Gens the execute load measure.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        private Measure GenExecLoadMeasure(Measure x)
        {
            var mx = new Measure
            {
                Id = x.Id,
                Name = x.Name,
                MeasureTitle = x.MeasureTitle,
                MeasureType = x.MeasureType,
                //   Topics = x.Topics,
                Description = x.Description,
                HigherScoresAreBetter = x.HigherScoresAreBetter,
                NQFEndorsed = x.NQFEndorsed,
                NQFID = x.NQFID,
                Source = x.Source,
                Owner = x.Owner,
                //WebsitesForMeasureDisplay = GetWebsites(session, x.Id)
            };
            x.Topics.ForEach(t => mx.AddTopic(t));
            return mx;
        }


        /// <summary>
        /// Handles the IsActiveChanged event of the ListTabViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        protected override void ListTabViewModel_IsActiveChanged(object sender, EventArgs e)
        {
            if (IsActive)
            {
                OnLoad();
                InitProperties();

                IsLoaded = false;
                SelectedDataSet = null;
                SelectedDataSet = ALL_DATASETS;

            }
            else
                OnUnLoad();

            IsLoaded = true;

        }

        /// <summary>
        /// Gets the websites.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="measureId">The measure identifier.</param>
        /// <returns></returns>
        private List<string> GetWebsites(ISession session, int measureId)
        {
            if (!session.IsOpen)
                session = DataserviceProvider.SessionFactory.OpenSession();

            return session.Query<Website>()
                             .Where(x => x.Measures.Any(wm => wm.OriginalMeasure.Id == measureId))
                             .Select(w => w.Name).ToList();
        }

        /// <summary>
        /// Determines whether this instance [can navigate details] the specified measure.
        /// </summary>
        /// <param name="measure">The measure.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can navigate details] the specified measure; otherwise, <c>false</c>.
        /// </returns>
        private bool CanNavigateDetails(Measure measure)
        {
            return true;
        }

        /// <summary>
        /// Navigates to details.
        /// </summary>
        /// <param name="measure">The measure.</param>
        public void NavigateToDetails(Measure measure)
        {
            var query = new UriQuery
            {
                {"MeasureId", SelectedMeasure.Id.ToString(CultureInfo.InvariantCulture)},
            };
            IsNavigatedFromManageMeasuresDetailsView = true;
            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MeasureDetailsView + query, UriKind.Relative));
        }

        /// <summary>
        /// Executes the details view command.
        /// </summary>
        private void ExecuteDetailsViewCommand()
        {
            if (!IsDetailsViewOn)
            {
                DetailViewWidth = DETAIL_VIEW_WIDTH_ON;
                SpliterWidth = SPLITER_WIDTH_ON;
                DataViewWidth = DATA_VIEW_WIDTH_ON;
                IsDetailsViewOn = true;
            }
            else
            {
                DetailViewWidth = ZERO_WIDTH;
                SpliterWidth = ZERO_WIDTH;
                DataViewWidth = DATA_VIEW_WIDTH_ON;
                IsDetailsViewOn = false;
            }
        }

        /// <summary>
        /// Determines whether this instance can assign the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>
        ///   <c>true</c> if this instance can assign the specified argument; otherwise, <c>false</c>.
        /// </returns>
        public bool CanAssign(object arg)
        {
            return true;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public override void Reset()
        {
            IsLoaded = false;
            SelectedDataSet = DataSetsCollectionView.FirstOrDefault();
            SelectedProperty = PropertyFilters.FirstOrDefault();
            PropertyFilterText = string.Empty;
            IsLoaded = true;
            GetPage();
            if (CollectionItems.Count > 0) CollectionItems.MoveCurrentToFirst();
        }

        #region Code to remove

        /// <summary>
        /// Gets the websites.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        /// *This method is called in 3 occasions:
        // 1. When User clicks Assign Topic Buttons => Topic popup is set to Open PARAMETER:null
        // 2. Pop Up is open and user clicks cancel PARAMETER: Cancel
        // 3. Pop Up is open and used clicks Save PARAMETER: Save      
        // */
        //public void AssignTopics(string param)
        //{
        //    IsTopicsEditOpen = !IsTopicsEditOpen;

        //    if (CANCEL.Equals(param, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(param))
        //    {
        //        UnselectTopics();
        //        return;
        //    }

        //    var toupdate =
        //        MeasuresCollectionView.OfType<MeasureModel>().Where(x => x.IsSelectedForTopicAssignment).ToList();

        //    foreach (TopicViewModel topic in TopicsCollectionView)
        //    {
        //        foreach (SubTopicViewModel subtopic in topic.ChildrenCollectionView)
        //        {
        //            if (!subtopic.IsSelected) continue;

        //            foreach (var m in toupdate.Where(m => m.Measure.Topics.All(x => x.Id != subtopic.Id)))
        //            {
        //                m.Measure.Topics.Add(subtopic.Entity);
        //            }
        //        }
        //    }

        //    foreach (var m in toupdate)
        //    {
        //        MeasuresService.AddToPipline(m.Measure);
        //    }

        //    MeasuresService.CommitPipeline();
        //}

        //private void UnselectTopics()
        //{
        //    foreach (TopicViewModel topic in TopicsCollectionView)
        //    {
        //        topic.IsSelected = false;
        //        foreach (SubTopicViewModel subtopic in topic.ChildrenCollectionView)
        //        {
        //            subtopic.IsSelected = false;
        //        }
        //    }
        //}

        #endregion

        /// <summary>
        /// Gets the Websites.
        /// </summary>
        /// <param name="session">The session.</param>
        private List<WebsiteStruct> GetWebsites(ISession session)
        {
            var query = string.Format("SELECT DISTINCT w.Name as WebsiteName, wm.OriginalMeasure_Id as MeasureId FROM {0}s w INNER JOIN {1} wm on w.Id = wm.Website_Id", typeof(Website).Name, WebsiteTableNames.WebsiteMeasuresTable);
            var result = session.CreateSQLQuery(query)
                          .AddScalar("WebsiteName", NHibernateUtil.String)
                          .AddScalar("MeasureId", NHibernateUtil.Int32)
                          .SetResultTransformer(new AliasToBeanResultTransformer(typeof(WebsiteStruct)))
                          .List().Cast<WebsiteStruct>().ToList();

            return result;
        }

        /// <summary>
        /// Gets the topics.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        private List<TopicStruct> GetTopics(ISession session)
        {
            var query = string.Format(@"select m.Name as MeasureCode, t.Name as SubTopicName, tc.Name as TopicName
                                            from Measures m
	                                    left join Measures_MeasureTopics mt  on m.Id = mt.Measure_Id
	                                    left join Topics t on t.Id =  mt.Topic_Id
	                                    left join TopicCategories tc on t.TopicCategory_id = tc.Id");
            var result = session.CreateSQLQuery(query)
                          .AddScalar("MeasureCode", NHibernateUtil.String)
                          .AddScalar("TopicName", NHibernateUtil.String)
                          .AddScalar("SubTopicName", NHibernateUtil.String)
                          .SetResultTransformer(new AliasToBeanResultTransformer(typeof(TopicStruct)))
                          .List().Cast<TopicStruct>().ToList();

            return result;
        }

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        /// <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// Gets the filter clause predicate.
        /// </summary>
        /// <value>
        /// The filter clause predicate.
        /// </value>
        public Expression<Func<Measure, bool>> FilterClausePredicate
        {
            get
            {
                if (SelectedDataSet == ALL_DATASETS && string.IsNullOrEmpty(PropertyFilterText)) return (m => m.Name != "" || m.Name != null);
                var datasets = SelectedDataSet != ALL_DATASETS ? new List<string> { SelectedDataSet } : DatasetList.Select(x => x.Key).ToList();


                if (!string.IsNullOrEmpty(PropertyFilterText))
                {
                    if (SelectedProperty == ModelPropertyFilterValues.MEASURE_CODE)
                    {
                        return (m => m.Name.ToLower().Contains(PropertyFilterText.ToLower()) && datasets.Contains(m.Owner.Name));
                    }
                    else if (SelectedProperty == ModelPropertyFilterValues.MEASURE_NAME)
                    {
                        return (m => m.MeasureTitle.Clinical.ToLower().Contains(PropertyFilterText.ToLower()) && datasets.Contains(m.Owner.Name));
                    }
                    else if (SelectedProperty == ModelPropertyFilterValues.TOPIC_NAME)
                    {
                        return (m => m.MeasureTopics.Any(t => t.Topic.Owner.Name.ToLower().Contains(PropertyFilterText.ToLower())) && datasets.Contains(m.Owner.Name));
                    }
                    else if (SelectedProperty == ModelPropertyFilterValues.SUBTOPIC_NAME)
                    {
                        return (m => m.MeasureTopics.Any(t => t.Topic.Name.ToLower().Contains(PropertyFilterText.ToLower())) && datasets.Contains(m.Owner.Name));
                    }
                    else if (SelectedProperty == ModelPropertyFilterValues.WEBSITE_NAME)
                    {
                        if (WebsiteStructs != null)
                        {
                            var mIds = WebsiteStructs.Where(w => w.WebsiteName.ToLower().Contains(PropertyFilterText.ToLower())).Select(w => w.MeasureId).ToList();
                            return (m => mIds.Contains(m.Id) && datasets.Contains(m.Owner.Name));
                        }
                        return m => false;
                    }
                }

                return (m => m.Owner.Name != "" && m.Owner.Name != null && m.Owner.Name.Contains(SelectedDataSet));
            }
        }

        #endregion

    }

    /// <summary>
    /// Struct for the website
    /// </summary>
    public struct WebsiteStruct
    {
        /// <summary>
        /// The website name
        /// </summary>
        public string WebsiteName;
        /// <summary>
        /// The measure identifier
        /// </summary>
        public int MeasureId;
    }

    /// <summary>
    /// struct for the Topic
    /// </summary>
    public struct TopicStruct
    {
        /// <summary>
        /// The measure code
        /// </summary>
        public string MeasureCode;

        /// <summary>
        /// The topic name
        /// </summary>
        public string TopicName;

        /// <summary>
        /// The sub topic name
        /// </summary>
        public string SubTopicName;
    }
}
