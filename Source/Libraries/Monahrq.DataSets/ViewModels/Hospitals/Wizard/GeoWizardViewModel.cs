using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Events;
using Monahrq.Infrastructure.Services.Hospitals;
using Monahrq.Sdk.Modules.Settings;
using Monahrq.Sdk.Regions;
using Monahrq.Theme.Controls.Wizard.Helpers;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Theme.Controls.Wizard.Models.Data;

namespace Monahrq.DataSets.ViewModels.Hospitals.Wizard
{
    public class GeoWizardViewModel<WizardBusinessObject> : IWizardViewModel, INotifyPropertyChanged where WizardBusinessObject : IWizardDataContextObject, new()
    {
        WizardBusinessObject BusinessObject
        {
            get;
            set;
        }

        protected IRegionManager RegionMgr { get; set; }
        IEventAggregator Events { get; set; }

        private readonly StepManager<WizardBusinessObject> _stepManager;
        private RelayCommand _moveNextCommand;
        private RelayCommand _importCompleteCommand;
        private RelayCommand _movePreviousCommand;
        protected RelayCommand _cancelCommand;

        public event NextEventHandler NextEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyCollection<CompleteStep<WizardBusinessObject>> Steps
        {
            get
            {
                return new ReadOnlyCollection<CompleteStep<WizardBusinessObject>>(_stepManager.Steps);
            }
        }

        public GeoWizardViewModel()
        {
            _stepManager = new StepManager<WizardBusinessObject>();
            RegionMgr = ServiceLocator.Current.GetInstance<IRegionManager>();
            Events = ServiceLocator.Current.GetInstance<IEventAggregator>();
            Events.GetEvent<WizardNavigateSelectStatesEvent>().Subscribe(uri =>
                {
                    RegionMgr.RequestNavigate(RegionNames.HospitalsMainRegion, new Uri(ViewNames.GeoContextWizard, UriKind.Relative));
                    _stepManager.Navigate(_stepManager.Steps.First());
                });
        }

        #region Properties

        T GuardBusinessObject<T>(Func<T> onNull, Func<T> onElse)
        {
            if (BusinessObject == null)
            {
                return onNull();
            }
            return onElse();
        }

        void GuardBusinessObject(Action guarded)
        {
            if (BusinessObject != null)
            {
                guarded();
            }
        }

        public String DataTypeDisplayName
        {
            get
            {
                return GuardBusinessObject<string>(() => string.Empty, () => BusinessObject.GetName());
            }
        }

        public int StepsCount
        {
            get { return Steps.Count; }
        }

        private int _currentIndex;
        public int CurrentIndex
        {
            get { return _currentIndex; }
            set
            {
                _currentIndex = value;
                OnPropertyChanged("CurrentIndex");
            }
        }

        private int _maxProgress;
        public int MaxProgress
        {
            get { return _stepManager.MaxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged("MaxProgress");
            }
        }

        private int _maxGroupProgress;
        public int MaxGroupProgress
        {
            get { return _maxGroupProgress; }
            set
            {
                _maxGroupProgress = value;
                OnPropertyChanged("MaxGroupProgress");
            }
        }

        private List<StepGroup> _stepGroups;
        public List<StepGroup> StepGroups
        {
            get { return _stepGroups; }
            set
            {
                _stepGroups = value;
                OnPropertyChanged("StepGroups");
            }
        }

        private StepGroup _CurrentStepGroup;
        public StepGroup CurrentStepGroup
        {
            get { return _CurrentStepGroup; }
            set
            {
                _CurrentStepGroup = value;
                OnPropertyChanged("CurrentStepGroup");
            }
        }

        private List<CompleteStep<WizardBusinessObject>> _currentGroupSteps;
        public List<CompleteStep<WizardBusinessObject>> CurrentGroupSteps
        {
            get { return _currentGroupSteps; }
            set
            {
                _currentGroupSteps = value;
                OnPropertyChanged("CurrentGroupSteps");
            }
        }

        private int _currentGroupIndex;
        public int CurrentGroupIndex
        {
            get { return _currentGroupIndex; }
            set
            {
                _currentGroupIndex = value;
                OnPropertyChanged("CurrentGroupIndex");
            }
        }
        #endregion

        public LinkedListNode<CompleteStep<WizardBusinessObject>> CurrentLinkedListStep
        {
            get { return _stepManager.CurrentLinkedListStep; }
            private set
            {
                if (value == _stepManager.CurrentLinkedListStep)
                {
                    return;
                }

                ActionsOnCurrentLinkedListStep(value);

                OnPropertyChanged("CurrentLinkedListStep");
                OnPropertyChanged("IsOnLastStep");
            }
        }

        public void ProvideSteps(StepCollection<WizardBusinessObject> stepCollection)
        {
            BusinessObject = stepCollection.Context;

            _stepManager.ProvideSteps(stepCollection.GetAllSteps());

            StepGroups = stepCollection.Collection.Keys.ToList();
            MaxGroupProgress = StepGroups.Count * 2;

            ActionsOnCurrentLinkedListStep(_stepManager.FirstStep);
        }

        private void ActionsOnCurrentLinkedListStep(LinkedListNode<CompleteStep<WizardBusinessObject>> step)
        {
            if (CurrentLinkedListStep != null)
            {
                CurrentLinkedListStep.Value.ViewModel.IsCurrentStep = false;
            }

            _stepManager.CurrentLinkedListStep = step;

            if (step != null)
            {
                step.Value.ViewModel.IsCurrentStep = true;
                step.Value.ViewModel.BeforeShow();

                foreach (var group in StepGroups)
                {
                    @group.IsCurrent = step.Value.GroupName == @group.DisplayName;
                }

                CurrentGroupIndex = (StepGroups.IndexOf(StepGroups.First(x => x.IsCurrent)) + 1) * 2 - 1;

                CurrentGroupSteps = Steps.Where(x => x.GroupName == step.Value.GroupName).ToList();
            }

            CurrentIndex = (_stepManager.CurrentIndex * 2) - 1; //(progress bar must stop in a center )
        }

        void Cancel()
        {
            var isCancelled = !CanMoveToPreviousStep;

            if (!isCancelled)
            {
                GuardBusinessObject(() => isCancelled = BusinessObject.Cancel());
            }
            if (isCancelled)
            {
                NavigateOut();
            }
        }

        private void NavigateOut()
        {
            if (HospitalRegion.Default.DefaultStates.Count == 0)
            {
                Events.GetEvent<NavigateToMeasureDataEvent>().Publish(EventArgs.Empty);
            }
            else
            {
                RegionMgr.RequestNavigate(RegionNames.HospitalsMainRegion, new Uri(ViewNames.HospitalsDataTabView, UriKind.Relative));
            }
        }

        #region Commands
        /// <summary>
        /// Returns the command which, when executed, cancels the order and causes the Wizard to be removed from the user interface.
        /// </summary>
        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(Cancel)); }
        }

        /// <summary>
        /// Returns the command which, when executed, causes the CurrentLinkedListStep
        /// property to reference the previous step in the workflow.
        /// </summary>
        public ICommand MovePreviousCommand
        {
            get {
                return _movePreviousCommand ??
                       (_movePreviousCommand =
                        new RelayCommand(this.MoveToPreviousStep, () => this.CanMoveToPreviousStep));
            }
        }

        /// <summary>
        /// Returns the command which, when executed, causes the CurrentLinkedListStep property to reference the next step in the workflow.  If the user
        /// is viewing the last step in the workflow, this causes the Wizard to finish and be removed from the user interface.
        /// </summary>
        public ICommand MoveNextCommand
        {
            get
            {
                return _moveNextCommand ??
                       (_moveNextCommand = new RelayCommand(MoveToNextStep, () => CanMoveToNextStep));
            }
        }

        public ICommand ImportCompleteCommand
        {
            get
            {
                return _importCompleteCommand ?? 
                       (
                           _importCompleteCommand =
                           new RelayCommand(ImportCompleteStep,
                                            () => CanMoveToImportCompleteStep) 
                       );
            }
        }

        bool CanMoveToPreviousStep
        {
            get { return CurrentLinkedListStep.Previous != null; }
        }

        void MoveToPreviousStep()
        {
            if (CanMoveToPreviousStep)
            {
                ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<WizardBackEvent>().Publish(EventArgs.Empty);
                
                CurrentLinkedListStep = CurrentLinkedListStep.Previous;

                _stepManager.ReworkListBasedOn(CurrentLinkedListStep.Value.ViewModel.OnNext());
            }
        }

        void ImportCompleteStep()
        {
            
            NavigateOut();
        }

        bool CanMoveToNextStep
        {
            get
            {
                var step = CurrentLinkedListStep;
                return (step != null) && (step.Value.ViewModel.IsValid()) && (step.Next != null);
            }
        }

        bool CanMoveToImportCompleteStep
        {
            get
            {
                var step = CurrentLinkedListStep;
                return (step != null) && (step.Value.ViewModel.IsValid()) && (step.Next == null);
            }
        }

        public Visibility MoveNextVisible
        {
            get
            {
                var step = CurrentLinkedListStep;
                return ((step != null) && (step.Next != null)) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility ImportCompleteVisible
        {
            get
            {
                var step = CurrentLinkedListStep;
                return ((step != null) && (step.Next == null)) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Note that currently, the step OnNext handler is only called when moving next; not when moving previous.
        /// </summary>
        void MoveToNextStep()
        {
            if (!CanMoveToNextStep) return;

            _stepManager.ReworkListBasedOn(CurrentLinkedListStep.Value.ViewModel.OnNext());
            CurrentLinkedListStep = CurrentLinkedListStep.Next;
            //CurrentLinkedListStep.Value.ViewModel.BeforeShow();
            CurrentLinkedListStep.Value.Visited = true;
        }

        #endregion


        /// <summary>
        /// Returns true if the user is currently viewing the last step in the workflow.
        /// </summary>
        public bool IsOnLastStep
        {
            get { return CurrentLinkedListStep.Next == null; }
        }

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private SubscriptionToken _navigateRequestToken = null;
        public void Attach()
        {
            Detach();
            _navigateRequestToken = ServiceLocator.Current.GetInstance<IEventAggregator>()
                                                  .GetEvent<WizardRequestNavigateEvent<WizardBusinessObject>>().Subscribe(NavigateTo);
        }

        public void Detach()
        {
            var token = Interlocked.Exchange<SubscriptionToken>(ref _navigateRequestToken, null);
            if (token != null)
            {
                ServiceLocator.Current.GetInstance<IEventAggregator>()
                              .GetEvent<WizardRequestNavigateEvent<WizardBusinessObject>>().Unsubscribe(token);
            }
        }

        private void NavigateTo(WizardRequestNavigateEventArgs<WizardBusinessObject> payload)
        { 
            _stepManager.Navigate(payload.StepContext);
           
        }
    }
}