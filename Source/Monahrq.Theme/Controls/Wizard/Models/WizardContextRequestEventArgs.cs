using Microsoft.Practices.Prism.Events;
using Monahrq.Sdk.Events;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Events;

namespace Monahrq.Theme.Controls.Wizard.Models
{
	/// <summary>
	/// Event Arg for WizardStepsRequest.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TId">The type of the identifier.</typeparam>
	/// <typeparam name="TId2">The type of the id2.</typeparam>
	/// <seealso cref="Monahrq.Infrastructure.Entities.Events.ExtendedEventArgs{T}" />
	public class WizardStepsRequestEventArgs<T, TId, TId2> : ExtendedEventArgs<T>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="WizardStepsRequestEventArgs{T, TId, TId2}"/> class.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="wingidId">The wingid identifier.</param>
		/// <param name="existingDatasetId">The existing dataset identifier.</param>
		public WizardStepsRequestEventArgs(T data, TId wingidId, TId2 existingDatasetId): base(data)
        {
            WingId = wingidId;
            ExistingDatasetId = existingDatasetId;
        }

		/// <summary>
		/// Gets or sets the existing dataset identifier.
		/// </summary>
		/// <value>
		/// The existing dataset identifier.
		/// </value>
		public TId2 ExistingDatasetId { get; set; }
		/// <summary>
		/// Gets the wing identifier.
		/// </summary>
		/// <value>
		/// The wing identifier.
		/// </value>
		public TId WingId { get; private set; }
		/// <summary>
		/// Gets or sets the wizard steps.
		/// </summary>
		/// <value>
		/// The wizard steps.
		/// </value>
		public IStepCollection WizardSteps { get; set; }
    }

	/// <summary>
	/// WizardStepsRequest Event.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TId">The type of the identifier.</typeparam>
	/// <typeparam name="TId2">The type of the id2.</typeparam>
	/// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Theme.Controls.Wizard.Models.WizardStepsRequestEventArgs{T, TId, TId2}}" />
	public class WizardStepsRequestEvent<T, TId, TId2> : CompositePresentationEvent<WizardStepsRequestEventArgs<T, TId, TId2>>
    {

    }

	/// <summary>
	/// Event Arg for WizardRequestNavigation.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class WizardRequestNavigateEventArgs<T>
        where T : IWizardDataContextObject, new()
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="WizardRequestNavigateEventArgs{T}"/> class.
		/// </summary>
		/// <param name="stepContext">The step context.</param>
		public WizardRequestNavigateEventArgs(CompleteStep<T> stepContext)
        {
            StepContext = stepContext;
        }
		/// <summary>
		/// Gets the step context.
		/// </summary>
		/// <value>
		/// The step context.
		/// </value>
		public CompleteStep<T> StepContext { get; private set; }
    }

	/// <summary>
	/// Model for the progress of a Wizard Request.
	/// </summary>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.IProgressState" />
	[ImplementPropertyChanged]
    public class ProgressState : IProgressState
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ProgressState"/> class.
		/// </summary>
		/// <param name="current">The current.</param>
		/// <param name="total">The total.</param>
		public ProgressState(int current, int total)
        {
            Current = Math.Abs(current);
            Total = Math.Abs(total);
        }

		/// <summary>
		/// Prevents a default instance of the <see cref="ProgressState"/> class from being created.
		/// </summary>
		private ProgressState(): this(0,1)
        {
        }

		/// <summary>
		/// The empty
		/// </summary>
		private static ProgressState _empty = new ProgressState();
		/// <summary>
		/// Gets the empty.
		/// </summary>
		/// <value>
		/// The empty.
		/// </value>
		public static ProgressState Empty
        {
            get { return _empty; }
        }
		/// <summary>
		/// Gets the current.
		/// </summary>
		/// <value>
		/// The current.
		/// </value>
		public int Current
        {
            get;
            private set;
        }

		/// <summary>
		/// Gets the total.
		/// </summary>
		/// <value>
		/// The total.
		/// </value>
		public int Total
        {
            get;
            private set;
        }

		/// <summary>
		/// Gets the ratio.
		/// </summary>
		/// <value>
		/// The ratio.
		/// </value>
		public double Ratio
        {
            get
            {
                return ((double)Current / (double) Math.Max(Total,1));
            }
        }
    }

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Theme.Controls.Wizard.Models.WizardEvent}" />
	public class WizardEvent : CompositePresentationEvent<WizardEvent> { }
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.ComponentModel.CancelEventArgs}" />
	public class WizardCancellingEvent : CompositePresentationEvent<CancelEventArgs> { }
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.EventArgs}" />
	public class WizardCancelEvent : CompositePresentationEvent<EventArgs> { }

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Infrastructure.Entities.Events.ExtendedEventArgs{System.Action}}" />
	public class WizardActionEvent : CompositePresentationEvent<ExtendedEventArgs<Action>> { }

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Sdk.Events.Empty}" />
	public class CtsCancelEvent : CompositePresentationEvent<Empty> { }

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Theme.Controls.Wizard.Models.WizardRequestNavigateEventArgs{T}}" />
	public class WizardRequestNavigateEvent<T> : CompositePresentationEvent<WizardRequestNavigateEventArgs<T>>
           where T : IWizardDataContextObject, new()
    {

    }

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.EventArgs}" />
	public class WizardBackEvent : CompositePresentationEvent<EventArgs> { }


}
