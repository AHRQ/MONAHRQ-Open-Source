using System.ComponentModel;

namespace Monahrq.Theme.Controls.Wizard.Models
{
	/// <summary>
	/// Details for a StepGroup.
	/// </summary>
	/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
	public class StepGroup : INotifyPropertyChanged
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="StepGroup"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public StepGroup(string name)
       {
           _displayName = name;
       }
		/// <summary>
		/// The display name
		/// </summary>
		private readonly string _displayName;
		/// <summary>
		/// Gets the display name.
		/// </summary>
		/// <value>
		/// The display name.
		/// </value>
		public string DisplayName { get { return _displayName; } }

		/// <summary>
		/// The is current
		/// </summary>
		private bool _isCurrent;
		/// <summary>
		/// Gets or sets a value indicating whether this instance is current.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is current; otherwise, <c>false</c>.
		/// </value>
		public bool IsCurrent 
        { get 
            { return _isCurrent; }
            set { _isCurrent = value; OnPropertyChanged("IsCurrent"); } 
        }

		/// <summary>
		/// Occurs when [property changed].
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;


		/// <summary>
		/// Called when [property changed].
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
