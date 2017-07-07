using System.ComponentModel;
using System.Runtime.CompilerServices;
using Monahrq.Theme.Controls.Wizard.Helpers;

namespace Monahrq.Theme.Controls.Wizard.Models
{
	/// <summary>
	/// Abstract base class for all steps shown in the wizard.
	/// </summary>
	/// <typeparam name="WizardDataContextObject">The type of the izard data context object.</typeparam>
	/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
	public abstract class WizardStepViewModelBase<WizardDataContextObject> : INotifyPropertyChanged
    {
		/// <summary>
		/// The data context object
		/// </summary>
		private readonly WizardDataContextObject _dataContextObject;
		/// <summary>
		/// The is current step
		/// </summary>
		private bool _isCurrentStep;
		/// <summary>
		/// The binary decision helper
		/// </summary>
		private readonly BinaryDecisionHelper _binaryDecisionHelper;
		/// <summary>
		/// Occurs when [property changed].
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged ;

		/// <summary>
		/// Called when [property changed].
		/// </summary>
		/// <param name="prop">The property.</param>
		protected virtual void OnPropertyChanged([CallerMemberName]string prop = null )
        {
            if (null == PropertyChanged) return;
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }



		/// <summary>
		/// Gets the data context object.
		/// </summary>
		/// <value>
		/// The data context object.
		/// </value>
		public WizardDataContextObject DataContextObject
        {
            get { return _dataContextObject; }
        }

		/// <summary>
		/// Gets the binary decision group.
		/// </summary>
		/// <value>
		/// The binary decision group.
		/// </value>
		public RouteOptionGroupViewModel<bool> BinaryDecisionGroup
        {
            get
            {
                return _binaryDecisionHelper.BinaryDecisionGroup;
            }
        }

		/// <summary>
		/// Gets the display name.
		/// </summary>
		/// <value>
		/// The display name.
		/// </value>
		public abstract string DisplayName { get; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is current step.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is current step; otherwise, <c>false</c>.
		/// </value>
		public bool IsCurrentStep
        {
            get { return _isCurrentStep; }
            set
            {
                if ( value == _isCurrentStep )
                {
                    return;
                }
                _isCurrentStep = value;
                OnPropertyChanged();
            }
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="WizardStepViewModelBase{WizardDataContextObject}"/> class.
		/// </summary>
		/// <param name="dataContextObject">The data context object.</param>
		protected WizardStepViewModelBase( WizardDataContextObject dataContextObject )
        {
            _dataContextObject = dataContextObject;
            _binaryDecisionHelper = new BinaryDecisionHelper();
        }

		/// <summary>
		/// For when yous need to save some values that can't be directly bound to UI elements.
		/// Not called when moving previous (see WizardViewModel.MoveToNextStep).
		/// </summary>
		/// <returns>
		/// An object that may modify the route
		/// </returns>
		public virtual RouteModifier OnNext()
        {
            // Must be virtual (as opposed to abstract) so descendants aren't forced to implement.
            return null;
        }

		/// <summary>
		/// For when yous need to set up some values that can't be directly bound to UI elements.
		/// </summary>
		public virtual void BeforeShow()
        {
        }

		/// <summary>
		/// Returns true if the user has filled in this step properly
		/// and the wizard should allow the user to progress to the
		/// next step in the workflow.
		/// </summary>
		/// <returns>
		///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
		/// </returns>
		public abstract bool IsValid();

		/// <summary>
		/// Configures the binary decision.
		/// </summary>
		/// <param name="displayName">Can't be empty string or the radio buttons won't work as a group</param>
		public void ConfigureBinaryDecision( string displayName = "_" )
        {
            _binaryDecisionHelper.ConfigureBinaryDecision( displayName );
        }

		/// <summary>
		/// Gets the value of binary decision.
		/// </summary>
		/// <returns></returns>
		public bool GetValueOfBinaryDecision()
        {
            return _binaryDecisionHelper.GetValueOfBinaryDecision();
        }

		/// <summary>
		/// Binaries the decision has been made.
		/// </summary>
		/// <returns></returns>
		public bool BinaryDecisionHasBeenMade()
        {
            return _binaryDecisionHelper.BinaryDecisionHasBeenMade();
        }

     
    }
}
