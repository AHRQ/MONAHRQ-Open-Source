using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using LinqKit;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Events;
using Monahrq.Websites.Events;
using Monahrq.Websites.Model;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.Websites.ViewModels
{
    [Export(typeof(WebsiteDatasetsViewModel))]
    [ImplementPropertyChanged]
    public class WebsiteDatasetsViewModel : WebsiteTabViewModel
    {
        #region Constructor

        public WebsiteDatasetsViewModel()
        {
        }

        #endregion

        #region Properties

        public ObservableCollection<InstalledDataset> InstalledDatasets { get; set; }

        private string _headerText;
        public string HeaderText
        {
            get
            { _headerText= "Choose Data Sets";
                return _headerText;
            }
            set
            {
                _headerText = value;
            }

        }

        #endregion

        #region Methods

        // clear WebsiteViewModel.Website.Datasets; populate it from the OC of selected datasets; save it
        public override void Save()
        {
            string message;
            bool errorsOccurredWhenSaving;

            ProcessWebsitesAndDatasets();

            if (CurrentWebsite.Datasets != null && CurrentWebsite.Datasets.Any())
            {
                //WebsiteViewModel.Website.CurrentStatus = WebsiteState.HasDatasources;
                base.ManageViewModel.WebsiteViewModel.Website.ActivityLog.Add(new ActivityLogEntry("Datsets selected and/or updated.", DateTime.Now));
            }
            //else
            //{
            //    WebsiteViewModel.Website.ActivityLog.Add(new ActivityLogEntry("Measures removed and/or updated.", DateTime.Now));
            //}

            if (!CurrentWebsite.IsPersisted)
            {
                errorsOccurredWhenSaving = WebsiteDataService.SaveNewWebsite(CurrentWebsite);
                message = String.Format("Website {0} has been added", CurrentWebsite.Name);
            }
            else
            {
                // If the website is edited, the current status must change in order to allow for the dependency check to be readily available when publishing
                base.ManageViewModel.WebsiteViewModel.Website.CurrentStatus = WebsiteState.Initialized;

                errorsOccurredWhenSaving = WebsiteDataService.UpdateWebsite(CurrentWebsite);
                message = String.Format("Website {0} has been updated", CurrentWebsite.Name);
            }
            // If no errors, move to the next tab
            if (!errorsOccurredWhenSaving)
            {
                EventAggregator.GetEvent<WebsiteCreatedOrUpdatedEvent>()
                               .Publish(new ExtendedEventArgs<GenericWebsiteEventArgs>(new GenericWebsiteEventArgs
                               {
                                   Website = base.ManageViewModel.WebsiteViewModel,
                                   Message = message
                               }));
                //EventAggregator.GetEvent<UpdateWebsiteTabContextEvent>().Publish(new UpdateTabContextEventArgs() { WebsiteViewModel = base.ManageViewModel.WebsiteViewModel });
            }
        }

        private void ProcessWebsitesAndDatasets()
        {
            if (CurrentWebsite == null || InstalledDatasets == null) return;

            CurrentWebsite.Datasets.Clear();

            // put the selected datasets into the website to be saved
            foreach (var ids in InstalledDatasets)
            {
                foreach (var wds in ids.WebsiteDatasets)
                {
                    //if ((wds.DataSetFile.Dataset != null && CurrentWebsite.Datasets.All(d => d.Dataset.Id != wds.DataSetFile.Dataset.Id)
                    // this is the simple object I think we SHOULD save for each website dataset...
                    //var dataSetFile = new DataSetFile(wds.DataSetFile.DatasetId, wds.DataSetFile.Name, wds.DataSetFile.Year, wds.DataSetFile.Quarter);

                    //if(CurrentWebsite.Datasets.All(ds => ds.Dataset.Id == ))
                    // BUT this is what the current infrastructure requires...
                    //var import = new DatasetImport {DatasetId = wds.DataSetFile.DatasetId};
                    //import.DatasetType = new TARGET() ?????????????????
                    //import.ImportDate = IMPORT DATE ???????????????
                    var websiteDataset = new WebsiteDataset();

                    if (wds.DataSetFile.Dataset != null)
                    {
                        websiteDataset.Dataset = wds.DataSetFile.Dataset;
                    }
                    else if (wds.DataSetFile.DatasetId > 0)
                    {
                        WebsiteDataService.GetEntityById<Dataset>(wds.DataSetFile.DatasetId, (record, exception) =>
                        {
                            if (exception == null && record != null)
                            {
                                websiteDataset.Dataset = record;
                            }
                        });
                    }

                    if (websiteDataset.Dataset != null &&
                        CurrentWebsite.Datasets.All(d => d.Dataset.Id != websiteDataset.Dataset.Id))
                    {
                        CurrentWebsite.Datasets.Add(websiteDataset);
                    }
                }
            }

            RaisePropertyChanged(() => CurrentWebsite.Datasets);
            RaisePropertyChanged(() => ManageViewModel.WebsiteViewModel.Website.Datasets);
        }

        public override void Continue()
        {
            ProcessWebsitesAndDatasets();
            // update available website measures
            var datasetNames = CurrentWebsite.Datasets.Select(d => d.Dataset.ContentType.Name).ToList();
            var availableMeasurs = WebsiteDataService.GetMeasureViewModels(m => m.IsOverride == false && datasetNames.Contains(m.Owner.Name)).ToList();
            CurrentWebsite.Measures = CurrentWebsite.Measures.RemoveNullValues().ToList();
            for (var i = CurrentWebsite.Measures.Count - 1; i >= 0; i--)
            {
                var item = CurrentWebsite.Measures[i];
                if (availableMeasurs.All(m => m.Measure.MeasureCode != item.ReportMeasure.MeasureCode))
                {
                    CurrentWebsite.Measures.Remove(item);
                }
            }
            foreach (var item in availableMeasurs)
            {
                if (CurrentWebsite.Measures.All(m => m.ReportMeasure.MeasureCode != item.Measure.MeasureCode))
                {
                    CurrentWebsite.Measures.Add(new WebsiteMeasure() { OriginalMeasure = item.Measure, OverrideMeasure = null, IsSelected = true });
                }
            }

            ListExtensions.ForEach(CurrentWebsite.Measures, wm =>
            {
                if (!wm.IsPersisted && wm.ReportMeasure.Name.StartsWith("IP") &&
                    (wm.ReportMeasure.MeasureTitle.Clinical.ContainsIgnoreCase("median") ||
                     wm.ReportMeasure.MeasureTitle.Policy.ContainsIgnoreCase("median")))
                {
                    wm.IsSelected = false;
                }
            });

            //EventAggregator.GetEvent<UpdateWebsiteTabContextEvent>().Publish(new UpdateTabContextEventArgs()
            //{
            //    WebsiteViewModel = base.ManageViewModel.WebsiteViewModel,
            //    ExecuteViewModel = WebsiteTabViewModels.Datasets
            //});
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected override void InitCommands()
        {
        }

        /// <summary>
        /// Initializes the properties.
        /// </summary>
        protected override void InitProperties()
        {

        }

        ///// <summary>
        ///// Determines whether this instance can execute.
        ///// </summary>
        ///// <returns></returns>
        //private bool CanExecute()
        //{
        //    return true;
        //}

        ///// <summary>
        ///// Executes the import command.
        ///// </summary>
        //private void ExecuteImportCommand()
        //{
        //    RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.WebsiteImportView, UriKind.Relative));
        //}

        public void LoadData()
        {
            // For each installed dataset, for each imported file, find the matches in the selected datasets saved in the website...
            try
            {

                // could move this into the loop and pass outer.Id, but here it's a single query
                var importedDatasets = WebsiteDataService.GetDatasetSummary();

                //if (InstalledDatasets == null || !InstalledDatasets.Any())
                //{
                var datasets = WebsiteDataService.GetInstalledDatasets();

                // create an OC of datasets for binding in the view
                InstalledDatasets = datasets.Distinct().Select(target => new InstalledDataset
                {
                    Id = target.Id,
                    IsTrendingEnabled = target.IsTrendingEnabled,
                    NameOfInstalledDataset = target.Name,
                    WebsiteViewModel = ManageViewModel.WebsiteViewModel,
                    Manager = ManageViewModel,
                    Events = EventAggregator
                }).ToObservableCollection();
                //}

                foreach (var outer in InstalledDatasets)
                {
                    // it should return an empty ToList if there are none, but just to be safe
                    if (!importedDatasets.Any(x => x.DatasetTypeId == outer.Id))
                    {
                        var specialRow = new ImportedFile(outer.Id, false, outer.NameOfInstalledDataset, "", "", "", "", ImportedFileRowTypes.NoneImported);
                        outer.ImportedFiles.Add(specialRow);
                    }
                    else
                    {
                        var specialRow = new ImportedFile(outer.Id, false, outer.NameOfInstalledDataset, "", "", "", "", ImportedFileRowTypes.PleaseSelect);
                        outer.ImportedFiles.Add(specialRow);

                        foreach (var inner in importedDatasets.Where(x => x.DatasetTypeId == outer.Id))
                        {
                            // This imported dataset is 'unselected' if there are no datasets in the website, or if none have id == inner.DatasetTypeId
                            bool selected = ManageViewModel.WebsiteViewModel != null && CurrentWebsite != null && CurrentWebsite.Datasets != null
                                && CurrentWebsite.Datasets.Any(d => d.Dataset.ContentType.Id == inner.DatasetTypeId && d.Dataset.Id == inner.DatasetImport.Id);

                            var name = string.Format("{0}: {1}", outer.NameOfInstalledDataset, inner.Name);
                            var websiteDataSet = (ManageViewModel.WebsiteViewModel != null && CurrentWebsite != null && CurrentWebsite.Datasets != null
                                && CurrentWebsite.Datasets.Any(ds => ds.Dataset.Id == inner.DatasetImport.Id))
                                                    ? CurrentWebsite.Datasets.SingleOrDefault(ds => ds.Dataset.Id == inner.DatasetImport.Id)
                                                    : null;


                            var file = new ImportedFile(inner.DatasetId, selected, name,
                                                        inner.Year.ToString(), inner.Quarter,
                                                        inner.VersionMonth, inner.VersionYear,
                                                        ImportedFileRowTypes.Normal,
                                                        websiteDataSet != null ? websiteDataSet.Dataset : null);

                            // Add selected files to the website list of datasets; add unselected files to the list of "available" files
                            if (selected)
                                outer.WebsiteDatasets.Add(file);
                            else
                                outer.ImportedFiles.Add(file);
                        }
                    }

                    // select the first row
                    outer.SelectedImportedFile = outer.ImportedFiles[0];
                }
            }
            catch (Exception ex)
            {
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            IsTabVisited = true;
            LoadData();

            // ProcessWebsitesAndDatasets();
        }

        public IEnumerable<DatasetSummary> GetDatasetSummary(int? id = null)
        {
            var datasets = new List<DatasetSummary>();

            var sessionProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            using (var session = sessionProvider.SessionFactory.OpenSession())
            {
                var criteria = id == null
                                   ? PredicateBuilder.True<Dataset>()
                                   : PredicateBuilder.False<Dataset>();

                criteria = criteria.Or(item => item.Id == id.GetValueOrDefault());

                var initialQuery = session.Query<Dataset>().Where(criteria);


                // Types
                var types = initialQuery.Where(item => item.ContentType != null)
                                        .Select(type => type.ContentType).ToList();

                //var datasetImports = session.Query<DatasetImport>().Where(d)

                datasets.AddRange(from item in initialQuery.ToList()
                                  let type = types.FirstOrDefault(t => t.Id == item.ContentType.Id)
                                  select new DatasetSummary(item, type));

                //var datasets = (from item in initialQuery
                //                join type in session.Query<ContentTypeRecord>()
                //                    on item.ContentType.Id equals type.Id
                //                join summary in session.Query<ContentItemSummaryRecord>()
                //                    on item.Summary.Id equals summary.Id
                //                select new DatasetSummary(item, type, summary, null)).ToList();
            }
            return datasets;
        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            Index = 1;
        }

        public override bool TabChanged()
        {
            base.TabChanged();

            ProcessWebsitesAndDatasets();

            return true;
        }

        public override void Reset()
        {
            base.Reset();

            if (InstalledDatasets != null && InstalledDatasets.Any())
            {
                ListExtensions.ForEach(InstalledDatasets, id =>
                {
                    if (id.WebsiteDatasets != null && id.WebsiteDatasets.Any())
                        id.WebsiteDatasets.Clear();

                   // RaisePropertyChanged(() => id.WebsiteDatasets);
                });

                //InstalledDatasets.Clear();
                RaisePropertyChanged(() => InstalledDatasets);
            }

            //if (ManageViewModel.WebsiteViewModel != null && ManageViewModel.WebsiteViewModel.Website != null)
            //{
            //    ManageViewModel.WebsiteViewModel.Website.Datasets = new List<WebsiteDataset>();
            //    RaisePropertyChanged(() => CurrentWebsite.Datasets);
            //    RaisePropertyChanged(() => ManageViewModel.WebsiteViewModel.Website.Datasets);
            //}

        }

        #endregion
    }

    public class InstalledDataset : BaseNotify
    {
        #region Fields and Constants

        IImportedFile _selectedImportedFile;

        #endregion

        #region Commands

        public ICommand AddFileCommand { get; set; }
        public ICommand RemoveFileCommand { get; set; }
        public ICommand ImportNewDataCommand { get; set; }

        #endregion

        #region Properties

        public WebsiteViewModel WebsiteViewModel { get; set; }

        public InstalledDataset()
        {
            ImportedFiles = new ObservableCollection<IImportedFile>();
            WebsiteDatasets = new ObservableCollection<IImportedFile>();

            AddFileCommand = new DelegateCommand(ExecuteAddFileCommand, CanExecuteAddFile);
            RemoveFileCommand = new DelegateCommand<object>(ExecuteRemoveFileCommand, CanExecuteRemove);
            ImportNewDataCommand = new DelegateCommand(ExecuteImportNewDataCommand, CanExecuteImportNew);
        }

        public int Id { get; set; }

        public bool IsTrendingEnabled { get; set; }

        public bool IsFileSelectable
        {
            get
            {
                return SelectedImportedFile.RowType == ImportedFileRowTypes.Normal && (WebsiteDatasets.Count == 0 || IsTrendingEnabled);
            }
        }

        // property is long name to avoid binding confusion with child Name property
        public string NameOfInstalledDataset { get; set; }

        // ImportedFiles is the list of all available files that have been imported for this installed dataset type
        public ObservableCollection<IImportedFile> ImportedFiles { get; set; }

        public IImportedFile SelectedImportedFile
        {
            get
            {
                return _selectedImportedFile;
            }
            set
            {
                _selectedImportedFile = value;
                RaisePropertyChanged(() => SelectedImportedFile);
            }
        }

        // WebsiteDatasets is the list of only the files (from available files) selected in this website
        public ObservableCollection<IImportedFile> WebsiteDatasets { get; set; }

        public WebsiteManageViewModel Manager { get; set; }

        #endregion

        #region Methods

        public bool CanExecuteImportNew()
        {
            return true;
        }

        public bool CanExecuteAddFile()
        {
            return true;
        }

        public bool CanExecuteRemove(object arg)
        {
            return true;
        }

        public void ExecuteImportNewDataCommand()
        {
            //tbd: for now go to data sets
            //var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            //if (regionManager == null) return;
            //regionManager.RequestNavigate(RegionNames.DataSetsRegion, new Uri(ViewNames.ManageMeasuresView, UriKind.Relative));

        }

        // The binding parameter is an ImportedFile
        public void ExecuteRemoveFileCommand(object param)
        {
            var file = param as IImportedFile;
            if (file == null) return;
            WebsiteDatasets.Remove(file);
            ImportedFiles.Add(file);

            // now there is at least 1 normal file in the dropdown, so show the Please message
            ImportedFiles[0].RowType = ImportedFileRowTypes.PleaseSelect;

            // always select the special row
            SelectedImportedFile = ImportedFiles[0];

            // ???
            file.IsSelected = WebsiteDatasets.Count > 0;
            Manager.IsNewDatasetIncluded = true;

            string datasetTypeName = null;
            if (file.DataSetFile.Dataset == null || file.DataSetFile.Dataset.ContentType == null)
            {
                var provider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
                using (var session = provider.SessionFactory.OpenStatelessSession())
                {
                    datasetTypeName = session.Query<Dataset>()
                                         .Where(d => d.Id == file.DataSetFile.DatasetId)
                                         .Select(d => d.ContentType.Name)
                                         .FirstOrDefault();
                }
            }
            else
            {
                datasetTypeName = file.DataSetFile.Dataset.ContentType.Name;

            }

            Manager.IsTrendingYearUpdated = !string.IsNullOrEmpty(datasetTypeName) && (datasetTypeName.Contains("Inpatient Discharge") || datasetTypeName.Contains("ED Treat And Release"));
        }

        // The binding parameter is an ImportedFile
        public void ExecuteAddFileCommand()
        {
            bool containsExistingDataSets = false;
            if (SelectedImportedFile == null) return;

            if (SelectedImportedFile.RowType != ImportedFileRowTypes.Normal) return;

            if (WebsiteViewModel.Website.Datasets.Any())
            {
                containsExistingDataSets = true;
            }
            WebsiteDatasets.Add(SelectedImportedFile);
            //var count = WebsiteViewModel.Website.Datasets.Count();

            if (containsExistingDataSets)
            {
                Application.DoEvents();
                Events.GetEvent<GenericNotificationEvent>().Publish("Dataset added. Please be sure to make relevant measure and report selections as you proceed");
            }
            //Events.GetEvent<SelectedItemsForNewlyAddedDatasets>().Publish(true);

            // ???
            SelectedImportedFile.IsSelected = WebsiteDatasets.Count > 0;

            // copy it before removing, then set selected to [0], then remove copy
            var temp = SelectedImportedFile;

            // always select the special row
            SelectedImportedFile = ImportedFiles[0];

            ImportedFiles.Remove(temp);

            // If no more normal files remain, replace the special Please item with the special All item
            if (!ImportedFiles.Any(f => f.RowType == ImportedFileRowTypes.Normal))
            {
                ImportedFiles[0].RowType = ImportedFileRowTypes.AllFilesAdded;
            }
            Manager.IsNewDatasetIncluded = true;
            var datasetType = SelectedImportedFile.DataSetFile.Name;
            Manager.IsTrendingYearUpdated = !string.IsNullOrEmpty(datasetType) && (datasetType.Contains("Inpatient Discharge") || datasetType.Contains("ED Treat And Release"));
        }

        #endregion

    }
}