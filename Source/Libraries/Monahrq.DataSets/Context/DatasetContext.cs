using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Events;
using Monahrq.DataSets.Services;
using Monahrq.DataSets.ViewModels.Crosswalk;
using Monahrq.DataSets.ViewModels.Validation;
using Monahrq.Default.DataProvider.Administration.File;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services.SysTray;
using Monahrq.Sdk.Extensions;
using Monahrq.Theme.Controls.Wizard.Models;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The dataset import wizard dataset context base class.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.Model.BaseDataContext" />
    public class DatasetContextBase : BaseDataContext
    {
        /// <summary>
        /// Gets or sets the steps.
        /// </summary>
        /// <value>
        /// The steps.
        /// </value>
        public IStepCollection<DatasetContextBase> Steps { get; set; }
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public ILoggerFacade Logger { get; private set; }
        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public IDomainSessionFactoryProvider Provider { get; set; }
        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        public IConfigurationService ConfigurationService { get; set; }

        /// <summary>
        /// Occurs when [finishing].
        /// </summary>
        public event EventHandler<CancelEventArgs> Finishing = delegate { };
        /// <summary>
        /// Occurs when [finished].
        /// </summary>
        public event EventHandler Finished = delegate { };

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetContextBase"/> class.
        /// </summary>
        public DatasetContextBase()
        {
            Logger = ServiceLocator.Current.GetInstance<ILoggerFacade>();
            Provider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            ConfigurationService = ServiceLocator.Current.GetInstance<IConfigurationService>();
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        /// <returns></returns>
        public override bool Cancel()
        {
            if (DatasetItem == null || DatasetItem.Id < 0) return true;

            var args = new CancelEventArgs(true);
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<WizardCancellingEvent>().Publish(args);
            var isCancelled = args.Cancel;
            if (isCancelled)
            {
                ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<WizardCancelEvent>().Publish(EventArgs.Empty);
                NotifyDeleteEntry();
                Application.DoEvents();
            }

            return isCancelled;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public override void Dispose()
        {}

        /// <summary>
        /// Notifies the delete entry.
        /// </summary>
        public void NotifyDeleteEntry()
        {
            if (DatasetItem == null || DatasetItem.Id <= 0) return;
            if (!DatasetItem.IsReImport)
            {
                ServiceLocator.Current.GetInstance<IEventAggregator>()
                                      .GetEvent<DeleteEntryEvent>()
                                      .Publish(new DeleteEntryEventArg
                                      {
                                          Dataset = DatasetItem,
                                          ShowUserPrompt = false
                                      });
            }
            
            DatasetItem = null;
            ExistingDatasetId = 0;
        }

        /// <summary>
        /// Gets the name of the target by.
        /// </summary>
        /// <param name="targetName">Name of the target.</param>
        /// <returns></returns>
        public Target GetTargetByName(string targetName)
        {
            Target target = null;
            using (var session = Provider.SessionFactory.OpenSession())
            {
                var targetQuery = session.CreateCriteria<Target>()
                                         .Add(Restrictions.InsensitiveLike("Name", targetName))
                                         .SetMaxResults(1)
                                         .Future<Target>();

                target = targetQuery.FirstOrDefault();
            }

            return target;
            //return SelectedDataType.Target;
        }

        /// <summary>
        /// Notifies the update entry.
        /// </summary>
        public void NotifyUpdateEntry()
        {
            if (DatasetItem == null || DatasetItem.Id <= 0) return;
            
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<UpdateEntryEvent>().Publish(DatasetItem);
        }

        /// <summary>
        /// Updates the target mapping.
        /// </summary>
        /// <param name="session">The session.</param>
        protected virtual void UpdateTargetMapping(ISession session)
        {}

        /// <summary>
        /// Saves the import entry.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="isWizardFinished">if set to <c>true</c> [is wizard finished].</param>
        /// <param name="updateFieldMapping">if set to <c>true</c> [update field mapping].</param>
        public void SaveImportEntry(Dataset item, bool isWizardFinished = false, bool updateFieldMapping = false)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                if (item.ContentType == null || item.ContentType.Id == 0)
                {
                    item.ContentType = SelectedDataType.Target;
                }

                using (var trans = session.BeginTransaction())
                {
                    session.Evict(item);

                    if(updateFieldMapping)
                        UpdateTargetMapping(session);

                    if (isWizardFinished)
                        item.IsFinished = true;

                    if (!item.IsPersisted)
                        session.Save(item);
                    else
                        item = session.Merge(item);

                    session.Flush();

                    trans.Commit();
                }
            }

            DatasetItem = item;

            // avoid updating the UI at every wizard step. Only call it once when done.
            if (isWizardFinished)
                NotifyUpdateEntry();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <returns></returns>
        public override string GetName()
        {
            return SelectedDataType == null ? string.Empty : SelectedDataType.DataTypeName;
        }

        /// <summary>
        /// Gets the dataset item.
        /// </summary>
        /// <returns></returns>
        public override Dataset GetDatasetItem()
        {
            ApplySummaryRecord(true);
            return DatasetItem;
        }

        /// <summary>
        /// Refreshes the target.
        /// </summary>
        /// <param name="targetToRefesh">The target to refesh.</param>
        /// <returns></returns>
        public override Target RefreshTarget(Target targetToRefesh)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                return session.Query<Target>()
                              .ToFuture()
                              .FirstOrDefault(t => t.Id == targetToRefesh.Id &&
                                              t.Name.ToUpper() == targetToRefesh.Name.ToUpper());
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is custom.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is custom; otherwise, <c>false</c>.
        /// </value>
        public override bool IsCustom
        {
            get { return DatasetItem.ContentType != null && DatasetItem.ContentType.IsCustom; }
        }

        /// <summary>
        /// Gets or sets the existing dataset identifier.
        /// </summary>
        /// <value>
        /// The existing dataset identifier.
        /// </value>
        public int? ExistingDatasetId { get; set; }

        /// <summary>
        /// Gets or sets the type of the selected data.
        /// </summary>
        /// <value>
        /// The type of the selected data.
        /// </value>
        public DataTypeModel SelectedDataType { get; set; }

        /// <summary>
        /// The lazy target type
        /// </summary>
        protected Type _lazyTargetType;

        static readonly object _lazyTargetLock = new object();
        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <value>
        /// The type of the target.
        /// </value>
        public Type TargetType
        {
            get
            {
                //var lazy = _lazyTargetType;
                if (_lazyTargetType == null)
                {
                    lock (_lazyTargetLock)
                    {
                        //lazy = _lazyTargetType;
                        //if (lazy == null)
                        //{
                            InitLazyType();
                        //}
                        //lazy = _lazyTargetType;
                    }
                }
                return _lazyTargetType;

                //var result = _lazyTargetType;
                //if (result == null)
                //{
                //    lock (_lazyTargetLock)
                //    {
                //        result = _lazyTargetType;
                //        if (result == null)
                //        {
                //            InitLazyType();
                //        }
                //    }
                //}
                //return result;
            }
        }

        /// <summary>
        /// The lazy properties
        /// </summary>
        protected IDictionary<string, PropertyInfo> _lazyProperties;

        static readonly object _lazyPropertyLock = new object();
        /// <summary>
        /// Gets the target properties.
        /// </summary>
        /// <value>
        /// The target properties.
        /// </value>
        public IDictionary<string, PropertyInfo> TargetProperties
        {
            get
            {
                var lazy = _lazyProperties;
                if (lazy == null)
                {
                    lock (_lazyPropertyLock)
                    {
                        lazy = _lazyProperties;
                        if (lazy == null)
                        {
                            InitLazyProperties();
                        }
                    }
                }
                var result = _lazyProperties;
                if (result == null)
                {
                    lock (_lazyPropertyLock)
                    {
                        result = _lazyProperties;
                        if (result == null)
                        {
                            InitLazyProperties();
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Initializes the lazy properties.
        /// </summary>
        protected virtual void InitLazyProperties()
        {
            //LazyProperties = new Lazy<IDictionary<string, PropertyInfo>>(() =>
            //    {
            //        return TargetType.GetProperties().ToDictionary(p => p.Name);
            //    }, true);
            _lazyProperties = TargetType.GetProperties().ToDictionary(p => p.Name);
        }

        /// <summary>
        /// Initializes the type of the lazy.
        /// </summary>
        protected virtual void InitLazyType()
        {
            //LazyTargetType = new Lazy<Type>(() =>
            //{
            //    return Type.GetType(SelectedDataType.Target.ClrType);
            //}, true);

            _lazyTargetType = !SelectedDataType.Target.IsCustom ? Type.GetType(SelectedDataType.Target.ClrType) : null;
        }

        /// <summary>
        /// Finishes this instance.
        /// </summary>
        public void Finish()
        {
            var canceller = new CancelEventArgs(false);
            OnFinishing(canceller);
            if (canceller.Cancel) return;

            //var item = DatasetItem;
            if (DatasetItem != null)
            {
                ApplySummaryRecord();
                SaveImportEntry(DatasetItem, true, true);
            }
            OnFinished(EventArgs.Empty);

            // Here we are kicking off the async grouper systray application after successful import of Inpatient Discharge dataset
            if (TargetType != null && TargetType.Name.EqualsIgnoreCase("InpatientTarget"))
            {
                LaunchInpatientGrouperProcessing();
            }
        }

        /// <summary>
        /// The ip thread identifier
        /// </summary>
        public static string IpThreadId;

        /// <summary>
        /// Launches the inpatient grouper processing.
        /// </summary>
        private void LaunchInpatientGrouperProcessing()
        {
            try
            {
                if (DatasetItem.DRGMDCMappingStatus != DrgMdcMappingStatusEnum.Intializing)
                    DatasetItem.DRGMDCMappingStatus = DrgMdcMappingStatusEnum.Intializing;

                SaveImportEntry(DatasetItem);

                DatasetSysTrayProcessor.ProcessDataset(DatasetItem);

                ServiceLocator.Current.GetInstance<IEventAggregator>()
                              .GetEvent<UpdateDrgMdsStatusEvent>()
                              .Publish(DatasetItem.Id.ToString());
            }
            catch (Exception)
            {
                Logger.Log("Failed launching system try application.", Category.Exception, Priority.High);
                throw;
            }
        }

        /// <summary>
        /// Applies the summary record.
        /// </summary>
        /// <param name="mappingOnly">if set to <c>true</c> [mapping only].</param>
        protected virtual void ApplySummaryRecord(bool mappingOnly = false)
        {
        }

        /// <summary>
        /// Raises the <see cref="E:Finished" /> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnFinished(EventArgs eventArgs)
        {
            Finished(this, eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="E:Finishing" /> event.
        /// </summary>
        /// <param name="canceller">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        protected virtual void OnFinishing(CancelEventArgs canceller)
        {
            Finishing(this, canceller);
        }

        #region PROPERTIES

        /// <summary>
        /// Gets or sets the dataset item.
        /// </summary>
        /// <value>
        /// The dataset item.
        /// </value>
        public Dataset DatasetItem { get; set; }

        const string HISTOGRAM_KEY = "{19766226-65A3-4D63-AE84-AD61773E256F}";

        /// <summary>
        /// Gets or sets the histogram.
        /// </summary>
        /// <value>
        /// The histogram.
        /// </value>
        public Histogram Histogram
        {
            get
            {
                return this[HISTOGRAM_KEY] as Histogram;
            }
            set
            {
                this[HISTOGRAM_KEY] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has header.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has header; otherwise, <c>false</c>.
        /// </value>
        public bool HasHeader { get; set; }

        #endregion
    }

    /// <summary>
    /// The import wizard dataset context.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.Model.DatasetContextBase" />
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [ImplementPropertyChanged]
    public class DatasetContext : DatasetContextBase
    {
        static readonly object _mappingsLock = new object();
        /// <summary>
        /// Gets the mappings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private MappingDictionary GetMappings(string key)
        {
            var result = this[key] as MappingDictionary;
            if (result == null)
            {
                lock (_mappingsLock)
                {
                    //result = this[key] as MappingDictionary;
                    this[key] = result = new MappingDictionary();
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the required mappings.
        /// </summary>
        /// <value>
        /// The required mappings.
        /// </value>
        public MappingDictionary RequiredMappings
        {
            get
            {
                return GetMappings(REQUIRED_MAPPINGS_BAG_KEY);
            }
        }

        /// <summary>
        /// Gets the optional mappings.
        /// </summary>
        /// <value>
        /// The optional mappings.
        /// </value>
        public MappingDictionary OptionalMappings
        {
            get
            {
                return GetMappings(OPTIONAL_MAPPINGS_BAG_KEY);
            }
        }

        const string REQUIRED_MAPPINGS_BAG_KEY = "{97272453-F5D9-4BA3-AE89-C2ED32D67DCD}";
        const string OPTIONAL_MAPPINGS_BAG_KEY = "{AA391A8D-ABD7-423B-988D-B341326F57ED}";
        const string TARGET_MAPPER_KEY = "{3D0549E9-3A5A-48F4-AB15-BE71D85E153C}";
        protected const string CROSSWALK_KEY = "{1C6575DA-17F6-4F6F-8364-C4F5CCCAC033}";
        const string CROSSWALK_CACHE_KEY = "{EABAA7AC-163A-4CDB-A403-434021B1BB20}";
        //const string ValidationErrorsKey = "{A8DEAC1A-D875-4F89-B115-D3E4D0C4A287}";
        //const string ImportTargetsKey = "{1934345D-AD05-41F0-AE3A-86A670BC4ABA}";
        const string SUPPRESSED_CONSTRAINTS_KEY = "{8BF762C9-BD62-4B83-9851-952EB3378767}";
        const string VALIDATION_SUMMARY_KEY = "{5679ED8B-E42D-4196-917A-F696F4BA9DAE}";
        const string DATASOURCE_DEFINITION_KEY = "{E7140112-3C33-4E6F-BC44-87A264547036}";

        const string TARGET_ELEMENTS_KEY = "{5C1A8DE7-4DA5-4BE9-B61C-06F8AD3483E7}";

        /// <summary>
        /// Gets or sets the datasource definition.
        /// </summary>
        /// <value>
        /// The datasource definition.
        /// </value>
        public IFileDatasourceViewModel DatasourceDefinition
        {
            get
            {
                return this[DATASOURCE_DEFINITION_KEY] as IFileDatasourceViewModel;
            }
            set
            {
                this[DATASOURCE_DEFINITION_KEY] = value;
            }
        }

        /// <summary>
        /// Gets or sets the mapper.
        /// </summary>
        /// <value>
        /// The mapper.
        /// </value>
        public ITargetMapper Mapper
        {
            get
            {
                return this[TARGET_MAPPER_KEY] as ITargetMapper;
            }
            set
            {
                this[TARGET_MAPPER_KEY] = value;
            }
        }

        /// <summary>
        /// Gets or sets the current crosswalk.
        /// </summary>
        /// <value>
        /// The current crosswalk.
        /// </value>
        public virtual IEnumerable<MappedFieldEntryViewModel> CurrentCrosswalk
        {
            get
            {
                return this[CROSSWALK_KEY] as IEnumerable<MappedFieldEntryViewModel>;
            }
            set
            {
                this[CROSSWALK_KEY] = value;
                CrosswalkCache = ElementCrossWalkMap.CreateCrosswalkCache(value);
            }
        }

        /// <summary>
        /// Gets or sets the crosswalk cache.
        /// </summary>
        /// <value>
        /// The crosswalk cache.
        /// </value>
        public List<ElementCrossWalkMap> CrosswalkCache
        {
            get
            {
                var ser = new JavaScriptSerializer();
                var temp = this[CROSSWALK_CACHE_KEY] as string;
                if (string.IsNullOrEmpty(temp) || string.IsNullOrWhiteSpace(temp)) 
                    return new List<ElementCrossWalkMap>();

                return ser.Deserialize<List<ElementCrossWalkMap>>(temp);
            }
            set
            {
                var ser = new JavaScriptSerializer();
                this[CROSSWALK_CACHE_KEY] = ser.Serialize(value ?? new List<ElementCrossWalkMap>());
            }
        }

        /// <summary>
        /// Afters the reset.
        /// </summary>
        protected override void AfterReset()
        {
            base.AfterReset();
            SuppressedConstraints = new List<Guid>();
        }

        /// <summary>
        /// Gets or sets the suppressed constraints.
        /// </summary>
        /// <value>
        /// The suppressed constraints.
        /// </value>
        public IList<Guid> SuppressedConstraints
        {
            get
            {
                return this[SUPPRESSED_CONSTRAINTS_KEY] as List<Guid>;
            }
            set
            {
                this[SUPPRESSED_CONSTRAINTS_KEY] = value;
            }
        }

        /// <summary>
        /// Gets or sets the validation summary.
        /// </summary>
        /// <value>
        /// The validation summary.
        /// </value>
        public ValidationResultsSummary ValidationSummary
        {
            get
            {
                return this[VALIDATION_SUMMARY_KEY] as ValidationResultsSummary;
            }
            set
            {
                this[VALIDATION_SUMMARY_KEY] = value;
            }
        }

        /// <summary>
        /// Gets or sets the target elements.
        /// </summary>
        /// <value>
        /// The target elements.
        /// </value>
        public List<Element> TargetElements
        {
            get
            {
                return this[TARGET_ELEMENTS_KEY] as List<Element>;
            }
            set
            {
                this[TARGET_ELEMENTS_KEY] = value;
            }
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>
        /// The services.
        /// </value>
        public IDataContextServices Services
        {
            get
            {
                return new DatacontextAware(this);
            }
        }

        /// <summary>
        /// Applies the summary record.
        /// </summary>
        /// <param name="mappingOnly">if set to <c>true</c> [mapping only].</param>
        protected override void ApplySummaryRecord(bool mappingOnly = false)
        {
            if (DatasetItem == null) return;

            if(!DatasetItem.Infoset.Element.IsEmpty)
                DatasetItem.Infoset.Element.RemoveNodes();
                
            ProgressState progress = null;
            if (!mappingOnly && ValidationSummary != null)
            {

                progress = new ProgressState(ValidationSummary.CountValid, ValidationSummary.Total);
            }

            if (/*DatasetItem.IsReImport &&*/ ValidationSummary != null)
            {
                if (DatasetItem.File.Contains(" (#"))
                    DatasetItem.File = DatasetItem.File.SubStrBefore(" (#");

                //DatasetItem.File += " (# Rows Imported: " +  ValidationSummary.CountValid + ")";
                DatasetItem.TotalRows = ValidationSummary.Total;
                DatasetItem.RowsImported = ValidationSummary.CountValid;
            } 

            var element = new DatasetSummaryHelper(this, progress, RequiredMappings, OptionalMappings).AsElement;
            DatasetItem.Infoset.Element.Add(element);
        }

        /// <summary>
        /// Updates the target mapping.
        /// </summary>
        /// <param name="session">The session.</param>
        protected override void UpdateTargetMapping(ISession session)
        {
            base.UpdateTargetMapping(session);

            if (SelectedDataType.Target.IsCustom) // We do not care for custom wing targets at this time. So we exit early. Will revisit later on. Jason
            {
                //session.Refresh(SelectedDataType.Target);
                return;
            }

            var elements = SelectedDataType.Target.Elements.ToList();
            foreach (var mapping in RequiredMappings)
            {
                var element = elements.FirstOrDefault(e => e.Name.ToUpper() == mapping.Key.ToUpper());

                if (element == null) continue;

                var mappingHints = element.MappingHints.ToList();
                if (!mappingHints.Any(hint => hint.EqualsIgnoreCase(mapping.Value)))
                {
                    mappingHints.Add(mapping.Value);
                }
                element.MappingHints = mappingHints;

                session.SaveOrUpdate(element);
            }

            foreach (var mapping in OptionalMappings)
            {
                var element = elements.FirstOrDefault(e => e.Name.ToUpper() == mapping.Key.ToUpper());

                if (element == null) continue;

                var mappingHints = element.MappingHints.ToList();
                if (!mappingHints.Any(hint => hint.EqualsIgnoreCase(mapping.Value)))
                {
                    mappingHints.Add(mapping.Value);
                }
                element.MappingHints = mappingHints;
                session.SaveOrUpdate(element);
            }
        }
    }


    /// <summary>
    /// The monahrq custom dataset field mapping dictionary.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String, System.String}}" />
    public class MappingDictionary : IEnumerable<KeyValuePair<string, string>>
    {
        /// <summary>
        /// Gets a value indicating whether [hints applied].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hints applied]; otherwise, <c>false</c>.
        /// </value>
        public bool HintsApplied
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the mappings.
        /// </summary>
        /// <value>
        /// The mappings.
        /// </value>
        IDictionary<string, string> Mappings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingDictionary"/> class.
        /// </summary>
        public MappingDictionary()
        {
            Mappings = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the <see cref="System.String"/> with the specified map to.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/>.
        /// </value>
        /// <param name="mapTo">The map to.</param>
        /// <returns></returns>
        public string this[string mapTo]
        {
            get
            {
                string result;
                if (!Mappings.TryGetValue(mapTo, out result)) return null;
                return result;
            }
            set
            {
                Mappings[mapTo] = value;
            }
        }

        /// <summary>
        /// Removes the specified p.
        /// </summary>
        /// <param name="p">The p.</param>
        public void Remove(string p)
        {
            Mappings.Remove(p);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Mappings.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Loads the hints.
        /// </summary>
        /// <param name="targetElements">The target elements.</param>
        /// <param name="histogram">The histogram.</param>
        [Obsolete]
        public void LoadHints(IEnumerable<MTargetField> targetElements, Histogram histogram)
        {
            //var crit = PredicateBuilder.True<Element>();
            var columnNames = histogram.FieldEntries.ToDictionary(entry => entry.Column.ColumnName.ToUpper().Trim());
            if (HintsApplied) return;
            foreach (var target in targetElements)
            {
                var hint = target.MappingHints.FirstOrDefault(columnNames.ContainsKey);
                if (!string.IsNullOrEmpty(hint))
                {
                    MOriginalField mapping = null;

                    if (columnNames.ContainsKey(hint))
                    {
                        mapping = target.OriginalFields
                                        .FirstOrDefault(fld => fld.Name.EqualsIgnoreCase(columnNames[hint].Column.ColumnName));
                    }
                    

                    Mappings[target.Name] = mapping == null ? null : mapping.Name;
                }
            }
            HintsApplied = true;
        }

        /// <summary>
        /// Gets the mapping from existing dataset.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        private string GetMappingFromExistingDataset(DatasetContext dataContext)
        {
            string mappingXmlToUse = null;
            using (var session = dataContext.Provider.SessionFactory.OpenStatelessSession())
            {
                mappingXmlToUse = session.CreateCriteria<Dataset>()
                    .Add(Restrictions.Eq("Id", dataContext.SelectedMappingDatasetId))
                    .SetMaxResults(1)
                    .SetProjection(Projections.Property("SummaryData"))
                    .UniqueResult<string>();
            }
            return mappingXmlToUse;
        }

        /// <summary>
        /// Loads the mapping hints.
        /// </summary>
        /// <typeparam name="TWizardDataContextObject">The type of the wizard data context object.</typeparam>
        /// <param name="targetElements">The target elements.</param>
        /// <param name="dataContextObject">The data context object.</param>
        /// <param name="sourceElements">The source elements.</param>
        public void LoadHints2<TWizardDataContextObject>(IEnumerable<MTargetField> targetElements, TWizardDataContextObject dataContextObject, IEnumerable<MOriginalField> sourceElements)
        {
            try
            {
                if (HintsApplied) return;

                var dataContext = dataContextObject as DatasetContext;
                if (dataContext != null &&
                   ((dataContext.MappingType == MappingType.ExistingDataSet && dataContext.SelectedMappingDatasetId > 0) ||
                   (dataContext.MappingType == MappingType.ExistingFile && !string.IsNullOrEmpty(dataContext.SelectDataMappingFile))))
                {
                    var dataSetXml = new XmlDocument();
                    string xPathQuery;

                    if (dataContext.MappingType == MappingType.ExistingDataSet)
                    {
                        string mappingXmlToUse = GetMappingFromExistingDataset(dataContext);
                        dataSetXml.LoadXml(mappingXmlToUse);
                        xPathQuery = "/Data/summary/mappings/required/mapping";
                    }
                    else
                    {
                        dataSetXml.Load(dataContext.SelectDataMappingFile);
                        xPathQuery = "/mappings/required/mapping";
                    }

                    var mappingsElement = dataSetXml.SelectNodes(xPathQuery).OfType<XmlNode>().ToList();

                    var columnNames = sourceElements.ToDictionary(entry => entry.Name.ToUpper().Trim());
                    foreach (var target in mappingsElement)
                    {
                        string hint = null;

                        var keyAttribute = target.Attributes["key"];
                        var valueAttribute = target.Attributes["value"];

                        if (valueAttribute == null || keyAttribute == null) continue;

                        if (valueAttribute.Value.ToUpper().In(columnNames.Keys))
                        {
                            hint = valueAttribute.Value.ToUpper();
                        }

                        if (!string.IsNullOrEmpty(hint))
                        {
                            MOriginalField mapping = null;

                            if (columnNames.ContainsKey(hint))
                            {
                                mapping = sourceElements.ToList().FirstOrDefault(fld => fld.Name.EqualsIgnoreCase(columnNames[hint.ToUpper()].Name));
                            }

                            Mappings[keyAttribute.Value] = mapping == null ? null : mapping.Name;
                        }
                    }
                }
                else
                {
                    var columnNames = sourceElements.ToDictionary(entry => entry.Name.ToUpper().Trim());
                    foreach (var target in targetElements)
                    {
                        string hint = null;

                        foreach (var mapHint in target.MappingHints.ToList())
                        {
                            if (mapHint.ToUpper().In(columnNames.Keys))
                            {
                                hint = mapHint.ToUpper();
                                break;
                            }
                        }

                        if (string.IsNullOrEmpty(hint)) continue;

                        MOriginalField mapping = null;

                        if (columnNames.ContainsKey(hint))
                        {
                            mapping = sourceElements.ToList().FirstOrDefault(fld => fld.Name.EqualsIgnoreCase(columnNames[hint.ToUpper()].Name));
                        }

                        Mappings[target.Name] = mapping == null ? null : mapping.Name;
                    }

                    #region Old code. It can be removed but after QA resolves issue. This is for historical reference at the moment - Jason

                    //var hint = target.MappingHints.FirstOrDefault(h => columnNames.Keys.Any(key => key.EqualsIgnoreCase(h) || 
                    //                                                                        key.StartsWith(h, StringComparison.OrdinalIgnoreCase) || 
                    //                                                                        key.EndsWith(h, StringComparison.OrdinalIgnoreCase)));
                    //if (!string.IsNullOrEmpty(hint))
                    //{
                    //    var mapping = sourceElements.FirstOrDefault(fld => 
                    //    {
                    //        var column = columnNames.FirstOrDefault(h => h.Key.EqualsIgnoreCase(hint) ||
                    //                                                                        h.Key.StartsWith(hint, StringComparison.OrdinalIgnoreCase) ||
                    //                                                                        h.Key.EndsWith(hint, StringComparison.OrdinalIgnoreCase));

                    //        if (column.Value != null)
                    //            return fld.Name.StartsWith(column.Value.Column.ColumnName, StringComparison.InvariantCulture);
                    //        else
                    //            return false;
                    //        //return fld.Name.StartsWith(columnNames[hint].Column.ColumnName, StringComparison.InvariantCulture);
                    //    });

                    //   Mappings[target.Name] = mapping == null ? null : mapping.Name;
                    //}

                    #endregion

                }

            }
            catch (Exception)
            {
                
            }
            finally
            {
                HintsApplied = true;
            }
        }
            
    }

    /// <summary>
    /// The dataset data cross walk map.
    /// </summary>
    public class CrosswalkMap
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public object Source { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value { get; set; }
    }

    /// <summary>
    /// The dataset element crosswalk map.
    /// </summary>
    public class ElementCrossWalkMap
    {
        /// <summary>
        /// Gets or sets the name of the element.
        /// </summary>
        /// <value>
        /// The name of the element.
        /// </value>
        public string ElementName { get; set; }
        /// <summary>
        /// Gets or sets the mappings.
        /// </summary>
        /// <value>
        /// The mappings.
        /// </value>
        public List<CrosswalkMap> Mappings { get; set; }

        /// <summary>
        /// Creates the crosswalk cache.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <returns></returns>
        static public List<ElementCrossWalkMap> CreateCrosswalkCache(IEnumerable<IMappedFieldEntryViewModel> models)
        {
            return models.Select(model => new ElementCrossWalkMap
                {
                    ElementName = model.Element.Name, 
                    Mappings = model.CrosswalkModels.Select(x => new CrosswalkMap
                        {
                            Source = x.Crosswalk.SourceValue, Value = x.Crosswalk.ScopeValue.Value
                        }).ToList()
                }).ToList();
        }
    }
}
