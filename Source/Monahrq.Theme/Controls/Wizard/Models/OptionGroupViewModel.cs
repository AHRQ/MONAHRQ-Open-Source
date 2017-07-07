using System.Collections.ObjectModel;
using Monahrq.Theme.Controls.Wizard.Helpers;

namespace Monahrq.Theme.Controls.Wizard.Models
{

	/// <summary>
	/// Optional Group ViewModel
	/// </summary>
	/// <typeparam name="TOption">Type of options that will be in the collection: regular or route options</typeparam>
	/// <typeparam name="TValue">The type of each option</typeparam>
	public abstract class OptionGroupViewModel<TOption, TValue>
    {
		/// <summary>
		/// The display name
		/// </summary>
		private readonly string _displayName;

		/// <summary>
		/// Initializes a new instance of the <see cref="OptionGroupViewModel{TOption, TValue}"/> class.
		/// </summary>
		/// <param name="displayName">If it's blank, then the display name will be the TValue type (if not bool)</param>
		public OptionGroupViewModel( string displayName )
        {
            if ( displayName == string.Empty )
            {
                _displayName = typeof( TValue ) == typeof( bool ) ? "" : typeof( TValue ).Name.SplitCamelCase();
            }
            else
            {
                _displayName = displayName;
            }
        }

		/// <summary>
		/// Gets the display name.
		/// </summary>
		/// <value>
		/// The display name.
		/// </value>
		public string DisplayName { get { return _displayName; } }
		/// <summary>
		/// Gets or sets the option models.
		/// </summary>
		/// <value>
		/// The option models.
		/// </value>
		public abstract ReadOnlyCollection<TOption> OptionModels { get; set; }
    }

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.OptionGroupViewModel{Monahrq.Theme.Controls.Wizard.Models.OptionViewModel{TValue}, TValue}" />
	public class RegularOptionGroupViewModel<TValue> : OptionGroupViewModel<OptionViewModel<TValue>, TValue>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="RegularOptionGroupViewModel{TValue}"/> class.
		/// </summary>
		/// <param name="displayName">If it's blank, then the display name will be the TValue type (if not bool)</param>
		public RegularOptionGroupViewModel( string displayName = "" )
            : base( displayName )
        {

        }
		/// <summary>
		/// Gets or sets the option models.
		/// </summary>
		/// <value>
		/// The option models.
		/// </value>
		public override ReadOnlyCollection<OptionViewModel<TValue>> OptionModels { get; set; }
    }

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.OptionGroupViewModel{Monahrq.Theme.Controls.Wizard.Models.RouteOptionViewModel{TValue}, TValue}" />
	public class RouteOptionGroupViewModel<TValue> : OptionGroupViewModel<RouteOptionViewModel<TValue>, TValue>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="RouteOptionGroupViewModel{TValue}"/> class.
		/// </summary>
		/// <param name="displayName">If it's blank, then the display name will be the TValue type (if not bool)</param>
		public RouteOptionGroupViewModel( string displayName = "" )
            : base( displayName )
        {

        }
		/// <summary>
		/// Gets or sets the option models.
		/// </summary>
		/// <value>
		/// The option models.
		/// </value>
		public override ReadOnlyCollection<RouteOptionViewModel<TValue>> OptionModels { get; set; }
    }

}
