using System;
using System.ComponentModel;

namespace Monahrq.Theme.Controls.Wizard.Models
{

	/// <summary>
	/// For our StepTemplateConverter
	/// </summary>
	public interface IProvideViewType
    {
		/// <summary>
		/// Gets the type of the view.
		/// </summary>
		/// <value>
		/// The type of the view.
		/// </value>
		Type ViewType { get; }
    }

	/// <summary>
	/// The complete step.
	/// </summary>
	/// <typeparam name="IWizardBusinessObject">The type of the wizard business object.</typeparam>
	/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.IProvideViewType" />
	public class CompleteStep<IWizardBusinessObject> : INotifyPropertyChanged, IProvideViewType
    {

		/// <summary>
		/// The relevant
		/// </summary>
		private bool _relevant = true;
		/// <summary>
		/// The visited
		/// </summary>
		private bool _visited;
		/// <summary>
		/// Occurs when [property changed].
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="CompleteStep{IWizardBusinessObject}"/> is relevant.
		/// </summary>
		/// <value>
		///   <c>true</c> if relevant; otherwise, <c>false</c>.
		/// </value>
		public bool Relevant
        {
            get
            {
                return _relevant;
            }
            set
            {
                if ( _relevant != value )
                {
                    _relevant = value;
                    OnPropertyChanged( "Relevant" );
                }
            }
        }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="CompleteStep{IWizardBusinessObject}"/> is visited.
		/// </summary>
		/// <value>
		///   <c>true</c> if visited; otherwise, <c>false</c>.
		/// </value>
		public bool Visited
        {
            get
            {
                return _visited;
            }
            set
            {
                if ( _visited != value )
                {
                    _visited = value;
                    OnPropertyChanged( "Visited" );
                }
            }
        }

		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>
		/// The view model.
		/// </value>
		public WizardStepViewModelBase<IWizardBusinessObject> ViewModel { get; set; }

		/// <summary>
		/// The class type of the actual xaml view to be used for this step
		/// </summary>
		/// <value>
		/// The type of the view.
		/// </value>
		public Type ViewType { get; set; }

		/// <summary>
		/// Gets or sets the name of the group.
		/// </summary>
		/// <value>
		/// The name of the group.
		/// </value>
		public string GroupName { get; set; }

		/// <summary>
		/// Called when [property changed].
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		private void OnPropertyChanged( string propertyName )
        {
            var handler = PropertyChanged;
            if ( handler != null )
            {
                handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

    }

}
