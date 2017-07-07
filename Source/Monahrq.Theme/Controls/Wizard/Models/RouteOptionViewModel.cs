namespace Monahrq.Theme.Controls.Wizard.Models
{

	/// <summary>
	/// WizardViewModel reads these to determine overall workflow.  It doesn't care about the type param in RouteOptionViewModel below, or anything
	/// else but these two properties.
	/// UPDATE::: Wizard flow is now determined by the RouteModifier object returned from the OnNext method of each step view model.
	/// The main remaining use of this guy is when you have a step that simply asks a yes/no question.  See BinaryDecisionHelper.
	/// </summary>
	public interface IRouteOption
    {
		/// <summary>
		/// Gets or sets a value indicating whether this instance is selected.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
		/// </value>
		bool IsSelected { get; set; }
    }

	/// <summary>
	/// ViewModel for RouteOptions.
	/// </summary>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.OptionViewModel{TValue}" />
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.IRouteOption" />
	public class RouteOptionViewModel<TValue> : OptionViewModel<TValue>, IRouteOption
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="RouteOptionViewModel{TValue}"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public RouteOptionViewModel( TValue value ) : base( value ) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="RouteOptionViewModel{TValue}"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="sortValue">The sort value.</param>
		public RouteOptionViewModel( TValue value, int sortValue ) : base( value, sortValue ) { }
    }

}
