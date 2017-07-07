using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.DataSets.HospitalRegionMapping.Events;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.Infrastructure.Types;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.ViewModels;
using NHibernate;
using NHibernate.Linq;
using PropertyChanged;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;

namespace Monahrq.DataSets.NHC.ViewModels
{
    /// <summary>
    /// The nursing home listing view model.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.ViewModels.ListTabViewModel{Monahrq.Infrastructure.Domain.NursingHomes.NursingHome}" />
    [Export]
    [ImplementPropertyChanged]
    public class NursingHomesViewModel : ListTabViewModel<NursingHome>
    {
        #region Fields and Constants
        private bool _isAllSelected;
        private string _filterText;
        private List<FilterDefinition> _filterEnumerations;
        private FilterDefinition _selectedFilter;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is all selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is all selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsAllSelected
        {
            get { return _isAllSelected; }
            set { _isAllSelected = value; }
        }

        /// <summary>
        /// Gets the filter enumerations.
        /// </summary>
        /// <value>
        /// The filter enumerations.
        /// </value>
        public List<FilterDefinition> FilterEnumerations
        {
            get
            {
                if (_filterEnumerations == null)
                {
                    _filterEnumerations = new List<FilterDefinition>() {
                        new FilterDefinition() {Caption = "Nursing Home Name"
                            , Predicate = (nh => nh.Name.Contains(FilterText))}
                        , new FilterDefinition() {Caption = "City"
                            , Predicate = (nh => nh.City.Contains(FilterText))}
                        , new FilterDefinition() {Caption = "State"
                            , Predicate = (nh => nh.State.Contains(FilterText))}
                        , new FilterDefinition() {Caption = "CMS Provider ID"
                            , Predicate = (nh => nh.ProviderId.Contains(FilterText))}

                    };
                }
                return _filterEnumerations;
            }
        }

        /// <summary>
        /// Gets or sets the selected filter.
        /// </summary>
        /// <value>
        /// The selected filter.
        /// </value>
        public FilterDefinition SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                if (_selectedFilter == value) return;
                FilterText = string.Empty;
                _selectedFilter = value;
            }
        }

        /// <summary>
        /// Gets or sets the filter text.
        /// </summary>
        /// <value>
        /// The filter text.
        /// </value>
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (value == _filterText) return;
                _filterText = value;
                if (PagingArguments.AllRecordsRequested)
                {
                    CollectionItems.Filter = null;
                    CollectionItems.Filter = CompositeFilter;
                }
                else GetPage();

            }
        }

        /// <summary>
        /// Gets the nursing home sample file.
        /// </summary>
        /// <value>
        /// The nursing home sample file.
        /// </value>
        public string NursingHomeSampleFile { get { return "Nursing Home Sample File.csv"; } }

        /// <summary>
        /// Gets the paging arguments.
        /// </summary>
        /// <value>
        /// The paging arguments.
        /// </value>
        public IPagingArguments PagingArguments
        {
            get
            {
                if (CollectionItems == null) return null;
                var pagingArgs = CollectionItems as IPagingArguments;
                return pagingArgs;
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the add new nursing home command.
        /// </summary>
        /// <value>
        /// The add new nursing home command.
        /// </value>
        public DelegateCommand AddNewNursingHomeCommand { get; set; }

        /// <summary>
        /// Gets or sets the import nursing home command.
        /// </summary>
        /// <value>
        /// The import nursing home command.
        /// </value>
        public DelegateCommand ImportNursingHomeCommand { get; set; }

        #endregion
        
        #region Methods

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            EventAggregator.GetEvent<ContextAppliedEvent>().Subscribe(s =>
            {
                if (s.Equals("Saved")) OnLoad();

            });
        }

        /// <summary>
        /// Instantiate CollectionItems to a Paging able collection
        /// </summary>
        protected override void InitProperties()
        {
            base.InitProperties();
            //Assert that CollectionItems implements IPagingArguments
            if (CollectionItems == null || !(CollectionItems is IPagingArguments))
            {

                CollectionItems = new PagingResults<NursingHome>();

                //Instantiate default sorting columns
                var sortColumns = new PagingSortingColumns();
                sortColumns.Add(new PagingSortingColumn() { SortMemberPath = "State", SortDirection = ListSortDirection.Ascending });
                sortColumns.Add(new PagingSortingColumn() { SortMemberPath = "Name", SortDirection = ListSortDirection.Ascending });

                //Instantiate all Paging Arguments
                (CollectionItems as PagingResults<NursingHome>)
                    .SetPagingArguments(rowsCount: 0, pageSize: 50
                    , pageIndex: 1, pagingFunction: () => GetPage()
                    , sortingColumns: sortColumns);
            }
        }

        /// <summary>
        /// Delegate that reloads (ExecLoad) Paging able collection
        /// </summary>
        public void GetPage()
        {
            using (var cursor = ApplicationCursor.SetCursor(System.Windows.Input.Cursors.Wait))
            {
                using (var session = DataserviceProvider.SessionFactory.OpenSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        ExecLoad(session);
                    }
                }
                cursor.Pop();
            }

        }

        /// <summary>
        /// Handles the IsActiveChanged event of the NursingHomesViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void ListTabViewModel_IsActiveChanged(object sender, EventArgs e)
        {
            base.ListTabViewModel_IsActiveChanged(sender, e);
            SelectedFilter = FilterEnumerations[0];
            FilterText = string.Empty;
            GetPage();
        }

        /// <summary>
        /// Executes the load.
        /// </summary>
        /// <param name="session">The session.</param>
        protected override void ExecLoad(ISession session)
        {
            //Skip ExecLoad if it is being executed by the Load method.
            //PagingControl handles Loading 
            if (IsLoading) return;
            //base.ExecLoad(session);
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            var states = configService.HospitalRegion.DefaultStates.OfType<string>().ToList();
            var nursingHomes = new List<NursingHome>();

            if (PagingArguments == null || PagingArguments.AllRecordsRequested)
                //Fetch a full recordset for in memory handling 
                ExecLoadFullRecordset(session, states);
            else {
                //Clear in memory settings
                ClearInMemorySettings();
                //Fetch just one page of records
                ExecLoadRecordsetPage(session, states);
            }

        }

        /// <summary>
        /// Gets the total rows count.
        /// </summary>
        /// <value>
        /// The total rows count.
        /// </value>
        public int TotalRowsCount { get { return PagingArguments.RowsCount ?? 0; } }

        /// <summary>
        /// Executes the load recordset page.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="states">The states.</param>
        protected void ExecLoadRecordsetPage(ISession session, List<string> states)
        {

            var fullRecordSet = session.Query<NursingHome>()
                .Where(nh => !nh.IsDeleted);

            //Filter by state
            fullRecordSet = fullRecordSet.Where(h => states.Contains(h.State));


            //Apply operator's filter
            var filteredRecordSet = fullRecordSet;
            if (!string.IsNullOrEmpty(FilterText))
            {
                filteredRecordSet = fullRecordSet.Where(SelectedFilter.Predicate);
            }

            //Refetch full recordset rows count  
            if (PagingArguments != null)
            {
                PagingArguments.RowsCount = filteredRecordSet.Count();
                RaisePropertyChanged(() => TotalRowsCount);
            }

            //Apply Sort 
            filteredRecordSet = PagingResults<NursingHome>
                .ApplySortExpressions(filteredRecordSet, PagingArguments.SortingColumns);

            //Get just one page of the recordset
            var nursingHomesPage = (filteredRecordSet
                .Skip((PagingArguments.PageIndex - 1) * PagingArguments.PageSize)
                .Take(PagingArguments.PageSize))
                .ToList<NursingHome>();

            //Populate DataGrid with new page and copy Paging arguments
            CollectionItems = new PagingResults<NursingHome>(nursingHomesPage, PagingArguments);

        }

        /// <summary>
        /// Executes the load full recordset.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="states">The states.</param>
        protected void ExecLoadFullRecordset(ISession session, List<string> states)
        {

            var nursingHomes = new List<NursingHome>();

            foreach (var state in states)
            {
                nursingHomes.AddRange(session.Query<NursingHome>().Where(nh => nh.State == state && !nh.IsDeleted).ToList());
            }
            CollectionItems = new PagingResults<NursingHome>(nursingHomes, PagingArguments);
            if (CollectionItems.SortDescriptions == null || CollectionItems.SortDescriptions.Count == 0)
            {
                CollectionItems.SortDescriptions.Add(new SortDescription("State", ListSortDirection.Ascending));
                CollectionItems.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }

            CollectionItems.MoveCurrentToFirst();
            if (!string.IsNullOrEmpty(FilterText))
            {
                CollectionItems.Filter = null;
                CollectionItems.Filter = CompositeFilter;
            }
        }

        /// <summary>
        /// Clears the in memory settings.
        /// </summary>
        void ClearInMemorySettings()
        {
            if (CollectionItems == null) return;
            //Clear Sorting settings
            if (CollectionItems.SortDescriptions != null) CollectionItems.SortDescriptions.Clear();
            //clear filter
            CollectionItems.Filter = null;
        }

        /// <summary>
        /// Called when [delete] Command is triggered.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected override async void OnDelete(NursingHome entity)
        {
            if (entity == null) return;

            var result = MessageBox.Show(@"Are you sure you want to delete nursing home """ + entity.Name + @"""?",
                @"Delete Confirmation", MessageBoxButtons.OKCancel);

            if (result != DialogResult.OK) return;

            var errorOccurred = false;
            var progressService = new ProgressService();

            progressService.SetProgress("Deleting nursing home", 0, false, true);

            await Task.Delay(500);

            var executedSuccessully = await progressService.Execute(() =>
            {
                using (var session = DataserviceProvider.SessionFactory.OpenSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        var updateQuery = string.Format(@"UPDATE {0} SET IsDeleted = 1 WHERE ID = {1}",
                            typeof(NursingHome).EntityTableName(), entity.Id);
                        session.CreateSQLQuery(updateQuery).ExecuteUpdate();
                        session.Flush();
                        trans.Commit();
                    }
                }
            },
            opResult =>
            {
                errorOccurred = !opResult.Status && opResult.Exception != null;
                progressService.SetProgress("Completed", 100, true, false);

            }, new CancellationToken());

            if (executedSuccessully && !errorOccurred)
                GetPage();
        }

        /// <summary>
        /// Called when [edit].
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected override void OnEdit(NursingHome entity)
        {
            var nursingHomelId = entity != null ? entity.Id.ToString(CultureInfo.InvariantCulture) : CurrentSelectedItem.Id.ToString(CultureInfo.InvariantCulture);

            var q = new UriQuery { { "NursingHomeId", nursingHomelId } };

            NavigateToEditScreen(q);
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected override void InitCommands()
        {
            base.InitCommands();
            AddNewNursingHomeCommand = new DelegateCommand(OnAddNewNursingHome);
        }

        /// <summary>
        /// Called when [add new nursing home].
        /// </summary>
        private void OnAddNewNursingHome()
        {
            var q = new UriQuery { { "NursingHomeId", "-1" } };
            NavigateToEditScreen(q);
        }

        /// <summary>
        /// Composites the filter.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public bool CompositeFilter(object obj)
        {
            var item = obj as NursingHome;
            if (item == null || string.IsNullOrEmpty(FilterText)) return true;

            var filterTxt = FilterText.Trim();
            if (SelectedFilter == FilterEnumerations[0]) //Nursing Home Name
                return !string.IsNullOrEmpty(item.Name) && item.Name.ContainsIgnoreCase(filterTxt);
            if (SelectedFilter == FilterEnumerations[1]) //City
                return !string.IsNullOrEmpty(item.City) && item.City.ContainsIgnoreCase(filterTxt);
            if (SelectedFilter == FilterEnumerations[2]) //State
                return !string.IsNullOrEmpty(item.State) && item.State.ContainsIgnoreCase(filterTxt);
            return SelectedFilter != FilterEnumerations[3] || (!string.IsNullOrEmpty(item.ProviderId) && item.ProviderId.ContainsIgnoreCase(filterTxt));
        }

        /// <summary>
        /// Called when [navigated to].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        /// <summary>
        /// Determines whether [is navigation target] [the specified navigation context].
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
        /// Called when [navigated from].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        /// <summary>
        /// Navigates to edit screen.
        /// </summary>
        /// <param name="query">The query.</param>
        private void NavigateToEditScreen(UriQuery query)
        {
            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.NursingHomeDetail + query, UriKind.Relative));
        }

        #endregion

        #region Inner Class

        /// <summary>
        /// The nursing homes grid filter definition class.
        /// </summary>
        public class FilterDefinition
        {
            /// <summary>
            /// Gets or sets the caption.
            /// </summary>
            /// <value>
            /// The caption.
            /// </value>
            public string Caption { get; set; }
            /// <summary>
            /// Gets or sets the filter.
            /// </summary>
            /// <value>
            /// The filter.
            /// </value>
            public Predicate<object> Filter { get; set; }

            /// <summary>
            /// Gets or sets the predicate.
            /// </summary>
            /// <value>
            /// The predicate.
            /// </value>
            public System.Linq.Expressions.Expression<Func<NursingHome, bool>> Predicate { get; set; }
            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return Caption;
            }
        }

        #endregion
    }
}
