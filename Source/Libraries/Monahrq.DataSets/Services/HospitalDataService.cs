using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals.Mapping;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Services.Hospitals;
using PropertyChanged;

namespace Monahrq.DataSets.Services
{
    /// <summary>
    /// The view model collection attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public class ViewModelCollectionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public Type EntityType { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelCollectionAttribute"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ViewModelCollectionAttribute(Type type)
        {
            EntityType = type;
        }
    }


    /// <summary>
    /// The mapping reference data contract.
    /// </summary>
    public class DataContracts
    {
        /// <summary>
        /// The mapping reference
        /// </summary>
        public const string MAPPING_REFERENCE = "MappingReference";
    }

    /// <summary>
    /// The hospital data service interface.
    /// </summary>
    public interface IHospitalDataService
    {
        /// <summary>
        /// Gets a value indicating whether this instance is mapping context available.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is mapping context available; otherwise, <c>false</c>.
        /// </value>
        bool IsMappingContextAvailable { get; }
        /// <summary>
        /// Gets or sets the lazy hospital registry.
        /// </summary>
        /// <value>
        /// The lazy hospital registry.
        /// </value>
        Lazy<HospitalRegistry> LazyHospitalRegistry { get; set; }
        /// <summary>
        /// Gets the state collection.
        /// </summary>
        /// <value>
        /// The state collection.
        /// </value>
        List<State> StateCollection { get; }
        //Base class implemnted
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback">The callback.</param>
        void GetAll<T>(Action<List<T>, Exception> callback) where T : class, IEntity;
        /// <summary>
        /// Gets the entity by identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="callback">The callback.</param>
        void GetEntityById<T>(object id, Action<T, Exception> callback);
        /// <summary>
        /// Refreshes the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        void RefreshEntity<T>(T entity, Action<bool, Exception> callback) where T : class, IEntity;
        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        void Save<T>(T entity, Action<object, Exception> callback) where T : class, IEntity;
        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="callback">The callback.</param>
        void Delete<T>(T entity, Action<bool, Exception> callback) where T : class, IEntity;
        /// <summary>
        /// Creates the mapping context.
        /// </summary>
        /// <param name="states">The states.</param>
        /// <param name="region">The region.</param>
        void CreateMappingContext(IEnumerable<State> states, Type region);

        /// <summary>
        /// Gets the state for region.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <returns></returns>
        string GetStateForRegion(Region region);
    }


    /// <summary>
    /// The hospital data service.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Services.DataServiceBase" />
    /// <seealso cref="Monahrq.DataSets.Services.IHospitalDataService" />
    [Export(typeof(IHospitalDataService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class HospitalDataService : DataServiceBase, IHospitalDataService
    {
        /// <summary>
        /// Gets or sets the lazy hospital registry.
        /// </summary>
        /// <value>
        /// The lazy hospital registry.
        /// </value>
        public Lazy<HospitalRegistry> LazyHospitalRegistry { get; set; }

        /// <summary>
        /// Gets the base data providers.
        /// </summary>
        /// <value>
        /// The base data providers.
        /// </value>
        [ImportMany(DataImportContracts.Hospitals)]
        public IEnumerable<IDataReaderDictionary> BaseDataProviders { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is mapping context available.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is mapping context available; otherwise, <c>false</c>.
        /// </value>
        public bool IsMappingContextAvailable { get; private set; }

        /// <summary>
        /// Gets the state collection.
        /// </summary>
        /// <value>
        /// The state collection.
        /// </value>
        public List<State> StateCollection { get; private set; }

        /// <summary>
        /// Gets or sets the hospital registry service.
        /// </summary>
        /// <value>
        /// The hospital registry service.
        /// </value>
        public IHospitalRegistryService HospitalRegistryService { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalDataService"/> class.
        /// </summary>
        /// <param name="hospitalMappingService">The hospital mapping service.</param>
        [ImportingConstructor]
        public HospitalDataService(
           IHospitalRegistryService hospitalMappingService)
        {
            HospitalRegistryService = hospitalMappingService;
            /*Load list of states from DB, and just keep them in mem for reference*/
            GetAll<State>((o, e) =>
                {
                    if (e == null)
                    {
                        StateCollection = o;
                    }
                    else
                    {
                        throw new NullReferenceException("Could not load State Definitions from local DB");
                    }
                });
        }

        /// <summary>
        /// Gets the state for region.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <returns></returns>
        public string GetStateForRegion(Region region)
        {
            using (var session = GetSession())
            {
                return session.Load<Region>(region.Id).State;
            }
        }

        #region Private Methods

        /*Returns list of all properties that are marked with ViewModelCollectionAttribute */
        /// <summary>
        /// Gets the view model collections.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<PropertyInfo> _getViewModelCollections()
        {
            return _getPropertiesOfType(GetType().GetProperties, typeof(ViewModelCollectionAttribute));
        }

        /// <summary>
        /// Gets the type of the properties of.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getMembers">The get members.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        private static IEnumerable<T> _getPropertiesOfType<T>(Func<BindingFlags, T[]> getMembers, Type attribute) where T : MemberInfo
        {
            return getMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(attribute, false).Any());
        }

        /// <summary>
        /// Initializes the current mapping context.
        /// </summary>
        /// <returns></returns>
        private bool _initCurrentMappingContext()
        {
            IsMappingContextAvailable = false;
            return IsMappingContextAvailable;
        }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
        }

        /// <summary>
        /// The worker
        /// </summary>
        BackgroundWorker _worker;
        /// <summary>
        /// Creates the worker.
        /// </summary>
        private void _createWorker()
        {
            _worker = new BackgroundWorker { WorkerReportsProgress = true };
            _worker.DoWork += _loadData;
            _worker.RunWorkerCompleted += _loadDataCompleted;
        }

        /// <summary>
        /// Handles the loadDataCompleted event of the  control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        private void _loadDataCompleted(object sender, RunWorkerCompletedEventArgs e)
        {}

        /// <summary>
        /// Handles the loadData event of the  control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        private void _loadData(object sender, DoWorkEventArgs e)
        { }


        private IEnumerable<State> _states;

        private Type _regionType;

        /// <summary>
        /// Creates the mapping context.
        /// </summary>
        /// <param name="states">The states.</param>
        /// <param name="regionType">Type of the region.</param>
        public void CreateMappingContext(IEnumerable<State> states, Type regionType)
        {
            this._states = states;
            this._regionType = regionType;

            if (_worker == null)
                _createWorker();

            if (_worker != null)
                _worker.RunWorkerAsync();

        }
        #endregion
    }

    /// <summary>
    /// The CMS view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class CmsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmsViewModel"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public CmsViewModel(int id, string name)
        {
            Id = id;
            Name = string.IsNullOrEmpty(name) || name.Equals(GetType().FullName) ? string.Empty : name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmsViewModel"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="hospitalName">Name of the hospital.</param>
        public CmsViewModel(int id, string name, string hospitalName)
            : this(id, name)
        {
            HospitalName = hospitalName;
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the name of the hospital.
        /// </summary>
        /// <value>
        /// The name of the hospital.
        /// </value>
        public string HospitalName { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// The CMS collection view model/
    /// </summary>
    [ImplementPropertyChanged]
    public class CmsCollectionViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmsCollectionViewModel"/> class.
        /// </summary>
        public CmsCollectionViewModel()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="CmsCollectionViewModel"/> class.
        /// </summary>
        /// <param name="hospitalMappingReference">The hospital mapping reference.</param>
        public CmsCollectionViewModel(HospitalMappingReference hospitalMappingReference)
        {
            if (hospitalMappingReference == null)
            {
                return;
            }

            HospitalMappingReference = hospitalMappingReference;

            CmsViewModels = new ObservableCollection<CmsViewModel>();
            CmsViewModels = hospitalMappingReference.CmsLookup.Select(cms => new CmsViewModel(cms.Item1, cms.Item2, cms.Item3)).ToObservableCollection();
        }

        /// <summary>
        /// Gets or sets the CMS view models.
        /// </summary>
        /// <value>
        /// The CMS view models.
        /// </value>
        public ObservableCollection<CmsViewModel> CmsViewModels { get; set; }

        CmsViewModel _selectedCms;

        /// <summary>
        /// Gets or sets the selected CMS.
        /// </summary>
        /// <value>
        /// The selected CMS.
        /// </value>
        public CmsViewModel SelectedCMS
        {
            get { return _selectedCms; }
            set
            {
                _selectedCms = value;
            }
        }

        /// <summary>
        /// Gets the hospital mapping reference.
        /// </summary>
        /// <value>
        /// The hospital mapping reference.
        /// </value>
        public HospitalMappingReference HospitalMappingReference { get; private set; }
    }
}
