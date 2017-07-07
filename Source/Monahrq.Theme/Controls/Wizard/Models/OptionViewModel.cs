using System;
using System.ComponentModel;
using Monahrq.Theme.Controls.Wizard.Helpers;
using System.Collections.ObjectModel;

namespace Monahrq.Theme.Controls.Wizard.Models
{

	/// <summary>
	/// Interface for a WizardOption.
	/// </summary>
	public interface IWizardOption
    {
		/// <summary>
		/// Gets a value indicating whether this instance is selected.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
		/// </value>
		bool IsSelected { get; }
		/// <summary>
		/// Gets the display name.
		/// </summary>
		/// <value>
		/// The display name.
		/// </value>
		string DisplayName { get; }
    }

	/// <summary>
	/// Represents a single selectable value with a user-friendly name that can be selected by the user.
	/// The name displayed will be taken from a camel-case split of the type (as in an enum name; Coffee.DarkBlend becomes "Dark Blend")
	/// or yes/no for bools.
	/// </summary>
	/// <typeparam name="TValue">The type of value represented by the option.</typeparam>
	/// <seealso cref="System.IComparable{Monahrq.Theme.Controls.Wizard.Models.OptionViewModel{TValue}}" />
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.IWizardOption" />
	/// <remarks>
	/// IComparable so you can sort the list before rendering
	/// </remarks>
	public class OptionViewModel<TValue> : IComparable<OptionViewModel<TValue>>, IWizardOption
    {

		/// <summary>
		/// A collection of messages which may appear next to an option explaining why it's disabled.
		/// </summary>
		private readonly ObservableCollection<string> _Messages;
		/// <summary>
		/// The enabled
		/// </summary>
		private bool _enabled = true;
		/// <summary>
		/// The unset sort value
		/// </summary>
		const int UNSET_SORT_VALUE = Int32.MinValue;

		/// <summary>
		/// The display name
		/// </summary>
		readonly string _displayName;
		/// <summary>
		/// The is selected
		/// </summary>
		bool _isSelected;
		/// <summary>
		/// The sort value
		/// </summary>
		readonly int _sortValue;
		/// <summary>
		/// The value
		/// </summary>
		readonly TValue _value;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="OptionViewModel{TValue}"/> is enabled.
		/// </summary>
		/// <value>
		///   <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged( "Enabled" );
            }
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="OptionViewModel{TValue}"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public OptionViewModel( TValue value )
            : this( value, UNSET_SORT_VALUE )
        {
            _Messages = new ObservableCollection<string>();
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="OptionViewModel{TValue}"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="sortValue">The sort value.</param>
		public OptionViewModel( TValue value, int sortValue )
        {
            if ( typeof( TValue ) == typeof( bool ) )
            {
                _displayName = ( Convert.ToBoolean( value ) ) ? "Yes" : "No";
            }
            else
            {
                _displayName = value.ToString().SplitCamelCase();
            }
            _value = value;
            _sortValue = sortValue;
            _Messages = new ObservableCollection<string>();
        }

		/// <summary>
		/// Gets the messages.
		/// </summary>
		/// <value>
		/// The messages.
		/// </value>
		public ObservableCollection<string> Messages
        {
            get
            {
                return _Messages;
            }
        }

		/// <summary>
		/// Returns the user-friendly name of this option.
		/// </summary>
		/// <value>
		/// The display name.
		/// </value>
		public string DisplayName
        {
            get { return _displayName; }
        }

		/// <summary>
		/// Gets/sets whether this option is in the selected state.
		/// When this property is set to a new value, this object's
		/// PropertyChanged event is raised.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
		/// </value>
		public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if ( value == _isSelected )
                {
                    return;
                }
                _isSelected = value;
                OnPropertyChanged( "IsSelected" );
            }
        }

		/// <summary>
		/// Returns the value used to sort this option.
		/// The default sort value is Int32.MinValue.
		/// </summary>
		/// <value>
		/// The sort value.
		/// </value>
		public int SortValue
        {
            get { return _sortValue; }
        }

		/// <summary>
		/// Returns the underlying value of this option.
		/// Note: this is a method, instead of a property,
		/// so that the UI cannot bind to it.
		/// </summary>
		/// <returns></returns>
		public TValue GetValue()
        {
            return _value;
        }

		//IComparable<OptionViewModel<TValue>>
		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="other">An object to compare with this instance.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="other" /> in the sort order.  Zero This instance occurs in the same position in the sort order as <paramref name="other" />. Greater than zero This instance follows <paramref name="other" /> in the sort order.
		/// </returns>
		public int CompareTo( OptionViewModel<TValue> other )
        {
            if ( other == null )
                return -1;

            if ( this.SortValue == UNSET_SORT_VALUE && other.SortValue == UNSET_SORT_VALUE )
            {
                return this.DisplayName.CompareTo( other.DisplayName );
            }
            else if ( this.SortValue != UNSET_SORT_VALUE && other.SortValue != UNSET_SORT_VALUE )
            {
                return this.SortValue.CompareTo( other.SortValue );
            }
            else if ( this.SortValue != UNSET_SORT_VALUE && other.SortValue == UNSET_SORT_VALUE )
            {
                return -1;
            }
            else
            {
                return +1;
            }
        }

		/// <summary>
		/// Occurs when [property changed].
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Called when [property changed].
		/// </summary>
		/// <param name="name">The name.</param>
		protected void OnPropertyChanged( string name )
        {
            if ( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( name ) );
            }
        }

    }

}
