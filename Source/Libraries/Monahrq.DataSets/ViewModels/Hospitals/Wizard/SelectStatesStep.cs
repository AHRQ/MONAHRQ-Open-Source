using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Context;
using Monahrq.DataSets.Services;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Services;
using Monahrq.Theme.Controls.Wizard.Helpers;
using Monahrq.Theme.Controls.Wizard.Models;
using NHibernate.Linq;
using PropertyChanged;
using Monahrq.Sdk.Extensions;
using Monahrq.Infrastructure.Services.Hospitals;
using Monahrq.Infrastructure.Services;
using System.Collections.Generic;
using Monahrq.Sdk.Modules.Settings;

namespace Monahrq.DataSets.ViewModels.Hospitals.Wizard
{
    [Export]
    [ImplementPropertyChanged]
    public class SelectStatesStep : WizardStepViewModelBase<GeographicContext>
    {
        public const string SELECT_STATE = "Select state";
        public DelegateCommand AddStateToContextCommand { get; set; }

        private GeographicContext Context { get; set; }
        public DelegateCommand<State> RemoveStateCommand { get; set; }
        HospitalRegistryService HospitalRegistryService { get; set; }

        public SelectStatesStep(GeographicContext context)
            : base(context)
        {
            Context = context;
            Initialize();
        }

        private void Initialize()
        {

            AddStateToContextCommand = new DelegateCommand(OnAddState, CanAddState);
            RemoveStateCommand = new DelegateCommand<State>(OnRemoveStateCommand, CanRemoveState);

            SelectedStates = CollectionHelper.EmptyListCollectionView<State>();
            EventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            StateCollection = Context.HospitalRegistryService
                        .GetStates()
                        .Select(state => new KeyValuePair<string, State>(state.Abbreviation, state))
                        .Concat(new[] { new KeyValuePair<string, State>(SELECT_STATE, null) })
                        .OrderBy(item => item.Value != null)
                        .ThenBy(item => item.Key).ToListCollectionView();

            var defaultStates = HospitalRegion.Default.DefaultStates.OfType<string>().ToList();

            defaultStates.ForEach(s =>
                {
                    var toRemove = StateCollection.OfType<KeyValuePair<string, State>>().First(kvp => kvp.Key == s);
                    StateCollection.Remove(toRemove);
                    SelectedStates.AddNewItem(toRemove.Value);
                });
            SelectedStates.CommitNew();
            StateCollection.CommitEdit();
            StateCollection.MoveCurrentToFirst();
            /*Regions for combo box, Object type return "SELECT" on display, and NULL on return using  */
            RegionTypes = (new[] { 
                                typeof(object),
                                typeof (HealthReferralRegion), 
                                typeof (HospitalServiceArea)}).ToListCollectionView();

            SelectedRegionType = HospitalRegion.Default.SelectedRegionType;

        }



        #region Commands

        private void OnRemoveStateCommand(State state)
        {
            try
            {
                SelectedStates.Remove(state);
                //                StateCollection.MoveCurrentToFirst();
                StateCollection = StateCollection.SourceCollection.OfType<KeyValuePair<string, State>>()
                        .Concat(new[] { new KeyValuePair<string, State>(state.Name, state) })
                        .OrderBy(item => item.Value != null)
                        .ThenBy(item => item.Key).ToListCollectionView();
                RaisePropertyChanged(() => SelectedStates);

            }
            catch (Exception ex)
            {
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
            }

        }

        private bool CanRemoveState(State state)
        {
            return true;
        }


        private bool CanAddState()
        {
            var selectStateBlank = StateCollection.GetItemAt(0);
            if (SelectedState.Value == null || selectStateBlank == null || SelectedState.Value.Equals(selectStateBlank))
                return false;
            return true;
        }

        private void OnAddState()
        {
            if (SelectedState.Value == null) return;

            var state = SelectedState.Value;

            SelectedStates.AddNewItem(SelectedState.Value);
            StateCollection.Remove(SelectedState);
            StateCollection.MoveCurrentToFirst();
            SelectedStates.CommitNew();

            RaisePropertyChanged(() => SelectedStates);
            RaisePropertyChanged(() => StateCollection);

        }

        #endregion

        #region Properties

        public Type SelectedRegionType { get; set; }


        public ListCollectionView RegionTypes
        {
            get;
            set;
        }


        KeyValuePair<string, State> _selectedState;
        public KeyValuePair<string, State> SelectedState
        {
            get { return _selectedState; }
            set
            {
                _selectedState = value;
                AddStateToContextCommand.Execute();
            }

        }


        public ListCollectionView StateCollection
        {
            get;
            set;
        }


        public ListCollectionView SelectedStates
        {
            get;
            set;
        }

        public override string DisplayName
        {
            get { return "Select States"; }
        }

        public override bool IsValid()
        {
            return SelectedStates.Count > 0
                && SelectedRegionType.IsSubclassOf(typeof(Region));

        }

        #endregion

        public override RouteModifier OnNext()
        {
            //next step is 
            Context.ContextStates = SelectedStates.OfType<State>();
            Context.ContextRegionType = RegionTypes.CurrentItem as Type;
            Context.LoadData();

            DataContextObject.Reset();
            return base.OnNext();
        }

        public override void BeforeShow()
        {
            base.BeforeShow();

        }
    }
}
