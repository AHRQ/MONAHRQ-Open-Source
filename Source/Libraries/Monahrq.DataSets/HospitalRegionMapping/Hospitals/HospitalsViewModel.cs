using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.HospitalRegionMapping.Events;
using Monahrq.DataSets.HospitalRegionMapping.Mapping;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Generators;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Services.Hospitals;
using Monahrq.Infrastructure.Types;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Services.Import;
using Monahrq.Sdk.ViewModels;
using Monahrq.Theme.Behaviors;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using PropertyChanged;
using Cursors = System.Windows.Input.Cursors;
using Region = Monahrq.Infrastructure.Domain.Regions.Region;
using RegionNames = Monahrq.Sdk.Regions.RegionNames;
using Monahrq.Infrastructure;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Monahrq.DataSets.HospitalRegionMapping.Hospitals
{
    /// <summary>
    /// The hospital list view model
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.ViewModels.ListTabViewModel{Monahrq.Infrastructure.Entities.Domain.Hospitals.Hospital}" />
    [Export]
    [ImplementPropertyChanged]
    public class HospitalsViewModel : ListTabViewModel<Hospital>
    {
        #region Fields and Constants

        /// <summary>
        /// The cancel
        /// </summary>
        public const string CANCEL = "Cancel";
        /// <summary>
        /// The save
        /// </summary>
        public const string SAVE = "SAVE";
        /// <summary>
        /// The none
        /// </summary>
        public const string NONE = "NONE";

        private bool _isAllSelected;

        /// <summary>
        /// The more than one hospital selected
        /// </summary>
        private const string MoreThanOneHospitalSelected =
            @"You have selected more than one hospital to assign hospital categories. Previously assigned hospital categories for the selected hospitals will be overwritten. Please select the hospital categories for these hospitals.";

        private readonly List<string> _hospitalTypes = new List<string> { "All", "Base", "Custom" };

        private string _selectedHospitalType;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the test collection items.
        /// </summary>
        /// <value>
        /// The test collection items.
        /// </value>
        public MultiSelectCollectionView<Hospital> TestCollectionItems { get; set; }

        /// <summary>
        /// Gets the hospital sample file.
        /// </summary>
        /// <value>
        /// The hospital sample file.
        /// </value>
        public string HospitalSampleFile
        {
            get { return "Hospital Import Sample.csv"; }
        }

        /// <summary>
        /// The available states
        /// </summary>
        public List<string> AvailableStates;

        /// <summary>
        /// Gets or sets the internal selected filter.
        /// </summary>
        /// <value>
        /// The internal selected filter.
        /// </value>
        public FilterDefinition InternalSelectedFilter { get; set; }

        /// <summary>
        /// Gets or sets the selected filter.
        /// </summary>
        /// <value>
        /// The selected filter.
        /// </value>
        public FilterDefinition SelectedFilter
        {
            get { return InternalSelectedFilter; }
            set { InternalSelectedFilter = value; }
        }

        /// <summary>
        /// Gets or sets the internal filter enumerations.
        /// </summary>
        /// <value>
        /// The internal filter enumerations.
        /// </value>
        public ObservableCollection<FilterDefinition> InternalFilterEnumerations { get; set; }

        /// <summary>
        /// Gets or sets the filter enumerations.
        /// </summary>
        /// <value>
        /// The filter enumerations.
        /// </value>
        public ObservableCollection<FilterDefinition> FilterEnumerations
        {
            get { return InternalFilterEnumerations; }
            set { InternalFilterEnumerations = value; }
        }

        /// <summary>
        /// Gets or sets the filter text.
        /// </summary>
        /// <value>
        /// The filter text.
        /// </value>
        public string FilterText { get; set; }

        /// <summary>
        /// Gets or sets the type of the selected hospital.
        /// </summary>
        /// <value>
        /// The type of the selected hospital.
        /// </value>
        public FilterDefinition SelectedHospitalType { get; set; }

        /// <summary>
        /// Gets the hospital types.
        /// </summary>
        /// <value>
        /// The hospital types.
        /// </value>
        public List<string> HospitalTypes { get { return _hospitalTypes; } }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is category edit open.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is category edit open; otherwise, <c>false</c>.
        /// </value>
        public bool IsCategoryEditOpen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is all selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is all selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsAllSelected
        {
            get { return _isAllSelected; }
            set
            {
                if (_isAllSelected == value) return;
                _isAllSelected = value;
                SelectHospitals(value);
                RaisePropertyChanged(() => IsAllSelected);
            }
        }

        /// <summary>
        /// Gets the hospital categories.
        /// </summary>
        /// <value>
        /// The hospital categories.
        /// </value>
        public IEnumerable<HospitalCategory> HospitalCategories
        {
            get
            {
                var categories = Parent.Categories.CollectionItems.OfType<HospitalCategory>().OrderBy(x => x.Name);

                var hospitalCategories = categories as IList<HospitalCategory> ?? categories.OrderBy(x => x.Name).ToList();
                if (!IsCategoryEditOpen) return hospitalCategories.OrderBy(x => x.Name);
                ListExtensions.ForEach(hospitalCategories, h => h.IsSelected = false);
                if (TestCollectionItems.OfType<Hospital>().Count(x => x.IsSelected) != 1) return hospitalCategories.OrderBy(x => x.Name);
                var hospital = TestCollectionItems.OfType<Hospital>().First(h => h.IsSelected);
                ListExtensions.ForEach(hospitalCategories,
                    c => c.IsSelected = hospital.Categories.Any(hc => hc.Name == c.Name));
                return hospitalCategories.OrderBy(hc => hc.Name);
            }
        }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public MappingViewModel Parent { get; set; }

        /// <summary>
        /// Gets the selected items count.
        /// </summary>
        /// <value>
        /// The selected items count.
        /// </value>
        public int SelectedItemsCount
        {
            get { return TestCollectionItems != null ? TestCollectionItems.SelectedItems.Count : 0; }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the add hospital command.
        /// </summary>
        /// <value>
        /// The add hospital command.
        /// </value>
        public DelegateCommand AddHospitalCommand { get; private set; }

        /// <summary>
        /// Gets the commit category assigment.
        /// </summary>
        /// <value>
        /// The commit category assigment.
        /// </value>
        public DelegateCommand CommitCategoryAssigment { get; private set; }

        /// <summary>
        /// Gets the import hospital data command.
        /// </summary>
        /// <value>
        /// The import hospital data command.
        /// </value>
        public DelegateCommand ImportHospitalDataCommand { get; private set; }

        /// <summary>
        /// Gets the export hospital data command.
        /// </summary>
        /// <value>
        /// The export hospital data command.
        /// </value>
        public DelegateCommand ExportHospitalDataCommand { get; private set; }

        /// <summary>
        /// Gets the delete hospital data command.
        /// </summary>
        /// <value>
        /// The delete hospital data command.
        /// </value>
        public DelegateCommand DeleteHospitalDataCommand { get; private set; }

        /// <summary>
        /// Gets the assign category command.
        /// </summary>
        /// <value>
        /// The assign category command.
        /// </value>
        public DelegateCommand AssignCategoryCommand { get; private set; }

        /// <summary>
        /// Gets or sets the hospital region changed command.
        /// </summary>
        /// <value>
        /// The hospital region changed command.
        /// </value>
        public DelegateCommand<Hospital> HospitalRegionChangedCommand { get; set; }

        /// <summary>
        /// Gets or sets the hospital CMS provider identifier changed command.
        /// </summary>
        /// <value>
        /// The hospital CMS provider identifier changed command.
        /// </value>
        public DelegateCommand<object[]> HospitalCmsProviderIdChangedCommand { get; set; }

        /// <summary>
        /// Gets or sets the deferred action.
        /// </summary>
        /// <value>
        /// The deferred action.
        /// </value>
        public DeferredAction DeferredAction { get; set; }

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        [Import]
        public IHospitalRegistryService Service { get; set; }

        /// <summary>
        /// Gets or sets the hospital importer.
        /// </summary>
        /// <value>
        /// The hospital importer.
        /// </value>
        [Import(ImporterContract.Hospital)]
        private IEntityFileImporter HospitalImporter { get; set; }

        #endregion

        #region Inner Classes

        /// <summary>
        /// The filter definition class.
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
            /// Gets or sets a value indicating whether this instance is hospital type.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is hospital type; otherwise, <c>false</c>.
            /// </value>
            public bool IsHospitalType { get; set; }
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

        /// <summary>
        /// the custom cms lookup item class.
        /// </summary>
        public class CmsLookupItem
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }
            /// <summary>
            /// Gets or sets the CMS provider identifier.
            /// </summary>
            /// <value>
            /// The CMS provider identifier.
            /// </value>
            public string CmsProviderID { get; set; }
            /// <summary>
            /// Gets or sets the name of the hospital.
            /// </summary>
            /// <value>
            /// The name of the hospital.
            /// </value>
            public string HospitalName { get; set; }
        }

        #endregion

        #region Commands Methods

        /// <summary>
        /// Called when [assign category command].
        /// </summary>
        private void OnAssignCategoryCommand()
        {
            var selectedHospitals = TestCollectionItems.SelectedItems.Count;
            // TestCollectionItems.OfType<Hospital>().Count(x => x.IsSelected);
            if (selectedHospitals > 1 && !IsCategoryEditOpen)
            {
                if (
                    MessageBox.Show(MoreThanOneHospitalSelected, "Warning", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning) == DialogResult.Cancel) return;
            }

            RaisePropertyChanged(() => HospitalCategories);
            IsCategoryEditOpen = !IsCategoryEditOpen;
        }

        /// <summary>
        /// Selects the hospitals.
        /// </summary>
        /// <param name="isAllSelected">if set to <c>true</c> [is all selected].</param>
        private void SelectHospitals(bool isAllSelected)
        {
            foreach (var hvm in TestCollectionItems.OfType<Hospital>().ToList())
            {
                hvm.IsSelected = isAllSelected;
            }
            TestCollectionItems.Refresh();
        }

        /// <summary>
        /// Determines whether this instance [can commit category].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can commit category]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanCommitCategory()
        {
            return TestCollectionItems != null && TestCollectionItems.OfType<Hospital>().Any(vm => vm.IsSelected);
        }

        /// <summary>
        /// Called when [committed].
        /// </summary>
        protected async void OnCommitted()
        {
            IsCategoryEditOpen = false;
            var anyErrors = false;
            Exception saveException = null;
            ProgressService progressService = new ProgressService();

            var selectedCategories = HospitalCategories.Where(x => x.IsSelected).DistinctBy(x => x.Id).ToList();
            try
            {
                // TODO: PUT A FEW COMMENTS HERE. WHAT ARE WE DOING?
                var selectedHospitals = TestCollectionItems.SelectedItems;

                progressService.SetProgress("Assignning Categories", 0, false, true);

                var savedhospitalCount = 0;

                var operationComplete = await progressService.Execute(() =>
                {
                    foreach (var x in selectedHospitals)
                    {
                        Hospital archivedHospital = null;
                        if (x.IsSourcedFromBaseData)
                        {
                            archivedHospital = Service.CreateHospitalArchive(x);
                            x.IsSourcedFromBaseData = false;
                            x.Id = 0;
                        }

                        x.Categories.Clear();
                        selectedCategories.ForEach(hc =>
                        {
                            x.Categories.Add(hc);
                        });

                        Service.Save(x);
                        savedhospitalCount++;
                      
                        if (archivedHospital != null)
                        {
                            archivedHospital.LinkedHospitalId = x.Id;
                            archivedHospital.ArchiveDate = DateTime.Now;

                            Service.Save(archivedHospital);
                        }
                    }
                }, result =>
                {
                    if (result.Status && result.Exception == null)
                    {
                        anyErrors = false;
                        saveException = null;
                    }
                    else
                    {
                        anyErrors = false;
                        saveException = null;
                    }
                }, CancellationToken.None);

                if (operationComplete)
                {
                    progressService.SetProgress("Completed", 100, true, false);

                    if (!anyErrors && saveException == null)
                    {
                        //if (savedhospitalCount > 0)
                        Notify(string.Format("{0} hospitals have been successfully assigned categories.",
                            savedhospitalCount));
                    }
                    else
                    {
                        var ex = saveException.GetBaseException();
                        Logger.Log(ex.Message, Category.Exception, Priority.High);
                        EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
                    }

                }

                FlushCache();

                if (!anyErrors && selectedHospitals.Count > 0)
                    Notify(string.Format("{0} hospitals have been successfully assigned categories.", savedhospitalCount));

            }
            catch (Exception ex)
            {
                Logger.Log(ex.GetBaseException().Message, Category.Exception, Priority.High);
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex.GetBaseException());
            }
            finally
            {
                selectedCategories.ForEach(item => item.IsSelected = false);
                //OnLoad();
                TestCollectionItems.Refresh();
            }
        }

        /// <summary>
        /// Flushes the cache.
        /// </summary>
        private void FlushCache()
        {
            var states = ConfigurationService.HospitalRegion.DefaultStates.OfType<string>()
                .OrderBy(x => x)
                .Select(s => s)
                .ToList();

            states.ForEach(state =>
            {
                DataserviceProvider.SessionFactory.ClearNhibernateQueryCaches("HospitalList:" + state);
            });
            // Flush query cache
        }

        /// <summary>
        /// Determines whether this instance [can delete data].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can delete data]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanDeleteData()
        {
            return true;
        }

        /// <summary>
        /// Called when [delete hospital data].
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void OnDeleteHospitalData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether this instance [can export data].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can export data]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExportData()
        {
            return true;
        }

        /// <summary>
        /// Called when [export hospital data].
        /// </summary>
        private void OnExportHospitalData()
        {
            var fileName = string.Format("{0}_Hospitals_{1:yyyyMMdd}.csv",
                string.Join("_", ConfigurationService.HospitalRegion.DefaultStates.OfType<string>().ToList()),
                DateTime.Now);
            //Open File Dialog so that user can point to the Export folder and file..
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                CheckFileExists = false,
                Filter = @"CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                AddExtension = true,
                FileName = fileName,
                InitialDirectory = MonahrqContext.FileExportsDirPath
            };

            if (!Directory.Exists(dlg.InitialDirectory)) Directory.CreateDirectory(dlg.InitialDirectory);

            //..but quit Export if user changes its mind by pressing Cancel button
            if (dlg.ShowDialog() != true) return;

            //Manufacture a filename if none is informed
            if (string.IsNullOrEmpty(dlg.FileName))
                dlg.FileName = fileName;

            //Cue Export with a wait cursor (Export may be too fast to be observed)
            using (ApplicationCursor.SetCursor(Cursors.Wait))
            {
                //Export the Hospitals collection in memory
                var hospitalList = new List<HospitalExport>();
                var hospitals = TestCollectionItems.SourceCollection.OfType<Hospital>().ToList();

                hospitals.ForEach(hosp => hospitalList.Add(new HospitalExport(hosp)));

                CSVGenerator.Create(hospitalList, dlg.FileName);
                Notify(string.Format("Hospital data exported to file {0}", dlg.FileName));
            }
        }

        /// <summary>
        /// Gets or sets the selected path.
        /// </summary>
        /// <value>
        /// The selected path.
        /// </value>
        public string SelectedPath { get; set; }

        /// <summary>
        /// Gets or sets the CMS collection.
        /// </summary>
        /// <value>
        /// The CMS collection.
        /// </value>
        public ObservableCollection<CmsLookupItem> CmsCollection { get; set; }

        /// <summary>
        /// Called when [import hospital data].
        /// </summary>
        private void OnImportHospitalData()
        {
            HospitalImporter.Execute(); // execute hospital file import

            FlushCache(); // Flush query cache

            ForceLoad(); // Force Reload
        }

        /// <summary>
        /// Called when [add hospital].
        /// </summary>
        private void OnAddHospital()
        {
            var query = new UriQuery
            {
                {"HospitalId", "-1"},
            };

            RegionManager.RequestNavigate(RegionNames.MainContent,
                new Uri(ViewNames.DetailsView + query, UriKind.Relative));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the CMS provider ids.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        private IList<CmsLookupItem> LoadCmsProviderIds(ISession session)
        {
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            var cmsLookup = new List<CmsLookupItem>();

            foreach (var state in configService.HospitalRegion.DefaultStates.OfType<string>().ToList())
            {
                IList<Hospital> tempCmsLookup = session.CreateCriteria<Hospital>()
                    .Add(Restrictions.Eq("State", state))
                    .Add(Restrictions.Eq("IsDeleted", false))
                    .Add(Restrictions.IsNotNull("CmsProviderID"))
                    .SetProjection(Projections.ProjectionList()
                        .Add(Projections.Alias(Projections.Property("CmsProviderID"), "CmsProviderID"))
                        .Add(Projections.Alias(Projections.Property("Id"), "Id"))
                        .Add(Projections.Alias(Projections.Property("Name"), "Name"))
                        .Add(Projections.Alias(Projections.Property("IsArchived"), "IsArchived")))
                    .SetResultTransformer(new AliasToBeanResultTransformer(typeof(Hospital)))
                    .List<Hospital>();

                if (tempCmsLookup.Any())
                {
                    var orderedCmsLookup =
                        tempCmsLookup.DistinctBy(c => c.CmsProviderID)
                            .OrderByDescending(c => c.Id)
                            .GroupBy(c => c.CmsProviderID)
                            .Select(x => x.First())
                            .ToList()
                            .OrderBy(x => x.CmsProviderID);
                    cmsLookup.AddRange(orderedCmsLookup.ToList().Select(x => new CmsLookupItem
                    {
                        Id = x.Id,
                        CmsProviderID = x.CmsProviderID,
                        HospitalName = x.Name
                    })
                        .ToList());
                }
            }
            return cmsLookup;
        }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            Index = 0;

            PropertyChanged += (o, e) =>
            {
                if (!string.Equals(e.PropertyName, "FilterText", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(e.PropertyName, "SelectedHospitalType", StringComparison.OrdinalIgnoreCase) ||
                    TestCollectionItems == null) return;

                var compositeFilter = ObjectExtensions.And(SelectedFilter.Filter, SelectedHospitalType.Filter);

                DeferredAction = DeferredAction.Create(() => TestCollectionItems.Filter = compositeFilter);
                // Defer applying search criteria until time has elapsed.
                DeferredAction.Defer(TimeSpan.FromSeconds(1));
            };

            HospitalImporter.Importing -= HospitalImporter_Importing;
            HospitalImporter.Importing += HospitalImporter_Importing;
            HospitalImporter.Imported -= HospitalImporter_Imported;
            HospitalImporter.Imported += HospitalImporter_Imported;

            CommitCategoryAssigment = new DelegateCommand(OnCommitted);
            AddHospitalCommand = new DelegateCommand(OnAddHospital, () => true);
            ImportHospitalDataCommand = new DelegateCommand(OnImportHospitalData, () => true);
            AssignCategoryCommand = new DelegateCommand(OnAssignCategoryCommand, CanCommitCategory);
            ExportHospitalDataCommand = new DelegateCommand(OnExportHospitalData, CanExportData);
            DeleteHospitalDataCommand = new DelegateCommand(OnDeleteHospitalData, CanDeleteData);
            HospitalRegionChangedCommand = new DelegateCommand<Hospital>(OnHospitalRegionChanged);
            HospitalCmsProviderIdChangedCommand = new DelegateCommand<object[]>(OnHospitalCmsProviderIdChanged);

            FilterEnumerations = new ObservableCollection<FilterDefinition>(CreateFilterDefinitions());
            SelectedFilter = FilterEnumerations[0];
            SelectedHospitalType = FilterEnumerations.FirstOrDefault(type => type.IsHospitalType);
            EventAggregator.GetEvent<HospitalsViewModelReadyEvent>().Publish(this);
            EventAggregator.GetEvent<SimpleImportCompletedEvent>().Subscribe(Requery);
            EventAggregator.GetEvent<ContextAppliedEvent>().Subscribe(agrs =>
            {
                if (agrs.Equals("Saved") && IsActive) OnLoad();
            });
        }

        /// <summary>
        /// Executes the load.
        /// </summary>
        /// <param name="session">The session.</param>
        protected override void ExecLoad(ISession session)
        {
            if (!Parent.Categories.IsLoaded) Parent.Categories.ForceLoad();

            var states =
                ConfigurationService.HospitalRegion.DefaultStates.OfType<string>()
                    .OrderBy(x => x)
                    .Select(s => s)
                    .ToList();
            var hospitals = new List<Hospital>();

            var allStateRegions =
                Service.GetRegions(session, states, ConfigurationService.HospitalRegion.SelectedRegionType, false)
                    .OrderBy(r => r.State).ToList();
            foreach (var state in states)
            {
                var stateAbbreviation = state;
                var tempList =
                    session.Query<Hospital>().Where(h => h.State == stateAbbreviation && !h.IsDeleted && !h.IsArchived)
                        .Distinct()
                        .Cacheable()
                        .CacheMode(CacheMode.Normal)
                        .CacheRegion("HospitalList:" + stateAbbreviation)
                        .ToList();

                foreach (var hs in tempList)
                {
                    var filteredRegions =
                        allStateRegions.Where(s => s.State.EqualsIgnoreCase(stateAbbreviation)).ToList();
                    filteredRegions.Insert(0, default(Region));

                    hs.RegionsListForDisplay = new ObservableCollection<Region>(filteredRegions.ToList());
                    hs.SetHospitalRegion(ConfigurationService.HospitalRegion.SelectedRegionType);
                    hs.ValueChanged += IsSelectedValueChanged;
                }

                hospitals.AddRange(tempList);
            }

            TestCollectionItems =
                new MultiSelectCollectionView<Hospital>(hospitals.OrderBy(h => h.State).ThenBy(h => h.Name).ToList());
            CmsCollection = LoadCmsProviderIds(session).ToObservableCollection();
            FilterText = string.Empty;
            SelectedFilter = FilterEnumerations.FirstOrDefault();
            SelectedHospitalType = FilterEnumerations.FirstOrDefault(hospitalType => hospitalType.IsHospitalType);
        }

        /// <summary>
        /// Determines whether [is selected value changed] [the specified sender].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void IsSelectedValueChanged(object sender, EventArgs e)
        {
            AssignCategoryCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(() => HospitalCategories);
            RaisePropertyChanged(() => SelectedItemsCount);
        }

        /// <summary>
        /// Called when [hospital region changed].
        /// </summary>
        /// <param name="hosp">The hosp.</param>
        private async void OnHospitalRegionChanged(Hospital hosp)
        {
            if (!CheckSelectedRegionHasChanged2(hosp, hosp.SelectedRegion)) return;
            try
            {
                var errorsExist = false;

                var progressService = new ProgressService();

                progressService.SetProgress("Assigning Categories", 0, false, true);

                progressService.Delay(1000);

                var hospital = hosp;

                var operationCompleted = await progressService.Execute(() =>
                {
                    // only update the database if the region changes
                    Hospital archivedHospital = null;
                    if (hospital.IsSourcedFromBaseData)
                    {
                        archivedHospital = Service.CreateHospitalArchive(hospital);
                        hospital.Id = 0;
                        hospital.IsSourcedFromBaseData = false;
                        archivedHospital.ValueChanged -= IsSelectedValueChanged;
                        archivedHospital.ValueChanged += IsSelectedValueChanged;
                    }

                    if (hospital.SelectedRegion is CustomRegion)
                    {
                        hospital.CustomRegion = hospital.SelectedRegion as CustomRegion;
                    }
                    else if (hospital.SelectedRegion is HospitalServiceArea)
                    {
                        hospital.HospitalServiceArea = hospital.SelectedRegion as HospitalServiceArea;
                        hospital.CustomRegion = null;
                    }
                    else if (hospital.SelectedRegion is HealthReferralRegion)
                    {
                        hospital.HealthReferralRegion = hospital.SelectedRegion as HealthReferralRegion;
                        hospital.CustomRegion = null;
                    }
                    else if (hospital.SelectedRegion.Id == 0)
                    {
                        switch (ConfigurationService.HospitalRegion.SelectedRegionType.Name)
                        {
                            case "HospitalServiceArea":
                                hospital.HospitalServiceArea = null;
                                break;
                            case "HealthReferralRegion":
                                hospital.HealthReferralRegion = null;
                                break;
                        }

                        hospital.CustomRegion = null;
                    }

                    Service.Save(hospital);

                    if (archivedHospital != null)
                    {
                        archivedHospital.LinkedHospitalId = hospital.Id;
                        Service.Save(archivedHospital);

                        Application.DoEvents();
                        TestCollectionItems.Remove(archivedHospital);
                        TestCollectionItems.CommitEdit();
                        Application.DoEvents();
                    }

                }, opResult =>
                {
                    if (!opResult.Status)
                    {
                        errorsExist = true;
                        var ex = opResult.Exception.GetBaseException();
                        SynchronizationContext.Current.Post(x =>
                        {
                            EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
                            Logger.Log(ex.ToString(), Category.Exception, Priority.High);
                        }, null);
                        progressService.SetProgress("Error", 100, true, false);
                        return;
                    }

                    errorsExist = false;
                    progressService.SetProgress("Completed", 100, true, false);
                }, new CancellationToken());

                if (operationCompleted && !errorsExist)
                {
                    TestCollectionItems.SortDescriptions.Clear();
                    TestCollectionItems.SortDescriptions.Add(new SortDescription("State", ListSortDirection.Ascending));
                    TestCollectionItems.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

                    hospital.IsSelected = false;
                    TestCollectionItems.Refresh();
                    CurrentSelectedItem = hospital;
                    RaisePropertyChanged(() => TestCollectionItems);
                }
            }
            catch (Exception exc)
            {
                var ex = exc.GetBaseException();
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
                Logger.Log(ex.ToString(), Category.Exception, Priority.High);
            }
        }

        /// <summary>
        /// Called when [hospital CMS provider identifier changed].
        /// </summary>
        /// <param name="values">The values.</param>
        private async void OnHospitalCmsProviderIdChanged(object[] values)
        {
            if (values == null || values.Length < 2) return;
            if (values[0] == null || values[1] == null) return;

            var hospital =
                TestCollectionItems.SourceCollection.OfType<Hospital>()
                    .FirstOrDefault(hosp => hosp.Id == int.Parse(values[0].ToString()));
            var cmsProviderId = values[1].ToString();

            if (hospital == null) return;
            if (string.IsNullOrEmpty(cmsProviderId)) return;

            // only update the database if the CMS Provider ID changes
            if (hospital.CmsProviderID.EqualsIgnoreCase(cmsProviderId)) return;

            try
            {
                var errorsExist = false;
                var progressService = new ProgressService();

                progressService.SetProgress("Assigning Categories", 0, false, true);

                progressService.Delay(1000);

                var operationComplete = await progressService.Execute(async () =>
                {
                    hospital.CmsProviderID = cmsProviderId;

                    var existingHospital =
                        Service.Get<Hospital>(
                            h => h.CmsProviderID.ToLower() == cmsProviderId.ToLower() && !h.IsArchived && !h.IsDeleted);

                    if (existingHospital != null && existingHospital.IsSourcedFromBaseData)
                    {
                        Hospital archivedHospital = Service.CreateHospitalArchive(existingHospital);
                        existingHospital.Id = 0;
                        existingHospital.IsSourcedFromBaseData = false;
                        existingHospital.CmsProviderID = null;
                        Service.Save(existingHospital);
                        archivedHospital.LinkedHospitalId = existingHospital.Id;
                        Service.Save(archivedHospital);
                    }
                    else if (existingHospital != null && !existingHospital.IsSourcedFromBaseData)
                    {
                        existingHospital.CmsProviderID = null;
                        Service.Save(existingHospital);
                    }

                    await Service.SaveAsync(hospital, opResult =>
                    {
                        Thread.Sleep(1000);

                        if (opResult.Status)
                        {
                            Service.Refresh(hospital);
                        }
                    });
                }, operationResult =>
                {
                    if (!operationResult.Status && operationResult.Exception != null)
                    {
                        errorsExist = true;

                        progressService.SetProgress("Error", 100, true, false);
                        var ex = operationResult.Exception.GetBaseException();

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            Logger.Log(ex.Message, Category.Exception, Priority.High);
                            EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
                        });

                        return;
                    }

                    progressService.SetProgress("Completed", 100, true, false);

                }, new CancellationToken());

                if (operationComplete && !errorsExist)
                {
                    progressService.SetProgress("Completed", 100, true, false);

                    TestCollectionItems.SortDescriptions.Clear();
                    TestCollectionItems.SortDescriptions.Add(new SortDescription("State", ListSortDirection.Ascending));
                    TestCollectionItems.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                    ForceLoad();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Category.Exception, Priority.High);
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public override void Refresh()
        {
            base.ForceLoad();
        }

        /// <summary>
        /// Called when [OnEdit].
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected override void OnEdit(Hospital entity)
        {
            using (ApplicationCursor.SetCursor(Cursors.Wait))
            {
                var hospitalId = entity != null
                    ? entity.Id.ToString(CultureInfo.InvariantCulture)
                    : CurrentSelectedItem.Id.ToString(CultureInfo.InvariantCulture);

                List<Hospital> hospitalsEditingList =
                    TestCollectionItems.Cast<Hospital>().OrderBy(h => h.State).ThenBy(h => h.Name).ToList();

                var hospitalIds = string.Join(",", hospitalsEditingList.Select(h => h.Id).ToList());

                var q = new UriQuery
                {
                    {"HospitalId", hospitalId},
                    {"IsHospitalsListEditing", "true"},
                    {"HospitalIds", hospitalIds}
                };
                RegionManager.RequestNavigate(RegionNames.MainContent
                    , new Uri(ViewNames.DetailsView + q, UriKind.Relative));
            }
        }

        /// <summary>
        /// Called when [delete].
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        protected override async void OnDelete(Hospital hospital)
        {
            var result = MessageBox.Show(@"Are you sure want to delete hospital """ + hospital.Name + @""".",
                @"Delete Confirmation", MessageBoxButtons.OKCancel);

            if (result != DialogResult.OK) return;
            if (TestCollectionItems.OfType<Hospital>().All(h => h.Id != hospital.Id)) return;

            var errorOccurred = false;
            var progressService = new ProgressService();

            progressService.SetProgress("Deleting Hospital", 0, false, true);

            await Task.Delay(500);

            var operationComplete = await progressService.Execute(() =>
            {
                using (var session = DataserviceProvider.SessionFactory.OpenSession())
                {
                    try
                    {
                        using (var tx = session.BeginTransaction())
                        {
                            session.Evict(hospital);

                            if (hospital.IsSourcedFromBaseData && hospital.IsArchived)
                            {
                                hospital.IsDeleted = true;
                                hospital = session.Merge(hospital);
                            }
                            else if (hospital.IsSourcedFromBaseData && !hospital.IsArchived)
                            {
                                hospital.IsArchived = true;
                                hospital.ArchiveDate = DateTime.Now;
                                hospital = session.Merge(hospital);
                            }
                            else
                            {
                                if (hospital.Categories.Any())
                                    hospital.Categories.Clear();

                                hospital.IsDeleted = true;
                                hospital = session.Merge(hospital);
                            }

                            session.Flush();
                            tx.Commit();
                        }
                    }
                    catch (InvalidConstraintException)
                    {
                        hospital.IsDeleted = true;
                        SaveSelectedItemCommand.Execute(hospital);
                    }
                    catch (ConstraintException)
                    {
                        hospital.IsDeleted = true;
                        SaveSelectedItemCommand.Execute(hospital);
                    }
                }
            }, opResult =>
            {
                if (!opResult.Status || opResult.Exception != null)
                {
                    errorOccurred = true;
                    var ex = opResult.Exception.GetBaseException();
                    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
                    Logger.Log(ex.ToString(), Category.Exception, Priority.High);
                }

                progressService.SetProgress("Completed", 100, true, false);

            }, new CancellationToken());

            if (operationComplete && !errorOccurred)
            {
                var hospital2Remove = TestCollectionItems.OfType<Hospital>().FirstOrDefault(h => h.Id == hospital.Id);

                if (hospital2Remove != null)
                {
                    TestCollectionItems.Remove(hospital2Remove);
                    TestCollectionItems.Refresh();
                }
            }
        }

        /// <summary>
        /// Called when [rollback custom hospital].
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        public async void OnRollbackCustomHospital(Hospital hospital)
        {
            try
            {
                var errorOccurred = false;
                var progressService = new ProgressService();

                progressService.SetProgress("Revert Hospital", 0, false, true);

                var operationComplete = await progressService.Execute(() =>
                {
                    Service.RollbackCustomHospitalToBaseHospital(hospital);
                },
                    opResult =>
                    {
                        progressService.SetProgress("Completed", 100, true, false);

                        if (!opResult.Status || opResult.Exception != null)
                        {

                            errorOccurred = true;
                            var ex = opResult.Exception.GetBaseException();
                            SynchronizationContext.Current.Post(x =>
                            {
                                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
                                Logger.Log(ex.ToString(), Category.Exception, Priority.High);
                            }, null);
                        }
                        else
                        {
                            errorOccurred = false;
                        }
                    }, new CancellationToken());


                if (operationComplete && !errorOccurred)
                {
                    Refresh();
                }
            }
            catch (Exception exc)
            {
                var ex = exc.GetBaseException();
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
                Logger.Log(ex.ToString(), Category.Exception, Priority.High);
            }
        }

        /// <summary>
        /// Checks the selected region has changed2.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <param name="selectedRegion">The selected region.</param>
        /// <returns></returns>
        private bool CheckSelectedRegionHasChanged2(Hospital hospital, Region selectedRegion)
        {
            if (selectedRegion == default(Region)) return false;
            if (hospital.CustomRegion != null) return hospital.CustomRegion.Id != selectedRegion.Id;
            if (ConfigurationService.HospitalRegion.SelectedRegionType == typeof(HealthReferralRegion))
                return hospital.HealthReferralRegion == null || hospital.HealthReferralRegion.Id != selectedRegion.Id;
            if (ConfigurationService.HospitalRegion.SelectedRegionType == typeof(HospitalServiceArea))
                return hospital.HospitalServiceArea == null || hospital.HospitalServiceArea.Id != selectedRegion.Id;
            if (ConfigurationService.HospitalRegion.SelectedRegionType == typeof(CustomRegion))
                return hospital.CustomRegion == null || hospital.CustomRegion.Id != selectedRegion.Id;

            return true;
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            EventAggregator.GetEvent<HospitalsViewModelReadyEvent>().Publish(this);
            if (!IsActive)
                IsActive = true;
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
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        /// <summary>
        /// Checks the selected region has changed.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <param name="selectedRegion">The selected region.</param>
        /// <returns></returns>
        private bool CheckSelectedRegionHasChanged(Hospital hospital, Region selectedRegion)
        {
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            if (selectedRegion == null) return true;

            if (configService.HospitalRegion.SelectedRegionType == typeof(HealthReferralRegion))
                return hospital.HealthReferralRegion == null || hospital.HealthReferralRegion.Id != selectedRegion.Id;
            if (configService.HospitalRegion.SelectedRegionType == typeof(HospitalServiceArea))
                return hospital.HospitalServiceArea == null || hospital.HospitalServiceArea.Id != selectedRegion.Id;
            return hospital.CustomRegion == null || hospital.CustomRegion.Id != selectedRegion.Id;
        }

        /// <summary>
        /// Creates the filter definitions.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<FilterDefinition> CreateFilterDefinitions()
        {
            return new List<FilterDefinition>
            {
                new FilterDefinition
                {
                    Caption = "Hospital Name",
                    Filter = item => (item as Hospital).ContainsName(FilterText)
                },

                new FilterDefinition
                {
                    Caption = "Category",
                    Filter = item => (item as Hospital).HasCategory(FilterText)
                },

                new FilterDefinition
                {
                    Caption = "Region",
                    Filter = item => (item as Hospital).HasRegion(FilterText)
                },

                new FilterDefinition
                {
                    Caption = "CMS Provider",
                    Filter = item => (item as Hospital).HasCmsProviderID(FilterText)
                },

                new FilterDefinition
                {
                    Caption = "State",
                    Filter = item => (item as Hospital).HasState(FilterText)
                },

                new FilterDefinition
                {
                    Caption = "All Hospitals",
                    Filter = item => (item as Hospital).HospitalsSelected(SelectedHospitalType.Caption),
                    IsHospitalType = true
                },

                new FilterDefinition
                {
                    Caption = "Base Hospitals",
                    Filter = item => (item as Hospital).HospitalsSelected(SelectedHospitalType.Caption),
                    IsHospitalType = true
                },

                new FilterDefinition
                {
                    Caption = "Custom Hospitals",
                    Filter = item => (item as Hospital).HospitalsSelected(SelectedHospitalType.Caption),
                    IsHospitalType = true
                }
            };
        }

        /// <summary>
        /// Handles the Imported event of the HospitalImporter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HospitalImporter_Imported(object sender, EventArgs e)
        {
            EventAggregator.GetEvent<ResumeNormalProcessingEvent>().Publish(Empty.Value);
        }

        /// <summary>
        /// Handles the Importing event of the HospitalImporter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HospitalImporter_Importing(object sender, EventArgs e)
        {
            EventAggregator.GetEvent<PleaseStandByEvent>().Publish(new PleaseStandByEventPayload("Importing data..."));
        }

        /// <summary>
        /// Requeries the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        private void Requery(ISimpleImportCompletedPayload payload)
        {
            if (!payload.Inserted.Any()) return;

            if (payload.CountInserted > 0)
            {
                OnLoad();
            }
        }

        /// <summary>
        /// Creates the please stand by event payload.
        /// </summary>
        /// <returns></returns>
        private PleaseStandByEventPayload CreatePleaseStandByEventPayload()
        {
            return new PleaseStandByEventPayload
            {
                Message =
                    string.Format(
                        "Loading hospitals and custom regions based on your regional settings.{0}Please stand by...",
                        Environment.NewLine)
            };
        }

        #endregion
    }
}