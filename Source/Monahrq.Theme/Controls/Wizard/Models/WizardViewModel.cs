using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Monahrq.Theme.Controls.Wizard.Helpers;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Prism.Events;
using System.Threading;

namespace Monahrq.Theme.Controls.Wizard.Models
{

	/// <summary>
	/// Interface for a WizardViewModel.
	/// </summary>
	public interface IWizardViewModel
    {
		/// <summary>
		/// Gets the move next command.
		/// </summary>
		/// <value>
		/// The move next command.
		/// </value>
		ICommand MoveNextCommand { get; }
		/// <summary>
		/// Gets the move previous command.
		/// </summary>
		/// <value>
		/// The move previous command.
		/// </value>
		ICommand MovePreviousCommand { get; }
		/// <summary>
		/// Gets a value indicating whether this instance is on last step.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is on last step; otherwise, <c>false</c>.
		/// </value>
		bool IsOnLastStep { get; }

		/// <summary>
		/// Attaches this instance.
		/// </summary>
		void Attach();

		/// <summary>
		/// Detaches this instance.
		/// </summary>
		void Detach();
    }

    //public delegate void NextEventHandler( object currentStep );

    ///// <summary>
    ///// The main ViewModel class for the wizard.  This class contains the various steps shown in the workflow and provides navigation between the steps.
    ///// </summary>
    ///// <typeparam name="WizardBusinessObject">The object the wizard models.  Must have parameterless constructor because we will create it within.</typeparam>
    //public class WizardViewModel<WizardBusinessObject> : IWizardViewModel, INotifyPropertyChanged
    //    where WizardBusinessObject : IWizardDataContextObject, new()
    //{

    //    private readonly WizardBusinessObject _businessObject;
    //    private readonly StepManager<WizardBusinessObject> _stepManager;
    //    private RelayCommand _moveNextCommand;
    //    private RelayCommand _movePreviousCommand;
    //    private RelayCommand _cancelCommand;

    //    public event NextEventHandler NextEvent;
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    /// <summary>
    //    /// Referenced only in xaml
    //    /// </summary>
    //    public ReadOnlyCollection<CompleteStep<WizardBusinessObject>> Steps
    //    {
    //        get
    //        {
                
    //            return new ReadOnlyCollection<CompleteStep<WizardBusinessObject>>( _stepManager.Steps );
    //        }
    //    }

        
    //    public int StepsCount
    //    {
    //        get { return Steps.Count; }
    //    }


    //    private int _currentIndex;
    //    public int CurrentIndex
    //    {
    //        get { return _currentIndex; }
    //        set { _currentIndex = value;
    //        OnPropertyChanged("CurrentIndex");}
    //    }

    //    private int _maxProgress;
    //    public int MaxProgress
    //    {
    //        get { return _stepManager.MaxProgress; }
    //        set
    //        {
    //            _maxProgress = value;
    //            OnPropertyChanged("MaxProgress");
    //        }
    //    }


    //    private int _maxGroupProgress;
    //    public int MaxGroupProgress
    //    {
    //        get { return _maxGroupProgress; }
    //        set
    //        {
    //            _maxGroupProgress = value;
    //            OnPropertyChanged("MaxGroupProgress");
    //        }
    //    }
        
    //    private string _leftOver;
    //    public string LeftOver
    //    {
    //        get { return _leftOver; }
    //        set
    //        {
    //            _leftOver = value;
    //            OnPropertyChanged("LeftOver");
    //        }
    //    }

    //    /// <summary>
    //    /// Returns the step ViewModel that the user is currently viewing.
    //    /// </summary>
    //    /// <summary>
    //    /// Returns the business object the wizard is building.  If this returns null, the user cancelled.
    //    /// </summary>
    //    public WizardBusinessObject BusinessObject
    //    {
    //        get { return _businessObject; }
    //    }

    //    public LinkedListNode<CompleteStep<WizardBusinessObject>> CurrentLinkedListStep
    //    {
    //        get { return _stepManager.CurrentLinkedListStep; }
    //        private set
    //        {
    //            if ( value == _stepManager.CurrentLinkedListStep )
    //            {
    //                return;
    //            }
                

    //            ActionsOnCurrentLinkedListStep( value );

            
    //            OnPropertyChanged( "CurrentLinkedListStep" );
    //            OnPropertyChanged( "IsOnLastStep" );
    //        }
    //    }

    //    public WizardViewModel()
    //    {
    //        _stepManager = new StepManager<WizardBusinessObject>();
    //        _businessObject = Activator.CreateInstance<WizardBusinessObject>();
            
    //    }


    //    private List<StepGroup> _stepGroups; 
    //    public List<StepGroup> StepGroups
    //    {
    //        get { return _stepGroups; }
    //        set { _stepGroups = value;
    //        OnPropertyChanged("StepGroups");
    //        }
    //    }

    //    private StepGroup _CurrentStepGroup;
    //    public StepGroup CurrentStepGroup
    //    {
    //        get { return _CurrentStepGroup; }
    //        set
    //        {
    //            _CurrentStepGroup = value;
    //            OnPropertyChanged("CurrentStepGroup");
    //        }
    //    }

    //    public void ProvideSteps(StepCollection<WizardBusinessObject> stepCollection)
    //    {
    //        _stepManager.ProvideSteps(stepCollection.GetAllSteps());

    //        StepGroups = stepCollection.Collection.Keys.ToList();
    //        MaxGroupProgress = StepGroups.Count*2;

    //        ActionsOnCurrentLinkedListStep( _stepManager.FirstStep );
    //    }

    //    private List<CompleteStep<WizardBusinessObject>> _currentGroupSteps ; 
    //    public List<CompleteStep<WizardBusinessObject>> CurrentGroupSteps
    //    {
    //        get { return _currentGroupSteps; }
    //        set { _currentGroupSteps = value;
    //        OnPropertyChanged("CurrentGroupSteps");
    //        }
    //    }
        
    //    private int _currentGroupIndex ;
    //    public int CurrentGroupIndex
    //    {
    //        get { return _currentGroupIndex; }
    //        set { _currentGroupIndex = value;
    //        OnPropertyChanged("CurrentGroupIndex");
    //        }
    //    }

    //    private void ActionsOnCurrentLinkedListStep(LinkedListNode<CompleteStep<WizardBusinessObject>> step)
    //    {
    //        if (CurrentLinkedListStep != null)
    //        {
    //            CurrentLinkedListStep.Value.ViewModel.IsCurrentStep = false;
    //        }

    //        _stepManager.CurrentLinkedListStep = step;

    //        if (step != null)
    //        {
    //            step.Value.ViewModel.IsCurrentStep = true;
    //            step.Value.ViewModel.BeforeShow();

    //            foreach (var group in StepGroups)
    //            {
    //                @group.IsCurrent = step.Value.GroupName == @group.DisplayName;
    //            }

    //            CurrentGroupIndex =(StepGroups.IndexOf(StepGroups.First(x => x.IsCurrent))+1)*2-1;

    //            CurrentGroupSteps = Steps.Where(x => x.GroupName == step.Value.GroupName).ToList();
    //        }


           

    //        CurrentIndex = (_stepManager.CurrentIndex * 2) - 1; //(progress bar must stop in a center )

          
    //    }
    //    void Cancel()
    //    {
    //        _businessObject.Cancel();
    //    }

    //    /// <summary>
    //    /// Returns the command which, when executed, cancels the order and causes the Wizard to be removed from the user interface.
    //    /// </summary>
    //    public ICommand CancelCommand
    //    {
    //        get
    //        {
    //            if ( _cancelCommand == null )
    //            {
    //                _cancelCommand = new RelayCommand( () => this.Cancel() );
    //            }
    //            return _cancelCommand;
    //        }
    //    }

    //    /// <summary>
    //    /// Returns the command which, when executed, causes the CurrentLinkedListStep
    //    /// property to reference the previous step in the workflow.
    //    /// </summary>
    //    public ICommand MovePreviousCommand
    //    {
    //        get
    //        {
    //            if ( _movePreviousCommand == null )
    //            {
    //                _movePreviousCommand = new RelayCommand( () => this.MoveToPreviousStep(), () => this.CanMoveToPreviousStep );
    //            }
    //            return _movePreviousCommand;
    //        }
    //    }

    //    /// <summary>
    //    /// Returns the command which, when executed, causes the CurrentLinkedListStep property to reference the next step in the workflow.  If the user
    //    /// is viewing the last step in the workflow, this causes the Wizard to finish and be removed from the user interface.
    //    /// </summary>
    //    public ICommand MoveNextCommand
    //    {
    //        get {
    //            return _moveNextCommand ??
    //                   (_moveNextCommand = new RelayCommand(MoveToNextStep, () => CanMoveToNextStep));
    //        }
    //    }

    //    bool CanMoveToPreviousStep
    //    {
    //        get { return CurrentLinkedListStep.Previous != null; }
    //    }

    //    void MoveToPreviousStep()
    //    {
    //        if ( CanMoveToPreviousStep )
    //        {
    //            CurrentLinkedListStep = CurrentLinkedListStep.Previous;
    //            //CurrentLinkedListStep.Value.ViewModel.BeforeShow();
    //        }
    //    }

    //    bool CanMoveToNextStep
    //    {
    //        get
    //        {
    //            var step = CurrentLinkedListStep;
    //            return ( step != null ) && ( step.Value.ViewModel.IsValid() ) && ( step.Next != null );
    //        }
    //    }

    //    /// <summary>
    //    /// Note that currently, the step OnNext handler is only called when moving next; not when moving previous.
    //    /// </summary>
    //    void MoveToNextStep()
    //    {
    //        if ( CanMoveToNextStep )
    //        {
    //            _stepManager.ReworkListBasedOn( CurrentLinkedListStep.Value.ViewModel.OnNext() );
    //            CurrentLinkedListStep = CurrentLinkedListStep.Next;
    //            //CurrentLinkedListStep.Value.ViewModel.BeforeShow();
    //            CurrentLinkedListStep.Value.Visited = true;
    //        }
    //    }

      

       

    //    /// <summary>
    //    /// Returns true if the user is currently viewing the last step in the workflow.
    //    /// </summary>
    //    public bool IsOnLastStep
    //    {
    //        get { return CurrentLinkedListStep.Next == null; }
    //    }

    //    private void OnPropertyChanged( string propertyName )
    //    {
    //        var handler = PropertyChanged;
    //        if ( handler != null )
    //        {
    //            handler( this, new PropertyChangedEventArgs( propertyName ) );
    //        }
    //    }


    //    private SubscriptionToken _navigateRequestToken = null;
    //    public void Attach()
    //    {
    //        Detach();
    //        _navigateRequestToken = ServiceLocator.Current.GetInstance<IEventAggregator>()
    //            .GetEvent<WizardRequestNavigateEvent<WizardBusinessObject>>().Subscribe(payload => NavigateTo(payload));
            
    //    }

    //    private void NavigateTo(WizardRequestNavigateEventArgs<WizardBusinessObject> payload)
    //    {
            
    //    }

    //    public void Detach()
    //    {
    //        var token = Interlocked.Exchange<SubscriptionToken>(ref _navigateRequestToken, null);
    //        if (token != null)
    //        {
    //            ServiceLocator.Current.GetInstance<IEventAggregator>()
    //                .GetEvent<WizardRequestNavigateEvent<WizardBusinessObject>>().Unsubscribe(token);
    //        }
    //    }
    //}

  
}
