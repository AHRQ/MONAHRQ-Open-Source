using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using LinqKit;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.HospitalRegionMapping.Events;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Services.Hospitals;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Types;
using PropertyChanged;
using CollectionHelper = Monahrq.Sdk.Extensions.CollectionHelper;
using Region = Monahrq.Infrastructure.Domain.Regions.Region;

namespace Monahrq.DataSets.HospitalRegionMapping.Context
{
    /// <summary>
    /// The context view model. This view model handles the editing of the global
    /// geographical context used throughout Monahrq.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.HospitalRegionMappingViewModel" />
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ImplementPropertyChanged]
    public class ContextViewModel : HospitalRegionMappingViewModel
    {
        #region Fields and Constants

        private Type _selectedRegionType;

        private SelectListItem _selectedState;

        #endregion

        #region Properties

        /// <summary>
        /// The select state
        /// </summary>
        public readonly SelectListItem SELECT_STATE = new SelectListItem { Text = "Select state", Value = null };

        /// <summary>
        /// Gets or sets the current mapping basis.
        /// </summary>
        /// <value>
        /// The current mapping basis.
        /// </value>
        HospitalRegionElement CurrentMappingBasis { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is initial mapping.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is initial mapping; otherwise, <c>false</c>.
        /// </value>
        private bool IsInitialMapping
        {
            get
            {
                //var temp = new HospitalRegionElement();
                return ConfigurationService != null && ConfigurationService.HospitalRegion.SelectedRegionType == null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the available states.
        /// </summary>
        /// <value>
        /// The available states.
        /// </value>
        public ListCollectionView AvailableStates { get; set; }

        /// <summary>
        /// Gets or sets the selected states.
        /// </summary>
        /// <value>
        /// The selected states.
        /// </value>
        public ListCollectionView SelectedStates { get; set; }

        /// <summary>
        /// Gets or sets the type of the selected region.
        /// </summary>
        /// <value>
        /// The type of the selected region.
        /// </value>
        public Type SelectedRegionType
        {
            get { return _selectedRegionType; }
            set
            {
                _selectedRegionType = value;
                ApplyContextCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [reference exists].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [reference exists]; otherwise, <c>false</c>.
        /// </value>
        public bool ReferenceExists { get; set; }

        /// <summary>
        /// Gets or sets the region types.
        /// </summary>
        /// <value>
        /// The region types.
        /// </value>
        public ListCollectionView RegionTypes { get; set; }

        //public KeyValuePair<string, State> SelectedState
        /// <summary>
        /// Gets or sets the state of the selected.
        /// </summary>
        /// <value>
        /// The state of the selected.
        /// </value>
        public SelectListItem SelectedState
        {
            get { return _selectedState; }
            set
            {
                _selectedState = value;

                if (_selectedState == null || _selectedState.Value == SELECT_STATE.Value) return;

                AddStateToContextCommand.Execute();
            }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get { return "Define your regional context"; } }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the add state to context command.
        /// </summary>
        /// <value>
        /// The add state to context command.
        /// </value>
        public DelegateCommand AddStateToContextCommand { get; set; }
        /// <summary>
        /// Gets or sets the apply context command.
        /// </summary>
        /// <value>
        /// The apply context command.
        /// </value>
        public DelegateCommand ApplyContextCommand { get; set; }
        /// <summary>
        /// Gets or sets the remove state from context command.
        /// </summary>
        /// <value>
        /// The remove state from context command.
        /// </value>
        public DelegateCommand<SelectListItem> RemoveStateFromContextCommand { get; set; }
        /// <summary>
        /// Gets or sets the navigate back command.
        /// </summary>
        /// <value>
        /// The navigate back command.
        /// </value>
        public DelegateCommand NavigateBackCommand { get; set; }

        #endregion

        #region Imports

        /// <summary>
        /// Gets or sets the hospital registry service.
        /// </summary>
        /// <value>
        /// The hospital registry service.
        /// </value>
        [Import]
        public IHospitalRegistryService HospitalRegistryService { get; set; }

        /// <summary>
        /// Gets or sets the region manager.
        /// </summary>
        /// <value>
        /// The region manager.
        /// </value>
        [Import]
        public IRegionManager RegionManager { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        protected ILogWriter Logger { get; set; }

        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        protected IConfigurationService ConfigurationService { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextViewModel"/> class.
        /// </summary>
        /// <param name="eventAggregator">The event aggregator.</param>
        [ImportingConstructor]
        public ContextViewModel(IEventAggregator eventAggregator)
        {
			//var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
			Events = eventAggregator;
            
            IsEnabled = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            CurrentMappingBasis = ConfigurationService.HospitalRegion;
            ReferenceExists = !IsInitialMapping;

            AddStateToContextCommand = new DelegateCommand(OnAddState, CanAddState);
            RemoveStateFromContextCommand = new DelegateCommand<SelectListItem>(OnRemoveStateCommand, CanRemoveState);
            ApplyContextCommand = new DelegateCommand(OnApplyContextCommand, CanApplyContextCommand);
            NavigateBackCommand = new DelegateCommand(OnNavigateBackCommand);
            SelectedStates = CollectionHelper.EmptyListCollectionView<SelectListItem>();
            var crit = PredicateBuilder.True<State>();

            ListExtensions.ForEach(CurrentMappingBasis.DefaultStates.OfType<string>().ToList(), 
                                   ab => crit = crit.And(st => st.Abbreviation != ab));

            var availStates = new List<SelectListItem>();
            availStates.Add(SELECT_STATE);

            var allStates = HospitalRegistryService.GetStates(null).ToList();

            availStates.AddRange(allStates.AsQueryable()
                              .Where(crit)
                              .Select(state => new SelectListItem { Model = state, Text = state.Abbreviation, Value = state.Abbreviation})
                              //.Concat(new[] { SELECT_STATE })
                              .OrderBy(item => item.Text).ToList());

            AvailableStates = availStates.ToListCollectionView();

            crit = PredicateBuilder.False<State>();

            ListExtensions.ForEach(CurrentMappingBasis.DefaultStates
              .OfType<string>(), ab => crit = crit.Or(st => st.Abbreviation == ab));

            var selectedStates = allStates.AsQueryable().Where(crit).Select(st => new SelectListItem { Text = st.Abbreviation, Value = st.Abbreviation, Model = st }).ToList();

            // Setting the Selected Reporting States for Global use.
            //if (!MonahrqContext.ReportingStatesContext.Any(s => selectedStates.Any(s1 => s.In(s1)) ))
            //    MonahrqContext.ReportingStatesContext.AddRange(selectedStates);
            MonahrqContext.ReportingStatesContext.AddRange(selectedStates.Select(s => s.Text).ToList());

            SelectedStates = selectedStates.ToListCollectionView();
            SelectedStates.CommitNew();
            AvailableStates.CommitEdit();
            AvailableStates.MoveCurrentToFirst();

            SelectedState = SELECT_STATE;

            // Regions for combo box, Object type return "SELECT" on display, and NULL on return using 
            RegionTypes = (new[] { 
                                typeof(object),
                                typeof (CustomRegion), 
                                typeof (HealthReferralRegion), 
                                typeof (HospitalServiceArea)}).ToListCollectionView();

            SelectedRegionType = CurrentMappingBasis.SelectedRegionType;
        }

        /// <summary>
        /// Called when [navigate back command].
        /// </summary>
        private void OnNavigateBackCommand()
        {
            Events.GetEvent<ContextAppliedEvent>().Publish("Canceled");
        }

        /// <summary>
        /// Determines whether this instance [can apply context command].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can apply context command]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanApplyContextCommand()
        {
            return SelectedStates.Count > 0 && SelectedRegionType.IsSubclassOf(typeof(Region));
        }

        /// <summary>
        /// Called when [apply context command].
        /// </summary>
        private void OnApplyContextCommand()
        {
            Events.GetEvent<PleaseStandByEvent>().Publish(CreatePleaseStandByEventPayload());
            try
            {
                SaveMapping();
                Events.GetEvent<ContextAppliedEvent>().Publish("Saved");
                Events.GetEvent<ResumeNormalProcessingEvent>().Publish(Empty.Value);
            }
            catch (Exception ex)
            {
                Events.GetEvent<ResumeNormalProcessingEvent>().Publish(Empty.Value);
                Events.GetEvent<ErrorNotificationEvent>().Publish(ex);
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
                Message = string.Format("Saving your selected regional settings.{0}Please stand by...", Environment.NewLine)
            };
        }

        /// <summary>
        /// Saves the mapping.
        /// </summary>
        private void SaveMapping()
        {
            ConfigurationService.HospitalRegion.AddStates(SelectedStates.OfType<SelectListItem>().Select(item => item.Text).ToArray(), true);

            ConfigurationService.HospitalRegion.SelectedRegionType = SelectedRegionType;
            ConfigurationService.Save();

            MonahrqContext.ReportingStatesContext = SelectedStates.OfType<SelectListItem>().Select(item => item.Text).ToList();
            MonahrqContext.SelectedRegionType = SelectedRegionType;

            if (SelectedRegionType != typeof(CustomRegion))
            {
                var sessionProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
                //Task.Factory.StartNew(() =>
                //    {
                try
                {
                    ClearRegions(sessionProvider, SelectedRegionType,
                                 SelectedStates.OfType<SelectListItem>().ToArray());
                }
                catch (Exception exc)
                {
                    Logger.Write(exc.InnerException ?? exc, TraceEventType.Error);
                }
                //});
            }
        }

        /// <summary>
        /// Clears the regions.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        /// <param name="selectedRegionType">Type of the selected region.</param>
        /// <param name="selectedStates">The selected states.</param>
        public void ClearRegions(IDomainSessionFactoryProvider sessionFactory, Type selectedRegionType, SelectListItem[] selectedStates)
        {
            if (selectedRegionType == typeof(CustomRegion)) return;

            var states = selectedStates.Aggregate(string.Empty, (current, state) => current + string.Format("'{0}',", state.Text));
            states = states.SubStrBeforeLast(",");

            var updateQuery = @"UPDATE [dbo].[Hospitals]
SET [CustomRegion_Id] = null
Where [CustomRegion_Id] is not null or [CustomRegion_Id]=0
AND [State] in (" + states + ") AND [IsArchived]=0 AND [IsDeleted]=0; ";

            using (var session = sessionFactory.SessionFactory.OpenStatelessSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    session.CreateSQLQuery(updateQuery)
                           .ExecuteUpdate();

                    trans.Commit();
                }
            }
        }

        /// <summary>
        /// Called when [remove state command].
        /// </summary>
        /// <param name="state">The state.</param>
        private void OnRemoveStateCommand(SelectListItem state)
        {
            try
            {
                SelectedStates.Remove(state);
                AvailableStates = AvailableStates.SourceCollection.OfType<SelectListItem>()
                        .Concat(new[] { state })
                        .OrderBy(item => item.Value != null)
                        .ThenBy(item => item.Text).ToListCollectionView();

                ApplyContextCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                Events.GetEvent<ErrorNotificationEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Determines whether this instance [can remove state] the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can remove state] the specified state; otherwise, <c>false</c>.
        /// </returns>
        private bool CanRemoveState(SelectListItem state)
        {
            return state != null;
        }

        /// <summary>
        /// Determines whether this instance [can add state].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can add state]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanAddState()
        {
            var selectStateBlank = AvailableStates.GetItemAt(0);
            return SelectedState != null && selectStateBlank != null && !SelectedState.Equals(selectStateBlank);
        }

        /// <summary>
        /// Called when [add state].
        /// </summary>
        private void OnAddState()
        {
            if (SelectedState == null || SelectedState == SELECT_STATE) return;
            AddStateToContext(SelectedState);
        }

        /// <summary>
        /// Adds the state to context.
        /// </summary>
        /// <param name="stateKvp">The state KVP.</param>
        private void AddStateToContext(SelectListItem stateKvp)
        {
            SelectedStates.AddNewItem(stateKvp);
            AvailableStates.Remove(stateKvp);
            AvailableStates.MoveCurrentToFirst();
            SelectedStates.CommitNew();
            ApplyContextCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public void Refresh()
        {
			var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
			ReferenceExists = !IsInitialMapping;
            CurrentMappingBasis = configService.HospitalRegion;
            if (SelectedStates != null)
            {
                SelectedStates.OfType<SelectListItem>().ToList().ForEach(OnRemoveStateCommand);
            }
            RaisePropertyChanged(() => SelectedStates);
            CurrentMappingBasis.DefaultStates
                .OfType<string>()
                .ToList()
                .ForEach(ab =>
                    {
                        var toAdd = AvailableStates.OfType<SelectListItem>().FirstOrDefault(kvp => kvp.Value != null && kvp.Value.ToString().EqualsIgnoreCase(ab) );
                        if (toAdd != null)
                        {
                            AddStateToContext(toAdd);
                        }
                    });
            RaisePropertyChanged(() => AvailableStates);
            SelectedRegionType = CurrentMappingBasis.SelectedRegionType ?? typeof(object);
            RaisePropertyChanged(() => SelectedRegionType);
        }

        /// <summary>
        /// Called when [navigated to].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {}

        /// <summary>
        /// Determines whether [is navigation target] [the specified navigation context].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        ///   <c>true</c> if [is navigation target] [the specified navigation context]; otherwise, <c>false</c>.
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
        {}

        #endregion
    }
}
