using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;
using System.Windows.Data;
//using System.Windows.Forms;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.Hospitals;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Types;
using Monahrq.Websites.Events;
using PropertyChanged;
using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;
using Monahrq.Infrastructure.Types;

namespace Monahrq.Websites.ViewModels
{
    [Export(typeof(WebsiteMeasuresViewModel))]
    [ImplementPropertyChanged]
    public class WebsiteMeasuresViewModel : WebsiteTabViewModel //, IWebsiteMeasuresViewModel
    {

        #region Fields and Constants

        /// <summary>
        /// The manag e_ measures
        /// </summary>
        public const string MANAGE_MEASURES = "Manage Measures";
        /// <summary>
        /// The al l_ datasets
        /// </summary>
        public const string ALL_DATASETS = "All Datasets";
        /// <summary>
        /// The cancel
        /// </summary>
        public const string CANCEL = "Cancel";
        /// <summary>
        /// The save
        /// </summary>
        public const string SAVE = "SAVE";

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteMeasuresViewModel"/> class.
        /// </summary>
        public WebsiteMeasuresViewModel()
        {
            //Measures = new ObservableCollection<MeasureModel>();
            //SelectedMeasures = new ObservableCollection<MeasureModel>();

            new MeasuresFilter(this);

            BaseTitle = MANAGE_MEASURES;
            PropertyFilters = CollectionViewSource.GetDefaultView(
                                    new ObservableCollection<string> {
                                        ModelPropertyFilterValues.NONE,
                                        ModelPropertyFilterValues.MEASURE_CODE,
                                        ModelPropertyFilterValues.MEASURE_NAME,
                                        ModelPropertyFilterValues.WEBSITE_NAME,
                                        ModelPropertyFilterValues.TOPIC_NAME,
                                        ModelPropertyFilterValues.SUBTOPIC_NAME }) as ListCollectionView;

            if (PropertyFilters != null) PropertyFilters.MoveCurrentToFirst();
            SelectedDataSet = ALL_DATASETS;
            SelectedProperty = ModelPropertyFilterValues.NONE;

            BatchPropertyFilters = CollectionViewSource.GetDefaultView(
                        new ObservableCollection<string> {
                                        ModelPropertyFilterValues.NONE,
                                        ModelPropertyFilterValues.MEASURE_CODE,
                                        ModelPropertyFilterValues.MEASURE_NAME, 
									//	ModelPropertyFilterValues.WEBSITE_NAME, 
									//	ModelPropertyFilterValues.TOPIC_NAME
									}) as ListCollectionView;

            if (BatchPropertyFilters != null) BatchPropertyFilters.MoveCurrentToFirst();
            BatchSelectedDataSet = ALL_DATASETS;
            BatchSelectedProperty = ModelPropertyFilterValues.NONE;

            BatchAvailableMeasures = new ObservableCollection<BatchMeasureModel>();
            AvailableMeasuresView = new MultiSelectCollectionView<WebsiteMeasure>(new List<WebsiteMeasure>());
            BatchAvailableMeasuresView = new MultiSelectCollectionView<BatchMeasureModel>(new List<BatchMeasureModel>());
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the edit selected measure command.
        /// </summary>
        /// <value>
        /// The edit selected measure command.
        /// </value>
        public DelegateCommand<WebsiteMeasure> EditSelectedMeasureCommand { get; set; }

        /// <summary>
        /// Gets or sets the cancel edit selected measure command.
        /// </summary>
        /// <value>
        /// The cancel edit selected measure command.
        /// </value>
        public DelegateCommand<string> CancelEditSelectedMeasureCommand { get; set; }

        /// <summary>
        /// Gets or sets the save selected measure command.
        /// </summary>
        /// <value>
        /// The save selected measure command.
        /// </value>
        public DelegateCommand<string> SaveSelectedMeasureCommand { get; set; }

        /// <summary>
        /// Gets or sets the navigate to details command.
        /// </summary>
        /// <value>
        /// The navigate to details command.
        /// </value>
        public DelegateCommand NavigateToDetailsCommand { get; set; }

        /// <summary>
        /// Gets or sets the apply data set filter command.
        /// </summary>
        /// <value>
        /// The apply data set filter command.
        /// </value>
        public DelegateCommand ApplyDataSetFilterCommand { get; set; }

        /// <summary>
        /// Gets or sets the batch apply data set filter command.
        /// </summary>
        /// <value>
        /// The apply data set filter command.
        /// </value>
        public DelegateCommand ApplyBatchDataSetFilterCommand { get; set; }

        /// <summary>
        /// Gets or sets the reset command.
        /// </summary>
        /// <value>
        /// The reset command.
        /// </value>
        public DelegateCommand ResetCommand { get; set; }

        /// <summary>
        /// Gets or sets the assign topics command.
        /// </summary>
        /// <value>
        /// The assign topics command.
        /// </value>
        public DelegateCommand AssignTopicsCommand { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand SearchCommand { get; set; }

        public DelegateCommand<object> EditBatchMeasuresCommand { get; set; }

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the measure service.
        /// </summary>
        /// <value>
        /// The measure service.
        /// </value>
        [Import]
        public IMeasureService MeasureService { get; set; }

        #endregion

        #region Methods.

        /// <summary>
		/// Initializes the commands.
		/// </summary>
		protected override void InitCommands()
        {
            base.InitCommands();

            EditSelectedMeasureCommand = new DelegateCommand<WebsiteMeasure>(EditSelectedMeasure, CanEdit);
            CancelEditSelectedMeasureCommand = new DelegateCommand<string>(CancelEditSelectedMeasure, CanEdit);
            SaveSelectedMeasureCommand = new DelegateCommand<string>(SaveSelectedMeasure, CanEdit);
            ResetCommand = new DelegateCommand(Reset);
            SearchCommand = new DelegateCommand(() => RaisePropertyChanged(() => IsAllSelected));
            EditBatchMeasuresCommand = new DelegateCommand<object>(EditBatchMeasures, CanEditBatchMeasures);
        }

        /// <summary>
        /// Initializes the properties.
        /// </summary>
        protected override void InitProperties()
        {
            //InitData();
            DataSetsCollectionView = CollectionViewSource.GetDefaultView(WebsiteDataService.GetDataSets().Where(ds => !ds.EqualsIgnoreCase("Physician Data")).ToObservableCollection()) as ListCollectionView;
            BatchDataSetsCollectionView = CollectionViewSource.GetDefaultView(WebsiteDataService.GetDataSets().Where(ds => !ds.EqualsIgnoreCase("Physician Data") && !ds.EqualsIgnoreCase("Nursing Home Compare Data")).ToObservableCollection()) as ListCollectionView;
			SelectedMeasureNameType = "Plain Title";
            FillCombos();
		}

        /// <summary>
        /// Initializes the data.
        /// </summary>
        protected void InitData()
        {
            var measures = new ObservableCollection<MeasureModel>();

            if (CurrentWebsite != null && CurrentWebsite.Datasets.Any())
            {
                measures = WebsiteDataService.GetMeasureViewModels(QueryExpression).ToObservableCollection();
                var dataSetTypedMeasures = measures.Where(m => CurrentWebsite.Datasets.Any(d => d.Dataset.ContentType.Name.EqualsIgnoreCase(m.DataSetName)))
                    .ToObservableCollection();

                measures = dataSetTypedMeasures;
            }

            MeasuresCollectionView = CollectionViewSource.GetDefaultView(measures) as ListCollectionView;

            new MeasuresFilter(this);
            BaseTitle = MANAGE_MEASURES;
            PropertyFilters = CollectionViewSource.GetDefaultView(
                                    new ObservableCollection<string> {
                                        ModelPropertyFilterValues.NONE,
                                        ModelPropertyFilterValues.MEASURE_CODE,
                                        ModelPropertyFilterValues.MEASURE_NAME,
                                        ModelPropertyFilterValues.WEBSITE_NAME,
                                        ModelPropertyFilterValues.TOPIC_NAME }) as ListCollectionView;

            if (PropertyFilters != null) PropertyFilters.MoveCurrentToFirst();
            SelectedDataSet = ALL_DATASETS;
            SelectedProperty = ModelPropertyFilterValues.NONE;


            BatchPropertyFilters = CollectionViewSource.GetDefaultView(
                                    new ObservableCollection<string> {
                                        ModelPropertyFilterValues.NONE,
                                        ModelPropertyFilterValues.MEASURE_CODE,
                                        ModelPropertyFilterValues.MEASURE_NAME, 
									//	ModelPropertyFilterValues.WEBSITE_NAME, 
									//	ModelPropertyFilterValues.TOPIC_NAME
									}) as ListCollectionView;

            if (BatchPropertyFilters != null) BatchPropertyFilters.MoveCurrentToFirst();
            BatchSelectedDataSet = ALL_DATASETS;
            BatchSelectedProperty = ModelPropertyFilterValues.NONE;
			SelectedMeasureNameType = "Plain Title";
		}

        private void SelectedItemsChanged(object sender, EventArgs e)
        {
            var overallMeasure = sender as WebsiteMeasure;
            //if (overallMeasure != null && !overallMeasure.OriginalMeasure.Name.Contains("NH-OA-01")) {
            //OveralNursingHomeMeasureSelection();
            if (overallMeasure != null && overallMeasure.OriginalMeasure.Name.Contains("NH-OA-01"))
            {
                MessageBox.Show("'Overall Rating' top measure can not be excluded.");
                overallMeasure.SelectedChanged -= SelectedItemsChanged;
                overallMeasure.IsSelected = true;
                overallMeasure.SelectedChanged += SelectedItemsChanged;
            }


            var tempCount = TotalSelectedItemsCount;
            TotalSelectedItemsCount = AvailableMeasuresView.SourceCollection.OfType<WebsiteMeasure>().Count(item => item.IsSelected);
            ViewableSelectedItemsCount = AvailableMeasuresView.OfType<WebsiteMeasure>().Count(item => item.IsSelected);
            RaisePropertyChanged(() => SelectedMeasures);
            RaisePropertyChanged(() => IsAllSelected);

            if (TotalSelectedItemsCount > tempCount)
                CurrentWebsite.ActivityLog.Add(new ActivityLogEntry("Measures selected and/or updated", DateTime.Now));
            else if (TotalSelectedItemsCount < tempCount)
                CurrentWebsite.ActivityLog.Add(new ActivityLogEntry("Measures removed and updated", DateTime.Now));

            EditBatchMeasuresCommand.RaiseCanExecuteChanged();
        }

        private void SelectedBatchItemsChanged(object sender, EventArgs e)
        {
            var overallMeasure = sender as BatchMeasureModel;

            PopulateCollectiveBatchMeasureData();
            RaisePropertyChanged(() => BatchEditingMeasure);
            RaisePropertyChanged(() => BatchSuppressionSelectionInfoMessage);
            RaisePropertyChanged(() => BatchSuppressionMessage);
            RaisePropertyChanged(() => BatchSelectedMeasures);
            RaisePropertyChanged(() => BatchViewableSelectedMeasures);
            RaisePropertyChanged(() => IsAllBatchSelected);
        }
        //private void OveralNursingHomeMeasureSelection()
        //{
        //    var overallMeasureCodes = new List<string> { "NH-HI-01", "NH-QM-01", "NH-SD-01" };
        //    var nursingHomeMeasures = AvailableMeasures.Where(x => x.OriginalMeasure is NursingHomeMeasure && overallMeasureCodes.Contains(x.OriginalMeasure.MeasureCode));

        //    var overallMeasure = AvailableMeasures.FirstOrDefault(x => x.OriginalMeasure.Name.Contains("NH-OA-01"));
        //    if (overallMeasure == null) return;

        //    overallMeasure.IsSelected = nursingHomeMeasures.All(x => x.IsSelected);
        //}

        public override void RefreshUIElements()
        {
            base.RefreshUIElements();

            Reset(); // Reset filter
                     //BatchReset();
            RaisePropertyChanged(() => SelectedMeasures);
            RaisePropertyChanged(() => BatchSelectedMeasures);
            RaisePropertyChanged(() => BatchViewableSelectedMeasures);
        }

        private void RefreshAvailableMeasures()
        {
            AvailableMeasuresView = CurrentWebsite.Measures.RemoveNullValues().ToMultiSelectListCollectionView();
            AvailableMeasuresView.OfType<WebsiteMeasure>().ToList().ForEach(x =>
            {
                x.SelectedChanged -= SelectedItemsChanged;
                x.SelectedChanged += SelectedItemsChanged;
            });
            //AvailableMeasuresView = AvailableMeasures.ToMultiSelectListCollectionView();
            ((System.Collections.Specialized.INotifyCollectionChanged)AvailableMeasuresView).CollectionChanged += (s, e) =>
                {
                ViewableSelectedItemsCount = AvailableMeasuresView.OfType<WebsiteMeasure>().Count(item => item.IsSelected);
                //AvailableMeasuresView.OfType<WebsiteMeasure>().Count(item => item.IsSelected);
                    RaisePropertyChanged(() => IsAllSelected);
                };


            ApplyDataSetFilterCommand.Execute();
            AvailableMeasuresView.Refresh();
        }
        private void RefreshBatchSelectedMeasures()
        {
            using (new Sdk.Model.DeferPropertyChangedEvents(this))
            {
                BatchAvailableMeasures = AvailableMeasuresView.SelectedItems//.OfType<WebsiteMeasure>()
                                            //.Where(awm => awm.IsSelected)
                                            //	.Select(wm => new BatchMeasureModel(GenMeasureModel(wm, true)))		//	MOD: Commented Out
											.Select(wm => new BatchMeasureModel(wm))							//	MOD: Replace
                                            .ToObservableCollection();

                BatchAvailableMeasures.ToList().ForEach(x =>
                {
                    x.SelectedChanged -= SelectedBatchItemsChanged;
                    x.SelectedChanged += SelectedBatchItemsChanged;
                });
                BatchAvailableMeasuresView = BatchAvailableMeasures.ToMultiSelectListCollectionView();
                ((System.Collections.Specialized.INotifyCollectionChanged)BatchAvailableMeasuresView).CollectionChanged += (s, e) =>
                {
                    PopulateCollectiveBatchMeasureData();
                    RaisePropertyChanged(() => BatchEditingMeasure);
                    RaisePropertyChanged(() => IsAllBatchSelected);
                    RaisePropertyChanged(() => BatchSuppressionMessage);
                    RaisePropertyChanged(() => BatchSuppressionSelectionInfoMessage);
                };


                ApplyBatchDataSetFilterCommand.Execute();
                BatchAvailableMeasuresView.Refresh();
            }
        }
        private void PopulateCollectiveBatchMeasureData()
        {
            // Fill in the 'Collective' BatchSelectedMeasure.   Only populate properties where all fields are equal in batchMeasures.

            BatchEditingMeasure.NumeratorOverride = BatchViewableSelectedMeasures.GetCollectiveValue(x => x.ProxyWebsiteMeasure.ReportMeasure.SuppressionNumerator.ToString(), null, " ").ConvertTo<string>();
            BatchEditingMeasure.DenominatorOverride = BatchViewableSelectedMeasures.GetCollectiveValue(x => x.ProxyWebsiteMeasure.ReportMeasure.SuppressionDenominator.ToString(), null, " ").ConvertTo<string>();
            BatchEditingMeasure.PerformMarginSuppressionOverride = BatchViewableSelectedMeasures.GetCollectiveValue<BatchMeasureModel, bool?>(x => x.ProxyWebsiteMeasure.ReportMeasure.PerformMarginSuppression, null, false);


            //BatchEditingMeasure.NumeratorOverride = BatchViewableSelectedMeasures.GetCollectiveValue(x => x.ProxyMeasureModel.NumeratorOverride, null, " ").ConvertTo<string>();
            //BatchEditingMeasure.DenominatorOverride = BatchViewableSelectedMeasures.GetCollectiveValue(x => x.ProxyMeasureModel.DenominatorOverride, null, " ").ConvertTo<string>();
            //BatchEditingMeasure.PerformMarginSuppressionOverride = BatchViewableSelectedMeasures.GetCollectiveValue<BatchMeasureModel, bool?>(x => x.ProxyMeasureModel.PerformMarginSuppression, null, false);
            //
            //BatchEditingMeasure.IsMinScaleByRadioButtonChecked = BatchViewableSelectedMeasures.GetCollectiveValue<BatchMeasureModel, bool?>(x => (bool?)x.ProxyMeasureModel.IsMinScaleByRadioButtonChecked, null);
            //BatchEditingMeasure.IsMediumScaleByRadioButtonChecked = BatchViewableSelectedMeasures.GetCollectiveValue<BatchMeasureModel, bool?>(x => (bool?)x.ProxyMeasureModel.IsMediumScaleByRadioButtonChecked, null);
            //BatchEditingMeasure.IsMaxScaleByRadioButtonChecked = BatchViewableSelectedMeasures.GetCollectiveValue<BatchMeasureModel, bool?>(x => (bool?)x.ProxyMeasureModel.IsMaxScaleByRadioButtonChecked, null);
        }

        /// <summary>
        /// Fills the combos.
        /// </summary>
        public void FillCombos()
        {
            Audiences = WebsiteDataService.Audiences.Select(o => new SelectListItem { Value = ((Audience)o.Value).ToString(), Text = o.Name, Model = o }).ToObservableCollection();
            Audiences.RemoveAt(0);

            #region old code
            //Audiences.Insert(0, new SelectListItem { Value = null, Text = DEFAULT_AUDIENCE_SELECT_TEXT, Model = null });

            //Years = BaseDataService.ReportingYears.Select(o => new SelectListItem { Value = o, Text = o, Model = o }).ToObservableCollection();
            //Years.Insert(0, new SelectListItem { Value = null, Text = DEFAULT_YEAR_SELECT_TEXT, Model = null });

            //var quarters = BaseDataService.ReportingQuarters;
            ////quarters.RemoveAt(0);
            //Quarters = quarters.Select(o => new SelectListItem { Value = o.Id, Text = o.Text, Model = o }).ToObservableCollection();
            //Quarters.Insert(0, new SelectListItem { Value = (int?)null, Text = DEFAULT_QUARTER_SELECT_TEXT, Model = null });
            //Quarters = CollectionViewSource.GetDefaultView(BaseDataService.ReportingQuarters) as ListCollectionView;
            #endregion
        }

        /// <summary>
        /// Refreshes the topics collection.
        /// </summary>
        /// <param name="id">The identifier.</param>
        private void RefreshTopicsCollection(int id)
        {
            TopicsCollectionView = CollectionViewSource.GetDefaultView(WebsiteDataService.GetTopicViewModels().ToObservableCollection()) as ListCollectionView;
            MeasuresCollectionView = CollectionViewSource.GetDefaultView(WebsiteDataService.GetMeasureViewModels(QueryExpression).ToObservableCollection()) as ListCollectionView;
            if (MeasuresCollectionView != null) MeasuresCollectionView.MoveCurrentToFirst();
        }

        private void HydrateTopics(Measure measure, bool doUnselectAlso = true)
        {
            if (measure == null)
                return;

            foreach (var topicViewModel in TopicsCollectionView.Cast<TopicViewModel>())
            {
                if (topicViewModel.ChildrenCollectionView == null || !topicViewModel.ChildrenCollectionView.Any())
                    continue;

                foreach (var viewModel in topicViewModel.ChildrenCollectionView.OfType<SubTopicViewModel>().ToList())
                {
                    if (measure.Topics.Any(t => t.Id == viewModel.Id))
                    {
                        viewModel.IsSelected = true;
                    }
                    else if (doUnselectAlso)
                    {
                        viewModel.IsSelected = false;
                    }
                }
            }
        }

        private void UnselectTopics()
        {
            if (TopicsCollectionView == null) return;

            foreach (TopicViewModel topic in TopicsCollectionView)
            {
                topic.IsSelected = false;

                if (topic.ChildrenCollectionView == null) continue;

                foreach (SubTopicViewModel subtopic in topic.ChildrenCollectionView)
                {
                    subtopic.IsSelected = false;
                }
            }
        }

        private void ReverseHydrateTopics(Measure measure)
        {
            if (measure == null) return;
            measure.ClearTopics();

            foreach (var topicViewModel in TopicsCollectionView.Cast<TopicViewModel>())
            {
                if (topicViewModel.ChildrenCollectionView == null || !topicViewModel.ChildrenCollectionView.Any())
                    continue;

                foreach (var selectedTopic in topicViewModel.SelectedTopics) //.ChildrenCollectionView.OfType<SubTopicViewModel>().ToList())
                {
                    measure.AddTopic(selectedTopic);
                }
            }
        }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            Index = 2;

            WebsiteEditMeasureViewModel.ManageViewModel = ManageViewModel;
            AddSubListTabViewModel(new SubListTabViewModel(WebsiteEditMeasureViewModel) { SyncStateActions = true });
        }



        public override void Save()
        {
            InternalSave(true);
        }
        private void InternalSave(bool refreshUI)
        {
            // Update
            //kz This code is all over the VMs need to move to the BaseViewModel (Maybe After the Alpha )
            string message = string.Empty;
            if (!WebsiteDataService.SaveOrUpdateWebsite(CurrentWebsite))
            {
                message = CurrentWebsite.IsPersisted
                                ? String.Format("Website {0} has been updated", CurrentWebsite.Name)
                                : String.Format("Website {0} has been added", CurrentWebsite.Name);

                //EventAggregator.GetEvent<ForceTabSaveEvent>().Publish(false);
            }

            if (!string.IsNullOrEmpty(message))
            {
                if (refreshUI) RefreshUIElements();
                var eventArgs = new ExtendedEventArgs<GenericWebsiteEventArgs>
                {
                    Data = new GenericWebsiteEventArgs { Website = base.ManageViewModel.WebsiteViewModel, Message = message }
                };

                EventAggregator.GetEvent<WebsiteCreatedOrUpdatedEvent>().Publish(eventArgs);
            }
            UnselectTopics();
        }

        public override void Continue()
        {

            //EventAggregator.GetEvent<UpdateWebsiteTabContextEvent>().Publish(
            //     new UpdateTabContextEventArgs()
            //     {
            //         WebsiteViewModel = base.ManageViewModel.WebsiteViewModel,
            //         ExecuteViewModel = WebsiteTabViewModels.Measures
            //     });

        }

        /// <summary>
        /// Creates a *useable* MeasureModel from a WebsiteMeasure.
        /// </summary>
        /// <param name="websiteMeasure"></param>
        /// <param name="isBatchLoading">
        /// Indicates that we are loading multiple MeasuresModels.  As such, currently no need to hydrate 
        /// topics.  If we ever add topics to Batch editing, we can should add a BatchHydrateTopics function
        /// that would be called at this functions call site, after all batched MeasureModes have been 
        /// generated.  Also, the TopicTree IsSelection type should be changed from bool to bool?, thus 
        /// introducing a 3 state true, false, mixed IsSelected indicator, where mixed means some of the 
        /// batched Measures selected the topic, and some
        /// have not.
        /// </param>
        /// <returns></returns>
        private MeasureModel GenMeasureModel(WebsiteMeasure websiteMeasure, bool isBatchLoading = false)
        {
            var newMeasureModel = new MeasureModel(websiteMeasure);
            newMeasureModel.InitMeasureOverride(newMeasureModel.MeasureOverwrite ?? newMeasureModel.Measure);
            newMeasureModel.IsPlainTitleSelected = (newMeasureModel.MeasureOverwrite.IsPersisted)
                                        ? newMeasureModel.MeasureOverwrite.MeasureTitle.Selected == SelectedMeasuretitleEnum.Plain
                                        : CurrentWebsite.Audiences.Any(a => a == Audience.Consumers);
            //: CurrentWebsite.Audience == Audience.Consumers;
            newMeasureModel.WasMeasureOverrideEdited = true;
            if (!isBatchLoading) HydrateTopics(newMeasureModel.MeasureOverwrite);
            return newMeasureModel;
        }

        /// <summary>
        /// Saves the Measure Model
        /// </summary>
        /// <param name="measureToSave"></param>
        private void SaveMeasureModel(MeasureModel measureToSave, bool isBatchLoading = false)
        {
            if (!isBatchLoading) measureToSave.UpdateForMeasureOverride();
            //measureToSave.ReconcileTopics(this, measureToSave.MeasureOverwrite);
            measureToSave.WasMeasureOverrideEdited = true;

            measureToSave.MeasureOverwrite.MeasureTitle.Selected = (measureToSave.IsPlainTitleSelected)
                                                                                ? SelectedMeasuretitleEnum.Plain
                                                                                : SelectedMeasuretitleEnum.Clinical;
            if (!isBatchLoading) ReverseHydrateTopics(measureToSave.MeasureOverwrite);

            WebsiteDataService.SaveMeasureOverride(measureToSave.MeasureOverwrite, (result, error) =>
            {
                if (error == null)
                {
                    var msg = string.Format("Measure \"{0}\" was successfully overwritten. Please save website \"{1}\".", measureToSave.MeasureOverwrite.MeasureTitle.Clinical, base.ManageViewModel.WebsiteViewModel.Website.Name);
                    EventAggregator.GetEvent<GenericNotificationEvent>().Publish(msg);
                    if (!isBatchLoading) HydrateTopics(measureToSave.MeasureOverwrite);
                    //EventAggregator.GetEvent<ForceTabSaveEvent>().Publish(true);
                    measureToSave.MeasureOverwrite = result;
                }
                else
                    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(error);
            });

            // InitData();
            if (measureToSave.WebsiteMeasure != null)
                measureToSave.WebsiteMeasure.OverrideMeasure = measureToSave.MeasureOverwrite;
        }

        public override void Refresh()
        {
            base.Refresh();
            IsTabVisited = true;

            var datasetNames = CurrentWebsite.Datasets.Select(d => d.Dataset.ContentType.Name).ToList();
            var availableMeasurs = ManageViewModel.AllAvailableMeasures.Where(x => datasetNames.Contains(x.DataSetName)).ToList();
            //var availableMeasurs = WebsiteDataService.GetMeasureViewModels(m => m.IsOverride == false && datasetNames.Contains(m.Owner.Name)).ToList();
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
                    CurrentWebsite.Measures.Add(new WebsiteMeasure { OriginalMeasure = item.Measure, OverrideMeasure = null, IsSelected = true });
                }
            }

            CurrentWebsite.Measures.ForEach(wm =>
            {
                if (!wm.IsPersisted && wm.ReportMeasure.Name.StartsWith("IP") && 
                    (wm.ReportMeasure.MeasureTitle.Clinical.ContainsIgnoreCase("median") ||
                     wm.ReportMeasure.MeasureTitle.Policy.ContainsIgnoreCase("median")))
                {
                    wm.IsSelected = false;
                }

                if(!wm.IsPersisted && wm.ReportMeasure.Name.EqualsIgnoreCase("CMS-OVERALL-STAR"))
                {
                    wm.IsSelected = false;
                }
            });

            RefreshAvailableMeasures();
            SelectedItemsChanged(null, null);
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset()
        {
            SelectedDataSet = DataSetsCollectionView.SourceCollection.OfType<string>().FirstOrDefault();
            SelectedProperty = PropertyFilters.SourceCollection.OfType<string>().FirstOrDefault();
            PropertyFilterText = string.Empty;
        }
        public void BatchReset()
        {
            BatchSelectedDataSet = BatchDataSetsCollectionView.SourceCollection.OfType<string>().FirstOrDefault();
            BatchSelectedProperty = BatchPropertyFilters.SourceCollection.OfType<string>().FirstOrDefault();
            BatchPropertyFilterText = string.Empty;
        }

        public void BatchSearchAction()
        {
            RaisePropertyChanged(() => IsAllBatchSelected);
        }

        /// <summary>
        /// Determines whether this instance can edit the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public bool CanEdit(object arg)
        {
            return true; // MeasuresCollectionView != null && MeasuresCollectionView.OfType<MeasureModel>().ToList().Any(x => x.IsSelectedForWebsiteAssignment);
        }

        /*This method is called in 3 occasions:
         1. When User clicks Assign Topic Buttons => Topic popup is set to Open PARAMETER:nullithin 30-days after getting care in the hospital for pneumonia
         2. Pop Up is open and user clicks cancel PARAMETER: Cancel
         3. Pop Up is open and used clicks Save PARAMETER: Save      
         */
        public void EditSelectedMeasure(WebsiteMeasure currentWebsiteMeasure)
        {
            //SelectedMeasure = GenMeasureModel(currentWebsiteMeasure);
            IsEditMeasureWindowOpen = true;

            WebsiteEditMeasureViewModel.AssignSelectedMeasure(currentWebsiteMeasure);
            WebsiteEditMeasureViewModel.MeasureSaved -= OnEditMeasureSaveHandler;
            WebsiteEditMeasureViewModel.MeasureEditCancelled -= OnEditMeasureCancelHandler;
            WebsiteEditMeasureViewModel.MeasureSaved += OnEditMeasureSaveHandler;
            WebsiteEditMeasureViewModel.MeasureEditCancelled += OnEditMeasureCancelHandler;
        }
        private void OnEditMeasureSaveHandler()
        {
            IsEditMeasureWindowOpen = false;
            WebsiteEditMeasureViewModel.MeasureSaved -= OnEditMeasureSaveHandler;
            WebsiteEditMeasureViewModel.MeasureEditCancelled -= OnEditMeasureCancelHandler;
        }
        private void OnEditMeasureCancelHandler()
        {
            IsEditMeasureWindowOpen = false;
            WebsiteEditMeasureViewModel.MeasureSaved -= OnEditMeasureSaveHandler;
            WebsiteEditMeasureViewModel.MeasureEditCancelled -= OnEditMeasureCancelHandler;
        }

        public void EditSelectedMeasureXXXXX(WebsiteMeasure currentWebsiteMeasure)
        {

            SelectedMeasure = null;

            TopicTypeEnum? topicType;

            IsMeasureTopicsEnabled = true;

            if (currentWebsiteMeasure.OriginalMeasure is NursingHomeMeasure)
            {
                topicType = TopicTypeEnum.NursingHome;
                IsMeasureTopicsEnabled = false;
            }
            else if (currentWebsiteMeasure.OriginalMeasure is HospitalMeasure)
                topicType = TopicTypeEnum.Hospital;
            else
                topicType = null;

            TopicsCollectionView = CollectionViewSource.GetDefaultView(WebsiteDataService.GetTopicViewModels(topicType).ToObservableCollection()) as ListCollectionView;

            //   if ("OPEN".EqualsIgnoreCase(action) || "EDIT".EqualsIgnoreCase(action))
            //   {
            //if (MessageBox.Show(@"When overwriting measures locally, this will save the website as well. Would you like to continue?",
            //                    @"Modification of measures for websites information", MessageBoxButtons.YesNo) == DialogResult.No)
            //    return;

            IsEditMeasureWindowOpen = true;


            //  var currentItem = AvailableMeasures.SingleOrDefault(m => m.Id.ToString().Equals(measureId) /*&& m.IsSelectedForWebsiteAssignment*/);
            //currentItem.IsSelectedForWebsiteAssignment = true;
            //MeasuresCollectionView.MoveCurrentTo(currentWebsiteMeasure);
            SelectedMeasure = GenMeasureModel(currentWebsiteMeasure);



            //  }
            #region old code
            //else if (CANCEL.EqualsIgnoreCase(paramArgs[0]) || "CLOSE".EqualsIgnoreCase(paramArgs[0]))
            //{
            //    IsMeasureWindowOpen = false;
            //    if (CANCEL.EqualsIgnoreCase(param) && SelectedMeasure != null)
            //    {
            //        SelectedMeasure.WasMeasureOverrideEdited = false;
            //    }
            //}
            //else
            //{
            //    IsMeasureWindowOpen = false;
            //    if ("SAVE".EqualsIgnoreCase(paramArgs[0]))
            //    {
            //        if (WebsiteViewModel.Website.Audience == Audience.AllAudiences)
            //        {
            //            SelectedMeasure.MeasureOverwrite.MeasureTitle.Selected.Value = SelectedMeasure.MeasureOverwrite.MeasureTitle.Plain.Value;
            //        }
            //        else if (WebsiteViewModel.Website.Audience == Audience.Consumers)
            //        {
            //            SelectedMeasure.MeasureOverwrite.MeasureTitle.Selected.Value = SelectedMeasure.MeasureOverwrite.MeasureTitle.Clinical.Value;
            //        }
            //    }
            //}
            #endregion
        }

        public void DeleteMeasureOverride(WebsiteMeasure wm)
        {
			// Validate deletion is valid.
            if (!wm.ReportMeasure.IsOverride)
                return;

			// Confirmation of deletion.
			//var warningMessage = string.Format("All custom changes for Measure \"{0}\" will be lost.  Are you sure you want to proceed?", wm.Name);
			var warningMessage = string.Format("All modifications will be lost.  Are you sure you want to proceed?", wm.Name);
			var result = MessageBox.Show(warningMessage, "Custom Measure Deletion Confirmation", MessageBoxButton.YesNo);

			if (result == MessageBoxResult.No ||
				result == MessageBoxResult.Cancel ||
				result == MessageBoxResult.None)
				return;

			// Delete Measure Override.
            this.WebsiteDataService.DeleteMeasureOverride(ref wm, (wmx, exception) =>
            {
                if (exception != null)
                    Logger.Write(exception);
            });

        }

        /// <summary>
        /// Cancels the edit selected measure.
        /// </summary>
        /// <param name="param">The parameter.</param>
        public void CancelEditSelectedMeasure(string param)
        {
			UnselectTopics();				// Clear all topic selections.
            SelectedMeasure = null;			// Clear Popup ViewModel.
            IsEditMeasureWindowOpen = false;	// Close Popup.
        }

        /// <summary>
        /// Saves the selected measure.
        /// </summary>
        /// <param name="param">The parameter.</param>
        public void SaveSelectedMeasure(string param)
        {
            var paramArgs = param.Split(new[] { ':' });
            string action = paramArgs[0];
            bool entireWebsiteSaveRequired = !SelectedMeasure.WebsiteMeasure.IsPersisted;
            //string measureId = paramArgs[1];

            IsEditMeasureWindowOpen = false;
            if ("SAVE".EqualsIgnoreCase(action))
            {
                SaveMeasureModel(SelectedMeasure);
            }

            // Clear all topic selections.
            UnselectTopics();

            // refresh the AvailableMeasures
            RefreshAvailableMeasures();

            // Save the ENTIRE WEBSITE!!!
            // Testers don't like functionality where Popup on 'saves' the edits to memory.  Want edits
            // committed to DB.
            //if (entireWebsiteSaveRequired)
            Save();
        }

        public void EditBatchMeasures(Object o)
        {
            using (ApplicationCursor.SetCursor(System.Windows.Input.Cursors.Wait))
            {
                // Show Batch edit window.
                IsBatchMeasureWindowOpen = true;

                // Clear currently BatchSelectedMeasure.
                // We are using the BatchSelectedMeasure as a shell to hold the 'collective' view of the currently selected Measures.
                BatchEditingMeasure = null;
                BatchEditingMeasure = new BatchMeasureModel();

                // Get Selected Measures.
                RefreshBatchSelectedMeasures();

                // No Measures selected.  This is already caught, but 
                if (!BatchAvailableMeasures.Any()) return;

                // Future - Topic Batch Editing


                // Fill in the 'Collective' BatchSelectedMeasure.   Only populate properties where all fields are equal in batchMeasures.
                PopulateCollectiveBatchMeasureData();
            }
        }

        public bool CanEditBatchMeasures(Object o)
        {
            // Can only batch edit if a Measure is selected.
            // ? Should this be only allow batch edit if multiple Measures are selected?
            return AvailableMeasuresView != null && AvailableMeasuresView.OfType<WebsiteMeasure>().Any(item => item.IsSelected);
        }

        public void CancelEditBatchMeasures()
        {
			UnselectTopics();					// Clear all topic selections.
			BatchEditingMeasure = null;			// Clear Batch Popup ViewModel.
			IsBatchMeasureWindowOpen = false;	// Close Batch Popup.
        }

        private void ShowPopupMessage(Point bpCenter, System.String message)
        {
            var tempWin = new Window()
            {
                Left = bpCenter.X,
                Top = bpCenter.Y,
                Width = 0,
                Height = 0,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
                Topmost = true
            };
            tempWin.Show(); tempWin.Hide();
            Xceed.Wpf.Toolkit.MessageBox.Show(
                tempWin,
                message,
                @"Save Batch Succeeded.",
                MessageBoxButton.OK);
            tempWin.Close();
        }

        public void SaveBatchMeasures()
        {


            // Check Measures Selected.
            if (!BatchViewableSelectedMeasures.Any())
            {
                var result =
                    System.Windows.MessageBox.Show(
                        @"There are no viewable and selected measures to update.",
                        @"Save Batch Failed.",
                        MessageBoxButton.OK);
                return;

                //var eventArgs = new ExtendedEventArgs<GenericWebsiteExEventArgs>
                //{
                //	Data = new GenericWebsiteExEventArgs
                //	{
                //		Website = base.ManageViewModel.WebsiteViewModel, 
                //		Message = "There are no viewable and selected measures to update.", 
                //		NotificationType = ENotificationType.Warning
                //	}
                //};
                //EventAggregator.GetEvent<WebsiteCreatedOrUpdatedExEvent>().Publish(eventArgs);
                //return;

                //EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(new Exception("There are no viewable and selected measures to update."));
                //return;
            }


            // Save Measures.
            foreach (var currentMeasure in BatchViewableSelectedMeasures)
            {
                var currentBatchMeasure = new BatchMeasureModel(GenMeasureModel(currentMeasure.ProxyWebsiteMeasure, true));
                var options = EConvertToOptions.AvoidNull | EConvertToOptions.UseDefault;

                currentBatchMeasure.ProxyMeasureModel.MeasureOverwrite.SuppressionNumerator = BatchEditingMeasure.NumeratorOverride.ConvertTo<decimal>(options, currentBatchMeasure.NumeratorOverride);
                currentBatchMeasure.ProxyMeasureModel.MeasureOverwrite.SuppressionDenominator = BatchEditingMeasure.DenominatorOverride.ConvertTo<decimal>(options, currentBatchMeasure.DenominatorOverride);
                currentBatchMeasure.ProxyMeasureModel.MeasureOverwrite.PerformMarginSuppression = BatchEditingMeasure.PerformMarginSuppressionOverride.ConvertTo<bool>(options, currentBatchMeasure.PerformMarginSuppressionOverride, false);

                //  Copied from MeasureModel::InitMeasureOverride
                //	- Why is it required to save these values in 2 locations within same object??
                currentBatchMeasure.ProxyMeasureModel.DenominatorOverride = currentBatchMeasure.ProxyMeasureModel.MeasureOverwrite.SuppressionDenominator.ToString();
                currentBatchMeasure.ProxyMeasureModel.NumeratorOverride = currentBatchMeasure.ProxyMeasureModel.MeasureOverwrite.SuppressionNumerator.ToString();
                currentBatchMeasure.ProxyMeasureModel.PerformMarginSuppression = currentBatchMeasure.ProxyMeasureModel.MeasureOverwrite.PerformMarginSuppression;

                //SaveMeasureModel(currentBatchMeasure,true);
                var websiteMeasure = ManageViewModel.WebsiteViewModel.Website.Measures.FirstOrDefault(wm =>
                        currentBatchMeasure.ProxyMeasureModel.MeasureOverwrite != null &&
                        wm.OriginalMeasure.Name.EqualsIgnoreCase(currentBatchMeasure.ProxyMeasureModel.MeasureOverwrite.Name));
                if (websiteMeasure != null)
                {
                    if (websiteMeasure.OriginalMeasure is NursingHomeMeasure) continue;

                    this.WebsiteDataService.SaveMeasureOverride(currentBatchMeasure.ProxyMeasureModel.MeasureOverwrite, (measure, exception) =>
                    {
                        if (exception == null)
                        {
                            websiteMeasure.OverrideMeasure = measure;
                        }
                        else
                        {
                            Logger.Write(exception);
                        }
                    });
                    // websiteMeasure.OverrideMeasure = currentBatchMeasure.MeasureOverwrite;
                    websiteMeasure.IsSelected = true;
                }
            }

            {
                //	Setup Save in background thread.
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (o, eq) =>
                    {
                        Application.Current.Dispatcher.Invoke(() => { ApplicationCursor.CurrentCursor().Push(System.Windows.Input.Cursors.Wait); });
                        {
                            try
                            {
                                IsBatchEditProgressIndicatorBusy = true;
                                BatchEditProgressIndicatorContent = "Saving Batch Measures";
								InternalSave(false);			// Save Measures.
								RefreshAvailableMeasures();		// Refresh Main Screen.
                                Application.Current.Dispatcher.BeginInvoke((Action)(() => { RefreshUIElements(); RefreshAvailableMeasures(); }));

                                // Update UI with Save Progress.
                                //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke((Action)(() => BatchEditProgressIndicatorContent = ""));
                            }
                            catch (Exception ex)
                            {
                                ex.GetType();
                            }
                        }
                        Application.Current.Dispatcher.Invoke((Action)(() => { ApplicationCursor.CurrentCursor().Pop(); }));
                    };
                worker.RunWorkerCompleted += (o, eq) =>
                    {
                        //	Save Completed.
                        IsBatchEditProgressIndicatorBusy = false;
                        BatchEditProgressIndicatorContent = "";

                        //  Notify user of successful Save.
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                //	var batchPopup = Application.Current.MainWindow.FindChild<Monahrq.Theme.Controls.xPopup>("BatchMeasurePopup");
                                var batchPopup = Application.Current.MainWindow.FindChild<Theme.Controls.PopupEx>("BatchMeasurePopup");
                                //	var batchPopup = Application.Current.MainWindow.FindChild<Xceed.Wpf.Toolkit.ChildWindow>("BatchMeasurePopup");
                                //	var batchPopupMB = Application.Current.MainWindow.FindChild<Xceed.Wpf.Toolkit.MessageBox>("BatchMeasurePopupMessageBox");
                                var batchPopupWin = FrameworkElementExtensions.GetParentWindow(batchPopup);// batchPopup.GetParentWindow();
                                                                                                           //	var batchPopupMBWin = FrameworkElementExtensions.GetParentWindow(batchPopupMB);
                                                                                                           //	var bpCenter = batchPopup.GetCenter(false);
                                var bpCenter = batchPopupWin.GetCenter(false);
                                bpCenter.X += batchPopup.HorizontalOffset;
                                bpCenter.Y += batchPopup.VerticalOffset;

                                //	Create Message.
                                //var bamvSelectedCount = BatchAvailableMeasuresView.OfType<BatchMeasureModel>().Count(bam => bam.IsSelected);
                                var bamvSelectedCount = BatchAvailableMeasuresView.SelectedItems.Count;
                                var mbFormat = bamvSelectedCount == 1 ?
                                        "{0} measure was updated." :
                                        "{0} measures were updated.";
                                var batchSaveMessage = string.Format(
                                        mbFormat,
                                        bamvSelectedCount,
                                        BatchAvailableMeasuresView.Count);

                                IsBatchMeasureWindowOpen = false;
                                RaisePropertyChanged(() => CurrentWebsite.Measures);
                                RaisePropertyChanged(() => ManageViewModel.WebsiteViewModel.Website.Measures);

                                //  Create a TopMost window that acts as host for MessageBox.
                                ShowPopupMessage(bpCenter, batchSaveMessage);

                                //batchPopupMB.Text = batchSaveMessage;
                                //batchPopupMB.Caption = @"Save Batch Succeeded.";
                                //batchPopupMB.ShowDialog();

                                //batchPopupMBWin.Topmost = true;
                                //batchPopupMB.ShowMessageBox(
                                //	batchSaveMessage,
                                //	@"Save Batch Succeeded.",
                                //	MessageBoxButton.OK);

                                //var messageBox = new Xceed.Wpf.Toolkit.MessageBox();
                                //var bpCenter = batchPopup.GetCenter(false);
                                //messageBox.Left = bpCenter.X - 50;
                                //messageBox.Top = bpCenter.Y - 50;
                                //messageBox.Parent = batchPopupWin ?? Application.Current.MainWindow;
                                //messageBox.Text =	batchSaveMessage;
                                //messageBox.Button = MessageBoxButton.OK;
                                //messageBox.ShowDialog();

                                //MessageBox.Show(
                                //	batchPopupWin ?? Application.Current.MainWindow,
                                //	batchSaveMessage,
                                //	@"Save Batch Succeeded.",
                                //	MessageBoxButton.OK);

                                //var eventArgs = new ExtendedEventArgs<GenericWebsiteEventArgs>
                                //{
                                //	Data = new GenericWebsiteEventArgs { Website = base.ManageViewModel.WebsiteViewModel, Message = "The measure batch save has completed successfully." }
                                //};
                                //EventAggregator.GetEvent<WebsiteCreatedOrUpdatedEvent>().Publish(eventArgs);
                            }));

                    };

                // Begin Save.
                worker.RunWorkerAsync();
            }
        }

        public override bool TabChanged()
        {
            return ValidateAllRequiredFields();
        }

        #endregion

        #region Properties.

        /// <summary>
        /// Gets or sets the topics collection view.
        /// </summary>
        /// <value>
        /// The topics collection view.
        /// </value>
        public ListCollectionView TopicsCollectionView { get; set; }
        /// <summary>
        /// Gets or sets the measures collection view.
        /// </summary>
        /// <value>
        /// The measures collection view.
        /// </value>
        public ListCollectionView MeasuresCollectionView { get; set; }

        /// <summary>
        /// Gets or sets the UI.
        /// </summary>
        /// <value>
        /// The UI.
        /// </value>
        public MeasuresUIModel UI { get; set; }

        public bool IsMeasureTopicsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the audiences.
        /// </summary>
        /// <value>
        /// The audiences.
        /// </value>
        public ObservableCollection<SelectListItem> Audiences { get; set; }

        //public ObservableCollection<WebsiteMeasure> AvailableMeasures { get; set; }

        public MultiSelectCollectionView<WebsiteMeasure> AvailableMeasuresView { get; set; }

        public int TotalSelectedItemsCount { get; set; }

        private int _viewableSelectedItemsCount;
        public int ViewableSelectedItemsCount
        {
            get { return _viewableSelectedItemsCount; }
            set
            {
                _viewableSelectedItemsCount = value;
                RaisePropertyChanged(() => ViewableSelectedItemsCount);
            }
        }

        #region Main Filter Properties.
        public string SelectedDataSet { get; set; }
        public string SelectedProperty { get; set; }
        private string _propertyFilterText;
        public string PropertyFilterText
        {
            get { return _propertyFilterText; }
            set
            {
                _propertyFilterText = value;
                RaisePropertyChanged(() => IsAllSelected);
            }
        }
        public ListCollectionView PropertyFilters { get; set; }

        /// <summary>
        /// Gets or sets the data sets collection view.
        /// </summary>
        /// <value>
        /// The data sets collection view.
        /// </value>
        public ListCollectionView DataSetsCollectionView { get; set; }

		private string _selectedMeasureNameType;
		public string SelectedMeasureNameType
		{
			get { return _selectedMeasureNameType; }
			set
			{
				if (_selectedMeasureNameType == value) return;
				_selectedMeasureNameType = value;
				//RaisePropertyChanged(() => MeasureNameColumnSortPath);
			}
		}
		#endregion

		#region Batch Filter Properties.
		public string BatchSelectedDataSet { get; set; }
        public string BatchSelectedProperty { get; set; }
        private string _batchPropertyFilterText;
        public string BatchPropertyFilterText
        {
            get { return _batchPropertyFilterText; }
            set
            {
                _batchPropertyFilterText = value;
                RaisePropertyChanged(() => IsAllBatchSelected);
            }
        }
        public ListCollectionView BatchPropertyFilters { get; set; }
        public ListCollectionView BatchDataSetsCollectionView { get; set; }
        #endregion

        #region Measures Variables.
        public MeasureModel SelectedMeasure { get; set; }
        public ObservableCollection<WebsiteMeasure> SelectedMeasures
        {
            get
            {
                return AvailableMeasuresView == null
                                        ? new ObservableCollection<WebsiteMeasure>()
                                        : AvailableMeasuresView.SelectedItems.ToObservableCollection();
            }
        }
        public bool IsAllSelected
        {
            get
            {
                //if (AvailableMeasuresView == null) return false;
                var totalCount = AvailableMeasuresView.Count;
                var selectedCount = AvailableMeasuresView.OfType<WebsiteMeasure>().Count(x => x.IsSelected);

                return selectedCount != 0 && selectedCount == totalCount;
            }
            set
            {
                foreach (var m in AvailableMeasuresView.OfType<WebsiteMeasure>().ToList())
                {
                    m.IsSelected = value;
                }

                // AvailableMeasuresView.Refresh();
                RaisePropertyChanged(() => SelectedMeasures);
                //RaisePropertyChanged(() => IsAllSelected);
            }
        }
        #endregion

        #region Batch Measure Variables.
        public BatchMeasureModel BatchEditingMeasure { get; set; }
        public ObservableCollection<BatchMeasureModel> BatchAvailableMeasures { get; set; }
        public MultiSelectCollectionView<BatchMeasureModel> BatchAvailableMeasuresView { get; set; }
        public string BatchSuppressionSelectionInfoMessage
        {
            get
            {
                return string.Format(
                    "Total Selected Measures: {0} / {1}     " +
                    "Viewable Selected Measures: {2} / {3}",
                    BatchAvailableMeasures.OfType<BatchMeasureModel>().Count(bam => bam.IsSelected),
                    BatchAvailableMeasures.Count,
                    BatchAvailableMeasuresView.OfType<BatchMeasureModel>().Count(bam => bam.IsSelected),
                    BatchAvailableMeasuresView.Count);
            }
        }
        public string BatchSuppressionMessage
        {
            get
            {
                return string.Format(
                    "{0} measures will be effected.",
                    BatchAvailableMeasuresView.OfType<BatchMeasureModel>().Count(bam => bam.IsSelected),
                    BatchAvailableMeasuresView.Count);
            }
        }
        public ObservableCollection<BatchMeasureModel> BatchSelectedMeasures
        {
            get
            {
                return BatchAvailableMeasures == null
                                        ? new ObservableCollection<BatchMeasureModel>()
                                        : BatchAvailableMeasures.Where(item => item.IsSelected).ToObservableCollection();
            }
        }
        public ObservableCollection<BatchMeasureModel> BatchViewableSelectedMeasures
        {
            get
            {
                return BatchAvailableMeasures == null
                                        ? new ObservableCollection<BatchMeasureModel>()
                                        : BatchAvailableMeasuresView.SelectedItems.ToObservableCollection();
            }
        }
        public bool IsAllBatchSelected
        {
            get
            {
                var totalCount = BatchAvailableMeasuresView.OfType<BatchMeasureModel>().Count();
                var selectedCount = BatchAvailableMeasuresView.SelectedItems.Count;

                return selectedCount != 0 && selectedCount == totalCount;
            }
            set
            {
                //var bamv = BatchAvailableMeasuresView.OfType<BatchMeasureModel>().Cast<Monahrq.Default.ViewModels.BaseNotify>();
                //using (new Monahrq.Sdk.Model.DeferPropertyChangedEvents(bamv))
                {
                    foreach (var m in BatchAvailableMeasuresView.OfType<BatchMeasureModel>().ToList())
                    {
                        m.IsSelected = value;
                        if(!BatchAvailableMeasuresView.SelectedItems.Contains(m))
                            BatchAvailableMeasuresView.SelectedItems.Add(m);
                    }
                    RaisePropertyChanged(() => BatchSelectedMeasures);
                    RaisePropertyChanged(() => BatchViewableSelectedMeasures);
                    RaisePropertyChanged(() => BatchSuppressionMessage);
                    RaisePropertyChanged(() => BatchSuppressionSelectionInfoMessage);
                }
            }
        }
        #endregion


        #region Measure Edit Properties.
        public bool IsEditMeasureWindowOpen { get; set; }
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        public WebsiteEditMeasureViewModel WebsiteEditMeasureViewModel { get; set; }
        #endregion

        public bool IsBatchEditProgressIndicatorBusy { get; set; }
        public string BatchEditProgressIndicatorContent { get; set; }
        public bool IsBatchMeasureWindowOpen { get; set; }

        public string TabTitle
        {
            get
            {
                return BaseTitle + " (" + MeasuresCollectionView.Count + ")";
            }
        }


        private string _headerText;
        public string HeaderText
        {
            get
            {
                _headerText = "Modify Measures";
                return _headerText;
            }
            set
            {
                _headerText = value;
            }
        }

        private Expression<Func<Measure, bool>> QueryExpression
        {
            get
            {
                var typeNames = string.Empty;
                foreach (var wd in CurrentWebsite.Datasets)
                {
                    typeNames += wd.Dataset.ContentType.Name + ",";
                }
                if (!string.IsNullOrEmpty(typeNames) && typeNames.EndsWith(","))
                    typeNames = typeNames.SubStrBeforeLast(",");

                Expression<Func<Measure, bool>> criteria = m => typeNames.Contains(m.Owner.Name) && !m.IsOverride;

                return criteria;
            }
        }


        #endregion


    }



    public static class ModelPropertyFilterValues
    {
        public const string NONE = "None";
        public const string MEASURE_CODE = "Measure Code";
        public const string MEASURE_NAME = "Measure Name";
        public const string WEBSITE_NAME = "Website Name";
        public const string TOPIC_NAME = "Topic";
        public const string SUBTOPIC_NAME = "Sub-Topic";
    }
}
