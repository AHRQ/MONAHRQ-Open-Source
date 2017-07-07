using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Win32;
using Monahrq.DataSets.HospitalRegionMapping.Events;
using Monahrq.DataSets.HospitalRegionMapping.Mapping;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Generators;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Services.Hospitals;
using Monahrq.Infrastructure.Services.Regions;
using Monahrq.Infrastructure.Types;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Services.Import;
using Monahrq.Sdk.ViewModels;
using NHibernate;
using NHibernate.Exceptions;
using NHibernate.Linq;
using PropertyChanged;
using MonahrqRegion = Monahrq.Infrastructure.Domain.Regions.Region;

namespace Monahrq.DataSets.HospitalRegionMapping.Regions
{
    [Export, ImplementPropertyChanged]
    public class RegionsViewModel : ListTabViewModel<MonahrqRegion>
    {
        #region Fields and Constants

        private int? _regionId;
        private string _regionTitle;
        private ObservableCollection<string> _stateAbCollection;
        private string _selectedState;
        private List<string> _filters;
        private string _searchText;
        private string _selectedFilter;

        #endregion

        #region Commands

        public DelegateCommand AddCustomRegionCommand { get; set; }

        public DelegateCommand ImportRegionCommand { get; set; }

        public DelegateCommand ImportPopulationCommand { get; set; }

        public DelegateCommand ExportPopulationCommand { get; set; }

        public DelegateCommand DeleteRegionCommand { get; set; }

        public DelegateCommand CloseNewCustomRegionPopUpCommand { get; set; }

        public DelegateCommand SaveCustomRegion { get; set; }

        #endregion

        #region Imports

        [Import]
        public IRegionDataService RegionDataService { get; set; }

        [Import(ImporterContract.CustomRegion)]
        public IEntityFileImporter RegionImporter { get; set; }

        [Import(ImporterContract.RegionsPopulation)]
        public IEntityFileImporter PopulationImporter { get; set; }

        [Import]
        IHospitalRegistryService Service { get; set; }

        #endregion

        #region Constructor

        public RegionsViewModel()
        {
        }

        #endregion

        #region Properties

        public bool IsNavigating { get; set; }

        [Required(ErrorMessage = @"Please enter a region title")]
        public string RegionTitle
        {
            get { return _regionTitle; }
            set
            {
                _regionTitle = value;
                RaisePropertyChanged(() => RegionTitle);
                ValidateName(ExtractPropertyName(() => RegionTitle), RegionTitle);
                //CommitCommand.RaiseCanExecuteChanged();
            }
        }

        [Required(ErrorMessage = @"Please enter a region  ID")]
        [CustomValidation(typeof(RegionsViewModel), "IsUniqueCustomRegion")]
        public int? RegionID
        {
            get { return _regionId; }
            set
            {
                _regionId = value;
                RaisePropertyChanged(() => RegionID);
                //_validateName(ExtractPropertyName(() => RegionID), RegionID);
                //CommitCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<string> StateABCollection
        {
            get { return _stateAbCollection; }
            set
            {
                _stateAbCollection = value;

                RaisePropertyChanged(() => StateABCollection);
            }
        }

        [Required(ErrorMessage = @"Please select a state")]
        public string SelectedState
        {
            get { return _selectedState; }
            set
            {
                _selectedState = value;
                RaisePropertyChanged(() => SelectedState);
                //CommitCommand.RaiseCanExecuteChanged();
            }
        }

        public string PopulationSampleFile { get { return "Region Population Import Sample.csv"; } }

        public string RegionSampleFile { get { return "Region Import Sample.csv"; } }

        public string ImportButtonCaption { get { return "IMPORT REGIONS"; } }

        public int MappedCustomRegionToPopulationCount { get; set; }

        public bool IsAddNewRegionPopupVisible { get; set; }

        public MappingViewModel Parent { get; set; }

        public List<string> Filters
        {
            get { return _filters ?? new List<string> { "Region Name", "State" }; }
            set { _filters = value; }
        }

        public string SelectedFilter
        {
            get { return _selectedFilter ?? Filters[0]; }
            set { _selectedFilter = value; }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                CollectionItems.Filter = null;
                if (string.IsNullOrEmpty(_searchText)) return;

                CollectionItems.Filter += o =>
                {
                    var region = o as MonahrqRegion;
                    if (region == null) return false;
                    return SelectedFilter == Filters[0] ? region.Name.ToLower().Contains(_searchText.ToLower()) : region.State.ToLower().Contains(_searchText.ToLower());
                };
            }
        }

        #endregion

        #region Methods

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            EventAggregator.GetEvent<EntityImportedEvent<CustomRegion>>().Subscribe(OnCustomRegionImported);
            EventAggregator.GetEvent<GeographicalContextChangeEvent>().Subscribe(empty => RefreshCollection());

            RegionImporter.Imported +=
                delegate
                {
                    EventAggregator.GetEvent<ResumeNormalProcessingEvent>().Publish(Empty.Value);
                };

            RegionImporter.Importing +=
                delegate
                {
                    EventAggregator.GetEvent<PleaseStandByEvent>().Publish(RegionImporter.CreatePleaseStandByEventPayload());
                };

            EventAggregator.GetEvent<SimpleImportCompletedEvent>().Subscribe(Requery);

            EventAggregator.GetEvent<ContextAppliedEvent>().Subscribe(s =>
            {
                if (s.Equals("Saved") && IsActive) OnLoad();
            });
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            IsNavigating = true;
            try
            {
                RefreshCollection();
                EventAggregator.GetEvent<RegionsViewModelReadyEvent>().Publish(this);
            }
            finally
            {
                IsNavigating = false;
            }
        }

        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //todo
        }

        protected override void InitCommands()
        {
            base.InitCommands();
            AddCustomRegionCommand = new DelegateCommand(OnAddCustomRegionImport, CanNewRegion);
            ImportRegionCommand = new DelegateCommand(OnRegionImport, CanImport);
            ImportPopulationCommand = new DelegateCommand(OnPopulationImport, CanImportPopulation);
            ExportPopulationCommand = new DelegateCommand(OnPopulationExport, () => true);
            DeleteRegionCommand = new DelegateCommand(OnRegionDelete, CanDelete);
            CloseNewCustomRegionPopUpCommand = new DelegateCommand(OnCancel);
            SaveCustomRegion = new DelegateCommand(OnSaveCustomRegion);
        }

        protected override void InitProperties()
        {
            base.InitProperties();
            Index = 2;
        }

        protected override void ExecLoad(ISession session)
        {
            //var regionResultTemp = new List<MonahrqRegion>();
            //var regionCustomTemp = new List<MonahrqRegion>();

            //HospitalRegion.Default.SelectedStates.ForEach(s =>
            //{
            //    regionCustomTemp.AddRange(session.Query<CustomRegion>().Where(x => s.Abbreviation == x.State)
            //                                     .Cacheable()
            //                                     .CacheRegion("CustomRegion:" + s.Abbreviation)
            //                                     .ToList());
            //    regionCustomTemp.ForEach(c => c.HospitalCount = session.CreateSQLQuery(string.Format("select distinct(count(h.Id)) from {0} h where h.CustomRegion_Id = {1}", typeof(Hospital).EntityTableName(), c.Id)).UniqueResult<int>());

            //    if (HospitalRegion.Default.SelectedRegionType == typeof(HealthReferralRegion))
            //    {
            //        var hrr = session.Query<HealthReferralRegion>().Where(x => s.Abbreviation == x.State)
            //                         .Cacheable()
            //                         .CacheRegion("HealthReferralRegion:" + s.Abbreviation)
            //                         .ToList();
            //        hrr.ForEach(c => c.HospitalCount = session.CreateSQLQuery(string.Format("select distinct(count(h.Id)) from {0} h where h.HealthReferralRegion_Id = {1}", typeof(Hospital).EntityTableName(), c.Id)).UniqueResult<int>());
            //        regionResultTemp.AddRange(hrr);
            //    }
            //    else if (HospitalRegion.Default.SelectedRegionType == typeof(HospitalServiceArea))
            //    {
            //        var hsa = session.Query<HospitalServiceArea>().Where(x => s.Abbreviation == x.State)
            //                         .Cacheable()
            //                         .CacheRegion("HospitalServiceArea:" + s.Abbreviation)
            //                         .ToList();
            //        hsa.ForEach(c => c.HospitalCount = session.CreateSQLQuery(string.Format("select distinct(count(h.Id)) from {0} h where h.HospitalServiceArea_Id = {1}", typeof(Hospital).EntityTableName(), c.Id)).UniqueResult<int>());
            //        regionResultTemp.AddRange(hsa);
            //    }
            //});
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            CollectionItems = Service.GetRegions(session, configService.HospitalRegion.DefaultStates.OfType<string>().ToArray(),
                                                 configService.HospitalRegion.SelectedRegionType).ToListCollectionView();

            //MergeCustomRegions(regionResultTemp, regionCustomTemp);//Service.GetStates(HospitalRegion.Default.DefaultStates.OfType<string>().ToArray())
            StateABCollection = new ObservableCollection<string>(configService.HospitalRegion.DefaultStates.OfType<string>().ToList());
            MappedCustomRegionToPopulationCount = Service.GetCustomRegionToPopulationMappingCount(ConfigurationService.HospitalRegion.DefaultStates.OfType<string>().ToList());
            Reset();
        }

        private void MergeCustomRegions(IEnumerable<MonahrqRegion> selectedRegions, IEnumerable<MonahrqRegion> regions)
        {
            var mergedRegions = new List<MonahrqRegion>();
            mergedRegions.AddRange(selectedRegions.ToList());
            mergedRegions.AddRange(regions.ToList());

            CollectionItems = new ListCollectionView(mergedRegions);

            //CollectionItems = new ListCollectionView(selectedRegions);

            //regions.ForEach(x =>
            //{
            //    if (CollectionItems.OfType<MonahrqRegion>().All(r => r.Id != x.Id))
            //    {
            //        CollectionItems.AddNewItem(x);
            //        CollectionItems.CommitNew();
            //    }
            //});
        }

        protected override void OnDelete(MonahrqRegion entity)
        {
            if (entity == null) return;

            if (MessageBox.Show(string.Format("Delete Region: {0}", entity.Name), "Delete Region?",
                        MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                return;

            if (entity.HospitalCount > 0)
            {
                MessageBox.Show(
                    string.Format("Please delete associations with hospitals before deleting region \"{0}\". There are currently {1} hospital(s) associated with this region.", entity.Name, entity.HospitalCount),
                    "Delete Region?", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.No); return;
            }

            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    var updateQuery = string.Format(@"UPDATE {0} SET {1}_Id = NULL WHERE {1}_Id = {2} AND IsDeleted = 1", typeof(Hospital).EntityTableName(), entity.GetType().Name, entity.Id);
                    session.CreateSQLQuery(updateQuery).ExecuteUpdate();
                    session.Flush();
                    trans.Commit();
                }
            }

            CollectionItems.Remove(entity);
            base.OnDelete(entity);

            CollectionItems.Refresh();
            MappedCustomRegionToPopulationCount = Service.GetCustomRegionToPopulationMappingCount(ConfigurationService.HospitalRegion.DefaultStates.OfType<string>().ToList());
        }

        protected override void OnEdit(MonahrqRegion entity)
        {
            base.OnEdit(entity);
            entity.IsEditing = true;
        }

        protected override void OnSaveSelectedItem(MonahrqRegion entity, bool showSuccessConfirmation = true)
        {
            entity.IsEditing = false;
            base.OnSaveSelectedItem(entity);
        }

        protected override void OnCancelSelectedItem(MonahrqRegion entity)
        {
            base.OnCancelSelectedItem(entity);
            entity.IsEditing = false;
        }

        protected bool CanCommit(object argument)
        {
            if (string.IsNullOrWhiteSpace((RegionTitle ?? string.Empty).Trim())) return false;
            return SelectedState != null;
        }

        private void Requery(ISimpleImportCompletedPayload payload)
        {
            //MappedCustomRegionToPopulationCount = HospitalRegistryService.GetCustomRegionToPopulationMappingCount();

            //if (!payload.Inserted.OfType<CustomRegion>().Any()) return;
            //Func<MonahrqRegion, RegionViewModel> vcmFactory = (reg) => new RegionViewModel(Controller, reg);

            //if (payload.CountInserted > 0)
            //{
            //    Items = new ObservableCollection<RegionViewModel>(Controller.Regions.Select(region => vcmFactory(region.Region)));
            //    ItemsView = new ListCollectionView(Items);
            //}
        }

        private void OnCustomRegionImported(MonahrqRegion newRegion)
        {
            //var vm = new RegionViewModel(Controller, newRegion);
            //Items.Add(vm);
            //CurrentItem = vm;
            MappedCustomRegionToPopulationCount = Service.GetCustomRegionToPopulationMappingCount(ConfigurationService.HospitalRegion.DefaultStates.OfType<string>().ToList());
        }

        private void OnRegionCollectionChanged(IEnumerable<MonahrqRegion> nRegionViewModels)
        {
            RefreshCollection();
        }

        public void RefreshCollection()
        {
            //StateABCollection = new ObservableCollection<State>(Controller.Service.GetStates(HospitalRegion.Default.DefaultStates.OfType<string>().ToArray())); //new ObservableCollection<State>(HospitalRegion.Default.SelectedStates);
            //var items = Controller.RefreshRegions().Items;

            //foreach (var item in items)
            //{
            //    item.Region.HospitalCount = Controller.Service.GetHospitalCountForRegion(item.Region.GetType().Name, item.Region.Id,
            //                                                                             item.Region.State.Id);
            //}
            ////var vms = items.Select(item => new RegionViewModel(Controller, item.Region));
            //Items = new ObservableCollection<RegionViewModel>(items);
            //ItemsView = new ListCollectionView(items);
            //MappedCustomRegionToPopulationCount = HospitalRegistryService.GetCustomRegionToPopulationMappingCount();
            //ItemsView.Refresh();
        }

        private void ValidateName(string property, string val)
        {
            //ClearErrors(property);
            //if (string.IsNullOrWhiteSpace(val))
            //{
            //    SetError(property, "Region name cannot be empty");
            //}
        }

        private void Reset()
        {
            RegionID = null;
            RegionTitle = string.Empty;
            SelectedState = null;
            RaisePropertyChanged(() => RegionID);
            RaisePropertyChanged(() => RegionTitle);
            RaisePropertyChanged(() => SelectedState);
            //ClearErrors(ExtractPropertyName(() => RegionTitle));
            //Committed = true;
        }

        protected void OnCommitted()
        {
            try
            {
                var newRegion = Service.CreateRegion();
                newRegion.Name = RegionTitle;
                newRegion.State = SelectedState;
                Service.Save(newRegion);
                //var vm = new RegionViewModel(Controller, newRegion);
                //Items.Add(vm);
                var msg = String.Format("Custom region {0} with state {1} has been added", newRegion.Name, newRegion.State);
                EventAggregator.GetEvent<GenericNotificationEvent>().Publish(msg);
                //                Controller.ReloadRegionalContext();
                Reset();
            }
            catch (Exception ex)
            {
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
            }
        }

        private bool CanDelete()
        {
            return true;
        }

        private bool CanImport()
        {
            return true;
        }

        private bool CanImportPopulation()
        {
            return true;
        }

        private bool CanNewRegion()
        {
            // The spec requires validation msgbox when user clicks Add New (otherwise users will be mystified why the Add New is disabled).
            return true;
            //return !Committed && !HasErrors && SelectedState != null;
        }

        private void OnRegionDelete()
        {
            //var regionToDelete = CurrentItem;

            //if (MessageBox.Show(
            //            string.Format("Delete Region: {0}", CurrentItem.Name),
            //            "Delete Region?",
            //            MessageBoxButton.YesNo,
            //            MessageBoxImage.Question,
            //            MessageBoxResult.No) == MessageBoxResult.No) return;


            //if (CurrentItem.Region.HospitalCount > 0)
            //{
            //    MessageBox.Show(
            //        string.Format("Please delete associations with hospitals before deleting region \"{0}\". There are currently {1} hospitals associated with this region.", CurrentItem.Name, CurrentItem.Region.HospitalCount),
            //        "Delete Region?",
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Exclamation,
            //        MessageBoxResult.No);
            //}

            //regionToDelete.DeleteRegionCommand.Execute();
            //Items.Remove(CurrentItem);
            //ItemsView.Remove(CurrentItem);
            //MappedCustomRegionToPopulationCount = HospitalRegistryService.GetCustomRegionToPopulationMappingCount();
            //ItemsView.Refresh();
        }

        void OnAddCustomRegionImport()
        {
            IsAddNewRegionPopupVisible = true;
        }

        private void OnPopulationImport()
        {
            PopulationImporter.Execute();
            MappedCustomRegionToPopulationCount = Service.GetCustomRegionToPopulationMappingCount(ConfigurationService.HospitalRegion.DefaultStates.OfType<string>().ToList());
        }

        private void OnPopulationExport()
        {
            var fileName = string.Format("{0}_RegionsPopulation_{1:yyyyMMdd}.csv"
                , string.Join("_", ConfigurationService.HospitalRegion.DefaultStates.OfType<string>().ToList())
                , DateTime.Now);

            var exportPath = MonahrqContext.FileExportsDirPath;

            if (Directory.Exists(exportPath))
                Directory.CreateDirectory(exportPath);

            //Open File Dialog so that user can point to the Export folder and file..
            var dlg = new SaveFileDialog
            {
                CheckFileExists = false,
                Filter = @"CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                AddExtension = true,
                FileName = fileName,
                InitialDirectory = MonahrqContext.FileExportsDirPath
            };

            //if (!Directory.Exists(dlg.InitialDirectory)) Directory.CreateDirectory(dlg.InitialDirectory);

            //..but quit Export if user changes its mind by pressing Cancel button
            if (dlg.ShowDialog() != true) return;

            //Manufacture a filename if none is informed
            if (string.IsNullOrEmpty(dlg.FileName))
                dlg.FileName = fileName;

            //Cue Export with a wait cursor (Export may be too fast to be observed)
            using (ApplicationCursor.SetCursor(Cursors.Wait))
            {
                //Export the Hospitals collection in memory
                //var hospitals = CollectionItems.SourceCollection as IList<Hospital>;
                var regionsPopulation = GetRegionsPopulations();
                if (regionsPopulation == null) return;

                CSVGenerator.Create(regionsPopulation, dlg.FileName);

                Notify(string.Format("Regions' Population exported to file {0}", dlg.FileName));
            }
        }

        private IList<RegionsPopulationModel> GetRegionsPopulations()
        {

            if (DataserviceProvider == null || DataserviceProvider.SessionFactory == null) return null;


            #region TSQL return
            //var results = session.CreateSQLQuery(
            //    "SELECT ImportRegionId as RegionId, CatId, CatVal" +
            //    "   , (CASE WHEN CatID = 2 THEN CatVal ELSE 0 END) as Sex" +
            //    "   , (CASE WHEN CatID = 1 THEN CatVal ELSE 0 END) as AgeGroup" +
            //    "   , (CASE WHEN CatID = 4 THEN CatVal ELSE 0 END) as Race" +
            //    "   , [year], Population" +
            //    "   FROM Regions AS R INNER JOIN Base_ZipCodeToHRRAndHSAs AS Z " +
            //    "       ON R.State = Z.State INNER JOIN Base_ZipCodeToPopulationStrats AS ZP" +
            //    "           ON Z.Zip = ZP.ZipCode" +
            //    "   WHERE (R.RegionType = 'CustomRegion')" +
            //    "       AND (R.State IN ('CA','FL','TX','VT','NH'))" +
            //    "SELECT ImportRegionId as RegionId, CatId, CatVal" +
            //    "   , (CASE WHEN CatID = 2 THEN CatVal ELSE 0 END) as Sex" +
            //    "   , (CASE WHEN CatID = 1 THEN CatVal ELSE 0 END) as AgeGroup" +
            //    "   , (CASE WHEN CatID = 4 THEN CatVal ELSE 0 END) as Race" +
            //    "   , [year], Population" +
            //    "   FROM regions R  inner join [RegionPopulationStrats] RP " +
            //    "       ON R.id = RP.RegionID" +
            //    "   WHERE (R.RegionType <> 'CustomRegion') " +
            //    "   AND state in ('CA','FL','TX','VT','NH')" +
            //    ""
            //    ).AddEntity(typeof(RegionsPopulationModel)).List<RegionsPopulationModel>();
            #endregion

            var statesList = ConfigurationService.HospitalRegion.DefaultStates.OfType<string>().ToList();
            List<RegionsPopulationModel> regionPopulation;
            List<RegionsPopulationModel> customRegionPopulation;

            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                int regionType;
                switch (ConfigurationService.HospitalRegion.SelectedRegionType.Name.ToUpper())
                {
                    case "HOSPITALSERVICEAREA":
                        regionType = 2;
                        break;
                    case "HEALTHREFERRALREGION":
                        regionType = 1;
                        break;
                    default:
                        regionType = 0;
                        break;
                }
                // var regionPopStratsQuery = session.Query<RegionPopulationStrats>().Where(rps => rps.RegionType == regionType);

                //regionPopulation = session.Query<MonahrqRegion>().Where(r => statesList.Contains(r.State))
                //                                                 .Join(regionPopStratsQuery, r => r.ImportRegionId, rp => rp.RegionID, (r, rp) => new RegionsPopulationModel
                //                                        {
                //                                            RegionId = r.ImportRegionId,
                //                                            Sex = (rp.CatID == 2 ? rp.CatVal : 0),
                //                                            AgeGroup = (rp.CatID == 1 ? rp.CatVal : 0),
                //                                            Race = (rp.CatID == 4 ? rp.CatVal : 0),
                //                                            Year = rp.Year,
                //                                            Population = rp.Population
                //                                        }).ToList();

                regionPopulation = session.Query<MonahrqRegion>().Where(r => statesList.Contains(r.State))
                    .Where(r => r.RegionType.ToUpper() == ConfigurationService.HospitalRegion.SelectedRegionType.Name.ToUpper())
                    .Join(session.Query<RegionPopulationStrats>(), r => r.ImportRegionId, rp => rp.RegionID, (r, rp) => new RegionsPopulationModel
                    {
                        RegionId = r.ImportRegionId,
                        Sex = (rp.CatID == 2 ? rp.CatVal : 0),
                        AgeGroup = (rp.CatID == 1 ? rp.CatVal : 0),
                        Race = (rp.CatID == 4 ? rp.CatVal : 0),
                        Year = rp.Year,
                        Population = rp.Population
                    }).ToList();


                //customRegionPopulation = session.Query<MonahrqRegion>().Where(r => statesList.Contains(r.State))
                //    .Where(r => r.RegionType.ToUpper() == "CUSTOMREGION")
                //    .Join(session.Query<ZipCodeToHRRAndHSA>(), r => r.State, z => z.State, (r, z) => new {r, z})
                //    .Join(session.Query<ZipCodeToPopulationStrats>(), rz => rz.z.Zip, zp => zp.ZipCode, (rz, zp) => new RegionsPopulationModel
                //        {
                //            RegionId = rz.r.ImportRegionId,
                //            Sex = (zp.CatID == 2 ? zp.CatVal : 0),
                //            AgeGroup = (zp.CatID == 1 ? zp.CatVal : 0),
                //            Race = (zp.CatID == 4 ? zp.CatVal : 0),
                //            Year = zp.Year,
                //            Population = zp.Population
                //        }).ToList();
            }

            var regionPopulationList = new List<RegionsPopulationModel>(regionPopulation);
            // regionPopulationList.AddRange(customRegionPopulation);

            return regionPopulationList;

        }

        private void OnRegionImport()
        {
            RegionImporter.Execute();
            OnLoad();
        }

        protected override void OnCancel()
        {
            base.OnCancel();
            IsAddNewRegionPopupVisible = false;
            Reset();
        }

        private void OnSaveCustomRegion()
        {
            Validate();
            if (HasErrors) return;

            //if (string.IsNullOrWhiteSpace(RegionTitle) || RegionID == null || SelectedState == null)
            //{
            //    MessageBox.Show("Please enter a region  ID , name and select a state before adding a custom geographic region.",
            //        "Unable to add Custom Region", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //    return;
            //}

            var customRegion = new CustomRegion
            {
                Code = string.Format("CUS{0}{1}", RegionID, SelectedState),
                Name = RegionTitle,
                IsSourcedFromBaseData = false,
                State = SelectedState,
                Version = 1,
                Created = DateTime.Now,
                ImportRegionId = RegionID
            };
            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                try
                {
                    OnAddNewItem(customRegion);
                    CollectionItems.AddNewItem(customRegion);
                    CollectionItems.CommitNew();
                    Reset();
                    var msg = String.Format("Custom region {0} with state {1} has been added", customRegion.Name, customRegion.State);
                    MappedCustomRegionToPopulationCount = Service.GetCustomRegionToPopulationMappingCount(ConfigurationService.HospitalRegion.DefaultStates.OfType<string>().ToList());
                    EventAggregator.GetEvent<GenericNotificationEvent>().Publish(msg);
                    IsAddNewRegionPopupVisible = false;
                }
                catch (GenericADOException)
                {
                    MessageBox.Show("region  ID already exist.", "Unable to add Custom Region", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        public static ValidationResult IsUniqueCustomRegion(string regionId)
        {
            if (string.IsNullOrEmpty(regionId)) return ValidationResult.Success;

            var dataProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            using (var session = dataProvider.SessionFactory.OpenSession())
            {
                var query = string.Format("select * from [Regions] where RegionType='CustomRegion' and [ImportRegionId]={0}", regionId);
                var q = session.CreateSQLQuery(query);

                if (q.List().Count > 0)
                {

                    return new ValidationResult("Region ID must be unique. There exists a region with the same ID.", new List<string> { "RegionID" });
                }
            }
            return ValidationResult.Success;

        }
        #endregion
    }
}
