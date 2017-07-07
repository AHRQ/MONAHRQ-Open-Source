using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LinqKit;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Annotations;
using Monahrq.DataSets.Events;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Services.SysTray;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Services;
using Monahrq.Sdk.Services.Tasks;
using Monahrq.Sdk.ViewModels;
using Monahrq.Theme.Controls.Wizard.Models;
using NHibernate.Linq;
using PropertyChanged;
using Monahrq.Sdk.Extensions;

namespace Monahrq.DataSets.ViewModels
{
    /// <summary>
    /// The dataset list tab view model
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    /// <seealso cref="Microsoft.Practices.Prism.Regions.INavigationAware" />
    /// <seealso cref="Monahrq.Sdk.ViewModels.ITabItem" />
    [Export]
    [ImplementPropertyChanged]
    public partial class DataSetListViewModel : INotifyPropertyChanged, IPartImportsSatisfiedNotification, INavigationAware, ITabItem
    {
        #region Fields and Constants

        /// <summary>
        /// The ip dataset refresh poller
        /// </summary>
        EntityPoller<Dataset> _ipDatasetRefreshPoller = null;
        /// <summary>
        /// The deletetoken
        /// </summary>
        SubscriptionToken _deletetoken;
        /// <summary>
        /// The update token
        /// </summary>
        SubscriptionToken _updateToken;
        /// <summary>
        /// The update status token
        /// </summary>
        SubscriptionToken _updateStatusToken;
        /// <summary>
        /// The cancelling token
        /// </summary>
        SubscriptionToken _cancellingToken;
        /// <summary>
        /// The process dataset DRG information token
        /// </summary>
        private SubscriptionToken _processDatasetDRGInfoToken;
        /// <summary>
        /// The is active
        /// </summary>
        private bool _isActive;
        /// <summary>
        /// Gets or sets the region MGR.
        /// </summary>
        /// <value>
        /// The region MGR.
        /// </value>
        private IRegionManager RegionMgr { get; set; }
        /// <summary>
        /// The selected data type
        /// </summary>
        private DataTypeModel _selectedDataType;
        /// <summary>
        /// The data types list
        /// </summary>
        private ObservableCollection<DataTypeModel> _dataTypesList;
        // public DelegateCommand<object> SelectedItemChangedCommand { get; set; }
        //private SubscriptionToken _refreshDatasetDRGInfoToken;

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the session provider.
        /// </summary>
        /// <value>
        /// The session provider.
        /// </value>
        IDomainSessionFactoryProvider SessionProvider { get; set; }

        /// <summary>
        /// Gets or sets the target lookup.
        /// </summary>
        /// <value>
        /// The target lookup.
        /// </value>
        IDictionary<string, Target> TargetLookup { get; set; }

        /// <summary>
        /// Gets or sets the type of the selected data.
        /// </summary>
        /// <value>
        /// The type of the selected data.
        /// </value>
        public DataTypeModel SelectedDataType
        {
            get { return _selectedDataType; }
            set
            {
                _selectedDataType = value;
                OnPropertyChanged("SelectedDataType");
            }
        }

        /// <summary>
        /// Gets or sets the data types list.
        /// </summary>
        /// <value>
        /// The data types list.
        /// </value>
        public ObservableCollection<DataTypeModel> DataTypesList
        {
            get { return _dataTypesList; }
            set
            {
                _dataTypesList = value;
                OnPropertyChanged("DataTypesList");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the name of the header.
        /// </summary>
        /// <value>
        /// The name of the header.
        /// </value>
        public string HeaderName { get; set; }

        /// <summary>
        /// Gets or sets the name of the region.
        /// </summary>
        /// <value>
        /// The name of the region.
        /// </value>
        public string RegionName { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is initial load.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is initial load; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialLoad { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the object is active.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if the object is active; otherwise <see langword="false" />.
        /// </value>
        public bool IsActive
        {
            get { return _isActive; ; }
            set
            {
                _isActive = value;
                OnIsActive();
            }
        }

        /// <summary>
        /// Notifies that the value for <see cref="P:Microsoft.Practices.Prism.IActiveAware.IsActive" /> property has changed.
        /// </summary>
        public event EventHandler IsActiveChanged;

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSetListViewModel"/> class.
        /// </summary>
        /// <param name="sessionProvider">The session provider.</param>
        [ImportingConstructor]
        public DataSetListViewModel(IDomainSessionFactoryProvider sessionProvider)
        {
            IsValid = true;
            SessionProvider = sessionProvider;
            RegionMgr = ServiceLocator.Current.GetInstance<IRegionManager>();
            ImportDataFileClick = new DelegateCommand<object>(ImportDataFileClickExecute, ImportDataFileClickCanExecute);
            EditDatasetMetadataCommand = new DelegateCommand<object>(OnEditDatasetMetadata, CanEditDatasetMetadata);
            HideDatasetMetadataPopUpCommand = new DelegateCommand(OnCloseEditDatasetMetadata, () => true);
            UpdateDatasetMetadataCommand = new DelegateCommand(OnUpdateDatasetMetadata, () => true);
        }

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        [Import]
        IEventAggregator Events { get; set; }

        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        IDatasetService Service { get; set; }

        /// <summary>
        /// Gets or sets the target deletion visitors.
        /// </summary>
        /// <value>
        /// The target deletion visitors.
        /// </value>
        [ImportMany]
        IEnumerable<ITargetDeletionVisitor> TargetDeletionVisitors { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        ILogWriter Logger { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the refresh command.
        /// </summary>
        /// <value>
        /// The refresh command.
        /// </value>
        public ICommand RefreshCommand { get; set; }

        /// <summary>
        /// Gets or sets the import data file click.
        /// </summary>
        /// <value>
        /// The import data file click.
        /// </value>
        public ICommand ImportDataFileClick { get; set; }

        /// <summary>
        /// Imports the data file click execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        private void ImportDataFileClickExecute(object arg)
        {
            SelectedDataSet = null;
            if (arg == null || SelectedDataType.DataTypeName.EqualsIgnoreCase("Medicare Provider Charge Data"))
            {
                RegionMgr.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.DataImportWizard, UriKind.Relative));
            }
            else
            {
                var selectEntry = arg as DataTypeDetailsViewModel;
                if (selectEntry != null)
                {
                    SelectedDataSet = new DatasetMetadataViewModel() { Dataset = selectEntry.Entry };
                }

                var attachedWebsites = Service.GetWebsitesForDataset(SelectedDataSet.Dataset.Id);
                if (attachedWebsites.Any())
                {
                    string websiteNames = string.Join(",", attachedWebsites);
                    var warningMessage = string.Format("This dataset \"{0}\" is already used in a website and re-importing the data may change the reports in the website. You must republish your website to include the updated data in your website. Do you want to proceed?", SelectedDataSet.Dataset.Name);
                    warningMessage += string.Format("{0}{0}Associate Website(s): {1}", Environment.NewLine, websiteNames);

                    if (MessageBox.Show(warningMessage, "Dataset ReImport Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        return;
                }

                RegionMgr.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.DataImportWizard, UriKind.Relative));
            }
        }

        /// <summary>
        /// Imports the data file click can execute.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        private bool ImportDataFileClickCanExecute(object arg)
        {
            return SelectedDataType != null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Subscribes this instance.
        /// </summary>
        public void Subscribe()
        {
            Unsubscribe();
            _deletetoken = Subscribe<DeleteEntryEvent, DeleteEntryEventArg>(OnDeleteDatasetItem);
            _updateToken = Subscribe<UpdateEntryEvent, Dataset>(UpdateItem);
            _updateStatusToken = Subscribe<UpdateDrgMdsStatusEvent, string>(UpdateDrgMdsStatus);
            _cancellingToken = Subscribe<WizardCancellingEvent, CancelEventArgs>(CancelPrompt, ThreadOption.PublisherThread);
            _processDatasetDRGInfoToken = Subscribe<ProcessDrgMdsDatasetInfoEvent, Dataset>(OnProcessDatasetDRGRecord);
        }

        /// <summary>
        /// Called when [delete dataset item].
        /// </summary>
        /// <param name="entry">The entry.</param>
        private void OnDeleteDatasetItem(DeleteEntryEventArg entry)
        {
            if (entry == null) return;
            DeleteItem(entry.Dataset, entry.ShowUserPrompt);
            if (SelectedDataSet != null) SelectedDataSet = null;
        }

        /// <summary>
        /// Determines whether this instance [can dataset be refreshed] the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can dataset be refreshed] the specified entry; otherwise, <c>false</c>.
        /// </returns>
        private bool CanDatasetBeRefreshed(Dataset entry)
        {
            return entry.DRGMDCMappingStatus != DrgMdcMappingStatusEnum.Completed ||
                   entry.DRGMDCMappingStatus != DrgMdcMappingStatusEnum.Error;
        }

        /// <summary>
        /// Called when [process dataset DRG record].
        /// </summary>
        /// <param name="item">The item.</param>
        private void OnProcessDatasetDRGRecord(Dataset item)
        {
            if (!item.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge")) return;
            if (!item.DRGMDCMappingStatus.In(new [] { DrgMdcMappingStatusEnum.Error, DrgMdcMappingStatusEnum.Pending })) return;
            
            item.DRGMDCMappingStatus = DrgMdcMappingStatusEnum.Intializing;
            Service.Save(item, (dataset, exception) =>
            {
                if (exception == null)
                {
                    item = (Dataset)dataset;
                }
            });

            DatasetSysTrayProcessor.ProcessDataset(item);

            UpdateDrgMdsStatus(item.Id.ToString());
        }

        /// <summary>
        /// Refreshes the ip dataset status.
        /// </summary>
        /// <param name="item">The item.</param>
        private void RefreshIPDatasetStatus(Dataset item)
        {
            if (item == null ||
                item.DRGMDCMappingStatus.In(new DrgMdcMappingStatusEnum[] { DrgMdcMappingStatusEnum.Completed, DrgMdcMappingStatusEnum.Error }))
                return;

            _ipDatasetRefreshPoller = new EntityPoller<Dataset>(o => o.Id == item.Id)
            {
                ActiveCycle = 200, //milliseconds
                IdleCycle = 30000, //30 seconds
                RetryAttempts = 5,
                RetryInitialInterval = 20000  //20 seconds
            };

            _ipDatasetRefreshPoller.EntityReceived += entity =>
            {
                using (var session = this.SessionProvider.SessionFactory.OpenSession())
                    session.Refresh(entity);
            };
            
            _ipDatasetRefreshPoller.Error += exception => Events.GetEvent<ErrorNotificationEvent>().Publish(exception);
            _ipDatasetRefreshPoller.Start();
        }

        /// <summary>
        /// Polls the entity status.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="reset">if set to <c>true</c> [reset].</param>
        /// <param name="terminate">if set to <c>true</c> [terminate].</param>
        private void PollEntityStatus(Dataset item, bool reset, bool terminate = false)
        {
            EntityPoller<Dataset> poller = null;
            if (reset)
            {
                poller = new EntityPoller<Dataset>(o => o.Id == item.Id)
                {
                    ActiveCycle = 200, //milliseconds
                    IdleCycle = 30000, //30 seconds
                    RetryAttempts = 5,
                    RetryInitialInterval = 20000  //20 seconds
                };
               
                poller.EntityReceived += entity =>
                {
                    var datasetType = DataTypesList.OfType<DataTypeModel>().FirstOrDefault(
                        e => e.DataTypeName.ToLower() == item.ContentType.Name.ToLower());

                    if (datasetType != null && datasetType.RecordsList.Any(dr => dr.Entry.Id == entity.Id))
                    {
                        var dsItem = datasetType.RecordsList.FirstOrDefault(dr => dr.Entry.Id == entity.Id);

                        if (dsItem != null)
                            dsItem.Entry = entity;
                    }
                };
                poller.Error += exception => Events.GetEvent<ErrorNotificationEvent>().Publish(exception);
                poller.Start();
            }

            if (terminate && poller != null)
            {
                poller.Stop();
            }
        }


        /// <summary>
        /// Updates the DRG MDS status.
        /// </summary>
        /// <param name="contentITemRecordId">The content i tem record identifier.</param>
        private void UpdateDrgMdsStatus(string contentITemRecordId)
        {
            if (string.IsNullOrEmpty(contentITemRecordId))
                return;

            int entryId;
            if (int.TryParse(contentITemRecordId, out entryId))
            {
                var current = SelectedDataType.RecordsList.FirstOrDefault(item => item.Entry.Id == entryId);
                if (current != null)
                {
                    SelectedDataType.RecordsList.Where(item => item.Entry.Id != entryId).ToList().ForEach(item => item.EnableGrouperProcessing = false);
                    var idx = SelectedDataType.RecordsList.IndexOf(current);
                    if (idx >= 0)
                    {
                        this.Service.Refresh(current.Entry);
                        SelectedDataType.RecordsList[idx] = new DataTypeDetailsViewModel(current.Entry);
                        PollEntityStatus(current.Entry, true);
                    }
                }
            }
        }

        /// <summary>
        /// Executes the delete item.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void ExecuteDeleteItem(Dataset obj)
        {
            using (new WaitCursor())
            {
                DeleteItem(obj);
                LoadData();
            }
        }

        /// <summary>
        /// Cancels the prompt.
        /// </summary>
        /// <param name="evnt">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void CancelPrompt(CancelEventArgs evnt)
        {
            var stop = MessageBox.Show("Cancel Import?", "Cancel",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            evnt.Cancel = stop;
        }

        /// <summary>
        /// Updates the item.
        /// </summary>
        /// <param name="entry">The entry.</param>
        private void UpdateItem(Dataset entry)
        {
            var current = SelectedDataType.RecordsList.FirstOrDefault(item => item.Entry.Id == entry.Id);
            if (current == null)
            {
                SelectedDataType.RecordsList.Add(new DataTypeDetailsViewModel(entry));
            }
            else
            {
                var idx = SelectedDataType.RecordsList.IndexOf(current);
                if (idx >= 0)
                {
                    SelectedDataType.RecordsList[idx] = new DataTypeDetailsViewModel(entry);
                }
            }
        }

        /// <summary>
        /// Subscribes the specified arguments.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <typeparam name="TPayload">The type of the payload.</typeparam>
        /// <param name="args">The arguments.</param>
        /// <param name="threadOption">The thread option.</param>
        /// <returns></returns>
        SubscriptionToken Subscribe<TEvent, TPayload>(Action<TPayload> args, ThreadOption threadOption = ThreadOption.UIThread)
            where TEvent : CompositePresentationEvent<TPayload>, new()
        {
            return Events.GetEvent<TEvent>().Subscribe(args, threadOption);
        }

        /// <summary>
        /// Unsubscribes the specified token.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="token">The token.</param>
        void Unsubscribe<T>(SubscriptionToken token) where T : EventBase, new()
        {
            var temp = Interlocked.Exchange(ref token, null);
            if (temp != null)
            {
                Events.GetEvent<T>().Unsubscribe(token);
            }
        }

        /// <summary>
        /// Unsubscribes this instance.
        /// </summary>
        public void Unsubscribe()
        {
            Unsubscribe<DeleteEntryEvent>(_deletetoken);
            Unsubscribe<UpdateEntryEvent>(_updateToken);
            Unsubscribe<UpdateDrgMdsStatusEvent>(_updateStatusToken);
            Unsubscribe<WizardCancellingEvent>(_cancellingToken);
        }

        /// <summary>
        /// Deletes the item.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="showDeletePrompt">if set to <c>true</c> [show delete prompt].</param>
        private void DeleteItem(Dataset entry, bool showDeletePrompt = true)
        {
            try
            {
                if (TargetLookup == null || !TargetLookup.Any())
                    return;

                if (TargetLookup[entry.ContentType.Name] == null)
                    return;

                if (!TargetLookup[entry.ContentType.Name].IsCustom && string.IsNullOrEmpty(TargetLookup[entry.ContentType.Name].ClrType))
                    return;

                if (showDeletePrompt)
                {
                    var attachedWebsites = Service.GetWebsitesForDataset(entry.Id);
                    if (attachedWebsites.Any())
                    {
                        string websiteNames = string.Join(",", attachedWebsites);
                        var warningMessage = string.Format("Unable to delete dataset \"{0}\" because it is associated with the following websites:", entry.Name);
                        warningMessage += string.Format("{0}{0}Associated Websites: {1}", Environment.NewLine, websiteNames);

                        MessageBox.Show(warningMessage, "Dataset Deletion Warning", MessageBoxButton.OK);
                        return;
                    }

                    //  Show blanket warning against dataset deletion.  But only show warning if this Dataset is Finished.
                    if (entry.IsFinished)
                    {
                        var warningMessage = string.Format("Dataset \"{0}\" will be deleted.  Are you sure you want to delete this data set?", entry.Name);
                        var result = MessageBox.Show(warningMessage, "Dataset Deletion Warning", MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.No ||
                            result == MessageBoxResult.Cancel ||
                            result == MessageBoxResult.None)
                            return;
                    }
                }

                if (DataTypesList == null)
                {
                    return;
                }

                foreach (var dataTypeModel in DataTypesList.Where(dataTypeModel => dataTypeModel.RecordsList.Any(ds => ds.Entry.Id == entry.Id)).ToList())
                {
                    dataTypeModel.RecordsList.ToList().RemoveAll(ds => ds.Entry.Id != entry.Id);
                }

                if (SelectedDataType != null)
                {
                    var item = SelectedDataType.RecordsList.FirstOrDefault(model => model.Entry.Id == entry.Id);
                    if (item != null)
                    {
                        SelectedDataType.RecordsList.Remove(item);
                    }
                }

                using (var session = SessionProvider.SessionFactory.OpenSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        session.Evict(entry);

                        var target = TargetLookup[entry.ContentType.Name];

                        #region Delete queries
                        string targetDeleteQuery;

                        var tableName = target.IsCustom ? entry.ContentType.DbSchemaName : entry.ContentType.ClrType != null ? Type.GetType(entry.ContentType.ClrType).EntityTableName() : string.Empty;

                        if (string.IsNullOrEmpty(tableName)) return;

                        //Disable Dataset Constraint 
                        //var disableConstraint = string.Format("ALTER TABLE  {0} NOCHECK CONSTRAINT FK_TARGETS_{1}_DATASETS", tableName, tableName.Replace("Targets_", ""));
                        var disableConstraint = string.Format("ALTER TABLE  {0} NOCHECK CONSTRAINT ALL;", tableName /*, tableName.Replace("Targets_", "")*/);
                        session.CreateSQLQuery(disableConstraint)
                           .SetTimeout(25000)
                           .ExecuteUpdate();


                        // Delete Transactional record
                        var transactionDelete = string.Format("delete from DatasetTransactionRecord ct where ct.Dataset.Id = {0}", entry.Id);
                        session.CreateQuery(transactionDelete)
                            .SetTimeout(25000)
                            .ExecuteUpdate();

                        //Delete websites with no website_id but have reference to this dataset being deleted
                        var orphanWesbite = string.Format("delete from Websites_WebsiteDatasets where Website_Id IS NULL and Dataset_Id = {0}", entry.Id);
                        session.CreateSQLQuery(orphanWesbite)
                            .SetTimeout(25000)
                            .ExecuteUpdate();

                        // finally delete content item record.
                        var importDelete = string.Format("delete from Dataset c where c.Id = {0}", entry.Id);
                        session.CreateQuery(importDelete)
                            .SetTimeout(25000)
                            .ExecuteUpdate();

                        #endregion

                        if (TargetDeletionVisitors != null)
                        {
                            TargetDeletionVisitors = TargetDeletionVisitors.OrderBy(v => v.Order).ToList();

                            var options = new VisitorOptions()
                            {
                                DataProvider = this.SessionProvider,
                                Logger = this.Logger
                            };
                            var factory = new TaskFactory(new MonahrqTaskScheduler(2));
                            var token = new CancellationTokenSource();

                            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                                new Action(() => ListExtensions.ForEach(TargetDeletionVisitors, visitor => factory.StartNew(() => entry.Accept(visitor, options), token.Token))));
                        }

                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Events.GetEvent<ErrorNotificationEvent>().Publish(ex);
            }
        }

        #region PropertyChanged Events

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <typeparam name="T">The type of the property that has a new value</typeparam>
        /// <param name="propertyExpression">A Lambda expression representing the property that has a new value.</param>
        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propertyName = ExtractPropertyName(propertyExpression);
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Extracts the property name from the property expression
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyExpression">An expression that evaluates to the property</param>
        /// <returns>
        /// The property name
        /// </returns>
        /// <exception cref="ArgumentNullException">propertyExpression</exception>
        /// <exception cref="ArgumentException">propertyExpression</exception>
        /// <remarks>
        /// Use this to take an expression like <code>() =&gt; MyProperty</code> and evaluate to the
        /// string "MyProperty"
        /// </remarks>
        protected string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(@"propertyExpression");
            }

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException(Sdk.Resources.BaseNotify_ExtractPropertyName_NotAMember, @"propertyExpression");
            }

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException(Sdk.Resources.BaseNotify_ExtractPropertyName_NotAProperty, @"propertyExpression");
            }

            var getMethod = property.GetGetMethod(true);

            if (getMethod == null)
            {
                // this shouldn't happen - the expression would reject the property before reaching this far
                throw new ArgumentException(Sdk.Resources.BaseNotify_ExtractPropertyName_NoGetter, @"propertyExpression");
            }

            if (getMethod.IsStatic)
            {
                throw new ArgumentException(Sdk.Resources.BaseNotify_ExtractPropertyName_Static, @"propertyExpression");
            }

            return memberExpression.Member.Name;
        }

        #endregion

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            IsActiveChanged += DataSetListViewModel_IsActiveChanged;
            //LoadData();
        }

        /// <summary>
        /// Handles the IsActiveChanged event of the DataSetListViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void DataSetListViewModel_IsActiveChanged(object sender, EventArgs e)
        {
            if (!IsActive) return;
            LoadData();
        }

        /// <summary>
        /// Gets the installed datasets.
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<DataTypeModel> GetInstalledDatasets()
        {
            ObservableCollection<DataTypeModel> data = new ObservableCollection<DataTypeModel>();

            using (var session = SessionProvider.SessionFactory.OpenSession())
            {
                //this.Service.CleanUnFinishedDatasets();

                ListExtensions.ForEach(TargetLookup, target =>
                {
                    var types = new List<int> { target.Value.Id };

                    var crit = PredicateBuilder.True<Dataset>();
                    crit = types.Aggregate(crit, (current, typeId) => current.And(item => item.ContentType.Id == typeId));

                    var contentItems = session.Query<Dataset>().Where(crit)
                        .ToFuture()
                        .Select(d => new Dataset
                        {
                            Id = d.Id,
                            IsFinished = d.IsFinished,
                            Name = d.Name,
                            ReportingQuarter = d.ReportingQuarter,
                            ReportingYear = d.ReportingYear,
                            DRGMDCMappingStatus = d.DRGMDCMappingStatus,
                            DRGMDCMappingStatusMessage = d.DRGMDCMappingStatusMessage,
                            ContentType = d.ContentType,
                            VersionYear = d.VersionYear,
							VersionMonth = d.VersionMonth,
                            DateImported = d.DateImported,
                            ProviderStates = d.ProviderStates,
                            TotalRows = d.TotalRows,
                            RowsImported = d.RowsImported
                        })
                        .ToList();

                    var items = contentItems.Select(item => new DataTypeDetailsViewModel(item)).ToList();
                    var dtm = new DataTypeModel(target.Value)
                    {
                        RecordsList = new ObservableCollection<DataTypeDetailsViewModel>(items)
                    };

                    data.Add(dtm);
                });
            }
            return data.OrderBy(ct => ct.DisplayOrder).ToObservableCollection();
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // remember which dataset was clicked to launch the wizard, so it can be reselected after the wizard
            // LoadData();
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <exception cref="InvalidOperationException">Data Types List cannot be null. Fatal error</exception>
        private void LoadData()
        {
            // Get target lookup
            TargetLookup = Service.GetDatasetTargets();

            string saveSelectedDataSetTypeName = null;
            if (SelectedDataType != null)
                saveSelectedDataSetTypeName = SelectedDataType.DataTypeName;

            DataTypesList = GetInstalledDatasets();

            var toReconcile = DataTypesList.Where(item => item.RecordsList.Any(d => !d.Entry.IsFinished)).SelectMany(s => s.RecordsList).ToList();
            foreach (var item in toReconcile.Where(item => !item.Entry.IsFinished).Select(item => item.Entry).ToList())
            {
                DeleteItem(item);
            }

            // try to return to the same dataset the user clicked previously, or select item 0
            if (saveSelectedDataSetTypeName != null)
            {
                var ds = DataTypesList.FirstOrDefault(a => a.DataTypeName == saveSelectedDataSetTypeName);
                if (ds != null)
                {
                    SelectedDataType = ds;
                    return;
                }
            }

            if (DataTypesList == null) throw new InvalidOperationException("Data Types List cannot be null. Fatal error");
            SelectedDataType = DataTypesList[0];
        }

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        /// <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            var nav = navigationContext;

            if (_ipDatasetRefreshPoller != null)
            {
                _ipDatasetRefreshPoller.Stop();
            }
        }

        /// <summary>
        /// Called when [is active].
        /// </summary>
        public void OnIsActive()
        {
            if (IsActiveChanged != null)
                IsActiveChanged(this, new EventArgs());
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public void Refresh()
        {
            LoadData();
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset() { }
        /// <summary>
        /// Called when [pre save].
        /// </summary>
        public void OnPreSave()
        {}

        /// <summary>
        /// Called when [tab changed].
        /// </summary>
        /// <returns></returns>
        public bool TabChanged()
        {
            return true;
        }

        /// <summary>
        /// Validates the on change.
        /// </summary>
        public void ValidateOnChange()
        {
            IsValid = true;
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets a value indicating whether [should validate].
        /// </summary>
        /// <value>
        /// <c>true</c> if [should validate]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool ShouldValidate
        {
            get { return false; } 
        }

        #endregion
    }
}
