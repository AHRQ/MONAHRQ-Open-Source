using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Monahrq.Theme.Controls.Wizard.Helpers;

namespace Monahrq.Theme.Controls.Wizard.Models
{
	/// <summary>
	/// Manages a StepGroup and StepCollection.
	/// </summary>
	/// <typeparam name="WizardBusinessObject">The type of the izard business object.</typeparam>
	public class StepManager<WizardBusinessObject>
    {
		/// <summary>
		/// The current linked list step
		/// </summary>
		private LinkedListNode<CompleteStep<WizardBusinessObject>> _currentLinkedListStep;
		/// <summary>
		/// The reconfiguring route
		/// </summary>
		private bool _reconfiguringRoute;
		/// <summary>
		/// The steps
		/// </summary>
		private List<CompleteStep<WizardBusinessObject>> _steps;
		/// <summary>
		/// The linked steps
		/// </summary>
		private LinkedList<CompleteStep<WizardBusinessObject>> _linkedSteps;

		/// <summary>
		/// Gets or sets the current linked list step.
		/// </summary>
		/// <value>
		/// The current linked list step.
		/// </value>
		public LinkedListNode<CompleteStep<WizardBusinessObject>> CurrentLinkedListStep
        {
            get
            {
                return _currentLinkedListStep;
            }
            set
            {
                _currentLinkedListStep = value;
                if ( ( _linkedSteps.First == _currentLinkedListStep ) && !_reconfiguringRoute )
                {
                    ResetRoute();
                }
            }
        }

		/// <summary>
		/// Gets the index of the current.
		/// </summary>
		/// <value>
		/// The index of the current.
		/// </value>
		public int CurrentIndex 
        {
            get { return _steps.IndexOf(CurrentLinkedListStep.Value)+1; }
        }

		/// <summary>
		/// Gets the maximum progress.
		/// </summary>
		/// <value>
		/// The maximum progress.
		/// </value>
		public int MaxProgress 
        {
            get { return _steps.Count*2; }
        }
		/// <summary>
		/// Gets the steps left.
		/// </summary>
		/// <value>
		/// The steps left.
		/// </value>
		public int StepsLeft
        {
            get { return _steps.Count - (CurrentIndex); }
        }

		/// <summary>
		/// Gets the steps.
		/// </summary>
		/// <value>
		/// The steps.
		/// </value>
		public List<CompleteStep<WizardBusinessObject>> Steps { get { return _steps; } }

		/// <summary>
		/// Gets the first step.
		/// </summary>
		/// <value>
		/// The first step.
		/// </value>
		public LinkedListNode<CompleteStep<WizardBusinessObject>> FirstStep
        {
            get
            {
                return _linkedSteps == null ? null : _linkedSteps.First;
            }
        }

		/// <summary>
		/// Provides the steps.
		/// </summary>
		/// <param name="steps">The steps.</param>
		public void ProvideSteps( List<CompleteStep<WizardBusinessObject>> steps )
        {
            _steps = steps;
            _linkedSteps = new LinkedList<CompleteStep<WizardBusinessObject>>( _steps );
            CurrentLinkedListStep = _linkedSteps.First;
        }

		/// <summary>
		/// Navigates the specified step.
		/// </summary>
		/// <param name="step">The step.</param>
		public void Navigate(CompleteStep<WizardBusinessObject> step)
        {
            var moveTo = _linkedSteps.Find(step);
            CurrentLinkedListStep = moveTo;
        }

		/// <summary>
		/// Reworks the list based on.
		/// </summary>
		/// <param name="rm">The rm.</param>
		public void ReworkListBasedOn( RouteModifier rm )
        {
            if ( rm == null )
            {
                return;
            }
            _reconfiguringRoute = true;
            ReorganizeLinkedList( rm );
            ResetListRelevancy();
            _reconfiguringRoute = false;
        }

		/// <summary>
		/// Each step in the wizard may modify the route, but it's assumed that if the user goes back to step one, the route initializes back to the way it
		/// was when it was created.
		/// </summary>
		private void ResetRoute()
        {
            var allStepViewTypes = _linkedSteps.ToList().ConvertAll( s => s.ViewType );
            ReworkListBasedOn( new RouteModifier() { IncludeViewTypes = allStepViewTypes } );
        }

		/// <summary>
		/// At this point, if a step is in the linked list, it's relevant; if not, it's not.
		/// </summary>
		private void ResetListRelevancy()
        {
            _steps.ForEach( s => s.Relevant = false );
            var linkedStep = _linkedSteps.First;
            while ( linkedStep != null )
            {
                linkedStep.Value.Relevant = true;
                linkedStep = linkedStep.Next;
            }
        }

		/// <summary>
		/// Re-create the linked list to reflect the new "workflow."
		/// </summary>
		/// <param name="rm">The rm.</param>
		private void ReorganizeLinkedList( RouteModifier rm )
        {
            var cacheCurrentStep = CurrentLinkedListStep.Value;
            var newSubList = CreateNewStepList( rm );

            // Re-create linked list.
            _linkedSteps = new LinkedList<CompleteStep<WizardBusinessObject>>( newSubList );
            ResetCurrentLinkedListStepTo( cacheCurrentStep );
        }

		/// <summary>
		/// Creates the new step list.
		/// </summary>
		/// <param name="rm">The rm.</param>
		/// <returns></returns>
		private List<CompleteStep<WizardBusinessObject>> CreateNewStepList( RouteModifier rm )
        {
            var result = new List<CompleteStep<WizardBusinessObject>>( _linkedSteps );

            EnsureNotModifyingCurrentStep( rm );

            if ( rm.ExcludeViewTypes != null )
            {
                rm.ExcludeViewTypes.ForEach( t => result.RemoveAll( step => step.ViewType.Equals( t ) ) );
            }
            if ( rm.IncludeViewTypes != null )
            {
                AddBack( result, rm.IncludeViewTypes );
            }

            return result;
        }

		/// <summary>
		/// Ensures the not modifying current step.
		/// </summary>
		/// <param name="rm">The rm.</param>
		private void EnsureNotModifyingCurrentStep( RouteModifier rm )
        {
            Func<Type, bool> currentStepCondition = t => t == CurrentLinkedListStep.Value.ViewType;
            if ( rm.ExcludeViewTypes != null )
            {
                Contract.Ensures( rm.ExcludeViewTypes.FirstOrDefault( currentStepCondition ) == null );
            }
            if ( rm.IncludeViewTypes != null )
            {
                Contract.Ensures( rm.IncludeViewTypes.FirstOrDefault( currentStepCondition ) == null );
            }
        }

		/// <summary>
		/// OMG, if the user chooses an option that changes the route through the wizard, then goes back and chooses a different option,
		/// we need to add the appropriate step(s) back into the workflow.
		/// </summary>
		/// <param name="workingStepList">The working step list.</param>
		/// <param name="viewTypes">The view types.</param>
		private void AddBack( List<CompleteStep<WizardBusinessObject>> workingStepList, List<Type> viewTypes )
        {
            foreach ( var vt in viewTypes )
            {
                // Find the step to add back in the main list of steps.
                var stepToAddBack = _steps.FirstOrDefault(s => s.ViewType == vt);
                if (workingStepList.Contains(stepToAddBack)) continue;
                // Re-insert the step into our working list (which will become the wizard's new linked list).
                if (stepToAddBack == null) continue;

                var indexOfStepToAddBack = _steps.IndexOf( stepToAddBack );
                // If it belongs at the head of the list, add it there.
                if ( indexOfStepToAddBack == 0 )
                {
                    workingStepList.Insert( 0, stepToAddBack );
                    continue;
                }

                // Otherwise we have to find the previous step in the main list, find that step in our working list and add in
                // the step after that step.
                var stepReinserted = false;
                var countOfStepsToPreviousFoundStep = 1;
                while ( !stepReinserted )
                {
                    var previousStep = _steps[indexOfStepToAddBack - countOfStepsToPreviousFoundStep];
                    for ( var i = 0; i < workingStepList.Count; i++ )
                    {
                        if (workingStepList[i].ViewType != previousStep.ViewType) continue;

                        workingStepList.Insert( i + 1, stepToAddBack );
                        stepReinserted = true;
                    }
                    // The previous step wasn't found; continue to the next previous step.
                    countOfStepsToPreviousFoundStep++;
                }
            }
        }

		/// <summary>
		/// Must maintain the current step reference (this re-creating of the linked list happens when the user makes a selection on
		/// the current step).
		/// After recreating the list, our CurrentLinkedListStep reference would be referring to an item in the old linked list.
		/// </summary>
		/// <param name="cacheCurrentStep">The cache current step.</param>
		/// <exception cref="Exception">Error resetting current step after reorganizing steps.</exception>
		private void ResetCurrentLinkedListStepTo( CompleteStep<WizardBusinessObject> cacheCurrentStep )
        {
            CurrentLinkedListStep = _linkedSteps.First;
            while ( CurrentLinkedListStep.Value != cacheCurrentStep )
            {
                if ( CurrentLinkedListStep.Next == null )
                {
                    throw new Exception( "Error resetting current step after reorganizing steps." );
                }
                CurrentLinkedListStep = CurrentLinkedListStep.Next;
            }
        }

    }

}
